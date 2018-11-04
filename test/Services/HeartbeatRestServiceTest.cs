using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PipServices3.Commons.Config;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Refer;
using Xunit;

namespace PipServices3.Rpc.Services
{
    public class HeartbeatRestServiceTest: IDisposable
    {
        private HeartbeatRestService _service;

        public HeartbeatRestServiceTest()
        {
            var config = ConfigParams.FromTuples(
                "connection.protocol", "http",
                "connection.host", "localhost",
                "connection.port", "3002"
            );
            _service = new HeartbeatRestService();
            _service.Configure(config);

            _service.OpenAsync(null).Wait();
        }

        public void Dispose()
        {
            _service.CloseAsync(null).Wait();
        }

        [Fact]
        public async Task TestHeartbeatAsync() 
        {
            DateTime? time = await Invoke<DateTime?>("/heartbeat");
            Assert.NotNull(time);
            Assert.True((DateTime.UtcNow - time.Value).TotalMilliseconds < 1000);
        }

        private static async Task<T> Invoke<T>(string route)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var response = await httpClient.GetAsync("http://localhost:3002" + route);
                var responseValue = response.Content.ReadAsStringAsync().Result;
                return JsonConverter.FromJson<T>(responseValue);
            }
        }
    }
}
