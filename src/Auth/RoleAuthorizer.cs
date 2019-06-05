using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices3.Commons.Errors;
using PipServices3.Rpc.Services;

namespace PipServices3.Rpc.Auth
{
    public class RoleAuthorizer
    {
        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<Task>, Task> UserInRoles(
            string[] roles)
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
                    var authorized = false;

                    foreach (var role in roles)
                    {
                        if (user.IsInRole(role))
                        {
                            authorized = true;
                            break;
                        }
                    }

                    if (!authorized)
                    {
                        await HttpResponseSender.SendErrorAsync(
                            response,
                            new UnauthorizedException(
                                null, "NOT_IN_ROLE",
                                "User must be " + String.Join(" or ", roles) + " to perform this operation"
                            ).WithDetails("roles", roles).WithStatus(403)
                        );
                    }
                    else
                    {
                        await next();
                    }
                }
            };
        }

        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<Task>, Task> UserInRole(string role)
        {
            return UserInRoles(new string[] {role});
        }

        public Func<HttpRequest, HttpResponse, ClaimsPrincipal, RouteData, Func<Task>, Task> Admin()
        {
            return UserInRole("admin");
        }
    }
}