using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using YASA.Engine.DAO;
using YASA.Engine.DTO;
using YASA.Engine.Numeric;

namespace YASA.Engine
{
    class IntegratedActiveContoursTextureSegmentationAlgorithm: ITextureSegmentationAlgorithm
    {
        private GaborEmbeddingMap _embeddingMap;

        private readonly int _mapHeight;
        private readonly int _mapWidth;

        private readonly VectorAnalysis _analysis = VectorAnalysis.GetInstance();
        private readonly GaborMetricsCalculator _metricsCalculator = GaborMetricsCalculator.GetInstance();

        private const int GaussKernelSize = 9;

        private double[,] _levelSetFunctionValues;

        public IntegratedActiveContoursTextureSegmentationAlgorithm(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            _embeddingMap = GaborEmbeddingMapDao.GetGaborEmbeddingMap(ref featureSpace);

            _mapHeight = _embeddingMap.FrequencyFunction.GetLength(0);
            _mapWidth = _embeddingMap.FrequencyFunction.GetLength(1);

           SmoothOutFetures();

            Util.Util.SaveMatrix(ref _embeddingMap.RealResponseFunction, "bb_realResponse");
        }

        private void SmoothOutFetures()
        {
            SmoothOuFeature(ref _embeddingMap.RealResponseFunction);
            SmoothOuFeature(ref _embeddingMap.ImgResponseFunction);
            SmoothOuFeature(ref _embeddingMap.FrequencyFunction);
            SmoothOuFeature(ref _embeddingMap.OrientationFunction);
            SmoothOuFeature(ref _embeddingMap.ScaleFunction);
        }

        private void SmoothOuFeature(ref double[,] feature)
        {
            double[, ,] data = new double[_mapHeight, _mapWidth, 1];

            for (int i = 0; i < _mapHeight; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    data[i, j, 0] = feature[i, j];
                }
            }

