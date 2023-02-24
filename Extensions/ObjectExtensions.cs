using System.Runtime.CompilerServices;

namespace TelegramBudget.Extensions;

internal static class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T ThrowIfNull<T>(this T? value, string parameterName)
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }
}