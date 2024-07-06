using AsusFanControl.Infrastructure;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Kernel.Sketches;
using System.Windows.Input;
using AsusFanControl.Model;
using AsusFanControl.Service;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace AsusFanControl.ViewModels
{
    public class GraphViewModel : ObservableObject
    {
        private readonly AppDbContext db;

        private GraphService graphService;

        private ObservableCollection<LineSeries<ObservablePoint, RectangleGeometry>> series;
        private ObservableCollection<ObservablePoint> seriesPoints;

        private string nameGraph;
        private string error;
        private bool isOpenDialog;

        private Graph selectedGraph;
        private ObservablePoint selectItem;

        private RelayCommand saveCommand;
        private RelayCommand createCommand;
        private RelayCommand undoCommand;
        private RelayCommand removeCommand;

        public GraphViewModel()
        {
            db = new AppDbContext();
            graphService = new GraphService(db);

            Graphs = new ObservableCollection<Graph>(graphService.GetGraphs());
            SeriesPoints = graphService.GraphPointToObservablePoint(Graphs.First());

            var lineSeries = new LineSeries<ObservablePoint, RectangleGeometry>
            {
                Values = seriesPoints,
                Fill = null,
                LineSmoothness = 0,
                YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Coordinate.PrimaryValue}% / {chartPoint.Coordinate.SecondaryValue}°C",
                AnimationsSpeed = TimeSpan.FromMilliseconds(200)
            };

            Series = new ObservableCollection<LineSeries<ObservablePoint, RectangleGeometry>>
            {
                lineSeries
            };

            lineSeries.ChartPointPointerDown += Series_ChartPointPointerDown;
            SelectedGraph = Graphs.First();
        }

        public ObservableCollection<ObservablePoint> SeriesPoints { get => seriesPoints; set => SetProperty(ref seriesPoints, value); }

        public ObservableCollection<LineSeries<ObservablePoint, RectangleGeometry>> Series { get => series; set => SetProperty(ref series, value); }

        public ObservablePoint? SelectItem { get => selectItem; set => SetProperty(ref selectItem, value); }

        public ObservableCollection<Graph> Graphs { get; set; }

        public Graph SelectedGraph
        {
            get => selectedGraph;
            set
            {
                SetProperty(ref selectedGraph, value);
                RefreshPoints();
            }
        }

        public List<Axis> XAxis { get; set; } = new List<Axis>()
        {
            new Axis()
            {
                MaxLimit = 115,
                MinLimit = 25,
                MinStep = 10,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#f0f0f0"), 0)
            }
        };

        public List<Axis> YAxis { get; set; } = new List<Axis>()
        {
            new Axis()
            {
                MaxLimit = 105,
                MinLimit = 5,
                MinStep = 10
            }
        };

        public bool IsOpenDialog { get => isOpenDialog; set => SetProperty(ref isOpenDialog, value); }

        public LabelVisual Title { get; set; } =
            new LabelVisual
            {
                Text = "График управление мощности кулеров от температуры ПК",
                TextSize = 25,
                Padding = new Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

        public string NameGraph
        {
            get => nameGraph;
            set
            {
                SetProperty(ref nameGraph, value);
                SetProperty(ref error, value);
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(async obj =>
                {
                    var graph = await graphService.SaveChanges(SeriesPoints, SelectedGraph);
                    var graphInCollection = Graphs.First(x => x.Id == graph.Id);

                    graphInCollection.Name = graph.Name;
                    graphInCollection.GraphPoints = graph.GraphPoints;

                    SelectedGraph = graph;

                    MessageBox.Show("Точки сохраннены у графика");
                }));
            }
        }

        public RelayCommand CreateCommand
        {
            get
            {
                return createCommand ?? (createCommand = new RelayCommand(async _ =>
                {
                    if (string.IsNullOrWhiteSpace(NameGraph))
                    {
                        MessageBox.Show("Нельзя создать пустой график!");
                        return;
                    }
                    if (Graphs.Any(x => x.Name == NameGraph))
                    {
                        MessageBox.Show("Имя занято!");
                        return;
                    }
                    var graph = await graphService.CreateGraph(NameGraph);
                    IsOpenDialog = false;
                    Graphs.Add(graph);
                }));
            }
        }

        public RelayCommand UndoCommand
        {
            get
            {
                return undoCommand ?? (undoCommand = new RelayCommand(async obj =>
                {
                    RefreshPoints();
                }));
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ?? (removeCommand = new RelayCommand(async obj =>
                {
                    if (await graphService.Remove(SelectedGraph))
                    {
                        Graphs.Remove(SelectedGraph);
                        SelectedGraph = Graphs.First();
                        MessageBox.Show("График удален");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка! Не найден график или вы пытайтесь удалить последний график!");
                    }
                }));
            }
        }

        private void RefreshPoints()
        {
            if (SelectedGraph is null)
                return;

            var graphPoints = SelectedGraph.GraphPoints.ToList();
            for (int i = 0; i < graphPoints.Count; i++)
            {
                ObservablePoint? point = SeriesPoints[i];
                point.X = graphPoints[i].X;
                point.Y = graphPoints[i].Y;
            }
        }

        public void CartesianChart_MouseMove(CartesianChart chart, MouseEventArgs e)
        {
            if (SelectItem == null || e.LeftButton == MouseButtonState.Pressed == false)
            {
                return;
            }

            var mousePosition = e.GetPosition(chart);
            var dataCoordinates = chart.ScalePixelsToData(new LvcPointD(mousePosition.X, mousePosition.Y));

            dataCoordinates.X = Math.Round(dataCoordinates.X);
            dataCoordinates.Y = Math.Round(dataCoordinates.Y);

            var maxX = 30 + (10 * (1 + SelectItem.MetaData.EntityIndex));
            var minX = 30 + (10 * (1 + SelectItem.MetaData.EntityIndex - 1));

            dataCoordinates.X = dataCoordinates.X <= 30 ? 30 : dataCoordinates.X;
            dataCoordinates.Y = dataCoordinates.Y <= 10 ? 10 : dataCoordinates.Y;

            dataCoordinates.X = dataCoordinates.X >= 110 ? 110 : dataCoordinates.X;
            dataCoordinates.Y = dataCoordinates.Y >= 100 ? 100 : dataCoordinates.Y;

            dataCoordinates.X = dataCoordinates.X > maxX ? maxX : dataCoordinates.X;
            dataCoordinates.X = dataCoordinates.X < minX ? minX : dataCoordinates.X;

            SelectItem.X = dataCoordinates.X;
            SelectItem.Y = dataCoordinates.Y;

            return;
        }

        public void CartesianChart_MouseUp(CartesianChart chart, MouseButtonEventArgs e)
        {
            if (SelectItem is not null)
                MinAndMaxPostionPoints(SelectItem);
            SelectItem = null;
        }

        private async void Series_ChartPointPointerDown(IChartView chart, LiveChartsCore.Kernel.ChartPoint<ObservablePoint, RectangleGeometry, LabelGeometry>? point)
        {
            var value = seriesPoints.First(v => v.MetaData == point.Model.MetaData);
            if (value != null)
            {
                selectItem = value;
            }
        }

        private void MinAndMaxPostionPoints(ObservablePoint selectItem)
        {
            foreach (var point in seriesPoints.Take(selectItem.MetaData.EntityIndex))
                point.Y = selectItem.Y < point.Y ? selectItem.Y : point.Y;

            foreach (var point in seriesPoints.Skip(selectItem.MetaData.EntityIndex))
                point.Y = selectItem.Y > point.Y ? selectItem.Y : point.Y;
        }
    }
}
