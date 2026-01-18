using System.Collections.Immutable;

namespace Daifugo.Lib;

public class MonteCarloSolver : ISolver
{
    public PlayerAction FindMostValidPlay(SolverInput input, int simulationCount)
    {
        // 合法手を列挙
        var playableCards = input.Hand
            .Where(card => DaifugoGame.IsValidPlay(card, input.LastPlayedCard))
            .ToList();

        // 解が一意に定まる場合はその解を返す
        switch (playableCards.Count)
        {
            // 合法手がない場合はパス
            case 0:
                return new PlayerAction.Pass();
            // 合法手が1つだけの場合はそれを返す
            case 1:
                return new PlayerAction.Play(playableCards[0]);
        }

        // 相手の手札を生成
        var opponentHands = _generateOpponentHands(input.OpponentHandCount, input.PlayHistory);
        // 勝利数を格納するハッシュ
        var winCount = new Dictionary<Card, int>();

        // 各合法手でシミュレーションを実行
        foreach (var playableCard in playableCards)
        {
            // 勝利数を初期化
            winCount[playableCard] = 0;
            
            // 自分の手札からプレイしたカードを削除
            var newMyHand = input.Hand.Remove(playableCard);

            // 全プレイヤーの手札
            var handsBuilder = ImmutableArray.CreateBuilder<ImmutableList<Card>>();
            foreach (var opponentHand in opponentHands.Take(input.PlayerIndex.Value))
            {
                handsBuilder.Add(opponentHand);
            }
            handsBuilder.Add(newMyHand);
            foreach (var opponentHand in opponentHands.Skip(input.PlayerIndex.Value))
            {
                handsBuilder.Add(opponentHand);
            }
            var nextHands = handsBuilder.ToImmutable();

            // 次のプレイヤーを決定
            var nextPlayerIndex = DaifugoGame.GetNextPlayablePlayerIndex(
                input.PlayerIndex, handCounts: nextHands.Select(hand => hand.Count).ToArray());

            // ゲームの状態を構成
            var gameState = new GameState
            {
                PlayerIndex = nextPlayerIndex,
                LastPlayedPlayerIndex = input.PlayerIndex,
                Hands = nextHands,
                LastPlayedCard = playableCard,
                PlayHistory = input.PlayHistory.Add(playableCard),
                PassStreak = input.PassStreak
            };

            // シミュレーションを実施
            for (var i = 0; i < simulationCount; i++)
            {
                // プレイアウト
                var winnerIndex = _playout(gameState);
                // 勝利したならカウントを増やす
                if (winnerIndex == input.PlayerIndex)
                {
                    winCount[playableCard] = winCount.GetValueOrDefault(playableCard) + 1;
                }
            }
        }

        // 最も勝利数が多いカードを取得
        var bestCard = winCount
            .OrderByDescending(x => x.Value)
            .First();
        // 勝利数が0より大きい場合のみ返す
        if (bestCard.Value > 0)
        {
            return new PlayerAction.Play(bestCard.Key);
        }

        // どのカードでも勝利できなかった場合はパス
        return new PlayerAction.Pass();
    }

    // 相手の手札を生成
    private static List<ImmutableList<Card>> _generateOpponentHands(ImmutableArray<int> opponentHandCount,
        ImmutableList<Card> playHistory)
    {
        var knownCards = playHistory.ToHashSet();
        // 全パターンのカードを生成
        var unknownCards = (
            from suit in Enum.GetValues<Suit>()
            from rank in Enum.GetValues<Rank>()
            select new Card(suit, rank)
        ).ToHashSet();
        
        // 既に分かっているカードを除外
        unknownCards.RemoveWhere(card => knownCards.Contains(card));
        
        // カードをシャッフル
        var shuffled = unknownCards.OrderBy(_ => Random.Shared.Next()).ToList();
        var result = new List<ImmutableList<Card>>();
        var index = 0;
        
        // 各対戦相手の手札を生成
        foreach (var count in opponentHandCount)
        {
            result.Add(shuffled.Skip(index).Take(count).ToImmutableList());
            index += count;
        }

        return result;
    }

    private static PlayerIndex _playout(GameState gameState)
    {
        // 勝者が決定するまでループ
        while (true)
        {
            // 次の行動を決定
            PlayerAction action;
            var playerHand = gameState.Hands[gameState.PlayerIndex.Value];
            // Console.WriteLine($"{gameState.PlayerIndex.Value}: {string.Join(", ", playerHand)}");
            // 合法手を取得
            var validCards = playerHand
                .Where(card => DaifugoGame.IsValidPlay(card, gameState.LastPlayedCard))
                .ToList();
            // 合法手があるならランダムに選択
            if (validCards.Count > 0)
            {
                var picked = Random.Shared.Next(validCards.Count);
                action = new PlayerAction.Play(validCards[picked]);
            }
            // 合法手がないならパス
            else
            {
                action = new PlayerAction.Pass();
            }

            // 選択した行動を元にゲーム状態を更新
            var nextGameState = DaifugoGame.PlayOneTurn(gameState, action);

            // ゲーム終了をチェック
            if (nextGameState.Hands[gameState.PlayerIndex.Value].Count == 0)
            {
                return gameState.PlayerIndex;
            }

            gameState = nextGameState;
        }
    }
}
