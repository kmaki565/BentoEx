using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace BentoEx.Model
{
    class BrowserAutomation
    {
        IWebDriver webDriver;

        readonly string companyCode;
        readonly string userId;
        readonly string password;

        /// <summary>
        /// Initializes browser automation. 
        /// Throws exception if WebDriver failed to initialize (e.g., version mismatched to the installed browser).
        /// </summary>
        /// <param name="company"></param>
        /// <param name="id"></param>
        /// <param name="pw"></param>
        public BrowserAutomation(string company, string id, string pw)
        {
            // Use Chromium Edge, or Chrome.
            if (EnvCheck.IsMsEdgeInstalled())
            {
                string webDriverPath = new DriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.MatchingBrowser);
                webDriver = new EdgeDriver(Path.GetDirectoryName(webDriverPath));
            }
            else if (EnvCheck.IsChromeInstalled())
            {
                string webDriverPath = new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
                webDriver = new ChromeDriver(Path.GetDirectoryName(webDriverPath));
            }
            else
            {
                throw new InvalidOperationException("Chromium Edge or Chrome browser is required for UI automation.");
            }

            companyCode = company;
            userId = id;
            password = pw;
        }

        public async Task OrderBentoes(IEnumerable<Bento> bentoes)
        {
            await Task.Run(() =>
            {
                LoginObento();
                foreach (var bt in bentoes)
                {
                    GetBentoForTheDay(bt);
                    AddBentoToCart(bt);
                    CheckInput(bt);
                    FixOrder();
                    Confirm();
                }
                webDriver.Quit();
            });
        }

        void LoginObento()
        {
            webDriver.Url = "https://www.obentonet.jp/login.html";
            IWebElement companyCodeBox = webDriver.FindElement(By.Id("CORPORATION_CD"));
            IWebElement loginIdBox = webDriver.FindElement(By.Name("LOGINID"));
            IWebElement passwdBox = webDriver.FindElement(By.Name("PASSWORD"));
            //IWebElement loginBtn = theWebDriver.FindElement(By.CssSelector(".buttonarea input"));

            companyCodeBox.SendKeys(companyCode);
            loginIdBox.SendKeys(userId);
            passwdBox.SendKeys(password);
            passwdBox.SendKeys(Keys.Enter);
            //loginBtn.Click();
        }
        void GetBentoForTheDay(Bento bt)
        {
            string dateString = bt.BentoDate.ToString("yyyy/MM/dd");
            dateString = dateString.Replace("/", "%2F"); // URL encoding
            webDriver.Url = String.Format("https://www.obentonet.jp/item_list.html?from={0}&to={0}", dateString);
        }
        /// <summary>
        /// Finds and clicks the order button. This assumes each order option has a different price.
        /// </summary>
        void AddBentoToCart(Bento bento)
        {
            var buttons = webDriver.FindElements(By.XPath($@"//input[@class='item_price' and @value='{bento.Price}']/following-sibling::img[@alt='買い物かごに入れる']"));
            if (buttons.Count() != 1)
                throw new Exception("Cannot determine which button to submit the order.");

            IWebElement cartBtn = buttons.First();
                
            if (cartBtn.GetAttribute("src").Contains("button_cart_ordered"))
            {
                throw new Exception("The specified item seems to be ordered already.");
            }
            cartBtn.Click();
        }
        void CheckInput(Bento bento)
        {
            int i = 0;
            while (webDriver.Url != "https://www.obentonet.jp/cart_seisan.html")
            {
                var task = Task.Delay(500);
                task.Wait();
                ++i;
                if (i == 10)
                    throw new Exception("Timeout occurred for cart_seisan");
            }

            if (!webDriver.PageSource.Contains(bento.PriceStr))
                throw new Exception("Bento type might be mismatched.");

            IWebElement btn = webDriver.FindElement(By.XPath("//input[@alt='入力内容を確認する']"));
            btn.Click();
        }
        void FixOrder()
        {
            IWebElement btn = webDriver.FindElement(By.XPath("//img[@alt='注文する']"));
            btn.Click();
        }
        void Confirm()
        {
            int i = 0;
            while (webDriver.Url != "https://www.obentonet.jp/cart_complete.html")
            {
                var task = Task.Delay(500);
                task.Wait();
                ++i;
                if (i == 20)
                    throw new Exception("Timeout occurred for confirm");
            }

            if (!webDriver.PageSource.Contains("ご注文完了"))
                throw new Exception("Order placement could not be confirmed");
        }
    }

    static class EnvCheck
    {
        public static bool IsChromeInstalled()
        {
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe");
            return regkey != null;
        }
        /// <summary>
        /// Checks if Chromium version of Edge browser is installed.
        /// </summary>
        /// <returns></returns>
        public static bool IsMsEdgeInstalled()
        {
            return File.Exists(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe");
        }

        public static bool IsVpnConnected()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface Interface in interfaces)
                {
                    if (Interface.OperationalStatus == OperationalStatus.Up && (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                    {
                        if (Interface.Description.Contains(@"Cisco AnyConnect"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
