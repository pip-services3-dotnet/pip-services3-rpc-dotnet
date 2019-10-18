using System;
using System.IO;
using PipServices3.Commons.Commands;
using PipServices3.Commons.Run;

namespace PipServices3.Rpc.Services
{
    /// <summary>
    /// Abstract service that receives remove calls via HTTP/REST protocol
    /// to operations automatically generated for commands defined in <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-commons-dotnet/master/doc/api/interface_pip_services_1_1_commons_1_1_commands_1_1_i_commandable.html">ICommandable</a> components.
    /// Each command is exposed as POST operation that receives all parameters in body object.
    /// 
    /// Commandable services require only 3 lines of code to implement a robust external
    /// HTTP-based remote interface.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// - base_route:              base route for remote URI
    /// 
    /// dependencies:
    /// - endpoint:              override for HTTP Endpoint dependency
    /// - controller:            override for Controller dependency
    /// 
    /// connection(s):
    /// - discovery_key:         (optional) a key to retrieve the connection from <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_connect_1_1_i_discovery.html">IDiscovery</a>
    /// - protocol:              connection protocol: http or https
    /// - host:                  host name or IP address
    /// - port:                  port number
    /// - uri:                   resource URI or connection string with all parameters in it
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// - *:counters:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_count_1_1_i_counters.html">ICounters</a> components to pass collected measurements
    /// - *:discovery:*:*:1.0        (optional) <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_connect_1_1_i_discovery.html">IDiscovery</a> services to resolve connection
    /// - *:endpoint:http:*:1.0          (optional) <a href="https://rawgit.com/pip-services3-dotnet/pip-services3-rpc-dotnet/master/doc/api/class_pip_services_1_1_rpc_1_1_services_1_1_http_endpoint.html">HttpEndpoint</a> reference
    /// </summary>
    /// <example>
    /// <code>
    /// class MyCommandableHttpService: CommandableHttpService 
    /// {
    ///     public MyCommandableHttpService()
    ///     {
    ///         base();
    ///         this._dependencyResolver.put(
    ///         "controller", new Descriptor("mygroup", "controller", "*", "*", "1.0") );
    ///     }
    /// }
    /// 
    /// var service = new MyCommandableHttpService();
    /// service.Configure(ConfigParams.fromTuples(
    /// "connection.protocol", "http",
    /// "connection.host", "localhost",
    /// "connection.port", 8080 ));
    /// 
    /// service.SetReferences(References.fromTuples(
    /// new Descriptor("mygroup","controller","default","default","1.0"), controller ));
    /// 
    /// service.Open("123");
    /// Console.Out.WriteLine("The REST service is running on port 8080");
    /// </code>
    /// </example>
    public class CommandableHttpService : RestService
    {
        /// <summary>
        /// Creates a new instance of the service.
        /// </summary>
        /// <param name="baseRoute">a service base route.</param>
        public CommandableHttpService(string baseRoute)
        {
            _baseRoute = baseRoute;
            _dependencyResolver.Put("controller", "none");
        }

        /// <summary>
        /// Registers all service routes in HTTP endpoint.
        /// </summary>
        public override void Register()
        {
            var controller = _dependencyResolver.GetOneRequired<ICommandable>("controller");
            var commands = controller.GetCommandSet().Commands;

            foreach (var command in commands)
            {
                RegisterRoute("post", command.Name, async (request, response, routeData) =>
                {
                    var correlationId = "";
                    try
                    {
                        var body = string.Empty;

                        using (var streamReader = new StreamReader(request.Body))
                        {
                            body = streamReader.ReadToEnd();
                        }

                        var parameters = string.IsNullOrEmpty(body) ? new Parameters() : Parameters.FromJson(body);
                        correlationId = request.Query.ContainsKey("correlation_id")
                           ? request.Query["correlation_id"][0]
                           : parameters.GetAsStringWithDefault("correlation_id", string.Empty);

                        using (var timing = Instrument(correlationId, _baseRoute + '.' + command.Name))
                        {
                            var result = await command.ExecuteAsync(correlationId, parameters);
                            await SendResultAsync(response, result);
                        }
                    }
                    catch (Exception ex)
                    {
                        InstrumentError(correlationId, _baseRoute + '.' + command.Name, ex);
                        await SendErrorAsync(response, ex);
                    }
                });
            }
        }

    }
}