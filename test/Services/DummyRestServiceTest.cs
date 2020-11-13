using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;
using PipServices3.Commons.Refer;
using Xunit;

namespace PipServices3.Rpc.Services
{
    [Collection("Sequential")]
    public class DummyRestServiceTest : IDisposable
    {
        private static readonly ConfigParams RestConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "localhost",
            "connection.port", 3003 
        );

        private static readonly ConfigParams ServiceConfig = ConfigParams.FromTuples(
            "base_route", "/api/v1",
            "openapi_content", "swagger yaml or json content",  // for test only
            "swagger.enable", "true"
        );

        private DummyRestService _service;
        private HttpEndpoint _httpEndpoint;

        public DummyRestServiceTest()
        {
            _httpEndpoint = new HttpEndpoint();
            _service = CreateService(_httpEndpoint, ServiceConfig);

            _httpEndpoint.Configure(RestConfig);
            _httpEndpoint.OpenAsync(null).Wait();
        }

        private DummyRestService CreateService(HttpEndpoint httpEndpoint, ConfigParams config)
        {
            var service = new DummyRestService();

            var references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), new DummyController(),
                new Descriptor("pip-services3", "endpoint", "http", "default", "1.0"), httpEndpoint
            );

            service.Configure(config);
            service.SetReferences(references);
            return service;
        }

        public void Dispose()
        {
            _service.CloseAsync(null).Wait();
            _httpEndpoint.CloseAsync(null).Wait();
        }

        [Fact]
        public void It_Should_Perform_CRUD_Operations()
        {
            It_Should_Be_Opened();

            It_Should_Create_Dummy();
            
            It_Should_Create_Dummy2();

            It_Should_Update_Dummy2();
            
            It_Should_Get_Dummy();
            
            It_Should_Get_Dummies();

            It_Should_Delete_Dummy();
        }

        [Fact]
        public void It_Should_Return_OpenApi_Content()
        {
            var baseRoute = ServiceConfig.GetAsString("base_route");
            var openApiContent = ServiceConfig.GetAsString("openapi_content");

            var result = SendRequest("get", string.Format("{0}/swagger", baseRoute), new { });
            Assert.Equal(openApiContent, result);
        }

        [Fact]
        public async Task It_Should_Return_OpenApi_FileAsync()
        {
            // create temp file
            var openApiContent = "swagger yaml or json content";
            var fileName = Path.GetTempFileName();

            File.WriteAllText(fileName, openApiContent);

            // recreate service with new configuration
            await _httpEndpoint.CloseAsync(null);
            await _service.CloseAsync(null);

            _httpEndpoint = new HttpEndpoint();
            _service = CreateService(_httpEndpoint, ConfigParams.FromTuples(
                "base_route", "/api/v1",
                "openapi_file", fileName,  // for test only
                "swagger.enable", "true"
            ));

            _httpEndpoint.Configure(RestConfig);
            await _httpEndpoint.OpenAsync(null);

            var result = SendRequest("get", "/api/v1/swagger", new { });
            Assert.Equal(openApiContent, result);

            File.Delete(fileName);
        }

        private void It_Should_Delete_Dummy()
        {
            var existingDummy = new Dummy("1", "Key 1", "Content 1");

            var result = SendRequest("delete", $"/api/v1/dummies/{existingDummy.Id}", new
            {
            });

            var resultDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(resultDummy);
            Assert.NotNull(resultDummy.Id);
            Assert.Equal(existingDummy.Key, resultDummy.Key);
            Assert.Equal(existingDummy.Content, resultDummy.Content);

            result = SendRequest("get", $"/api/v1/dummies/{existingDummy.Id}", new
            {
            });

            Assert.Empty(result);
        }

        private void It_Should_Be_Opened()
        {
            Assert.True(_httpEndpoint.IsOpen());
        }

        private void It_Should_Create_Dummy()
        {
            var newDummy = new Dummy("1", "Key 1", "Content 1");

            var result = SendRequest("post", "/api/v1/dummies", new
            {
                dummy = newDummy
            });

            var resultDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(resultDummy);
            Assert.NotNull(resultDummy.Id);
            Assert.Equal(newDummy.Key, resultDummy.Key);
            Assert.Equal(newDummy.Content, resultDummy.Content);
        }

        private void It_Should_Create_Dummy2()
        {
            var newDummy = new Dummy("2", "Key 2", "Content 2");

            var result = SendRequest("post", "/api/v1/dummies/file", newDummy, true);

            var resultDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(resultDummy);
            Assert.NotNull(resultDummy.Id);
            Assert.Equal(newDummy.Key, resultDummy.Key);
            Assert.Equal(newDummy.Content, resultDummy.Content);
        }
        
        private void It_Should_Update_Dummy2()
        {
            var dummy = new Dummy("2", "Key 2", "Content 3");

            var result = SendRequest("put", $"/api/v1/dummies/myOwnId", dummy);

            var resultDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(resultDummy);
            Assert.NotNull(resultDummy.Id);
            Assert.Equal(dummy.Key, resultDummy.Key);
            Assert.Equal(dummy.Content, resultDummy.Content);
        }

        private void It_Should_Get_Dummy()
        {
            var existingDummy = new Dummy("1", "Key 1", "Content 1");

            var result = SendRequest("get", $"/api/v1/dummies/{existingDummy.Id}?correlation_id=test", new
            {
            });

            var resultDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(resultDummy);
            Assert.NotNull(resultDummy.Id);
            Assert.Equal(existingDummy.Key, resultDummy.Key);
            Assert.Equal(existingDummy.Content, resultDummy.Content);
        }

        private void It_Should_Get_Dummies()
        {
            var existingDummy = new Dummy("1", "Key 1", "Content 1");
            
            var result = SendRequest("get", $"/api/v1/dummies?filter=key={existingDummy.Key}", new
            {
            });

            var resultDummies = JsonConverter.FromJson<DataPage<Dummy>>(result);

            Assert.NotNull(resultDummies);
            Assert.NotNull(resultDummies.Data);
            Assert.Single(resultDummies.Data);
            
            result = SendRequest("get", $"/api/v1/dummies", new
            {
            });

            resultDummies = JsonConverter.FromJson<DataPage<Dummy>>(result);

            Assert.NotNull(resultDummies);
            Assert.NotNull(resultDummies.Data);
            Assert.Equal(2, resultDummies.Data.Count());
        }

        private static string SendRequest(string method, string route, dynamic request, bool formData = false)
        {
            using (var httpClient = new HttpClient())
            {
                HttpContent content;
                if (formData)
                {
                    content = new MultipartFormDataContent()
                    {
                        {new StringContent(JsonConverter.ToJson(request)), "file", "test_file.json"}
                    };
                }
                else
                {
                    content = new StringContent(JsonConverter.ToJson(request), Encoding.UTF8, "application/json");
                }

                var response = new HttpResponseMessage();

                    switch (method)
                    {
                        case "get":
                            response = httpClient.GetAsync($"http://localhost:3003{route}").Result;
                            break;
                        case "post":
                            response = httpClient.PostAsync($"http://localhost:3003{route}", content).Result;
                            break;
                        case "put":
                            response = httpClient.PutAsync($"http://localhost:3003{route}", content).Result;
                            break;
                        case "delete":
                            response = httpClient.DeleteAsync($"http://localhost:3003{route}").Result;
                            break;
                    }

                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}