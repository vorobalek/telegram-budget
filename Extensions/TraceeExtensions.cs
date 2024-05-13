using System.Text;
using Tracee;

namespace TelegramBudget.Extensions;

public static class TraceeExtensions
{
    public static void CollectAll(this ITracee tracee, LogLevel logLevel)
    {
        tracee.Log(logLevel, BuildPrettyLog(tracee.Collect()));
    }

    private static string BuildPrettyLog(IReadOnlyDictionary<ITraceeMetricLabels, ITraceeMetricValue> metrics)
    {
        var minDepth = metrics.Min(metric => metric.Key.Depth);
        var prepared = metrics
            .OrderBy(metric => metric.Key.Created)
            .ThenBy(metric => metric.Key.Key)
            .Select(metric =>
            (
                StackId: $"{metric.Key.StackId}",
                Key: $"{(
                    metric.Key.Depth - minDepth > 0
                        ? new string('.', metric.Key.Depth - minDepth)
                        : string.Empty
                )}{metric.Key.Key}",
                Value: $"{metric.Value.Milliseconds} ms"
            )).ToArray();

        var (stackIdTitle, metricsTitle, durationTitle) = ("Stack #", "Metric", "Duration (ms)");
        
        var (paddingStackId, paddingKey, paddingValue) = prepared
            .Aggregate(
                (stackIdTitle.Length, metricsTitle.Length, durationTitle.Length),
                (padding, next) =>
                {
                    var (paddingStackId, paddingKey, paddingValue) = padding;
                    return (
                        Math.Max(paddingStackId, next.StackId.Length),
                        Math.Max(paddingKey, next.Key.Length),
                        Math.Max(paddingValue, next.Value.Length));
                });
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"| {
            stackIdTitle.PadRight(paddingStackId)
        } | {
            metricsTitle.PadRight(paddingKey)
        } | {
            durationTitle.PadLeft(paddingValue)
        } |");
        stringBuilder.AppendLine($"|{new string('–', paddingStackId + paddingKey + paddingValue + 8)}|");
        foreach (var (stackId, key, value) in prepared)
            stringBuilder.AppendLine($"| {stackId.PadRight(paddingStackId)} | {key.PadRight(paddingKey)} | {value.PadLeft(paddingValue)} |");
        return stringBuilder.ToString();
    }
}