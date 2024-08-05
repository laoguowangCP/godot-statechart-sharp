<p align="center">
  <img src="./docs/asset/StatechartLogo.svg" height="200px" />
</p>

# Statechart Sharp

 > A simple statechart plugin for Godot, implemented in C#.

- [Introduction](#introduction)
- [Quick Start](#quick-start)
- [Feature](#feature)

## Introduction

What is statechart?

- A state machine.
- Supports hierarchy states.
- Has various state mode.

This plugin provides basic nodes to build statechart in Godot editor.

## Quick Start

> [!IMPORTANT]
>
> .NET-enabled version of Godot is required.

Download repository, copy `addons/statechart_sharp` to your project folder. Build project once, then enable plugin in project setting. You'll see new nodes added to "create new node" interface:

<img src="./docs/asset/ss_imported_nodes.png" alt="ss_imported_nodes" style="width:400px;"/>

**Step 1** : Add nodes:

- Add Statechart

  <img src="./docs/asset/ss_add_statechart.png" alt="ss_add_statechart" style="width:256px;"/>

- Add State(s)

  <img src="./docs/asset/ss_add_states.png" alt="ss_add_state" style="width:256px;"/>

- Add Transition(s) and Reaction(s)

  <img src="./docs/asset/ss_add_transition_&_reaction.png" alt="ss_add_transition" style="width:256px;"/>

**Step 2** : Set properties and connect signals.

**Step 3** : Build and run.

Refer to [manual](./docs/manual.md) to see how these nodes work together. Get example scenes in `./statechart_sharp_example` folder.

## Feature

- Follow statechart specification:

  - Use document order.
  - Support automatic transition.

- Designed for Godot:

  - Node based.
  - Support node loop events (process, input, etc.).
  - Choose your style: extend scripts, or use signals only.
