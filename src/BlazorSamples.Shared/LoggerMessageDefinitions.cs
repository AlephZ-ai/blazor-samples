using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace BlazorSamples.Shared;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 42000, Level = LogLevel.Error, Message = "Exception     - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogException(this ILogger log, Exception exception, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42001, Level = LogLevel.Debug, Message = "Enter         - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogEnter(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42002, Level = LogLevel.Debug, Message = "Exit          - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogExit(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42003, Level = LogLevel.Debug, Message = "Loop          - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogLoop(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42004, Level = LogLevel.Debug, Message = "Await         - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogAwait(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42005, Level = LogLevel.Debug, Message = "Yield         - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogYield(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42006, Level = LogLevel.Debug, Message = "Received      - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogReceived(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42007, Level = LogLevel.Debug, Message = "Sent          - Member: {callingMember}, Line: {callingLine}, File: {callingFile}")]
    public static partial void LogSent(this ILogger log, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
    [LoggerMessage(EventId = 42008, Level = LogLevel.Debug, Message = "Buffer Resize - Member: {callingMember}, Line: {callingLine}, File: {callingFile}; Original Size: {originalSize}, New Size: {newSize}")]
    public static partial void LogBufferResize(this ILogger log, int originalSize, int newSize, [CallerMemberName] string callingMember = "", [CallerLineNumber] int callingLine = 0, [CallerFilePath] string callingFile = "");
}
