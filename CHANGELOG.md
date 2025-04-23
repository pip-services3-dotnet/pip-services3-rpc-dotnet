# <img src="https://uploads-ssl.webflow.com/5ea5d3315186cf5ec60c3ee4/5edf1c94ce4c859f2b188094_logo.svg" alt="Pip.Services Logo" width="200"> <br/> Remote Procedure Calls for Pip.Services in .NET Changelog

## <a name="3.7.4"></a> 3.7.4 (2025-04-23)

### Features
* Added request_max_size option to HttpEndpoint

## <a name="3.7.3"></a> 3.7.3 (2024-08-14)

### Fixes
* Fix CommandableSwaggerDocument to provide registering for empty schema
* Change docker compose syntax

## <a name="3.7.2"></a> 3.7.2 (2024-05-29)

### Features
* Enabled the ability to override RestClient.ExtractContentEntityAsync

## <a name="3.7.1"></a> 3.7.1 (2023-12-15)

### Breaking Changes
* Migrate to .NET 8.0

## <a name="3.6.4-3.6.5"></a> 3.6.4-3.6.5 (2023-12-14)

### Features
* Added MapSchema to OpenAPI document

## <a name="3.6.3"></a> 3.6.3 (2022-10-11)

### Features
* Added SendTooManyRequests method to RestOperations
* Reactored some other Send* methods

## <a name="3.6.2"></a> 3.6.2 (2022-10-11)

### Features
* Extended IInitializable interface

## <a name="3.6.1"></a> 3.6.1 (2022-09-11)

### Features
* Added schema validations for services
* Added more tests


## <a name="3.6.0"></a> 3.6.0 (2022-08-04)

### Breaking Changes
* Migrate to .NET 6.0

## <a name="3.5.3"></a> 3.5.3 (2022-07-20)

### Features
* Added supports HEAD request for RestClient

## <a name="3.5.0-3.5.2"></a> 3.5.0-3.5.2 (2021-09-02)

### Breaking Changes
* Migrate to .NET Core 5.0

### Features
* Added to RestOperations SafeInvokeAsync, HandleError and Instrument methods

## <a name="3.4.5"></a> 3.4.5 (2021-08-18)
### Fixes
* Fixed the HTTPS credentials

## <a name="3.4.3-3.4.4"></a> 3.4.3-3.4.4 (2021-06-09) 
### Features
* Added CORS headers to HttpEndpoint

## <a name="3.4.2"></a> 3.4.2 (2021-06-09) 

### Features
* Updated references

## <a name="3.4.0"></a> 3.4.0 (2021-04-22)

### Fixes
* **test** Added TestRpcClient
* **test** Added TestCommandableHttpClient

## <a name="3.3.15-3.3.22"></a> 3.3.15-3.3.22 (2021-04-05)

### Fixes
* **clients** Fixed counter's method names
* **clients** Fixed processing of empty routes
* **clients** Fixed adding null filter/paging parameters

### Features
* Extended RestClient to include CorrelationID into request headers
* Upgraded OpenApiDoc to sends more types of responses

## <a name="3.3.12-3.3.14"></a> 3.3.12-3.3.14 (2021-01-06)

### Features
* Added rest route metadata to generate OpenApiSpec
* Registering OpenApiSpec from metadata in RestService

## <a name="3.3.8-3.3.10"></a> 3.3.8-3.3.10 (2020-12-25)
### Features
* Extended RestClient with PATCH method (only for netstandard2.1)
* Added route builder helper

## <a name="3.3.7"></a> 3.3.7 (2020-12-11)

### Features
* Refactored usage of SwaggerService
* Added IInitializable interface

## <a name="3.3.6"></a> 3.3.6 (2020-12-09)

### Features
* Removed Swashbuckle package
* Using ISwaggerService to configuure Swagger UI

## <a name="3.3.5"></a> 3.3.5 (2020-12-05)

### Features
* added helper methods for processing HTTP requests

## <a name="3.3.4"></a> 3.3.4 (2020-12-03)

### Features
* Added SwaggerUI based on Swashbuckle
* Updated swagger document generation

