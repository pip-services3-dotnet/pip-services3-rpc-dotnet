using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices.Commons.Config;

namespace PipServices.Rpc.Services
{
    /// <summary>
    /// Service returns heartbeat via HTTP/REST protocol.
    /// 
    /// The service responds on /heartbeat route(can be changed)
    /// with a string with the current time in UTC.
    /// 
    /// This service route can be used to health checks by loadbalancers and
    /// container orchestrators.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// base_route:              base route for remote URI(default: "")
    /// route:                   route to heartbeat operation(default: "heartbeat")
    /// dependencies:
    /// endpoint:              override for HTTP Endpoint dependency 
    /// connection(s):           
    /// discovery_key:         (optional) a key to retrieve the connection from IDiscovery
    /// protocol:              connection protocol: http or https
    /// host:                  host name or IP address
    /// port:                  port number
    /// uri:                   resource URI or connection string with all parameters in it
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0               (optional) ILogger components to pass log messages
    /// - *:counters:*:*:1.0             (optional) ICounters components to pass collected measurements
    /// - *:discovery:*:*:1.0            (optional) IDiscovery services to resolve connection
    /// - *:endpoint:http:*:1.0          (optional) HttpEndpoint reference
    /// </summary>
    /// <example>
    /// <code>
    /// var service = new HeartbeatService();
    /// service.Configure(ConfigParams.fromTuples(
    /// "route", "ping",
    /// "connection.protocol", "http",
    /// "connection.host", "localhost",
    /// "connection.port", 8080 ));
    /// 
    /// service.Open("123");
    /// Console.Out.WriteLine("The Heartbeat service is accessible at http://+:8080/ping");
    /// </code>
    /// </example>
    public class HeartbeatRestService: RestService
    {
        private string _route = "heartbeat";

        /// <summary>
        /// Creates a new instance of this service.
        /// </summary>
        public HeartbeatRestService()
        {
        }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            _route = config.GetAsStringWithDefault("route", _route);
        }

        /// <summary>
        /// Registers all service routes in HTTP endpoint.
        /// </summary>
        public override void Register()
        {
            RegisterRoute("get", _route, Heartbeat);
        }

        /// <summary>
        /// Handles heartbeat requests
        /// </summary>
        /// <param name="request">a HTTP request</param>
        /// <param name="response">a HTTP response</param>
        /// <param name="routeData"> a current routing path</param>
        private async Task Heartbeat(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            await SendResultAsync(response, DateTime.UtcNow);
        }
    }
}
