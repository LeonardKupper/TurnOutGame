using System;
using System.Threading.Tasks;
using TurnOut.Core.Models;

namespace TurnOut.Core.Services
{
    public class ClientService
    {
        private readonly GameSessionService _gameSessionService;
        private readonly GameInstanceService _gameInstanceService;

        public ClientService(GameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;

            // Initialize player
            Player = new Player();
            Player.ConnectionIdentifier = new Guid();
            Player.IsBoundToClient = true;
        }

        public GameScreen CurrentScreen { get; private set; }

        public Player Player { get; init; }

        public void SwitchToScreen(GameScreen screen)
        {
            CurrentScreen = screen;
        }


        public void SetPlayerName(string name)
        {
            Player.Name = name;
        }

        public void CreateAndJoinSession()
        {
            var newSession = _gameSessionService.CreateNewSession();
            _gameSessionService.AddPlayerToSession(newSession, Player);
        }

        public bool JoinSession(string sessionCode)
        {
            var session = _gameSessionService.FindSessionByCode(sessionCode);
            if (session is null)
            {
                return false;
            }
            _gameSessionService.AddPlayerToSession(session, Player);
            return true;
        }


        public void SignalTurnIsReady()
        {
            Player.IsReadyInTurn = true;
            _gameInstanceService.CheckPlanningState();
        }

        public void RevokeReady()
        {
            Player.IsReadyInTurn = false;
        }

    }
}
