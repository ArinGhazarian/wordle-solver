namespace Wordle;

public class WordleTrie
{
    public TrieNode Root { get; } = new() { Value = '/' };

    public int TotalWords { get; private set; }

    /// <summary>
    /// Suggests all possible words based on the provided word history.
    /// </summary>
    /// <param name="wordHistory">The history of all guessed words and their respective Wordle feedbacks.</param>
    /// <returns>All possible words ordred descendingly by their freqencies.</returns>
    public IEnumerable<string> SuggestWords(IEnumerable<(string GuessedWord, Status[] Feedback)>? wordHistory = null)
    {
        var excludedLetters = new HashSet<char>();
        var includedLetters = new HashSet<char>();
        var letterHistory = new Dictionary<int, List<(char Letter, Status Status)>>();

        foreach (var (guessed, feedback) in wordHistory ?? [])
        {
            for (var i = 0; i < guessed.Length; i++)
            {
                var ch = guessed[i];

                if (!letterHistory.TryAdd(i, [(ch, feedback[i])]))
                {
                    letterHistory[i].Add((ch, feedback[i]));
                }

                if (feedback[i] == Status.Gray)
                {
                    excludedLetters.Add(ch);
                }
                else // Status.Yellow or Status.Green
                {
                    includedLetters.Add(ch);
                }
            }
        }

        var paths = new List<(string Word, long? Frequency)>();
        FindAllPaths(Root, -1, excludedLetters, letterHistory, paths);

        return paths.Where(x => includedLetters.Count <= 0 || !includedLetters.Except(x.Word).Any())
                    .OrderByDescending(x => x.Frequency)
                    .Select(x => x.Word);
    }

    /// <summary>
    /// Finds all possibe paths starting from the root of the Trie.
    /// </summary>
    /// <param name="node">Current node being processed.</param>
    /// <param name="index">The level of the current node that corresponds to the letter index with the Root node being -1.</param>
    /// <param name="excludedLetters">All excluded letters.</param>
    /// <param name="letterHistory">Letter history by index.</param>
    /// <param name="paths">All possible path outputs.</param>
    private void FindAllPaths(
        TrieNode node,
        int index,
        HashSet<char> excludedLetters,
        Dictionary<int, List<(char Letter, Status Status)>> letterHistory,
        List<(string Word, long? Frequency)> paths)
    {
        if (node.Children.Count == 0)
        {
            paths.Add((node.Word!, node.Frequency));
            return;
        }

        var children = node.Children.Where(n => !excludedLetters.Contains(n.Value));

        if (letterHistory.TryGetValue(index + 1, out var nextLetterHistory))
        {
            if (nextLetterHistory.Any(x => x.Status == Status.Green))
            {
                var letter = nextLetterHistory.First(x => x.Status == Status.Green).Letter;
                children = children.Where(n => n.Value == letter);
            }
            else if (nextLetterHistory.Any(x => x.Status == Status.Yellow))
            {
                var letters = nextLetterHistory.Where(x => x.Status == Status.Yellow).Select(x => x.Letter).ToList();
                children = children.Where(n => !letters.Contains(n.Value));
            }
        }

        foreach (var child in children)
        {
            FindAllPaths(child, index + 1, excludedLetters, letterHistory, paths);
        }
    }

    public static async Task<WordleTrie> FromDictionary(string dictionaryFilePath)
    {
        if (!File.Exists(dictionaryFilePath))
        {
            throw new FileNotFoundException("Dictionay file not found!");
        }

        var trie = new WordleTrie();

        using var file = File.OpenText(dictionaryFilePath);

        var line = await file.ReadLineAsync();
        while (!string.IsNullOrEmpty(line))
        {
            var segments = line.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var word = segments[0];
            var frequency = segments.Length > 1 ? long.Parse(segments[1]) : (long?)null;

            var currentNode = trie.Root;
            trie.TotalWords++;

            foreach (var ch in word.ToLower())
            {
                var existingChild = currentNode.Children.SingleOrDefault(n => n.Value == ch);
                if (existingChild is null)
                {
                    var node = new TrieNode { Value = ch, Parent = currentNode };
                    currentNode.Children.Add(node);
                    currentNode = node;
                }
                else
                {
                    currentNode = existingChild;
                }
            }

            currentNode.IsWord = true;
            currentNode.Word = word;
            currentNode.Frequency = frequency;
            line = await file.ReadLineAsync();
        }

        return trie;
    }
}
