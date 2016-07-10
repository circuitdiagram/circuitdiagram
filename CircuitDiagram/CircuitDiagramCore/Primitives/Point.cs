using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Primitives
{
    public struct Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public Point Add(Vector v)
        {
            return new Point(X + v.X, Y + v.Y);
        }

        public Point WithNewX(double x)
        {
            return new Point(x, Y);
        }

        public Point WithNewY(double y)
        {
            return new Point(X, y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static Point Parse(string s)
        {
            try
            {
                int c = s.IndexOf(",");
                double x = double.Parse(s.Substring(0, c));
                double y = double.Parse(s.Substring(c + 1));
                return new Point(x, y);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid format", ex);
            }
        }

        public static Point Add(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point Add(Point a, Vector b)
        {
            return new Point(a.X + b.Y, a.Y + b.Y);
        }

        public bool Equals(Point other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point && Equals((Point)obj);
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !left.Equals(right);
        }
    }
}
