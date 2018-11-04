using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Commons.Run;
using PipServices3.Components.Count;
using PipServices3.Components.Log;

namespace PipServices3.Rpc.Services
{
    /// <summary>
    /// Abstract service that receives remove calls via HTTP/REST protocol.
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
    /// class MyRestService: RestService 
    /// {
    ///     private IMyController _controller;
    ///     ...
    ///     public MyRestService()
    ///     {
    ///         base();
    ///         this._dependencyResolver.put(
    ///         "controller", new Descriptor("mygroup", "controller", "*", "*", "1.0"));
    ///     }
    ///     
    ///     public void SetReferences(IReferences references)
    ///     {
    ///         base.SetReferences(references);
    ///         this._controller = this._dependencyResolver.getRequired<IMyController>("controller");
    ///     }
    ///     
    ///     public void register()
    ///     {
    ///         ...
    ///     }
    /// }
    /// 
    /// var service = new MyRestService();
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
    public abstract class RestService: IOpenable, IConfigurable, IReferenceable, IUnreferenceable, IRegisterable
    {
        private static readonly ConfigParams _defaultConfig = ConfigParams.FromTuples(
            "base_route", "",
            "dependencies.endpoint", "pip-services3:endpoint:http:*:1.0"
        );

        /// <summary>
        /// The HTTP endpoint that exposes this service.
        /// </summary>
        protected HttpEndpoint _endpoint;
        /// <summary>
        /// The logger.
        /// </summary>
        protected CompositeLogger _logger = new CompositeLogger();
        /// <summary>
        /// The performance counters.
        /// </summary>
        protected CompositeCounters _counters = new CompositeCounters();
        /// <summary>
        /// The dependency resolver.
        /// </summary>
        protected DependencyResolver _dependencyResolver = new DependencyResolver(_defaultConfig);
        /// <summary>
        /// The base route.
        /// </summary>
        protected string _baseRoute;

        private ConfigParams _config;
        private IReferences _references;
        private bool _localEndpoint;
        private bool _opened;

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public virtual void Configure(ConfigParams config)
        {
            _config = config.SetDefaults(_defaultConfig);
            _dependencyResolver.Configure(config);

            _baseRoute = config.GetAsStringWithDefault("base_route", _baseRoute);
        }

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public virtual void SetReferences(IReferences references)
        {
            _references = references;

            _logger.SetReferences(references);
            _counters.SetReferences(references);
            _dependencyResolver.SetReferences(references);

            // Get endpoint
            _endpoint = _dependencyResolver.GetOneOptional("endpoint") as HttpEndpoint;
            _localEndpoint = _endpoint == null;

            // Or create a local one
            if (_endpoint == null)
                _endpoint = CreateLocalEndpoint();

            // Add registration callback to the endpoint
            _endpoint.Register(this);
        }

        /// <summary>
        /// Unsets (clears) previously set references to dependent components.
        /// </summary>
        public virtual void UnsetReferences()
        {
            // Remove registration callback from endpoint
            if (_endpoint != null)
            {
                _endpoint.Unregister(this);
                _endpoint = null;
            }
        }

        private HttpEndpoint CreateLocalEndpoint()
        {
            var endpoint = new HttpEndpoint();
    
            if (_config != null)
                endpoint.Configure(_config);

            if (_references != null)
                endpoint.SetReferences(_references);
    
            return endpoint;
        }

