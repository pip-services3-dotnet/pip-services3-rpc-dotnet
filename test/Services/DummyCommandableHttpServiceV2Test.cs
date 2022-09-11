using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;
using PipServices3.Commons.Refer;
using Xunit;

namespace PipServices3.Rpc.Services
{
    public sealed class DummyCommandableHttpServiceV2Test
    {
        private static int testPort = 3000;

        private static readonly ConfigParams restConfig = ConfigParams.FromTuples(
            "connection.protocol", "http",
            "connection.host", "localhost",
            "connection.port", testPort,
            "swagger.enable", "true"
        );

        private readonly Dummy DUMMY1 = new(null, "Key 1", "Content 1");
        private readonly Dummy DUMMY2 = new(null, "Key 1", "Content 1");

        private DummyCommandableHttpServiceV2 _service;
        private HttpClient _httpClient;

        public DummyCommandableHttpServiceV2Test()
        {
            var ctrl = new DummyController();

            _httpClient = new HttpClient();
            _service = new DummyCommandableHttpServiceV2();
            _service.Configure(restConfig);

            References references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), ctrl,
                new Descriptor("pip-services-dummies", "service", "http", "default", "1.0"), _service
            );

            _service.SetReferences(references);

            _service.OpenAsync(null).Wait();
        }

        [Fact]
        public async Task Test_CRUD_OperationsAsync()
        {
            // Create one dummy
            var result = await SendRequestAsync("/dummy/create_dummy", new
            {
                dummy = DUMMY1
            });

            var dummy = JsonConverter.FromJson<Dummy>(result);
            Assert.NotNull(dummy);
            Assert.NotNull(dummy.Id);
            Assert.Equal(dummy.Key, DUMMY1.Key);
            Assert.Equal(dummy.Content, DUMMY1.Content);

            var dummy1 = dummy;

            // Create another dummy
            result = await SendRequestAsync("/dummy/create_dummy", new
            {
                dummy = DUMMY2
            });

            dummy = JsonConverter.FromJson<Dummy>(result);
            Assert.NotNull(dummy);
            Assert.NotNull(dummy.Id);
            Assert.Equal(dummy.Key, DUMMY2.Key);
            Assert.Equal(dummy.Content, DUMMY2.Content);

            // Get all dummies // TODO add by filters
            result = await SendRequestAsync("/dummy/get_dummies", new
            {
            });

            var page = JsonConverter.FromJson<DataPage<Dummy>>(result);

            Assert.NotNull(page);
            Assert.NotNull(page.Data);
            Assert.Equal(2, page.Data.Count);

            // Update the dummy
            dummy1.Content = "Updated Content 1";

            result = await SendRequestAsync("/dummy/update_dummy", new { dummy = dummy1 });

            dummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(dummy);
            Assert.NotNull(dummy.Id);
            Assert.Equal(dummy.Key, dummy1.Key);
            Assert.Equal(dummy.Content, dummy1.Content);

            dummy1 = dummy;

            // Get the dummy by id
            result = await SendRequestAsync("/dummy/get_dummy_by_id", new
            {
                dummy_id= dummy1.Id
            });

            dummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(dummy);
            Assert.NotNull(dummy.Id);
            Assert.Equal(dummy.Key, dummy1.Key);
            Assert.Equal(dummy.Content, dummy1.Content);

            // Delete dummy
            result = await SendRequestAsync("/dummy/delete_dummy", new
            {
                dummy_id= dummy1.Id
            });

            var deletedDummy = JsonConverter.FromJson<Dummy>(result);

            Assert.NotNull(deletedDummy);
            Assert.NotNull(deletedDummy.Id);
            Assert.Equal(dummy1.Key, deletedDummy.Key);
            Assert.Equal(dummy1.Content, deletedDummy.Content);

            result = await SendRequestAsync("/dummy/get_dummy_by_id", new
            {
                dummy_id = dummy1.Id
            });

            Assert.Empty(result);

            // Failed validation
            result = await SendRequestAsync("/dummy/create_dummy", new
            {
            });

            var res = JsonConverter.FromJson<IDictionary<string, object>>(result);
            Assert.Equal("INVALID_DATA", res["code"]);
        }

        private async Task<string> SendRequestAsync(string route, dynamic request)
        {
            HttpContent content = new StringContent(JsonConverter.ToJson(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"http://localhost:{testPort}{route}", content);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
