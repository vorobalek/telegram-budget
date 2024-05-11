using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Services.TelegramApi.NewHandlers;

namespace TelegramBudget.Services.TelegramApi;

public static class Keyboards
{
    public delegate string PaginationButtonCallbackDataProvider(int currentPageNumber, int targetPageNumber);

    public delegate string SelectItemInlineButtonProvider<in T>(T key);

    public static readonly InlineKeyboardMarkup CmdAllInline =
        new([[InlineKeyboardButton.WithCallbackData(TR.L + "HELP_COMMANDS", "cmd.all")]]);

    public static readonly InlineKeyboardButton BackToMainInlineButtonOld =
        InlineKeyboardButton.WithCallbackData(TR.L + "BACK", "main.old");

    public static readonly InlineKeyboardMarkup BackToMainInlineOld =
        new([[BackToMainInlineButtonOld]]);

    public static readonly InlineKeyboardButton BackToMainInlineButton =
        InlineKeyboardButton.WithCallbackData(TR.L + "_BTN_MAIN", NewMainHandler.Command);

    private static readonly InlineKeyboardButton HistoryInlineButton =
        InlineKeyboardButton.WithCallbackData(TR.L + "_BTN_HISTORY", NewHistoryHandler.Command);

    public static readonly InlineKeyboardButton SwitchBudgetInlineButton =
        InlineKeyboardButton.WithCallbackData(TR.L + "_BTN_SWITCH", NewSwitchHandler.Command);

    public static readonly InlineKeyboardButton CreateBudgetInlineButton =
        InlineKeyboardButton.WithCallbackData(TR.L + "_BTN_CREATE", NewCreateHandler.Command);

    private static readonly InlineKeyboardMarkup ActiveBudgetChosenMainInline =
        new([[HistoryInlineButton, SwitchBudgetInlineButton]]);

    private static readonly InlineKeyboardMarkup ActiveBudgetNotChosenMainInline =
        new([[SwitchBudgetInlineButton]]);

    public static InlineKeyboardMarkup BuildMainInline(bool hasActiveBudget)
    {
        return hasActiveBudget
            ? ActiveBudgetChosenMainInline
            : ActiveBudgetNotChosenMainInline;
    }

    public static IEnumerable<IEnumerable<InlineKeyboardButton>> BuildSelectItemInlineButtons<T>(
        ICollection<T> availableBudgets,
        SelectItemInlineButtonProvider<T> textInlineButtonProvider,
        SelectItemInlineButtonProvider<T> dataInlineButtonProvider)
    {
        return availableBudgets
            .Select(item =>
                (IEnumerable<InlineKeyboardButton>)
                [
                    InlineKeyboardButton
                        .WithCallbackData(
                            textInlineButtonProvider(item),
                            dataInlineButtonProvider(item))
                ])
            .ToList();
    }

    public static IEnumerable<IEnumerable<InlineKeyboardButton>> BuildPaginationInlineButtons(
        int currentPageNumber,
        int pageCount,
        PaginationButtonCallbackDataProvider backwardCallbackDataProvider,
        PaginationButtonCallbackDataProvider forwardCallbackDataProvider,
        int? stepLineLength = null)
    {
        var lines = new List<IEnumerable<InlineKeyboardButton>>();

        if (stepLineLength is not null)
        {
            var stepButtons = BuildPaginationStepInlineButtons(
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
            var moveButtons = BuildPaginationMoveInlineButtons(
                    currentPageNumber,
                    pageCount,
                    backwardCallbackDataProvider,
                    forwardCallbackDataProvider)
                .ToArray();
            if (moveButtons.Length > 0)
                lines.Add(moveButtons);
        }

        return lines;
    }

    private static IEnumerable<InlineKeyboardButton> BuildPaginationStepInlineButtons(
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
                        string.Format(TR.L + "_BTN_MOVE_BACK", i),
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
                        string.Format(TR.L + "_BTN_MOVE_NEXT", i),
                        forwardCallbackDataProvider(
                            currentPageNumber,
                            i)));
            }
        }

        return buttons;
    }

    private static IEnumerable<InlineKeyboardButton> BuildPaginationMoveInlineButtons(
        int currentPageNumber,
        int pageCount,
        PaginationButtonCallbackDataProvider backwardCallbackDataProvider,
        PaginationButtonCallbackDataProvider forwardCallbackDataProvider)
    {
        var buttons = new List<InlineKeyboardButton>();

        if (currentPageNumber > 1)
            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    string.Format(TR.L + "_BTN_MOVE_BACK", currentPageNumber - 1),
                    backwardCallbackDataProvider(
                        currentPageNumber,
                        currentPageNumber - 1)));
        if (currentPageNumber < pageCount)
            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    string.Format(TR.L + "_BTN_MOVE_NEXT", currentPageNumber + 1),
                    forwardCallbackDataProvider(
                        currentPageNumber,
                        currentPageNumber + 1)));

        return buttons;
    }
}