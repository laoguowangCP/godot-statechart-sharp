#if TOOLS
using Godot;
using System;

namespace LGWCP.StatechartSharp
{
    public partial class StatechartInspectorPlugin : EditorInspectorPlugin
    {
        public override bool _CanHandle(GodotObject @object)
        {
            // return @object is State;
            return false;
        }

        public override bool _ParseProperty(
            GodotObject @object,
            Variant.Type type,
            string name,
            PropertyHint hintType,
            string hintString,
            PropertyUsageFlags usageFlags,
            bool wide)
        {
            if (@object is State)
            {
                if (name == "StateMode")
                {
                    AddPropertyEditor(name, new StateModeEditor());
                    return true;
                }
            }

            return false;
        }
    }
}

#endif
