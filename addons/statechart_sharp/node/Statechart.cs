using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    [GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
    public partial class Statechart : StatechartComposition
    {
        protected bool IsRunning { get; set; }
        public double Delta { get; protected set; }
        public double PhysicsDelta { get; protected set; }
        public InputEvent GameInput { get; protected set; }
        public List<State> States { get; protected set; }
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