using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turnout.Core.Utility;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;
using TurnOut.Core.Services;

namespace TurnOut.DbgVis.Shared
{
    public class GameVisualizerBase : ComponentBase, IDisposable
    {
        // Services initialized by DI:
        [Inject] protected ClientService _clientService { get; set; }
        [Inject] protected GameInstanceService _gameInstanceService { get; set; }

        // Initialized from instance service:
        protected TurnPlanningService _turnPlanningService { get; set; }


        protected override void OnInitialized()
        {
            // Get instance scoped services
            _turnPlanningService = _gameInstanceService.GetTurnPlanningService();

            // Subscribe this client instance for render updates
            _gameInstanceService.RenderUpdate += HandleRenderUpdate;
            base.OnInitialized();
        }

        // Render Update Handler
        private void HandleRenderUpdate(object sender, EventArgs e)
        {
            // Dispatch to UI render thread
            InvokeAsync(StateHasChanged);
        }


        public GameInstance Instance => _gameInstanceService.GameInstance;
        public Player Player => _clientService.Player;
        public Dictionary<UnitBase, List<IUnitMove>> TurnPlan => _turnPlanningService.GetTurnPlan();



        // Release resources
        void IDisposable.Dispose()
        {
            // Remove render update handler:
            _gameInstanceService.RenderUpdate -= HandleRenderUpdate;
        }


        // Visualization utilities:

        public string[] GetIconsForMove(IUnitMove m) => new string[] { "" };

        public string[] GetIconsForMove(MobilityExtension.StepForwardMove m) => new string[] { "fas fa-arrow-alt-circle-up" };
        public string[] GetIconsForMove(MobilityExtension.TurnLeftMove m) => new string[] { "fas fa-undo" };
        public string[] GetIconsForMove(MobilityExtension.TurnRightMove m) => new string[] { "fas fa-redo" };

        public string[] GetIconsForMove(FlagCarryingExtension.GrabFlagMove m) => new string[] { "far fa-hand-paper", "fas fa-flag" };
        public string[] GetIconsForMove(FlagCarryingExtension.DropFlagMove m) => new string[] { "fas fa-times", "fas fa-flag" };

        public string[] GetIconsForMove(BeamShootingExtension.ShootBeamMove m) => new string[] { "fas fa-crosshairs" };

        public string[] GetIconsForMove(DashAttackExtension.DashLeftMove m) => new string[] { "fas fa-chevron-left", "fas fa-dizzy" };
        public string[] GetIconsForMove(DashAttackExtension.DashRightMove m) => new string[] { "fas fa-dizzy", "fas fa-chevron-right" };
    
    
        public string GetTeamColor(Team team)
        {
            return (team == Instance.TeamAlpha) ? "red" : "blue";
        }

        public string GetCarriedFlagColor(UnitBase unit)
        {
            if (!unit.Has<FlagCarryingExtension>(out var flagCarrier)) return null;
            return !flagCarrier.IsCarryingFlag ? null : GetTeamColor(flagCarrier.CarriedFlag.Team);
        }
    }
}
