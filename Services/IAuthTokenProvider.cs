namespace MazeEscape.Services
{
    /// <summary>
    /// Manages JWT token lifecycle: obtains tokens from API, refreshes on expiration,
    /// and stores securely on device.
    /// </summary>
    public interface IAuthTokenProvider
    {
        /// <summary>
        /// Gets a valid, non-expired JWT token. Automatically refreshes if needed.
        /// </summary>
        /// <returns>Valid JWT Bearer token</returns>
        /// <exception cref="HttpRequestException">If token refresh fails due to network or API error</exception>
        Task<string> GetValidTokenAsync();

        /// <summary>
        /// Clears any cached token and secure storage. Forces fresh token on next GetValidTokenAsync call.
        /// </summary>
        Task ClearTokenAsync();
    }
}
