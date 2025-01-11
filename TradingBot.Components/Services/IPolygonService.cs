using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TradingBot.Domain;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Settings;

namespace TradingBot.Api.Services;

public interface IPolygonService
{
    Task<List<Candle>> GetAggregateBars(string ticker, int multiplier, TimeFrame timespan, string from, string to);
    Task<List<TickerType>> GetTickerTypes(AssetClass assetClass, Locale locale);
}

public class PolygonService : IPolygonService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly ILogger<PolygonService> _logger;
    private readonly AppSettings _appSettings;


    public PolygonService(ILogger<PolygonService> logger, AppSettings appSettings)
    {
        _httpClient = new HttpClient();
        _logger = logger;
        _appSettings = appSettings;

        _baseUrl = _appSettings.PolygonSettings.BaseUrl;
        _apiKey = _appSettings.PolygonSettings.ApiKey;
    }

    #region implimentation

    public async Task<List<Candle>> GetAggregateBars(string ticker, int multiplier, TimeFrame timespan, string from, string to)
    {
        var rtn = new List<Candle>();

        string url = $"{_baseUrl}/v2/aggs/ticker/{ticker}/range/{multiplier}/{timespan.ToString().ToLower()}/{from}/{to}?adjusted=true&sort=asc&apiKey={_apiKey}";

        while (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                JObject apiData = await GetData(url);

                var results = (JArray)apiData["results"];

                foreach (var result in results)
                {
                    Candle candle = JsonConvert.DeserializeObject<Candle>(result.ToString());

                    rtn.Add(candle);
                }

                // Get the next URL for pagination
                if (apiData["next_url"] is null)
                {
                    url = null;
                }
                else
                {
                    url = $"{apiData["next_url"]?.ToString()}&apiKey={_apiKey}";
                }

            }
            catch (Exception ex)
            {
                // Handle and log exceptions, then rethrow or return partial data as needed
                _logger.LogError($"Error fetching data: {ex.Message}");
                throw new ApplicationException("Failed to fetch aggregate bars.", ex);
            }
        }

        return rtn;
    }

    public async Task<List<TickerType>> GetTickerTypes(AssetClass assetClass, Locale locale)
    {
        var rtn = new List<TickerType>();

        string url = $"{_baseUrl}/v3/reference/tickers/types?asset_class={assetClass.ToString().ToLower()}&locale={locale.ToString().ToLower()}&apiKey={_apiKey}";

        try
        {
            JObject apiData = await GetData(url);

            var results = (JArray)apiData["results"];

            foreach (var result in results)
            {
                TickerType tickerType = JsonConvert.DeserializeObject<TickerType>(result.ToString());

                rtn.Add(tickerType);
            }

            // Get the next URL for pagination
            if (apiData["next_url"] is null)
            {
                url = null;
            }
            else
            {
                url = $"{apiData["next_url"]?.ToString()}&apiKey={_apiKey}";
            }

        }
        catch (Exception ex)
        {
            // Handle and log exceptions, then rethrow or return partial data as needed
            _logger.LogError($"Error fetching data: {ex.Message}");
            throw new ApplicationException("Failed to fetch Ticker Types.", ex);
        }

        return rtn;
    }

    #endregion

    private async Task<JObject> GetData(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        JObject apiData = JObject.Parse(responseBody);
        return apiData;
    }
}