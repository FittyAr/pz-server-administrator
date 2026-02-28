namespace pz_server_administrator.Services;

public interface IRconService
{
    bool IsConnected { get; }
    Task<bool> ConnectAsync(string host, int port, string password);
    Task DisconnectAsync();
    Task<string> SendCommandAsync(string command);

    // Event triggered when a new message is received (e.g., chat, log)
    event Action<string>? OnMessageReceived;

    // Event triggered when connection state changes
    event Action<bool>? OnConnectionStateChanged;
}
