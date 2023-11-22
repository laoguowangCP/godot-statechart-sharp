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
        private bool _running;
        private double _delta;
        public double GetDelta() => _delta;
        private InputEvent _inputEvent;
        public InputEvent GetInputEvent() => _inputEvent;
        private List<State> _states;
        private List<State> _activeStates;
        private List<State> _prevActiveStates;
        private Stack<State> _iterStack;
        private Queue<StringName> _queuedEvents;
        

        public override void _Ready()
        {
            Node child = GetChild<Node>(0);
            if (child is null || child is not State)
            {
                GD.PushError("Require 1 State as child.");
                return;
            }

            // TODO: initiate statechart
            _running = false;

            _states = new List<State>();
            _activeStates = new List<State>();
            _iterStack = new Stack<State>();

            State root = child as State;
            _iterStack.Push(root);
            while (_iterStack.Count > 0)
            {
                State top = _iterStack.Peek();
                _states.Add(top);
                top.Init(this, _states.Count-1);

                // update _activeStates
                State topParent = top.ParentState;
                if (topParent is null)
                {
                    // root is active
                    top.isActive = true;
                    _activeStates.Add(top);
                }
                else if (topParent.isActive)
                {
                    if (topParent.GetStateMode() == StateModeEnum.Compond)
                    {
                        // for compound parent, first substate is active
                        if (topParent.Substates.Count > 0 && top == topParent.Substates[0])
                        {
                            top.isActive = true;
                            _activeStates.Add(top);
                        }
                    }
                    else if (topParent.GetStateMode() == StateModeEnum.Parallel)
                    {
                        // for parallel parent, all substate is active
                        top.isActive = true;
                        _activeStates.Add(top);
                    }
                }
                

                // Iterate states
                List<State> substates = top.Substates;
                if (substates.Count > 0)
                {
                    for (int i=substates.Count-1; i>0; i--)
                    {
                        _iterStack.Push(substates[i]);
                    }
                }
                else // substates.Count == 0, Pop
                {
                    State lastPop;
                    do
                    {
                        lastPop = _iterStack.Pop();
                    } while (_iterStack.Count > 0 && lastPop.ParentState != _iterStack.Peek());
                }
            }

            // Backup _activeStates
            _prevActiveStates = new List<State>(_activeStates);
        }

        public override void _Process(double delta)
        {
            _delta = delta;
            Step(BEFORE_PROCESS);
            Step(PROCESS);
        }

        public override void _PhysicsProcess(double delta)
        {
            _delta = delta;
            Step(BEFORE_PHYSICS_PROCESS);
            Step(PHYSICS_PROCESS);
        }

        public override void _Input(InputEvent @event)
        {
            _inputEvent = @event;
            Step(BEFORE_INPUT);
            Step(INPUT);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            _inputEvent = @event;
            Step(BEFORE_UNHANDLED_INPUT);
            Step(UNHANDLED_INPUT);
        }

        public void Step(StringName transEvent)
        {
            _queuedEvents.Enqueue(transEvent);
            if (_running)
            {
                // GD.PushWarning("State chart triggered when running.");
                return;
            }
            
            _running = true;

            while (_queuedEvents.Count > 0)
            {
                StringName eventName = _queuedEvents.Dequeue();
            }

            _running = false;
        }
    }
}