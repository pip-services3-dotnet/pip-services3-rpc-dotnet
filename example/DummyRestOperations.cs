using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Refer;
using PipServices3.Commons.Run;

namespace PipServices3.Rpc.Services
{
    public class DummyRestOperations: RestOperations
    {
        private IDummyController _controller;

        public DummyRestOperations()
        {
            _dependencyResolver.Put("controller",
                new Descriptor("pip-services3-dummies", "controller", "default", "*", "*"));
        }
        
        public new void SetReferences(IReferences references)
        {
            base.SetReferences(references);

            _controller = _dependencyResolver.GetOneRequired<IDummyController>("controller");
        }

        public async Task GetPageByFilterAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var filter = GetFilterParams(request);
            var paging = GetPagingParams(request);
            var sort = GetSortParams(request);

            var result = await _controller.GetPageByFilterAsync(correlationId, filter, paging);

            await SendResultAsync(response, result);
        }
        
        public async Task CreateAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var dummy = JsonConverter.FromJson<Dummy>(JsonConverter.ToJson(parameters.GetAsObject("dummy")));

            var result = await _controller.CreateAsync(correlationId, dummy);

            await SendResultAsync(response, result);
        }
        
        public async Task UpdateAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var dummy = JsonConverter.FromJson<Dummy>(JsonConverter.ToJson(parameters.GetAsObject("dummy")));

            var result = await _controller.UpdateAsync(correlationId, dummy);

            await SendResultAsync(response, result);
        }
        
        public async Task GetByIdAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var id = parameters.GetAsNullableString("dummy_id") ?? parameters.GetAsNullableString("id");
            
            var result = await _controller.GetOneByIdAsync(correlationId, id);

            await SendResultAsync(response, result);
        }
        
        public async Task DeleteByIdAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var id = parameters.GetAsNullableString("dummy_id") ?? parameters.GetAsNullableString("id");
            
            var result = await _controller.DeleteByIdAsync(correlationId, id);

            await SendResultAsync(response, result);
        }
    }
}