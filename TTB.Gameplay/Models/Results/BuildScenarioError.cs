using System;
using TTB.Gameplay.Models.ContextModels;

namespace TTB.Gameplay.Models.Results
{
    public enum BuildErrorType
    {
        NoSpaceInQueue,
        NotEnoughResources,
        NotEnoughCropProduction,
        BuildingIsUnderConstruction
    }

    public class BuildScenarioError : ScenarioError
    {
        public BuildErrorType BuildErrorType { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public Resources ResourcesNeeded { get; set; }
        public Village Village { get; set; }
    }
}
