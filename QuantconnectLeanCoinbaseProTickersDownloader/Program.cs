// See https://aka.ms/new-console-template for more information
using QuantConnect.Data;
using QuantConnect;
using QuantConnect.ToolBox.GDAXDownloader;
using QuantConnect.Logging;
using System.Globalization;
using QuantConnect.Configuration;
using System.Text.Json;
using QuantconnectLeanCoinbaseProTickersDownloader;

string configFilePath = "";
string outputPath = "";
string tickerFilePath;

if (args.Length >= 3)
{
    configFilePath = args[0];
    outputPath = args[1];
    tickerFilePath = args[2];
}
else
{
    Console.WriteLine("No arguments, Expected, in order: <ConfigFilePath> <OutputPath> <TickersFile> ");
    return;
}

if(File.Exists(configFilePath))
{
    Config.SetConfigurationFile(configFilePath);
}
else
{
    Console.WriteLine(configFilePath + " does not exists");
    return;
}

const string LAST_DOWNLOAD_SUCCESS_TIME_FILE = "last_success_time";
const string DATE_FORMAT = "yyyyMMdd-HH:mm:ss";




string lastSuccessFilePath = Path.Combine(outputPath, LAST_DOWNLOAD_SUCCESS_TIME_FILE);

DataSerializationObject tickersAndDate = new DataSerializationObject();
if(File.Exists(lastSuccessFilePath))
{
    string jsonFileStr = File.ReadAllText(lastSuccessFilePath);
    tickersAndDate = JsonSerializer.Deserialize<DataSerializationObject>(jsonFileStr)!;
}

string[] tickers;
if (File.Exists(tickerFilePath))
{
    string tickersStr = File.ReadAllText(tickerFilePath);
    Console.WriteLine(tickersStr);
    tickers = tickersStr.Split(",");
}
else
{
    Console.WriteLine("Ticker file does not exists");
    return;
}

Resolution[] TIME_RESOLUTIONS = { Resolution.Minute, Resolution.Hour, Resolution.Daily };
try
{
    var downloader = new GDAXDownloader();
    
    // Load settings from config.json
    var dataDirectory = Globals.DataFolder;

    foreach (var ticker in tickers)
    {
        const string market = Market.GDAX;
        var symbolObject = Symbol.Create(ticker, SecurityType.Crypto, market);

        DateTime fromDate = new DateTime(DateTime.Now.Year - 2, 1, 1, 0, 0, 0);
        DateTime toDate = DateTime.Now;

        string fromDateStr;
        if (tickersAndDate.tickersAndLastTime.TryGetValue(ticker, out fromDateStr))
        {
            fromDate = DateTime.ParseExact(fromDateStr, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        foreach (Resolution timeResolution in TIME_RESOLUTIONS)
        {
            
            var data = downloader.Get(new DataDownloaderGetParameters(symbolObject, timeResolution, fromDate, toDate));

            // Save the data
            var writer = new LeanDataWriter(timeResolution, symbolObject, dataDirectory, TickType.Trade);
            var distinctData = data.GroupBy(i => i.Time, (key, group) => group.First()).ToArray();

            writer.Write(distinctData);
        }
        tickersAndDate.tickersAndLastTime[ticker] = DateTime.Now.ToString(DATE_FORMAT);
    }

    // Success
    

}
catch (Exception err)
{
    Log.Error(err);
    Log.Error(err.Message);
    Log.Error(err.StackTrace);
}
finally
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    string jsonString = JsonSerializer.Serialize(tickersAndDate, options);

    File.WriteAllText(lastSuccessFilePath, jsonString);
}