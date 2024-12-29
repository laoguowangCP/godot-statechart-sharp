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
    using var rootState = sb.GetState();
    using var s1 = sb.GetState();
    using var s2 = sb.GetState();
    using var t1 = sb.GetTransition();
    using var t2 = sb.GetTransition();
    using var a1 = sb.GetReaction();
    using var a2 = sb.GetReaction();

    rootstate.Add(s1)
}
```
