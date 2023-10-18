using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Net.Http;
using System.Web;
using System.Text.Json;

namespace cAlgo;

[Indicator(IsOverlay = true, AccessRights = AccessRights.FullAccess)]
public class USMARKETS : Indicator
{
    private HttpClient _httpClient;
    private const string BaseUrl = "https://quote.cnbc.com/quote-html-webservice/restQuote/symbolType/symbol";
    private const string DOW30Symbols = "AXP|AMGN|AAPL|BA|CAT|CSCO|CVX|GS|HD|HON|IBM|INTC|JNJ|KO|JPM|MCD|MMM|MRK|MSFT|NKE|PG|TRV|UNH|CRM|VZ|V|WBA|WMT|DIS|DOW";
    //private const string NASDAQ100Symbols = "CMCSA|COST|CSX|CTSH|DDOG|DXCM|FANG|DLTR|EA|EBAY|ENPH|ON|EXC|FAST|GFS|META|FI|FTNT|GILD|GOOG|GOOGL|HON|ILMN|INTC|INTU|ISRG|MRVL|IDXX|JD|KDP";

    private readonly TextBlock tb1 = new();
    private readonly TextBlock tb2 = new();
    private readonly TextBlock tb3 = new();

    [Parameter("Text Color", DefaultValue = "WhiteSmoke")]
    public Color TextColor { get; set; }

    [Parameter("Refresh (Second)", DefaultValue = 5)]
    public double RefreshRate { get; set; }

    protected override void OnDestroy()
    {
        _httpClient.Dispose();
    }
    
    protected override void Initialize()
    {
        tb1.Text = "Up Quotes: 0";
        tb1.Padding = "2.5";
        tb1.ForegroundColor = TextColor;
        tb2.Text = "Down Quotes: 0";
        tb2.Padding = "2.5";
        tb2.ForegroundColor = TextColor;
        tb3.Text = "Unchanged Quotes: 0";
        tb3.Padding = "2.5";
        tb3.ForegroundColor = TextColor;

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = "10"
        };
        stack.AddChild(tb1);
        stack.AddChild(tb2);
        stack.AddChild(tb3);

        var panel = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Child = stack
        };

        Chart.AddControl(panel);
        _httpClient=new();
        OnTimer();
        Timer.Start(TimeSpan.FromSeconds(RefreshRate));
    }

    protected async override void OnTimer()
    {
        try
        {        
            var response = await _httpClient.GetAsync($"{BaseUrl}?symbols={HttpUtility.UrlEncode(DOW30Symbols)}");
            if (response.IsSuccessStatusCode)
            {
                var upCounter = 0;
                var downCounter = 0;
                var unchangedCounter = 0;

                var stream = await response.Content.ReadAsStreamAsync();
                var quotes = await JsonSerializer.DeserializeAsync<Result>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                quotes.FormattedQuoteResult.FormattedQuote.ForEach(quote =>
                {
                    if (quote.Change.StartsWith("+"))
                    {
                        upCounter++;
                    }
                    else if (quote.Change.Contains("-"))
                    {
                        downCounter++;
                    }
                    else
                    {
                        unchangedCounter++;
                    }
                });
                BeginInvokeOnMainThread(() =>
                {
                    tb1.Text = $"Up Quotes: {upCounter}";
                    tb2.Text = $"Down Quotes: {downCounter}";
                    tb3.Text = $"Unchanged Quotes: {unchangedCounter}";
                });

            }
            else
            {
                Print(response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            Print(ex);
        }

    }

    public override void Calculate(int index)
    {
        // Calculate value at specified index
        // Result[index] = 
    }
}

public class FormattedQuote
{
    public string Symbol { get; set; }
    public string SymbolType { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string OnAirName { get; set; }
    public string AltName { get; set; }
    public string Last { get; set; }
    public string LastTimedate { get; set; }
    public DateTime LastTime { get; set; }
    public string Changetype { get; set; }
    public string Type { get; set; }
    public string SubType { get; set; }
    public string Exchange { get; set; }
    public string Source { get; set; }
    public string Open { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public string Change { get; set; }
    public string ChangePct { get; set; }
    public string CurrencyCode { get; set; }
    public string Volume { get; set; }
    public string VolumeAlt { get; set; }
    public string Provider { get; set; }
    public string PreviousDayClosing { get; set; }
    public string AltSymbol { get; set; }
    public string RealTime { get; set; }
    public string Curmktstatus { get; set; }
    public string Pe { get; set; }
    public string MktcapView { get; set; }
    public string Dividend { get; set; }
    public string Dividendyield { get; set; }
    public string Beta { get; set; }
    public string Tendayavgvol { get; set; }
    public string Pcttendayvol { get; set; }
    public string Yrhiprice { get; set; }
    public string Yrhidate { get; set; }
    public string Yrloprice { get; set; }
    public string Yrlodate { get; set; }
    public string Eps { get; set; }
    public string Sharesout { get; set; }
    public string Revenuettm { get; set; }
    public string Fpe { get; set; }
    public string Feps { get; set; }
    public string Psales { get; set; }
    public string Fsales { get; set; }
    public string Fpsales { get; set; }
    public string Streamable { get; set; }
    public string IssueId { get; set; }
    public string IssuerId { get; set; }
    public string CountryCode { get; set; }
    public string TimeZone { get; set; }
    public string FeedSymbol { get; set; }
    public string Portfolioindicator { get; set; }
    public string ROETTM { get; set; }
    public string NETPROFTTM { get; set; }
    public string GROSMGNTTM { get; set; }
    public string TTMEBITD { get; set; }
    public string DEBTEQTYQ { get; set; }
}

public class FormattedQuoteResult
{
    public List<FormattedQuote> FormattedQuote { get; set; }
}

public class Result
{
    public FormattedQuoteResult FormattedQuoteResult { get; set; }
}
