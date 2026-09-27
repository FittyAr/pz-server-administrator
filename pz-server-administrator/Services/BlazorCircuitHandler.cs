using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

namespace pz_server_administrator.Services;

/// <summary>
/// Handler para registrar en Serilog el ciclo de vida completo de los circuitos de Blazor Server,
/// facilitando el diagnóstico inmediato de desconexiones o fallos.
/// </summary>
public class BlazorCircuitHandler : CircuitHandler
{
    private readonly ILogger<BlazorCircuitHandler> _logger;

    public BlazorCircuitHandler(ILogger<BlazorCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[BlazorCircuit] Circuito iniciado: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogDebug("[BlazorCircuit] Conexión SignalR activa para circuito: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogWarning("[BlazorCircuit] Conexión SignalR interrumpida para circuito: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[BlazorCircuit] Circuito cerrado: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }
}
