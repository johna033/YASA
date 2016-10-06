using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using YASA.Engine.DTO;
using YASA.Engine.Numeric;

namespace YASA.Engine
{
    sealed class GaborFeatureSpaceGenerator
    {
        private static GaborFeatureSpaceGenerator _instance;
        private const int GaborKernelSize = 6;

        private readonly VectorAnalysis _analysis = VectorAnalysis.GetInstance();

        public static GaborFeatureSpaceGenerator GetInstance()
        {
            return _instance ?? (_instance = new GaborFeatureSpaceGenerator());
        }

        public GaborFeatureSpaceElement[,] GenerateFutureSpaceWithMaximalResponseCoefficients(
            Image<Gray, float> image, double[] orientations, double[] scales, double[] frequencies)
        {

            int szOrientations = orientations.Length;
            int szScales = scales.Length;
            int szFrequencies = frequencies.Length;

            Size imageSize = image.Size;

            int width = imageSize.Width;
            int height = imageSize.Height;

            //TODO find maximum coefficient at each point. 3D ARRAY (X,Y, Z = [0; NUMBER_OF_DIFFERENT_WAVELETS])
            GaborFeatureSpaceElement[] featureSpace = AllocateIntermediateFeatureSpace(imageSize);

            Image<Gray, float> imageConvolutionReal = new Image<Gray, float>(imageSize);
            Image<Gray, float> imageConvolutionImg = new Image<Gray, float>(imageSize);


           
            for (int i = 0; i < szOrientations; i++)
            {
                for (int j = 0; j < szScales; j++)
                {
                    for (int k = 0; k < szFrequencies; k++)
                    {
                        
                        GaborWavelet wavelet = GaborWaveletGenerator.GenerateGaborWavelet(GaborKernelSize, scales[j],
                            orientations[i], frequencies[k]);

                        CvInvoke.cvFilter2D(image.Ptr, imageConvolutionReal.Ptr, wavelet.ConvolutionKernelReal, new Point(-1, -1));
                        CvInvoke.cvFilter2D(image.Ptr, imageConvolutionImg.Ptr, wavelet.ConvolutionKernelImg, new Point(-1, -1));

                        GaborCoefficient coefficient = new GaborCoefficient{RealPart = imageConvolutionReal.Data, ImgPart = imageConvolutionImg.Data};

                        for (int l = 0; l < height; l++)
                        {
                            for (int p = 0; p < width; p++)
                            {
                                Vector2D coeff = new Vector2D
                                {
                                    X = coefficient.RealPart[l, p, 0],
                                    Y = 0 //coefficient.ImgPart[l, p, 0]
                                };
                                float responseMagnitude = (float) _analysis.GetEuclideanMetricLength(ref coeff);

                                if (featureSpace[l*width + p].ResponseMagnitude < responseMagnitude)
                                {

                                    featureSpace[l*width + p].AssociatedGaborWavelet = wavelet;
                                    featureSpace[l*width + p].RealResponsePart = (float) coeff.X;
                                    coeff.Y = coefficient.ImgPart[l, p, 0];
                                    featureSpace[l*width + p].ImgResponsePart = (float) coeff.Y;
                                    featureSpace[l*width + p].ResponseMagnitude = responseMagnitude;
                                }
                            }
                        }
                    }
                }
            }

            return ConvertFromIntermediate(ref featureSpace, imageSize);
        }

        private GaborFeatureSpaceElement[] AllocateIntermediateFeatureSpace(Size imageSize)
        {
            int height = imageSize.Height;
            int width = imageSize.Width;
            GaborFeatureSpaceElement[] space = new GaborFeatureSpaceElement[height*width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    space[i*width+j] = new GaborFeatureSpaceElement {xLoc = j, yLoc = i};
                }
            }
            return space;
        }

        private GaborFeatureSpaceElement[,] ConvertFromIntermediate(ref GaborFeatureSpaceElement[] space, Size imageSize)
        {
            int height = imageSize.Height;
            int width = imageSize.Width;
            GaborFeatureSpaceElement[,] convertedSpace = new GaborFeatureSpaceElement[height, width];

            for (int i = 0; i < width*height; i++)
            {
                convertedSpace[space[i].yLoc, space[i].xLoc] = space[i];
            }

            return convertedSpace;
        }
    }
}
