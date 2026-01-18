namespace Daifugo.Lib;

public abstract record PlayerAction
{
    /// <summary>
    /// カードのプレイ
    /// </summary>
    /// <param name="Card">場に出すカード</param>
    public sealed record Play(Card Card) : PlayerAction;

    /// <summary>
    /// パス
    /// </summary>
    public sealed record Pass : PlayerAction;
}