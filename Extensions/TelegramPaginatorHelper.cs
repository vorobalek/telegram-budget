using System.Text;

namespace TelegramBudget.Extensions;

public static class TelegramPaginatorHelper
{
    public delegate void AppendCurrentString(StringBuilder pageBuilder, string currentString);

    public delegate void AppendNewPageHeader(StringBuilder pageBuilder, int pageNumber);

    public delegate string CreateCurrentString<in T>(T element);

    public delegate Task SendPageAsync(string pageContent, CancellationToken cancellationToken);


    public static async Task SendPaginatedAsync<T>(
        this IEnumerable<T> enumerable,
        AppendNewPageHeader appendNewPageHeader,
        CreateCurrentString<T> createCurrentString,
        AppendCurrentString appendCurrentString,
        SendPageAsync sendPageAsync,
        int pageSizeLimit,
        CancellationToken cancellationToken)
    {
        var pageBuilder = new StringBuilder();
        var temporaryPageBuilder = new StringBuilder();
        var pageNumber = 0;
        foreach (var element in enumerable)
        {
            var currentString = createCurrentString(element);

            if (pageBuilder.Length == 0)
            {
                appendNewPageHeader(pageBuilder, ++pageNumber);
                appendCurrentString(pageBuilder, currentString);
                continue;
            }

            temporaryPageBuilder.Clear();
            temporaryPageBuilder.Append(pageBuilder);
            appendCurrentString(temporaryPageBuilder, currentString);
            if (temporaryPageBuilder.Length <= pageSizeLimit)
            {
                appendCurrentString(
                    pageBuilder,
                    currentString);
                continue;
            }

            await sendPageAsync(
                pageBuilder.ToString(),
                cancellationToken);

            pageBuilder.Clear();
            appendNewPageHeader(pageBuilder, ++pageNumber);
            appendCurrentString(pageBuilder, currentString);
        }

        await sendPageAsync(
            pageBuilder.ToString(),
            cancellationToken);
    }
}