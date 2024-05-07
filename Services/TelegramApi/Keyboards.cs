using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBudget.Services.TelegramApi;

public static class Keyboards
{
    public static readonly InlineKeyboardMarkup CmdAllInline =
        new([[InlineKeyboardButton.WithCallbackData(TR.L + "BTN_START_COMMANDS", "cmd.all")]]);

    public static readonly InlineKeyboardMarkup BackToMainInline =
        new([[InlineKeyboardButton.WithCallbackData(TR.L + "BTN_BACK", "main")]]);

    public static readonly InlineKeyboardMarkup MenuInline =
        new([[InlineKeyboardButton.WithCallbackData(TR.L + "BTN_HISTORY", "hst")]]);

    public delegate string PaginationButtonCallbackDataProvider(int currentPageNumber, int targetPageNumber);

    public static InlineKeyboardMarkup GetPaginationInline(
        int currentPageNumber,
        int pageCount,
        PaginationButtonCallbackDataProvider backwardCallbackDataProvider,
        PaginationButtonCallbackDataProvider forwardCallbackDataProvider,
        int? stepLineLength = null)
    {
        var lines = new List<IEnumerable<InlineKeyboardButton>>();

        if (stepLineLength is not null)
        {
            var stepButtons = GetPaginationStepButtons(
                    currentPageNumber,
                    pageCount,
                    stepLineLength.Value,
                    backwardCallbackDataProvider,
                    forwardCallbackDataProvider)
                .ToArray();
            if (stepButtons.Length > 0)
                lines.Add(stepButtons);
        }
        else
        {
            var moveButtons = GetPaginationMoveButtons(
                    currentPageNumber,
                    pageCount,
                    backwardCallbackDataProvider,
                    forwardCallbackDataProvider)
                .ToArray();
            if (moveButtons.Length > 0)
                lines.Add(moveButtons);
        }

        lines.Add([InlineKeyboardButton.WithCallbackData(TR.L + "BTN_BACK", "main")]);

        return new InlineKeyboardMarkup(lines);
    }

    private static IEnumerable<InlineKeyboardButton> GetPaginationStepButtons(
        int currentPageNumber,
        int pageCount,
        int stepLineLength,
        PaginationButtonCallbackDataProvider backwardCallbackDataProvider,
        PaginationButtonCallbackDataProvider forwardCallbackDataProvider)
    {
        var buttons = new List<InlineKeyboardButton>();

        if (currentPageNumber > 1)
        {
            var lowerLimit = Math.Max(currentPageNumber - stepLineLength / 2, 1);
            for (var i = lowerLimit; i < currentPageNumber; ++i)
            {
                if (i == lowerLimit)
                {
                    buttons.Add(
                        InlineKeyboardButton.WithCallbackData(
                            string.Format(TR.L + "BTN_JUMP_FIRST", 1),
                            backwardCallbackDataProvider(
                                currentPageNumber,
                                1)));
                    continue;
                }
                
                buttons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(TR.L + "BTN_MOVE_BACK", i),
                        backwardCallbackDataProvider(
                            currentPageNumber,
                            i)));
            }
        }

        if (currentPageNumber < pageCount)
        {
            var upperLimit = Math.Min(pageCount, currentPageNumber + stepLineLength / 2);
            for (var i = currentPageNumber + 1; i <= upperLimit; ++i)
            {
                if (i == upperLimit)
                {
                    buttons.Add(
                        InlineKeyboardButton.WithCallbackData(
                            string.Format(TR.L + "BTN_JUMP_LAST", pageCount),
                            forwardCallbackDataProvider(
                                currentPageNumber,
                                pageCount)));
                    continue;
                }
                
                buttons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(TR.L + "BTN_MOVE_NEXT", i),
                        forwardCallbackDataProvider(
                            currentPageNumber,
                            i)));
            }
        }

        return buttons;
    }

    private static IEnumerable<InlineKeyboardButton> GetPaginationMoveButtons(
        int currentPageNumber,
        int pageCount,
        PaginationButtonCallbackDataProvider backwardCallbackDataProvider,
        PaginationButtonCallbackDataProvider forwardCallbackDataProvider)
    {
        var buttons = new List<InlineKeyboardButton>();

        if (currentPageNumber > 1)
            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    string.Format(TR.L + "BTN_MOVE_BACK", currentPageNumber - 1),
                    backwardCallbackDataProvider(
                        currentPageNumber, 
                        currentPageNumber - 1)));
        if (currentPageNumber < pageCount)
            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    string.Format(TR.L + "BTN_MOVE_NEXT", currentPageNumber + 1),
                    forwardCallbackDataProvider(
                        currentPageNumber, 
                        currentPageNumber + 1)));

        return buttons;
    }
}