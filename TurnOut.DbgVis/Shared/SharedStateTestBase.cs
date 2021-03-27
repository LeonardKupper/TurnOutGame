using Microsoft.AspNetCore.Components;
using System;
using TurnOut.DbgVis.Data;

namespace TurnOut.DbgVis.Shared
{
    public class SharedStateTestBase : ComponentBase, IDisposable
    {

        [Inject]
        protected SharedStateService _sharedStateService { get; set; }


        // Initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // bind state change handler
            _sharedStateService.StateChanged += HandleStateChanged;
        }

        // State change handler
        private void HandleStateChanged(object sender, EventArgs e)
        {
            // dispatch to UI render thread
            InvokeAsync(StateHasChanged);
        }

        // Release resources
        void IDisposable.Dispose()
        {
            // Remove handler:
            _sharedStateService.StateChanged -= HandleStateChanged;
        }


        protected string NewMessage { get; set; }

        protected void SendMessage()
        {
            _sharedStateService.AddMessage(NewMessage);
            NewMessage = "";
        }
    }
}
