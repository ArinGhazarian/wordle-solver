using Wordle;

var green = Status.Green;
var gray = Status.Gray;
var yellow = Status.Yellow;

var trie = await WordleTrie.FromDictionary("./resources/words_alpha_five_letter_freq.txt");

var words = trie.SuggestWords("blast", [gray, green, gray, gray, gray]);
words = trie.SuggestWords("clime", [gray, green, gray, gray, gray]);
words = trie.SuggestWords("flour", [green, green, gray, yellow, gray]);
words = trie.SuggestWords("flung", [green, green, green, green, gray]);

Console.ReadLine();