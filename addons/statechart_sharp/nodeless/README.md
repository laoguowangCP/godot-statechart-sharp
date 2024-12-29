<p align="center">
  <img src="./asset/StatechartNodelessLogo.svg" height="200px" />
</p>

# Statechart Sharp Nodeless

# TODO

# Scratch

## Initialization

```csharp
// Get statechart and builder
var (sc, sb) = Statechart<BaseStatechartDuct, string>.GetStatechartAndBuilder();

// Use build comp to config
{
    // Get some comps
    using var rootState = sb.GetState();
    using var s1 = sb.GetState();
    using var s2 = sb.GetState();
    using var t1 = sb.GetTransition();
    using var t2 = sb.GetTransition();
    using var a1 = sb.GetReaction();
    using var a2 = sb.GetReaction();

    // Builder duplicate comp
    using var a3 = sb.Duplicate(a1);

    // Config like node tree
    rootstate
        .Add(s1
            .Add(t1)
            .Add(a1))
        .Add(s2
            .Add(a2)
            .Add(t2))
    
    sb.Commit(sc, rootState);
}
```

Statechart commit procedure:

- Process build: transition promoter will change build comps
- Instance build comps to real comps
