using CryptoIndexSeries.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Route("api/latestTrades")]
public class TradesController : ControllerBase
{

    [HttpGet("~/api/exchangeList")]
    public IActionResult GetExchangeList()
    {
        var values = new List<Exchange>{
            new Exchange { name = "Binance" },
            new Exchange { name = "Coinbase" },
            new Exchange { name = "Huobi" }
        };
        var json = JsonConvert.SerializeObject(values);
        return Ok(json);
    }


    [HttpGet("~/api/symbolList/{exchange}")]
    public async Task<IActionResult> GetSymbolList(string exchange)
    {
        var dataJSON = await System.IO.File.ReadAllTextAsync("./symbolList.json");
        var dataObjects = JsonConvert.DeserializeObject<List<Data>>(dataJSON);
        var exchangeNameLower = exchange.ToLower();

        if (exchangeNameLower == "binance")
        {
            var binanceSymbolList = JsonConvert.SerializeObject(dataObjects?[0].SymbolData);
            return Ok(binanceSymbolList);
        }
        else if (exchangeNameLower == "coinbase")
        {
            var coinbaseSymbolList = JsonConvert.SerializeObject(dataObjects?[1].SymbolData);
            return Ok(coinbaseSymbolList);
        }
        else if (exchangeNameLower == "huobi")
        {
            var huobiSymbolList = JsonConvert.SerializeObject(dataObjects?[2].SymbolData);
            return Ok(huobiSymbolList);
        }
        else
        {
            return BadRequest();
        }

    }

    [HttpGet("~/api/{exchange}/{symbol}")]
    public async Task<ActionResult<IEnumerable<Trade>>> GetTrades(string exchange, string symbol)
    {
        var assetSymbol = symbol.Split("-");
        var httpClient = new HttpClient();

        if (exchange.ToLower() == "binance")
        {
            var url = $"https://api.binance.com/api/v3/trades?symbol={assetSymbol[0] + assetSymbol[1]}";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var trades = JsonConvert.DeserializeObject<List<BinanceTrade>>(result);
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

                var tradeList = trades?.TakeLast(100).Reverse().Select(t => new Trade
                {
                    AssetSymbol = assetSymbol[0],
                    AssetQuantity =t.qty.ToString("0.#########"),
                    QuoteAssetSymbol = assetSymbol[1],
                    QuoteAssetQuantity = t.quoteQty.ToString("0.#########"),
                    Date = dtDateTime.AddMilliseconds(t.time).ToLocalTime().ToString()
                });
                return Ok(tradeList);
            }
        }

        else if (exchange.ToLower() == "coinbase")
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "curl/7.58.0");
            var url = $"https://api.exchange.coinbase.com/products/{symbol}/trades?limit=100";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var trades = JsonConvert.DeserializeObject<List<CoinbaseTrade>>(result);
                var tradeList = trades?.Select(t => new Trade
                {
                    AssetSymbol = assetSymbol[0],
                    AssetQuantity = t.size.ToString("0.#########"),
                    QuoteAssetSymbol = assetSymbol[1],
                    QuoteAssetQuantity = (t.price * t.size).ToString("0.#########"),
                    Date = t.time.ToLocalTime().ToString(),
                });
                return Ok(tradeList);
            }
        }

        else if (exchange.ToLower() == "huobi")
        {
            var url = $"https://api.huobi.pro/market/history/trade?symbol={assetSymbol[0].ToLower() + assetSymbol[1].ToLower()}&size=100";
            var response = await httpClient.GetAsync(url);
            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var result1 = JsonConvert.DeserializeObject<dynamic>(result);
                System.DateTime dtDateTime1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                var tradeList = new List<Trade>();
                if(result1 != null){
                    foreach (var innerData in result1.data)
                    {
                        var lastData = innerData.data[0];
                        var amount = lastData.amount;
                        var price = lastData.price;
                        var time = lastData.ts;
                        DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds((Convert.ChangeType(time, typeof(long))) / 1000);
                        tradeList.Add(new Trade
                        {
                            AssetSymbol = assetSymbol[0],
                            AssetQuantity = amount.ToString("0.#########"),
                            QuoteAssetSymbol = assetSymbol[1],
                            QuoteAssetQuantity = (amount * price).ToString("0.#########"),
                            Date = dateTimeOffSet.LocalDateTime.ToString(),
                        });
                    }
                    return Ok(tradeList);
                }
            }
        }
        return BadRequest();
    }
}