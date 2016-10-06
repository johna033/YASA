namespace YASA.Engine.DTO
{
    sealed class GaborEmbeddingMap
    {
        public double[,] FrequencyFunction;
        public double[,] ScaleFunction;
        public double[,] OrientationFunction;
        public double[,] RealResponseFunction;
        public double[,] ImgResponseFunction;

        public GaborEmbeddingMap()
        {
        }

        public GaborEmbeddingMap(int width, int height)
        {
            FrequencyFunction = new double[height,width];
            ScaleFunction = new double[height, width];
            OrientationFunction = new double[height, width];
            RealResponseFunction = new double[height,width];
            ImgResponseFunction = new double[height,width];
        }
    }
}
