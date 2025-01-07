using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;


public class TransitionInt<TDuct, TEvent> : Composition<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected delegate void GuardEvent(TDuct duct);
    protected event GuardEvent Guard;
    protected delegate void InvokeEvent(TDuct duct);
    protected event InvokeEvent Invoke;

    public TEvent Event;
    protected List<StateInt<TDuct, TEvent>> TargetStates;
    public StateInt<TDuct, TEvent> SourceState;
    public StateInt<TDuct, TEvent> LcaState;
    public SortedSet<StateInt<TDuct, TEvent>> EnterRegion;
    protected SortedSet<StateInt<TDuct, TEvent>> EnterRegionEdge;
    protected SortedSet<StateInt<TDuct, TEvent>> DeducedEnterStates;
    public bool IsTargetless;

    public void TransitionInvoke(TDuct duct)
    {
        Invoke.Invoke(duct);
    }

    public SortedSet<StateInt<TDuct, TEvent>> GetDeducedEnterStates()
    {
        DeducedEnterStates.Clear();
        foreach (var edgeState in EnterRegionEdge)
        {
            edgeState.DeduceDescendants(DeducedEnterStates, false, true);
        }
        return DeducedEnterStates;
    }
}
