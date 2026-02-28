using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Bannedip
{
    public string Ip { get; set; } = null!;

    public string? Username { get; set; }

    public string? Reason { get; set; }
}
