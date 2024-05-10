using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace TelegramBudget.Services.Trace;

public class TraceService : ITraceService
{
    private TraceService? _parent;
    
    private bool _disposed;
    private readonly string _key;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentStack<TraceService> _stack;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly ConcurrentDictionary<string, ConcurrentBag<long>> _synced = new();

    public static ITraceService Create(
        string key,
        ILoggerFactory loggerFactory)
    {
        return new TraceService(key, loggerFactory, [], null);
    }

    private TraceService(
        string key,
        ILoggerFactory loggerFactory,
        IReadOnlyCollection<TraceService> stack,
        TraceService? parent)
    {
        _key = key;
        _loggerFactory = loggerFactory;
        _stack = new ConcurrentStack<TraceService>(stack);
        _logger = _loggerFactory.CreateLogger($"Trace.{_key}");
        _logger.LogTrace("[trace created {Milliseconds} ms] {Key}", _stopwatch.ElapsedMilliseconds, _key);
        _parent = parent;
    }

    public long Milliseconds => _stopwatch.ElapsedMilliseconds;

    public void LogTrace(
        string key,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    {
        var value = _stopwatch.ElapsedMilliseconds;
        _logger.LogTrace(
            "[{Milliseconds} ms] {Key}\n{MemberName}\n{SourceFilePath}:{SourceLineNumber}", 
            value, 
            key,
            memberName,
            sourceFilePath,
            sourceLineNumber);
    }

    public ITraceService Create(
        string? key = null,
        string memberName = "")
    {
        if (_stack.TryPeek(out var peek))
            return peek.Create(key, memberName);

        key = key switch
        {
            null => memberName switch
            {
                _ when !string.IsNullOrWhiteSpace(memberName) => $"{_key}_memberName",
                _ => $"{_key}_{Guid.NewGuid():N}"
            },
            _ => $"{_key}_{key}"
        };

        var trace = new TraceService(key, _loggerFactory, _stack.ToList().AsReadOnly(), this);
        _stack.Push(trace);

        return trace;
    }

    public void LogDebugAll()
    {
        var stringBuilder = new StringBuilder();
        foreach (var metrics in _synced.OrderBy(e => e.Key))
        {
            foreach (var value in metrics.Value)
            {
                stringBuilder.AppendLine($"[{value} ms] {metrics.Key}");
            }
        }
#pragma warning disable CA2254
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _logger.LogDebug(stringBuilder.ToString());
#pragma warning restore CA2254
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            while (_stack.TryPop(out var trace))
            {
                trace.Dispose();
            }

            _stopwatch.Stop();
            foreach (var metrics in _synced)
            {
                _parent?._synced.AddOrUpdate(
                    metrics.Key,
                    _ => new ConcurrentBag<long>(metrics.Value),
                    (_, v) =>
                    {
                        foreach (var value in metrics.Value)
                        {
                            v.Add(value);
                        }

                        return v;
                    });
            }
            _parent?._synced.AddOrUpdate(
                _key,
                _ => new ConcurrentBag<long>([_stopwatch.ElapsedMilliseconds]),
                (_, v) =>
                {
                    v.Add(_stopwatch.ElapsedMilliseconds);
                    return v;
                });
            _logger.LogTrace("[trace disposed {Milliseconds} ms] {Key}", _stopwatch.ElapsedMilliseconds, _key);
        }
        _disposed = true;
    }
}