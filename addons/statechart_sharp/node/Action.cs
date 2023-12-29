using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Action.svg"), Tool]
    public partial class Action : StatechartComposition
    {
        #region define signals

        [Signal] public delegate void InvokeEventHandler(Action action);
        
        #endregion

        #region define properties

        [Export] protected TransitionEventNameEnum ActionEvent { get; set; } = TransitionEventNameEnum.PROCESS;
        [Export] protected StringName CustomEventName { get; set; }
        
        public StringName EventName { get; protected set; }

        public double Delta
        {
            get { return HostStatechart.Delta; }
        }
        public double PhysicsDelta
        {
            get { return HostStatechart.PhysicsDelta; }
        }
        public InputEvent GameInput
        {
            get { return HostStatechart.Input; }
        }
        public InputEvent GameUnhandledInput
        {
            get { return HostStatechart.UnhandledInput; }
        }

        #endregion

        internal override void Init(Statechart hostStatechart, ref int ancestorId)
        {
            base.Init(hostStatechart, ref ancestorId);
            #if DEBUG
            if (ActionEvent == TransitionEventNameEnum.CUSTOM && CustomEventName == null)
            {
                GD.PushError(Name, ": no event name for custom-event.");
            }
            #endif
            EventName = StatechartConfig.GetTransitionEventName(ActionEvent, CustomEventName);
        }

        internal void ActionInvoke(StringName eventName)
        {
            if (EventName == eventName)
            {
                EmitSignal(SignalName.Invoke, this);
            }
        }

    }
}