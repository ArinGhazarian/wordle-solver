namespace Wordle;

public class WordleTrie
{
    private readonly HashSet<char> _excludedLetters = [];
    private readonly HashSet<char> _includedLetters = [];
    private readonly Dictionary<int, List<(char Letter, Status Status)>> _history = [];

    public TrieNode Root { get; } = new() { Value = '/' };

    public int TotalWords { get; private set; }

    /// <summary>
    /// Suggests all possible words based on the guessed word.
    /// </summary>
    /// <param name="guessed">The guessed word</param>
    /// <param name="letterStatuses">The status of each letter in the guessed word</param>
    /// <returns>Top all possible words ordred descendingly by their freqencies.</returns>
    public List<string> SuggestWords(string guessed, Status?[] letterStatuses)
    {
        for (var i = 0; i < guessed.Length; i++)
        {
            var ch = guessed[i];

            if (!_history.ContainsKey(i))
            {
                _history[i] = [];
            }
            _history[i].Add((ch, (Status)letterStatuses[i]!));

            if (letterStatuses[i] == Status.Gray)
            {
                _excludedLetters.Add(ch);
            }
            else if (letterStatuses[i] is Status.Yellow or Status.Green)
            {
                _includedLetters.Add(ch);
            }
        }

        var paths = new List<(string Word, long? Frequency)>();
        FindAllPaths(Root, guessed, -1, paths);

        return paths.Where(x => _includedLetters.Count <= 0 || !_includedLetters.Except(x.Word).Any())
                    .OrderByDescending(x => x.Frequency)
                    .Select(x => x.Word)
                    .ToList();
    }

    private void FindAllPaths(TrieNode node, string guessed, int index, List<(string Word, long? Frequency)> paths)
    {
        if (node.Children.Count == 0)
        {
            paths.Add((node.Word!, node.Frequency));
            return;
        }

        var children = node.Children.Where(n => !_excludedLetters.Contains(n.Value));
        
        if (_history.TryGetValue(index + 1, out var nextLetterHistory))
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
            FindAllPaths(child, guessed, index + 1, paths);
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
