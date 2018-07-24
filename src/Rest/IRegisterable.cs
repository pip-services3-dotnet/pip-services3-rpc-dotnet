using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PipServices.Rpc.Services
{
    public interface IRegisterable
    {
        void Register();
    }
}
