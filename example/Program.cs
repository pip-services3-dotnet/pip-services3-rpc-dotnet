using System;
using PipServices.Commons.Config;
using PipServices.Commons.Refer;
using PipServices.Components.Log;
using PipServices.Rpc.Services;

namespace PipServices.Rpc
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new DummyController();
            var service = new DummyCommandableHttpService();
            var logger = new ConsoleLogger();

            service.Configure(ConfigParams.FromTuples(
                "connection.protocol", "http",
                "connection.host", "localhost",
                "connection.port", 3000
            ));

            service.SetReferences(References.FromTuples(
                new Descriptor("pip-services-dummies", "controller", "default", "default", "1.0"), controller,
                new Descriptor("pip-services-dummies", "service", "rest", "default", "1.0"), service,
                new Descriptor("pip-services-commons", "logger", "console", "default", "1.0"), logger
            ));

            service.OpenAsync(null).Wait();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
