using Microsoft.EntityFrameworkCore;
using pz_server_administrator.Data.Database.Players;
using pz_server_administrator.Data.Database.Vehicles;
using pz_server_administrator.Data.Database.ServerTest;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

public interface IDatabaseContextFactory
{
    PlayersContext? CreatePlayersContext();
    VehiclesContext? CreateVehiclesContext();
    ServerTestContext? CreateServerTestContext();
    ModsContext? CreateModsContext();
}
