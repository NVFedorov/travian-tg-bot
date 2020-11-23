using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class FindNatarsScenario : BaseScenario
    {
        public FindNatarsScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            this._driver.FindElement(By.CssSelector("#n3 a")).Click();
            var mapContainer = this._driver.FindElement(By.Id("mapContainer"));

            var action = new Actions(this._driver);
            int w = mapContainer.Size.Width / 9;
            int h = mapContainer.Size.Height / 7;
            int i = 0;
            int j = 1;
            action.MoveToElement(mapContainer)
                .MoveByOffset(-5 * w + w / 2, -4 * h + h / 2)
                .MoveByOffset(i * w + w / 2, j * h + h / 2)
                .Click()
                .Build()
                .Perform();

            return result;
        }
    }
}
