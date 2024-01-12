<p align="center">
  <img src="./addons/statechart_sharp/icon/Statechart.svg" height="200px" />
</p>

# Statechart Sharp

 > A simple statechart plugin for Godot, implemented in C#.

## Table of Content

- [Introduction](#introduction)
- [Start](#start)
- [Specification](#specification)

  - [Statechart](#statechart)
  - [State](#state)
  - [Transition](#transition)
  - [Reaction](#reaction)

- [Todo](#todo)

## Introduction

What is statechart? Simple put:

- It's a statemachine.
- Supports hierarchy.
- With various state mode.

This plugin provides basic nodes to build statechart in Godot editor.

## Start

> [!IMPORTANT]
>
> Before you start:
>
> 1. You need .NET-enabled version of Godot.
> 2. Download repository.
> 3. Copy `addons/statechart_sharp` to your project folder.
> 4. Build project once.
> 5. Enable plugin in project setting.

**Step 1** : Add <img src="./addons/statechart_sharp/icon/Statechart.svg" alt="Statechart" style="width:16px;" align="bottom"/> Statechart node

**Step 2** : Add <img src="./addons/statechart_sharp/icon/State.svg" style="width:16px;" alt="State" align="bottom"/> State node

- A root state, as child of a statechart.
- Add substates under root state.
- Switch state mode: compound, parallel, or history.
- Connect signals.

**Step 3** : Add <img src="./addons/statechart_sharp/icon/Transition.svg" style="width:16px;" alt="Transition" align="bottom"/> Transition node

- As child of a state: where we exit from.
- Assign target states: where we enter to.
- Switch transition event: node loop event, custom event, or auto.
- Connect signals.

**Step 4** : Add <img src="./addons/statechart_sharp/icon/Reaction.svg" style="width:16px;" alt="Action" align="bottom"/> Reaction node

- As child of a state.
- Switch reaction event: node loop event, or custom event.
- Connect signals.

**Step 5** : Build and run

- Statechart will step node loop events through states, as long as it is not disabled or paused.
- To step a custom event, call `Statechart.Step(StringName)` .

## Specification

To get full perspective on statechart, you may refer to:

- https://statecharts.dev/ for conceptual introduction.
- https://www.w3.org/TR/scxml/ for detailed specification.

### Statechart

The control node of whole statechart. You can simply take it as "state machine" as in common state machine system. To make it work properly, add exactly 1 child `State` node as "root state" (non-history state), which is always active.

At the very beginning, statechart initializes itself, then root state, and all the statechart composition descendant to it. All the composition will be indexed with **"document order"** — the order they showed in an expanded node tree, which is used in most order-considering cases.

`Statechart` provides `Step` method, which is the main way we interact with statechart. Node loop events (process, input, etc.) also call `Step` method. The only difference is that they use built-in event, defined in `util/StatechartConfig.cs` .

During the `Step` method, statechart will do following things:

1. Queue event, if statechart is already running a step. Otherwise, statechart will fetch event from queue, and do following tasks.
2. With fetched event, statechart select transitions from active states.
3. Execute selected transitions.
4. Select and execute auto transitions from active states, do several rounds.
5. With fetched event, statechart invoke reactions from active states.
6. If event queue is not cleared, fetch another event from queue, back to 2.

Here's several specification you shall follow:

- Do not manually call `Step` with node loop event, especially when a step is running.
- Do not call same event from a running step, this may cause unintended endless loop.
- Better not use `null` event or event with empty string, `Step` won't process. `null` event is used as auto transition internally.

| Property | Description |
| ---- | ---- |
| `int MaxAutoTransitionRound` | Max iteration rounds of selecting auto transitions in a single step. If `<=0` , statechart will ignore any auto transition. |
| `enum EventFlagEnum EventFlag` | Event flags to control node loop events (process, input, etc.) , which are all disabled by default. |

| Method | Description |
| ---- | ---- |
| `void Step(StringName)` | Make statechart run a step with given event. |

### State

This node works as "state" in common state machine system, while can be arranged in a tree structure, as hierarchical state machine do.

Beware, only a collection of states in the tree are active. Root state is no doubt always active. For the rest, they largely depend on their parent's "state mode": `Compound`, `Parallel`, or `History`. State mode determines how state deal with their child state (substate), and other behaviors when traversing state tree.

`Compound` is the default mode of a state:

- If it is active, then exactly 1 substate (non-history) will be active (if there's any), which is called current state.
- Initial state is the substate a compound will choose as current state by default. You can assign it in inspector, or the first substate (non-history) will be initial state (if there's any).

`Parallel` is kinda opposite to compund:

- If it is active, then all substate (non-history) will be active.

`History` is a special mode only used as transition's target.

- Never active.
- You can switch whether a history state is deep or not.

  - Shallow history only recovers parent's status. For compound parent, it will redirect transition target to the sibling once active till last exit of parent state. For parallel parent, it will redirect transition target to all the non-hisotry siblings.

  - A deep history recovers parent's status as well as descendants'. Redirecting process will be done recursively on redirected targets' substate.

- State may never been active before as a history state's parent. For compound parent, history will be redirected to parent's initial state. For parallel parent, history will be redirected to all non-history siblings.

To utilize full feature of a state, you can:

- Connect signals:

  - `Enter` emited when state is entered. This usually happens when state is set active by transitions or statechart initialization.
  - `Exit` emited when state is exit. This usually happens when state is set unactive by transitions.

- Add state, transition, reaction as child node.

Here's several specification you shall follow:

- Better not append substate to history: state, transition, reaction won't function as history's child. However, you may occasionally assign a history's substate as a transition target, which will cause unexpected behavior.
- Better not append shallow history to parallel. Targeting such history works, but targeting the parallel parent will do the same.

| Property | Description |
| ---- | ---- |
| `enum StateModeEnum StateMode` | Enumeration of state mode. |
| `bool IsDeepHistory` | Used in history mode only. |
| `State InitialState` | The substate that will be choosed as current state by default. If not assigned, first substate will be initial state (if there's any). Used in compound mode only. |
| `double Delta` | Recently updated delta time parsed from `_Process(double delta)` .  |
| `double PhysicsDelta` | Recently updated delta time parsed from `_PhysicsProcess(double delta)` .  |
| `InputEvent Input` | Recently updated input event parsed from `_Input(InputEvent @event)` .  |
| `InputEvent UnhandledInput` | Recently updated input event parsed from `_UnhandledInput(InputEvent @event)` .  |

| Signal | Description |
| ---- | ---- |
| `void Enter(State)` | Emited when state is entered. Parsed state is used to access delta time and input event when handling node loop events. |
| `void Exit(State)` | Emited when state is exit. Parsed state is used to access delta time and input event when handling node loop events. |

### Transition

This node represents a transition between state(s). Append it as child node of a non-history state, then this state will be treated as "source state" — the state we'll transit from. As for the target state(s) — the state(s) we'll transit to, assignment in inspector is required.

Source state and target states, , determines which of the states will.

| Signal | Description |
| ---- | ---- |
| `void Guard(Transition)` | Emited when transition is checked. Parsed transition is used to access delta time and input event when handling node loop events. |
| `void Invoke(Transition)` | Emited when transition is invoked. Parsed transition is used to access delta time and input event when handling node loop events. |

| Property | Description |
| ---- | ---- |
| `double Delta` | Recently updated delta time parsed from `_Process(double delta)` .  |
| `double PhysicsDelta` | Recently updated delta time parsed from `_PhysicsProcess(double delta)` .  |
| `InputEvent Input` | Recently updated input event parsed from `_Input(InputEvent @event)` .  |
| `InputEvent UnhandledInput` | Recently updated input event parsed from `_UnhandledInput(InputEvent @event)` .  |

### Reaction

| Signal | Description |
| ---- | ---- |
| `void Invoke(Reaction)` | Emited when reaction is invoked. Parsed reaction is used to access delta time and input event when handling node loop events. |

| Property | Description |
| ---- | ---- |
| `double Delta` | Recently updated delta time parsed from `_Process(double delta)` .  |
| `double PhysicsDelta` | Recently updated delta time parsed from `_PhysicsProcess(double delta)` .  |
| `InputEvent Input` | Recently updated input event parsed from `_Input(InputEvent @event)` .  |
| `InputEvent UnhandledInput` | Recently updated input event parsed from `_UnhandledInput(InputEvent @event)` .  |

## Todo

Statechart:

- add save/load

Example:

- test scene
- practical usage
