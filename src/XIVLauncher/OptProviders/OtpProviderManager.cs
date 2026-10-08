using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;


namespace XIVLauncher.OtpProviders
{
    // handles OTP provider loading and execution. 
    // strictly validates 6-digit codes only. 
    // all failures are logged and returned as errors; nothing crashes XL
    public sealed class OtpProviderManager
    {
        private readonly IReadOnlyList<IOtpProvider> providers;

        public OtpProviderManager(IEnumerable<IOtpProvider> providers)
        // Sanitize input, never null out the list
        {
            this.providers = providers?.ToList() ?? new List<IOtpProvider>();
        }

        public IReadOnlyList<IOtpProvider> Providers => providers;

        public IOtpProvider GetProvider(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return null;

            return providers.FirstOrDefault(provider => string.Equals(provider.Id, providerId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<OtpProviderResult> GetOtpAsync(
            string providerId,
            CancellationToken cancellationToken = default)
        {
            var provider = GetProvider(providerId);

            if (provider == null)
            {
                return new OtpProviderResult
                { Success = false, ErrorMessage = "OTP provider not found.", };
            }

            try
            {
                var result = await provider.GetOtpAsync(cancellationToken);

                if (result == null)
                {
                    return new OtpProviderResult { Success = false, ErrorMessage = "OTP provider returned no result.", };
                }

                if (!result.Success) return result;
                // strict 6-digit numeric check. no exceptions.
                if (string.IsNullOrWhiteSpace(result.Otp) || result.Otp.Length != 6 || !result.Otp.All(char.IsDigit))
                {
                    Log.Warning("OTP provider {ProviderId} returned an invalid OTP.", provider.Id);
                    return new OtpProviderResult { Success = false, ErrorMessage = "OTP provider returned an invalid code.", };
                }
                return result;
            }
            catch (OperationCanceledException)
            // catch everything. log it. fail safe.
            {
                return new OtpProviderResult { Success = false, ErrorMessage = "OTP request was cancelled.", };
            }
            catch (Exception ex)
            // if no otp provier found, it logs it and comes back as a failure instead of crashing XL.
            {
                Log.Warning(ex, "OTP provider {ProviderId} failed.", provider.Id);
                return new OtpProviderResult { Success = false, ErrorMessage = "OTP provider failed.", };
            }
        }
    }
}