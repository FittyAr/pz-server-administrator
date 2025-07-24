using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
            LoadAvailableLanguages();

            if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("Language", out var language) ?? false)
            {
                _currentLanguage = language;
            }

            await LoadLanguage(_currentLanguage);
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
            if (AvailableLanguages.Contains(language))
            {
                _currentLanguage = language;
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("Language", language);
                await LoadLanguage(language);
                OnLanguageChanged?.Invoke();
            }
        }

        private void LoadAvailableLanguages()
        {
            var solutionDir = GetSolutionDirectory();
            if (string.IsNullOrEmpty(solutionDir)) return;

            var langDirPath = Path.Combine(solutionDir, "config", "lang");
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
            var solutionDir = GetSolutionDirectory();
            if (string.IsNullOrEmpty(solutionDir)) return;

            var langFilePath = Path.Combine(solutionDir, "config", "lang", $"{language}.json");
            if (File.Exists(langFilePath))
            {
                var json = await File.ReadAllTextAsync(langFilePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
        }

        public async Task<string> GetLanguageNameAsync(string language)
        {
            var solutionDir = GetSolutionDirectory();
            if (string.IsNullOrEmpty(solutionDir)) return language;

            var langFilePath = Path.Combine(solutionDir, "config", "lang", $"{language}.json");
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

        private string? GetSolutionDirectory()
        {
            var directory = new DirectoryInfo(_env.ContentRootPath);
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory?.FullName;
        }
    }
}