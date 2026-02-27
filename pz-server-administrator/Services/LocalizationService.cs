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
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        private string _currentLanguage = "en"; // Default language

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="env">The web host environment.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public LocalizationService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            // The language loading is now fully async. 
            // We need a separate method to initialize the service.
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("[LocalizationService] InitializeAsync started");
            LoadAvailableLanguages();
            Console.WriteLine($"[LocalizationService] Available languages loaded: {string.Join(", ", AvailableLanguages)}");

            if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Language", out var language) ?? false)
            {
                _currentLanguage = language;
                Console.WriteLine($"[LocalizationService] Language from cookie: {language}");
            }
            else
            {
                Console.WriteLine($"[LocalizationService] No language cookie found, using default: {_currentLanguage}");
            }

            await LoadLanguage(_currentLanguage);
            Console.WriteLine($"[LocalizationService] Language loaded: {_currentLanguage}, translations count: {_translations.Count}");
            // No disparamos OnLanguageChanged durante la inicialización
            // ya que no hay componentes suscritos aún
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
            Console.WriteLine($"[LocalizationService] SetLanguage called with: {language}");
            Console.WriteLine($"[LocalizationService] Available languages: {string.Join(", ", AvailableLanguages)}");

            if (AvailableLanguages.Contains(language))
            {
                Console.WriteLine($"[LocalizationService] Language {language} found, changing from {_currentLanguage}");
                _currentLanguage = language;
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("Language", language);
                await LoadLanguage(language);
                Console.WriteLine($"[LocalizationService] Language loaded, translations count: {_translations.Count}");
                OnLanguageChanged?.Invoke();
                Console.WriteLine($"[LocalizationService] OnLanguageChanged event invoked");
            }
            else
            {
                Console.WriteLine($"[LocalizationService] Language {language} not found in available languages");
            }
        }

        private void LoadAvailableLanguages()
        {
            var configDir = GetConfigDirectory();
            if (string.IsNullOrEmpty(configDir)) return;

            var langDirPath = Path.Combine(configDir, "lang");
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
            var configDir = GetConfigDirectory();
            if (string.IsNullOrEmpty(configDir)) return;

            var langFilePath = Path.Combine(configDir, "lang", $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
        }

        public async Task<string> GetLanguageNameAsync(string language)
        {
            var configDir = GetConfigDirectory();
            if (string.IsNullOrEmpty(configDir)) return language;

            var langFilePath = Path.Combine(configDir, "lang", $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (translations != null && translations.TryGetValue("Language.Name", out var name))
                {
                    return name;
                }
            }
            return language; // Fallback to language code
        }

        /// <summary>
        /// Gets the 'config' directory. Checks ContentRootPath first (where build copies files),
        /// then walks up directories looking for a config/lang folder.
        /// </summary>
        private string? GetConfigDirectory()
        {
            // First: check if config/lang exists directly in ContentRootPath (copied by MSBuild)
            var directPath = Path.Combine(_env.ContentRootPath, "config");
            if (Directory.Exists(Path.Combine(directPath, "lang")))
            {
                Console.WriteLine($"[LocalizationService] Found config at ContentRootPath: {directPath}");
                return directPath;
            }

            // Fallback: walk up directories looking for config/lang (dev scenario without file copy)
            var directory = new DirectoryInfo(_env.ContentRootPath);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "config");
                if (Directory.Exists(Path.Combine(candidate, "lang")))
                {
                    Console.WriteLine($"[LocalizationService] Found config by walking up: {candidate}");
                    return candidate;
                }
                directory = directory.Parent;
            }

            Console.WriteLine($"[LocalizationService] WARNING: config/lang directory not found from {_env.ContentRootPath}!");
            return null;
        }
    }
}