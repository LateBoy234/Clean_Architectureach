using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmpClean.Presentation.Model
{
    internal class MeasurementData
    {
        public List<float> MeasurementItems { get; set; } = new List<float>();

        public int index { get; set; }
    }
}
