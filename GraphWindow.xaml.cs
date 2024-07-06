using System.Windows;
using AsusFanControl.ViewModels;
using LiveChartsCore.SkiaSharpView.WPF;


namespace AsusFanControl
{
    public partial class GraphWindow : Window
    {
        private readonly GraphViewModel dataContext;

        public GraphWindow()
        {
            dataContext = new GraphViewModel();
            DataContext = dataContext;
            InitializeComponent();
        }

        private void CartesianChart_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var chart = sender as CartesianChart;
            if (chart is not null)
                dataContext.CartesianChart_MouseMove(chart, e);
        }

        private void CartesianChart_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var chart = sender as CartesianChart;
            if (chart is not null)
                dataContext.CartesianChart_MouseUp(chart, e);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
