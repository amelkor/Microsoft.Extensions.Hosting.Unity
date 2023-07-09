using System.Threading;

#if UNITASK_ENABLED
using Cysharp.Threading.Tasks;
#endif

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IUnityHost : IHost
    {
#if UNITASK_ENABLED
        new UniTask StartAsync(CancellationToken cancellationToken = default);
        new UniTask StopAsync(CancellationToken cancellationToken = default);
#endif
    }
}