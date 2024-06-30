const { S3Client, GetObjectCommand, CopyObjectCommand, DeleteObjectCommand } = require('@aws-sdk/client-s3');
const csvParser = require("csv-parser");
const { getCorsHeaders } = require("./cors");
const { HTTP_STATUS, MESSAGES } = require("./constants");

exports.handler = async (event) => {
    const s3 = new S3Client();

    const origin = event.headers?.origin;
    const headers = getCorsHeaders(origin);

    const bucketName = event['Records'][0]['s3']['bucket']['name'];
    const objectName = event['Records'][0]['s3']['object']['key'];

    console.log('bucket:', bucketName);
    console.log('key:', objectName);

    try {
      const getObjectCommand = new GetObjectCommand({
        Bucket: bucketName,
        Key: objectName,
    });

    const s3Object = await s3.send(getObjectCommand);

        const results = [];
        const parser = s3Object.Body.pipe(csvParser());

        for await (const record of parser) {
            results.push(record);
        }

        console.log('Parsed CSV data:', results)

        const newObjectKey = `parsed/${objectName.split('/').pop()}`;
        await s3.send(new CopyObjectCommand({
            Bucket: bucketName,
            CopySource: `${bucketName}/${objectName}`,
            Key: newObjectKey
        }));

        console.log('Copied file:', newObjectKey);

        await s3.send(new DeleteObjectCommand({
            Bucket: bucketName,
            Key: objectName
        }));

        console.log('Deleted file:', objectName);

        return {
          statusCode: HTTP_STATUS.OK,
            headers,
            body: JSON.stringify({ message: 'CSV File was processed' }),
        }
    } catch (error) {
        console.log('Error processing CSV file:', error);

        return {
            statusCode: HTTP_STATUS.INTERNAL_SERVER_ERROR,
            headers,
            body: JSON.stringify({ message:  MESSAGES.INTERNAL_SERVER_ERROR }),
        };
    }
};