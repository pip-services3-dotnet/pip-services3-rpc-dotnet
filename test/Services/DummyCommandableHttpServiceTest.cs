using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;
using PipServices3.Rpc.Services;
using Xunit;

namespace PipServices3.Rpc.Services
{
    public sealed class DummyCommandableHttpServiceTest
    {
        private static readonly ConfigParams RestConfig = ConfigParams.FromTuples(
            "connection.uri", "http://localhost:3000",
            "options.timeout", 15000,
            "swagger.enable", "true"
            //"connection.protocol", "http",
            //"connection.host", "localhost",
            //"connection.port", 3000
        );

        public DummyCommandableHttpServiceTest()
        {
        }

        private DummyCommandableHttpService CreateAndOpenService(ConfigParams config)
        {
            var service = new DummyCommandableHttpService();

            var references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), new DummyController(),
                new Descriptor("pip-services3-dummies", "service", "rest", "default", "1.0"), service
            );

            service.Configure(config);
            service.SetReferences(references);

            service.OpenAsync(null).Wait();

            return service;
        }
        
        [Fact]
        public async Task TestOpenApiAsync()
        {
            DummyCommandableHttpService service = CreateAndOpenService(RestConfig);

            try
            {
                var serviceUri = RestConfig.GetAsString("connection.uri");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{serviceUri}/dummy/swagger");
                var openApiContent = await response.Content.ReadAsStringAsync();

                Assert.StartsWith("openapi:", openApiContent);
            }
            finally
            {
                await service.CloseAsync(null);
            }
		}

        [Fact]
        public async Task TestOpenApiOverrideAsync()
        {
            // turn off and override standard swagger document formation
            var config = RestConfig.SetDefaults(ConfigParams.FromTuples("swagger.auto", false));

            DummyCommandableHttpService service = CreateAndOpenService(config);

            try
            {
                var serviceUri = RestConfig.GetAsString("connection.uri");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{serviceUri}/dummy/swagger");
                var openApiContent = await response.Content.ReadAsStringAsync();

                Assert.StartsWith("swagger yaml content from resource", openApiContent);
            }
            finally
            {
                await service.CloseAsync(null);
            }
        }
    }
}
