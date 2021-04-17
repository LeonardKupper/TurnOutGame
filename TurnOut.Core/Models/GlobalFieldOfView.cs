namespace TurnOut.Core.Models
{
    public abstract class FieldOfViewBase
    {
        public double ViewAngle { get; set; }
        public float ViewDepth { get; set; }
    }

    public class GlobalFieldOfView : FieldOfViewBase
    {
        public (float x, float y) Position { get; set; }
        public double DirectionAngle { get; set; }
    }

    public class RelativeFieldOfView : FieldOfViewBase
    {
        public (float front, float right) PostionOffset { get; set; }
        public double AngleOffset { get; set; }
    }
}