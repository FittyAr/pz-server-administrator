using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Whitelist
{
    public int Id { get; set; }

    public string? World { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? LastConnection { get; set; }

    public int Role { get; set; }

    public int? AuthType { get; set; }

    public string? GoogleKey { get; set; }

    public string? Steamid { get; set; }

    public string? Ownerid { get; set; }

    public string? DisplayName { get; set; }
}
