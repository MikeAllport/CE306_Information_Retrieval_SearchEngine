﻿using System;
using System.Collections.Generic;
namespace IGUIAdapter
{
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
