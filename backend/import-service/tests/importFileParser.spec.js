const { S3Client, GetObjectCommand, CopyObjectCommand, DeleteObjectCommand } = require('@aws-sdk/client-s3');
const csvParser = require('csv-parser');
const { getCorsHeaders } = require('../lambdas/cors');
const { HTTP_STATUS, MESSAGES } = require('../lambdas/constants');
const { handler } = require('../lambdas/importFileParser');

jest.mock('@aws-sdk/client-s3');
jest.mock('csv-parser');

describe('import Lambda handler', () => {
  const headers = getCorsHeaders('*');

  const mockEvent = {
    headers: {
      origin: 'http://example.com'
    },
    Records: [{
      s3: {
        bucket: {
          name: 'my-bucket'
        },
        object: {
          key: 'path/to/myfile.csv'
        }
      }
    }]
  };

  let sendMock;

  beforeEach(() => {
    sendMock = jest.fn();
    S3Client.mockImplementation(() => ({
      send: sendMock
    }));
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  test('should process CSV and return success response', async () => {
    // Mock the S3 getObject response to return a readable stream
    const { PassThrough } = require('stream');
    const mockStream = new PassThrough();
    mockStream.end('id,name\n1,Product1\n2,Product2\n');

    sendMock
      .mockResolvedValueOnce({ Body: mockStream }) // getObjectCommand
      .mockResolvedValueOnce({}) // copyObjectCommand
      .mockResolvedValueOnce({}); // deleteObjectCommand

    // Mock the csvParser to return a readable stream of parsed CSV objects
    const mockCsvStream = new PassThrough({ objectMode: true });
    setImmediate(() => {
      mockCsvStream.write({ id: '1', name: 'Product1' });
      mockCsvStream.write({ id: '2', name: 'Product2' });
      mockCsvStream.end();
    });

    csvParser.mockReturnValue(mockCsvStream);
    const response = await handler(mockEvent);
    expect(sendMock).toHaveBeenCalledTimes(3);
    expect(sendMock).toHaveBeenNthCalledWith(1, expect.any(GetObjectCommand));
    expect(sendMock).toHaveBeenNthCalledWith(2, expect.any(CopyObjectCommand));
    expect(sendMock).toHaveBeenNthCalledWith(3, expect.any(DeleteObjectCommand));
    expect(response).toEqual({
      statusCode: HTTP_STATUS.OK,
      headers,
      body: JSON.stringify({ message: 'CSV File was processed' })
    });
  });

  test('should handle errors and return error response', async () => {
    const errorMessage = 'Something went wrong';
    sendMock.mockRejectedValue(new Error(errorMessage));
    const response = await handler(mockEvent);
    expect(sendMock).toHaveBeenCalledTimes(1);
    expect(response.statusCode).toBe(HTTP_STATUS.INTERNAL_SERVER_ERROR);
    expect(response).toEqual({
      statusCode: HTTP_STATUS.INTERNAL_SERVER_ERROR,
      headers,
      body: JSON.stringify({ message: MESSAGES.INTERNAL_SERVER_ERROR })
    });
  });
});