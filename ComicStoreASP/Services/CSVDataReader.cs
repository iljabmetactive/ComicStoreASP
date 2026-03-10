using ComicStoreASP.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;
namespace ComicStoreASP.Services
{
    public class CSVDataReader
    {
        public virtual IEnumerable<Comics> ReadCsvFile(Stream fileStream, int maxRowCount = 7000)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,

                    // IMPORTANT: makes headers forgiving
                    PrepareHeaderForMatch = args =>
                        args.Header.Replace(" ", "")
                                   .Replace("_", "")
                                   .ToLower(),

                    // Change if your CSV uses ;
                    Delimiter = ","
                };

                using var reader = new StreamReader(fileStream);
                using var csv = new CsvReader(reader, config);

                var records = new List<Comics>(maxRowCount);

                foreach(var record in csv.GetRecords<Comics>())
                {
                    records.Add(record);
                    
                    if(records.Count >= maxRowCount)
                    {
                        break;
                    }
                }
                return records;
            }
            catch (HeaderValidationException ex)
            {
                // Specific exception for header issues
                throw new ApplicationException("CSV file header is invalid.", ex);
            }
            catch (TypeConverterException ex)
            {
                // Specific exception for type conversion issues
                throw new ApplicationException("CSV file contains invalid data format.", ex);
            }
            catch (Exception ex)
            {
                // General exception for other issues
                throw new ApplicationException("Error reading CSV file", ex);
            }
        }
    }
}
