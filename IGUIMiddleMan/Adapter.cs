using System;

namespace IGUIAdapter
{
    public interface Adapter
    {
        public void AddConsoleMessage(
            string message,
            GUIColor? foregroundColor = null,
            GUIColor? backgroundColor = null
            );
    }
}
