# Remove procedure calls for Pip.Services in .NET Changelog

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

