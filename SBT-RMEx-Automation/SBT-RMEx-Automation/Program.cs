using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CsvFileTransferApp;
using Renci.SshNet;
using Serilog;
using Serilog.Events;

class Program
{
    static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("/logs/SBT-RMEx_{Date}.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        string clientsJsonFile = "clientDetails.json";
        DateTime currentDate = DateTime.Now.Date;

        try
        {
            var clients = JsonSerializer.Deserialize<Client[]>(File.ReadAllText(clientsJsonFile));

            foreach (var clientInfo in clients)
            {
                string sourceDirectory = clientInfo.SourceDirectory;
                string destinationDirectory = clientInfo.DestinationDirectory;

                DirectoryInfo dirInfo = new DirectoryInfo(sourceDirectory);

                // Getting all .csv files in the source directory created on the current date
                var csvFiles = dirInfo.GetFiles("*.csv")
                    .Where(file => file.CreationTime.Date == currentDate);

                using (var client = new SftpClient(clientInfo.IpAddress, clientInfo.Username, clientInfo.Password))
                {
                    client.Connect();
                    client.ChangeDirectory(destinationDirectory);

                    foreach (var csvFile in csvFiles)
                    {
                        using (var fileStream = File.OpenRead(csvFile.FullName))
                        {
                            client.UploadFile(fileStream, csvFile.Name);
                        }

                        // Moving to a different directory as backup
                        string processedDirectory = Path.Combine(sourceDirectory, "Processed");
                        Directory.CreateDirectory(processedDirectory);
                        File.Move(csvFile.FullName, Path.Combine(processedDirectory, csvFile.Name));

                        Log.Information($"Transferred: {csvFile.Name}");
                    }

                    client.Disconnect();
                }
            }

            Log.Information("All matching files transferred successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error: {ex.Message}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

namespace CsvFileTransferApp
{
    public class Client
    {
        public string? ClientName { get; set; }
        public string? IpAddress { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SourceDirectory { get; set; }
        public string? DestinationDirectory { get; set; }
    }
}
