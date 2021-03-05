using System;
using System.Collections.Generic;
namespace IGUIAdapter
{
    /// <summary>
    /// Adapter is a class allowing the Program to interface with the GUI to add console messages
    /// and export chart data allowing for non infinite recursive inclusions with 'using' keyword
    /// </summary>
    public interface Adapter
    {
        public void AddConsoleMessage(
            string message,
            GUIColor? foregroundColor = null,
            GUIColor? backgroundColor = null
            );

        public void SetChart(Tuple<float[], float[], string> inputs);
    }
}
