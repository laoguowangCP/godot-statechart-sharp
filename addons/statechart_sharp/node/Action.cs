using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Action.svg")]
    public partial class Action : StatechartComposition
    {
        #region define signals

        [Signal] public delegate void ActionEventHandler(Transition t);
        
        #endregion

        #region define properties

        [Export] protected EventNameEnum TransitionEvent { get; set; } = EventNameEnum.PROCESS;
        [Export] protected StringName CustomEventName { get; set; }
        
        public StringName EventName { get; protected set; }

        #endregion

        public void Init(State sourceState)
        {
            HostStatechart = sourceState.HostStatechart;
            EventName = GetEventName(TransitionEvent, CustomEventName);
        }
    }
}