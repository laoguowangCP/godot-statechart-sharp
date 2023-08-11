using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class LazyState : IStateChartComponent
    {
        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();

        public virtual void SubstateTransit() {}

        public virtual bool IsInstant() { return false; }

        public virtual void StateEnter()
        {
            EmitSignal(SignalName.Enter);
        }
        public virtual void StateExit()
        {
            EmitSignal(SignalName.Exit);
        }
    }

}