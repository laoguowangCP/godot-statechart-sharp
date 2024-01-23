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
- [StatechartConfig](#statechartconfig)

<br/>

## Statechart

The control node of whole statechart. You can simply take it as "state machine" as in common state machine system. To make it work, add 1 child state node (non-history) as "root state".

When statechart is ready, it initializes itself, then root state, then nodes descendant to root state. All the composition will be indexed with **"document order"** — the order they showed in an expanded node tree, which is used in most order-considering cases, like entering states or invoking reactions.

After setup, statechart's main logic runs in `void Step(StringName)`. Statechart calls it on node loop events (process, input, etc.). Or you can call it manually with your custom event. Here following things will be done:

1. Get event. If statechart is already running a step, queue event and return.
2. Select transitions from active states.
3. Execute selected transitions.
4. Select and execute automatic transitions from active states for several rounds.
5. Select and execute reactions from active states.
6. If event queue is not cleared, get next event, back to 2.

With a event queue, statechart can store parsed events for latter process. It happens when state/transition/reaction calls another step during a running step. Event like this is called "internal event".

Internal event and "automatic transitions" are both useful, but they may cause unintended endless loop. 

### Properties of Statechart

**`int MaxInternalEventCount`** : Max internal event count handled in 1 step. If `<=0` , statechart will ignore any internal event.

**`int MaxAutoTransitionRound`** : Max iteration rounds of selecting auto transitions in a single step. If `<=0` , statechart will ignore any auto transition.

**`enum EventFlagEnum EventFlag`** : Event flags control activity of node loop events (process, input, etc.) . All disabled by default.

### Methods of Statechart

**`void Step(StringName)`** : Make statechart run a step with given event.

</br>

## State

This node works as "state" in common state machine system. They can be arranged in a tree structure as hierarchical state machine do.

Beware, not all states in the tree are active. Root state is always active. For the rest, their activity are decided by their parent state, according to parent's state mode.

`Compound` is the default state mode:

- If a compound state is active, then exactly 1 substate (non-history) will be active (if there's any), which is called current state.
- Initial state is the substate a compound will choose as current state by default. You can assign it in inspector, or the first substate (non-history) will be initial state (if there's any).

`Parallel` mode is opposite to compund:

- If it is active, then all substate (non-history) will be active.

`History` is a special mode. Usually history state is treated as "pseudo state" — a reference or a pointer to other state(s), only used as a transition's target.

- Never active.
- You can switch whether a history state is deep or shallow.

  - A shallow history points to parent's substate(s), once active till last exit of parent state. With compound parent, it points to the sibling once active (or parent's initial state, if parent has never been active before). With parallel parent, it points to all the non-history siblings.
  - A deep history points to leaf state(s) descendant to parent, once active till last exit of parent state. It makes parent state looks for shallow history recursively onto its descendants.

With given active states, we can further more express their behaviors. Use signals, or composite with other nodes:

- Reaction node is used as a state's child, to express how state react during a step.
- Transition node is used as a state's child, to how state transit during a step.
- `Enter`/`Exit` signal is emitted when state is set active/unactive during a transition, used to express actions on state entry/exit.

### Properties of State

**`enum StateModeEnum StateMode`** : Enumeration of state mode.

**`bool IsDeepHistory`** : Used in history mode only.

**`State InitialState`** : The substate that will be choosed as current state by default. If not assigned, first substate will be initial state (if there's any). Used in compound mode only.

### Signals of State

**`void Enter(StatechartDuct)`** : Emited when state is entered. Parsed state is used to access delta time and input event when handling node loop events.

**`void Exit(StatechartDuct)`** : Emited when state is exit. Parsed state is used to access delta time and input event when handling node loop events.

<br/>

## Transition

Transition node is used as child node of a non-history state. Parented state is the state to leave, known as "source". As for the state(s) to go for — known as "target(s)". If no target(s) is assigned, it becomes a "targetless transition".

As mentioned, when statechart runs a step, firt it needs to select transitions from active states. To do this, active states are queried recursively. With a given event, a state first passes recursion onto its direct child (current state of a compound, or all non-history substate of a parallel), then deals with the returned case:

- Case "-1" : no transition selected in descendants, or there's no descendant. Compound or parallel state checks its own transitions. Return case 1 if any transition is selected, else return case -1 .
- Case "0" : transition selected in descendants, but not all branches are covered. Some of the branches still ask for an enabled transition from anscestors, but expecting no confliction to selected ones. In this case state check its own **targetless** transitions. Return case 1 if any transition is selected, else return case 0 .
- Case "1" : transition selected in descendants, and all branches are covered. No need to check any more. Return case 1 .

So you may have noticed that transition is also based on event. Default event is "Process", so transition can only be checked in a process step. You can switch it to other node loop event, or a custom event, or an "Auto" event.

Transition with "Auto" event, known as "", will be automaticly selected and executed after a

In step 3, with all the states queried, statechart will execute selected transitions with following procedure:

1. Deduce and merge exit set from selected transitions. During the iteration, transition will be disposed if its source state is already in the merging exit set.
2. Exit set excepts from active states. States in exit set emits `Exit` signal in **reversed** document order
3. For selected transitions, emit `invoke` signal in document order.
4. Deduce and merge enter set from selected transitions.
5. Enter set unions with active states. States in enter set emits `Enter` signal in document order.

Here's several specification you shall follow:

- Beware that a transition is enabled by default.

### Properties of Transition

**`TransitionEventNameEnum TransitionEvent`** : 

**`StringName CustomEventName`** : 

**`Array<State> TargetStatesArray`** : 

### Signals of Transition

**`void Guard(Transition)`** : Emited when transition is checked.

**`void Invoke(Transition)`** : Emited when transition is invoked.

<br/>

## Reaction

### Properties of Reaction



### Signals of Reaction

**`void Invoke(StatechartDuct)`** : Emit when reaction is invoked.

<br/>

## StatechartComposition

Base node of Statechart, State, Transition and Reaction.

<br/>

## StatechartDuct

Conducting object parsed through signals. Used to access "context" from statechart, like delta time or input, when handling node loop events (process, input, etc.).

Beware, the variants it packs changes insistently. Ideally, you may use them only in connected method scope.

### Properties of StatechartDuct

**`double Delta`** : Recently updated delta time statechart received from `_Process(double delta)` .

**`double PhysicsDelta`** : Recently updated delta time statechart received from `_PhysicsProcess(double delta)` .

**`InputEvent Input`** : Recently updated input event statechart received from `_Input(InputEvent @event)` .

**`InputEvent ShortcutInput`** : Recently updated input event statechart received from `_ShortcutInput(InputEvent @event)` .

**`InputEvent UnhandledKeyInput`** : Recently updated input event statechart received from `_UnhandledKeyInput(InputEvent @event)` .

**`InputEvent UnhandledInput`** : Recently updated input event statechart received from `_UnhandledInput(InputEvent @event)` .

**`bool IsEnabled`** : Whether the pending transition is enabled or not. Used for methods connected to transition's `Guard` signal.

**`StatechartComposition CompositionNode`** : The statechart composition node who emit the signal. Useful when debugging.

## StatechartConfig
