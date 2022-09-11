﻿using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Rpc.Services;
using Xunit;

namespace PipServices3.Rpc.Clients
{
    public sealed class UninitializedClient
    {
        [Fact]
        public void TestImproperProtocol()
        {
            var restConfig = ConfigParams.FromTuples(
                "connection.protocol", "ftp"
            );

            DummyController _ctrl;
            DummyCommandableHttpClient _client;
            DummyCommandableHttpService _service;


            _ctrl = new DummyController();

            _service = new DummyCommandableHttpService();

            _client = new DummyCommandableHttpClient();

            var references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), _ctrl,
                new Descriptor("pip-services3-dummies", "service", "rest", "default", "1.0"), _service,
                new Descriptor("pip-services3-dummies", "client", "rest", "default", "1.0"), _client
            );
            _service.Configure(restConfig);
            _client.Configure(restConfig);

            _client.SetReferences(references);
            _service.SetReferences(references);

            _service.OpenAsync(null).Wait();


            var response = Record.ExceptionAsync(async () => await _client.OpenAsync(null));

            Assert.NotNull(response?.Result);
            var exception = Assert.IsType<ConfigException>(response.Result);
            Assert.Equal("WRONG_PROTOCOL", exception.Code);



            var task = _client.CloseAsync(null);
            task.Wait();

            task = _service.CloseAsync(null);
            task.Wait();
        }

        [Fact]
        public void TestNoHostPortUriSet()
        {
            var restConfig = ConfigParams.FromTuples(
                "connection.protocol", "http",
                "connection.port", "0"          // default is 8080
                );

            DummyController _ctrl;
            DummyCommandableHttpClient _client;

            DummyCommandableHttpService _service;


            _ctrl = new DummyController();

            _service = new DummyCommandableHttpService();

            _client = new DummyCommandableHttpClient();

            var references = References.FromTuples(
                new Descriptor("pip-services3-dummies", "controller", "default", "default", "1.0"), _ctrl,
                new Descriptor("pip-services3-dummies", "service", "rest", "default", "1.0"), _service,
                new Descriptor("pip-services3-dummies", "client", "rest", "default", "1.0"), _client
            );
            _service.Configure(restConfig);
            _client.Configure(restConfig);

            _client.SetReferences(references);
            _service.SetReferences(references);

            _service.OpenAsync(null).Wait();

            
            var response = Record.ExceptionAsync(async () => await _client.OpenAsync(null));
            


            Assert.NotNull(response?.Result);
            var exception = Assert.IsType<ConfigException>(response.Result);
            Assert.Equal("NO_PORT", exception.Code);


            var task = _client.CloseAsync(null);
            task.Wait();

            task = _service.CloseAsync(null);
            task.Wait();
        }

    }
}
