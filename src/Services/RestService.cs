using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Components.Count;
using PipServices.Components.Log;

namespace PipServices.Rpc.Services
{
    public abstract class RestService: IOpenable, IConfigurable, IReferenceable, IUnreferenceable, IRegisterable
    {
        private static readonly ConfigParams _defaultConfig = ConfigParams.FromTuples(
            "base_route", "",
            "dependencies.endpoint", "pip-services:endpoint:http:*:1.0"
        );

        protected HttpEndpoint _endpoint;    
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected DependencyResolver _dependencyResolver = new DependencyResolver(_defaultConfig);
        protected string _baseRoute;

        private ConfigParams _config;
        private IReferences _references;
        private bool _localEndpoint;
        private bool _opened;

        public virtual void Configure(ConfigParams config)
        {
            _config = config.SetDefaults(_defaultConfig);
            _dependencyResolver.Configure(config);

            _baseRoute = config.GetAsStringWithDefault("base_route", _baseRoute);
        }

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

        protected Timing Instrument(string correlationId, string name)
        {
            _logger.Trace(correlationId, "Executing {0} method", name);
            return _counters.BeginTiming(name + ".exec_time");
        }

        public bool IsOpen()
        {
            return _opened;
        }

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

        protected Task SendErrorAsync(HttpResponse response, Exception ex)
        {
            return HttpResponseSender.SendErrorAsync(response, ex);
        }

        protected Task SendResultAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendResultAsync(response, result);
        }

        protected Task SendEmptyResultAsync(HttpResponse response)
        {
            return HttpResponseSender.SendEmptyResultAsync(response);
        }

        protected Task SendCreatedResultAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendCreatedResultAsync(response, result);
        }

        protected Task SendDeletedAsync(HttpResponse response, object result)
        {
            return HttpResponseSender.SendDeletedResultAsync(response, result);
        }

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
