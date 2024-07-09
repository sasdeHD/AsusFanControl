using AsusFanControl.Infrastructure;
using AsusFanControl.Model;
using AsusFanControl.ViewModel;
using AsusFanControl.ViewModels;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using H.NotifyIcon.EfficiencyMode;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AsusFanControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RelayCommand showHideCommand;
        private GraphWindow? graphWindow;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
            SystemTrayIcon.DoubleClickCommand = ShowHideCommand;
        }

        private void Slider_Complete(object sender, MouseButtonEventArgs e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                var viewModel = slider.DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.SetFanPowerCommand.Execute(null);
                }
            }
        }

        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Button_Click_Closed(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_Graph_Form(object sender, RoutedEventArgs e)
        {
            graphWindow = new GraphWindow();
            graphWindow.Closed += GrahpViewWindowClosed;
            graphWindow.Show();
        }

        private void GrahpViewWindowClosed(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.RefreshGraphs();

            graphWindow = null;
        }

        private void StatusModeListBox_Selected(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                var listBox = sender as ListBox;
                viewModel.ModeSelectedCommand.Execute(listBox.SelectedIndex);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusModeListBox.Visibility == Visibility.Collapsed)
            {
                StatusModeListBox.Visibility = Visibility.Visible;
            }
            else
            {
                StatusModeListBox.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel is not null)
                viewModel.Dispose();

            if (graphWindow != null)
                graphWindow.Close();
            base.OnClosed(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            showHideCommand.Execute(null);
        }

        private RelayCommand ShowHideCommand
        {
            get
            {
                return showHideCommand ?? (showHideCommand = new RelayCommand(obj =>
                {
                    if (Visibility == Visibility.Visible)
                        Hide();
                    else
                    {
                        WindowState = WindowState.Normal;
                        Topmost = true;
                        Show();
                        Topmost = false;
                    }
                }));
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }
    }
}