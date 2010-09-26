using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An ordered collection of four items of the same type.
    /// </summary>
    public struct Quadruple<T> : IEquatable<Quadruple<T>>
        where T : IEquatable<T>
    {
        public Quadruple(T A, T B, T C, T D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public T A;
        public T B;
        public T C;
        public T D;

        public bool Equals(Quadruple<T> other)
        {
            return this.A.Equals(other.A) && this.B.Equals(other.B) && this.C.Equals(other.C) && this.D.Equals(other.D);
        }

        public override int GetHashCode()
        {
            int h = 0x1337BED5;
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            int c = this.C.GetHashCode();
            int d = this.D.GetHashCode();
            h += a << 3 + b << 7 + c << 13 + d << 17 +
                d >> 3 + c >> 7 + b >> 13 + a >> 17;
            h = h ^ a ^ b ^ c ^ d;
            return h;
        }
    }

    /// <summary>
    /// An ordered collection of three items of the same type.
    /// </summary>
    public struct Triple<T> : IEquatable<Triple<T>>
        where T : IEquatable<T>
    {
        public Triple(T A, T B, T C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public T A;
        public T B;
        public T C;

        public bool Equals(Triple<T> other)
        {
            return this.A.Equals(other.A) && this.B.Equals(other.B) && this.C.Equals(other.C);
        }

        public override int GetHashCode()
        {
            int h = 0x1337BED5;
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            int c = this.C.GetHashCode();
            h += a << 3 + b << 7 + c << 13 +
                 c >> 3 + b >> 7 + a >> 13;
            h = h ^ a ^ b ^ c;
            return h;
        }
    }

}