const { handler } = require('../lambdas/createProduct');
const { DynamoDBDocumentClient, TransactWriteCommand, UpdateCommand } = require("@aws-sdk/lib-dynamodb");
const { HTTP_STATUS, MESSAGES } = require('../lambdas/constants');
const ProductBuilder = require('../utils/productBuilder');
const StockBuilder = require('../utils/stockBuilder');
const crypto = require('crypto');

const productId = 'some-id';
const newProduct = new ProductBuilder().withId(productId).build();
const newStock = new StockBuilder().withId(productId).build();
const productBody = {
    title: newProduct.title,
    description: newProduct.description,
    price: newProduct.price,
    count: newStock.count
};
const invalidItems = [null, undefined, NaN, '', '\t', '\n', 123];

jest.mock("@aws-sdk/lib-dynamodb", () => {
    const originalModule = jest.requireActual("@aws-sdk/lib-dynamodb");
    return {
        ...originalModule,
        DynamoDBDocumentClient: {
            from: jest.fn().mockReturnValue({
                send: jest.fn()
            })
        }
    };
});

jest.mock('crypto', () => ({
    randomUUID: jest.fn().mockReturnValue(productId),
}));

describe('createProduct', () => {
    let dynamoDB;

    beforeEach(() => {
        dynamoDB = DynamoDBDocumentClient.from();
    });

    afterEach(() => {
        jest.clearAllMocks();
    });

    it('should create a new product and stock and return 201', async () => {
        dynamoDB.send
            .mockResolvedValueOnce({ Attributes: { count: 1 } }) // Lock
            .mockResolvedValueOnce({}) // Transaction
            .mockResolvedValueOnce({}); // Unlock

        const event = {
            headers: {},
            body: JSON.stringify(productBody)
        };
        const result = await handler(event);

        expect(dynamoDB.send).toHaveBeenCalledWith(expect.any(UpdateCommand)); // Lock
        expect(dynamoDB.send).toHaveBeenCalledWith(expect.any(TransactWriteCommand)); // Transaction
        expect(dynamoDB.send).toHaveBeenCalledWith(expect.any(UpdateCommand)); // Unlock
        expect(result.statusCode).toBe(HTTP_STATUS.CREATED);
        expect(JSON.parse(result.body)).toEqual(newProduct);
    });

    describe('parameter validation', () => {
        describe('title validation', () => {
            it('should return 400 if title is invalid', async () => {
                for (const item of invalidItems) {

                    const event = {
                        headers: {},
                        body: JSON.stringify({
                            title: item
                        })
                    };
                    const result = await handler(event);

                    expect(result.statusCode).toBe(HTTP_STATUS.BAD_REQUEST);
                    expect(JSON.parse(result.body).message).toBe(MESSAGES.INVALID_INPUT_TITLE);
                }
            });
        });
    });

    it('should return 423 if an error occurs during locking', async () => {
        dynamoDB.send.mockRejectedValueOnce(new Error("DynamoDB error"));

        const event = {
            headers: {},
            body: JSON.stringify(productBody)
        };
        const result = await handler(event);

        expect(result.statusCode).toBe(HTTP_STATUS.LOCKED);
        expect(JSON.parse(result.body).message).toBe(MESSAGES.RESOURCE_LOCKED);
    });

    it('should return 500 if an error occurs during transaction', async () => {
        dynamoDB.send
            .mockResolvedValueOnce({ Attributes: { count: 1 } }) // Lock
            .mockRejectedValueOnce(new Error("DynamoDB error")) // Transaction
            .mockResolvedValueOnce({}); // Unlock

        const event = {
            headers: {},
            body: JSON.stringify(productBody)
        };
        const result = await handler(event);

        expect(result.statusCode).toBe(HTTP_STATUS.INTERNAL_SERVER_ERROR);
        expect(JSON.parse(result.body).message).toBe(MESSAGES.INTERNAL_SERVER_ERROR);
    });
});