using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LGWCP.GodotPlugin.StatechartSharp
{
    public enum EventNameEnum : int
    {
        PRE_PROCESS,
        PRE_PHYSICS_PROCESS,
        PRE_INPUT,
        PRE_UNHANDLED_INPUT,
        PROCESS,
        PHYSICS_PROCESS,
        INPUT,
        UNHANDLED_INPUT,
        AUTO,
        CUSTOM
    }

    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg")]
    public partial class Transition : StatechartComposition
    {
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>

        #region define signals

        [Signal] public delegate void GuardEventHandler(Transition t);
        [Signal] public delegate void ActionEventHandler(Transition t);
        
        #endregion

        #region define properties

        // If transitEvent is null, transition is auto (checked on state enter)
        [Export] protected EventNameEnum TransitionEvent { get; set; } = EventNameEnum.PRE_PROCESS;
        [Export] protected StringName CustomEventName { get; set; }
        [Export] protected Array<State> TargetStatesArray;
        public StringName EventName { get; protected set; }
        protected List<State> TargetStates { get; set; }
        public State SourceState { get; protected set; }
        public State LcaState { get; protected set; }
        public SortedSet<State> EnterStates  { get; protected set; }
        public bool IsEnabled { get; set; }
        public bool IsTargetless { get => TargetStates.Count == 0; }
        public bool IsAuto { get; protected set; } = false;
        public double Delta
        {
            get { return SourceState.HostStatechart.Delta; }
        }
        public InputEvent GameInput
        {
            get { return SourceState.HostStatechart.GameInput; }
        }

        #endregion

        public override void _Ready()
        {
            // Convert GD collection to CS collection
            TargetStates = new List<State>(TargetStatesArray);
        }

        public void Init(State sourceState)
        {
            EventName = GetEventName(TransitionEvent);
            if (!IsAuto && EventName == null)
            {
                #if DEBUG
                GD.PushWarning(Name, ": no transition event, if is eventless, check IsAuto.");
                #endif
            }
            SourceState = sourceState;

            // Init EnterStates
            EnterStates = new SortedSet<State>(new CompositionComparer());

            /*
            TODO:
                1. Find LCA(Least Common Ancestor) of source and targets.
                2. Record path from LCA to targets in EnterStates
            */
            State iter = SourceState;
            List<State> srcToRoot = new List<State>();
            while (iter.ParentState != null)
            {
                iter = iter.ParentState;
                srcToRoot.Add(iter);
            }
            
            // Init LCA as SourceState, the first state in srcToRoot
            int reversedLcaIdx = srcToRoot.Count;
            List<State> tgtToRoot = new List<State>();
            foreach (State s in TargetStates)
            {
                // Record target-to-root, with validation
                if (s.HostStatechart != SourceState.HostStatechart)
                {
                    #if DEBUG
                    GD.PushWarning(Name, ": target states should under same statechart.");
                    #endif
                    continue;
                }
                tgtToRoot.Clear();
                iter = s;
                bool isAvailable = true;
                while (iter.ParentState != null)
                {
                    // Check transition conflicts
                    if (iter.StateMode == StateModeEnum.Compond && EnterStates.Contains(iter))
                    {
                        isAvailable = false;
                        #if DEBUG
                        GD.PushWarning(GetPath(), ": target-state conflict, name: ", s.Name);
                        #endif
                        break;
                    }
                    tgtToRoot.Add(iter);
                    iter = iter.ParentState;
                }
                // Transition t conflicts, not available
                if (!isAvailable)
                {
                    continue;
                }


                // LCA between src and 1 tgt
                int maxIdx = 1;
                for (int i = 1; i <= srcToRoot.Count && i <= tgtToRoot.Count; i++)
                {
                    if (srcToRoot[^i] == tgtToRoot[^i])
                    {
                        maxIdx = i;
                    }
                    else
                    {
                        // Last state is LCA
                        break;
                    }
                }
                if (maxIdx < reversedLcaIdx)
                {
                    reversedLcaIdx = maxIdx;
                }

                /*
                    Update enter-states set:
                        1. LCA is included in enter-states
                        2. LCA is the first in enter-states
                */
                for (int i = maxIdx; i <= tgtToRoot.Count; i++)
                {
                    EnterStates.Add(tgtToRoot[^i]);
                }
            }

            // Targetless, enter-states count is 0
            if (IsTargetless)
            {
                return;
            }

            // LCA
            LcaState = srcToRoot[^reversedLcaIdx];
            GD.Print("First element in EnterStates is LCA: ", EnterStates.First() == LcaState);
        }

        public void Check()
        {
            // Transition is enabled on default.
            IsEnabled = true;
            EmitSignal(SignalName.Guard, this);
        }

        protected StringName GetEventName(EventNameEnum transitionEvent) => transitionEvent switch
        {
            EventNameEnum.PRE_PROCESS => "pre_process",
            EventNameEnum.PRE_PHYSICS_PROCESS => "pre_physics_process",
            EventNameEnum.PRE_INPUT => "pre_input",
            EventNameEnum.PRE_UNHANDLED_INPUT => "pre_unhandled_input",
            EventNameEnum.PROCESS => "process",
            EventNameEnum.PHYSICS_PROCESS => "physics_process",
            EventNameEnum.INPUT => "input",
            EventNameEnum.UNHANDLED_INPUT => "unhandled_input",
            EventNameEnum.CUSTOM => CustomEventName,
            _ => null
        };
    }
}