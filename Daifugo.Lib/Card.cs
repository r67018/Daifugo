namespace Daifugo.Lib;

public enum Suit
{
    Spade,
    Heart,
    Diamond,
    Club,
    Joker,
}

public static class SuitExtensions
{
    public static string ToSymbol(this Suit suit)
    {
        return suit switch
        {
            Suit.Spade => "♠",
            Suit.Heart => "♥",
            Suit.Diamond => "♦",
            Suit.Club => "♣",
            Suit.Joker => "🃏",
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
    }
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
    
    public static string ToSymbol(this Rank rank)
    {
        return rank switch
        {
            Rank.Three => "3",
            Rank.Four => "4",
            Rank.Five => "5",
            Rank.Six => "6",
            Rank.Seven => "7",
            Rank.Eight => "8",
            Rank.Nine => "9",
            Rank.Ten => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            Rank.Two => "2",
            Rank.Joker => "Joker",
            _ => throw new ArgumentOutOfRangeException(nameof(rank)),
        };
    }
}

public readonly record struct Card(Suit Suit, Rank Rank) : IComparable<Card>
{
    public int CompareTo(Card other)
    {
        var rankComparison = Rank.CompareTo(other.Rank);
        if (rankComparison != 0) return rankComparison;
        return Suit.CompareTo(other.Suit);
    }
}
