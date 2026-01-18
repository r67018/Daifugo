using System.Collections.Immutable;
using Daifugo.Lib;

// プレイヤーの人数
const int playerCount = 3;

// 全パターンのカードを生成してシャッフル
var cards =
    from suit in Enum.GetValues<Suit>()
    from rank in Enum.GetValues<Rank>()
    select new Card(suit, rank);
var shuffled = cards.Shuffle().ToArray();

// 手札を配る
var hands = Enumerable.Range(0, playerCount).Select(_ => new List<Card>()).ToArray();
for (var i = 0; i < shuffled.Length; i++)
{
    hands[i % playerCount].Add(shuffled[i]);
}

// ゲーム状態を初期化
var gameState = new GameState
{
    PlayerIndex = new PlayerIndex(0),
    LastPlayedPlayerIndex = new PlayerIndex(playerCount - 1), // とりあえず
    Hands = [.. hands.Select(h => h.ToImmutableList())],
    LastPlayedCard = null,
    PlayHistory = [],
    PassStreak = 0
};

var solver = new MonteCarloSolver();

var turnCount = 0;
while (!DaifugoGame.IsGameOver(gameState.Hands.Select(h => h.Count).ToArray()))
{
    // 現在のプレイヤーの行動を決定
    var solverInput = new SolverInput(gameState);
    var action = solver.FindMostValidPlay(solverInput, 10);
    var currentPlayer = gameState.PlayerIndex;

    // ゲーム状態を更新
    gameState = DaifugoGame.PlayOneTurn(gameState, action);

    // デバッグ
    turnCount++;
    Console.WriteLine($"--- Turn {turnCount} ---");
    // 今回の行動を表示
    Console.WriteLine($"Player {currentPlayer.Value} played: {action}");
    // 場に出ているカードを表示
    Console.WriteLine($"Last Played Card: {gameState.LastPlayedCard}");
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

