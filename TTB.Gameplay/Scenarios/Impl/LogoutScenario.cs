using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class LogoutScenario : BaseScenario
    {
        public LogoutScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            _driver.FindElement(By.CssSelector("li.logout > a")).Click();
            return new BaseScenarioResult();
        }
    }
}
