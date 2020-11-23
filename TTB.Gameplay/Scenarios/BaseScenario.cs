using System;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TTB.Gameplay.Extensions;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios
{
    public abstract class BaseScenario : IScenario
    {
        protected readonly IWebDriver _driver;
        protected BaseScenario scenario;

        public BaseScenario(IWebDriverProvider driverProvider)
        {
            _driver = driverProvider.GetWebDriver();
        }

        public BaseScenarioResult Execute(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            _driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/dorf1.php"));
            _driver.Manage().Window.Maximize();
            if (_driver.Url.Contains("login"))
            {
                var loginResult = Login(context);
                if (loginResult != null)
                {
                    return loginResult;
                }
            }
            else
            {
                var playerNameElem = _driver.FindElement(By.ClassName("playerName"));
                var a = playerNameElem.FindElements(By.XPath($"//a[contains(text(), '{context.Player.UserName}')]"));
                if (a == null || !a.Any())
                {
                    var loginResult = Login(context);
                    if (loginResult != null)
                    {
                        return loginResult;
                    }
                }
            }

            var dismissCookieNoticeElems = _driver.FindElements(By.Id("dismissCookieNotice"));
            if (dismissCookieNoticeElems.Any())
            {
                dismissCookieNoticeElems.First().Click();
            }

            result = Proceed(context);
            result.Success = result.Errors.Count == 0;
            result.Cookies = _driver.Manage().Cookies.AllCookies.ToList();

            return result;
        }

        public void Decorate(IScenario scenario)
        {
            this.scenario = scenario as BaseScenario;
        }

        private BaseScenarioResult Proceed(ScenarioContext context)
        {
            var result = ExecuteScenario(context);
            if (scenario != null)
            {
                var decoratieResult = scenario.Proceed(context);
                result.Merge(decoratieResult);
            }

            return result;
        }

        protected abstract BaseScenarioResult ExecuteScenario(ScenarioContext context);

        protected static string Decode(string input)
        {
            var strBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                int uc = input[i];
                if (uc > 256)
                {
                    strBuilder.Append("\\u").Append(uc.ToString("X4"));
                }
                else
                {
                    if (uc == '\n')
                    {
                        strBuilder.Append("\\n");
                    }
                    else
                    {
                        strBuilder.Append(input[i]);
                    }
                }
            }

            var result = strBuilder.ToString().ToLower();
            result = result.Replace("(", "").Replace(")", "").Replace("\\u202d", "").Replace("\\u202c", "").Replace("\\u2212", "-");
            return result;
        }

        protected Village ParseVillageDetailsFromUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
            var villageUrl = _driver.Url;
            var wrapper = _driver.FindElement(By.Id("map_details"));
            var players = wrapper.FindElements(By.CssSelector("table#village_info td.player a"));
            var player = players.Any() ? players.First().Text : "Unknown player";

            var alliances = wrapper.FindElements(By.CssSelector("table#village_info td.alliance a"));
            var alliance = alliances.Any() ? alliances.First().Text : "Unknown player";
            var header = _driver.FindElement(By.CssSelector("div#tileDetails h1.titleInHeader"));
            var village = header.Text;

            // TODO: parse coordinates, check if this is capital
            var result = new Village
            {
                Alliance = alliance,
                Name = village,
                PlayerName = player,
                Url = villageUrl
            };

            var xText = header.FindElement(By.ClassName("coordinateX")).Text;
            var yText = header.FindElement(By.ClassName("coordinateY")).Text;
            var successX = int.TryParse(Decode(xText), out int x);
            var successY = int.TryParse(Decode(yText), out int y);
            if (successX && successY)
            {
                result.CoordinateX = x;
                result.CoordinateY = y;
            }
            else
            {
                result.CoordinateX = int.MinValue;
                result.CoordinateY = int.MinValue;
            }

            return result;
        }

        protected static Resources GetResourcesOnWarehouse(IWebDriver driver)
        {
            var lumber = driver.FindElement(By.Id("l1")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var clay = driver.FindElement(By.Id("l2")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var iron = driver.FindElement(By.Id("l3")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var crop = driver.FindElement(By.Id("l4")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();

            var result = new Resources
            {
                Lumber = int.Parse(lumber),
                Clay = int.Parse(clay),
                Iron = int.Parse(iron),
                Crop = int.Parse(crop)
            };

            return result;
        }

        protected static IWebElement WaitUntilElementExists(IWebDriver driver, By elementLocator, int timeout = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(ExpectedConditions.ElementExists(elementLocator));
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Element with locator: '" + elementLocator + "' was not found in current context page.");
                throw;
            }
        }

        private BaseScenarioResult Login(ScenarioContext context)
        {
            if (context.Cookies != null && context.Cookies.Any())
            {
                foreach (var cookie in context.Cookies)
                {
                    _driver.Manage().Cookies.AddCookie(cookie);
                }
            }

            _driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/dorf1.php"));
            if (_driver.Url.Contains("login"))
            {
                var login = _driver.FindElement(By.XPath("//input[@name='name']"));
                var password = _driver.FindElement(By.XPath("//input[@name='password']"));

                login.SendKeys(context.Player.UserName);
                password.SendKeys(context.Player.Password);
                _driver.FindElement(By.Id("s1"))?.Click();
                if (_driver.Url.Contains("login"))
                {
                    var result = new BaseScenarioResult();
                    result.Errors.Add(new ScenarioError
                    {
                        ErrorMessage = "Unable to login. Try to update the cookies.",
                        ErrorSource = nameof(BaseScenario),
                        ErrorType = "Critical"
                    });

                    return result;
                }
            }

            return null;
        }
    }
}
