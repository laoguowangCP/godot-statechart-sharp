# Statechart Sharp manual

To get full perspective on statechart, you may refer to:

- https://statecharts.dev/ for conceptual introduction.
- https://www.w3.org/TR/scxml/ for detailed specification.

> [!Warning]
>
> This plugin is a stylized implementation of statechart pattern, details may differ from harel statecharts definition. XML extention defined in SCXML is not implemented.

<br/>

- [Statechart](#statechart)
- [State](#state)
- [Transition](#transition)
- [Reaction](#reaction)
- [StatechartComposition](#statechartcomposition)
- [StatechartDuct](#statechartduct)

<br/>

## Statechart

The control node of whole statechart. You can simply take it as "state machine" as in common state machine system. To make it work, add 1 child state node (non-history) as "root state".

When statechart is ready, it initializes itself, then root state, then nodes descendant to root state. All the composition will be indexed with **"document order"** — the order they showed in an expanded node tree, which is used in most order-considering cases, like entering states or invoking reactions.

After setup, statechart's main logic runs in `void Step(StringName)`, where parameter is the given "event". Here following things will be done:

1. Queue event, if statechart is already running a step. Otherwise, do following tasks with the given event.
2. Select transitions (with given event) from active states.
3. Execute selected transitions.
4. Select and execute automatic transitions from active states, for several rounds.
5. Select reactions (with given event) from active states.
6. Execute selected reactions.
7. If event queue is not cleared, get next event, back to 2.

So as you can see, statechart is based on events. It involves 2 situation:

- Statechart steps builtin events, triggered by node loop events (process, input, etc.).
- Name your custom event. Step it manually.

With an event queue, statechart can store parsed events for latter process. It happens when state/transition/reaction steps another event during a running step. Event like this is called "internal event".

Normally, transition are selected if event matches, but there's an exception. "Automatic transitions", also known as eventless transitions, are handled whenever a step is called. This will go for several rounds, until there's no more automatic transition to be executed.

### Properties of Statechart

**`int MaxInternalEventCount`** : Max internal event count handled in 1 step. If <=0 , statechart will ignore any internal event.

**`int MaxAutoTransitionRound`** : Max iteration rounds of selecting automatic transitions in 1 step. If <=0 , statechart will ignore any automatic transition.

**`enum EventFlagEnum EventFlag`** : Event flags control activity of node loop events (process, input, etc.) . All disabled by default.

### Methods of Statechart

**`void Step(StringName)`** : Make statechart run a step with given event.

</br>

## State

This node works as "state" in common state machine system.

States can be arranged in a tree structure as hierarchical state machine do. Likely, not all states in the tree are active. Root state is always active. But for the rest, their activity are decided by their parent state, according to parent's state mode.

`Compound`: the default mode.

- If a compound state is active, then exactly 1 substate (non-history) will be active (if there's any), which is called current state.
- Initial state is the substate a compound will choose as current state by default. You can assign it in inspector, or the first substate (non-history) will be initial state (if there's any).

`Parallel`:

- If it is active, then all substate (non-history) will be active.

`History`: with this mode, state represents the history "active status" of parent's descendant states.

- Never active. History state is only used as a transition's target.
- You can switch whether a history state is deep or shallow.

  - A shallow history points to parent's substate(s), once active till last exit of parent state. With compound parent, it points to the sibling once active (or parent's initial state, if parent has never been active before). With parallel parent, it points to all the non-history siblings.
  - A deep history points to leaf state(s) descendant to parent, once active till last exit of parent state. It makes parent state looks for shallow history recursively onto its descendants.

With given active states, we can further more express their behaviors. Use signals, or composite with other nodes:

- Reaction node is used as a state's child, to express how state react during a step.
- Transition node is used as a state's child, to how state transit during a step.
- `Enter`/`Exit` signal is emitted when state is set active/unactive during a transition, used to express actions on state entry/exit.

### Properties of State

**`enum StateModeEnum StateMode`** : Enumeration of state mode.

**`bool IsDeepHistory`** : History is deep or not (shallow). Only used in history mode.

**`State InitialState`** : The substate that will be choosed as current state by default. If not assigned, first non-history substate will be initial state (if there's any). Only used in compound mode.

### Signals of State

**`void Enter(StatechartDuct)`** : Emitted when state is entered.

**`void Exit(StatechartDuct)`** : Emitted when state is exit.

<br/>

## Transition

Transition node is used as child node of a non-history state, to represent the transition from one state to another/others.

First of all, transition has event. It is used to response to the statechart's step event. You can choose between builtin node loop events and custom event. Additionally, it has an "Auto" event — a null event, which makes it an automatic transition.

From macro scope, transition is involed in 3 stages:

1. Transition is initialized as statechart's composition. Here transition test whether it is valid, and deduce **LCA**, **enter region** and **enter region's edge** from targets.
2. During a statechart's step, transitions are selected and executed from active states with given event.
3. During a statechart's step, automatic transitions are selected and executed for several rounds.

### Initialization

For a transition node, its parented state is called "source state", the state we transit from. Correspondingly, there's "target state(s)", the state we transit to.

Transition in statechart can be simple as common state machine, or complex, as transition may have multiple targets and could happen across hierarchy level. But after all, we just need to translate target(s) and source to the actual states to enter/exit.

First of all, we find "least common anscetor (LCA)" of target(s) and source. It is the state with no descendant can be anscestor to all of the target(s) and source. With the LCA found, states to enter/exit can be deduced by following rules:

- All active states descendant to LCA should be exit.
- For states to be entered, we pick them from descendants of the LCA:

  1. Target(s) are picked, as well as their anscestors, as long as there's no confliction.
  2. Supplement. Supplementary descendants are picked according to the mode of already picked state:

      - Picked state is not history. For compound with no substate picked, pick its initial state. For parallel, pick its unpicked non-hisotry substates. Do this recursively.
      - Picked state is shallow history. Substitute history with sibling state(s) once active. Then supplement this/these sibling(s) as non-history state.
      - Picked state is deep history. Substitute history with all descendants of parent state once active.

- Targetless exception. If transition is assigned with no target, it becomes a "targetless transition". No state(s) will be entered or exit.

Conflict happens when 2 targets descendant to different substate of a compound state. We'll dispose the latter target, by the order they assigned in target array on inspector.

According to the rules, the states to exit depend on status of active states. Also transition can not figure out full collection of states to enter, since we may handle history states. Though nothing is certain during initialization, transition produces following results to save some work:

- LCA
- Enter region: a set of non-history states that is determined to be entered.
- Enter region edge: a set of targeted history states.

### Select and execute

As mentioned, when statechart runs a step, first it needs to select transitions from active states.

- For leaf active states (active states with no non-history substate), selecting transition means iterating through its child transitions (by document order). If event matches, emit the `Guard` signal. It is used to check whether the transition is enabled (should happen) or not. If a transition is enabled, stop iteration and submit it to statechart for latter process.

- For the rest of the active states (active states with non-history substate), it comes with a recursive process. Leaf states select first, while other states react according to returned case from their descendant(s), and then pass on the case to their parent:

  - **Case "-1"** : no transition selected in descendants. State checks its own transitions. Return 1 if a transition is selected, else return -1 .
  - **Case "0"** : transition selected in descendants, but not all of descendant leaf state has an anscestor with selected transition. They still ask for an enabled transition from anscestors, but expecting no confliction to selected one(s). In this case state checks its own **targetless** transitions. Return 1 if any is selected, else return 0 .
  - **Case "1"** : transition selected in descendants, and all descendant leaf states has an anscestor with selected transition. No need to check any more. Return 1 .

After that, selected transitions will be executed. Simple put, we just exit old states, emit transition's `Invoke` signal, and enter new states, while doing some detection to avoid conflicts. The detailed procedures are followed:

1. Iterate selected transitions with document order:

    1. Deduce exit states. It is the collection of active states descendant to LCA.
    2. Filter. If one's source state is descendant to any former's LCA, or any former's source state is descendant to this LCA, dispose this transition. Otherwise we enlist it to "filtered transitions".
    3. Merge exit states to "exit set".

2. Remove states in exit set from active states, and emit their `Exit` signals in **reversed** document order.
3. For filtered transitions, emit `Invoke` signals in document order.
4. Iterate filtered transitions with document order:

    1. Deduce enter states. It is the combination of pre-calculated enter region and deduced enter states for history target.
    2. Merge enter states to "enter set".

5. Add states in enter set to active states, update their compound parent's current state, and emit their `Enter` signals in document order.

### Automatic transition

Automatic transitions are handled after normal given-an-event transitions in every step. The "select and execute" procedures are same, but only 2 differences:

- Use "Auto" (null) event.
- Do several rounds, until no automatic transition is selected any more.

### Invalid transition

An invalid transition will be ignored. It won't be used, and none of its signals will be emitted. Transitions will be set invalid in following cases:

- Event type is "Custom", but no event name assigned.
- Event type is "Auto", while transition is targetless. This will cause endless loop during automatic transitions because source state keeps active whenever this transition invokes.
- Event type is "Auto", while enter region contains source state. This will cause endless loop during automatic transitions because source state keeps active whenever this transition invokes.

Basically, setting a transition invalid is to avoid dangerous situations. It usually happens when assigning target(s) for automatic transitions. However, it can be the case that an automatic transition sets its anscetor's history as target, and an endless loop caused by this is unpredictable. Instead, sticking to safe configuration is recommended: only use the state that has no automatic transition within itself or in its descendants.

### Properties of Transition

**`TransitionEventNameEnum TransitionEvent`** : Enumeration of transition event.

**`StringName CustomEventName`** : Name of the custom event. Only used if transition event is "Custom".

**`Array<State> TargetStatesArray`** : Target states of the transition.

### Signals of Transition

**`void Guard(Transition)`** : Emitted when transition is checked. Used to judge whether transition is enabled (by default) or not. If it is, then transition will be selected and then executed.

**`void Invoke(Transition)`** : Emitted when transition is invoked. It happens when transition is selected and executed, after states' exit and before states' enter.

<br/>

## Reaction

Reaction node is used as child node of a non-history state, to represent the actual reaction of an active state during a step.

As shown in statechart step procedure, after all the transitions handled, we have updated active states as result. Then, their reactions will be collected if event matches, and invoked in document order.

Likely, reaction has event, responsing to statechart's step event. You can choose between builtin node loop events or custom event (no "automatic reaction" by the way).

### Properties of Reaction

**`ReactionEventNameEnum ReactionEvent`** : Enumeration of reaction event.
**`StringName CustomEventName`** : Name of the custom event. Only used if transition event is "Custom".

### Signals of Reaction

**`void Invoke(StatechartDuct)`** : Emitted when reaction is invoked. It happens when reaction is child to active state and matches the step event.

<br/>

## StatechartComposition

Base node of `Statechart`, `State`, `Transition` and `Reaction`. Derived from `Node` .

<br/>

## StatechartDuct

Conducting object parsed through signals, derived from `GodotObject` . It is used to access "context" from statechart, like delta time or input, when handling node loop events (process, input, etc.).

Beware, the variants it packs changes insistently. Ideally, you may use them only in connected method.

For state's enter signals, `StatechartDuct` should be handled carefully. It is because that enter signal is also emitted from active states during statechart's initialization. You can tell whether it is initial enter with `IsRunning` parameter, since statechart is not yet running during initialization.

### Properties of StatechartDuct

**`double Delta`** : Recently updated delta time statechart received from `_Process(double delta)` .

**`double PhysicsDelta`** : Recently updated delta time statechart received from `_PhysicsProcess(double delta)` .

**`InputEvent Input`** : Recently updated input event statechart received from `_Input(InputEvent @event)` .

**`InputEvent ShortcutInput`** : Recently updated input event statechart received from `_ShortcutInput(InputEvent @event)` .

**`InputEvent UnhandledKeyInput`** : Recently updated input event statechart received from `_UnhandledKeyInput(InputEvent @event)` .

**`InputEvent UnhandledInput`** : Recently updated input event statechart received from `_UnhandledInput(InputEvent @event)` .

**`bool IsEnabled`** : Status of the pending transition. Used in transition's `Guard` signal.

**`StatechartComposition CompositionNode`** : The statechart composition node who emit the signal.

**`bool IsRunning`** : Whether the statechart is running.

<br/>
