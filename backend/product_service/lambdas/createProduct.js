const { DynamoDBClient } = require("@aws-sdk/client-dynamodb");
const { DynamoDBDocumentClient, TransactWriteCommand, UpdateCommand } = require("@aws-sdk/lib-dynamodb");
const { getCorsHeaders } = require('./cors');
const crypto = require('crypto');
const { HTTP_STATUS, MESSAGES } = require('./constants');
const { validateInput } = require('./validation');

const client = new DynamoDBClient({});
const dynamoDB = DynamoDBDocumentClient.from(client);
const productsTableName = process.env.PRODUCTS_TABLE;
const stocksTableName = process.env.STOCKS_TABLE;
const locksTableName = process.env.LOCKS_TABLE;

function generateUUID() {
    return crypto.randomUUID();
}

const lock = async (name) => {
    const lockKey = { _id: name };
    const updateExpression = "SET #count = if_not_exists(#count, :start) + :increment";
    const expressionAttributeNames = { "#count": "count" };
    const expressionAttributeValues = { ":start": 0, ":increment": 1 };

    const params = {
        TableName: locksTableName,
        Key: lockKey,
        UpdateExpression: updateExpression,
        ExpressionAttributeNames: expressionAttributeNames,
        ExpressionAttributeValues: expressionAttributeValues,
        ConditionExpression: "attribute_not_exists(#count) OR #count = :start",
        ReturnValues: "UPDATED_NEW"
    };

    try {
        const result = await dynamoDB.send(new UpdateCommand(params));
        return result.Attributes.count === 1;
    } catch (error) {
        console.log(`Failed to obtain lock: ${error}`);
        return false;
    }
};

const unlock = async (name) => {
    const lockKey = { _id: name };
    const updateExpression = "SET #count = #count - :decrement";
    const expressionAttributeNames = { "#count": "count" };
    const expressionAttributeValues = { ":decrement": 1 };

    const params = {
        TableName: locksTableName,
        Key: lockKey,
        UpdateExpression: updateExpression,
        ExpressionAttributeNames: expressionAttributeNames,
        ExpressionAttributeValues: expressionAttributeValues,
        ReturnValues: "UPDATED_NEW"
    };

    try {
        await dynamoDB.send(new UpdateCommand(params));
        return true;
    } catch (error) {
        console.log(MESSAGES.FAILED_TO_UNLOCK(error));
        return false;
    }
};

exports.handler = async (event) => {
    const origin = event.headers.origin;
    const headers = getCorsHeaders(origin, 'POST,OPTIONS');
    const { title, description, price, count } = JSON.parse(event.body);

    try {
        const validationError = validateInput({ title, description, price, count });
        if (validationError) {
            return {
                statusCode: HTTP_STATUS.BAD_REQUEST,
                body: JSON.stringify({ message: validationError }),
                headers,
            };
        }

        const productId = generateUUID();

        const productItem = {
            id: productId,
            title,
            description,
            price
        };

        const stockItem = {
            product_id: productId,
            count
        };

        const lockName = "createProductLock";

        const isLocked = await lock(lockName);
        if (!isLocked) {
            console.log(MESSAGES.RESOURCE_LOCKED);
            return {
                statusCode: HTTP_STATUS.LOCKED,
                body: JSON.stringify({ message: MESSAGES.RESOURCE_LOCKED }),
                headers,
            };
        }

        const transactParams = {
            TransactItems: [
                {
                    Put: {
                        TableName: productsTableName,
                        Item: productItem
                    }
                },
                {
                    Put: {
                        TableName: stocksTableName,
                        Item: stockItem
                    }
                }
            ]
        };

        try {
            await dynamoDB.send(new TransactWriteCommand(transactParams));

            const returnItem = {
                id: productId,
                title,
                description,
                price,
                count
            };

            await unlock(lockName);

            return {
                statusCode: HTTP_STATUS.CREATED,
                body: JSON.stringify(returnItem),
                headers,
            };
        } catch (error) {
            console.log(MESSAGES.TRANSACTION_FAILED(error));
            await unlock(lockName);
            return {
                statusCode: HTTP_STATUS.INTERNAL_SERVER_ERROR,
                body: JSON.stringify({ message: MESSAGES.INTERNAL_SERVER_ERROR }),
                headers,
            };
        }
    } catch (error) {
        console.log(MESSAGES.ERROR_CREATING_PRODUCT(error));//153
        //await unlock(lockName);
        return {
            statusCode: HTTP_STATUS.INTERNAL_SERVER_ERROR,
            body: JSON.stringify({ message: MESSAGES.INTERNAL_SERVER_ERROR }),
            headers,
        };
    }
};
