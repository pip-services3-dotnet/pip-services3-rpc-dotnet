using PipServices.Commons.Refer;

namespace PipServices.Rpc.Services
{
    public sealed class DummyCommandableHttpServiceV2 : CommandableHttpService
    {
        private IDummyController _controller;

        public DummyCommandableHttpServiceV2() 
            : base("Dummy")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services-dummies", "controller", "default", "*", "1.0"));
        }

        public override void SetReferences(IReferences references)
        {
            base.SetReferences(references);

            _controller = references.GetOneRequired<IDummyController>(new Descriptor("pip-services-dummies", "controller", "default", "*", "1.0"));
        }
    }
}
