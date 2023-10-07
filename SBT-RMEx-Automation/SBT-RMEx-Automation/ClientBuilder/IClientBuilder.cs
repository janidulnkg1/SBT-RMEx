namespace SBT_RMEx_Automation.ClientBuilder
{
    public interface IClientBuilder
    {
        IClientBuilder WithIpAddress(string ipAddress);
        IClientBuilder WithUsername(string username);
        IClientBuilder WithPassword(string password);
        IClientBuilder WithSourceDirectory(string sourceDirectory);
        IClientBuilder WithDestinationDirectory(string destinationDirectory);
        Client Build();
    }
}
