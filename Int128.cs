using System.Text;

namespace ConsoleApp1;

internal class Int128
{
    public Int128(ulong x, ulong y)
    {
        High = x;
        Low = y;
    }

    public Int128() : this(0, 0)
    {
    }

    public ulong High { get; }
    public ulong Low { get; }

    public static Int128 Zero => new(0, 0);
    public static Int128 MinusOne => new(ulong.MaxValue, ulong.MaxValue);
    public static Int128 One => new(0, 1);
    public static Int128 Two => new(0, 2);
    public static Int128 Ten => new(0, 10);

    /// <summary>
    ///     Maximum value of Int128 : 170_141_183_460_469_231_731_687_303_715_884_105_727
    /// </summary>
    public static Int128 MaxValue => new(ulong.MaxValue >> 1, ulong.MaxValue);

    /// <summary>
    ///     Minimum value of Int128 : -170_141_183_460_469_231_731_687_303_715_884_105_728
    /// </summary>
    public static Int128 MinValue => new(1ul << 63, 0);

    private static Int128 Mask128 => new(ulong.MaxValue, ulong.MaxValue);

    // Implicit conversion from ulong to Int128
    public static implicit operator Int128(ulong value)
    {
        return new Int128(0, value);
    }

    // Implicit conversion from ulong to Int128
    public static implicit operator Int128(long value)
    {
        if (value < 0) return -new Int128(0, (ulong)-value);

        return new Int128(0, (ulong)value);
    }

    // Implicit conversion from string to Int128
    public static implicit operator Int128(string value)
    {
        return ParseFromString(value);
    }

    public static int Sign(Int128 value)
    {
        if (value.High >> 63 != 0) return -1;

        if (value == Zero) return 0;

        return 1;
    }

    public static Int128 Abs(Int128 value)
    {
        if (Sign(value) == -1) return -value;

        return value;
    }

    public static bool operator ==(Int128 x, Int128 y)
    {
        return x.High == y.High && x.Low == y.Low;
    }

    public static bool operator !=(Int128 x, Int128 y)
    {
        return !(x == y);
    }

    public static bool operator >(Int128 x, Int128 y)
    {
        if (Sign(x) > Sign(y)) return true;
        if (Sign(x) < Sign(y)) return false;
        if (Sign(x) == -1 && Sign(y) == -1)
        {
            x = -x;
            y = -y;
            return x.High < y.High || (x.High == y.High && x.Low < y.Low);
        }

        return x.High > y.High || (x.High == y.High && x.Low > y.Low);
    }

    public static bool operator >=(Int128 x, Int128 y)
    {
        return x > y || x == y;
    }

    public static bool operator <(Int128 x, Int128 y)
    {
        return !(x >= y);
    }

    public static bool operator <=(Int128 x, Int128 y)
    {
        return !(x > y);
    }

    public override bool Equals(object obj)
    {
        return obj is Int128 other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(High, Low);
    }

    public static Int128 operator +(Int128 x, Int128 y)
    {
        var x2 = x.High + y.High;
        var y2 = x.Low + y.Low;
        if (y2 < x.Low) x2 += 1;
        return new Int128(x2, y2);
    }

    public static Int128 operator -(Int128 x, Int128 y)
    {
        var x2 = x.High - y.High;
        var y2 = x.Low - y.Low;
        if (y2 > x.Low) x2 -= 1;
        return new Int128(x2, y2);
    }

    public static Int128 operator -(Int128 x)
    {
        return new Int128(~x.High, ~x.Low) + One;
    }

