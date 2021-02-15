using System;

namespace IGUIAdapter
{
    public interface Adapter
    {
        void AddConsoleMessage(
            string message,
            GUIColor? foregroundColor = null,
            GUIColor? backgroundColor = null
            );
    }
}
