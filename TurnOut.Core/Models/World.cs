using TurnOut.Core.Models.Entities;

namespace TurnOut.Core.Models
{
    public class World
    {
        public World(int w, int h)
        {
            Board = new EntityBase[w, h];
            Animations = new string[w, h];
        }

        public EntityBase[,] Board { get; set; }
        public string[,] Animations { get; set; }
        public (int w, int h) BoardDimensions => (Board.GetLength(0), Board.GetLength(1));
    }
}
