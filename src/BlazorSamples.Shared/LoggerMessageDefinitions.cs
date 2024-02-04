using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace BlazorSamples.Shared;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 42001, Level = LogLevel.Debug, Message = "Enter   - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogEnter(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42002, Level = LogLevel.Debug, Message = "Exit    - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogExit(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42003, Level = LogLevel.Debug, Message = "Await   - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogAwait(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42004, Level = LogLevel.Debug, Message = "Receive - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogReceive(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
}
