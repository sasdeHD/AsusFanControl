using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model
{
    public class GraphPoint
    {
        public GraphPoint()
        {

        }

        public GraphPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int GraphId { get; set; }
        public Graph Graph { get; set; }
    }
}
