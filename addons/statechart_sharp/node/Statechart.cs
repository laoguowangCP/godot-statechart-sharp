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
        protected State RootState { get; set; }
        protected SortedSet<State> ActiveStates { get; set; }
        protected Queue<StringName> QueuedEvents { get; set; }
        protected Queue<Transition> QueuedTransitions { get; set; }
        protected Queue<Transition> QueuedEventless { get; set; }
        protected SortedSet<State> ExitSet { get; set; }
        protected SortedSet<State> EnterSet { get; set; }
        protected Stack<State> IterStack { get; set; }
        

        public override void _Ready()
        {
            // Initiate statechart
            IsRunning = false;

            States = new List<State>();
            ActiveStates = new SortedSet<State>(new StateComparer());
            QueuedTransitions = new Queue<Transition>();
            QueuedEventless = new Queue<Transition>();
            ExitSet = new SortedSet<State>(new ReversedStateComparer());
            EnterSet = new SortedSet<State>(new StateComparer());

            IterStack = new Stack<State>();
            
            // Collect states, activate initial-states
            Node child = GetChild<Node>(0);
            if (child is null || child is not State)
            {
                GD.PushError("Require 1 State as child.");
                return;
            }
            RootState = child as State;
            IterStack.Push(RootState);
            while (IterStack.Count > 0)
            {
                // Init state
                State top = IterStack.Peek();
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
                        IterStack.Push(substates[i]);
                    }
                }
                else // substates.Count == 0, Pop
                {
                    State lastPop;
                    do // Pop until: top is brother of the last pop state
                    {
                        lastPop = IterStack.Pop();
                    } while (IterStack.Count > 0 && lastPop.ParentState == IterStack.Peek());
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
                    foreach(State es in t.EnterRegion)
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
            EnterSet.Clear();
            IterStack.Clear();

            /*
                1. Do transition
                    1.1 Find transitions: matched event-name && is enabled
                    1.2 Foreach transition:
                        - Check confliction: LCA in exit-set
                        - Deduce enter-region to full-enter-set
                        - Union exit-set and enter-set
                    1.3 Update active-states
                2. Do eventless transition
                    1.1 Find transitions: eventless && is enabled
                    1.2 Foreach transition:
                        - Check confliction: LCA in exit-set
                        - Deduce enter-region to full-enter-set
                        - Union exit-set and enter-set
                    1.3 Update active-states
            */
            // 1. Iter active-states, queue transitions (recursively)
            RootState.SelectTransitions(eventName);

            // 2.
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
                // Do queued transitions
                

                if (QueuedEventless.Count == 0)
                {
                    return;
                }

                // Do queued eventless transitions
                
            }
        }

        public void RegisterTransition(Transition transition)
        {
            // TODO: Generate exit_set/enter_set here
            QueuedTransitions.Enqueue(transition);
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