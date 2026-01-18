using System.Collections.Immutable;

namespace Daifugo.Lib;

public class MonteCarloSolver : ISolver
{
    public PlayerAction FindMostValidPlay(SolverInput input, int simulationCount)
    {
        // 合法手を列挙
        var legalPlays = _generateLegalPlays(input.Hand, input.LastPlayedCards);

        // 解が一意に定まる場合はその解を返す
        switch (legalPlays.Count)
        {
            // 合法手がない場合はパス
            case 0:
                return new PlayerAction.Pass();
            // 合法手が1つだけの場合はそれを返す
            case 1:
                return new PlayerAction.Play(legalPlays[0]);
        }

        // 相手の手札を生成
        var opponentHands = _generateOpponentHands(input.OpponentHandCount, input.PlayHistory);
        // 勝利数を格納するハッシュ
        var winCount = new Dictionary<ImmutableArray<Card>, int>();

        // 各合法手でシミュレーションを実行
        foreach (var play in legalPlays)
        {
            // 勝利数を初期化
            winCount[play] = 0;

            // 自分の手札からプレイしたカードを削除
            var newMyHand = input.Hand.RemoveRange(play);

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
                LastPlayedCards = play,
                PlayHistory = input.PlayHistory.Add(play),
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
                    winCount[play] = winCount.GetValueOrDefault(play) + 1;
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

        // どのカードでも勝利できなかった場合はランダム
        return new PlayerAction.Play(legalPlays[Random.Shared.Next(legalPlays.Count)]);
    }

    private static List<ImmutableArray<Card>> _generateLegalPlays(ImmutableList<Card> hand, ImmutableArray<Card>? lastPlayedCards)
    {
        var legalPlays = new List<ImmutableArray<Card>>();
        for (var k = 1; k <= 4; k++)
        {
            foreach (var combination in _getCombinations(hand, k))
            {
                if (DaifugoGame.IsLegalPlay(combination, lastPlayedCards))
                {
                    legalPlays.Add(combination);
                }
            }
        }

        return legalPlays;
    }

    private static IEnumerable<ImmutableArray<Card>> _getCombinations(ImmutableList<Card> list, int length)
    {
        if (length == 0)
        {
            yield return ImmutableArray<Card>.Empty;
            yield break;
        }

        for (var i = 0; i <= list.Count - length; i++)
        {
            var head = list[i];
            var rest = list.GetRange(i + 1, list.Count - (i + 1));
            foreach (var tail in _getCombinations(rest, length - 1))
            {
                yield return tail.Insert(0, head);
            }
        }
    }

    // 相手の手札を生成
    private static List<ImmutableList<Card>> _generateOpponentHands(ImmutableArray<int> opponentHandCount,
        ImmutableList<ImmutableArray<Card>> playHistory)
    {
        // 既に分かっているカードを収集
        // フラットにしてHashSetに変換
        var knownCards = playHistory.SelectMany(x => x).ToHashSet();
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

            // 合法手を取得
            var validPlays = _generateLegalPlays(playerHand, gameState.LastPlayedCards);

            // 合法手があるならランダムに選択
            if (validPlays.Count > 0)
            {
                var picked = Random.Shared.Next(validPlays.Count);
                action = new PlayerAction.Play(validPlays[picked]);
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
