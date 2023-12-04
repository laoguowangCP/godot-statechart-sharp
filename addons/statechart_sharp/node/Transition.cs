using System.Collections.Generic;
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
        CUSTOM
    }

    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg")]
    public partial class Transition : Node
    {
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>

        #region define signals

        [Signal] public delegate void GuardEventHandler(Transition t);
        
        #endregion

        #region define properties

        // If transitEvent is null, transition is auto (checked on state enter)
        [Export] public bool IsAuto = false;
        [Export] public EventNameEnum TransitionEvent { get; protected set; } = EventNameEnum.PRE_PROCESS;
        [Export] public StringName CustomEventName { get; protected set; }
        [Export] protected Array<State> TargetStatesArray;
        public StringName EventName { get; protected set; }
        protected List<State> TargetStates { get; set; }
        public State SourceState { get; protected set; }
        public State LcaState { get; protected set; }
        public SortedSet<State> EnterStates  { get; protected set; }
        public Statechart HostStatechart { get; protected set; }
        public bool IsEnabled { get; set; }
        public double Delta
        {
            get { return HostStatechart.Delta; }
        }
        public InputEvent GameInput
        {
            get { return HostStatechart.GameInput; }
        }

        #endregion

        public override void _Ready()
        {
            // Convert GD collection to CS collection
            if (TargetStatesArray.Count > 0)
            {
                TargetStates = new List<State>(TargetStatesArray);
            }
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
            HostStatechart = SourceState.HostStatechart;

            // Targetless transition
            if (TargetStates == null)
            {
                return;
            }

            // Init EnterStates
            EnterStates = new SortedSet<State>(new StateComparer());

            /*
                TODO:
                    1. Find LCA(Least Common Ancestor) of source and targets.
                    2. Record path from LCA to targets in EnterStates, order: parent first, then reversed children
            */
            State iter = SourceState;
            List<State> srcToRoot = new List<State>();
            while (iter.ParentState != null)
            {
                iter = iter.ParentState;
                srcToRoot.Add(iter);
            }
            
            // Init LCA as SourceState, the last state in srcToRoot
            int reversedLcaIdx = srcToRoot.Count;
            List<State> tgtToRoot = new List<State>();
            foreach (State s in TargetStates)
            {
                // Record target-to-root, with validation
                if (s.HostStatechart != HostStatechart)
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


                // Update LCA index
                int maxIdx = 1;
                for (int i = 1; i <= srcToRoot.Count && i <= tgtToRoot.Count; i++)
                {
                    if (srcToRoot[^i] == tgtToRoot[^i])
                    {
                        maxIdx = i;
                    }
                    else
                    {
                        // Last state is LCCA
                        break;
                    }
                }
                if (maxIdx < reversedLcaIdx)
                {
                    reversedLcaIdx = maxIdx;
                }

                // Update enter-states set
                foreach(State stateOnPath in tgtToRoot)
                {
                    EnterStates.Add(stateOnPath);
                }
            }

            // LCA
            LcaState = srcToRoot[^reversedLcaIdx];
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