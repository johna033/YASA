using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace YASA.Engine.DTO
{
    class GaborWavelet
    {
        public ConvolutionKernelF ConvolutionKernelReal { get; set; }
        public ConvolutionKernelF ConvolutionKernelImg { get; set; }
        public double Scale { get; set; }
        public double Orientation { get; set; }
        public double Frequency { get; set; }


    }
}
