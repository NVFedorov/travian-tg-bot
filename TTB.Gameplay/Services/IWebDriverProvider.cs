using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace TTB.Gameplay.Services
{
    public interface IWebDriverProvider
    {
        IWebDriver GetWebDriver();
    }
}
