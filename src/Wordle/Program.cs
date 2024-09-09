using Wordle;
using Wordle.Extensions;
using Spectre.Console;
using WordleStatus = Wordle.Status;

var trie = await WordleTrie.FromDictionary("./resources/words_alpha_five_letter_freq.txt");

var guess = "";
var history = new List<(string GuessedWord, WordleStatus[] WordleFeedback)>(5);
var suggestedWords = trie.SuggestWords();
var suggestedWordsCount = trie.TotalWords;

var counter = 0;
foreach (var position in new[] { "1st", "2nd", "3rd", "4th", "5th", "6th" })
{
    Divider($"[green]{position} guess[/]");

    if (suggestedWordsCount > 0)
    {
        AnsiConsole.MarkupLine($"There are a total of {suggestedWordsCount} suggestion(s) for your next guess. Now you have the following options:");
        do
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(
                        "1. Select one of the top 10 suggestions.",
                        "2. Select the top 1 suggestion.",
                        "3. Random pick from top 100 suggestions.",
                        "4. Let me enter my own."));

            var option = int.Parse(selection[0].ToString());
            guess = option switch
            {
                1 => AnsiConsole.Prompt(new SelectionPrompt<string>()
                                .Title("Select one of the following suggested words:")
                                .AddChoices(suggestedWords.Take(10))),
                2 => suggestedWords.First(),
                3 => suggestedWords.PickOne(),
                _ => GetUserInput()
            };

            NewLine();
            AnsiConsole.MarkupLine($"{new[] { "Perfect", "Awesome", "Great" }.PickOne()}! Your {position} guess is: [yellow]{guess}[/]");
        }
        while (!AskConfirmation("If you're happy with the word, enter it into Wordle and press [green]y[/] to continue or [red]n[/] to select another option."));
    }
    else
    {
        AnsiConsole.MarkupLine("It looks like something went wrong because no suggestions were found. This typically indicates that the feedback data entered may be incorrect.");
        AnsiConsole.MarkupLine("But don't worry you can still enter your own guess.");
        guess = GetUserInput();
    }

    NewLine();

    if (counter >= 5)
    {
        AnsiConsole.MarkupLine($"This is your last chance! enter the 6th and last guess ([bold yellow]{guess}[/] into Wordle :crossed_fingers:)");
        AnsiConsole.Ask<string>("Press any key to show the summary.");
        break;
    }

    if (!AskConfirmation("Great! Now go ahead and press [green]y[/] to enter the Wordle feedback or if you hit the jackpot :party_popper: press [red]n[/] to skip to the summary."))
    {
        break;
    }

    Divider($"[blue]Feedback for {position} guess ([bold yellow]\"{guess}\"[/])[/]");
    var wordleFeedback = CollectWordleFeedback();

    history.Add((guess!, wordleFeedback));

    suggestedWords = trie.SuggestWords(history).ToArray();
    suggestedWordsCount = suggestedWords.Count();

    counter++;
}

Divider("Summary");

foreach (var (word, feedback) in history)
{
    AnsiConsole.MarkupLine(string.Join("", word.Select((letter, i) => $"[invert {feedback[i].ToString().ToLower()}]{letter}[/]")));
}

AnsiConsole.MarkupLine(string.Join("", guess!.Select(letter => counter < 5 ? $"[invert green]{letter}[/]" : letter.ToString())));

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

static string GetUserInput()
{
    string? guess;
    do
    {
        AnsiConsole.MarkupLine("Please enter your 1st guess (5 letter word, letters must all be from english alphabet) followed by enter:");
        guess = Console.ReadLine()?.ToLower();
    }
    while (!IsValidWord(guess));

    return guess!;
}