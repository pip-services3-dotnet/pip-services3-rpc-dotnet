using System.IO;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
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
        
        public async Task CreateFromFileAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var dummyFile = parameters.RequestFiles.Count > 0 ? parameters.RequestFiles[0] : null;
            
            byte[] fileContent;

            using (var memoryStream = new MemoryStream())
            {
                    await dummyFile.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    fileContent = new byte[memoryStream.Length];
                    await memoryStream.ReadAsync(fileContent, 0, fileContent.Length);
            }

            var json = Encoding.UTF8.GetString(fileContent);
            var dummy = JsonConverter.FromJson<Dummy>(json);
            
            var result = await _controller.CreateAsync(correlationId, dummy);

            await SendResultAsync(response, result);
        }
        
        public async Task UpdateAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
            RouteData routeData)
        {
            var correlationId = GetCorrelationId(request);
            var parameters = GetParameters(request);
            var dummy = JsonConverter.FromJson<Dummy>(parameters.RequestBody);
            
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