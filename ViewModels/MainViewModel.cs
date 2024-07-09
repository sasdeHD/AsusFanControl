using AsusFanControl.Infrastructure;
using AsusFanControl.Model;
using AsusFanControl.Model.Enums;
using AsusFanControl.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AsusFanControl.ViewModel
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private double? tempProcessor;
        private string? fanSpeed;
        private readonly List<int> temperatureList = new List<int>(10);
        private int fanPower;

        private AppDbContext db;
        private AppSetting setting;
        private Graph selectedGraph;

        private AsusService asusService;
        private GraphService graphService;
        private SettingService settingService;

        private CancellationTokenSource cancellationTokenSourceAutoFan;
        private CancellationTokenSource cancellationTokenSourceInfoFan;

        private RelayCommand setFanPowerCommand;
        private RelayCommand modeSelectedCommand;
        private RelayCommand switchFanControlStateCommand;
        private RelayCommand saveSettingCommand;


        public MainViewModel()
        {
            db = new AppDbContext();
            asusService = new AsusService();
            settingService = new SettingService(db);
            graphService = new GraphService(db);

            Setting = settingService.GetSetting();
            FanPower = Setting.FanPower;

            Graphs = new ObservableCollection<Graph>(graphService.GetGraphs());

            if (Setting.SelectedGraph != null)
            {
                SelectedGraph = Setting.SelectedGraph;
            }
            else
            {
                SelectedGraph = Graphs.First();
            }

            cancellationTokenSourceInfoFan = new CancellationTokenSource();
            cancellationTokenSourceAutoFan = new CancellationTokenSource();

            Task.Run(() => LoopMachineGetInfo(cancellationTokenSourceInfoFan.Token));
            Task.Run(() => LoopMachineAutoFan(cancellationTokenSourceInfoFan.Token));
        }

        public ObservableCollection<Graph> Graphs { get; set; }

        public Graph SelectedGraph
        {
            get => selectedGraph;
            set
            {
                SetProperty(ref selectedGraph, value);
                Setting.SelectedGraph = value;
            }
        }

        public AppSetting Setting { get => setting; set => SetProperty(ref setting, value); }

        public double? TempProcessor { get => tempProcessor; set => SetProperty(ref tempProcessor, value); }

        public string? FanSpeed { get => fanSpeed; set => SetProperty(ref fanSpeed, value); }

        public int FanPower { get => fanPower; set => SetProperty(ref fanPower, value); }

        public RelayCommand SetFanPowerCommand
        {
            get
            {
                return setFanPowerCommand ?? (setFanPowerCommand = new RelayCommand(ojb =>
                {
                    if (Setting.FanAutoState)
                    {
                        FanPower = Math.Min(fanPower, 99);
                        Setting.FanPower = FanPower;
                        asusService.SetFanSpeeds(FanPower);
                    }
                }));
            }
        }

        public RelayCommand SwitchFanControlStateCommand
        {
            get
            {
                return switchFanControlStateCommand ?? (switchFanControlStateCommand = new RelayCommand(obj =>
                {
                    if (Setting.FanAutoState)
                    {
                        FanPower = Setting.FanPower;
                        asusService.SetFanSpeeds(Setting.FanPower);
                    }
                }));
            }
        }

        public RelayCommand SaveSettingCommand
        {
            get
            {
                return saveSettingCommand ?? (saveSettingCommand = new RelayCommand(obj =>
                {
                    settingService.SaveSetting(Setting);
                }));
            }
        }

        public RelayCommand ModeSelectedCommand
        {
            get
            {
                return modeSelectedCommand ?? (modeSelectedCommand = new RelayCommand(obj =>
                {
                    var currentMode = (StatusMode)obj;
                    Setting.FanMode = currentMode;
                }));
            }
        }

        public void RefreshGraphs()
        {
            var currentGraph = SelectedGraph;
            Graphs.Clear();
            foreach (var graph in graphService.GetGraphs())
                Graphs.Add(graph);

            if (Graphs.Any(c => c.Id == currentGraph.Id) == false)
                SelectedGraph = Graphs.First();
            else
                SelectedGraph = currentGraph;
        }

        private async Task LoopMachineGetInfo(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (Setting.SensorInfoEnabled)
                {
                    FanSpeed = string.Join("/", asusService.GetFanSpeeds().Select(x => x.ToString()));
                    TempProcessor = asusService.Thermal_Read_Cpu_Temperature();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        private void UpdateTemperatureList(int newTemperature)
        {
            if (temperatureList.Count == 10)
            {
                temperatureList.RemoveAt(0);
            }
            temperatureList.Add(newTemperature);
        }

        private double CalculateAverageTemperature()
        {
            if (temperatureList.Count == 0)
            {
                return 0;
            }
            switch (Setting.FanMode)
            {
                case StatusMode.Soft:
                    return temperatureList.Average();
                case StatusMode.Middle:
                    return temperatureList.Take(5).Average();
                case StatusMode.Hard:
                    return temperatureList.First();
                default:
                    return temperatureList.Average();
            }
        }

        private async Task LoopMachineAutoFan(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (SelectedGraph is null || Setting.FanAutoState)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                    continue;
                }

                UpdateTemperatureList((int)asusService.Thermal_Read_Cpu_Temperature());
                double averageTemperature = CalculateAverageTemperature();
                var temperaturePoints = SelectedGraph.GraphPoints.ToList();
                var result = FindNearestPoints(temperaturePoints, averageTemperature);

                switch (Setting.FanMode)
                {
                    case StatusMode.Soft:
                        FanPower = SetFanSpeedInterpolated(result, averageTemperature, 1.0);
                        break;

                    case StatusMode.Middle:
                        FanPower = SetFanSpeedInterpolated(result, averageTemperature, 1.5);
                        break;

                    case StatusMode.Hard:
                        FanPower = SetFanSpeedFixed(result, averageTemperature);
                        break;

                    default:
                        break;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        private int SetFanSpeedInterpolated(Tuple<GraphPoint, GraphPoint> points, double averageTemperature, double speedMultiplier)
        {
            int speed;
            if (points.Item2 != null)
            {
                double x1 = points.Item1.X;
                double x2 = points.Item2.X;
                double y1 = points.Item1.Y;
                double y2 = points.Item2.Y;

                double percentage = (averageTemperature - x1) / (x2 - x1);

                double interpolatedFanSpeed = y1 + (y2 - y1) * percentage * speedMultiplier;
                speed = (int)Math.Min(interpolatedFanSpeed, 99); 
            }
            else
            {
                speed = (int)Math.Min(points.Item1.Y * speedMultiplier, 99);
            }

            asusService.SetFanSpeeds(speed);
            return speed;
        }

        private int SetFanSpeedFixed(Tuple<GraphPoint, GraphPoint> points, double averageTemperature)
        {
            int speed;
            if (points.Item2 != null)
            {
                double x1 = points.Item1.X;
                double x2 = points.Item2.X;
                double y1 = points.Item1.Y;
                double y2 = points.Item2.Y;

                double percentage = (averageTemperature - x1) / (x2 - x1);

                double interpolatedFanSpeed = y1 + (y2 - y1) * percentage * 2.0;
                speed = (int)Math.Min(interpolatedFanSpeed, 99);
            }
            else
            {
                speed = (int)Math.Min(points.Item1.Y * 2.0, 99);
            }

            asusService.SetFanSpeeds(speed);
            return speed;
        }

        public Tuple<GraphPoint, GraphPoint> FindNearestPoints(List<GraphPoint> points, double target)
        {
            GraphPoint closestPoint = new GraphPoint();
            double minDistance = double.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if ((points[i].X <= target && target <= points[i + 1].X) || (points[i + 1].X <= target && target <= points[i].X))
                {
                    return new Tuple<GraphPoint, GraphPoint>(points[i], points[i + 1]);
                }

                double distance1 = Math.Abs(target - points[i].X);
                if (distance1 < minDistance)
                {
                    minDistance = distance1;
                    closestPoint = points[i];
                }
            }

            return new Tuple<GraphPoint, GraphPoint>(closestPoint, null);
        }

        public void StopLoopMachines()
        {
            cancellationTokenSourceInfoFan?.Cancel();
            cancellationTokenSourceAutoFan?.Cancel();
        }

        public void Dispose()
        {
            StopLoopMachines();
            asusService.ChangeMode(FanMode.Auto);
            cancellationTokenSourceInfoFan?.Dispose();
            cancellationTokenSourceAutoFan?.Dispose();
        }
    }
}
