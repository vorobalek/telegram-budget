using System.Text;
using Tracee;

namespace TelegramBudget.Extensions;

public static class TraceeExtensions
{
    public static void CollectAll(this ITracee tracee, LogLevel logLevel)
    {
#pragma warning disable CA2254
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        tracee.Logger.Log(logLevel, BuildPrettyLog(tracee.Collect()));
#pragma warning restore CA2254
    }

    private static string BuildPrettyLog(IReadOnlyDictionary<ITraceeMetricLabels, ITraceeMetricValue> metrics)
    {
        var minDepth = metrics.Min(metric => metric.Key.Depth);
        var prepared = metrics
            .GroupBy(metric => metric.Key.Key)
            .Select(group => new
            {
                group.Key,
                Created = group.Max(metric => metric.Key.Created),
                Depth = group.Min(metric => metric.Key.Depth),
                Value = group.Sum(metric => metric.Value.Milliseconds)
            })
            .OrderBy(metric => metric.Created)
            .ThenBy(metric => metric.Key)
            .Select(metric =>
            (
                Key: $"{(
                    metric.Depth - minDepth > 0
                        ? new string('.', metric.Depth - minDepth)
                        : string.Empty
                )}{metric.Key}",
                Value: $"{metric.Value} ms"
            )).ToArray();

        var (metricsTitle, durationTitle) = ("Metric", "Duration (ms)");
        
        var (paddingKey, paddingValue) = prepared
            .Aggregate(
                (metricsTitle.Length, durationTitle.Length),
                (padding, next) =>
                {
                    var (paddingKey, paddingValue) = padding;
                    return (
                        Math.Max(paddingKey, next.Key.Length),
                        Math.Max(paddingValue, next.Value.Length));
                });
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"| {
            metricsTitle.PadRight(paddingKey)
        } | {
            durationTitle.PadLeft(paddingValue)
        } |");
        stringBuilder.AppendLine($"|{new string('–', paddingKey + paddingValue + 5)}|");
        foreach (var (key, value) in prepared)
            stringBuilder.AppendLine($"| {key.PadRight(paddingKey)} | {value.PadLeft(paddingValue)} |");
        return stringBuilder.ToString();
    }
}