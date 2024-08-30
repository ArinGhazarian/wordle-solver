using System;
namespace Wordle;

public class TrieNode
{
    public required char Value { get; set; }

    public TrieNode? Parent { get; set; }

    public List<TrieNode> Children { get; } = new List<TrieNode>(26);

    public bool IsWord { get; set; }

    public string? Word { get; set; }

    public override string ToString() => IsWord ? $"{Value} ({Word})" : Value.ToString(); 
}
