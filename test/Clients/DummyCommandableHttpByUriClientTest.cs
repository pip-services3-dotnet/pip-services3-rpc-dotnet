//using System;
//using System.Threading;

//using PipServices.Commons.Config;
//using PipServices.Commons.Refer;
//using PipServices.Rpc.Services;
//using Xunit;

//namespace PipServices.Rpc.Clients
//{
//    public sealed class DummyCommandableHttpByUriClientTest : IDisposable
//    {
//        private static readonly ConfigParams RestConfig = ConfigParams.FromTuples(
//            "connection.protocol", "http",
//            "connection.uri", "localhost:3000"
//        );

//        private readonly DummyController _ctrl;
//        private readonly DummyCommandableHttpClient _client;
//        private readonly DummyClientFixture _fixture;
//        private readonly CancellationTokenSource _source;

//        private readonly DummyCommandableHttpService _service;

//        public DummyCommandableHttpByUriClientTest()
//        {
//            _ctrl = new DummyController();

//            _service = new DummyCommandableHttpService();

//            _client = new DummyCommandableHttpClient();

//            var references = References.FromTuples(
//                new Descriptor("pip-services-dummies", "controller", "default", "default", "1.0"), _ctrl,
//                new Descriptor("pip-services-dummies", "service", "rest", "default", "1.0"), _service,
//                new Descriptor("pip-services-dummies", "client", "rest", "default", "1.0"), _client
//            );
//            _service.Configure(RestConfig);
//            _client.Configure(RestConfig);

//            _client.SetReferences(references);
//            _service.SetReferences(references);

//            _service.OpenAsync(null).Wait();

//            _fixture = new DummyClientFixture(_client);

//            _source = new CancellationTokenSource();

//            _client.OpenAsync(null).Wait();
//        }

//        [Fact]
//        public void TestCrudOperations()
//        {
//            var task = _fixture.TestCrudOperations();
//            task.Wait();
//        }

//        public void Dispose()
//        {
//            var task = _client.CloseAsync(null);
//            task.Wait();

//            task = _service.CloseAsync(null);
//            task.Wait();
//        }
//    }
//}
