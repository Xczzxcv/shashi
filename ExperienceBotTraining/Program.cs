using System.Globalization;
using Core;

namespace ExperienceBotTraining;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ParseArguments(args, out var generationsAmount, out var trainingSpeed);

        Console.WriteLine("Training experienced bot. Press enter to stop");
        var cts = new CancellationTokenSource();
        Task.Run(() =>
        {
            Console.ReadLine();
            Console.WriteLine("Training will be stopped as soon as current generation will be processed");
            cts.Cancel();
        });
        var experiencedBotTrainingManager = new ExperiencedBotTrainingManager();
        experiencedBotTrainingManager.Init();

        await experiencedBotTrainingManager.TrainBot(generationsAmount, trainingSpeed, cts.Token);
        
        experiencedBotTrainingManager.Dispose();
        GameHelper.LogPostRunStats();
    }

    private static void ParseArguments(string[] args, out int generationsAmount, out double trainingSpeed)
    {
        Console.WriteLine("ARGS: " + string.Join(", ", args.Select(arg => $"'{arg}'")));
        if (args.Length == 2)
        {
            generationsAmount = int.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture,
                out var inputGenerationsAmount)
                ? inputGenerationsAmount
                : throw new ArgumentException($"Can't parse generations amount from 1-st argument ({args[0]}");
            trainingSpeed = double.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture,
                out var inputTrainingSpeed)
                ? inputTrainingSpeed
                : throw new ArgumentException($"Can't parse training speed from 2-nd argument ({args[1]})");
        }
        else
        {
            generationsAmount = 1;
            trainingSpeed = 1;
        }
    }
}