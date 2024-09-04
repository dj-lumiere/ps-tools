using System.Text;

namespace ConsoleApp1;

internal class UInt128
{
    public UInt128(ulong x, ulong y)
    {
        High = x;
        Low = y;
    }

    public UInt128() : this(0, 0)
    {
    }

    public ulong High { get; }
    public ulong Low { get; }

    public static UInt128 Zero => new(0, 0);
    public static UInt128 One => new(0, 1);
    public static UInt128 Two => new(0, 2);
    public static UInt128 Ten => new(0, 10);

    /// <summary>
    ///     Maximum value of UInt128 : 340_282_366_920_938_463_463_374_607_431_768_211_455
    /// </summary>
    public static UInt128 MaxValue => new(ulong.MaxValue, ulong.MaxValue);

    // Implicit conversion from ulong to UInt128
    public static implicit operator UInt128(ulong value)
    {
        return new UInt128(0, value);
    }

    // Implicit conversion from string to UInt128
    public static implicit operator UInt128(string value)
    {
        return ParseFromString(value);
    }

    public static bool operator ==(UInt128 x, UInt128 y)
    {
        return x.High == y.High && x.Low == y.Low;
    }

    public static bool operator !=(UInt128 x, UInt128 y)
    {
        return !(x == y);
    }

    public static bool operator >(UInt128 x, UInt128 y)
    {
        return x.High > y.High || (x.High == y.High && x.Low > y.Low);
    }

    public static bool operator >=(UInt128 x, UInt128 y)
    {
        return x > y || x == y;
    }

    public static bool operator <(UInt128 x, UInt128 y)
    {
        return !(x >= y);
    }

    public static bool operator <=(UInt128 x, UInt128 y)
    {
        return !(x > y);
    }

    public override bool Equals(object obj)
    {
        return obj is UInt128 other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(High, Low);
    }

    public static UInt128 operator +(UInt128 x, UInt128 y)
    {
        var x2 = x.High + y.High;
        var y2 = x.Low + y.Low;
        if (y2 < x.Low) x2 += 1;
        return new UInt128(x2, y2);
    }

    public static UInt128 operator -(UInt128 x, UInt128 y)
    {
        var x2 = x.High - y.High;
        var y2 = x.Low - y.Low;
        if (y2 > x.Low) x2 -= 1;
        return new UInt128(x2, y2);
    }

    /// <summary>
    ///     Multiplies two UInt128 values and returns the result modulo 2^128.
    /// </summary>
    /// <param name="x">The first UInt128 operand.</param>
    /// <param name="y">The second UInt128 operand.</param>
    /// <returns>The product of x and y, modulo 2^128.</returns>
    public static UInt128 operator *(UInt128 x, UInt128 y)
    {
        // Decompose the operands x and y into their high and low 64-bit parts.
        var (xh, xl) = (x.High, x.Low);
        var (yh, yl) = (y.High, y.Low);

        // Further decompose the 64-bit parts into 32-bit chunks.
        // x = x3<<96 + x2<<64 + x1<<32 + x0
        var (x3, x2) = (xh >> 32, xh & 0xffffffff);
        var (x1, x0) = (xl >> 32, xl & 0xffffffff);

        // y = y3<<96 + y2<<64 + y1<<32 + y0
        var (y3, y2) = (yh >> 32, yh & 0xffffffff);
        var (y1, y0) = (yl >> 32, yl & 0xffffffff);

        // Perform multiplication while considering the modulus 2^128.
        // The result of x * y is decomposed into z = z3<<96 + z2<<64 + z1<<32 + z0
        // where each zN is computed based on the components of x and y.
        var z0 = x0 * y0; // Compute z0 = x0 * y0
        var z1 = x1 * y0; // Compute z1 = x1 * y0
        var z2 = x2 * y0 + x1 * y1 + x0 * y2; // Compute z2 = x2 * y0 + x1 * y1 + x0 * y2
        var z3 = x3 * y0 + x2 * y1 + x1 * y2 + x0 * y3; // Compute z3 = x3 * y0 + x2 * y1 + x1 * y2 + x0 * y3

        // Add the contribution of x0 * y1 to z1.
        z1 += x0 * y1;

        // If adding x0 * y1 causes an overflow in z1, increment z3 to account for the overflow.
        if (z1 < x0 * y1) z3 += 1;

        // Compute the lower 64 bits of the product.
        var low = z0 + (z1 << 32);

        // Compute the higher 64 bits of the product.
        var high = (z1 >> 32) + z2 + (z3 << 32);

        // If adding z0 to z1<<32 causes an overflow in low, increment high to account for it.
        if (low < z0) high += 1;

        // Print the computed low and high parts (for debugging purposes).
        //Console.WriteLine($"low={low}, high={high}");

        // Return the final result as a new UInt128 composed of the computed high and low parts.
        return new UInt128(high, low);
    }

