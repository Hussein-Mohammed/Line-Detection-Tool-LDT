using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fibers_Density_Estimater.Headers
{
    public class Temp_Parameters
    {
        public string ID { get; set; } = "";
        public bool Rotate { get; set; } = false;
        public float Total_Evaluation { get; set; } = 0.0F;
        public float Length_mm { get; set; } = 20;

        // Detection Parameters
        public Size ksize { get; set; } = new Size(17,17);
        public float Min_Scale { get; set; } = 0.7F;
        public float Max_Scale { get; set; } = 0.3F;
        public Size TilesGridSize { get; set; } = new Size(5, 5);
        public double ClipLimit { get; set; } = 40;
        public bool Apply_HistEqu { get; set; } = true;
        public bool Apply_CLAHE { get; set; } = true;
        public int Channel { get; set; } = 0;
        public double DetectionBorder { get; set; } = 0.3;
    }
}
