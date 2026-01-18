using System.Collections.Immutable;

namespace Daifugo.Lib;

/// <summary>
/// ソルバーへの入力情報
/// ソルバーが知ることができない情報(相手の手札)を隠蔽するためのレコード
/// </summary>
/// <param name="PlayerIndex">自分の順番</param>
/// <param name="Hand">自分の手札</param>
/// <param name="OpponentHandCount">相手の手札の枚数</param>
/// <param name="LastPlayedCards">現在の場に出ているカード（最後に出されたカード）</param>
/// <param name="PlayHistory">今までに出されたカード</param>
public record SolverInput(
    PlayerIndex PlayerIndex,
    ImmutableList<Card> Hand,
    ImmutableArray<int> OpponentHandCount,
    ImmutableArray<Card>? LastPlayedCards,
    ImmutableList<ImmutableArray<Card>> PlayHistory,
    int PassStreak = 0
)
{
    public SolverInput(GameState gameState) : this(
        gameState.PlayerIndex,
        gameState.Hands[gameState.PlayerIndex.Value],
        OpponentHandCount: [..gameState.Hands.RemoveAt(gameState.PlayerIndex.Value).Select(hand => hand.Count)],
        gameState.LastPlayedCards,
        gameState.PlayHistory,
        gameState.PassStreak
    )
    {
    }
}
