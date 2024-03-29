﻿using Core;
using Core.NeuralNet;

namespace Shashi_console;

public static class Program
{
    public static async Task Main()
    {
        var rateBoardNet = new RateBoardNeuralNet();
        rateBoardNet.Init();
        await GameHelper.SimulateMultipleGames(new GameHelper.GameSimulationArgs
        {
            WhitesPlayer = new ExperiencedBotPlayer(rateBoardNet),
            BlacksPlayer = null,
            Logger = new ConsoleLogger(),
            GamesAmount = 1,
        });
        GameHelper.LogPostRunStats();
    }
}