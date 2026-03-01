using System.Text.Json;

namespace pz_server_administrator.Services
{
    /// <summary>
    /// Service to manage application localization.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        public event Action? OnLanguageChanged;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfigurationService _configService;
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        private string _currentLanguage = "es"; // Default language es

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        public LocalizationService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IConfigurationService configService)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _configService = configService;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("[LocalizationService] InitializeAsync started");
            LoadAvailableLanguages();
            Console.WriteLine($"[LocalizationService] Available languages loaded: {string.Join(", ", AvailableLanguages)}");

            // Prioridad 1: Cookie (sesión actual)
            if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Language", out var language) ?? false)
            {
                _currentLanguage = language;
                Console.WriteLine($"[LocalizationService] Language from cookie: {language}");
            }
            // Prioridad 2: Configuración persistente
            else
            {
                var config = _configService.GetConfiguration();
                if (!string.IsNullOrEmpty(config.AppSettings.Language))
                {
                    _currentLanguage = config.AppSettings.Language;
                    Console.WriteLine($"[LocalizationService] Language from config: {_currentLanguage}");
                }
            }

            await LoadLanguage(_currentLanguage);
            Console.WriteLine($"[LocalizationService] Language loaded: {_currentLanguage}, translations count: {_translations.Count}");
        }

        /// <inheritdoc />
        public string Get(string key)
        {
            return _translations.TryGetValue(key, out var value) ? value : key;
        }

        /// <inheritdoc />
        public string Get(string key, params object[] args)
        {
            var template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template; // Return unformatted string if formatting fails
            }
        }

        /// <inheritdoc />
        public string CurrentLanguage => _currentLanguage;

        /// <inheritdoc />
        public IEnumerable<string> AvailableLanguages { get; private set; } = new List<string>();

        /// <inheritdoc />
        public async Task SetLanguage(string language)
        {
            if (AvailableLanguages.Contains(language))
            {
                _currentLanguage = language;

                try
                {
                    if (_httpContextAccessor.HttpContext != null && !_httpContextAccessor.HttpContext.Response.HasStarted)
                    {
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("Language", language);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LocalizationService] Error saving language cookie: {ex.Message}");
                }

                await LoadLanguage(language);

                // Persistir en AppSettings
                var config = _configService.GetConfiguration();
                if (config.AppSettings.Language != language)
                {
                    config.AppSettings.Language = language;
                    await _configService.SaveConfigurationAsync(config);
                }

                OnLanguageChanged?.Invoke();
            }
        }

        private void LoadAvailableLanguages()
        {
            var langDirPath = GetLangDirectory();
            if (string.IsNullOrEmpty(langDirPath)) return;

            if (Directory.Exists(langDirPath))
            {
                AvailableLanguages = Directory.GetFiles(langDirPath, "*.json")
                                              .Select(Path.GetFileNameWithoutExtension)
                                              .Where(lang => !string.IsNullOrEmpty(lang))
                                              .ToList()!;
            }
        }

        private async Task LoadLanguage(string language)
        {
            var langDirPath = GetLangDirectory();
            if (string.IsNullOrEmpty(langDirPath)) return;

            var langFilePath = Path.Combine(langDirPath, $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
        }

        public async Task<string> GetLanguageNameAsync(string language)
        {
            var langDirPath = GetLangDirectory();
            if (string.IsNullOrEmpty(langDirPath)) return language;

            var langFilePath = Path.Combine(langDirPath, $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (translations != null && translations.TryGetValue("Language.Name", out var name))
                {
                    return name;
                }
            }
            return language;
        }

        private string? GetLangDirectory()
        {
            var directPath = Path.Combine(_env.ContentRootPath, "Resources", "lang");
            if (Directory.Exists(directPath)) return directPath;

            var directory = new DirectoryInfo(_env.ContentRootPath);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Resources", "lang");
                if (Directory.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }

            return null;
        }
    }
}