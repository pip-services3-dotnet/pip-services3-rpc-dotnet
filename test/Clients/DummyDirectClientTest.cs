using System;
using PipServices3.Commons.Refer;
using Xunit;

namespace PipServices3.Rpc.Clients
{
    public class DummyDirectClientTest: IDisposable
    {
        private readonly DummyController _ctrl;
        private readonly DummyDirectClient _client;
        private readonly DummyClientFixture _fixture;

        public DummyDirectClientTest()
        {
            _ctrl = new DummyController();
            _client = new DummyDirectClient();

            var references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), _ctrl
            );
            _client.SetReferences(references);

            _fixture = new DummyClientFixture(_client);

            var clientTask = _client.OpenAsync(null);
            clientTask.Wait();
        }

        [Fact]
        public void TestCrudOperations()
        {
            var task = _fixture.TestCrudOperations();
            task.Wait();
        }

        public void Dispose()
        {
            var task = _client.CloseAsync(null);
            task.Wait();
        }
    }
}
