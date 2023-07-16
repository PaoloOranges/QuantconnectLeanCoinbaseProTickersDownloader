// See https://aka.ms/new-console-template for more information
using QuantConnect.ToolBox.GDAXDownloader;
using System.Globalization;
using System.IO;

Console.WriteLine("Hello, World!");

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

string[] TIME_RESOLUTIONS = { "Minute", "Hour", "Daily" };

foreach (string timeResolution in TIME_RESOLUTIONS)
{
    GDAXDownloaderProgram.GDAXDownloader(tickers, timeResolution, fromDate, toDate);
}

// Success
File.WriteAllText(lastSuccessFilePath, DateTime.Now.ToString(DATE_FORMAT));