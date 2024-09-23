# Statechart Sharp manual

To get full perspective on statechart, you may refer to:

- https://statecharts.dev/ for conceptual introduction.
- https://www.w3.org/TR/scxml/ for detailed specification.

> [!WARNING]
>
> This plugin is a stylized implementation of statechart pattern, details may differ from harel statecharts definition. XML extention defined in SCXML is not implemented.

<br/>

- [Statechart](#statechart)
- [State](#state)
- [Transition](#transition)
- [Reaction](#reaction)
- [TransitionPromoter](#transitionpromoter)
- [StatechartComposition](#statechartcomposition)
- [StatechartDuct](#statechartduct)
- [StatechartSnapshot](#statechartsnapshot)

<br/>

## Statechart

> **Inherits**: [StatechartComposition](#statechartcompositiont) < Node

The control node of all statechart compositions. It requires 1 child state (non-history) as "root state".

When statechart is ready, it walks through all the descendant nodes to:

- Index statechart compositions with **"document order"** — the order they listed in expanded node tree (start with 0, statechart node itself). It will be used in most order-considering cases.
- Enter states active initially.

Statechart is based on event, using `StringName` type. It can be 1) builtin events like proccess/input or 2) custom event. Given a event, statechart can run a **step**, where following things will be done:

1. Select and execute [transition](#transition)s from active states (with given event).
2. Select and execute [automatic transition](#automatic-transition)s from active states.
3. Select and execute [reaction](#reaction)s from active states (with given event).

Additionaly, statechart accepts step calls when a step is already running. Events parsed by these steps, known as **internal event**s, will be queued. It goes like (not real code):

```gdscript
func step(event):
  events.enqueue(event)
  if not running:
    while events.count > 0:
      handle(events.dequeue())
```

Here is the overview of how statechart handles a single event (not real code):

```gdscript
func handle(event):
  transitions = select_transitions(event)
  transitions.execute()

  do:
    transitions = select_transitions(Auto)
    transitions.execute()
  while transitions.count > 0

  reactions = select_reations(event)
  reactions.execute()
```

### Properties of Statechart

**`int MaxInternalEventCount`** : Max internal event count handled per step. If <=0 , ignore any internal event.

**`int MaxAutoTransitionRound`** : Max iteration rounds of selecting and executing automatic transitions per event. If <=0 , ignore any automatic transition.

**`enum EventFlagEnum EventFlag`** : Control activity of node loop events (process, input, etc.) . All disabled by default.

**`bool IsWaitParentReady`** : If true, statechart setup process will wait parent node to be ready.

### Methods of Statechart

**`void Step(StringName)`** : Make statechart run a step with given event.

**`StatechartSnapshot Save(bool)`** : Save statechart. If arg1 is set true, it saves all states' configuration, so history info can be preserved. Else it only saves active states' configuration. Return [StatechartSnapshot](#statechartsnapshot) object, null if save is not successed.

**`bool Load(StatechartSnapshot, bool, bool)`** : Load statechart. Arg1 is the `StatechartSnapshot` you saved. Arg2 is `isExitOnLoad`, if true, active states will be exit before configuration update. Arg3 is `isEnterOnLoad`, if true, active states will be entered after configuration update. Return true if load is successed.

</br>

## State

> **Inherits**: [StatechartComposition](#statechartcomposition) < Node

Base composition of statechart. Only if a state is **active**, its [transition](#transition)(s) and [reaction](#reaction)(s) will be used during a step. Setting a state **active**/**inactive** is called to **enter**/**exit** a state. When happened, state will emit `Enter`/`Exit` signal.

Similar to hierarchy state machine, states can be arranged in a tree structure (where child state is called **substate**), except that the state has 3 **mode**s to choose from. It alters hierarchical behavior:

**Compound**: default mode.

- If a compound state is active, then exactly 1 substate (non-history) will be active (if there's any). The active substate is called **current state**.
- Compound state will take its first substate (non-history) as **initial state** (if there's any) — the substate a compound will choose as current state by default.

**Parallel**:

- If a parallel state is active, then all substate(s) (non-history) will be active.

**History**:

- Never active.
- A history state represents "history active status". It is only used as target of [transition](#transition), replacing itself to pointed state(s) during runtime. You can switch between **shallow history** or **deep history**:

  - A shallow history points to sibling state(s), once active till last exit of parent state.
  - A deep history points to all state(s) descendant to parent state, once active till last exit of parent state.

### Properties of State

**`StateModeEnum StateMode`** : Enumeration of state mode.

**`bool IsDeepHistory`** : History is deep or not (shallow). Only used in history mode.

**`State InitialState`** : The substate that will be choosed as current state by default. If not assigned, first non-history substate will be initial state (if there's any). Only used in compound mode.

### Signals of State

**`void Enter(StatechartDuct)`** : Emitted when state is entered.

**`void Exit(StatechartDuct)`** : Emitted when state is exit.

### Methods of State

**`void CustomStateEnter(StatechartDuct)`**: Overrideable state enter behavior. Emit `Enter` signal by default.

**`void CustomStateExit(StatechartDuct)`**: Overrideable state exit behavior. Emit `Exit` signal by default.

<br/>

## Transition

> **Inherits**: [StatechartComposition](#statechartcomposition) < Node

Transition is used as child of a non-history state, to represent transition from parented state (called **source state**) to state(s) expected to be entered (known as **target state**(s)).

Transition is involed in:

- [Initialization](#initialization)
- [Select and execute](#select-and-execute)

### Initialization

A transition is about "exiting a set of state(s)" and then "entering another set of state(s)". But they are not definite during initialization. Instead, transition deduces:

- **LCA state**, the least common anscetor state of target state(s) and source state. When transition happens, any active states descendant to LCA should be exit.
- **Enter region**, a set of states to be entered definitely when transition happens. It includes 1) non-history target(s), 2) states descendant to LCA and ancestor to any target, 3) descendants of above states to be entered by default.
- **Enter region's edge**, a set of history state(s) from target(s). Used for runtime deduction.

Beware the exception: **targetless transition** has 0 target assigned. LCA is parent of source state, while no state(s) will be entered or exit if this targetless transition happens.

Conflict happens when mutiple targets are or descendant to different substates of a compound state. Transition will dispose the latter target(s), by the order they assigned in target array in inspector.

### Select and execute

Transition is based on event, responsing to the statechart's step event. It can be 1) builtin events like proccess/input, 2) custom event or 3) "Auto" event — a null event, which makes it an **automatic** transition. Transitions of active states, if event matches, will be selected and executed in document order during a step. Non-automatic transitions will be dealt first.

To select transitions, a recursion is invoked on active states, starting with leaf state(s):

- For a leaf state, its child transitions are iterated in document order. If event matches, `Guard` signal will be emitted to check whether the transition is enabled. If does, then transition is selected, and iteration stops.
- For non-leaf states, they need consider "selecting situation" of their descendant state(s):

  - **Case "-1"** : no transition selected in active descendant state(s). State checks child transitions.
  - **Case "0"** : transition selected in descendant state(s), but not all of active descendant leaf state has an ancestor with selected transition. They still ask for an enabled transition from ancestors, but expecting no confliction to selected one(s). In this case, state only checks targetless transitions.
  - **Case "1"** : transition selected in descendants, and all active descendant leaf has an ancestor with selected transition. No need to check.

After that, selected transitions are executed. Here we update active states, invoke transitions, while doing some validation to avoid conflicts:

1. Iterate selected transitions with document order. For each transition, do filter. If one's source state is descendant to any former's LCA, or any former's source state is descendant to this LCA, dispose this transition. Otherwise, add transition to **filtered transitions**, deduce states to exit, and merge them to **exit set**.

2. For states in exit set, remove them from active states, and emit their `Exit` signals in *reversed* document order.
3. For filtered transitions, emit `Invoke` signals in document order.
4. Iterate filtered transitions with document order. For each transition, emit `Invoke` signal, deduce enter states (combines enter region and deduced states from enter region edge), and merge them to **enter set**.
5. For states in enter set, add them to active states, and emit their `Enter` signals in document order.

[Automatic transition](#automatic-transition)s are handled after normal given-an-event transitions. The procedures are same, but only 2 differences:

- Use "Auto" event.
- Do several rounds, till no automatic transition is selected to execute.

### Automatic transition

Also known as eventless transition. Automatic transitions do not response to certain event, but always handled after normal given-an-event transitions.

To make a transition automatic, set its event property to "Auto".

### Invalid transition

An invalid transition won't be checked and its signals won't be emitted. Transitions will be set invalid in following cases:

- Event type is "Custom", but no event name assigned.
- Event type is "Auto", while transition is targetless. This may cause loop transition, since source state keeps active whenever this transition invokes.
- Event type is "Auto", while enter region contains source state. This may cause loop transition, since source state keeps active whenever this transition invokes.

> [!TIP]
>
> Setting a transition invalid is intended to suppress dangerous configuration of automatic transitions. But situations can be unpredictable (automatic transition sets its anscetor's history as target, pairs of cyclic automatic transition, etc.) . A safer way to utilize automatic transition is to only use certain state as target, as long as it has no automatic transition itself or in its descendants.

### Properties of Transition

**`TransitionEventNameEnum TransitionEvent`** : Enumeration of transition event.

**`StringName CustomEventName`** : Name of the custom event. Only used if transition event is "Custom".

**`Array<State> TargetStatesArray`** : Target states of the transition.

### Signals of Transition

**`void Guard(StatechartDuct)`** : Emitted when transition is checked. Used to judge whether transition is enabled (by default) or not. If it is, then transition will be selected and then executed.

**`void Invoke(StatechartDuct)`** : Emitted when transition is invoked. It happens when transition is selected and executed, after states' exit and before states' enter.

### Methods of Transition

**`bool CustomTransitionGuard(StatechartDuct)`**: Overrideable transition guard behavior. Emit `Guard` signal by default.

**`void CustomTransitionInvoke(StatechartDuct)`**: Overrideable transition invoke behavior. Emit `Invoke` signal by default.

<br/>

## Reaction

> **Inherits**: [StatechartComposition](#statechartcomposition) < Node

Reaction node is used as child node of a non-history state, to represent what state do during a step.

Reaction is based on event, responsing to the statechart's step event. It can be 1) builtin events like proccess/input, 2) custom event.

Reactions of active states, if event matches, will be invoked in document order at the end of a step.

### Properties of Reaction

**`ReactionEventNameEnum ReactionEvent`** : Enumeration of reaction event.
**`StringName CustomEventName`** : Name of the custom event. Only used if transition event is "Custom".

### Signals of Reaction

**`void Invoke(StatechartDuct)`** : Emitted when reaction is invoked. It happens when reaction is child to active state and matches the step event.

### Methods of Reaction

**`void CustomReactionInvoke(StatechartDuct)`**: Overrideable reaction invoke behavior. Emit `Invoke` signal by default.

<br/>

## TransitionPromoter

> **Inherits**: [StatechartComposition](#statechartcomposition) < Node

TransitionPromoter node is used as child node of a transition. When parented transition is ready, the promoter will:

1. Get certain leaf states descendant to parent state of the transition.
2. Duplicate transition, add it as first child to each leaf state.
3. Delete origin transition and promoter itself.

TransitionPromoter is useful if you need to "prioritize" a transition, meaning you want your transition to be considered first in a subtree of statechart. As transition selection is started from leaf states, prioritized transition needs multiple copies in *certain leaf states*. The query of certain leaf states comes up with a recursion, started with parent state of the transition:

- Compound state: query each substate. If no descendant state is selected, then commit itself as a leaf state.
- Parallel state: query substates, but stop if any descendant is selected. Or else no descendant is leaf state, then commit itself as leaf state.
- History state: do nothing.

<br/>

## StatechartComposition

> **Inherits**: Node

Base composition node for statechart.

### Methods of StatechartComposition

**`StatechartComposition Append(StatechartComposition)`**: Add parsed composition node as child, returns parent node itself. Used for building statechart from scripts.

<br/>

## StatechartDuct

> **Inherits**: RefCounted

Conducting "context" from statechart when handling node loop events (process, input, etc.), including deltatime, input, etc.

For state's enter signals, `StatechartDuct` should be handled carefully, as enter signal is emitted during statechart's initialization. You can tell whether it is initial enter with `IsRunning` parameter, `IsRunning == false` indicates statechart is not yet running.

### Properties of StatechartDuct

**`double Delta`** : Recently updated delta time statechart received from `_Process(double delta)` .

**`double PhysicsDelta`** : Recently updated delta time statechart received from `_PhysicsProcess(double delta)` .

**`InputEvent Input`** : Recently updated input event statechart received from `_Input(InputEvent @event)` .

**`InputEvent ShortcutInput`** : Recently updated input event statechart received from `_ShortcutInput(InputEvent @event)` .

**`InputEvent UnhandledKeyInput`** : Recently updated input event statechart received from `_UnhandledKeyInput(InputEvent @event)` .

**`InputEvent UnhandledInput`** : Recently updated input event statechart received from `_UnhandledInput(InputEvent @event)` .

**`bool IsTransitionEnabled`** : Status of the pending transition. Used in transition's `Guard` signal.

**`StatechartComposition CompositionNode`** : The statechart composition node who emit the signal and parse this object.

**`bool IsRunning`** : Whether the statechart of the parsed statechart composition is running a step.

<br/>

## StatechartSnapshot

> **Inherits**: Resource

Save object of a statechart. It stores the save flag (boolean parsed during `Statechart.Save(bool)`) and states configuration (an integer array, listing compound states' substate index). Can be serialized/deserialized without custom resource saver/loader.

<br/>
