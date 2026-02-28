using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Players;

public partial class NetworkPlayer
{
    public int Id { get; set; }

    public string? World { get; set; }

    public string? Username { get; set; }

    public int? PlayerIndex { get; set; }

    public string? Name { get; set; }

    public string? Steamid { get; set; }

    public double? X { get; set; }

    public double? Y { get; set; }

    public double? Z { get; set; }

    public int? Worldversion { get; set; }

    public byte[]? Data { get; set; }

    public bool? IsDead { get; set; }
}
