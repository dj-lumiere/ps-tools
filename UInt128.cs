internal class UInt128
{
    public UInt128(UInt64 x, UInt64 y)
    {
        High = x;
        Low = y;
    }

    public UInt128() : this(0, 0)
    {
    }

    public UInt64 High { get; }
    public UInt64 Low { get; }

    public static UInt128 Zero => new(0, 0);
    public static UInt128 One => new(0, 1);
    public static UInt128 Two => new(0, 2);
    public static UInt128 Ten => new(0, 10);

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

    // x * y % 2^128
    public static UInt128 operator *(UInt128 x, UInt128 y)
    {
        var (xh, xl) = (x.High, x.Low);
        var (yh, yl) = (y.High, y.Low);

        var z0 = xl * yl;
        var z1 = xl * yh + xh * yl;

        var low = z0 + (z1 << 64);
        var high = z1 + (low < z0 ? 1UL : 0);
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
        for (var i = 0; i < inputArray.Length; i++)
        {
            x = (x << 3) + (x << 1);
            x += new UInt128(0, (UInt64)(inputArray[i] - '0'));
        }

        return x;
    }
}
