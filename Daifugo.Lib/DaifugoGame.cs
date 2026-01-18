namespace Daifugo.Lib;

/// <summary>
/// 大富豪のゲームロジックを提供するクラス
/// </summary>
public static class DaifugoGame
{
    public static GameState PlayOneTurn(GameState gameState, PlayerAction action)
    {
        var nextGameState = ApplyPlayAction(gameState, action);

        // 手札が残っているプレイヤーの数を数える
        var activePlayerCount = nextGameState.Hands.Count(hand => hand.Count > 0);

        // 全員がパスした場合、場をリセット
        if (nextGameState.PassStreak >= activePlayerCount - 1)
        {
            nextGameState = nextGameState with
            {
                LastPlayedCard = null,
                PassStreak = 0 // 新しい場になるのでパス回数もリセット
            };
        }

        return nextGameState;
    }

    /// <summary>
    /// 指定したカードがプレイできるかチェックする関数
    /// </summary>
    /// <param name="card">プレイしたいカード</param>
    /// <param name="lastPlayedCard">山札の一番上のカード</param>
    /// <returns></returns>
    public static bool IsValidPlay(Card card, Card? lastPlayedCard)
    {
        // 一枚もプレイされていないなら必ず出せる
        if (lastPlayedCard == null) return true;
        // ジョーカーは常に出せる
        if (card.Rank == Rank.Joker) return true;
        // 山場がジョーカーの場合、スペードの3は出せる
        if (lastPlayedCard.Value.Rank == Rank.Joker && card is { Suit: Suit.Spade, Rank: Rank.Three }) return true;
        // 既にプレイされているなら、一番上のカードよりも強いカードを出す必要がある
        return card.Rank > lastPlayedCard.Value.Rank;
    }

    /// <summary>
    /// ゲームが終了かチェックする関数
    /// </summary>
    /// <param name="handCounts">全プレイヤーの手札の枚数</param>
    /// <returns></returns>
    public static bool IsGameOver(IReadOnlyCollection<int> handCounts)
    {
        // 全員の手札が空なら終了
        return handCounts.All(count => count == 0);
    }

    /// <summary>
    /// 手札が0のプレイヤーを飛ばして、次に行動するプレイヤーのインデックスを求める
    /// </summary>
    /// <param name="index">現在のプレイヤーのインデックス</param>
    /// <param name="handCounts">各プレイヤーの手札の枚数</param>
    /// <returns>次に行動するプレイヤーのインデックス</returns>
    public static PlayerIndex GetNextPlayablePlayerIndex(PlayerIndex index, IReadOnlyList<int> handCounts)
    {
        var playerCount = handCounts.Count;

        // 各プレイヤーを順番にチェック
        // プレイヤーの数だけループすれば、全員チェックできるので十分
        var nextIndex = index;
        foreach (var _ in handCounts)
        {
            nextIndex = new PlayerIndex((nextIndex.Value + 1) % playerCount);
            if (handCounts[nextIndex.Value] > 0)
            {
                return nextIndex;
            }
        }
        // 全員の手札が0の場合は例外を投げる
        throw new InvalidOperationException("All players have empty hands.");
    }



    private static GameState ApplyPlayAction(GameState gameState, PlayerAction action)
    {
        // 次のプレイヤーを決定
        var nextPlayerIndex = GetNextPlayablePlayerIndex(
            gameState.PlayerIndex, handCounts: gameState.Hands.Select(hand => hand.Count).ToArray());

        switch (action)
        {
            // カードを出す場合
            case PlayerAction.Play play:
                var nextGameState = new GameState
                {
                    PlayerIndex = nextPlayerIndex,
                    LastPlayedPlayerIndex = gameState.PlayerIndex,
                    Hands = gameState.Hands.SetItem(gameState.PlayerIndex.Value,
                        gameState.Hands[gameState.PlayerIndex.Value].Remove(play.Card)),
                    LastPlayedCard = play.Card,
                    PlayHistory = gameState.PlayHistory.Add(play.Card),
                    PassStreak = 0
                };

                // ジョーカーをスペードの3で返した場合は、場をリセットし、同じプレイヤーが続けて出せるようにする
                if (play.Card is { Suit: Suit.Spade, Rank: Rank.Three } &&
                    gameState.LastPlayedCard is { Rank: Rank.Joker })
                {
                    nextGameState = nextGameState with
                    {
                        LastPlayedCard = null,
                        PlayerIndex = gameState.PlayerIndex,
                        PassStreak = 0
                    };
                }

                return nextGameState;

            // パスの場合は次のプレイヤーに交代し、パス回数を増やす
            case PlayerAction.Pass:
                return gameState with
                {
                    PlayerIndex = nextPlayerIndex,
                    PassStreak = gameState.PassStreak + 1
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(action), "Unknown PlayerAction type.");
        }
    }
}