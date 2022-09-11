using PipServices3.Commons.Refer;

namespace PipServices3.Rpc.Services
{
    public sealed class DummyCommandableHttpServiceV2 : CommandableHttpService
    {
        private IDummyController _controller;

        public DummyCommandableHttpServiceV2() 
            : base("dummy")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services3-dummies", "controller", "default", "*", "1.0"));
        }

        public override void Register()
        {
            if (!_swaggerAuto && _swaggerEnable)
                RegisterOpenApiSpec("swagger yaml content");

            base.Register();
        }
    }
}
