using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Vehicles;

public partial class Vehicle
{
    public int Id { get; set; }

    public int? Wx { get; set; }

    public int? Wy { get; set; }

    public double? X { get; set; }

    public double? Y { get; set; }

    public int? Worldversion { get; set; }

    public byte[]? Data { get; set; }
}
