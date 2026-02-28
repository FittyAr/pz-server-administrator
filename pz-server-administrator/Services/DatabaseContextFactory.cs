using Microsoft.EntityFrameworkCore;
using pz_server_administrator.Data.Database.Players;
using pz_server_administrator.Data.Database.Vehicles;
using pz_server_administrator.Data.Database.ServerTest;
using pz_server_administrator.Data.Database.Mods;
using System.IO;

namespace pz_server_administrator.Services;

public class DatabaseContextFactory : IDatabaseContextFactory
{
    private readonly IConfigurationService _configurationService;
    private readonly IPzServerService _pzServerService;

    public DatabaseContextFactory(IConfigurationService configurationService, IPzServerService pzServerService)
    {
        _configurationService = configurationService;
        _pzServerService = pzServerService;
    }

    private string? GetServerDirectory()
    {
        var config = _configurationService.GetConfiguration();
        return config?.AppSettings?.ServerDirectoryPath;
    }

    public PlayersContext? CreatePlayersContext()
    {
        var path = _configurationService.GetConfiguration()?.AppSettings?.PlayersDatabasePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<PlayersContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new PlayersContext(options);
    }

    public VehiclesContext? CreateVehiclesContext()
    {
        var path = _configurationService.GetConfiguration()?.AppSettings?.VehiclesDatabasePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<VehiclesContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new VehiclesContext(options);
    }

    public ServerTestContext? CreateServerTestContext()
    {
        var path = _configurationService.GetConfiguration()?.AppSettings?.ServerTestDatabasePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<ServerTestContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new ServerTestContext(options);
    }

    public ModsContext? CreateModsContext()
    {
        var path = _configurationService.GetConfiguration()?.AppSettings?.ModsDatabasePath;

        // Si no está configurado, usamos uno por defecto en el directorio de la aplicación o config
        if (string.IsNullOrEmpty(path))
        {
            // Intentar usar el mismo directorio que appsettings.json si es posible, pero aquí no lo sabemos directamente.
            // Para simplificar por ahora, si está vacío intentamos crearlo en la raíz del contenido.
            // En producción, debería estar pre-configurado o autodetectado.
            path = "Mods.db";
        }

        var options = new DbContextOptionsBuilder<ModsContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        var context = new ModsContext(options);

        // Aseguramos que la DB y el esquema existan (archivo propio de la app)
        context.Database.EnsureCreated();

        return context;
    }
}
