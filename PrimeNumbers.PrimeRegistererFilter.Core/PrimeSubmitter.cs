using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrimeNumbers.PrimeRegistererFilter.Core
{
    public sealed class PrimeSubmitter : IPrimeSubmitter, IDisposable
    {
        private readonly HttpClient _client;

        private PrimeSubmitter(HttpClient client)
        {
            _client = client;
        }

        public static PrimeSubmitter CreateNew(Uri connectionUri)
        {
            HttpClient client = new()
            {
                BaseAddress = connectionUri
            };
            return new PrimeSubmitter(client);
        }

        public async Task SubmitPrimeRecord(PrimeRecord record)
        {
            HttpContent content;
            using (MemoryStream jsonStream = new())
            using (StreamReader reader = new(jsonStream, Encoding.UTF8))
            {
                await JsonSerializer.SerializeAsync(jsonStream, record);
                jsonStream.Position = 0;
                string jsonString = await reader.ReadToEndAsync().ConfigureAwait(false);
                content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }
            HttpResponseMessage response = await _client.PostAsync("Primes", content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Submitting prime record failed, got status code [{response.StatusCode}].");
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
