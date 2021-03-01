﻿using System;
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
        private bool[] buttonVisibilities = new bool[5] { true, false, false, false, false };
        private int _consoleRowCount = 0;
        private Program program;
        Application ownerApp;
        public delegate void PerformActionNoArg();
        public delegate void PerformAction1Arg(string arg);
        public MainWindow()
        {

            InitializeComponent();
            program = new Program(this);
            ResetButtonVisibilities();
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                buttonVisibilities[1] = true;
                ResetButtonVisibilities();
                AddConsoleMessage("Indexing full text documents, please wait a moment...", GUIColor.ERROR_COLOR);
                DoAction(program.PerformFullIndexing, openFileDialog.FileName);
            }
        }


        private void OnComp2Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[1] = false;
            buttonVisibilities[2] = true;
            ResetButtonVisibilities();
            AddConsoleMessage("Proccessing documents, this may take a few minutes...", GUIColor.ERROR_COLOR);
            DoAction(program.PerformTokenization);
        }

        private void OnComp3Click(object sender, RoutedEventArgs e)
        {            
            buttonVisibilities[2] = false;
            buttonVisibilities[3] = true;
            ResetButtonVisibilities();
            AddConsoleMessage("Selecting Keywords for documents, this may take a few minutes...", GUIColor.ERROR_COLOR);
            DoAction(program.PerformKeywordSelection);

        }

        private void OnComp4Click(object sender, RoutedEventArgs e)
        {
            buttonVisibilities[3] = false;
            buttonVisibilities[4] = true;
            ResetButtonVisibilities();
            AddConsoleMessage("Stemming Keywords in documents, please wait a moment...", GUIColor.ERROR_COLOR);
            DoAction(program.PerformStemming);
        }

        private void OnComp5Click(object sender, RoutedEventArgs e)
        {
            if (QueryField.Text.Length == 0)
                return;

            program.PerformSearch(QueryField.Text, FieldName.CAST);
        }

        private void ResetButtonVisibilities()
        {
            Comp1But.IsEnabled = buttonVisibilities[0];
            Comp2But.IsEnabled = buttonVisibilities[1];
            Comp3But.IsEnabled = buttonVisibilities[2];
            Comp4But.IsEnabled = buttonVisibilities[3];
            ZipfAnalysis.IsEnabled = buttonVisibilities[2];
        }

        // Allows gui repaint during slow loading actions
        // Code adapted from:
        // https://stackoverflow.com/questions/818911/force-a-wpf-control-to-refresh
        // @Remco
        private void DoAction(PerformActionNoArg method)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(50);
                try
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                        new Action(() =>
                        {
                            method();
                        }));
                }
                catch { }
            }));
            thread.Start();
        }

        // Allows gui repaint during slow loading actions
        // Code adapted from:
        // https://stackoverflow.com/questions/818911/force-a-wpf-control-to-refresh
        // @Remco
        private void DoAction(PerformAction1Arg method, string arg)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(50); // this is important ...
                try
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                        new Action(()=>
                        {
                            method(arg);
                        }));
                }
                catch { }
            }));
            thread.Start();
        }

        public void AddConsoleMessage(
            string message,
            GUIColor? forgroundColor = null,
            GUIColor? backgroundColor = null
            )
        {
            SolidColorBrush forground = GetBrush(forgroundColor, Brushes.Black), 
                background = GetBrush(backgroundColor, Brushes.White);

            RowDefinition row = new RowDefinition();
            TextBlock textToAdd = new TextBlock();
            textToAdd.FontFamily = new FontFamily("Consolas");
            textToAdd.Text = "> " + message;

            textToAdd.Background = background;
            textToAdd.Foreground = forground;
            textToAdd.FontStretch = FontStretches.UltraExpanded;
            textToAdd.TextAlignment = TextAlignment.Left;
            textToAdd.TextWrapping = TextWrapping.Wrap;

            int rows = Regex.Matches(message, "\n").Count;
            if (rows == 0)
                rows = 1;
            row.MinHeight = 20;
            MessageGrid.RowDefinitions.Add(row);
            MessageGrid.Children.Add(textToAdd);
            Grid.SetRow(textToAdd, _consoleRowCount++);
            consoleViewer.ScrollToBottom();
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
