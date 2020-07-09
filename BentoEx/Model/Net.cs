using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace BentoEx.Model
{
    public class NetAccess
    {
        const string SiteUrl = "https://www.obentonet.jp/";
        CookieContainer cc;

        string companyCode;
        string userId;
        string password;

        public NetAccess()
        {
            cc = new CookieContainer();
        }

        public void SupplyLoginInfo(string company, string id, string pw)
        {
            companyCode = company;
            userId = id;
            password = pw;
        }

        public async Task<List<Bento>> GetBentoList(DateTime startDate)
        {
            try
            {
                if (cc.Count == 0)
                {
                    if (await Login() == false)
                        throw new Exception("Unable to login.");
                }

                // Retrieve Monday to Friday
                string urlString = String.Format("item_list.html?from={0}&to={1}", startDate.ToString("yyyy/MM/dd"), startDate.AddDays(4).ToString("yyyy/MM/dd"));
                urlString = urlString.Replace("/", "%2F"); // URL encoding
                urlString = SiteUrl + urlString;

                string html = await HttpGet(urlString, cc);
                return ParseHtml(html, startDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetBentoList failed");
                return new List<Bento>();
            }
        }

        List<Bento> ParseHtml(string html, DateTime startDate)
        {
            // HtmlDocumentオブジェクトを構築する
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var boxClasses =
                htmlDoc.DocumentNode
                .SelectNodes(@"//div[@class=""box""]")
                .Where(a => a.InnerHtml.Contains(@"class=""info"""))
                .Select(a => new
                {
                    html = a.InnerHtml
                });

            var bentoes = new List<Bento>();

            foreach (var b in boxClasses)
            {
                Match match = Regex.Match(b.html, @"配達日：(20[12][0-9]/[01][0-9]/[0-3][0-9])");
                var date = match.Groups[1].ToString();

                match = Regex.Match(b.html, @"alt=""(.+)""");
                var menu = match.Groups[1].ToString();

                match = Regex.Match(b.html, @"定価.*(\d{3}円)");
                var price = match.Groups[1].ToString();

                bentoes.Add(new Bento()
                {
                    BentoDate = DateTime.Parse(date),
                    BentoMenu = menu,
                    Type = b.html.Contains("ライス大盛") ? Bento.BentoType.ohmori :
                    b.html.Contains("おかずのみ") ? Bento.BentoType.okazu :
                    Bento.BentoType.normal,
                    Price = price,
                    ToBeOrdered = false,
                    OrderState = b.html.Contains(@"class=""ordered""") ? Bento.OrderStatus.ordered :
                    b.html.Contains(@"deadline_msg") ? Bento.OrderStatus.expired :
                    Bento.OrderStatus.blank
                });
            }     
            return bentoes;
        }

        Encoding encoder = Encoding.GetEncoding("utf-8");

        async Task<string> HttpGet(string url, CookieContainer cc)
        {
            string result = "";
            // リクエストの作成
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.CookieContainer = cc;
            req.UserAgent = "Mozilla";

            using (WebResponse res = await req.GetResponseAsync())
            {
                // レスポンスの読み取り
                using (Stream resStream = res.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(resStream, encoder);
                    result = sr.ReadToEnd();
                    sr.Close();
                }
            }

            return result;
        }

        async Task<string> HttpPost(string url, string content, CookieContainer cc)
        {
            string result = "";
            byte[] payload = Encoding.ASCII.GetBytes(content);

            // リクエストの作成
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = payload.Length;
            req.CookieContainer = cc;
            req.UserAgent = "Mozilla";
            req.Referer = "https://www.obentonet.jp/login.html";
            req.Headers.Add("Origin", "https://www.obentonet.jp");
            req.Accept = "*/*";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "ja,en-US;q=0.9,en;q=0.8");

            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // ポスト・データの書き込み
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(payload, 0, payload.Length);
            }

            using (WebResponse res = await req.GetResponseAsync())
            {
                // レスポンスの読み取り
                using (Stream resStream = res.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(resStream, encoder);
                    result = sr.ReadToEnd();
                    sr.Close();
                }
            }
            return result;
        }

        public async Task<bool> Login()
        {
            string url = "https://www.obentonet.jp/top_login.html";
            string postContent = String.Format("request=logon&redirectTo=https%3A%2F%2Fwww.obentonet.jp%2F&CORPORATION_CD={0}&jp.co.interfactory.framework.trim.CORPORATION_CD=&LOGINID={1}&PASSWORD={2}&x=0&y=0", companyCode, userId, password);

            string resHtml = await HttpPost(url, postContent, cc);

            return !(resHtml.Contains(@"パスワードが違います"));
        }
    }
}
