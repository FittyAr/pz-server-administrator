using System.Threading.Tasks;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Interfaz para la comunicación con la plataforma central de la comunidad.
/// Permite reportar configuraciones funcionales y obtener presets globales.
/// </summary>
public interface ICommunityService
{
    /// <summary>
    /// Envía la configuración actual de mods al servidor central (si el perfil lo permite).
    /// </summary>
    Task ReportConfigurationAsync(string apiKey, List<ModInstance> activeMods);

    /// <summary>
    /// Obtiene recomendaciones de mods o presets basados en la tendencia global.
    /// </summary>
    Task<string> GetGlobalRecommendationsAsync();

    /// <summary>
    /// Sincroniza la lista negra de incompatibilidades desde la nube.
    /// </summary>
    Task SyncIncompatibilitiesAsync(string apiKey);
}
