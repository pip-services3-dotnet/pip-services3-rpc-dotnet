# <img src="https://uploads-ssl.webflow.com/5ea5d3315186cf5ec60c3ee4/5edf1c94ce4c859f2b188094_logo.svg" alt="Pip.Services Logo" width="200"> <br/> Remote Procedure Calls for .NET Changelog

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

