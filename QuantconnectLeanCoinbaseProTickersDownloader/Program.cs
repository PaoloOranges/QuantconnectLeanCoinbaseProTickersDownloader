// See https://aka.ms/new-console-template for more information
using QuantConnect.Data;
using QuantConnect;
using QuantConnect.ToolBox.GDAXDownloader;
using QuantConnect.Logging;
using System.Globalization;

string outputPath = "";
string tickerFilePath;

if (args.Length >= 2)
{
    outputPath = args[0];
    tickerFilePath = args[1];
}
else
{
    Console.WriteLine("No arguments, Expected, in order: OutputPath TickersFile ");
    return;
}

const string LAST_DOWNLOAD_SUCCESS_TIME_FILE = "last_success_time";
const string DATE_FORMAT = "yyyyMMdd-HH:mm:ss";

DateTime fromDate;
DateTime toDate = DateTime.Now;

string lastSuccessFilePath = Path.Combine(outputPath, LAST_DOWNLOAD_SUCCESS_TIME_FILE);

if(File.Exists(lastSuccessFilePath))
{
    string startTimeStr = File.ReadAllText(lastSuccessFilePath);    
    fromDate = DateTime.ParseExact(startTimeStr, DATE_FORMAT, CultureInfo.InvariantCulture);
}
else
{
    fromDate = new DateTime(DateTime.Now.Year - 2, 1, 1, 0, 0, 0);
}
Console.WriteLine("Start gathering tickers from " + fromDate.ToString(DATE_FORMAT));

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
    foreach (Resolution timeResolution in TIME_RESOLUTIONS)
    {

        // Load settings from config.json
        var dataDirectory = Globals.DataFolder;

        // Create an instance of the downloader
        const string market = Market.GDAX;
        var downloader = new GDAXDownloader();
        foreach (var ticker in tickers)
        {
            // Download the data
            var symbolObject = Symbol.Create(ticker, SecurityType.Crypto, market);

            var data = downloader.Get(new DataDownloaderGetParameters(symbolObject, timeResolution, fromDate, toDate));

            // Save the data
            var writer = new LeanDataWriter(timeResolution, symbolObject, dataDirectory, TickType.Trade);
            var distinctData = data.GroupBy(i => i.Time, (key, group) => group.First()).ToArray();

            writer.Write(distinctData);
        }

    }

    // Success
    File.WriteAllText(lastSuccessFilePath, DateTime.Now.ToString(DATE_FORMAT));

}
catch (Exception err)
{
    Log.Error(err);
    Log.Error(err.Message);
    Log.Error(err.StackTrace);
}