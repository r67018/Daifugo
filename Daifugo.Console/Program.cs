using System.Collections.Immutable;
using Daifugo.Lib;

// プレイヤーの人数
const int playerCount = 3;

// 全パターンのカードを生成してシャッフル
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

// ダイヤの3を持っているプレイヤーを先攻にする
var startingPlayerIndex = Array.FindIndex(hands, hand => hand.Contains(new Card(Suit.Diamond, Rank.Three)));

// ゲーム状態を初期化
var gameState = new GameState
{
    PlayerIndex = new PlayerIndex(startingPlayerIndex),
    LastPlayedPlayerIndex = new PlayerIndex((startingPlayerIndex - 1) % playerCount), // とりあえず
    Hands = [.. hands.Select(h => h.ToImmutableList())],
    LastPlayedCards = null,
    PlayHistory = [],
    PassStreak = 0
};

var solver = new MonteCarloSolver();

var turnCount = 0;
while (!DaifugoGame.IsGameOver(gameState.Hands.Select(h => h.Count).ToArray()))
{
    // 現在のプレイヤーの行動を決定
    var solverInput = new SolverInput(gameState);
    var action = solver.FindMostValidPlay(solverInput, 5);
    var currentPlayer = gameState.PlayerIndex;

    // ゲーム状態を更新
    gameState = DaifugoGame.PlayOneTurn(gameState, action);

    // デバッグ
    turnCount++;
    Console.WriteLine($"--- Turn {turnCount} ---");

    // ImmutableArrayのデバッグ出力用フォーマッタ
    string ToDisplayString(Card card)
    {
        if (card.Suit == Suit.Joker || card.Rank == Rank.Joker) return "JK";

        var suitStr = card.Suit switch
        {
            Suit.Spade => "♠",
            Suit.Heart => "♥",
            Suit.Diamond => "♦",
            Suit.Club => "♣",
            _ => "?"
        };

        var rankStr = card.Rank switch
        {
            Rank.Ace => "A",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Joker => "JK", // これは上のifで引っかかるはずだが念の���め
            _ => ((int)card.Rank).ToString()
        };

        // Rank enumの値は強さ順（3=3, ..., A=14, 2=15）になっているため、
        // 表示用に数値を補正する必要がある。
        // ただし現在のRank enum定義は:
        // Three=3, Four=4, ..., Nine=9, Ten=10
        // Jack=11, Queen=12, King=13, Ace=14, Two=15

        // 3~10はそのままでOK
        // 11=J, 12=Q, 13=K
        // 14=A (Ace)
        // 15=2 (Two)

        switch (card.Rank)
        {
            case Rank.Ace: rankStr = "A"; break;
            case Rank.Two: rankStr = "2"; break;
            // 3~10まではenumの文字列表現ではなく数値を使いたいが、enum値が数値と一致している
            // しかしToString()すると "Three" とかになるのでキャストが必要
            // 11,12,13はJ,Q,K
            default:
                if (card.Rank >= Rank.Three && card.Rank <= Rank.Ten)
                {
                    rankStr = ((int)card.Rank).ToString();
                }
                break;
        }

        return $"{suitStr}{rankStr}";
    }

    string FormatCards(IEnumerable<Card> cards) => string.Join(", ", cards.Select(ToDisplayString));

    // 今回の行動を表示
    var actionMessage = action switch
    {
        PlayerAction.Play play => $"Play {{ Cards = [{FormatCards(play.Cards)}] }}",
        PlayerAction.Pass => "Pass",
        _ => action.ToString() ?? ""
    };
    Console.WriteLine($"Player {currentPlayer.Value} played: {actionMessage}");

    // 場に出ているカードを表示
    var lastPlayedMessage = gameState.LastPlayedCards.HasValue
        ? $"[{FormatCards(gameState.LastPlayedCards.Value)}]"
        : "None";
    Console.WriteLine($"Last Played Card: {lastPlayedMessage}");
    // 各プレイヤーの手札数を表示
    Console.Write("Hands: ");
    for (var i = 0; i < playerCount; i++)
    {
        Console.Write($"{gameState.Hands[i].Count} ");
    }
    Console.WriteLine();
    Console.WriteLine("------------------");

    Console.ReadLine();
}

Console.WriteLine("Game Over!");

