namespace POS.WebAPI.Configuration;

/// <summary>
/// Rate limiting configuration settings
/// </summary>
public class RateLimitOptions
{
    public const string Section = "RateLimit";

    /// <summary>
    /// Enable rate limiting
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// General rate limit settings
    /// </summary>
    public GeneralRules General { get; set; } = new();

    /// <summary>
    /// Authentication endpoint rate limit settings
    /// </summary>
    public AuthRules Auth { get; set; } = new();

    public class GeneralRules
    {
        /// <summary>
        /// Maximum number of requests per time window
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Time window in seconds
        /// </summary>
        public int Window { get; set; } = 60;
    }

    public class AuthRules
    {
        /// <summary>
        /// Maximum number of login attempts per time window
        /// </summary>
        public int PermitLimit { get; set; } = 5;

        /// <summary>
        /// Time window in seconds
        /// </summary>
        public int Window { get; set; } = 300; // 5 minutes
    }
}
