using AsusFanControl.Infrastructure;
using AsusFanControl.Model;
using LiveChartsCore.Defaults;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace AsusFanControl.Service
{
    public class GraphService
    {
        private AppDbContext db;

        public GraphService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<Graph> CreateGraph(string Name)
        {
            var graphPoints = ObservablePointToGraphPoint(DefaultPoints());

            var graphCreate = new Graph()
            {
                Name = Name,
                GraphPoints = graphPoints
            };

            await db.Graph.AddAsync(graphCreate);
            await db.SaveChangesAsync();
            return graphCreate;
        }

        public List<Graph> GetGraphs()
        {
            return db.Graph.Include(g => g.GraphPoints).ToList();
        }

        public async Task<Graph> SaveChanges(ObservableCollection<ObservablePoint> points, Graph graph)
        {
            var graphPoints = graph.GraphPoints.ToList();
            for (int i = 0; i < points.Count; i++)
            {
                ObservablePoint? point = points[i];
                graphPoints[i].X = point.X.Value;
                graphPoints[i].Y = point.Y.Value;
            }
            db.Graph.Update(graph);
            await db.SaveChangesAsync();
            return graph;
        }

        public async Task<bool> Remove(Graph graph)
        {
            var graphDelete = await db.Graph.FirstOrDefaultAsync(g => g.Id == graph.Id);
            if (graphDelete != null && await db.Graph.CountAsync() > 1)
            {
                db.Graph.Remove(graphDelete);

                var setting = db.Setting.Single();
                setting.SelectedGraph = GetGraphs().First();
                await db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private ObservableCollection<ObservablePoint> DefaultPoints()
        {
            return new ObservableCollection<ObservablePoint>
            {
                new (30,30),
                new (40,40),
                new (57,45),
                new (64,56),
                new (70,60),
                new (80,80),
                new (90,95),
                new (100,96),
            };
        }

        public ObservableCollection<ObservablePoint> GraphPointToObservablePoint(Graph graph)
        {
            var points = new ObservableCollection<ObservablePoint>();
            foreach (var point in graph.GraphPoints)
                points.Add(new ObservablePoint(point.X, point.Y));

            return points;
        }

        private List<GraphPoint> ObservablePointToGraphPoint(ObservableCollection<ObservablePoint> points)
        {
            var graphPoints = new List<GraphPoint>();
            foreach (var point in points)
                graphPoints.Add(new GraphPoint(point.X.Value, point.Y.Value));

            return graphPoints;
        }
    }
}
