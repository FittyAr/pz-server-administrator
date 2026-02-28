using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Bannedid
{
    public string Steamid { get; set; } = null!;

    public string? Reason { get; set; }
}
