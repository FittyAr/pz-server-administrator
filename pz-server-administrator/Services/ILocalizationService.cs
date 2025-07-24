using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pz_server_administrator.Services
{
    /// <summary>
    /// Defines the contract for a service that provides localization (translation) functionalities.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Event that is triggered when the language or translations change.
        /// </summary>
        event Action? OnLanguageChanged;
        /// <summary>
        /// Gets the translation for a given key in the current language.
        /// </summary>
        /// <param name="key">The key of the translation string.</param>
        /// <returns>The translated string, or the key itself if not found.</returns>
        string Get(string key);

        /// <summary>
        /// Gets the translation for a given key in the current language and formats it with the provided arguments.
        /// </summary>
        /// <param name="key">The key of the translation string.</param>
        /// <param name="args">The arguments to format the string with.</param>
        /// <returns>The translated and formatted string, or the key itself if not found.</returns>
        string Get(string key, params object[] args);

        /// <summary>
        /// Gets the currently selected language.
        /// </summary>
        string CurrentLanguage { get; }

        /// <summary>
        /// Gets the list of available languages.
        /// </summary>
        IEnumerable<string> AvailableLanguages { get; }

        /// <summary>
        /// Sets the current language.
        /// </summary>
        /// <param name="language">The language to set (e.g., "en", "es").</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetLanguage(string language);

        /// <summary>
        /// Initializes the service asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Gets the name of a specific language from its localization file.
        /// </summary>
        /// <param name="language">The language code (e.g., "en", "es").</param>
        /// <returns>The name of the language.</returns>
        Task<string> GetLanguageNameAsync(string language);
    }
}