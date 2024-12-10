namespace LGWCP.Godot.StatechartSharp.Nodeless;

public interface IStatechartComposition<T>
    where T : IStatechartComposition<T>
{
    protected void _Setup();
}
