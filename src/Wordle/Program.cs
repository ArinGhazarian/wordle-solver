using Wordle;
using Wordle.Extensions;
using Spectre.Console;
using WordleStatus = Wordle.Status;

var trie = await WordleTrie.FromDictionary("./resources/words_alpha_five_letter_freq.txt");

var guess = "";
var history = new List<(string GuessedWord, WordleStatus[] WordleFeedback)>(5);

var selection = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("How do you wanna pick your first guess?")
        .AddChoices(
            "1. Random pick from the top 100 words.",
            "2. Let me enter my own."));
var option = int.Parse(selection[0].ToString());
if (option == 1)
{
    do
    {
        guess = trie.SuggestWords().Take(100).PickOne();
        AnsiConsole.MarkupLine($"First guess: [bold {RandomColor()}]{guess}[/]");
    }
    while (!AskConfirmation("Press [green]y[/] if you're feeling lucky or [red]n[/] to pick another guess."));
}
else if (option == 2)
{
    do
    {
        AnsiConsole.MarkupLine("Please enter your 1st guess (5 letter word, letters must all be from english alphabet) followed by enter:");
        guess = Console.ReadLine()?.ToLower();
    }
    while (!IsValidWord(guess));
}


foreach (var position in new[] { "1st", "2nd", "3rd", "4th", "5th" })
{
    NewLine();
    AnsiConsole.MarkupLine($"Great! Now enter the {position} guess ([bold yellow]{guess}[/]) into Wordle and type in the feedback.");

    if (!AskConfirmation("Once you're done, press [green]y[/] to continue or if you hit jackpot press [red]n[/] to skip to the summary."))
    {
        break;
    }

    Divider($"[blue]Feedback for {position} guess ([bold yellow]\"{guess}\"[/])[/]");
    var wordleFeedback = CollectWordleFeedback();

    history.Add((guess!, wordleFeedback));

    NewLine();
    AnsiConsole.MarkupLine($"Now based on the entered feedback for [bold yellow]{guess}[/] and all previous feedbacks, I am going to give you some suggestions.");
    var suggestedWords = trie.SuggestWords(history).ToArray();

    Divider("[green]Suggestions for the next guess[/]");

    var suggestionsCont = suggestedWords.Length;

    if (suggestionsCont <= 0)
    {
        AnsiConsole.MarkupLine("[red]Oops! It looks like something went wrong because no suggestions were found. This typically indicates that the feedback data entered may be incorrect.[/]");
        Environment.Exit(1);
    }

    AnsiConsole.MarkupLine($"There are {suggestionsCont} suggestion(s) in total. Now you have the following options:");
    do
    {
        selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices(
                    "1. Select one of the top 10 suggestions.",
                    "2. Select the top 1 suggestion.",
                    "3. Random pick one of the suggestions."));

        option = int.Parse(selection[0].ToString());
        if (option is 1)
        {
            guess = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select one of the following suggested words:")
                .AddChoices(suggestedWords.Take(10)));
        }
        else if (option is 2)
        {
            guess = suggestedWords.First();
        }
        else
        {
            guess = suggestedWords.PickOne();
        }

        NewLine();
        AnsiConsole.MarkupLine($"{new[] { "Perfect", "Awesome", "Great" }.PickOne()}! Your {position} guess is: [{RandomColor()}]{guess}[/]");
    }
    while (!AskConfirmation("If you're happy with the word, press [green]y[/] to continue or [red]n[/] to select another option."));
}

Divider("Summary");

foreach (var (word, feedback) in history)
{
    AnsiConsole.MarkupLine(string.Join("", word.Select((letter, i) => $"[invert {feedback[i].ToString().ToLower()}]{letter}[/]")));
}

static bool IsValidWord(string? word)
{
    if (string.IsNullOrEmpty(word))
    {
        AnsiConsole.MarkupLine("[red]Entered word cannot be empty! Please try again.[/]");
        return false;
    }

    if (word.Length != 5)
    {
        AnsiConsole.MarkupLine("[red]Entered word must exactly have 5 letters! Please try again.[/]");
        return false;
    }

    if (!word.ToLower().All(c => char.IsAsciiLetterLower(c)))
    {
        AnsiConsole.MarkupLine("[red]Entered word must only contain english letters! Please try again.[/]");
        return false;
    }

    return true;
}

static void NewLine() => AnsiConsole.WriteLine("");

static string RandomColor() => new[] { "green", "navy", "purple", "teal", "lime", "blue", "fuchsia", "olive" }.PickOne();

static void Divider(string text)
{
    NewLine();
    AnsiConsole.Write(new Rule($"{text}").RuleStyle("gray").LeftJustified());
    NewLine();
}

static bool AskConfirmation(string message)
{
    return AnsiConsole.Confirm(message);
}

static WordleStatus[] CollectWordleFeedback()
{
    WordleStatus[] wordleFeedback;
    do
    {
        wordleFeedback = new WordleStatus[5];

        foreach (var position in new[] { "1st", "2nd", "3rd", "4th", "5th" }.Select((d, i) => (Index: i, Display: d)))
        {
            var status = AnsiConsole.Prompt(
                new TextPrompt<string>($"Select {position.Display} letter's status ([invert gray]g[/]ray, [invert yellow]y[/]ellow, gree[invert green]n[/])")
                    .AddChoice("g")
                    .AddChoice("y")
                    .AddChoice("n"));

            wordleFeedback[position.Index] = status switch
            {
                "g" => WordleStatus.Gray,
                "y" => WordleStatus.Yellow,
                "n" => WordleStatus.Green,
                _ => throw new Exception("Invalid feedback character!")
            };
        }

        NewLine();
        AnsiConsole.Markup("Great! Here are your choices: ");
        AnsiConsole.MarkupLine(string.Join(", ", wordleFeedback.Select(x => $"[invert {x.ToString().ToLower()}]{x}[/]")));
        NewLine();
    }
    while (!AskConfirmation($"Press [green]y[/] to continue or [red]n[/] to re-enter the feedback."));

    return wordleFeedback;
}