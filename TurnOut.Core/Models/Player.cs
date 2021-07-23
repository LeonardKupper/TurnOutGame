using System;
using System.Collections.Generic;
using TurnOut.Core.Models.Entities.Units;

namespace TurnOut.Core.Models
{
    public class Player
    {
        public Guid ConnectionIdentifier { get; set; }
        public GameSession Session { get; set; }
        public bool IsInSession => Session is not null;



        public Team Team { get; set; }
        public string Name { get; set; }
        public List<UnitBase> Units { get; set; }

        public bool IsBoundToClient { get; set; }

        public bool IsReadyInTurn { get; set; }
    }
}