## <a name="3.3.2-3.3.3"></a> 3.3.2-3.3.3 (2020-12-01)

### Features
* Added methods to support swagger
### Fixes
* Fixed descriptor (pip-services3->pip-services)

## <a name="3.3.1"></a> 3.3.1 (2020-06-26)

### Features
* Implemented support backward compatibility

## <a name="3.3.0"></a> 3.3.0 (2020-05-26)
### Breaking Changes
* Migrated to .NET Core 3.1

## <a name="3.2.6"></a> 3.2.6 (2020-01-31)
### Fixes
* **clients** Added invocation timeout for RestClient

## <a name="3.2.5"></a> 3.2.5 (2020-01-13)
### Breaking Changes
* Added 'pip-services' descriptors

## <a name="3.2.4"></a> 3.2.4 (2019-12-21)
### Fixes
* **services** Fixed rest operations retrieving order

## <a name="3.2.3"></a> 3.2.3 (2019-12-20)
### Fixes
* **services** Fixed rest operations retrieving

## <a name="3.2.2"></a> 3.2.2 (2019-11-01)
### Fixes
* **services** Fixed metrics naming in RestService

## <a name="3.2.1"></a> 3.2.1 (2019-10-29)
### Fixes
* **clients** Added async and await to CallCommandAsync in CommandableHttpClient

## <a name="3.2.0"></a> 3.2.0 (2019-10-19)
* **clients** Added instrumentation to error handling

## <a name="3.0.10"></a> 3.0.10 (2019-09-26)
* **rest operations** Added fetching of files from request parameters

## <a name="3.0.9"></a> 3.0.9 (2019-09-16)
* **endpoint** Added response compression ability to HttpEndpoint

## <a name="3.0.8"></a> 3.0.8 (2019-07-10)
* **connect** Added https support without credentials to HttpConnectionResolver

## <a name="3.0.7"></a> 3.0.7 (2019-07-08)
* **rest** Extended RestService
* **rest** Added RestOperations
* **rest** Added Interceptor
* **endpoint** Extend HttpEndpoint
* **swagger** Added ISwaggerService
* **status** Added AboutOperation
* **status** Added HeartbeatOperations
* **connect** Extend HttpConnectionResolver
* **auth** Added BasicAuthorizer
* **auth** Added OwnerAuthorizer
* **auth** Added RoleAuthorizer
* **test** Added DummyRestService
* **test** Added DummyRestOperations
* **test** Added DummyRestServiceTest

### Features
* Added RESTful support

## <a name="3.0.0-3.0.5"></a> 3.0.0-3.0.5 (2019-04-10)
* **rest** Extended RestClient

### Breaking Changes
* Moved to a separate package

### Features
* Added CORS support to RestService

## <a name="2.3.4"></a> 2.3.4 (2018-06-11)
* **status** Added StatusRestService
* **status** Added HeartbeatRestService
* **rest** Added ResolveAllAsync method to HttpConnectionResolver

## <a name="2.2.1"></a> 2.2.1 (2018-03-19)
* **rest** Added base routes
* **rest** Added retries to RestClient
* Reimplemented HttpEndpoint
* **rest** HttpResponseSender
* **rest** HttpConnectionResolver

## <a name="2.1.0-2.1.6"></a> 2.1.0-2.1.6 (2018-03-09)
* **rest** Add HttpEndpoint
* **rest** Add exception handling in RestService

### Features
* Converted to .NET Standard 2.0

## <a name="2.0.0-2.0.8"></a> 2.0.0-2.0.8 (2017-06-12)

### Features
* **rest** Add missing functionality of DirectClient
* **rest** Add CommandableHttpClient and CommandableHttpService
* **test** Re-structure tests - separate pure unit-tests libraries

## <a name="2.0.0"></a> 2.0.0 (2017-02-25)

### Breaking Changes
* Migrated to pip-services3-commons 2.0

## <a name="1.0.0"></a> 1.0.0 (2016-11-21)

Initial public release

### Features
* **messaging** Abstract and in-memory message queues
* **rest** RESTful service and client
* **rest** Implemented connection parameters and credentials
* **messaging** MsmqMessageQueue

### Bug Fixes
No fixes in this version

