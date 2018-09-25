using System;
using System.Threading;
using System.Threading.Tasks;

using PipServices.Commons.Config;
using PipServices.Commons.Refer;
using PipServices.Rpc.Services;
using Xunit;

namespace PipServices.Rpc.Clients
{
    public sealed class DummyCommandableHttpClientTest : IDisposable
    {
        private static readonly ConfigParams RestConfig = ConfigParams.FromTuples(
            "connection.uri", "http://localhost:3000"
            //"connection.protocol", "http",
            //"connection.host", "localhost",
            //"connection.port", 3000
        );

        private readonly DummyController _ctrl;
        private readonly DummyCommandableHttpClient _client;
        private readonly DummyClientFixture _fixture;
        private readonly CancellationTokenSource _source;

        private readonly DummyCommandableHttpService _service;

        public DummyCommandableHttpClientTest()
        {
            _ctrl = new DummyController();

            _service = new DummyCommandableHttpService();

            _client = new DummyCommandableHttpClient();

            var references = References.FromTuples(
                new Descriptor("pip-services-dummies", "controller", "default", "default", "1.0"), _ctrl,
                new Descriptor("pip-services-dummies", "service", "rest", "default", "1.0"), _service,
                new Descriptor("pip-services-dummies", "client", "rest", "default", "1.0"), _client
            );
            _service.Configure(RestConfig);
            _client.Configure(RestConfig);

            _client.SetReferences(references);
            _service.SetReferences(references);

            _service.OpenAsync(null).Wait();

            _fixture = new DummyClientFixture(_client);

            _source = new CancellationTokenSource();

            _client.OpenAsync(null).Wait();
        }

        [Fact]
        public void TestCrudOperations()
        {
            var task = _fixture.TestCrudOperations();
            task.Wait();
        }

        [Fact]
        public void TestExceptionPropagation()
        {
            try
            {
                _client.RaiseExceptionAsync("123").Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            var task = _client.CloseAsync(null);
            task.Wait();

            task = _service.CloseAsync(null);
            task.Wait();
        }
    }
}
