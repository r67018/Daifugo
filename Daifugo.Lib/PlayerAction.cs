using System.Collections.Immutable;

namespace Daifugo.Lib;

public abstract record PlayerAction
{
    /// <summary>
    /// カードのプレイ
    /// </summary>
    /// <param name="Cards">場に出すカード</param>
    public sealed record Play(ImmutableArray<Card> Cards) : PlayerAction;

    /// <summary>
    /// パス
    /// </summary>
    public sealed record Pass : PlayerAction;
}