using Wordle;
using Wordle.Extensions;

var green = Status.Green;
var gray = Status.Gray;
var yellow = Status.Yellow;

var trie = await WordleTrie.FromDictionary("./resources/words_alpha_five_letter_freq.txt");

var words = trie.SuggestWords().ToList();
words = trie.SuggestWords(
    [
        ("great", [gray, yellow, green, gray, yellow]),
        ("there", [yellow, gray, green, green, gray]),
        ("utero", [gray, green, green, green, gray]),
    ]).ToList();

Console.ReadLine();