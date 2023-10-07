using Renci.SshNet;
using SBT_RMEx_Automation;
using SBT_RMEx_Automation.ClientBuilder;
using Serilog;
using System.Text.Json;

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
        // Using ClientBuilder to create a Client object
        IClientBuilder clientBuilder = new ClientBuilder()
            .WithIpAddress(clientInfo.IpAddress)
            .WithUsername(clientInfo.Username)
            .WithPassword(clientInfo.Password)
            .WithSourceDirectory(clientInfo.SourceDirectory)
            .WithDestinationDirectory(clientInfo.DestinationDirectory);

        Client client = clientBuilder.Build();

        DirectoryInfo dirInfo = new DirectoryInfo(client.SourceDirectory);

        // Getting all .csv files in the source directory created on the current date
        var csvFiles = dirInfo.GetFiles("*.csv")
            .Where(file => file.CreationTime.Date == currentDate);

        using (var sftpClient = new SftpClient(client.IpAddress, client.Username, client.Password))
        {
            sftpClient.Connect();
            sftpClient.ChangeDirectory(client.DestinationDirectory);

            foreach (var csvFile in csvFiles)
            {
                using (var fileStream = File.OpenRead(csvFile.FullName))
                {
                    sftpClient.UploadFile(fileStream, csvFile.Name);
                }

                // Moving to a different directory as backup
                string processedDirectory = Path.Combine(client.SourceDirectory, "Processed");
                Directory.CreateDirectory(processedDirectory);
                File.Move(csvFile.FullName, Path.Combine(processedDirectory, csvFile.Name));

                Log.Information($"Transferred: {csvFile.Name}");
            }

            sftpClient.Disconnect();
        }
    }

    Log.Information("All .csv files transferred successfully.");
}
catch (Exception ex)
{
    Log.Error(ex, $"Error: {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}
