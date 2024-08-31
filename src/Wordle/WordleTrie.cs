namespace Wordle;

public class WordleTrie
{
    private readonly HashSet<char> _skippedLetters = [];

    public TrieNode Root { get; } = new() { Value = '/' };

    public int TotalWords { get; private set; }

    /// <summary>
    /// Suggests all possible words based on the guessed word.
    /// </summary>
    /// <param name="guessed">The guessed word</param>
    /// <param name="letterStatuses">The status of each letter in the guessed word</param>
    /// <returns>All possible words.</returns>
    public List<string> SuggestWords(string guessed, Status?[] letterStatuses)
    {
        foreach (var ch in guessed.Where((_, i) => letterStatuses[i] == Status.Gray))
        {
            _skippedLetters.Add(ch);
        }

        var paths = new List<string>();
        FindAllPaths(Root, guessed, letterStatuses, -1, "", paths);

        return paths;
    }

    private void FindAllPaths(TrieNode node, string guessed, Status?[] letterStatuses, int index, string path, List<string> paths)
    {
        path += node != Root ? node.Value : "";

        if (node.Children.Count == 0)
        {
            paths.Add(path);
            return;
        }

        var children = node.Children.Where(n => !_skippedLetters.Contains(n.Value));
        var nextLetterStatus = letterStatuses[index + 1];
        if (nextLetterStatus == Status.Green)
        {
            children = children.Where(n => n.Value == guessed[index + 1]);
        }
        else if (nextLetterStatus == Status.Yellow)
        {
            children = children.Where(n => n.Value != guessed[index + 1]);
        }

        foreach (var child in children)
        {
            FindAllPaths(child, guessed, letterStatuses, index + 1, path, paths);
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
            var segments =  line.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
