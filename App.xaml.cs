using AsusFanControl.Infrastructure;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using AsusFanControl.Model;

namespace AsusFanControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Database.EnsureCreated();

                    if (db.Setting.Any() == false)
                    {
                        var graph = new Graph()
                        {
                            Name = "По умолчанию",
                            GraphPoints = new List<GraphPoint>
                        {
                            new (30,30),
                            new (40,40),
                            new (57,45),
                            new (64,56),
                            new (70,60),
                            new (80,80),
                            new (90,95),
                            new (100,96),
                        }
                        };

                        db.Graph.Add(graph);

                        var setting = new AppSetting()
                        {
                            SensorInfoEnabled = true,
                            FanAutoState = true,
                            FanMode = Model.Enums.StatusMode.Soft,
                            FanPower = 50,
                            SelectedGraph = graph
                        };

                        db.Setting.Add(setting);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            base.OnStartup(e);
        }
    }
}
