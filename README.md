# React-shop-cloudfront

This is frontend starter project for nodejs-aws mentoring program. It uses the following technologies:

- [Vite](https://vitejs.dev/) as a project bundler
- [React](https://beta.reactjs.org/) as a frontend framework
- [React-router-dom](https://reactrouterdotcom.fly.dev/) as a routing library
- [MUI](https://mui.com/) as a UI framework
- [React-query](https://react-query-v3.tanstack.com/) as a data fetching library
- [Formik](https://formik.org/) as a form library
- [Yup](https://github.com/jquense/yup) as a validation schema
- [Vitest](https://vitest.dev/) as a test runner
- [MSW](https://mswjs.io/) as an API mocking library
- [Eslint](https://eslint.org/) as a code linting tool
- [Prettier](https://prettier.io/) as a code formatting tool
- [TypeScript](https://www.typescriptlang.org/) as a type checking tool

## Available Scripts

### `start`

Starts the project in dev mode with mocked API on local environment.

### `build`

Builds the project for production in `dist` folder.

### `preview`

Starts the project in production mode on local environment.

### `test`, `test:ui`, `test:coverage`

Runs tests in console, in browser or with coverage.

### `lint`, `prettier`

Runs linting and formatting for all files in `src` folder.

# Task 2 (Serve SPA in AWS S3 and Cloudfront Services)
https://github.com/rolling-scopes-school/aws/blob/main/aws-developer/02_serving_spa/task.md

+ Installed the latest version of AWS CDK (https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html).
+ Configured credentials for AWS to make them accessible for AWS CLI & CDK.
+ Forked the React Shop single-page app from https://github.com/rolling-scopes-school/nodejs-aws-shop-react.
+ Installed dependencies (need to use npm i --force).

[Architecture] (Architecture.pdf)

# Task 2.1
## Manual Deployment

1) In the AWS Console, created and configured an S3 bucket
[S3 bucket](https://shop-web-app.s3.eu-central-1.amazonaws.com/index.html/)

2) Created a CloudFront distribution for app 
[Cloud Front](https://d2zyxqnb5qq3f8.cloudfront.net/)

3) AWS CLI commands:
```sh
`aws --version`
`aws s3 ls`
`aws s3api get-bucket-policy --bucket shop-web-app`
`aws cloudfront list-distributions`
```
```sh
curl -o NUL -s -w "Total: %{time_total}s\n" https://d2zyxqnb5qq3f8.cloudfront.net
# Total: 0.124938s

curl -o NUL -s -w "Total: %{time_total}s\n" https://shop-web-app.s3.eu-central-1.amazonaws.com/index.html
# Total: 0.177813s

curl -o $null -s -w "Total: %{time_total}s\n" https://d2zyxqnb5qq3f8.cloudfront.net
# Total: 0.036126s

curl -o $null -s -w "Total: %{time_total}s\n" https://shop-web-app.s3.eu-central-1.amazonaws.com/index.html
# Total: 0.126337s
```

# Task 2.2

Used [AWS CDK](https://docs.aws.amazon.com/cdk/api/v2/docs/aws-construct-library.html)
1) Created new folder, for example 'cdk'
2) Installed 
* `npm install -g aws-cdk`
3) Configured 
* `cdk init app --language csharp`
* `cdk bootstrap` or `cdk bootstrap aws://ID/REGION`
 (
    delete options: 
    * `aws cloudformation describe-stacks`
    * `aws cloudformation delete-stack --stack-name CdkStack --region eu-central-1`
    * `aws cloudformation delete-stack --stack-name CdkStack --retain-resources shopwebappbucket4BE87B22`
    * `aws cloudformation delete-stack --stack-name CDKToolkit`
    * `npm uninstall -g aws-cdk`
)
4) Added new options to CdkStack.cs and Program.cs
# core for AWS CDK (Program.cs)
```csharp
public static void Main(string[] args)
{
    var app = new App();
    new CdkStack(app, "CdkStack", new StackProps
    {
        Env = new Environment
        {
            Account = "ACCOUNT_ID",
            Region = "eu-central-1",
        }
    });
    app.Synth();
}
```
# CdkStack.cs
```csharp
internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
{
    // Create the S3 bucket
    var bucket = new Bucket(this, $"{BucketId}-bucket", new BucketProps
    {
        ...
    });

    var oai = new OriginAccessIdentity(this, "OAI");

    // Create an IAM policy statement to allow GetObject action on the S3 bucket
    var bucketPolicyStatement = new PolicyStatement(new PolicyStatementProps
    {
        ...
    });

    bucket.AddToResourcePolicy(bucketPolicyStatement);

    // Create CloudFront distribution
    var distribution = new CloudFrontWebDistribution(this, $"{BucketId}-distribution", new CloudFrontWebDistributionProps
    {
        OriginConfigs = new[]
        {
          ...
        }
    });

    // Deploy files to the S3 bucket
    new BucketDeployment(this, $"{BucketId}-deployment", new BucketDeploymentProps
    {
        ...
    });

    // Export CloudFront distribution ID
    new CfnOutput(this, "DistributionURL", new CfnOutputProps
    {
        Value = distribution.DistributionDomainName,
        Description = "CloudFront distribution"
    });

    // Output S3 bucket URL
    new CfnOutput(this, "BucketURL", new CfnOutputProps
    {
        Value = bucket.BucketWebsiteUrl,
        Description = "The URL of the S3 bucket website endpoint"
    });
}
```

5) Builded C# project
* `dotnet build src` compile this app
* `cdk deploy`       deploy this stack to your default AWS account/region
* `cdk diff`         compare deployed stack with current state
* `cdk synth`        emits the synthesized CloudFormation template
6) Added options to package.json:
   "scripts": {
    "cdk-deploy": "cdk deploy --app \"cdk_dest/cdk.out\"",
    "deploy": "npm run build && npm run cdk-deploy"
    }
7) Result of operation  'cdk deploy --all'
[Settings] (img.png)
8) Checked links:
[S3 bucket link](http://shop-web-app-automated.s3-website.eu-central-1.amazonaws.com/) - the 403 error should be shown
[Cloud Front link](https://dfmqzjmg0ul9o.cloudfront.net/) - should be available.


# Task 3

Task: https://github.com/rolling-scopes-school/aws/blob/main/aws-developer/03_serverless_api/task.md

### What was done?
- Created two Lambda functions: `getProductsList` and `getProductsById`.
- Integrated these functions with API Gateway.
- Added CORS support.
- Created Swagger documentation.
- Added unit tests for Lambda functions.

### Additional scope
- Swagger documentation: added `openapi.yaml` file.
- Unit tests: created tests for `getProductsList` and `getProductsById`.
- Code is separated into different modules for better maintainability.

### How to run Swagger locally?

1. Ensure you have the necessary dependencies installed:
   * `npm install swagger-ui-express yamljs express --legacy-peer-deps`
2. Run the Swagger server:
   * `npm run start-swagger`

### Links
- [Product Service API](https://s0ob4l26k9.execute-api.us-east-1.amazonaws.com/prod/products)
- [Product Service API for id = 1](https://s0ob4l26k9.execute-api.us-east-1.amazonaws.com/prod/products/1)
- [Frontend](https://d2i6ibe4yd5bdd.cloudfront.net/)
- [Swagger](http://localhost:3000/api-docs)

### Task 4.1
1. Created two DynamoDB tables:
   - **products** table with fields:
     - `id` (uuid, Primary key)
     - `title` (text, not null)
     - `description` (text)
     - `price` (integer)
   - **stocks** table with fields:
     - `product_id` (uuid, Foreign key referencing `products.id`)
     - `count` (integer, representing total stock count)

2. Wrote a script (`populate-tables.js`) to fill tables with test data.

### Task 4.2
1. Updated AWS CDK Stack:
   - Created DynamoDB tables (`products` and `stocks`).
   - Updated Lambda’s environment variables.

2. Integrated `getProductsList` Lambda:
   - Returns list of products via `GET /products` endpoint from DynamoDB.
   - Implemented joining of `products` and `stocks` tables.

3. Implemented Product model:
   - Combined `product` and `stock` data into a single model (`Product`).

### Task 4.3
1. Created `createProduct` Lambda function:
   - Triggered by HTTP POST method.
   - Implements logic to create a new item in `products` table.

2. Curl:

curl -X POST \
  https://s0ob4l26k9.execute-api.us-east-1.amazonaws.com/prod/products \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "some new title",
    "description": "some new description",
    "price": 11,
    "count": 22
  }'


### Task 4.4
1. Committed changes to new branch

### Additional Tasks
- **Error Handling**:
  - Implemented error handling:
    - Returns 400 status code for invalid product data.
    - Returns 500 status code for any unhandled errors.
  
- **Logging**:
  - Added console.log for each incoming request and their arguments.

- **Transactions**:
  - Implemented transaction-based creation of products and stocks to ensure data integrity.

### Links
- [Product Service API](https://s0ob4l26k9.execute-api.us-east-1.amazonaws.com/prod/products)
- [Product Service API for id = 1](https://s0ob4l26k9.execute-api.us-east-1.amazonaws.com/prod/products/1)
- [Frontend](https://d2i6ibe4yd5bdd.cloudfront.net/)
- [Swagger](http://localhost:3000/api-docs)