namespace Wordle;

public enum Status
{
    Gray, // wrong letter
    Yellow, // correct letter, wrong spot
    Green // correct letter, correct spot
}