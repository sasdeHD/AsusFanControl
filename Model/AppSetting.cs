using AsusFanControl.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model
{
    [Table("Setting")]
    public class AppSetting
    {
        public int Id { get; set; }
        public int FanPower { get; set; }

        public bool FanAutoState { get; set; }
        public bool SensorInfoEnabled { get; set; }

        public long SelectedGraphId { get; set; }
        public virtual Graph? SelectedGraph { get; set; }

        public StatusMode FanMode { get; set; } = StatusMode.Soft;
    }
}
