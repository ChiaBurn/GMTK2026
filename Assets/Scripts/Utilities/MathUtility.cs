using System;
using System.Collections.Generic;

namespace CountdownAutoBattle.Utilities
{
    public static class MathUtility
    {
        public static int GreatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                int remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }

        public static int LeastCommonMultiple(int a, int b)
        {
            if (a <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(a),
                    "LCM values must be greater than zero.");
            }

            if (b <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(b),
                    "LCM values must be greater than zero.");
            }

            /*
             * 先除再乘，降低中間值溢位風險。
             */
            return checked(
                a / GreatestCommonDivisor(a, b) * b);
        }

        public static int LeastCommonMultiple(
            IReadOnlyList<int> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Count == 0)
            {
                throw new ArgumentException(
                    "LCM requires at least one value.",
                    nameof(values));
            }

            int result = values[0];

            for (int i = 1; i < values.Count; i++)
            {
                result = LeastCommonMultiple(
                    result,
                    values[i]);
            }

            return result;
        }
    }
}