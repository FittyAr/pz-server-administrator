using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace pz_server_administrator.Services
{
    /// <summary>
    /// Service to manage application localization.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly IWebHostEnvironment _env;
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        private string _currentLanguage = "en"; // Default language

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="env">The web host environment.</param>
        public LocalizationService(IWebHostEnvironment env)
        {
            _env = env;
            // The language loading is now fully async. 
            // We need a separate method to initialize the service.
        }

        public async Task InitializeAsync()
        {
            LoadAvailableLanguages();
            await LoadLanguage(_currentLanguage);
        }

        /// <inheritdoc />
        public string Get(string key)
        {
            return _translations.TryGetValue(key, out var value) ? value : key;
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
                await LoadLanguage(language);
            }
        }

        private void LoadAvailableLanguages()
        {
            var langDirPath = Path.Combine(_env.ContentRootPath, "..", "config", "lang");
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
            var langFilePath = Path.Combine(_env.ContentRootPath, "..", "config", "lang", $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
        }
    }
}