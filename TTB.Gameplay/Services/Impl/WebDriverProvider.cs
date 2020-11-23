using System;
using System.IO;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using TTB.Gameplay.Settings;

namespace TTB.Gameplay.Services.Impl
{
    public class WebDriverProvider : IWebDriverProvider, IDisposable
    {
        private readonly WebDriverSettings _webDriverSettings;
        private readonly FirefoxOptions _options;
        private IWebDriver _driver;

        public WebDriverProvider(IOptionsMonitor<WebDriverSettings> webDriverSettings)
        {
            this._webDriverSettings = webDriverSettings.CurrentValue;
            this._options = new FirefoxOptions();
            this.InitiateWebDriver();
        }

        public IWebDriver GetWebDriver()
        {
            this.InitiateWebDriver();
            return this._driver;
        }

        private void InitiateWebDriver()
        {
            try
            {
                if (this._driver == null || string.IsNullOrEmpty(this._driver.CurrentWindowHandle))
                {
                    AssignDriver();
                }
            }
            catch (WebDriverException)
            {
                AssignDriver();
            }
        }

        private void AssignDriver()
        {
            try
            {
                if (string.IsNullOrEmpty(this._webDriverSettings.RemoteDriverUrl) && string.IsNullOrEmpty(this._webDriverSettings.LocalDriverPath))
                    throw new Exception("The configuration for WebDriver is not provided");

                if (string.IsNullOrEmpty(this._webDriverSettings.RemoteDriverUrl))
                {
                    var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._webDriverSettings.LocalDriverPath));
                    // thanks for the workaround: https://github.com/SeleniumHQ/selenium/pull/5257
                    var service = FirefoxDriverService.CreateDefaultService(path);
                    service.Start();
                    this._driver = new RemoteWebDriver(new Uri($"http://127.0.0.1:{service.Port}"), new FirefoxOptions());
                }
                else
                {
                    this._driver = new RemoteWebDriver(new Uri(this._webDriverSettings.RemoteDriverUrl), this._options.ToCapabilities(), TimeSpan.FromMinutes(5));
                }
            }
            catch (Exception exc)
            {

            } 
        }

        public void Dispose()
        {
            if (this._driver != null)
            {
                _driver.Quit();
            }
        }
    }
}
