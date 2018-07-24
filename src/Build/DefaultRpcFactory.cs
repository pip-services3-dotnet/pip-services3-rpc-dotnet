using PipServices.Commons.Refer;
using PipServices.Components.Build;
using PipServices.Rpc.Services;

namespace PipServices.Rpc.Build
{
    public class DefaultRpcFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "rpc", "default", "1.0");
        public static Descriptor HttpEndpointDescriptor = new Descriptor("pip-services", "endpoint", "http", "*", "1.0");
        public static Descriptor StatusServiceDescriptor = new Descriptor("pip-services", "status-service", "http", "*", "1.0");
        public static Descriptor HeartbeatServiceDescriptor = new Descriptor("pip-services", "heartbeat-service", "http", "*", "1.0");

        public DefaultRpcFactory()
        {
            RegisterAsType(HttpEndpointDescriptor, typeof(HttpEndpoint));
            RegisterAsType(StatusServiceDescriptor, typeof(StatusRestService));
            RegisterAsType(HeartbeatServiceDescriptor, typeof(HeartbeatRestService));
        }
    }
}
