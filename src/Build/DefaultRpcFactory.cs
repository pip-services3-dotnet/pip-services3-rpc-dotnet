using PipServices.Commons.Refer;
using PipServices.Components.Build;
using PipServices.Rpc.Services;

namespace PipServices.Rpc.Build
{
    /// <summary>
    /// Creates RPC components by their descriptors.
    /// </summary>
    /// See <see cref="Factory"/>, <see cref="HttpEndpoint"/>, <see cref="StatusRestService"/>, <see cref="HeartbeatRestService"/>
    public class DefaultRpcFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "rpc", "default", "1.0");
        public static Descriptor HttpEndpointDescriptor = new Descriptor("pip-services", "endpoint", "http", "*", "1.0");
        public static Descriptor StatusServiceDescriptor = new Descriptor("pip-services", "status-service", "http", "*", "1.0");
        public static Descriptor HeartbeatServiceDescriptor = new Descriptor("pip-services", "heartbeat-service", "http", "*", "1.0");

        /// <summary>
        /// Create a new instance of the factory.
        /// </summary>
        public DefaultRpcFactory()
        {
            RegisterAsType(HttpEndpointDescriptor, typeof(HttpEndpoint));
            RegisterAsType(StatusServiceDescriptor, typeof(StatusRestService));
            RegisterAsType(HeartbeatServiceDescriptor, typeof(HeartbeatRestService));
        }
    }
}
