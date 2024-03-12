using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TradingBot.DataHandler;

public class MarketData
{
    private readonly string _apiBaseUrl;

    public MarketData(string apiBaseUrl)
    {
        _apiBaseUrl = apiBaseUrl;
    }

    public async Task<string> GetMarketDataAsync(string endpoint)
    {
        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(_apiBaseUrl);
            HttpResponseMessage response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                return data;
            }
            else
            {
                // Handle HTTP request errors here
                throw new Exception("Failed to retrieve market data.");
            }
        }
    }
}
