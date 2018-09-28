using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices.Commons.Config;
using PipServices.Commons.Convert;
using PipServices.Commons.Refer;
using PipServices.Components.Info;

namespace PipServices.Rpc.Services
{
    /// <summary>
    /// Service that returns microservice status information via HTTP/REST protocol.
    /// 
    /// The service responds on /status route(can be changed) with a JSON object:
    /// {
    /// "id":            unique container id(usually hostname)
    /// "name":          container name(from ContextInfo)
    /// "description":   container description(from ContextInfo)
    /// "start_time":    time when container was started
    /// "current_time":  current time in UTC
    /// "uptime":        duration since container start time in milliseconds
    /// "properties":    additional container properties(from ContextInfo)
    /// "components":    descriptors of components registered in the container
    /// }
    /// 
    /// ### Configuration parameters ###
    /// 
    /// base_route:              base route for remote URI
    /// route:                   status route(default: "status")
    /// dependencies:
    /// endpoint:              override for HTTP Endpoint dependency
    /// controller:            override for Controller dependency
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
    /// - *:discovery:*:*:1.0            (optional) IDiscovery services
    /// - *:endpoint:http:*:1.0          (optional) HttpEndpoint reference
    /// </summary>
    /// <example>
    /// <code>
    /// var service = new StatusService();
    /// service.Configure(ConfigParams.FromTuples(
    /// "connection.protocol", "http",
    /// "connection.host", "localhost",
    /// "connection.port", 8080 ));
    /// 
    /// service.Open("123");
    /// Console.Out.WriteLine("The Status service is accessible at http://+:8080/status");
    /// </code>
    /// </example>
    public class StatusRestService: RestService
    {
        private DateTime _startTime = DateTime.UtcNow;
        private IReferences _references;
        private ContextInfo _contextInfo;
        private string _route = "status";

        /// <summary>
        /// Creates a new instance of this service.
        /// </summary>
        public StatusRestService()
        {
            _dependencyResolver.Put("context-info", new Descriptor("pip-services", "context-info", "default", "*", "1.0"));
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
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
		public override void SetReferences(IReferences references)
		{
            _references = references;
            base.SetReferences(references);

            _contextInfo = _dependencyResolver.GetOneOptional<ContextInfo>("context-info");
		}

        /// <summary>
        /// Registers all service routes in HTTP endpoint.
        /// </summary>
		public override void Register()
        {
            RegisterRoute("get", _route, Status);
        }

        private async Task Status(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            var id = _contextInfo != null ? _contextInfo.ContextId : "";
            var name = _contextInfo != null ? _contextInfo.Name : "Unknown";
            var description = _contextInfo != null ? _contextInfo.Description : "";
            var uptime = (DateTime.UtcNow - _startTime).TotalMilliseconds;
            var properties = _contextInfo.Properties;

            var components = new List<string>();
            if (_references != null)
            {
                foreach (var locator in _references.GetAllLocators())
                    components.Add(locator.ToString());
            }

            var status = new
            {
                id = id,
                name = name,
                description = description,
                start_time = StringConverter.ToString(_startTime),
                current_time = StringConverter.ToString(DateTime.UtcNow),
                uptime = uptime,
                properties = properties,
                components = components
            };

            await SendResultAsync(response, status);
        }
    
    }
}
