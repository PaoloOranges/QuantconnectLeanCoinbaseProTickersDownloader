// See https://aka.ms/new-console-template for more information
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
const string TICKERS_FILE = "tickers";

DateTime startTime;
DateTime endTime = DateTime.Now;

string lastSuccessFilePath = Path.Combine(outputPath, LAST_DOWNLOAD_SUCCESS_TIME_FILE);

if(File.Exists(lastSuccessFilePath))
{
    Console.WriteLine(File.ReadAllText(lastSuccessFilePath));    
}