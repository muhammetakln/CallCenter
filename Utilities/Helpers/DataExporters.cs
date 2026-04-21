using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MiniExcelLibs;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Utilities.Helpers
{
    /// <summary>
    /// C# nesnelerini CSV, Excel, JSON dosyalarına yazır (dışa aktarır)
    /// </summary>
    public static class DataExporters
    {
        /// <summary>
        /// C# nesnelerini CSV dosyasına yazar
        /// </summary>
        public static async Task ExportCsvAsync<T>(
            IEnumerable<T> records,
            Stream stream)
        {
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(
                writer,
                new CsvConfiguration(CultureInfo.InvariantCulture)
            );

            // Başlık satırı ve kayıtları yaz
            await csv.WriteRecordsAsync(records);
            await writer.FlushAsync();
        }

        /// <summary>
        /// C# nesnelerini Excel dosyasına yazar
        /// </summary>
        public static async Task ExportExcelAsync<T>(
            IEnumerable<T> records,
            Stream stream) where T : class, new()
        {
            // MiniExcelLibs kullanarak stream'e yaz
            await stream.SaveAsAsync(records);
        }

        /// <summary>
        /// C# nesnelerini JSON dosyasına yazar
        /// </summary>
        public static async Task ExportJsonAsync<T>(
            IEnumerable<T> records,
            Stream stream)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Okunabilir format (pretty-print)
                Converters = { new JsonStringEnumConverter() }
            };

            await JsonSerializer.SerializeAsync(stream, records, options);
        }
    }
}

