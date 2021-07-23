using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TurnOut.Core.Models.Entities.Units;

namespace TurnOut.Core.Models
{
    /// <summary>
    /// Memory model encapsulating a single multiplayer game instance.
    /// </summary>
    public class GameInstance
    {
        public World GameWorld { get; set; }
        public Team TeamAlpha { get; set; }
        public Team TeamOmega { get; set; }

        public Team WinningTeam { get; set; }
        public bool GameHasEnded => (WinningTeam is not null);

        public bool IsInPlanningPhase { get; set; }
        public CountdownState TurnPlanningCountdown { get; set; }

        public UnitBase CurrentlyExecutingUnit { get; set; }


        public IEnumerable<Player> AllPlayers => TeamAlpha.Players.Union(TeamOmega.Players);

    }
}
