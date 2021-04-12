namespace TurnOut.Core.Models
{
    public class FieldOfView
    {
        public (float x, float y) Position { get; set; }
        public (float dx, float dy) FacingDirection { get; set; }
        public double FovAngle { get; set; }
        public float MaxViewDistance { get; set; }
    }
}