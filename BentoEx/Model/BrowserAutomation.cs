using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BentoEx.Model
{
    class BrowserAutomation
    {
        IWebDriver webDriver;

        readonly string companyCode;
        readonly string userId;
        readonly string password;

        public BrowserAutomation(string company, string id, string pw)
        {
            // I use Chrome
            webDriver = new ChromeDriver();

            companyCode = company;
            userId = id;
            password = pw;
        }

        public async Task OrderBentoes(List<DateTime> dates)
        {
            await Task.Run(() =>
            {
                LoginObento();
                foreach (var dt in dates)
                {
                    GetBentoForTheDay(dt);
                    AddBentoToCart();
                    CheckInput();
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
        void GetBentoForTheDay(DateTime dt)
        {
            string dateString = dt.ToString("yyyy/MM/dd");
            dateString = dateString.Replace("/", "%2F"); // URL encoding
            webDriver.Url = String.Format("https://www.obentonet.jp/item_list.html?from={0}&to={0}", dateString);
        }
        /// <summary>
        /// WARNING! This method always adds the top item to the shopping cart assuming the first one is a full-bento.
        /// </summary>
        void AddBentoToCart()
        {
            IWebElement cartBtn = webDriver.FindElement(By.XPath("//img[@alt='買い物かごに入れる']"));
            if (cartBtn.GetAttribute("src").Contains("button_cart_ordered"))
            {
                throw new Exception("The specified item seems to be ordered already.");
            }
            cartBtn.Click();
        }
        void CheckInput()
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

            if (webDriver.PageSource.Contains("おかずのみ"))
                throw new Exception("おかずのみが注文されようとしています");

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

    class BrowserCheck
    {
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";

        static public bool IsChromeInstalled()
        {
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(keyPath, false);

            return (regkey != null);
        }
    }
}
