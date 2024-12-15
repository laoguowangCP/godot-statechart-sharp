namespace LGWCP.Godot.StatechartSharp.Nodeless;


public interface IStatechartDuct
{
    public void SetTransitionEnabled();
}

public abstract class BaseStatechartDuct : IStatechartDuct
{
    protected bool IsTransitionEnabled = false;
    public void SetTransitionEnabled()
    {
        IsTransitionEnabled = true;
    }
}

