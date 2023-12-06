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
  
  - add "isAuto" flag (eventless, check on enter)
  - replace "transitionMode" with "event" (`StringName`)
  - multiple targets: check targets have parallel least-common-ancestor

- Component: shorten class name (parallelComponent/compondComponent)

## Pseudo code for `HandleTransition(event)`

- Prepare:
  
  1. prev_active_states BACKUPS active_states

- Do transitions: while not STOPPED:

	1. Initially, iterate active-states, find available transition
    2. Transition found with LCA and ENTERSET:
    	a. Prune LCA's subtree from active-states
        b. Insert states under LCA to active-states
	3. ENTERSET is cleared, LCA subtree is finished, continue iterate active-states
    4. Auto transition triggered when traversing LCA's subtree with new LCA and ENTERSET:
    	a. prune