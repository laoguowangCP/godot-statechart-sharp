using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Action.svg")]
    public partial class Action : StatechartComposition
    {
        #region define signals

        [Signal] public delegate void ActionEventHandler(Transition t);
        
        #endregion

        #region define properties

        [Export] protected TransitionEventNameEnum ActionEvent { get; set; } = TransitionEventNameEnum.PROCESS;
        [Export] protected StringName CustomEventName { get; set; }
        
        public StringName EventName { get; protected set; }

        #endregion

        public override void Init(Statechart hostStatechart, ref int ancestorId)
        {
            base.Init(hostStatechart, ref ancestorId);
            if (ActionEvent == TransitionEventNameEnum.CUSTOM && CustomEventName == null)
            {
                #if DEBUG
                GD.PushError(Name, ": no event name for custom-event.");
                #endif
            }
            EventName = StatechartConfig.GetTransitionEventName(ActionEvent, CustomEventName);
        }
    }
}