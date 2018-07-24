using PipServices.Commons.Refer;

namespace PipServices.Rpc.Services
{
    public sealed class DummyCommandableHttpService : CommandableHttpService
    {
        public DummyCommandableHttpService() 
            : base("dummy")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services-dummies", "controller", "default", "*", "1.0"));
        }
    }
}
