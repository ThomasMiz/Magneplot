using Silk.NET.Maths;
using System;
using System.Linq;

namespace Magneplot.Generator
{
    static class MathUtils
    {
        public const double TwoPi = 6.283185307179586476925286766559;

        public static double Lerp(double min, double max, double step)
        {
            return min * (1 - step) + max * step;
        }

        public static double AreaOfTriangle(in Vector3D<double> v1, in Vector3D<double> v2, in Vector3D<double> v3)
        {
            return (v1.X * (v2.Y - v3.Y) + v2.X * (v3.Y - v1.Y) + v3.X * (v1.Y - v2.Y)) / 2;
        }

        public static uint CiclycalShiftRight(uint value, int amount)
        {
            return value << amount | value >> ((sizeof(uint) * 8) - amount);
        }

        public static uint HashToUint(params object[] values)
        {
            uint hashCode = 0;
            unchecked
            {
                foreach (var item in values)
                {
                    hashCode = CiclycalShiftRight(hashCode, 7) ^ (uint)(item?.GetHashCode() ?? 0);
                }
            }

            return hashCode;
        }

        // Taken from https://stackoverflow.com/questions/53086/can-i-depend-on-the-values-of-gethashcode-to-be-consistent
        // Because I need a string's hash code to be consistent
        public static unsafe int GetHashCode32(this string s)
        {
            fixed (char* str = s.ToCharArray())
            {
                char* chPtr = str;
                int num = 0x15051505;
                int num2 = num;
                int* numPtr = (int*)chPtr;
                for (int i = s.Length; i > 0; i -= 4)
                {
                    num = (num << 5) + num + (num >> 0x1b) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    num2 = (num2 << 5) + num2 + (num2 >> 0x1b) ^ numPtr[1];
                    numPtr += 2;
                }
                return num + num2 * 0x5d588b65;
            }
        }

        public static Vector3D<double> FindNormalVectorTo(Vector3D<double> direction)
        {
            // Takes the unit vector that is the least parallel to the direction vector, then makes it
            // normal to it by applying Gram–Schmidt.
            (Vector3D<double>, double)[] arr = [
                (Vector3D<double>.UnitX, Vector3D.Dot(Vector3D<double>.UnitX, direction)),
                (Vector3D<double>.UnitY, Vector3D.Dot(Vector3D<double>.UnitY, direction)),
                (Vector3D<double>.UnitZ, Vector3D.Dot(Vector3D<double>.UnitZ, direction))
            ];

            (Vector3D<double>, double) val = arr.MinBy(x => Math.Abs(x.Item2));
            return val.Item1 - val.Item2 * direction;
        }
    }
}
