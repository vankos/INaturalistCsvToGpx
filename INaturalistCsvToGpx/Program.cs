// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

string[] file = OpenCsv();
string gpx = GenerateGpx(file);
SaveGpx(gpx);
Console.WriteLine("Done! Press any key to exit.");
Console.ReadKey();

string[] OpenCsv()
{
    string[] commandLineArgs = Environment.GetCommandLineArgs();
    ValidateComandLineParam(commandLineArgs);
    string filePath = commandLineArgs[1];
    string[] allLines = File.ReadAllLines(filePath);
    return allLines;
}

void ValidateComandLineParam(string[] commandLineArgs)
{
    if(commandLineArgs.Length != 2)
    {
        Console.WriteLine("Usage: INaturalistCsvToGpx <file>");
        Environment.Exit(1);
    }

    if (!commandLineArgs[1].EndsWith(".csv"))
    {
        Console.WriteLine("File must be a CSV file");
        Environment.Exit(1);
    }

    if (!File.Exists(commandLineArgs[1]))
    {
        Console.WriteLine("File not found: " + commandLineArgs[1]);
        Environment.Exit(1);
    }
}

string GenerateGpx(string[] file)
{
    string headersString = file[0];
    string[] headers = headersString.Split(",");
    int latIndex = Array.IndexOf(headers, Constants.LattitudeFieldName);
    if(latIndex == -1)
    {
        Console.WriteLine($"{Constants.LattitudeFieldName} field not found");
        Environment.Exit(1);
    }


    int lonIndex = Array.IndexOf(headers, Constants.LongitudeFieldName);
    if (lonIndex == -1)
    {
        Console.WriteLine($"{Constants.LongitudeFieldName} field not found");
        Environment.Exit(1);
    }

    StringBuilder gpxStringBuilder = new StringBuilder();
    using (XmlWriter writer = XmlWriter.Create(gpxStringBuilder, new XmlWriterSettings { Indent = true }))
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("gpx");
        writer.WriteAttributeString("version", "1.1");
        writer.WriteAttributeString("creator", "INaturalistCsvToGpx");
        writer.WriteElementString("desc", "Converted gpx from INaturalist CSV");

        foreach (string line in file.Skip(1))
        {
            string[] fields = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            string lat = fields[latIndex];
            string lon = fields[lonIndex];
            writer.WriteStartElement("wpt");
            writer.WriteAttributeString("lat", lat);
            writer.WriteAttributeString("lon", lon);
            for (int i = 0; i < fields.Length; i++)
            {
                if (i == latIndex || i == lonIndex)
                {
                    continue;
                }
                writer.WriteComment($"{headers[i]}: {fields[i]}");
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    return gpxStringBuilder.ToString();
}

void SaveGpx(string gpx)
{
    string fileName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[1]) + ".gpx";
    File.WriteAllText(fileName, gpx);
}