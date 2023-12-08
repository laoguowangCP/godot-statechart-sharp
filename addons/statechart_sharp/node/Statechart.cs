using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
    public partial class Statechart : Node
    {
        #region Preset EventName

        public readonly StringName PRE_PROCESS = "pre_process";
        public readonly StringName PRE_PHYSICS_PROCESS = "pre_physics_process";
        public readonly StringName PRE_INPUT = "pre_input";
        public readonly StringName PRE_UNHANDLED_INPUT = "pre_unhandled_input";
        public readonly StringName PROCESS = "process";
        public readonly StringName PHYSICS_PROCESS = "physics_process";
        public readonly StringName INPUT = "input";
        public readonly StringName UNHANDLED_INPUT = "unhandled_input";


        #endregion

        protected bool IsRunning { get; set; }
        public double Delta { get; protected set; }
        public double PhysicsDelta { get; protected set; }
        public InputEvent GameInput { get; protected set; }
        protected List<State> States { get; set; }
        protected SortedSet<State> ActiveStates { get; set; }
        protected Queue<StringName> QueuedEvents { get; set; }
        protected Queue<Transition> QueuedTransitions { get; set; }
        protected Queue<Transition> QueuedEventless { get; set; }
        protected SortedSet<State> ExitSet { get; set; }
        

        public override void _Ready()
        {
            // Initiate statechart
            IsRunning = false;

            States = new List<State>();
            ActiveStates = new SortedSet<State>(new StateComparer());
            QueuedTransitions = new Queue<Transition>();
            QueuedEventless = new Queue<Transition>();
            ExitSet = new SortedSet<State>(new StateComparer());

            Stack<State> iterStack = new Stack<State>();
            
            // Collect states, activate initial-states
            Node child = GetChild<Node>(0);
            if (child is null || child is not State)
            {
                GD.PushError("Require 1 State as child.");
                return;
            }
            State root = child as State;
            iterStack.Push(root);
            while (iterStack.Count > 0)
            {
                // Init state
                State top = iterStack.Peek();
                States.Add(top);
                top.Init(this, States.Count-1);

                // Update ActiveStates
                State topParent = top.ParentState;
                if (topParent is null)
                {
                    // Root is active
                    top.IsActive = true;
                    ActiveStates.Add(top);
                }
                else if (topParent.IsActive)
                {
                    if (topParent.StateMode == StateModeEnum.Compond)
                    {
                        // For active compound, first substate is active
                        if (topParent.Substates.Count > 0 && top == topParent.Substates[0])
                        {
                            top.IsActive = true;
                            ActiveStates.Add(top);
                        }
                    }
                    else if (topParent.StateMode == StateModeEnum.Parallel)
                    {
                        // For active parallel, all substate is active
                        top.IsActive = true;
                        ActiveStates.Add(top);
                    }
                }
                
                // Iterate states
                List<State> substates = top.Substates;
                if (substates.Count > 0)
                {
                    // Reversed push
                    for (int i=substates.Count-1; i>=0; i--)
                    {
                        iterStack.Push(substates[i]);
                    }
                }
                else // substates.Count == 0, Pop
                {
                    State lastPop;
                    do // Pop until: top is brother of the last pop state
                    {
                        lastPop = iterStack.Pop();
                    } while (iterStack.Count > 0 && lastPop.ParentState == iterStack.Peek());
                }
            }

            #if DEBUG
            GD.Print("-------- States --------");
            foreach(State s in States)
            {
                GD.Print(s.StateId, ". ", s.Name);
                foreach (var t in s.Transitions)
                {
                    GD.Print("  - ", t.Name);
                }
            }
            GD.Print("-------- Initial ActiveStates --------");
            foreach(State s in ActiveStates)
            {
                GD.Print(s.StateId, ". ", s.Name);
            }
            #endif

            // Init Transitions
            foreach (State s in States)
            {
                foreach (Transition t in s.Transitions)
                {
                    t.Init(s);
                }
            }

            
            #if DEBUG
            GD.Print("-------- Transitions --------");
            foreach (State s in States)
            {
                foreach (Transition t in s.Transitions)
                {
                    GD.Print(t.Name, ", Source", t.SourceState.Name);
                    foreach(State es in t.EnterStates)
                    {
                        GD.Print("  - ", es.Name, " ", es.StateId);
                    }
                }
            }
            #endif

            // Enter active states, document order
            foreach (State s in ActiveStates)
            {
                s.EmitSignal(State.SignalName.Enter);
            }
        }

        public void Step(StringName eventName)
        {
            // Queue transition event
            QueuedEvents.Enqueue(eventName);
            if (IsRunning)
            {
                return;
            }
            
            IsRunning = true;

            while (QueuedEvents.Count > 0)
            {
                StringName nextEvent = QueuedEvents.Dequeue();
                HandleEvent(nextEvent);
            }

            IsRunning = false;
        }

        /// <summary>
        /// Macro step
        /// </summary>
        /// <param name="eventName"></param>
        protected void HandleEvent(StringName eventName)
        {
            // Backup ActiveStates
            SortedSet<State> prevActiveStates = new(ActiveStates);
            ExitSet.Clear();

            /*
            TODO: Step algorithm
                1. Iter active-states, queue transitions
                2. Iter queued transitions
                    2.1 do transition
                        2.1.1 validate confliction: src not in exit-set
                        2.1.2 GetViewBitween(LCA.lower, LCA.upper) from active-states
                        2.1.3 subtract view from active-states, union view to exit-states
                        2.1.4 deduce descendants of enter-states
                        2.1.5 for descendants of enter-states, queue eventless transitions
                        2.1.6 union active-states and enter-states(include descendants)
                    2.2 iter queued eventless transitions
                        2.2.1 do transition(same above)
                3. unchanged = prev - exit
                4. Iter prev - unchanged
                    4.1 do exit
                5. Iter active - unchanged
                    5.1 do enter
            */

            // 1. Iter active-states, queue transitions
            foreach(State s in ActiveStates)
            {
                foreach (Transition t in s.Transitions)
                {
                    // filter eventless and not-this-event
                    if (t.IsAuto || t.EventName != eventName)
                    {
                        continue;
                    }

                    // Check guard
                    t.Check();

                    // Targetless transition checks only
                    if (!t.IsTargetless && t.IsEnabled)
                    {
                        QueuedTransitions.Enqueue(t);
                        break;
                    }
                }
            }

            if (QueuedTransitions.Count == 0)
            {
                return;
            }

            // 2. Iter queued transitions
            for (Transition t=QueuedTransitions.Dequeue();
                QueuedTransitions.Count>0;
                t=QueuedTransitions.Dequeue())
            {
                // Do transition
                DoTransition(t);

                if (QueuedEventless.Count == 0)
                {
                    return;
                }

                // Iter queued eventless transitions
                for (Transition et=QueuedEventless.Dequeue();
                    QueuedEventless.Count>0;
                    et=QueuedEventless.Dequeue())
                {
                    DoTransition(et);
                }
            }
        }

        protected void DoTransition(Transition t)
        {
            // 1. Validate confliction: src not in exit-set
            if (ExitSet.Contains(t.SourceState))
            {
                return;
            }

            // 2. GetViewBetween(LCA.lower, LCA.upper) from active-states
            var lcaState = t.LcaState;
            SortedSet<State> statesToExit = ActiveStates.GetViewBetween(lcaState.Substates[0], lcaState.Substates[^1]);


            // 3. Subtract view from active-states, union view to exit-states
            // 4. Deduce descendants of enter-states
            // 5. For descendants of enter-states, queue eventless transitions
            // 6. union active-states and enter-states(include descendants)
        }

        public override void _Process(double delta)
        {
            Delta = delta;
            Step(PRE_PROCESS);
            Step(PROCESS);
        }

        public override void _PhysicsProcess(double delta)
        {
            Delta = delta;
            Step(PRE_PHYSICS_PROCESS);
            Step(PHYSICS_PROCESS);
        }

        public override void _Input(InputEvent @event)
        {
            GameInput = @event;
            Step(PRE_INPUT);
            Step(INPUT);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            GameInput = @event;
            Step(PRE_UNHANDLED_INPUT);
            Step(UNHANDLED_INPUT);
        }
    }
}