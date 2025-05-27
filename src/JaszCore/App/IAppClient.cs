namespace JaszCore.App
{
    public interface IAppClient
    {
        string GetSystemId();
        string[] GetAppArgs();
    }
}