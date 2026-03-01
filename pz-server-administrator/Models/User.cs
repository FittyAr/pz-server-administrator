namespace pz_server_administrator.Models;

/// <summary>
/// Complete configuration structure from appsettings.json
/// </summary>
public class ZsmConfiguration
{
    /// <summary>
    /// Application settings
    /// </summary>
    public AppSettings AppSettings { get; set; } = new();

    /// <summary>
    /// List of system users
    /// </summary>
    public List<AuthUser> Users { get; set; } = new();

    /// <summary>
    /// Role permissions configuration
    /// </summary>
    public Dictionary<string, RolePermissions> Roles { get; set; } = new();

    /// <summary>
    /// AI Agent settings
    /// </summary>
    public AiConfiguration Ai { get; set; } = new();

    /// <summary>
    /// Community cloud settings
    /// </summary>
    public CloudConfiguration Cloud { get; set; } = new();
}

public enum AiProviderType
{
    None,
    Gemini,
    OpenAI,
    Anthropic,
    Ollama
}

public class AiConfiguration
{
    public AiProviderType Provider { get; set; } = AiProviderType.None;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty; // e.g. "gemini-1.5-flash", "gpt-4o"
    public string CustomEndpoint { get; set; } = string.Empty; // For Ollama or local APIs
    public bool IsApiKeyValid { get; set; }
    public bool AiAutoFixEnabled { get; set; }
}

public class CloudConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public bool IsApiKeyValid { get; set; }
    public bool CloudSyncEnabled { get; set; }
    public bool AutoReport { get; set; }
    public DateTime? LastSync { get; set; }
}