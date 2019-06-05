using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PipServices3.Rpc.Services
{
    public class Interceptor
    {
        public string Route { get; set; }

        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Task>, Task> Action
        {
            get;
            set;
        }
    }
}