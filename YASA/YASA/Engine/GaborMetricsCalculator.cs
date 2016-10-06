using System;
using YASA.Engine.DTO;
using YASA.Engine.Numeric;

namespace YASA.Engine
{
    sealed class GaborMetricsCalculator
    {
        /*               |F E|
         * metrics(x,y) =|   |
         *               |E G|
         */
        private static GaborMetricsCalculator _instance;
        private readonly VectorAnalysis _analysis = VectorAnalysis.GetInstance();

        private GaborMetricsCalculator(){}

        public static GaborMetricsCalculator GetInstance()
        {
            return _instance ?? (_instance = new GaborMetricsCalculator());
        }

        /// <summary>
        /// Calculates determinant of the metrix at point, considering all parameters of the embedding map (real, img, orientation, scale, frquency)
        /// </summary>
        /// <param name="embeddingMap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetGaborFeatureMetricsDeterminantAtPoint(ref GaborEmbeddingMap embeddingMap, int x, int y)
        {

            Vector2D realGradient = _analysis.CalculateGradient(ref embeddingMap.RealResponseFunction, x, y);
            Vector2D imgGradient = _analysis.CalculateGradient(ref embeddingMap.ImgResponseFunction, x, y);
            Vector2D frequencyGradient = _analysis.CalculateGradient(ref embeddingMap.FrequencyFunction, x, y);
            Vector2D orientationGradient = _analysis.CalculateGradient(ref embeddingMap.OrientationFunction, x, y);
            Vector2D scaleGradient = _analysis.CalculateGradient(ref embeddingMap.ScaleFunction, x, y);

            double f = 1 + (realGradient.X*realGradient.X) + (imgGradient.X*imgGradient.X) +
                       (scaleGradient.X*scaleGradient.X) +
                       (orientationGradient.X*orientationGradient.X) + (frequencyGradient.X*frequencyGradient.X);

            double e = (realGradient.X*realGradient.Y) + (imgGradient.X*imgGradient.Y) +
                       (scaleGradient.X*scaleGradient.Y) +
                       (orientationGradient.X*orientationGradient.Y) + (frequencyGradient.X*frequencyGradient.Y);

            double g = 1 + (realGradient.Y*realGradient.Y) + (imgGradient.Y*imgGradient.Y) +
                       (scaleGradient.Y*scaleGradient.Y) +
                       (orientationGradient.Y*orientationGradient.Y) + (frequencyGradient.Y*frequencyGradient.Y);

            return Math.Abs(
                f*g - e*e);
        }
    }
}
