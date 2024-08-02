using System.Security.Cryptography;
using System.Linq;
using System;

using Unknown6656.Mathematics.LinearAlgebra;
using Unknown6656.Mathematics.Cryptography;
using Unknown6656.Mathematics.Analysis;
using Unknown6656.Generics;

using netrandom = System.Random;

namespace Unknown6656.Mathematics.Numerics;

// This could hugely benefit from the 'Shapes and Extensions' feature.


public abstract class Random
    : netrandom
{
    public static Random BuiltinRandom { get; } = new _builtin();
    public static Random XorShift { get; } = new XorShift();

    public long Seed { get; }


    public Random()
        : this(Guid.NewGuid().BinaryCast<Guid, long>() ^ DateTime.UtcNow.Ticks)
    {
    }

    public Random(long seed)
        : base(seed.GetHashCode())
    {
        Seed = seed;

        Init();
    }


    protected abstract void Init();

    public abstract override int Next();

    #region BOOLEAN

    public bool NextBool() => NextBool(.5);

    public bool NextBool(Scalar true_probability) => NextDouble() >= true_probability;

    public T Choose<T>(T left, T right) => Choose(left, right, .5);

    public T Choose<T>(T left, T right, Scalar right_probability) => NextBool(right_probability) ? right : left;

    public ref T Choose<T>(ref T left, ref T right) => ref Choose(ref left, ref right, .5);

    public ref T Choose<T>(ref T left, ref T right, Scalar right_probability) => ref NextBool(right_probability) ? ref right : ref left;

    public ref T Choose<T>(params T[] items) => ref items[Next(items.Length)];

    #endregion
    #region INTEGER

    public byte NextByte() => (byte)(Next() & 0xff);

    public sbyte NextSByte() => (sbyte)NextByte();

    public char NextChar() => (char)NextUInt16();

    public short NextInt16() => (short)NextUInt16();

    public ushort NextUInt16() => (ushort)(Next() & 0xffff);

    public override int Next(int max) => (int)((uint)Next() / ((float)uint.MaxValue + 1) * max);

    public override int Next(int min, int max) => min + Next(max - min);

    public int NextInt32() => Next();

    public int NextInt32(int max) => Next(max);

    public int NextInt32(int min, int max) => Next(min, max);

    public uint NextUInt32() => (uint)Next();

    public override long NextInt64() => NextBinary<long>();

    public override long NextInt64(long max) => (long)((ulong)NextInt64() / ((double)ulong.MaxValue + 1) * max);

    public override long NextInt64(long min, long max) => min + NextInt64(max - min);

    public ulong NextUInt64() => NextBinary<ulong>();

    public UInt128 NextUInt128() => NextBinary<UInt128>();

    #endregion
    #region FLOATING POINT

    public Half NextHalf() => (Half)NextDouble();

    public override float NextSingle() => (float)NextDouble();

    public override double NextDouble() => ((double)NextUInt64() / long.MaxValue) % 1d;

    protected override double Sample() => NextDouble();

    public decimal NextDecimal() => (decimal)NextDouble();
#if F16
    public Scalar NextScalar() => NextHalf();
#elif F32
    public Scalar NextScalar() => NextSingle();
#elif F64
    public Scalar NextScalar() => NextDouble();
#endif
    public Scalar NextScalar(Scalar max) => NextScalar() * max;

    public Scalar NextScalar(Scalar min, Scalar max) => min + NextScalar(max - min);

    public Complex NextComplex() => new(NextScalar(), NextScalar());

    public Complex NextComplex(Scalar length) => NextComplex().Normalized * length;

    #endregion
    #region SEQUENCES

    public byte[] NextBytes(int count)
    {
        byte[] buffer = new byte[count];

        NextBytes(buffer);

        return buffer;
    }

    public override void NextBytes(byte[] buffer) => NextBytes(buffer, 0, buffer.Length);

    public void NextBytes(byte[] buffer, int offset) => NextBytes(buffer, offset, buffer.Length - offset);

    public void NextBytes(byte[] buffer, int offset, int length) => NextBytes(new Span<byte>(buffer), offset, length);

    public override void NextBytes(Span<byte> buffer) => NextBytes(buffer, 0, buffer.Length);

    public void NextBytes(Span<byte> buffer, int offset) => NextBytes(buffer, offset, buffer.Length - offset);

    public virtual unsafe void NextBytes(Span<byte> buffer, int offset, int length)
    {
        fixed (byte* bptr = buffer)
        {
            int* iptr = (int*)(bptr + offset);
            int i = 0;

            while (i < length)
                if (length - i > 4)
                {
                    iptr[i / 4] = Next();
                    i += 4;
                }
                else
                    bptr[i++ + offset] = NextByte();
        }
    }

    public sbyte[] NextSBytes(int count) => Enumerable.Range(0, count).ToArray(_ => NextSByte());

    public char[] NextChars(int count) => Enumerable.Range(0, count).ToArray(_ => NextChar());

    public short[] NextInt16s(int count) => Enumerable.Range(0, count).ToArray(_ => NextInt16());

    public ushort[] NextUInt16s(int count) => Enumerable.Range(0, count).ToArray(_ => NextUInt16());

    public Half[] NextHalves(int count) => Enumerable.Range(0, count).ToArray(_ => NextHalf());

    public float[] NextSingles(int count) => Enumerable.Range(0, count).ToArray(_ => NextSingle());

    public double[] NextDoubles(int count) => Enumerable.Range(0, count).ToArray(_ => NextDouble());

    public decimal[] NextDecimals(int count) => Enumerable.Range(0, count).ToArray(_ => NextDecimal());

    public Half NextGaussian(Half mean, Half deviation) => (Half)NextGaussian((double)mean, (double)deviation);

    public float NextGaussian(float mean, float deviation) => (float)NextGaussian((double)mean, deviation);

    public decimal NextGaussian(decimal mean, decimal deviation) => (decimal)NextGaussian((double)mean, (double)deviation);

    public Scalar NextGaussian(Scalar mean, Scalar deviation) => NextGaussian(mean.Determinant, deviation.Determinant);

    public double NextGaussian(double mean, double deviation)
    {
        double u1 = 1 - NextDouble();
        double u2 = 1 - NextDouble();

        return mean + deviation * Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(Math.Tau * u2);
    }

    public float[] NextGaussians(int count, float mean, float deviation) => Enumerable.Range(0, count).ToArray(_ => NextGaussian(mean, deviation));

    public decimal[] NextGaussians(int count, decimal mean, decimal deviation) => Enumerable.Range(0, count).ToArray(_ => NextGaussian(mean, deviation));

    public Scalar[] NextGaussians(int count, Scalar mean, Scalar deviation) => Enumerable.Range(0, count).ToArray(_ => NextGaussian(mean, deviation));

    public double[] NextGaussians(int count, double mean, double deviation) => Enumerable.Range(0, count).ToArray(_ => NextGaussian(mean, deviation));

    public Scalar[] NextScalars(int count) => Enumerable.Range(0, count).ToArray(_ => NextScalar());

    public Scalar[] NextScalars(int count, Scalar max) => Enumerable.Range(0, count).ToArray(_ => NextScalar(max));

    public Scalar[] NextScalars(int count, Scalar min, Scalar max) => Enumerable.Range(0, count).ToArray(_ => NextScalar(min, max));

    public Complex[] NextComplexes(int count, Scalar length) => Enumerable.Range(0, count).ToArray(_ => NextComplex(length));

    public Complex[] NextComplexes(int count) => Enumerable.Range(0, count).ToArray(_ => NextComplex());

    public int[] NextInt32s(int count) => Enumerable.Range(0, count).ToArray(_ => NextInt32());

    public uint[] NextUInt32s(int count) => Enumerable.Range(0, count).ToArray(_ => NextUInt32());

    public long[] NextInt64s(int count) => Enumerable.Range(0, count).ToArray(_ => NextInt64());

    public ulong[] NextUInt64s(int count) => Enumerable.Range(0, count).ToArray(_ => NextUInt64());

    public UInt128[] NextUInt128s(int count) => Enumerable.Range(0, count).ToArray(_ => NextUInt128());

    public bool[] NextBools(int count) => Enumerable.Range(0, count).ToArray(_ => NextBool());

    public bool[] NextBools(int count, Scalar true_probability) => Enumerable.Range(0, count).ToArray(_ => NextBool(true_probability));

    #endregion
    #region GENERICS

    public unsafe T NextBinary<T>()
        where T : unmanaged
    {
        T res = default;
        byte* ptr = (byte*)&res;
        int remaining = sizeof(T);

        while (remaining > 0)
            if (remaining >= sizeof(int))
            {
                *(int*)ptr = Next();
                ptr += sizeof(int);
                remaining -= sizeof(int);
            }
            else
            {
                *ptr = NextByte();
                ++ptr;
                --remaining;
            }

        return res;
    }

    public T[] NextBinary<T>(int count) where T : unmanaged => Enumerable.Range(0, count).ToArray(_ => NextBinary<T>());

    #endregion


    private sealed class _builtin
        : Random
    {
        private readonly netrandom _random;


        public _builtin() : base() => _random = new();

        public _builtin(long seed) : base(seed) => _random = new((int)seed);

        protected override void Init()
        {
        }

        public override int Next() => _random.Next();
    }
}