    public static Int128 operator *(Int128 x, Int128 y)
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
        return new Int128(high, low);
    }

    public static Int128 operator <<(Int128 x, int y)
    {
        if (y < 0) throw new ArgumentOutOfRangeException("y", y, "Cannot be negative");
        if (y == 0) return x;

        if (y < 64)
        {
            var high = (x.High << y) | (x.Low >> (64 - y));
            var low = x.Low << y;
            return new Int128(high, low);
        }

        if (y < 128)
        {
            // Shift crosses the 128-bit boundary
            var high = x.Low << (y - 64);
            return new Int128(high, 0);
        }

        return Zero;
    }

    public static Int128 operator >> (Int128 x, int y)
    {
        if (y < 0) throw new ArgumentOutOfRangeException("y", y, "Cannot be negative");
        if (y == 0) return x;
        if (y >= 128) return Sign(x) < 0 ? MinusOne : Zero;
        if (y < 64)
        {
            var low = (x.Low >> y) | (x.High << (64 - y));
            var high = (ulong)((long)x.High >> y);
            return new Int128(high, low);
        }

        else
        {
            // Shift crosses the 128-bit boundary
            var low = (ulong)((long)x.High >> (y - 64)); // Handle sign extension properly
            return new Int128((ulong)((long)x.High >> 63), low);
        }
    }

    public static Int128 operator &(Int128 x, Int128 y)
    {
        return new Int128(x.High & y.High, x.Low & y.Low);
    }

    public static Int128 operator |(Int128 x, Int128 y)
    {
        return new Int128(x.High | y.High, x.Low | y.Low);
    }

    public static (Int128, Int128) DivModAbsolute(Int128 x, Int128 y)
    {
        if (y.High == 0)
        {
            if (x.High == 0)
                // Simple 64-bit division
                return (new Int128(0, x.Low / y.Low), new Int128(0, x.Low % y.Low));
            var mask = 0xffffffffffffffff;
            // We must consider `x.High` as well
            var quotientHigh = x.High / y.Low;
            var remainderHigh = x.High % y.Low;
            var quotientMid = mask / y.Low;
            var remainderMid = mask % y.Low;
            var quotientLow = x.Low / y.Low;
            var remainderLow = x.Low % y.Low;
            remainderMid += 1;
            if (remainderMid >= y.Low)
            {
                quotientMid += 1;
                remainderMid -= y.Low;
            }

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
            return (new Int128(quotientHigh, quotientLow), new Int128(0, remainderLow));
        }

        var initialGuess = x.High / y.High;
        var multiplication = y * new Int128(0, initialGuess);
        if (multiplication > x)
        {
            initialGuess -= 1;
            multiplication -= x;
        }

        return (new Int128(0, initialGuess), x - multiplication);
    }

    public static (Int128, Int128) DivMod(Int128 x, Int128 y)
    {
        if (y == Zero) throw new ArgumentException("Cannot divide by Zero");
        var isXNegative = Sign(x) == -1;
        var isYNegative = Sign(y) == -1;
        x = Abs(x);
        y = Abs(y);
        var (quotient, remainder) = DivModAbsolute(x, y);

        if (isXNegative && isYNegative)
        {
            quotient = remainder != Zero ? quotient + One : quotient;
            remainder = remainder != Zero ? y - remainder : remainder;
        }

        else if (isXNegative)
        {
            quotient = remainder != Zero ? -(quotient + One) : quotient;
            remainder = remainder != Zero ? y - remainder : remainder;
        }

        else if (isYNegative)
        {
            quotient = -quotient;
        }

        return (quotient, remainder);
    }

    public static Int128 operator /(Int128 x, Int128 y)
    {
        return DivMod(x, y).Item1;
    }

    public static Int128 operator %(Int128 x, Int128 y)
    {
        return DivMod(x, y).Item2;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var y = Zero;
        var x = this;
        var isNegative = false;
        if (x == Zero) return "0";
        if (x < Zero)
        {
            isNegative = true;
            x = -x;
        }

        while (x > Zero)
        {
            (x, y) = DivMod(x, Ten);
            sb.Append(y.Low);
        }

        if (isNegative) sb.Append("-");

        return new string(sb.ToString().Reverse().ToArray());
    }

    public static string Hex(Int128 x)
    {
        var sb = new StringBuilder();
        var y = Zero;
        var isNegative = false;
        if (x == Zero) return "0x0";
        if (x < Zero)
        {
            isNegative = true;
            x = -x;
        }

        while (x > Zero)
        {
            (x, y) = (x >> 4, x.Low & 0xf);
            if (y.Low < 10)
                sb.Append(y.Low);
            else
                sb.Append((char)('A' + (y.Low - 10)));
        }

        sb.Append("x0");
        if (isNegative) sb.Append("-");

        return new string(sb.ToString().Reverse().ToArray());
    }

    public static string Bin(Int128 x)
    {
        var sb = new StringBuilder();
        var y = Zero;
        var isNegative = false;
        if (x == Zero) return "0b0";
        if (x < Zero)
        {
            isNegative = true;
            x = -x;
        }

        while (x > Zero)
        {
            (x, y) = (x >> 1, x.Low & 1);
            sb.Append(y.Low);
        }

        sb.Append("b0");
        if (isNegative) sb.Append("-");

        return new string(sb.ToString().Reverse().ToArray());
    }

    public static Int128 ParseFromString(string input)
    {
        var x = Zero;
        var isNegative = false;
        var inputArray = input.ToArray();
        for (var i = 0; i < inputArray.Length; i++)
        {
            if (i == 0 && inputArray[i] == '-')
            {
                isNegative = true;
                continue;
            }

            var digit = (ulong)(inputArray[i] - '0');
            if (digit > 9) throw new ArgumentOutOfRangeException();
            x = (x << 3) + (x << 1);
            x += new Int128(0, digit);
        }

        if (isNegative) return -x;
        return x;
    }
}
