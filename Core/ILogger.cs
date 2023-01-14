namespace Core;

public interface ILogger
{
    public void Init();
    public void Log(string logMessage);
}