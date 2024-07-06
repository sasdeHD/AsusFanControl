using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model
{
    public class Graph
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<GraphPoint> GraphPoints { get; set; }
    }
}