public sealed class XorShift
    : Random
{
    private volatile uint _x, _y, _z, _w;


    public XorShift()
        : base()
    {
    }

    public XorShift(long seed)
        : base((int)seed)
    {
    }

    protected override void Init()
    {
        _x = (uint)(Seed & 0xffffffffu);
        _y = (uint)(Seed >> 32);
        _z = _x.ROL((int)_y);
        _w = _x ^ (_z ^ _y).ROR((int)_x);

        if (_x == 0)
            _x = 0x6a598431;

        if (_y == 0)
            _y = 0x94b86cf9;

        if (_z == 0)
            _z = (uint)(_x ^ Seed ^ _y);

        if (_w == 0)
            _w = _x.ROL((int)_y) ^ _y.ROL((int)_x) + 1;
    }

    public override int Next()
    {
        uint s = _x;
        uint t = _w;

        _w = _z;
        _z = _y;
        _y = s;

        t ^= t << 11;
        t ^= t >> 8;
        _x = t ^ s ^ (s >> 19);

        return (int)_x;
    }
}

public sealed class CongruenceGenerator
    : Random
{
    private ModuloRing _x;


    protected override void Init() => (_x, _) = TextbookRSA.GenerateKeyPair();

    public override int Next() => (_x = _x.Power(2)).GetHashCode();
}

public sealed class CryptoRandom<RNG>(RNG rng)
    : Random
    where RNG : RandomNumberGenerator
{
    public RNG RandomNumberGenerator { get; } = rng;


    public override unsafe int Next()
    {
        byte[] bytes = new byte[sizeof(int)];

        RandomNumberGenerator.GetBytes(bytes);

        fixed (byte* ptr = bytes)
            return *(int*)ptr;
    }

    protected override void Init()
    {
    }
}

public sealed class TurbulenceShift
    : Random
{
    private volatile int _state;


    public TurbulenceShift()
        : base()
    {
    }

    public TurbulenceShift(long seed)
        : base(seed)
    {
    }

    protected override void Init() => _state = Seed.GetHashCode();

    public override int Next()
    {
        uint i = (uint)_state;

        i ^= 2747636419u;
        i *= 2654435769u;
        i ^= i >> 16;
        i *= 2654435769u;
        i ^= i >> 16;
        i *= 2654435769u;

        return _state = (int)i;
    }
}


// TODO : mersenne twister https://de.wikipedia.org/wiki/Mersenne-Twister

public sealed class MersenneTwisterMT19937
{
}

public sealed class MersenneTwisterTT800
{
}
