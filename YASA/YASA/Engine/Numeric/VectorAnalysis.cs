using System;
using YASA.Engine.DTO;

namespace YASA.Engine.Numeric
{
    class VectorAnalysis
    {
        private static VectorAnalysis _instance;

        private const double Epsilon = 1e-1;

        private VectorAnalysis(){}

        public static VectorAnalysis GetInstance()
        {
            return _instance ?? (_instance = new VectorAnalysis());
        }

        public double GetEuclideanMetricLength(ref Vector2D vector)
        {
            return Math.Sqrt(vector.X*vector.X + vector.Y*vector.Y);
        }

        public Vector2D CalculateGradient(ref double[,] function, int x, int y)
        {
            int rows = function.GetLength(0);
            int columns = function.GetLength(1);
            return new Vector2D
            {
                X = GetDerivative(true, ref function, y, x, rows, columns),
                Y = GetDerivative(false, ref function, y, x, rows, columns)
            };
        }

        public Vector2D CalculateGradientOnDomain(ref double[,] function, ref double[,] domain, int x, int y)
        {
            int rows = function.GetLength(0);
            int columns = function.GetLength(1);
            double dDomainDx = GetDerivative(true, ref domain, y, x, rows, columns);
            double dDomainDy = GetDerivative(false, ref domain, y, x, rows, columns);

            int signDx = Math.Sign(dDomainDx) == 0 ? 1 : Math.Sign(dDomainDx);
            int signDy = Math.Sign(dDomainDy) == 0 ? 1 : Math.Sign(dDomainDy);
            return new Vector2D
            {
                X = GetDerivative(true, ref function, y, x, rows, columns) / (Math.Abs(dDomainDx) < Epsilon ? signDx * 1 : dDomainDx),
                Y = GetDerivative(false, ref function, y, x, rows, columns) / (Math.Abs(dDomainDy) < Epsilon ? signDy * 1 : dDomainDy)
            };
        }

        public double CalculateDivergence(ref Vector2D[,] field, int x, int y)
        {
            int rows = field.GetLength(0);
            int columns = field.GetLength(1);

            return GetDerivative(true, ref field, y, x, rows, columns) + GetDerivative(false, ref field, y, x, rows, columns);
        }

        private double GetDerivative(bool xAxis, ref Vector2D[,] field, int row, int col, int rows, int columns)
        {
            if (xAxis)
            {
                if (col == 0 || col == columns - 1)
                {
                    if (col == 0)
                    {
                        return field[row, col + 1].X - field[row, col].X;
                    }

                    return field[row, col].X - field[row, col - 1].X;
                }

                if (col >= 2 && col <= columns - 3)
                {
                    return (-field[row, col + 2].X + 8 * field[row, col + 1].X - 8 * field[row, col - 1].X +
                            field[row, col - 2].X) / 12.0;
                }

                return (field[row, col + 1].X - field[row, col - 1].X) / 2;
            }

            //if Y-axis
            if (row == 0 || row == rows - 1)
            {
                if (row == 0)
                {
                    return field[row + 1, col].Y - field[row, col].Y;
                }
                return field[row, col].Y - field[row - 1, col].Y;
            }

            if (row >= 2 && row <= rows - 3)
            {
                return (-field[row + 2, col].Y + 8 * field[row + 1, col].Y - 8 * field[row - 1, col].Y +
                        field[row - 2, col].Y) / 12.0;
            }

            return (field[row+1, col].Y - field[row-1, col].Y) / 2;
        }

        private double GetDerivative(bool xAxis, ref double[,] function, int row, int col, int rows, int columns)
        {
            if (xAxis)
            {
                if (col == 0 || col == columns - 1)
                {
                    if (col == 0)
                    {
                        return function[row, col + 1] - function[row, col];
                    }

                        return function[row, col] - function[row, col - 1];
                }

                if (col >= 2 && col <= columns - 3)
                {
                    return (-function[row, col + 2] + 8*function[row, col + 1] - 8*function[row, col - 1] +
                            function[row, col - 2])/12.0;
                }

                return (function[row, col + 1] - function[row, col - 1])/2;
            }

            if (row == 0 || row == rows - 1)
            {
                if (row == 0)
                {
                    return function[row + 1, col] - function[row, col];
                }
                return function[row, col] - function[row - 1, col];
            }

            if (row >= 2 && row <= rows - 3)
            {
                return (-function[row+2, col] + 8 * function[row+1, col] - 8 * function[row-1, col] +
                        function[row-2, col]) / 12.0;
            }

            return (function[row+1, col] - function[row-1, col])/2;
        }
    }
}
