using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
    public partial class Statechart : StatechartComposition
    {
        [Export(PropertyHint.Range, "0,32,")] protected int MaxAutoTransitionRound = 8;
        protected bool IsRunning { get; set; }
        public double Delta { get; protected set; }
        public double PhysicsDelta { get; protected set; }
        public InputEvent GameInput { get; protected set; }
        public InputEvent GameUnhandledInput { get; protected set; }
        public List<State> States { get; protected set; }
        protected State RootState { get; set; }
        protected SortedSet<State> ActiveStates { get; set; }
        protected Queue<StringName> QueuedEvents { get; set; }
        protected List<Transition> EnabledTransitions { get; set; }
        protected List<Transition> EnabledAutoTransitions { get; set; }
        protected List<Transition> EnabledFilteredTransitions { get; set; }
        protected SortedSet<State> ExitSet { get; set; }
        protected SortedSet<State> EnterSet { get; set; }
        

        public override void _Ready()
        {
            // Initiate statechart
            IsRunning = false;

            States = new List<State>();
            ActiveStates = new SortedSet<State>(new StateComparer());
            QueuedEvents = new Queue<StringName>();
            EnabledTransitions = new List<Transition>();
            EnabledAutoTransitions = new List<Transition>();
            EnabledFilteredTransitions = new List<Transition>();
            ExitSet = new SortedSet<State>(new ReversedStateComparer());
            EnterSet = new SortedSet<State>(new StateComparer());
            
            // Collect states, activate initial-states
            Init();

            // Init Transitions
            // Enter active states, document order
            PostInit();
        }

        public override void Init()
        {
            OrderId = 0;
            HostStatechart = this;
            foreach (Node child in GetChildren())
            {
                if (child is State rootState)
                {
                    RootState = rootState;
                }
            }
            
            if (RootState != null)
            {
                int ancestorId = 0;
                RootState.Init(this, ref ancestorId);
            }
        }

        public override void PostInit()
        {
            // Get activeStates
            RootState.RegisterActiveState(ActiveStates);

            if (RootState != null)
            {
                RootState.PostInit();
            }
            return;
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
            /*
                1. Select transitions
                2. Do transitions
                3. While iter < MAX_AUTO_ROUND:
                    - Select auto-transition
                    - Do auto-transition
                    - Break if no queued auto-transition
                4. Do action of active-states

            */
            // 1. Select transitions
            RootState.SelectTransitions(EnabledTransitions, eventName);

            // 2. Do transitions
            DoTransitions(EnabledTransitions);

            // 3. Auto transition
            for (int i = 0; i <= MaxAutoTransitionRound; ++i)
            {
                RootState.SelectTransitions(EnabledTransitions);

                // Active-states are stable
                if (EnabledTransitions.Count == 0)
                {
                    break;
                }
                DoTransitions(EnabledTransitions); 
            }

            // 4. Invoke action of active-states
            foreach (State s in ActiveStates)
            {
                s.StateInvoke(eventName);
            }
        }

        protected void DoTransitions(List<Transition> enabledTransitions)
        {
            /*
            Batch:
                1. Process exit-set (with filter)
                2. Invoke transitions
                3. Process enter-set (update current)
            */

            // 1. Exit-set
            foreach (Transition t in enabledTransitions)
            {
                if (ExitSet.Contains(t.LcaState))
                {
                    continue;
                }
                EnabledFilteredTransitions.Add(t);
                SortedSet<State> exitStates = ActiveStates.GetViewBetween(
                    t.LcaState.LowerState, t.LcaState.UpperState);
                ExitSet.UnionWith(exitStates);
            }
            ActiveStates.ExceptWith(ExitSet);
            foreach (State s in ExitSet)
            {
                s.StateExit();
            }

            // 2. Invoke transition
            foreach (Transition t in EnabledFilteredTransitions)
            {
                t.TransitionInvoke();
            }

            // 3. Enter-set
            foreach (Transition t in EnabledFilteredTransitions)
            {
                SortedSet<State> enterRegion = t.EnterRegion;
                SortedSet<State> deducedEnterStates = t.GetDeducedEnterStates();
                EnterSet.UnionWith(enterRegion);
                EnterSet.UnionWith(deducedEnterStates);
            }
            ActiveStates.UnionWith(EnterSet);
            foreach (State s in EnterSet)
            {
                s.StateEnter();
            }

            // 4. Clear
            enabledTransitions.Clear();
            ExitSet.Clear();
            EnterSet.Clear();
            EnabledFilteredTransitions.Clear();
        }

        public override void _Process(double delta)
        {
            Delta = delta;
            Step(StatechartConfig.EVENT_PROCESS);
        }

        public override void _PhysicsProcess(double delta)
        {
            Delta = delta;
            Step(StatechartConfig.EVENT_PHYSICS_PROCESS);
        }

        public override void _Input(InputEvent @event)
        {
            GameInput = @event;
            Step(StatechartConfig.EVENT_INPUT);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            GameInput = @event;
            Step(StatechartConfig.EVENT_UNHANDLED_INPUT);
        }
    }
}