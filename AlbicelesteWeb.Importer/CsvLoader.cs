using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;


namespace AlbicelesteWeb.Importer;

public static class CsvLoader    

{
    public static List<T> Load<T>(string path)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Replace("_", "").ToLowerInvariant(),
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<T>().ToList();
    }
}