namespace TelegramBudget.Services.Trace;

public interface ITraceService : IDisposable
{
    long Milliseconds { get; }

    void LogTrace(
        string key,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);

    ITraceService Create(
        string? key = null,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "");

    void LogDebugAll();
}