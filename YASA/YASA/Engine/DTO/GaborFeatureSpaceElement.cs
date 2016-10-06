namespace YASA.Engine.DTO
{
    internal sealed class GaborCoefficient
    {
        public float[,,] RealPart;
        public float[,,] ImgPart;

        public void SetParts(float[,,] realPart, float[,,] imgPart)
        {
            RealPart = realPart;
            ImgPart = imgPart;
        }
    }

    sealed class GaborFeatureSpaceElement
    {
        public GaborWavelet AssociatedGaborWavelet;
        public float ResponseMagnitude;
        public float RealResponsePart;
        public float ImgResponsePart;
        public int xLoc;
        public int yLoc;

    }
}
