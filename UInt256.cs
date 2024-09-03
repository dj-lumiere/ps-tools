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

    public static UInt256 operator *(UInt256 x, UInt256 y)
    {
        var (xh, xl) = (x.High, x.Low);
        var (yh, yl) = (y.High, y.Low);

        var z0 = xl * yl;
        var z1 = xl * yh + xh * yl;
        var z2 = xh * yh;

        var low = z0 + (z1 << 64);
        var high = z2 + (z1 >> 64) + (low < z0 ? 1UL : 0);
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

    public static (UInt256, UInt256) DivMod(UInt256 x, UInt256 y)
    {
        if (y == Zero) throw new ArgumentException("Cannot divide by Zero");
        if (y.High == 0)
        {
            if (x.High == 0)
                // Simple 128-bit division
                return (new UInt256(0, x.Low / y.Low), new UInt256(0, x.Low % y.Low));

            // We must consider `x.High` as well
            var mask = new UInt128(0xffffffffffffffff, 0xffffffffffffffff);
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
            return (new UInt256(quotientHigh, quotientLow), new UInt256(0, remainderLow));
        }

        var initialGuess = x.High / y.High;
        var multiplication = y * new UInt256(0, initialGuess);
        if (multiplication > x)
        {
            initialGuess -= 1;
            multiplication -= x;
        }

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
        for (var i = 0; i < inputArray.Length; i++)
        {
            x = (x << 3) + (x << 1);
            var digit = (ulong)(inputArray[i] - '0');
            if (digit > 9)
            {
                throw new ArgumentOutOfRangeException();
            }
            x += new UInt256(0, new UInt128(0, digit));
        }

        return x;
    }
}
