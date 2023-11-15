# state-chart-sharp

 A simple state chart for Godot, implemented in C#.

---

## TODO

- Overall:

  - change to event driven
  - all stack no recursion
  - follow SCXML standard
  - add Save/Load feature

- StateChart:

  - queue event when running
  - parse event, rather than process/input etc.
  - replace state recursion with stack
  - add Save/Load method

- State:

  - remove "isInstant" flag
  - remove `SubstateTransit`
  - eliminate loop function (StateProcess/StateInput etc.)
  - store Transitions with `Dictionary<StringName, List<Transition>>` , "_" string as "NULL" event.
  - SCXML standard: enter/transition use document order, exit use reversed document order
  - add Save/Load method
  
- Transition:

  - add "isInstant" flag
  - replace "transitionMode" with "event" (`StringName`)

- Component: shorten class name (parallelComponent/compondComponent)
