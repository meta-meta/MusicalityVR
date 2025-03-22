using System;

namespace Musicality
{
    [Serializable]
    public struct Ratio : IEquatable<Ratio>
    {
        // https://stackoverflow.com/a/41766138/1477489
        private static int Gcd(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        public Ratio(int numerator, int denominator)
        {
            if (denominator == 0) throw new Exception("denominator cannot be zero");
            var gcd = Gcd(numerator, denominator);
            Denominator = denominator / gcd;
            Numerator = numerator / gcd;
        }

        public int Numerator;
        public int Denominator;
        public float Decimal => (float)Numerator / Denominator;

        public static Ratio operator *(Ratio a, Ratio b)
            => new Ratio(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

        public static Ratio operator *(Ratio a, int b)
            => new Ratio(a.Numerator * b, a.Denominator);

        public static Ratio operator /(Ratio a, Ratio b)
        {
            if (b.Numerator == 0) throw new DivideByZeroException();
            return new Ratio(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        }

        public static Ratio operator /(Ratio a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            return new Ratio(a.Numerator, a.Denominator * b);
        }

        public static bool operator ==(Ratio a, Ratio b)
        {
            return a.Numerator == b.Numerator && a.Denominator == b.Denominator;
        }
        
        public static bool operator !=(Ratio a, Ratio b)
        {
            return !(a == b);
        }

        public bool Equals(Ratio other)
        {
            return other == this;
        }
        
        public override int GetHashCode() => (Numerator, Denominator).GetHashCode();

    }
}