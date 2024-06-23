using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using Function = Amazon.CDK.AWS.Lambda.Function;
using FunctionProps = Amazon.CDK.AWS.Lambda.FunctionProps;

namespace Cdk
{
    public class CdkStack : Stack
    {
        private const string BackendPath = "product_service";
        private const string LambdaPath = "lambdas";
        private readonly string[] _allowMethods = { "GET", "OPTIONS", "POST" };
        private readonly string[] _allowHeaders =
            { "Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token" };

        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create DynamoDB table ProductsTable
            var productsTable = new Table(this, "ProductsTable", new TableProps
            {
                TableName = "products",
                PartitionKey = new Attribute { Name = "id", Type = AttributeType.STRING },
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create DynamoDB table StocksTable
            var stocksTable = new Table(this, "StocksTable", new TableProps
            {
                TableName = "stocks",
                PartitionKey = new Attribute { Name = "product_id", Type = AttributeType.STRING },
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create DynamoDB table LocksTable
            var locksTable = new Table(this, "LocksTable", new TableProps
            {
                TableName = "transaction_locks",
                PartitionKey = new Attribute { Name = "_id", Type = AttributeType.STRING },
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create Lambda function for getProductsList
            var getProductsListFunction = new Function(this, "GetProductsListFunction", new FunctionProps
            {
                Runtime = Runtime.NODEJS_18_X,
                Handler = "getProductsList.handler",
                Code = Code.FromAsset($"../../{BackendPath}/{LambdaPath}"),
                Environment = new Dictionary<string, string>
                {
                    { "PRODUCTS_TABLE", productsTable.TableName },
                    { "STOCKS_TABLE", stocksTable.TableName }
                }
            });

            // Create Lambda function for getProductsById
            var getProductsByIdFunction = new Function(this, "GetProductsByIdFunction", new FunctionProps
            {
                Runtime = Runtime.NODEJS_18_X,
                Handler = "getProductsById.handler",
                Code = Code.FromAsset($"../../{BackendPath}/{LambdaPath}"),
                Environment = new Dictionary<string, string>
                {
                    { "PRODUCTS_TABLE", productsTable.TableName },
                    { "STOCKS_TABLE", stocksTable.TableName }
                }
            });

            // Create Lambda function for createProduct
            var createProductFunction = new Function(this, "CreateProductFunction", new FunctionProps
            {
                Runtime = Runtime.NODEJS_18_X,
                Handler = "createProduct.handler",
                Code = Code.FromAsset($"../../{BackendPath}/{LambdaPath}"),
                Environment = new Dictionary<string, string>
                {
                    { "PRODUCTS_TABLE", productsTable.TableName },
                    { "STOCKS_TABLE", stocksTable.TableName },
                    { "LOCKS_TABLE", locksTable.TableName }
                }
            });

            // Grant Lambda functions access to DynamoDB tables
            productsTable.GrantReadWriteData(getProductsListFunction);
            stocksTable.GrantReadWriteData(getProductsListFunction);
            productsTable.GrantReadWriteData(getProductsByIdFunction);
            stocksTable.GrantReadWriteData(getProductsByIdFunction);
            productsTable.GrantReadWriteData(createProductFunction);
            stocksTable.GrantReadWriteData(createProductFunction);
            locksTable.GrantReadWriteData(createProductFunction);

            // Create API Gateway with CORS preflight options
            var api = new RestApi(this, "ProductApi", new RestApiProps
            {
                RestApiName = "Product Service",
                DefaultCorsPreflightOptions = new CorsOptions
                {
                    AllowOrigins = GetAllowOrigins(),
                    AllowMethods = _allowMethods,
                    AllowHeaders = _allowHeaders
                }
            });

            // Integrate getProductsListFunction with API Gateway
            var productsResource = api.Root.AddResource("products");
            var getProductsListIntegration = new LambdaIntegration(getProductsListFunction);
            productsResource.AddMethod("GET", getProductsListIntegration);

            // Integrate createProductFunction with API Gateway
            var createProductIntegration = new LambdaIntegration(createProductFunction);
            productsResource.AddMethod("POST", createProductIntegration);

            // Ensure CORS preflight is only added once
            if (productsResource.DefaultCorsPreflightOptions == null)
            {
                productsResource.AddCorsPreflight(new CorsOptions
                {
                    AllowOrigins = GetAllowOrigins(),
                    AllowMethods = _allowMethods,
                    AllowHeaders = _allowHeaders
                });
            }

            // Integrate getProductsByIdFunction with API Gateway
            var productByIdResource = productsResource.AddResource("{productId}");
            var getProductsByIdIntegration = new LambdaIntegration(getProductsByIdFunction);
            productByIdResource.AddMethod("GET", getProductsByIdIntegration);

            // Ensure CORS preflight is only added once
            if (productByIdResource.DefaultCorsPreflightOptions == null)
            {
                productByIdResource.AddCorsPreflight(new CorsOptions
                {
                    AllowOrigins = GetAllowOrigins(),
                    AllowMethods = _allowMethods,
                    AllowHeaders = _allowHeaders
                });
            }
        }

        private static string[] GetAllowOrigins()
        {
            //const string localUrl = "http://localhost:3000";
            //const string docsUrl = "http://localhost:3000/api-docs";
            return new[]
            {
                //localUrl, docsUrl, $"https://{}"
                "*"
            };
        }
    }
}
