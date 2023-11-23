using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass, Icon("res://addons/state_chart_sharp/icon/StateChart.svg")]
    public partial class StateChart : Node
    {
        #region Preset EventName

        readonly StringName PROCESS = "process";
        readonly StringName BEFORE_PROCESS = "before_process";
        readonly StringName PHYSICS_PROCESS = "physics_process";
        readonly StringName BEFORE_PHYSICS_PROCESS = "before_physics_process";
        readonly StringName INPUT = "input";
        readonly StringName BEFORE_INPUT = "before_input";
        readonly StringName UNHANDLED_INPUT = "unhandled_input";
        readonly StringName BEFORE_UNHANDLED_INPUT = "before_unhandled_input";


        #endregion

        // State chart should not be triggered while running
        protected bool IsRunning { get; set; }
        public double Delta { get; protected set; }
        public InputEvent InputEvent { get; protected set; }
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
                    do
                    {
                        lastPop = IterStack.Pop();
                    } while (IterStack.Count > 0 && lastPop.ParentState == IterStack.Peek());
                }
            }

            #if DEBUG
            GD.Print("-------- States --------");
            foreach(State s in States)
            {
                GD.Print(s.Name);
            }
            GD.Print("-------- Initial ActiveStates --------");
            foreach(State s in ActiveStates)
            {
                GD.Print(s.Name);
            }
            #endif
        }

        public override void _Process(double delta)
        {
            Delta = delta;
            Step(BEFORE_PROCESS);
            Step(PROCESS);
        }

        public override void _PhysicsProcess(double delta)
        {
            Delta = delta;
            Step(BEFORE_PHYSICS_PROCESS);
            Step(PHYSICS_PROCESS);
        }

        public override void _Input(InputEvent @event)
        {
            InputEvent = @event;
            Step(BEFORE_INPUT);
            Step(INPUT);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            InputEvent = @event;
            Step(BEFORE_UNHANDLED_INPUT);
            Step(UNHANDLED_INPUT);
        }

        public void Step(StringName transEvent)
        {
            return;
            QueuedEvents.Enqueue(transEvent);
            if (IsRunning)
            {
                // GD.PushWarning("State chart triggered when running.");
                return;
            }
            
            IsRunning = true;

            while (QueuedEvents.Count > 0)
            {
                StringName eventName = QueuedEvents.Dequeue();
                IterStack.Clear();

                // Backup ActiveStates
                PrevActiveStates.Clear();
                PrevActiveStates.AddRange(ActiveStates);

                // TODO: do Transition
                
                // TODO: after Transition, handle Exit & Enter
                
                
            }

            IsRunning = false;
        }
    }
}