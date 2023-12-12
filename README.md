# statechart-sharp

 A simple statechart for Godot, implemented in C#.

---

## TODO

- Overall:
  
  - change to event driven
  - all stack no recursion
  - follow SCXML standard
  - add Save/Load feature

- Statechart:
  
  - queue event when running
  - parse event, rather than process/input etc.
  - replace state recursion with stack
  - add Save/Load method

- State:
  
  - remove "isInstant" flag
  - remove `SubstateTransit`
  - eliminate loop function (StateProcess/StateInput etc.)
  - enter/transition use document order, exit use reversed document order
  - add Save/Load method

- Transition:
  
  - The exit set of a set of transitions is the union of the exit sets of the individual transitions.
  - The entry set of a set of transitions is the union of the entry sets of the individual transitions.

- Component: shorten class name (parallelComponent/compondComponent)
