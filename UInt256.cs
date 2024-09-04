using System.Text;

namespace ConsoleApp1;

internal class UInt256
{
    public UInt256(UInt128 x, UInt128 y)
    {
        High = x;
        Low = y;
    }

    public UInt256() : this(0, 0)
    {
    }

    public UInt128 High { get; }
    public UInt128 Low { get; }

    public static UInt256 Zero => new(0, 0);
    public static UInt256 One => new(0, 1);
    public static UInt256 Two => new(0, 2);
    public static UInt256 Ten => new(0, 10);

    /// <summary>
    ///     Maximum value of UInt256 :
    ///     115_792_089_237_316_195_423_570_985_008_687_907_853_269_984_665_640_564_039_457_584_007_913_129_639_935
    /// </summary>
    public static UInt256 MaxValue => new(UInt128.MaxValue, UInt128.MaxValue);

    // Implicit conversion from ulong to UInt256
    public static implicit operator UInt256(ulong value)
    {
        return new UInt256(0, value);
    }

    // Implicit conversion from string to UInt256
    public static implicit operator UInt256(string value)
    {
        return ParseFromString(value);
    }

    public static bool operator ==(UInt256 x, UInt256 y)
    {
        return x.High == y.High && x.Low == y.Low;
    }

    public static bool operator !=(UInt256 x, UInt256 y)
    {
        return !(x == y);
    }

    public static bool operator >(UInt256 x, UInt256 y)
    {
        return x.High > y.High || (x.High == y.High && x.Low > y.Low);
    }

    public static bool operator >=(UInt256 x, UInt256 y)
    {
        return x > y || x == y;
    }

    public static bool operator <(UInt256 x, UInt256 y)
    {
        return !(x >= y);
    }

    public static bool operator <=(UInt256 x, UInt256 y)
    {
        return !(x > y);
    }

    public override bool Equals(object obj)
    {
        return obj is UInt256 other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(High, Low);
    }

    public static UInt256 operator +(UInt256 x, UInt256 y)
    {
        var x2 = x.High + y.High;
        var y2 = x.Low + y.Low;
        if (y2 < x.Low) x2 += 1;
        return new UInt256(x2, y2);
    }

    public static UInt256 operator -(UInt256 x, UInt256 y)
    {
        var x2 = x.High - y.High;
        var y2 = x.Low - y.Low;
        if (y2 > x.Low) x2 -= 1;
        return new UInt256(x2, y2);
    }

    /// <summary>
    ///     Multiplies two UInt256 values and returns the result modulo 2^256.
    /// </summary>
    /// <param name="x">The first UInt256 operand.</param>
    /// <param name="y">The second UInt256 operand.</param>
    /// <returns>The product of x and y, modulo 2^256.</returns>
    public static UInt256 operator *(UInt256 x, UInt256 y)
    {
        // Decompose the operands x and y into their high and low 128-bit parts.
        var (xh, xl) = (x.High, x.Low);
        var (yh, yl) = (y.High, y.Low);

        // Further decompose the 128-bit parts into 64-bit chunks.
        // x = x3<<192 + x2<<128 + x1<<64 + x0
        var (x3, x2) = (xh >> 64, xh & 0xffffffffffffffff);
        var (x1, x0) = (xl >> 64, xl & 0xffffffffffffffff);

        // y = y3<<192 + y2<<128 + y1<<64 + y0
        var (y3, y2) = (yh >> 64, yh & 0xffffffffffffffff);
        var (y1, y0) = (yl >> 64, yl & 0xffffffffffffffff);

        // Perform multiplication while considering the modulus 2^256.
        // The result of x * y is decomposed into z = z3<<192 + z2<<128 + z1<<64 + z0
        // where each zN is computed based on the components of x and y.
        var z0 = x0 * y0; // Compute z0 = x0 * y0
        var z1 = x1 * y0; // Compute z1 = x1 * y0
        var z2 = x2 * y0 + x1 * y1 + x0 * y2; // Compute z2 = x2 * y0 + x1 * y1 + x0 * y2
        var z3 = x3 * y0 + x2 * y1 + x1 * y2 + x0 * y3; // Compute z3 = x3 * y0 + x2 * y1 + x1 * y2 + x0 * y3

        // Add the contribution of x0 * y1 to z1.
        z1 += x0 * y1;

        // If adding x0 * y1 causes an overflow in z1, increment z3 to account for the overflow.
        if (z1 < x0 * y1) z3 += 1;

        // Compute the lower 128 bits of the product.
        var low = z0 + (z1 << 64);

        // Compute the higher 128 bits of the product.
        var high = (z1 >> 64) + z2 + (z3 << 64);

        // If adding z0 to z1<<64 causes an overflow in low, increment high to account for it.
        if (low < z0) high += 1;

        // Print the computed low and high parts (for debugging purposes).
        //Console.WriteLine($"low={low}, high={high}");

        // Return the final result as a new UInt256 composed of the computed high and low parts.
        return new UInt256(high, low);
    }


