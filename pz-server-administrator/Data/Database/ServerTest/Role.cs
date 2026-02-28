using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double ColorR { get; set; }

    public double ColorG { get; set; }

    public double ColorB { get; set; }

    public bool? Readonly { get; set; }

    public int Position { get; set; }
}
