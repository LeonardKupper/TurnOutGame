using System.Collections.Generic;

namespace TurnOut.Core.Models.EntityExtensions
{
    public class VisionExtension : IEntityExtension
    {
        public List<RelativeFieldOfView> RelativeFOVs { get; set; } = new();
    }

}