using Forex.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Forex.Services
{
    public static class DataService
    {
        private static readonly int CURRENCY_ID_USDOLLAR = 1316;
        private static readonly DateTime DATE_VAL_MAX = DateTime.Now.AddYears(1);
        private static readonly DateTime DATE_VAL_MIN = DateTime.Now.AddYears(-10);

        public static async Task<List<RateItem>> GetExchangeRateAsync(DateTime date, DateTime? since = null)
        {
            if (since != null && since.Value.Date != date.Date)
            {
                throw new ArgumentException();
            }

            List<RateItem> items = new List<RateItem>();

            bool completed = false;
            int page = 1;
            while (!completed)
            {
                var data = await GetExchangeRateAsync(CURRENCY_ID_USDOLLAR, date.Date, date.Date, page++);

                foreach (DataRow row in data.Data.Rows)
                {
                    var time = ValueConverter.Convert<DateTime>(row["发布时间"]);

                    if (time > DATE_VAL_MAX || time < DATE_VAL_MIN)
                    {
                        // ignore invalid row
                        continue;
                    }

                    if (since != null && time <= since.Value)
                    {
                        completed = true;
                        break;
                    }

                    items.Add(new RateItem
                    {
                        CurrencyId = CURRENCY_ID_USDOLLAR,
                        Rate = ValueConverter.Convert<double>(row["现汇买入价"]) / 100,
                        Time = time
                    });
                }

                if (page > data.PageNum)
                {
                    completed = true;
                }
            }

            return items;
        }

        private static async Task<Result> GetExchangeRateAsync(int currencyId, DateTime? fromDate, DateTime? toDate, int page)
        {
            var html = await SendRequestAsync("http://srh.bankofchina.com/search/whpj/search.jsp", HttpMethod.Post, "application/x-www-form-urlencoded", new Dictionary<string, string> {
                { "erectDate", fromDate?.ToString("yyyy-MM-dd") },
                { "nothing", toDate?.ToString("yyyy-MM-dd") },
                { "page", page.ToString()},
                { "pjname", currencyId.ToString() },
            });

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var htmlTable = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'BOC_main')]/table");

            var htmlRows = htmlTable.Elements("tr");

            DataTable table = new DataTable();
            var headerCells = htmlRows.First().Elements("th");
            foreach (var cell in headerCells)
            {
                table.Columns.Add(cell.InnerText);
            }

            for (int i = 1; i < htmlRows.Count(); i++)
            {
                var row = table.NewRow();

                var cells = htmlRows.ElementAt(i).Elements("td");
                if (cells.Count() != table.Columns.Count)
                {
                    // skip as columns don't match
                    break;
                }

                for (var j = 0; j < cells.Count(); j++)
                {
                    row[j] = cells.ElementAt(j).InnerText;
                }

                table.Rows.Add(row);
            }

            var matchCount = Regex.Match(html, @"var m_nRecordCount = (\d+);");
            if (!matchCount.Success)
            {
                throw new Exception("Failed to detect the record count from the source page");
            }
            var matchPageSize = Regex.Match(html, @"var m_nPageSize = (\d+);");
            if (!matchPageSize.Success)
            {
                throw new Exception("Failed to detect the page size from the source page");
            }

            return new Result
            {
                Data = table,
                PageNum = (int)Math.Ceiling(double.Parse(matchCount.Groups[1].Value) / int.Parse(matchPageSize.Groups[1].Value))
            };
        }

        private static async Task<string> SendRequestAsync(string endpoint, HttpMethod method, string contentType = null, object data = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var message = new HttpRequestMessage(method, endpoint);
                if (data != null)
                {
                    switch (contentType)
                    {
                        case "application/json":
                            var json = JsonConvert.SerializeObject(data);

                            message.Content = new StringContent(json, Encoding.UTF8, "application/json");
                            break;
                        case "application/x-www-form-urlencoded":
                            message.Content = new FormUrlEncodedContent(data as Dictionary<string, string>);
                            break;
                        default:
                            break;
                    }
                }

                using (var response = await client.SendAsync(message))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception(string.IsNullOrWhiteSpace(content) ? response.StatusCode.ToString() : content);
                    }

                    return content;
                }
            }
        }
    }

    public class Result
    {
        public DataTable Data { get; set; }
        public int PageNum { get; set; }
    }
}
