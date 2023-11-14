# state-chart-sharp

 A simple state chart for Godot, implemented in C#.

---

## TODO

- Overall: change to event driven, follow SCXML standard, add Save/Load feature
- StateChart:

  - queue event when running
  - parse event, rather than process/input etc.
  - add Save/Load method

- State:

  - move "isInstant" flag to `Transition`
  - rewrite `SubstateTransit`:

    - always check instant transition on enter, except self transit

  - eliminate loop function (stateProcess/stateImput etc) , self transition shall do the job
  - store Transitions with `Dictionary<StringName, List<Transition>>` , "_" string as "NULL" event.
  - exit order: "reversed document order", parallel should iterate substates reversely
  - add Save/Load method
  
- Transition:

  - add "isInstant" flag
  - replace "transitionMode" with "event" (`StringName`)

- Component: shorten class name (parallelComponent/compondComponent)
