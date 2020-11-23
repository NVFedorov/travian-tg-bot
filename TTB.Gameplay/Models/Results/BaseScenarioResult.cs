using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using TTB.Gameplay.Models.ContextModels;

namespace TTB.Gameplay.Models.Results
{
    public class BaseScenarioResult
    {
        public BaseScenarioResult()
        {
            this.Messages = new List<string>();
            this.Errors = new List<ScenarioError>();
            this.Scans = new List<Incoming>();
            this.Villages = new List<Village>();
        }

        public Player Player { get; set; }
        public bool Success { get; set; }
        public bool IsUserUnderAttack { get; set; }
        public List<string> Messages { get; set; }
        public List<ScenarioError> Errors { get; set; }
        public List<Incoming> Scans { get; set; }
        public List<Village> Villages { get; set; }
        public List<Cookie> Cookies { get; set; }

        public void Merge (BaseScenarioResult obj)
        {
            this.Success &= obj.Success;
            this.IsUserUnderAttack = this.IsUserUnderAttack || obj.IsUserUnderAttack;
            this.Messages.AddRange(obj.Messages);
            this.Errors.AddRange(obj.Errors);
            this.Scans.AddRange(obj.Scans);
            foreach(var ov in obj.Villages)
            {
                var i = this.Villages.FindIndex(x => x.CoordinateX == ov.CoordinateX && x.CoordinateY == ov.CoordinateY);
                if (i > -1)
                {
                    this.Villages[i] = ov;
                }
                else
                {
                    this.Villages.Add(ov);
                }
            }
        }
    }
}
