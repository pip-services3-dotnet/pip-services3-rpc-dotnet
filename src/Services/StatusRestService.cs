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
    public class StatusRestService: RestService
    {
        private DateTime _startTime = DateTime.UtcNow;
        private IReferences _references;
        private ContextInfo _contextInfo;
        private string _route = "status";

        public StatusRestService()
        {
            _dependencyResolver.Put("context-info", new Descriptor("pip-services", "context-info", "default", "*", "1.0"));
        }

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            _route = config.GetAsStringWithDefault("route", _route);
        }

		public override void SetReferences(IReferences references)
		{
            _references = references;
            base.SetReferences(references);

            _contextInfo = _dependencyResolver.GetOneOptional<ContextInfo>("context-info");
		}

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
