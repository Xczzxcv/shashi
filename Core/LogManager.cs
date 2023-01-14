namespace Core;

public class LogManager
{
    private ILogger _logger;

    public LogManager()
    {
        _logger = new MockLogger();
        _logger.Init();
    }

    public void Setup(ILogger? logger = null)
    {
        if (logger == null)
        {
            return;
        }

        _logger = logger;
        _logger.Init();
    }

    public void Log(string logMessage)
    {
        _logger.Log(logMessage);
    }
}