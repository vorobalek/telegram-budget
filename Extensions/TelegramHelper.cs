namespace TelegramBudget.Extensions;

public static class TelegramHelper
{
    private static string GetFullName(string firstName, string? lastName)
    {
        return $"{firstName.EscapeHtml()}" +
               $"{(lastName is not null
                   ? " " + lastName.EscapeHtml()
                   : string.Empty)}";
    }

    public static string GetFullNameLink(long id, string firstName, string? lastName)
    {
        return "<a href=\"tg://user?id=" +
               $"{id}\">" +
               GetFullName(firstName, lastName) +
               "</a>";
    }
}