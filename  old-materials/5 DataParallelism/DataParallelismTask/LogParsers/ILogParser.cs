namespace LogParsing.LogParsers
{
    public interface ILogParser
    {
        string[] GetRequestedIdsFromLogFile();
    }
}