    public static UInt256 operator <<(UInt256 x, int y)
    {
        if (y == 0) return x;

        if (y < 128)
        {
            var high = (x.High << y) | (x.Low >> (128 - y));
            var low = x.Low << y;
            return new UInt256(high, low);
        }
        else
        {
            // Shift crosses the 128-bit boundary
            var high = x.Low << (y - 128);
            return new UInt256(high, 0);
        }
    }

    public static UInt256 operator >> (UInt256 x, int y)
    {
        if (y == 0) return x;

        if (y < 128)
        {
            var low = (x.Low >> y) | (x.High << (128 - y));
            var high = x.High >> y;
            return new UInt256(high, low);
        }
        else
        {
            // Shift crosses the 128-bit boundary
            var low = x.High >> (y - 128);
            return new UInt256(0, low);
        }
    }

    public static UInt256 operator &(UInt256 x, UInt256 y)
    {
        return new UInt256(x.High & y.High, x.Low & y.Low);
    }

    public static UInt256 operator |(UInt256 x, UInt256 y)
    {
        return new UInt256(x.High | y.High, x.Low | y.Low);
    }

    /// <summary>
    ///     Divides two UInt256 numbers and returns both the quotient and remainder.
    /// </summary>
    /// <param name="x">The dividend.</param>
    /// <param name="y">The divisor.</param>
    /// <returns>A tuple where the first element is the quotient and the second element is the remainder.</returns>
    /// <exception cref="ArgumentException">Thrown when attempting to divide by zero.</exception>
    public static (UInt256, UInt256) DivMod(UInt256 x, UInt256 y)
    {
        // Check if the divisor is zero, which would cause an invalid operation.
        if (y == Zero) throw new ArgumentException("Cannot divide by Zero");

        // If the high 128-bits of y are zero, we can simplify the division.
        if (y.High == 0)
        {
            // If both x and y can fit within 128-bits, perform a simple 128-bit division.
            if (x.High == 0)
                return (new UInt256(0, x.Low / y.Low), new UInt256(0, x.Low % y.Low));

            // For larger numbers, we need to consider both the high and low parts of x.
            // The expression (a*2^128+b)/c is handled as:
            //   (a/c)*2^128 + (a%c)*(2^128/c) + ((a%c)*(2^128%c)+b)/c
            // The expression (a*2^128+b)%c is handled as:
            //   ((a%c)*(2^128%c)+(b%c))%c
            var mask = new UInt128(0xffffffffffffffff, 0xffffffffffffffff);

            // Calculate 2^128 / y.Low and 2^128 % y.Low.
            var quotientMid = mask / y.Low;
            var remainderMid = mask % y.Low;
            remainderMid += 1;
            if (remainderMid >= y.Low)
            {
                quotientMid += 1;
                remainderMid -= y.Low;
            }

            // Calculate the high part of the quotient and remainder.
            var quotientHigh = x.High / y.Low;
            var remainderHigh = x.High % y.Low;

            // Calculate the low part of the quotient and remainder.
            var quotientLow = x.Low / y.Low;
            var remainderLow = x.Low % y.Low;

            // Adjust the quotient and remainder using the intermediate values.
            quotientMid *= remainderHigh;
            remainderMid *= remainderHigh;
            quotientMid += remainderMid / y.Low;
            remainderMid %= y.Low;

            // Update the remainder and adjust the quotient if necessary.
            remainderLow += remainderMid;
            if (remainderLow >= y.Low)
            {
                quotientLow += 1;
                remainderLow -= y.Low;
            }

            // Combine the quotientHigh and quotientLow to form the final quotient.
            quotientLow += quotientMid;
            return (new UInt256(quotientHigh, quotientLow), new UInt256(0, remainderLow));
        }

        // When y.High is non-zero, perform a more complex division.
        // Start by making an initial guess for the quotient based on the high parts of x and y.
        var initialGuess = x.High / y.High;

        // Multiply the guess by y to approximate the multiplication result.
        var multiplication = y * new UInt256(0, initialGuess);

        // If the multiplication result is greater than x, reduce the guess by one.
        if (multiplication > x)
        {
            initialGuess -= 1;
            multiplication -= x;
        }

        // Return the quotient and the difference between x and the multiplied guess as the remainder.
        return (new UInt256(0, initialGuess), x - multiplication);
    }


    public static UInt256 operator /(UInt256 x, UInt256 y)
    {
        return DivMod(x, y).Item1;
    }

    public static UInt256 operator %(UInt256 x, UInt256 y)
    {
        return DivMod(x, y).Item2;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var y = Zero;
        var x = this;
        if (x == Zero) return "0";
        while (x > Zero)
        {
            (x, y) = DivMod(x, Ten);
            sb.Append(y.Low);
        }

        return new string(sb.ToString().Reverse().ToArray());
    }

    public static UInt256 ParseFromString(string input)
    {
        var x = Zero;
        var inputArray = input.ToArray();
        for (var i = 0; i < inputArray.Length; i += 1)
        {
            x = (x << 3) + (x << 1);
            var digit = (ulong)(inputArray[i] - '0');
            if (digit > 9) throw new ArgumentOutOfRangeException();
            x += new UInt256(0, new UInt128(0, digit));
        }

        return x;
    }
}
