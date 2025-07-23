# Módulo RCON

Este módulo proporciona control remoto del servidor dedicado mediante el protocolo RCON.

## Objetivos

- Enviar comandos al servidor sin necesidad de acceso físico o SSH.
- Mostrar respuestas en una consola integrada.
- Proveer historial de comandos y respuestas.

## Funcionalidad

- Autenticación con host, puerto y contraseña.
- Envío de comandos personalizados o predefinidos:
  - kick, ban, say, quit, etc.
- Acceso controlado por rol: solo disponible para moderadores y administradores.

## Consideraciones Técnicas

- Evaluar uso de bibliotecas existentes compatibles con .NET para RCON.
- En caso de implementación personalizada: utilizar TCP sockets y manejar handshake del protocolo.

## Próximos pasos

- Definir comandos soportados por PZ.
- Implementar cliente RCON básico.
- Consola visual integrada en Blazor.
