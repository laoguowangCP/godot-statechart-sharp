using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
    public partial class Statechart : Node
    {
        #region Preset EventName

        readonly StringName PROCESS = "process";
        readonly StringName PRE_PROCESS = "pre_process";
        readonly StringName PHYSICS_PROCESS = "physics_process";
        readonly StringName PRE_PHYSICS_PROCESS = "pre_physics_process";
        readonly StringName INPUT = "input";
        readonly StringName PRE_INPUT = "pre_input";
        readonly StringName UNHANDLED_INPUT = "unhandled_input";
        readonly StringName PRE_UNHANDLED_INPUT = "pre_unhandled_input";


        #endregion

        protected bool IsRunning { get; set; }
        public double Delta { get; protected set; }
        public InputEvent GameInput { get; protected set; }
        protected List<State> States { get; set; }
        protected List<State> ActiveStates { get; set; }
        protected List<State> PrevActiveStates { get; set; }
        protected Stack<State> IterStack { get; set; }
        protected Queue<StringName> QueuedEvents { get; set; }
        

        public override void _Ready()
        {
            // Initiate statechart
            IsRunning = false;

            States = new List<State>();
            ActiveStates = new List<State>();
            PrevActiveStates = new List<State>();
            IterStack = new Stack<State>();
            
            // Collect states, activate initial-states
            Node child = GetChild<Node>(0);
            if (child is null || child is not State)
            {
                GD.PushError("Require 1 State as child.");
                return;
            }
            State root = child as State;
            IterStack.Push(root);
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
                            topParent.CurrentState = top;
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
                    foreach(State es in t.EnterStates)
                    {
                        GD.Print("  - ", es.Name, " ", es.StateId);
                    }
                }
            }
            #endif

            // Enter active states
            foreach (State s in ActiveStates)
            {
                s.EmitSignal(State.SignalName.Enter);
            }
        }

        public void Step(StringName transitionEvent)
        {
            return;
            // Queue transition event
            QueuedEvents.Enqueue(transitionEvent);
            if (IsRunning)
            {
                return;
            }
            
            IsRunning = true;

            while (QueuedEvents.Count > 0)
            {
                StringName nextEvent = QueuedEvents.Dequeue();
                IterStack.Clear();

                // Backup ActiveStates
                PrevActiveStates.Clear();
                PrevActiveStates.AddRange(ActiveStates);

                // TODO: do Transition
                IterStack.Push(ActiveStates[0]);
                while (IterStack.Count > 0)
                {
                    // Iterate Transitions
                    State top = IterStack.Peek();
                    foreach(Transition t in top.Transitions)
                    {
                        t.Check(); // Check guard
                        /*
                            If t available:
                                critic = t.LCCA
                                IterStack.Pop() until critic
                                List<State> enterStates = t.EnterStates
                                IterStack.Push()
                                break
                        */
                    }
                }
                
                // TODO: after Transition, handle Exit & Enter
                
            }

            IsRunning = false;
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