namespace Daifugo.Lib;

public interface ISolver
{
    public PlayerAction FindMostValidPlay(SolverInput input, int simulationCount);
}