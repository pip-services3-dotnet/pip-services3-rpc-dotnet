using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using PipServices3.Commons.Errors;
using PipServices3.Rpc.Services;

namespace PipServices3.Rpc.Auth
{
    public class OwnerAuthorizer
    {
        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<Task>, Task> Owner(
            string idParam = "user_id")
        {
            return async (request, response, user, routeData, next) =>
            {
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    await HttpResponseSender.SendErrorAsync(
                        response,
                        new UnauthorizedException(
                            null, "NOT_SIGNED",
                            "User must be signed in to perform this operation"
                        ).WithStatus(401)
                    );
                }
                else
                {
                    var identity = user.Identity as ClaimsIdentity;
                    var userIdClaim = identity?.Claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");

                    if (!request.Query.TryGetValue(idParam, out StringValues userId) ||
                        userIdClaim?.Value != userId.ToString())
                    {
                        await HttpResponseSender.SendErrorAsync(
                            response,
                            new UnauthorizedException(
                                null, "FORBIDDEN",
                                "Only data owner can perform this operation"
                            ).WithStatus(403)
                        );
                    }
                    else
                    {
                        await next();
                    }
                }
            };
        }

        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<Task>, Task> OwnerOrAdmin(
            string idParam = "user_id")
        {
            return async (request, response, user, routeData, next) =>
            {
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    await HttpResponseSender.SendErrorAsync(
                        response,
                        new UnauthorizedException(
                            null, "NOT_SIGNED",
                            "User must be signed in to perform this operation"
                        ).WithStatus(401)
                    );
                }
                else
                {
                    var identity = user.Identity as ClaimsIdentity;
                    var userIdClaim = identity?.Claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");
                    var isAdmin = user.IsInRole("Admin") || user.IsInRole("admin");
                    
                    if (!request.Query.TryGetValue(idParam, out StringValues userId) ||
                        userIdClaim?.Value != userId.ToString() && !isAdmin)
                    {
                        await HttpResponseSender.SendErrorAsync(
                            response,
                            new UnauthorizedException(
                                null, "FORBIDDEN",
                                "Only data owner can perform this operation"
                            ).WithStatus(403)
                        );
                    }
                    else
                    {
                        await next();
                    }
                }
            };
        }
    }
}