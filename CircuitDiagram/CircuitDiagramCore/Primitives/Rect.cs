using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Primitives
{
    public sealed class Rect : IEquatable<Rect>
    {
        public Rect()
            : this(0, 0, 0, 0)
        {
        }

        public Rect(Point location, Size size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public Rect(Point topLeft, Point bottomRight)
        {
            X = topLeft.X;
            Y = topLeft.Y;
            Width = bottomRight.X - topLeft.X;
            Height = bottomRight.Y - topLeft.Y;
        }

        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double X { get; }

        public double Y { get; }

        public double Width { get; }

        public double Height { get; }

        public Point TopLeft => new Point(X, Y);

        public Point BottomRight => new Point(X + Width, Y + Height);

        public Size Size => new Size(Width, Height);

        /// <summary>
        /// Returns a new <see cref="Rect" /> containing this <see cref="Rect"/> and the other <see cref="Rect"/>.
        /// </summary>
        public Rect Union(Rect other)
        {
            return new Rect(new Point(Math.Min(X, other.X), Math.Min(Y, other.Y)),
                            new Point(Math.Max(BottomRight.X, other.BottomRight.X), Math.Max(BottomRight.Y, other.BottomRight.Y)));
        }

        #region Equality

        public bool Equals(Rect other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Rect && Equals((Rect)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
