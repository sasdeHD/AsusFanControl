using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model.Enums
{
    public enum LogLevel
    {
        [Description("TRACE")]
        Trace = 0,

        [Description("DEBUG")]
        Debug = 1,

        [Description("INFO")]
        Information = 2,

        [Description("WARNING")]
        Warning = 3,

        [Description("ERROR")]
        Error = 4,

        [Description("CRITICAL")]
        Critical = 5,

        [Description("")]
        None = 6
    }
}
