namespace Daifugo.Lib;

public enum Suit
{
    Spade,
    Heart,
    Diamond,
    Club,
    Joker,
}

public enum Rank
{
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14,
    Two = 15,
    Joker = 16,
}

public static class RankExtensions
{
    public static Rank ToRank(this int value)
    {
        if (value == 1)
        {
            return Rank.Ace;
        }
        if (Enum.IsDefined(typeof(Rank), value))
        {
            return (Rank)value;
        }

        throw new ArgumentOutOfRangeException(nameof(value));
    }
}

public readonly record struct Card(Suit Suit, Rank Rank);
