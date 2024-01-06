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

### Statechart <img src="./addons/statechart_sharp/icon/Statechart.svg" alt="Statechart" style="width:16px;" align="bottom"/>

The control node of whole statechart. You can simply take it as "state machine node" as in common state machine system. To make it work properly, add exactly 1 child `State` node as `RootState` , which is always active.

Statechart provides `Step` method, which is the sole way we interact with statechart. To be specific, node loop event also calls `Step` method, the only difference is that they use built-in event string, defined in `util/StatechartConfig.cs` .

During the `Step` method, statechart will do following things:

1. Queue event, if statechart is already running a step. If not the case, statechart will fetch event from queue, and continues following tasks.
2. With fetched event, statechart select transitions from active states.
3. Execute selected transitions.
4. Select and execute auto transitions from active states, do several rounds.
5. With fetched event, statechart invoke reactions from active states.
6. If event queue is not cleared, fetch another event from queue, back to 2.

Here's several specification you shall follow:

- Do not manually call `Step` with node loop event, especially when a step is running.
- Do not call same event as a step is running, this may cause unintended endless loop.

| Property | Description |
| ---- | ---- |
| `int MaxAutoTransitionRound` | Max iteration rounds of selecting auto transitions in a single step. If `MaxAutoTransitionRound<=0` , statechart will ignore any auto transition. |

| Method | Description |
| ---- | ---- |
| `void Step(StringName eventName)` | Step statechart with event. |


### State <img src="./addons/statechart_sharp/icon/State.svg" alt="Statechart" style="width:16px;" align="bottom"/>


| Property | Description |
| ---- | ---- |
|  |


### Transition <img src="./addons/statechart_sharp/icon/Transition.svg" alt="Statechart" style="width:16px;" align="bottom"/>

### Reaction <img src="./addons/statechart_sharp/icon/Reaction.svg" alt="Statechart" style="width:16px;" align="bottom"/>

## Todo

Statechart:

- add save/load
- loop event flags

Example:

- test scene
- practical usage
