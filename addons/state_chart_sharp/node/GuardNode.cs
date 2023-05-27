using Godot;


namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class GuardNode : Node
    {
        [Signal] public delegate void GuardCheckEventHandler(GuardNode guard);
        
        private bool _isPermitted;
        public bool IsPermitted
        {
            get { return _isPermitted; }
            set { _isPermitted = value; }
        }
        public bool Check()
        {
            _isPermitted = false;
            EmitSignal(SignalName.GuardCheck, this);
            // GD.Print(_isPermitted);
            return IsPermitted;
        }
    }
}