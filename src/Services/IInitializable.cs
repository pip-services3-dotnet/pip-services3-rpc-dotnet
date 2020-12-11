using Microsoft.AspNetCore.Builder;

namespace PipServices3.Rpc.Services
{
    public interface IInitializable
    {
        void Initialize(IApplicationBuilder applicationBuilder);
    }
}
