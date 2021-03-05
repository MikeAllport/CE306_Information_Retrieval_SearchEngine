using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;

namespace SimpleWPFChart
{
    /// <summary>
    /// This is a brief window class which enables drawing of charts
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string title)
        {
            this.Title = title;
            InitializeComponent();
        }

        /// <summary>
        /// Creates the chart in the window from a given dataset of x/y/title list inputs
        /// </summary>
        /// <param name="inputs">The dataset to be drawn to a graph></param>
        public void SetChart(Tuple<float[], float[], string> inputs)
        {
            int datasetIndex = 0;
            LineSeries series = new LineSeries();
            series.DependentValuePath = "Value";
            series.IndependentValuePath = "Key";
            KeyValuePair<float, float>[] values;
            if (inputs.Item1.Length > 1000)
            {
                values = new KeyValuePair<float, float>[inputs.Item1.Length / 10];
                for (int i = 0; i < inputs.Item1.Length / 10; i++)
                {
                    values[i] = new KeyValuePair<float, float>(inputs.Item1[i], inputs.Item2[i]);
                    // because DataVisualization module has extremely high memory usage with large charts
                }
            }
            else
            {
                values = new KeyValuePair<float, float>[inputs.Item1.Length];
                for (int i = 0; i < inputs.Item1.Length; i++)
                {
                    values[i] = new KeyValuePair<float, float>(inputs.Item1[i], inputs.Item2[i]);
                    // because DataVisualization module has extremely high memory usage with large charts
                }
            }
            series.ItemsSource = values;
            series.Title = inputs.Item3;
            Chart.Series.Add(series);
            Chart.Title = inputs.Item3;
        }
    }
}
