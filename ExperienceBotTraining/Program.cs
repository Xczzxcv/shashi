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
            Console.WriteLine("Simulation will be stopped as soon as current generation will be processed");
            cts.Cancel();
        });
        var experiencedBotTrainingManager = new ExperiencedBotTrainingManager();
        experiencedBotTrainingManager.Init();

        const int generationsAmount = 300;
        const double trainingSpeed = 0.5;
        await experiencedBotTrainingManager.TrainBot(generationsAmount, trainingSpeed, cts.Token);
        
        experiencedBotTrainingManager.Dispose();
        GameHelper.LogPostRunStats();
        Console.WriteLine("Training ended. Press enter to finish program");
        Console.ReadLine();
    }
}