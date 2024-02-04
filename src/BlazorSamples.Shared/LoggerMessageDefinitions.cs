using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace BlazorSamples.Shared;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 42001, Level = LogLevel.Debug, Message = "Enter   - {callingMember}")]
    public static partial void LogEnter(this ILogger log, [CallerMemberName] string callingMember = "");
    [LoggerMessage(EventId = 42002, Level = LogLevel.Debug, Message = "Exit    - {callingMember}")]
    public static partial void LogExit(this ILogger log, [CallerMemberName] string callingMember = "");
    [LoggerMessage(EventId = 42003, Level = LogLevel.Debug, Message = "Await   - {callingMember}")]
    public static partial void LogAwait(this ILogger log, [CallerMemberName] string callingMember = "");
    [LoggerMessage(EventId = 42004, Level = LogLevel.Debug, Message = "Receive - {callingMember}; ValueWebSocketReceiveResult: {result}; Memory<byte>: {buffer}")]
    public static partial void LogReceive(this ILogger log, ValueWebSocketReceiveResult result, Memory<byte> buffer, [CallerMemberName] string callingMember = "");
}
