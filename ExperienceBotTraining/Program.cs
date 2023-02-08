using Core;

namespace ExperienceBotTraining;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Training experienced bot. Press enter to stop");
        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
            Console.ReadLine();
            Console.WriteLine("Simulation will be stopped as soon as current game will end");
            cts.Cancel();
        });
        var experiencedBotTrainingManager = new ExperiencedBotTrainingManager();
        experiencedBotTrainingManager.Init();

        await experiencedBotTrainingManager.TrainBot(200, cts.Token);
        
        experiencedBotTrainingManager.Dispose();
        GameHelper.LogPostRunStats();
    }
}