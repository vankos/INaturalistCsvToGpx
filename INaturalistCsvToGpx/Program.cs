// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Xml;
using Microsoft.VisualBasic.FileIO;

ValidateComandLineParam();
string gpx = GenerateGpx();
SaveGpx(gpx);
Console.WriteLine("Done! Press any key to exit.");
Console.ReadKey();

void ValidateComandLineParam()
{
    string[] commandLineArgs = Environment.GetCommandLineArgs();
    if (commandLineArgs.Length != 2)
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

string GenerateGpx()
{
    string[] commandLineArgs = Environment.GetCommandLineArgs();
    string filePath = commandLineArgs[1];
    using TextFieldParser parser = new(filePath);
    parser.TextFieldType = FieldType.Delimited;
    parser.SetDelimiters(",");
    string[]? headers = parser.ReadFields();
    if (headers == null)
    {
        Console.WriteLine("No headers found in CSV file");
        Environment.Exit(1);
    }

    GetCoordinatesFieldsIndexes(headers, out int latIndex, out int lonIndex);
    StringBuilder gpxStringBuilder = new();
    int commonNameFieldIndex = Array.IndexOf(headers, Constants.CommonNameFieldName);
    using (XmlWriter writer = XmlWriter.Create(gpxStringBuilder, new XmlWriterSettings { Indent = true }))
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("gpx");
        writer.WriteAttributeString("version", "1.1");
        writer.WriteAttributeString("creator", "INaturalistCsvToGpx");
        writer.WriteElementString("desc", "Converted gpx from INaturalist CSV");
        while (!parser.EndOfData)
        {
            string[]? fields = parser.ReadFields();
            if (fields == null)
            {
                continue;
            }

            string lat = fields[latIndex];
            string lon = fields[lonIndex];
            writer.WriteStartElement("wpt");
            writer.WriteAttributeString("lat", lat);
            writer.WriteAttributeString("lon", lon);
            writer.WriteStartElement("desc");
            for (int i = 0; i < fields.Length; i++)
            {
                if (i == latIndex || i == lonIndex)
                {
                    continue;
                }
               
                writer.WriteString($"{headers[i]}: {fields[i]} <br>");
            }

            writer.WriteEndElement();
            writer.WriteStartElement("name");
            if (commonNameFieldIndex != -1)
            {
                writer.WriteString(fields[commonNameFieldIndex]);
            }
            else
            {
                writer.WriteString($"Intauralist export");
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

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

static void GetCoordinatesFieldsIndexes(string[] headers, out int latIndex, out int lonIndex)
{
    latIndex = Array.IndexOf(headers, Constants.LattitudeFieldName);
    if (latIndex == -1)
    {
        Console.WriteLine($"{Constants.LattitudeFieldName} field not found");
        Environment.Exit(1);
    }

    lonIndex = Array.IndexOf(headers, Constants.LongitudeFieldName);
    if (lonIndex == -1)
    {
        Console.WriteLine($"{Constants.LongitudeFieldName} field not found");
        Environment.Exit(1);
    }
}