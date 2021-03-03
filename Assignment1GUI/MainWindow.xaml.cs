using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Assignment1;
using System.Text.RegularExpressions;
using IGUIAdapter;
using SimpleWPFChart;

using System.Threading;
using System.Windows.Threading;

namespace Assignment1GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Adapter
    {
        private bool[] buttonVisibilities;
        private int _consoleRowCount = 0;
        private Program program;

        public MainWindow()
        {

            InitializeComponent();
            program = new Program(this);
            ResetButtonVisibilities(new bool[5] { true, false, false, false, false });
            FieldSelect.SelectedItem = FieldSelectDefault; 
        }

        // Method code adapted from:
        // https://stackoverflow.com/questions/7425618/how-can-i-add-a-hint-text-to-wpf-textbox
        // @ Mohammad Mahroz
        private void QueryBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (QueryField.Text.Length == 0)
            {
                QueryFieldHint.Visibility = Visibility.Visible;
                QueryField.Background = new SolidColorBrush(Colors.White) { Opacity = 0 };
            }
            else
            {
                QueryFieldHint.Visibility = Visibility.Hidden;
                QueryField.Background = new SolidColorBrush(Colors.White) { Opacity = 1 };
            }
        }

        private void OnComp1Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[0] = false;
            ResetButtonVisibilities(buttonVisibilities);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                AddConsoleMessage("Indexing full text documents, please wait a moment...", GUIColor.ERROR_COLOR);
                new Thread(()=> {
                    if (program.PerformFullIndexing(openFileDialog.FileName))
                    {
                        ResetButtonVisibilities(new bool[] { true, true, false, false, false });
                    }
                }).Start();
            }
        }


        private void OnComp2Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[0] = false;
            buttonVisibilities[1] = false;
            ResetButtonVisibilities(buttonVisibilities);
            AddConsoleMessage("Proccessing documents, this may take a few minutes...", GUIColor.ERROR_COLOR);
            new Thread(() =>
            {
                if (program.PerformTokenization())
                    ResetButtonVisibilities(new bool[] { true, false, true, false, false });
            }).Start();
        }

        private void OnComp3Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[0] = false;
            buttonVisibilities[2] = false;
            ResetButtonVisibilities(buttonVisibilities);
            AddConsoleMessage("Selecting Keywords for documents, this may take a few minutes...", GUIColor.ERROR_COLOR);
            new Thread(() =>
            {
                if (program.PerformKeywordSelection())
                    ResetButtonVisibilities(new bool[] { true, false, false, true, false });
            }).Start();
        }

        private void OnComp4Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[0] = false;
            buttonVisibilities[3] = false;
            ResetButtonVisibilities(buttonVisibilities);
            AddConsoleMessage("Stemming Keywords in documents, please wait a moment...", GUIColor.ERROR_COLOR);
            new Thread(() =>
            {
                if (program.PerformStemming())
                    ResetButtonVisibilities(new bool[] { true, false, false, false, true });
            }).Start();
        }

        private void OnComp5Click(object sender, RoutedEventArgs e)
        {
            string title = ((ComboBoxItem)FieldSelect.SelectedItem).Content.ToString();
            FieldName field = FieldNameEnum.FromString(title);
            MessageGrid.Children.Clear();
            _consoleRowCount = 0;
            var queryResults = program.PerformSearch(QueryField.Text, field);
            AddConsoleMessage("Query Results\n" + queryResults.Serialize());
            consoleViewer.ScrollToTop();
        }

        private void ResetButtonVisibilities(bool[] buttons)
        {
            Dispatcher.Invoke(() => 
            {
                buttonVisibilities = buttons;
                Comp1But.IsEnabled = buttonVisibilities[0];
                Comp2But.IsEnabled = buttonVisibilities[1];
                Comp3But.IsEnabled = buttonVisibilities[2];
                Comp4But.IsEnabled = buttonVisibilities[3];
                Comp5But.IsEnabled = buttonVisibilities[4];
                ZipfAnalysis.IsEnabled = buttonVisibilities[1];
            });
        }

        public void AddConsoleMessage(
            string message,
            GUIColor? forgroundColor = null,
            GUIColor? backgroundColor = null
            )
        {
            this.Dispatcher.Invoke(() =>
            {
                SolidColorBrush forground = GetBrush(forgroundColor, Brushes.Black),
                    background = GetBrush(backgroundColor, Brushes.White);

                RowDefinition row = new RowDefinition();
                TextBox textToAdd = new TextBox();
                textToAdd.FontFamily = new FontFamily("Consolas");
                textToAdd.Text = "> " + message;

                textToAdd.Background = background;
                textToAdd.Foreground = forground;
                textToAdd.FontStretch = FontStretches.UltraExpanded;
                textToAdd.TextAlignment = TextAlignment.Left;
                textToAdd.TextWrapping = TextWrapping.Wrap;

                textToAdd.Background = new SolidColorBrush(Colors.White) { Opacity = 0 };
                textToAdd.BorderThickness = new Thickness(0);
                textToAdd.IsReadOnly = true;
                textToAdd.TextWrapping = TextWrapping.Wrap;

                int rows = Regex.Matches(message, "\n").Count;
                if (rows == 0)
                    rows = 1;
                row.MinHeight = 20;
                MessageGrid.RowDefinitions.Add(row);
                MessageGrid.Children.Add(textToAdd);
                Grid.SetRow(textToAdd, _consoleRowCount++);
                consoleViewer.ScrollToBottom();
            });
        }

        private static SolidColorBrush GetBrush(GUIColor? colGUI, SolidColorBrush defaultBrush)
        {
            SolidColorBrush brush;
            if (colGUI == null)
            {
                brush = defaultBrush;
            }
            else
            {
                GUIColor fg = (GUIColor)colGUI;
                Color col = new Color();
                col.R = Convert.ToByte(fg.R);
                col.G = Convert.ToByte(fg.G);
                col.B = Convert.ToByte(fg.B);
                col.A = Convert.ToByte(255);
                brush = new SolidColorBrush(col);
            }
            return brush;
        }


        private void OnZipfAnalysis(object sender, RoutedEventArgs e)
        {
            program.RunZipfsSelectionAnalysis();
        }

        public void SetChart(Tuple<float[], float[], string> input)
        {
            SimpleWPFChart.MainWindow chart = new SimpleWPFChart.MainWindow(input.Item3);
            chart.SetChart(input);
            chart.Show();
        }
    }
}
