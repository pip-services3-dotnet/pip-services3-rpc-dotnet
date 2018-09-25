using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices.Commons.Config;

namespace PipServices.Rpc.Services
{
    public class HeartbeatRestService: RestService
    {
        private string _route = "heartbeat";

        public HeartbeatRestService()
        {
        }

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            _route = config.GetAsStringWithDefault("route", _route);
        }

        public override void Register()
        {
            RegisterRoute("get", _route, Heartbeat);
        }

        private async Task Heartbeat(HttpRequest request, HttpResponse response, RouteData routeData)
        {
            await SendResultAsync(response, DateTime.UtcNow);
        }
    }
}
