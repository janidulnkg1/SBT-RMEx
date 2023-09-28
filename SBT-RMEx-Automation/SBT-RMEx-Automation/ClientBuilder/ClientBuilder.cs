using CsvFileTransferApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBT_RMEx_Automation.ClientBuilder
{
    public class ClientBuilder : IClientBuilder
    {
        private Client client = new Client();

        public IClientBuilder WithIpAddress(string ipAddress)
        {
            client.IpAddress = ipAddress;
            return this;
        }

        public IClientBuilder WithUsername(string username)
        {
            client.Username = username;
            return this;
        }

        public IClientBuilder WithPassword(string password)
        {
            client.Password = password;
            return this;
        }

        public IClientBuilder WithSourceDirectory(string sourceDirectory)
        {
            client.SourceDirectory = sourceDirectory;
            return this;
        }

        public IClientBuilder WithDestinationDirectory(string destinationDirectory)
        {
            client.DestinationDirectory = destinationDirectory;
            return this;
        }

        public Client Build()
        {
            return client;
        }
    }
}
