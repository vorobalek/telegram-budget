using System.Text;

namespace TelegramBudget.Extensions;

public static class TelegramPaginatorHelper
{
    public delegate void AppendCurrentString(StringBuilder pageBuilder, string currentString);

    public delegate void AppendNewPageHeader(StringBuilder pageBuilder, int pageNumber);

    public delegate string CreateCurrentString<in T>(T element);

    public static IEnumerable<string> CreatePaginated<T>(
        this IEnumerable<T> enumerable,
        AppendNewPageHeader appendNewPageHeader,
        CreateCurrentString<T> createCurrentString,
        AppendCurrentString appendCurrentString,
        int pageSizeLimit)
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

            yield return pageBuilder.ToString();

            pageBuilder.Clear();
            appendNewPageHeader(pageBuilder, ++pageNumber);
            appendCurrentString(pageBuilder, currentString);
        }

        yield return pageBuilder.ToString();
    }

    public delegate Task SendPageAsync(string pageContent, CancellationToken cancellationToken);

    public static async Task SendPaginatedAsync<T>(this IEnumerable<T> enumerable,
        int pageSizeLimit,
        AppendNewPageHeader appendNewPageHeader,
        CreateCurrentString<T> createCurrentString,
        AppendCurrentString appendCurrentString,
        SendPageAsync sendPageAsync,
        CancellationToken cancellationToken)
    {
        foreach (var pageContent in CreatePaginated(
                     enumerable,
                     appendNewPageHeader,
                     createCurrentString,
                     appendCurrentString,
                     pageSizeLimit))
        {
            await sendPageAsync(
                pageContent,
                cancellationToken);
        }
    }

    public static string? CreatePage<T>(this IEnumerable<T> enumerable,
        int pageSizeLimit,
        int pageNumber,
        AppendNewPageHeader appendNewPageHeader,
        CreateCurrentString<T> createCurrentString,
        AppendCurrentString appendCurrentString,
        out int actualPageNumber,
        out int actualPageCount)
    {
        var pagesContent = CreatePaginated(
                enumerable,
                appendNewPageHeader,
                createCurrentString,
                appendCurrentString,
                pageSizeLimit)
            .ToArray();
        
        actualPageCount = pagesContent.Length;
        actualPageNumber = Math.Min(pageNumber, actualPageCount);

        return actualPageCount == 0 ? null : pagesContent[actualPageNumber - 1];
    }
}