namespace TelegramBudget.Services.Trace;

public interface ITraceService : IDisposable
{
    long Milliseconds { get; }

    void Log(
        LogLevel logLevel,
        string key,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);

    ITraceService Scope(
        string? key = null,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "");

    ITraceService Fixed(string key);

    void LogAll(LogLevel logLevel);
    void LogSynced(LogLevel logLevel);
}