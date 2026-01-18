using System.Collections.Immutable;

namespace Daifugo.Lib;

public static class DaifugoHelper
{
    public static GameState GenerateInitialGameState(int playerCount)
    {
        var hands = GenerateInitialHands(playerCount);
        var startingPlayerIndex = StartingPlayerIndex(hands);

        return new GameState
        {
            PlayerIndex = new PlayerIndex(startingPlayerIndex),
            LastPlayedPlayerIndex = new PlayerIndex((startingPlayerIndex - 1 + playerCount) % playerCount), // とりあえず
            Hands = hands,
            Table = ImmutableList<ImmutableArray<Card>>.Empty,
            PlayHistory = [],
            PassStreak = 0
        };
    }

    private static ImmutableArray<ImmutableList<Card>> GenerateInitialHands(int playerCount)
    {
        var cards =
            from suit in Enum.GetValues<Suit>().Where(s => s != Suit.Joker)
            from rank in Enum.GetValues<Rank>().Where(r => r != Rank.Joker)
            select new Card(suit, rank);
        cards = cards.Append(new Card(Suit.Joker, Rank.Joker));
        var shuffled = cards.Shuffle().ToArray();

        // 手札を配る
        var hands = Enumerable.Range(0, playerCount).Select(_ => new List<Card>()).ToArray();
        for (var i = 0; i < shuffled.Length; i++)
        {
            hands[i % playerCount].Add(shuffled[i]);
        }

        return [..hands.Select(hand => hand.OrderBy(card => card).ToImmutableList())];
    }

    private static int StartingPlayerIndex(ImmutableArray<ImmutableList<Card>> hands)
    {
        return Array.FindIndex(hands.ToArray(), hand => hand.Contains(new Card(Suit.Diamond, Rank.Three)));
    }

}