using System.Threading;
using System.Threading.Tasks;

namespace XIVLauncher.OtpProviders
{
    public interface IOtpProvider
    { string Id { get; } string DisplayName { get; } Task<OtpProviderResult> GetOtpAsync(CancellationToken cancellationToken = default); }
}