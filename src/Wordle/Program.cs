using Wordle;
using Wordle.Extensions;

var green = Status.Green;
var gray = Status.Gray;
var yellow = Status.Yellow;

var trie = await WordleTrie.FromDictionary("./resources/words_alpha_five_letter_freq.txt");

var word = trie.SuggestWords("", [null, null, null, null, null]).Take(100).PickOne();
var words = trie.SuggestWords("music", [yellow, gray, gray, gray, yellow]).ToList();
words = trie.SuggestWords("cream", [green, gray, yellow, yellow, yellow]).ToList();

Console.ReadLine();