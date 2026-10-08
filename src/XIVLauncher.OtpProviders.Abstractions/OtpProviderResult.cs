namespace XIVLauncher.OtpProviders
{
    public sealed class OtpProviderResult
    { public bool Success { get; init; } public string? Otp { get; init; } public string? ErrorMessage { get; init; } }
}