using Microsoft.EntityFrameworkCore;
using pz_server_administrator.Data.Database.Players;
using pz_server_administrator.Data.Database.Vehicles;
using pz_server_administrator.Data.Database.ServerTest;
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
        var dir = GetServerDirectory();
        if (string.IsNullOrEmpty(dir)) return null;

        var path = Path.Combine(dir, "Saves", "Multiplayer", "servertest", "players.db");
        if (!File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<PlayersContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new PlayersContext(options);
    }

    public VehiclesContext? CreateVehiclesContext()
    {
        var dir = GetServerDirectory();
        if (string.IsNullOrEmpty(dir)) return null;

        var path = Path.Combine(dir, "Saves", "Multiplayer", "servertest", "vehicles.db");
        if (!File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<VehiclesContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new VehiclesContext(options);
    }

    public ServerTestContext? CreateServerTestContext()
    {
        var dir = GetServerDirectory();
        if (string.IsNullOrEmpty(dir)) return null;

        var path = Path.Combine(dir, "db", "servertest.db");
        if (!File.Exists(path)) return null;

        var options = new DbContextOptionsBuilder<ServerTestContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        return new ServerTestContext(options);
    }
}
