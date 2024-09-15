using FluentAssertions;

namespace Wordle.Tests;

public class WordleTireTests
{
    private const int TOTAL_WORDS = 11895;
    private const Status Y = Status.Yellow;
    private const Status N = Status.Green;
    private const Status G = Status.Gray;

    [Fact]
    public async Task FromDictionary_Should_Return_A_WordleTrie()
    {
        // Arrange, Act
        var wordleTrie = await BuildWordleTrie();

        // Assert
        wordleTrie.Should().BeOfType<WordleTrie>();
    }

    [Fact]
    public async Task FromDictionary_Should_Parse_All_Words()
    {
        // Arrange, Act
        var wordleTrie = await BuildWordleTrie();

        // Assert
        wordleTrie.TotalWords.Should().Be(TOTAL_WORDS);
    }

    [Fact]
    public async Task SuggestWords_Should_Return_All_Words_By_Default()
    {
        // Arrange
        var wordleTrie = await BuildWordleTrie();

        // Act
        var suggestions = wordleTrie.SuggestWords();

        // Assert
        suggestions.Should().HaveCount(TOTAL_WORDS);
    }

    [Fact]
    public async Task SuggestWords_Should_Suggest_Four_Words_For_about_YYNGG()
    {
        // Arrange
        var wordleTrie = await BuildWordleTrie();

        // Act
        var words = wordleTrie.SuggestWords([("about", [Y, Y, N, G, G])]);

        // Assert
        words.Should().HaveCount(4);
        words.Should().ContainInOrder(["broad", "cooba", "broma", "caoba"]);
    }

    [Fact]
    public async Task SuggestWords_Should_Suggest_Six_Words_For_great_GYNGY_and_there_YGNNG()
    {
        // Arrage
        var wordleTrie = await BuildWordleTrie();

        // Act
        var words = wordleTrie.SuggestWords([("great", [G, Y, N, G, Y]), ("there", [Y, G, N, N, G])]);

        // Assert
        words.Should().HaveCount(6);
        words.Should().ContainInOrder(["stern", "utero", "steri", "uteri", "sterk", "stero"]);

    }

    private async Task<WordleTrie> BuildWordleTrie() => await WordleTrie.FromDictionary("./Resources/words_alpha_five_letter_freq.txt");
}
