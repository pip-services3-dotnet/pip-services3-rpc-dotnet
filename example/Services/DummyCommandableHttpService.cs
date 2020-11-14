using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;

namespace PipServices3.Rpc.Services
{
    public sealed class DummyCommandableHttpService : CommandableHttpService
    {
		private bool _swaggerOverride = false;

        public DummyCommandableHttpService() 
            : base("dummy")
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services3-dummies", "controller", "default", "*", "1.0"));
        }

		public override void Configure(ConfigParams config)
		{
			base.Configure(config);

			_swaggerOverride = config.GetAsBooleanWithDefault("swagger.override", false);
		}

		public override void Register()
		{
			if (_swaggerOverride && _swaggerEnable)
			{
				RegisterOpenApiFromResource("DummyRestServiceSwagger.yaml");
			}

			base.Register();
		}
	}
}
