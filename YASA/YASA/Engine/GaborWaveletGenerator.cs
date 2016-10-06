using System;
using Emgu.CV;
using YASA.Engine.DTO;

namespace YASA.Engine
{
    sealed class GaborWaveletGenerator
    {
        private static GaborWaveletGenerator _instance;

        public static GaborWavelet GenerateGaborWavelet(int kernelSize, double scale, double orientation, double frequency)
        {
            if (_instance == null)
            {
                _instance = new GaborWaveletGenerator();
            }

            GaborWavelet wavelet = new GaborWavelet
            {
                ConvolutionKernelImg = _instance.MakeGaborKernel(false, kernelSize, scale, orientation, frequency),
                ConvolutionKernelReal = _instance.MakeGaborKernel(true, kernelSize, scale, orientation, frequency),
                Scale = scale,
                Orientation = orientation,
                Frequency = frequency
            };

            return wavelet;
        }
        // psi = 0 - phase offset
        // gamma = 1
        private ConvolutionKernelF MakeGaborKernel(bool real, int kernelSize, double scale, double orientation, double frequency)
        {
            ConvolutionKernelF kernel = new ConvolutionKernelF(kernelSize, kernelSize);

            int halfKernelSize = (kernelSize - 1) / 2;

            double doubleScaleSqr = 2*scale*scale;

            double sineTheta = Math.Sin(orientation);
            double cosineTheta = Math.Cos(orientation);

            double exponentCoefficient = 1/(2*Math.PI*doubleScaleSqr);

            double aspectRatio = 2; // either one over frequency in rads, or just greater than 1

            for (int x = -halfKernelSize, xk = 0; x <= halfKernelSize; ++x, ++xk)
            {
                for (int y = -halfKernelSize, yk = 0; y <= halfKernelSize; ++y, ++yk)
                {
                    double rotatedX = (x * cosineTheta + y * sineTheta)/scale;
                    double rotatedY = (-x * sineTheta + y * cosineTheta)/scale;



                    double exponent = exponentCoefficient*Math.Exp(-((rotatedX * rotatedX) / (doubleScaleSqr * aspectRatio) + (rotatedY * rotatedY) / (doubleScaleSqr)));

                    double fourierTransformPart = GetPartOfTheFourier(real, frequency, rotatedX);


                    kernel[xk, yk] = (float) (exponent*fourierTransformPart);
                }

            }

            return kernel;
        }

        private double GetPartOfTheFourier(bool real, double frequency, double rotatedX)
        {
            double dcCorrection = Math.Exp(-2.5*2.5/2)/2;
            return real ? Math.Cos(2*Math.PI*frequency*rotatedX) - dcCorrection : Math.Sin(2*Math.PI*frequency*rotatedX) - dcCorrection;
        }
    }
}