    public static UInt128 operator <<(UInt128 x, int y)
    {
        if (y == 0) return x;

        if (y < 64)
        {
            var high = (x.High << y) | (x.Low >> (64 - y));
            var low = x.Low << y;
            return new UInt128(high, low);
        }
        else
        {
            // Shift crosses the 128-bit boundary
            var high = x.Low << (y - 64);
            return new UInt128(high, 0);
        }
    }

    public static UInt128 operator >> (UInt128 x, int y)
    {
        if (y == 0) return x;

        if (y < 64)
        {
            var low = (x.Low >> y) | (x.High << (64 - y));
            var high = x.High >> y;
            return new UInt128(high, low);
        }
        else
        {
            // Shift crosses the 128-bit boundary
            var low = x.High >> (y - 64);
            return new UInt128(0, low);
        }
    }

    public static UInt128 operator &(UInt128 x, UInt128 y)
    {
        return new UInt128(x.High & y.High, x.Low & y.Low);
    }

    public static UInt128 operator |(UInt128 x, UInt128 y)
    {
        return new UInt128(x.High | y.High, x.Low | y.Low);
    }

    public static (UInt128, UInt128) DivMod(UInt128 x, UInt128 y)
    {
        if (y == Zero) throw new ArgumentException("Cannot divide by Zero");
        if (y.High == 0)
        {
            if (x.High == 0)
                // Simple 64-bit division
                return (new UInt128(0, x.Low / y.Low), new UInt128(0, x.Low % y.Low));
            // We must consider `x.High` as well
            // a = x.High, b = x.Low, c = y.Low
            // (a*2^64+b)/c = (a/c)*2^64+b/c+(2^64*a%c)/c
            // (a*2^64+b)%c = ((a%c)*(2^64%c)%c+b%c)%c.
            var mask = 0xffffffffffffffff;
            var quotientMid = mask / y.Low;
            var remainderMid = mask % y.Low;
            remainderMid += 1;
            if (remainderMid >= y.Low)
            {
                quotientMid += 1;
                remainderMid -= y.Low;
            }

            var quotientHigh = x.High / y.Low;
            var remainderHigh = x.High % y.Low;
            var quotientLow = x.Low / y.Low;
            var remainderLow = x.Low % y.Low;
            quotientMid *= remainderHigh;
            remainderMid *= remainderHigh;
            quotientMid += remainderMid / y.Low;
            remainderMid %= y.Low;
            remainderLow += remainderMid;
            if (remainderLow >= y.Low)
            {
                quotientLow += 1;
                remainderLow -= y.Low;
            }

            quotientLow += quotientMid;
            return (new UInt128(quotientHigh, quotientLow), new UInt128(0, remainderLow));
        }

        var initialGuess = x.High / y.High;
        var multiplication = y * new UInt128(0, initialGuess);
        if (multiplication > x)
        {
            initialGuess -= 1;
            multiplication -= x;
        }

        return (new UInt128(0, initialGuess), x - multiplication);
    }

    public static UInt128 operator /(UInt128 x, UInt128 y)
    {
        return DivMod(x, y).Item1;
    }

    public static UInt128 operator %(UInt128 x, UInt128 y)
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

    public static UInt128 ParseFromString(string input)
    {
        var x = Zero;
        var inputArray = input.ToArray();
        for (var i = 0; i < inputArray.Length; i += 1)
        {
            var digit = (ulong)(inputArray[i] - '0');
            if (digit > 9) throw new ArgumentOutOfRangeException();
            x = (x << 3) + (x << 1);
            x += new UInt128(0, digit);
        }

        return x;
    }
}
