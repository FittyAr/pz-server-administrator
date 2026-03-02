using System.Net.Sockets;
using System.Text;

namespace pz_server_administrator.Services;

public class RconService : IRconService, IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;

    private const int SERVERDATA_AUTH = 3;
    private const int SERVERDATA_EXECCOMMAND = 2;
    private const int SERVERDATA_RESPONSE_VALUE = 0;
    private const int SERVERDATA_AUTH_RESPONSE = 2; // Auth response is type 2

    private int _requestId = 1;
    private bool _isAuthenticated = false;

    public bool IsConnected => _client?.Connected == true && _isAuthenticated;

    public event Action<string>? OnMessageReceived;
    public event Action<bool>? OnConnectionStateChanged;

    public async Task<bool> ConnectAsync(string host, int port, string password)
    {
        try
        {
            if (_client != null)
            {
                await DisconnectAsync();
            }

            _client = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _client.ConnectAsync(host, port, cts.Token);
            _stream = _client.GetStream();
            _stream.ReadTimeout = 5000;
            _stream.WriteTimeout = 5000;

            // Send Authentication Packet
            int authId = ++_requestId;
            await SendPacketAsync(authId, SERVERDATA_AUTH, password);

            // Read Response
            var authResponse = await ReadPacketAsync();

            // Server might send an empty RESPONSE_VALUE right after AUTH packet, read it if it exists
            if (authResponse.Type == SERVERDATA_RESPONSE_VALUE)
            {
                authResponse = await ReadPacketAsync();
            }

            if (authResponse.Type == SERVERDATA_AUTH_RESPONSE && authResponse.Id != -1)
            {
                _isAuthenticated = true;
                OnConnectionStateChanged?.Invoke(true);
                return true;
            }

            if (authResponse.Id == -1)
            {
                throw new UnauthorizedAccessException("Contraseña RCON incorrecta.");
            }

            throw new Exception($"Respuesta de autenticación inesperada (Tipo: {authResponse.Type})");
        }
        catch (OperationCanceledException)
        {
            await DisconnectAsync();
            throw new Exception("Tiempo de espera agotado (Timeout). El servidor no respondió.");
        }
        catch (SocketException ex)
        {
            await DisconnectAsync();
            throw new Exception($"Error de red ({ex.SocketErrorCode}): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RconService] Error connecting: {ex.Message}");
            await DisconnectAsync();
            throw;
        }
    }

    public Task DisconnectAsync()
    {
        _isAuthenticated = false;
        _stream?.Close();
        _client?.Close();

        _stream = null;
        _client = null;

        OnConnectionStateChanged?.Invoke(false);
        return Task.CompletedTask;
    }

    public async Task<string> SendCommandAsync(string command)
    {
        if (!IsConnected || _stream == null)
            return "Error: No conectado a RCON.";

        try
        {
            int reqId = ++_requestId;
            await SendPacketAsync(reqId, SERVERDATA_EXECCOMMAND, command);

            // RCON protocol responses can be split into multiple packets
            // We usually wait for the actual response.
            var response = await ReadPacketAsync();

            if (response.Id == reqId && response.Type == SERVERDATA_RESPONSE_VALUE)
            {
                OnMessageReceived?.Invoke(response.Body);
                return response.Body;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RconService] Error sending command: {ex.Message}");
            await DisconnectAsync();
            return $"Error: {ex.Message}";
        }
    }

    private async Task SendPacketAsync(int id, int type, string body)
    {
        if (_stream == null) return;

        // Ensure string is ASCII/UTF8
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        // Size = 4(ID) + 4(Type) + body.Length + 1(null) + 1(null)
        int size = 10 + bodyBytes.Length;

        byte[] packet = new byte[size + 4];

        BitConverter.GetBytes(size).CopyTo(packet, 0); // Size
        BitConverter.GetBytes(id).CopyTo(packet, 4);   // RequestID
        BitConverter.GetBytes(type).CopyTo(packet, 8); // Type

        bodyBytes.CopyTo(packet, 12);                  // Body
        packet[12 + bodyBytes.Length] = 0x00;          // Null terminator string 1
        packet[13 + bodyBytes.Length] = 0x00;          // Null terminator string 2

        await _stream.WriteAsync(packet, 0, packet.Length);
        await _stream.FlushAsync();
    }

    private async Task<(int Size, int Id, int Type, string Body)> ReadPacketAsync()
    {
        if (_stream == null) throw new InvalidOperationException("Not connected.");

        byte[] sizeBuffer = new byte[4];
        int bytesRead = await _stream.ReadAsync(sizeBuffer, 0, 4);
        if (bytesRead < 4) throw new EndOfStreamException("Connection closed while reading packet size.");

        int size = BitConverter.ToInt32(sizeBuffer, 0);

        byte[] packetBuffer = new byte[size];
        int totalRead = 0;

        while (totalRead < size)
        {
            int read = await _stream.ReadAsync(packetBuffer, totalRead, size - totalRead);
            if (read == 0) throw new EndOfStreamException("Connection closed while reading packet data.");
            totalRead += read;
        }

        int id = BitConverter.ToInt32(packetBuffer, 0);
        int type = BitConverter.ToInt32(packetBuffer, 4);

        // Find the first null terminator for the body
        int bodyLength = 0;
        for (int i = 8; i < size; i++)
        {
            if (packetBuffer[i] == 0)
            {
                bodyLength = i - 8;
                break;
            }
        }

        string body = Encoding.UTF8.GetString(packetBuffer, 8, bodyLength);

        return (size, id, type, body);
    }

    public void Dispose()
    {
        _ = DisconnectAsync();
    }
}
