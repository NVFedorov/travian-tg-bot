using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class SendResourcesScenario : VillageScenario<SendResourcesAction>
    {
        public SendResourcesScenario(IWebDriverProvider driverProvider, SendResourcesAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri + "/dorf2.php"));
            // open market (g17)
            this._driver.FindElement(By.CssSelector("#village_map .g17 .level")).Click();
            var send = this._driver.Url + "&t=5";
            this._driver.Navigate().GoToUrl(send);

            int tradersAvailable = int.Parse(Decode(this._driver.FindElement(By.CssSelector(".merchantsAvailable")).Text));
            int maxCarryingCapacity = int.Parse(this._driver.FindElement(By.Id("addRessourcesLink1")).Text);
            //int toSend = tradersAvailable * maxCarryingCapacity / 4;

            //var inputs = this._driver.FindElements(By.CssSelector("#send_select .val input"));

            //// TODO: distinguish the input fields and send resources from action property
            //foreach (var input in inputs)
            //{
            //    input.SendKeys(toSend.ToString());
            //}

            _driver.FindElement(By.Id("r1")).SendKeys(_action.Resources.Lumber.ToString());
            _driver.FindElement(By.Id("r2")).SendKeys(_action.Resources.Clay.ToString());
            _driver.FindElement(By.Id("r3")).SendKeys(_action.Resources.Iron.ToString());
            _driver.FindElement(By.Id("r4")).SendKeys(_action.Resources.Crop.ToString());

            this._driver.FindElement(By.Id("xCoordInput")).SendKeys(_action.To.CoordinateX.ToString());
            this._driver.FindElement(By.Id("yCoordInput")).SendKeys(_action.To.CoordinateY.ToString());

            // prepare
            this._driver.FindElement(By.Id("enabledButton")).Click();

            // TODO: add error if can't send

            // send
            this._driver.FindElement(By.Id("enabledButton")).Click();
            result.Messages.Add($"Resources were sent from {_action.Village.Name} to {_action.To.Name}");

            return result;
        }
    }
}
