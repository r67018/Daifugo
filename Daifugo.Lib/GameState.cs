using System.Collections.Immutable;

namespace Daifugo.Lib;

public readonly record struct PlayerIndex(int Value);

/// <summary>
/// ターン開始時のゲーム状態を表すレコード
/// </summary>
public record GameState
{
    /// <summary>
    /// 操作するプレイヤーのインデックス(0から始まる)
    /// </summary>
    public required PlayerIndex PlayerIndex { get; init; }

    /// <summary>
    /// 最後にカードをプレイしたプレイヤーのインデックス
    /// </summary>
    public required PlayerIndex LastPlayedPlayerIndex { get; init; }

    /// <summary>
    /// 各プレイヤーの手札
    /// </summary>
    public required ImmutableArray<ImmutableList<Card>> Hands { get; init; }

    /// <summary>
    /// 現在の場に出ているカード
    /// まだプレイされていない場合はnull
    /// </summary>
    public required Card? LastPlayedCard { get; init; }
    
    /// <summary>
    /// 今までに出されたカード
    /// </summary>
    public required ImmutableList<Card> PlayHistory { get; init; }

    /// <summary>
    /// パスが連続した回数
    /// </summary>
    public required int PassStreak { get; init; } = 0;
}