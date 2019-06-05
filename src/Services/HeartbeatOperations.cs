using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PipServices3.Rpc.Services
{
    public class HeartbeatOperations: RestOperations
    {
        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, Task> GetHeartbeatOperation()
        {
            return async (req, res, user) => { await HeartbeatAsync(req, res, user); };
        }

        private async Task HeartbeatAsync(HttpRequest httpRequest, HttpResponse response, ClaimsPrincipal user)
        {
            await SendResultAsync(response, DateTime.UtcNow);
        }
    }
}