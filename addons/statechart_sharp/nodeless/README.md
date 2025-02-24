<p align="center">
  <img src="./asset/StatechartNodelessLogo.svg" height="200px" />
</p>

# Statechart Sharp Nodeless

## What's the difference

- Compositions are not nodes.
- Use action delegates, may not flexible as signals, but faster.
- Build statechart with code.
- No builtin event steps (process, input, etc.).
- `StatechartSnapshot` is not `Resource`.
- Faster (maybe)

## Build statechart

You need code like this:

```csharp
// create statechart
var sc = new Statechart<StatechartDuct, string>();

// you'd like to mess around your naming in local scope
{
    // get some comps
    var root = sc.GetParallel();
    var x = sc.GetCompound();
    var xa = sc.GetCompound();
    var xb = sc.GetCompound();
    var y = sc.GetCompound();

    var xa_2_xb = sc.GetTransition("go", targetStates: new[] { xb } );

    var x_r = sc.GetReaction("go");

    // build
    root
        .Append(x
            .SetInitialState(xb)
            .Append(xa
                .Append(xa_2_xb))
            .Append(xb)
            .Append(x_r))
        .Append(y);

    // get ready
    sc.Ready(rootState: root);
}

```

Check out `statechart_sharp_example/test/test_nodeless/TestNodeless.cs` , i built many statecharts here to test.

## Get compositions from statechart

Statechart is generic now. `TDuct` is statechart duct type. You can extend it, add "delta time" or "input" if you like. Think it as "blackboard".  `TEvent` is event type.

You can step, save, load as usual.

For convenience, you can get different compositions from statechart.

**Statechart<TDuct, TEvent>.Compound GetCompound(Action<TDuct>[] enters = null, Action<TDuct>[] exits = null, Statechart<TDuct, TEvent>.State initialState = null)**: get a compound state.

**Statechart<TDuct, TEvent>.Parallel GetParallel(Action<TDuct>[] enters = null, Action<TDuct>[] exits = null)**: get a parallel state.

**Statechart<TDuct, TEvent>.History GetHistory()**: get a history state.

**Statechart<TDuct, TEvent>.DeepHistory GetDeepHistory()**: get a deep history state.

**Statechart<TDuct, TEvent>.Transition GetTransition(TEvent @event, Action<TDuct>[] guards = null, Action<TDuct>[] invokes = null, Statechart<TDuct, TEvent>.State[] targetStates = null)**: get a transition (not auto).

**Statechart<TDuct, TEvent>.Transition GetAutoTransition(Action<TDuct>[] guards = null, Action<TDuct>[] invokes = null, Statechart<TDuct, TEvent>.State[] targetStates = null)**: get an auto transition.

**Statechart<TDuct, TEvent>.Reaction GetTransition(TEvent @event, Action<TDuct>[] invokes = null)**: get a reaction.

## Duplicate composition



