using YASA.Engine.DTO;

namespace YASA.Engine.DAO
{
    class GaborEmbeddingMapDao
    {

        private static GaborEmbeddingMapDao _instance;

        private GaborEmbeddingMapDao()
        {
        }

        private static GaborEmbeddingMapDao GetInstance()
        {
            return _instance ?? (_instance = new GaborEmbeddingMapDao());
        }
        //TODO there has to be an easier(neater) way of doing this
        public static GaborEmbeddingMap GetGaborEmbeddingMap(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            GaborEmbeddingMapDao instance = GetInstance();

            
            return new GaborEmbeddingMap
            {
                FrequencyFunction = instance.GetFrequencyFunction(ref featureSpace),
                OrientationFunction = instance.GetOrientationFunction(ref featureSpace),
                ScaleFunction = instance.GetScaleFunction(ref featureSpace),
                ImgResponseFunction = instance.GetImgResponseFunction(ref featureSpace),
                RealResponseFunction = instance.GetRealResponseFunction(ref featureSpace)
            };
        }

        private double[,] GetRealResponseFunction(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            int height = featureSpace.GetLength(0);
            int width = featureSpace.GetLength(1);

            double[,] realResponseFunction = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    realResponseFunction[i, j] = featureSpace[i, j].RealResponsePart;
                }
            }

            return realResponseFunction;
        }

        private double[,] GetImgResponseFunction(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            int height = featureSpace.GetLength(0);
            int width = featureSpace.GetLength(1);

            double[,] imgResponseFunction = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    imgResponseFunction[i, j] = featureSpace[i, j].ImgResponsePart;
                }
            }

            return imgResponseFunction;
        }

        private double[,] GetFrequencyFunction(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            int height = featureSpace.GetLength(0);
            int width = featureSpace.GetLength(1);

            double[,] frequencyFunction = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    frequencyFunction[i, j] = featureSpace[i, j].AssociatedGaborWavelet.Frequency;
                }
            }
            return frequencyFunction;
        }

        private double[,] GetScaleFunction(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            int height = featureSpace.GetLength(0);
            int width = featureSpace.GetLength(1);

            double[,] scaleFunction = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    scaleFunction[i, j] = featureSpace[i, j].AssociatedGaborWavelet.Scale;
                }
            }

            return scaleFunction;
        }

        private double[,] GetOrientationFunction(ref GaborFeatureSpaceElement[,] featureSpace)
        {
            int height = featureSpace.GetLength(0);
            int width = featureSpace.GetLength(1);

            double[,] orientationFunction = new double[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    orientationFunction[i, j] = featureSpace[i, j].AssociatedGaborWavelet.Scale;
                }
            }

            return orientationFunction;
        }
    }
}
