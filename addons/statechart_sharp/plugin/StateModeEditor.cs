#if TOOLS
using Godot;
using System;

namespace LGWCP.StatechartSharp
{
    public partial class StateModeEditor : EditorProperty
    {
        private OptionButton PropertyControl = new();
        private StateModeEnum CurrentStateMode = StateModeEnum.Compond;
        private bool IsUpdating = false;
        public StateModeEditor()
        {
            PropertyControl.AddIconItem(null, "Compond State");
            PropertyControl.AddIconItem(null, "Parallel State");
            PropertyControl.AddIconItem(null, "History State");

            PropertyControl.ItemSelected += OnItemSelected;

            // Register property
            AddChild(PropertyControl);
            AddFocusable(PropertyControl);
        }

        private void OnItemSelected(long index)
        {
            // Ignore the signal if the property is currently being updated.
            if (IsUpdating)
            {
                return;
            }

            // Generate a new random integer between 0 and 99.
            switch (index)
            {
                case 0:
                    CurrentStateMode = StateModeEnum.Compond;
                    break;
                case 1:
                    CurrentStateMode = StateModeEnum.Parallel;
                    break;
                case 2:
                    CurrentStateMode = StateModeEnum.History;
                    break;
            }
            GD.Print(CurrentStateMode);
            // EmitChanged(GetEditedProperty(), CurrentStateMode);
        }
    }
}

#endif