        /// <summary>
        /// Adds instrumentation to log calls and measure call time. It returns a Timing
        /// object that is used to end the time measurement.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="name">a method name.</param>
        /// <returns>Timing object to end the time measurement.</returns>
        protected Timing Instrument(string correlationId, string name)
        {
            _logger.Trace(correlationId, "Executing {0} method", name);
            return _counters.BeginTiming(name + ".exec_time");
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>true if the component has been opened and false otherwise.</returns>
        public bool IsOpen()
        {
            return _opened;
        }

        /// <summary>
        /// Opens the component.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns></returns>
        public async virtual Task OpenAsync(string correlationId)
        {
            if (IsOpen()) return;

            if (_endpoint == null)
            {
                _endpoint = CreateLocalEndpoint();
                _endpoint.Register(this);
                _localEndpoint = true;
            }

            if (_localEndpoint)
            {
                await _endpoint.OpenAsync(correlationId).ContinueWith(task => 
                {
                    _opened = task.Exception == null;
                });
            }
            else
            {
                _opened = true;
            }
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns></returns>
        public virtual Task CloseAsync(string correlationId)
        {
            if (!IsOpen())
            {
                if (_endpoint == null)
                {
                    throw new InvalidStateException(correlationId, "NO_ENDPOINT", "HTTP endpoint is missing");
                }

                if (_localEndpoint)
                {
                    _endpoint.CloseAsync(correlationId);
                }

                _opened = false;
            }

            return Task.Delay(0);
        }

        /// <summary>
        /// Sends error serialized as ErrorDescription object and appropriate HTTP status
        /// code.If status code is not defined, it uses 500 status code.
        /// </summary>
        /// <param name="response">a Http response</param>
        /// <param name="ex">an error object to be sent.</param>
        protected Task SendErrorAsync(HttpResponse response, Exception ex)
        {
            return HttpResponseSender.SendErrorAsync(response, ex);
        }

        /// <summary>
        /// Sends error serialized as ErrorDescription object and appropriate HTTP status
        /// code.If status code is not defined, it uses 500 status code.
        /// </summary>
        /// <param name="response">a Http response</param>
        /// <param name="ex">an error object to be sent.</param>
        protected Task SendResultAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendResultAsync(response, result);
        }

        /// <summary>
        /// Creates a callback function that sends an empty result with 204 status code.
        /// If error occur it sends ErrorDescription with approproate status code.
        /// </summary>
        /// <param name="response">aHttp response</param>
        protected Task SendEmptyResultAsync(HttpResponse response)
        {
            return HttpResponseSender.SendEmptyResultAsync(response);
        }

        /// <summary>
        /// Creates a callback function that sends newly created object as JSON. That
        /// callack function call be called directly or passed as a parameter to business logic components.
        /// 
        /// If object is not null it returns 201 status code. For null results it returns
        /// 204 status code. If error occur it sends ErrorDescription with approproate status code.
        /// </summary>
        /// <param name="response">a Http response</param>
        /// <param name="result">a body object to created result</param>
        protected Task SendCreatedResultAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendCreatedResultAsync(response, result);
        }

        /// <summary>
        /// Creates a callback function that sends deleted object as JSON. That callack
        /// function call be called directly or passed as a parameter to business logic components.
        /// 
        /// If object is not null it returns 200 status code. For null results it returns
        /// 204 status code. If error occur it sends ErrorDescription with approproate status code.
        /// </summary>
        /// <param name="response">a Http response</param>
        /// <param name="result">a body object to deleted result</param>
        protected Task SendDeletedAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendDeletedResultAsync(response, result);
        }

        /// <summary>
        /// Registers a route in HTTP endpoint.
        /// </summary>
        /// <param name="method">HTTP method: "get", "head", "post", "put", "delete"</param>
        /// <param name="route">a command route. Base route will be added to this route</param>
        /// <param name="action">an action function that is called when operation is invoked.</param>
        protected virtual void RegisterRoute(string method, string route,
             Func<HttpRequest, HttpResponse, RouteData, Task> action)
        {
            if (_endpoint == null) return;

            if (route[0] != '/')
                route = "/" + route;

            if (_baseRoute != null && _baseRoute.Length > 0) {
                var baseRoute = _baseRoute;
                if (baseRoute[0] != '/')
                    baseRoute = "/" + baseRoute;
                route = baseRoute + route;
            }

            _endpoint.RegisterRoute(method, route, action);
        }    

        public virtual void Register()
        { }
    }
}
