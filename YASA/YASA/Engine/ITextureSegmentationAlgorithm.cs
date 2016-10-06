using System.Collections.Generic;
using System.Drawing;

namespace YASA.Engine
{
    internal interface ITextureSegmentationAlgorithm
    {
        LinkedList<Point> GetBorderPoints();
    }
}
