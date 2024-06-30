using System.Collections.Generic;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Constructs;
using Function = Amazon.CDK.AWS.Lambda.Function;
using FunctionProps = Amazon.CDK.AWS.Lambda.FunctionProps;

namespace CdkTest
{
    public class CdkStack : Stack
    {
        private const string BucketName = "csv-file-import-bucket";
        private const string UploadedFolder = "uploaded/";
        private const string LambdaCodePath = "../lambdas";
        private const string ApiName = "Import Service";
        private const string ImportApiResource = "import";
        private readonly string[] _allowMethods = { "GET", "PUT", "OPTIONS", "POST" };
        private readonly string[] _allowHeaders =
            { "Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token" };

        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create S3 bucket
            var bucket = new Bucket(this, "ImportBucket", new BucketProps
            {
                BucketName = BucketName,
                RemovalPolicy = RemovalPolicy.DESTROY,
                AutoDeleteObjects = true,
                Cors = new[]
                {
                    new CorsRule
                    {
                        AllowedOrigins = GetAllowOrigins(),
                        AllowedMethods = new[]
                        {
                            HttpMethods.GET, HttpMethods.PUT, HttpMethods.POST, HttpMethods.DELETE, HttpMethods.HEAD
                        },
                        AllowedHeaders = _allowHeaders
                    }
                }
            });

            var lambdaFunctionProps = new FunctionProps
            {
                Runtime = Runtime.NODEJS_18_X,
                Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), $"{LambdaCodePath}")),
                Environment = new Dictionary<string, string>
                {
                    { "BUCKET_NAME", BucketName }
                }
            };

            var importProductsFile = new Function(this, "ImportProductsFileFunction", new FunctionProps
            {
                Handler = "importProductsFile.handler",
                Runtime = lambdaFunctionProps.Runtime,
                Code = lambdaFunctionProps.Code,
                Environment = lambdaFunctionProps.Environment
            });

            var importFileParser = new Function(this, "ImportFileParserFunction", new FunctionProps
            {
                Handler = "importFileParser.handler",
                Runtime = lambdaFunctionProps.Runtime,
                Code = lambdaFunctionProps.Code,
                Environment = lambdaFunctionProps.Environment
            });

            bucket.GrantReadWrite(importProductsFile);
            bucket.GrantReadWrite(importFileParser);
            bucket.GrantDelete(importFileParser);

            bucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(importFileParser), new NotificationKeyFilter
            {
                Prefix = UploadedFolder
            });

            var api = new RestApi(this, "ImportApi", new RestApiProps
            {
                RestApiName = ApiName,
                DefaultCorsPreflightOptions = new CorsOptions
                {
                    AllowOrigins = GetAllowOrigins(),
                    AllowMethods = _allowMethods,
                    AllowHeaders = _allowHeaders
                }
            });

            var importResource = api.Root.AddResource(ImportApiResource);

            importResource.AddMethod("GET", new LambdaIntegration(importProductsFile), new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.querystring.name", true }
                },
                RequestValidatorOptions = new RequestValidatorOptions
                {
                    ValidateRequestParameters = true
                },
                
            });

            // Ensure CORS preflight is only added once
            if (importResource.DefaultCorsPreflightOptions == null)
            {
                importResource.AddCorsPreflight(new CorsOptions
                {
                    AllowOrigins = GetAllowOrigins(),
                    AllowMethods = _allowMethods,
                    AllowHeaders = _allowHeaders
                });
            }

            // Outputs
            new CfnOutput(this, "ImportApiUrl", new CfnOutputProps
            {
                Value = api.Url,
                Description = "The URL of the import API"
            });

            new CfnOutput(this, "BucketName", new CfnOutputProps
            {
                Value = bucket.BucketName,
                Description = "The name of the S3 bucket"
            });
        }

        private static string[] GetAllowOrigins()
        {
            return Cors.ALL_ORIGINS;
            //const string localUrl = "http://localhost:3000";
            //const string docsUrl = "http://localhost:3000/api-docs";
            //return new[]
            //{
            //    //localUrl, docsUrl, $"https://{}"
            //    "*"
            //};
        }
    }
}