using System;
using System.Collections.Generic;
using System.IO;
using YASA.Engine.DTO;


namespace YASA.Engine.Util
{
    sealed class Util
    {

        public static void SaveMatrix(ref double[,] matrix, string filename)
        {
            TextWriter tw = new StreamWriter(@"~/../../../"+filename+@".m");
            tw.WriteLine("gaborResponse=[");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    tw.Write(((float)matrix[i,j])+" ");
                }
                tw.Write(";");
                tw.WriteLine();
            }
            tw.WriteLine("];");
            tw.WriteLine("mesh(gaborResponse);");
            tw.Close();
        }

        public static void SaveArray(ref List<double> array, string filename)
        {
            TextWriter tw = new StreamWriter(@"~/../../../" + filename + @".m");
            tw.WriteLine("x=[1:"+array.Count+"];");
            tw.WriteLine("gaborResponse=[");
            foreach (double d in array)
            {
                tw.Write(d+",");
            }
            tw.WriteLine("];");
            tw.WriteLine("plot(x,gaborResponse)");
            tw.Close();
        }


        public static Vector2D[,] AllocateVectorField(int width, int height)
        {
            Vector2D[,] field = new Vector2D[height,width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    field[i,j] = new Vector2D();
                }
            }

            return field;
        }

        public static double MinValue(ref double[,] matrix, out double expected)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double min = double.MaxValue;
            expected = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    expected += matrix[i, j];
                    if (min > Math.Abs(matrix[i, j]))
                        min = Math.Abs(matrix[i, j]);
                }
            }
            expected /= height*width;
            double dev = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    dev += (expected - matrix[i, j]) * (expected - matrix[i, j]);
                }
            }

            Console.WriteLine("STD: "+Math.Sqrt(dev/(width*height)));
            Console.WriteLine("Mean: "+expected);
            return min;
        }
    }
}
