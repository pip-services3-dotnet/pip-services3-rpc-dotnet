using PipServices3.Commons.Refer;

namespace PipServices3.Rpc.Services
{
    public sealed class DummyCommandableHttpService : CommandableHttpService
    {
        public DummyCommandableHttpService() 
            : base("dummy")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services3-dummies", "controller", "default", "*", "1.0"));
        }
    }
}
