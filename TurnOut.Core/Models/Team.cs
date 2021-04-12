using System.Collections.Generic;
using TurnOut.Core.Models.Entities;

namespace TurnOut.Core.Models
{
    public class Team
    {
        public Team()
        {
            Flag.Team = this;
        }

        public string TeamName { get; set; }

        public Team OpponentTeam { get; set; }

        public List<Player> Players { get; set; }

        public Flag Flag { get; set; } = new Flag();

        public List<(int x, int y)> VisiblePositions { get; set; }
    }
}
