using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;
using PipServices3.Rpc.Auth;

namespace PipServices3.Rpc.Services
{
    public class DummyRestService : RestService
    {
        private DummyRestOperations _operations = new DummyRestOperations();
        private int _numberOfCalls = 0;
        private string _openApiContent;
        private string _openApiFile;

        public override void Configure(ConfigParams config)
		{
			base.Configure(config);

            _openApiContent = config.GetAsNullableString("openapi_content");
            _openApiFile = config.GetAsNullableString("openapi_file");
        }

		public override void SetReferences(IReferences references)
        {
            base.SetReferences(references);

            _operations.SetReferences(references);
        }

        public int GetNumberOfCalls()
        {
            return _numberOfCalls;
        }

        private async Task IncrementNumberOfCallsAsync(HttpRequest req, HttpResponse res, ClaimsPrincipal user, RouteData rd,
            Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Task> next)
        {
            _numberOfCalls++;
            await next(req, res, user, rd);
        }

        public override void Register()
        {
            var auth = new BasicAuthorizer();
            
            RegisterInterceptor("", IncrementNumberOfCallsAsync);
            
            RegisterRouteWithAuth("get", "/dummies", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.GetPageByFilterAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("get", "/dummies/{id}", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.GetByIdAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("post", "/dummies", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.CreateAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("post", "/dummies/file", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.CreateFromFileAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("put", "/dummies", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.UpdateAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("put", "/dummies/{id}", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.UpdateAsync(request, response, user, routeData);
                });
            
            RegisterRouteWithAuth("delete", "/dummies/{id}", auth.Anybody(),
                async (request, response, user, routeData) =>
                {
                    await _operations.DeleteByIdAsync(request, response, user, routeData);
                });

			if (!string.IsNullOrWhiteSpace(_openApiContent))
				RegisterOpenApiSpec(_openApiContent);

            if (!string.IsNullOrWhiteSpace(_openApiFile))
                RegisterOpenApiFromFile(_openApiFile);
        }
    }
}