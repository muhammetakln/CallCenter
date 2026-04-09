using System.Text.Json;

namespace LogTest.Models
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public override string ToString()
        {
            //sınfın kendisini json formatına döndürürr.Daha okunaklı hata mesajları halien getirrir.
            return JsonSerializer.Serialize(this);

        }
    }
}
