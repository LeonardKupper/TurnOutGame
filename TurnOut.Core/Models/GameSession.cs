using System.Collections.Generic;

namespace TurnOut.Core.Models
{
    /// <summary>
    /// Models a multiplayer game session
    /// </summary>
    public class GameSession
    {
        public string SessionCode { get; set; }
        public HashSet<Player> ConnectedPlayers { get; set; }
    }
}
