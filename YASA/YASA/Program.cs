using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using YASA.Engine;
using YASA.Engine.DTO;

namespace YASA
{
    class Program
    {
        static void Main(string[] args)
        {
            Image<Gray, float> image = new Image<Gray, float>(@"~/../../../dataset/palm_tree1.jpg");
           /* image = image.Canny(1.0, 2);
            ImageViewer.Show(image);
            image.Save(@"~/../../../dataset/canny_control.jpg");*/

           double[] orientations = new double[20];//{0, Math.PI/8, 2*Math.PI/8, 3*Math.PI/8, 4*Math.PI/8, 5*Math.PI/8, 6*Math.PI/8, 7*Math.PI/8};
            for (int i = 0; i <= 19; i++)
            {
                orientations[i] = (i*Math.PI)/20.0;
            }
            double[] scales = {1,2,3,4,5,6,7,8,9,10,11};
            double[] frequencies = new double[15];//= {0.3, 0.6, 0.9, 1.2, 1.5};

            for (int i = 0; i < 15; i++)
            {
                frequencies[i] = i*0.1;
            }
            Stopwatch sw0 = new Stopwatch();
            sw0.Start();
            GaborFeatureSpaceElement[,] gaborFeatureSpace =
                GaborFeatureSpaceGenerator.GetInstance().GenerateFutureSpaceWithMaximalResponseCoefficients(image, orientations,
                    scales, frequencies);
            sw0.Stop();
            Console.WriteLine("Gabor feature space generated in "+sw0.Elapsed);

            IntegratedActiveContoursTextureSegmentationAlgorithm segAlgorithm = new IntegratedActiveContoursTextureSegmentationAlgorithm(ref gaborFeatureSpace);

            Stopwatch sw = new Stopwatch();
            sw.Start();
             LinkedList<Point> border = segAlgorithm.GetBorderPoints();
            sw.Stop();
            Image<Rgb, float> image1 = new Image<Rgb, float>(@"~/../../../dataset/palm_tree1.jpg");
            foreach (Point point in border)
            {
                image1[point] = new Rgb(Color.Yellow);
            }

            ImageViewer.Show(image1);
            image1.Save(@"~/../../../dataset/segmented.jpg");
            Console.WriteLine("Done segmenting. Time: "+sw.Elapsed);

            Console.Read();
        }
    }
}