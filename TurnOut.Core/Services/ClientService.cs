using System.Threading.Tasks;
using TurnOut.Core.Models;

namespace TurnOut.Core.Services
{
    public class ClientService
    {
        private readonly GameInstanceService _gameInstanceService;

        public ClientService(GameInstanceService gameInstanceService)
        {
            _gameInstanceService = gameInstanceService;
        }

        public Player Player { get; set; }

        public void TempAssignPlayer(Player player)
        {
            Player = player;
            Player.IsBoundToClient = true;
            _gameInstanceService.DispatchRenderUpdate();
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
