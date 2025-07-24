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
        /// Gets the translation for a given key in the current language.
        /// </summary>
        /// <param name="key">The key of the translation string.</param>
        /// <returns>The translated string, or the key itself if not found.</returns>
        string Get(string key);

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
    }
}