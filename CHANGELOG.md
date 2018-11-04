# Remove procedure calls for Pip.Services in .NET Changelog

## <a name="3.0.0"></a> 3.0.0 (2018-08-23)

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