            Image<Gray, double> img = new Image<Gray, double>(data);
            img = img.SmoothGaussian(GaussKernelSize);

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    feature[y, x] = img.Data[y, x, 0];
                }
            }
        }
        private List<double> deltas = new List<double>(); 
        private void CoupledBeltramiFlowDiffusion()
        {
            double[,] determinantValues = new double[_mapHeight,_mapWidth];
            double[,] squareRootOfDeterminantValues = new double[_mapHeight, _mapWidth];

            Vector2D[,] realVectorField = Util.Util.AllocateVectorField(_mapWidth, _mapHeight);
            Vector2D[,] imgVectorField = Util.Util.AllocateVectorField(_mapWidth, _mapHeight);
            Vector2D[,] frequencyVectorField = Util.Util.AllocateVectorField(_mapWidth, _mapHeight);
            Vector2D[,] scaleVectorField = Util.Util.AllocateVectorField(_mapWidth, _mapHeight);
            Vector2D[,] orientationVectorField = Util.Util.AllocateVectorField(_mapWidth, _mapHeight);

            double simDelta = 1.0f;

            for (; simDelta > 0.005; )
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    for (int x = 0; x < _mapWidth; x++)
                    {
                        determinantValues[y, x] =
                            1/_metricsCalculator.GetGaborFeatureMetricsDeterminantAtPoint(ref _embeddingMap, x, y);
                        squareRootOfDeterminantValues[y, x] = Math.Sqrt(1+determinantValues[y, x]);
                    }
                }

                
               for (int y = 0; y < _mapHeight; y++)
                {
                    for (int x = 0; x < _mapWidth; x++)
                    {
                        Vector2D real = _analysis.CalculateGradientOnDomain(ref determinantValues,
                            ref _embeddingMap.RealResponseFunction, x, y);
                        Vector2D img = _analysis.CalculateGradientOnDomain(ref determinantValues,
                            ref _embeddingMap.ImgResponseFunction, x, y);
                        Vector2D scale = _analysis.CalculateGradientOnDomain(ref determinantValues,
                            ref _embeddingMap.ScaleFunction, x, y);
                        Vector2D frequency = _analysis.CalculateGradientOnDomain(ref determinantValues,
                            ref _embeddingMap.FrequencyFunction, x, y);
                        Vector2D orientation = _analysis.CalculateGradientOnDomain(ref determinantValues,
                            ref _embeddingMap.OrientationFunction, x, y);

                        double denominator = 1/(2*squareRootOfDeterminantValues[y, x]);
                       
                        realVectorField[y, x].X = real.X / denominator;
                        realVectorField[y, x].Y = real.Y / denominator;

                        imgVectorField[y, x].X = img.X/ denominator;
                        imgVectorField[y, x].Y = img.Y / denominator;

                        orientationVectorField[y, x].X = orientation.X / denominator;
                        orientationVectorField[y, x].Y = orientation.Y / denominator;

                        frequencyVectorField[y, x].X = frequency.X / denominator;
                        frequencyVectorField[y, x].Y = frequency.Y / denominator;

                        scaleVectorField[y, x].X = scale.X / denominator;
                        scaleVectorField[y, x].Y = scale.Y / denominator;
                    }
                }
                CoupledBeltramiFlowUpdate(ref realVectorField, ref imgVectorField, ref orientationVectorField, ref scaleVectorField, ref frequencyVectorField, ref squareRootOfDeterminantValues, out simDelta);
            }
            SmoothOutFetures();
            Util.Util.SaveMatrix(ref _embeddingMap.RealResponseFunction, "realResponse");
            Util.Util.SaveMatrix(ref _embeddingMap.ImgResponseFunction, "imgResponse");
            Util.Util.SaveMatrix(ref _embeddingMap.ScaleFunction, "scaleResponse"); 
            Util.Util.SaveMatrix(ref _embeddingMap.OrientationFunction, "orientationResponse");
            Util.Util.SaveMatrix(ref _embeddingMap.FrequencyFunction, "frequencyResponse");

            Util.Util.SaveArray(ref deltas, "deltaBehavior");
            
        }

       
        private void CoupledBeltramiFlowUpdate(ref Vector2D[,] realField, ref Vector2D[,] imgField, ref Vector2D[,] orientationField, ref Vector2D[,] scaleField, ref Vector2D[,] frequencyField, ref double[,] squareRootsOfDet, out double delta)
        {

            delta = 0;
            for (int i = 0; i < _mapHeight; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    double realDelta = -1 / squareRootsOfDet[i, j] * _analysis.CalculateDivergence(ref realField, j, i);
                    double imgDelta = -1 / squareRootsOfDet[i, j] *
                                                                _analysis.CalculateDivergence(ref imgField, j, i);
                    double frequencyDelta = -1 / squareRootsOfDet[i, j] *
                                                                _analysis.CalculateDivergence(ref frequencyField, j, i);
                    double orientationDelta = -1 / squareRootsOfDet[i, j] *
                                                                _analysis.CalculateDivergence(ref orientationField, j, i);
                    double scaleDelta = -1 / squareRootsOfDet[i, j]*
                                        _analysis.CalculateDivergence(ref scaleField, j, i);

                    _embeddingMap.RealResponseFunction[i, j] += realDelta;

                    _embeddingMap.ImgResponseFunction[i, j] += imgDelta;

                    _embeddingMap.FrequencyFunction[i, j] += frequencyDelta;

                    _embeddingMap.OrientationFunction[i, j] += orientationDelta;

                    _embeddingMap.ScaleFunction[i, j] += scaleDelta;

                    delta += (Math.Abs(realDelta) + Math.Abs(imgDelta) + Math.Abs(frequencyDelta) + Math.Abs(orientationDelta) + Math.Abs(scaleDelta));
                }
            }

            delta /= (5*_mapHeight*_mapWidth);
            deltas.Add(delta);
            Console.WriteLine(delta);

        }

        public double[,] GetStoppingTermFunction()
        {
            double[,] stoppingTermFunction = new double[_mapHeight, _mapWidth];

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    stoppingTermFunction[y, x] = 1/
                                                 (1 +
                                                  _metricsCalculator.GetGaborFeatureMetricsDeterminantAtPoint(
                                                      ref _embeddingMap, x, y));
                }
            }

            
            SmoothOuFeature(ref stoppingTermFunction);
            Util.Util.SaveMatrix(ref stoppingTermFunction, "stoppingTerm");

            return stoppingTermFunction;
        }


        public LinkedList<Point> GetBorderPoints()
        {
            CoupledBeltramiFlowDiffusion();

            _levelSetFunctionValues = GetStoppingTermFunction();
            LinkedList<Point> points = new LinkedList<Point>();
            
           double[,] gradientMagnitudes = new double[_mapHeight,_mapWidth];
            //GaussKernelSize = 5;
            SmoothOuFeature(ref _levelSetFunctionValues);
            double expected;
            double min = Util.Util.MinValue(ref _levelSetFunctionValues, out expected);

            for (int i = 0; i < _mapHeight; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    Vector2D t = _analysis.CalculateGradient(ref _levelSetFunctionValues, j, i);
                    gradientMagnitudes[i, j] = _analysis.GetEuclideanMetricLength(ref t);

                    if (_levelSetFunctionValues[i, j] > expected)
                    {
                        points.AddLast(new Point(j, i));
                    }
                }
            }

            Util.Util.SaveMatrix(ref _levelSetFunctionValues, "levelSet");
            Util.Util.SaveMatrix(ref gradientMagnitudes, "levelSetGradient");
            float max = 0;
            using (Image<Gray, float> img = new Image<Gray, float>(_mapWidth, _mapHeight))
            {
                for (int i = 0; i < _mapHeight; i++)
                {
                    for (int j = 0; j < _mapWidth; j++)
                    {
                        img.Data[i, j, 0] = (float) (255*7*gradientMagnitudes[i, j]);
                        if (img.Data[i, j, 0] > max)
                        {
                            max = img.Data[i, j, 0];
                        }

                    }
                }
                img.Save(@"~/../../../dataset/segmented_gradient.jpg");
                Image<Gray, byte> img1 = img.Convert<Gray, byte>();
                img1 = img1.Canny(0.5,0.5);
                img1.Save(@"~/../../../dataset/segmented_canny.jpg");

            }

            return points;
        }

        
    }
}
