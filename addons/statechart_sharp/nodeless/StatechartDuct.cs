namespace LGWCP.Godot.StatechartSharp.Nodeless;


public interface IStatechartDuct
{
    public void SetTransitionEnabled();
    public void SetTransitionDisabled();
    public bool IsTransitionEnabled();
}

public abstract class StatechartDuct : IStatechartDuct
{
    public abstract bool IsTransitionEnabled();

    public abstract void SetTransitionDisabled();

    public abstract void SetTransitionEnabled();
}

public class BaseStatechartDuct : StatechartDuct
{
    protected bool IsEnabled = false;

    public BaseStatechartDuct() {}

    public override void SetTransitionEnabled()
    {
        IsEnabled = true;
    }

    public override void SetTransitionDisabled()
    {
        IsEnabled = false;
    }

    public override bool IsTransitionEnabled()
    {
        return IsEnabled;
    }
}

