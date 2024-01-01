<p align="center">
  <img src="./addons/statechart_sharp/icon/Statechart.svg" height="200px" />
</p>

# Statechart Sharp

 > A simple statechart plugin for Godot, implemented in C#.

## Table of Content

- [Introduction](#introduction)
- [Start](#start)
- [Specification](#specification)
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

### Step 1 : Add <img src="./addons/statechart_sharp/icon/Statechart.svg" alt="Statechart" style="width:16px;" align="bottom"/> Statechart node

### Step 2 : Add <img src="./addons/statechart_sharp/icon/State.svg" style="width:16px;" alt="State" align="bottom"/> State node

- A root state, as child of a statechart.
- Add substates under root state.
- Switch state mode:

  - Compond: active 1 substate.
  - Parallel: active all substate.
  - Hisotry: history of parent state, used in transition.

- Connect signals:

  - Enter: called when entering state.
  - Exit: called when exiting state.

### Step 3 : Add <img src="./addons/statechart_sharp/icon/Transition.svg" style="width:16px;" alt="Transition" align="bottom"/> Transition node

- As child of a state, where we exit from.
- Assign target states: where we enter to.
- Switch event: choose when transition happens.

  - Node loop event: process, input, etc.
  - Custom event: assign a name for it.
  - Auto: happens after another transition happens.

- Connect signals:

  - Guard: called to check if transition is available.
  - Invoke: called when transition happens.

### Step 4 : Add <img src="./addons/statechart_sharp/icon/Reaction.svg" style="width:16px;" alt="Action" align="bottom"/> Reaction node

- As child of a state.
- Switch event: choose when reaction will be made.

  - Node loop event: process, input, etc.
  - Custom event: assign a name for it.

- Connect signals:

  - Invoke: called when reaction is made.

### Step 5 : Build and run

- Statechart will step loop events through states as long as it is not disabled or paused.
- To step a custom event, call `Statechart.Step(StringName)` .

## Specification

## Todo

Statechart:

- add save/load
- loop event bitmask

Example:

- test scene
- practical usage

Helper:

- readme specification
- editor annotation
