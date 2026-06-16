using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Interfaces;

public class KiteInstrumentLoader : IKiteInstrumentLoader
{
    // Target dictionary: tradingsymbol → instrument_token
    private Dictionary<string, int> symbolToTokenDictionary = new Dictionary<string, int>();
    private readonly HttpClient _httpClient;
    private readonly ILogger<KiteInstrumentLoader> _logger;

    public KiteInstrumentLoader(IHttpClientFactory httpClientFactory, ILogger<KiteInstrumentLoader> logger)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.ZerodhaInstrumentsClient);
        _logger = logger;
        // Load the instruments on startup
        LoadEquityInstrumentsAsync().GetAwaiter().GetResult();
    }

    /// Gets the instrument token for a given stock symbol.
    /// Returns -1 if the symbol is not found.
    public int GetTokenCorrespondingToStockSymbol(string stockSymbol)
    {
        if (symbolToTokenDictionary.TryGetValue(stockSymbol, out int token))
        {
            return token;
        }
        else
        {
            _logger.LogWarning("Stock symbol {stockSymbol} not found in symbolToTokenDictionary.", stockSymbol);
            return -1; // or throw an exception, depending on your error handling strategy
        }
    }

    /// <summary>
    /// Fetches the Kite instruments CSV, filters for equity/cash instruments
    /// (those with an empty expiry field), and populates symbolToTokenDictionary.
    /// </summary>
    private async Task LoadEquityInstrumentsAsync()
    {
        try
        {
            // 1. Fetch the CSV
            HttpResponseMessage response = await _httpClient.GetAsync(Constants.InstrumentsEndpoint);
            response.EnsureSuccessStatusCode();

            string csvContent = await response.Content.ReadAsStringAsync();

            // 2. Parse line by line
            string[] lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Column indices (0-based):
            // instrument_token, exchange_token, tradingsymbol, name, last_price, expiry, ...
            const int colInstrumentToken = 0;
            const int colTradingSymbol = 2;
            const int colExpiry = 5;

            bool isFirstLine = true;

            foreach (string rawLine in lines)
            {
                // Skip the header row
                if (isFirstLine) { isFirstLine = false; continue; }

                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // 3. Parse the CSV line, respecting quoted fields
                string[] fields = ParseCsvLine(line);

                if (fields.Length <= colExpiry) continue;

                string expiry = fields[colExpiry].Trim();

                // 4. Filter: keep only rows where expiry is EMPTY (equity/cash instruments)
                //    Futures/options have a date like "2026-05-27"; equities have an empty string
                if (!string.IsNullOrEmpty(expiry)) continue;

                string tradingSymbol = fields[colTradingSymbol].Trim();

                if (!int.TryParse(fields[colInstrumentToken].Trim(), out int instrumentToken))
                    continue;

                if (string.IsNullOrEmpty(tradingSymbol)) continue;

                // 5. Populate the dictionary — TryAdd skips duplicates gracefully
                symbolToTokenDictionary.TryAdd(tradingSymbol, instrumentToken);
            }

            _logger.LogInformation("Loaded {Count} equity instruments from Kite API.", symbolToTokenDictionary.Count);
        }
        catch (System.Exception)
        {
            _logger.LogError("Failed to load instruments from Kite API.");
            throw;
        }
    }

    /// <summary>
    /// Parses a single CSV line into fields, handling double-quoted values.
    /// e.g. 256265,1001,INFY,"Infosys Limited",0,,0,0.05,1,EQ,NSE,NSE
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        int i = 0, len = line.Length;

        while (i <= len)
        {
            if (i == len) { fields.Add(string.Empty); break; }

            if (line[i] == '"')
            {
                i++; // skip opening quote
                int start = i;
                while (i < len && line[i] != '"') i++;
                fields.Add(line.Substring(start, i - start));
                i++; // skip closing quote
                if (i < len && line[i] == ',') i++;
            }
            else
            {
                int start = i;
                while (i < len && line[i] != ',') i++;
                fields.Add(line.Substring(start, i - start));
                if (i < len && line[i] == ',') i++;
            }
        }

        return fields.ToArray();
    }
}