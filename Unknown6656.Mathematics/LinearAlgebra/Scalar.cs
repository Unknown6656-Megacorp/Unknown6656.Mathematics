using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using System;

using Unknown6656.Mathematics.Analysis;
using Unknown6656.Mathematics.Numerics;
using Unknown6656.Generics;
using Unknown6656.Common;

namespace Unknown6656.Mathematics.LinearAlgebra;


/// <summary>
/// Represents a scalar value (a single number) in mathematics.
/// This struct internally wraps the datatype <see cref="__scalar"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = sizeof(__scalar), Pack = sizeof(__scalar)), NativeCppClass, Serializable, CLSCompliant(false)]
public unsafe readonly partial struct Scalar
    : IScalar<Scalar>
    , Algebra<Scalar>.IMatrix<Scalar, Scalar>
    , Algebra<Scalar>.IVector<Scalar, Scalar>
    , Algebra<Scalar, Polynomial>.IComposite1D
    , Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>
    , num.IFloatingPointIeee754<Scalar>
    , IComparable<Scalar>
    , IEquatable<Scalar>
    , IComparable
    , IDisplayable
    , ICloneable
{
    #region CONSTS AND FIELDS

    /// <inheritdoc cref="IMatrix{M,S}.Size"/>
    public const int Dimension = 1;

    /// <summary>
    /// The default value for <see cref="ComputationalEpsilon"/> (1e-9).
    /// </summary>
    public const double DefaultComputationalEpsilon = 1e-9;

    private static Scalar _cepsilon = DefaultComputationalEpsilon;

    internal static readonly Regex REGEX_BIN = new(@"^[+\-]?(?<num>0b[01]+|[01]+b)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    internal static readonly Regex REGEX_OCT = new(@"^[+\-]?(?<num>0o[0-7]+|[0-7]+o)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    internal static readonly Regex REGEX_HEX = new(@"^[+\-]?(?<num>0x[0-9a-f]+|[0-9a-f]+h)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    internal static readonly Regex REGEX_DEC1 = new(@"^[+\-]?((?<const>π|pi|τ|tau|φ|phi|e)\*?)?(?<factor>(\d*\.)?\d+(e[+\-]?\d+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    internal static readonly Regex REGEX_DEC2 = new(@"^[+\-]?((?<factor>(\d*\.)?\d+(e[+\-]?\d+)?)\*?)?(?<const>π|pi|τ|tau|φ|phi|e)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex REGEX_SCALAR = new(@"^[+\-]?(0b[01]+|[01]+b|0o[0-7]+|[0-7]+o|0x[0-9a-f]+|[0-9a-f]+h|((π|pi|τ|tau|φ|phi|e)\*?)?((\d*\.)?\d+(e[+\-]?\d+)?)|(((\d*\.)?\d+(e[+\-]?\d+)?)\*?)?(π|pi|τ|tau|φ|phi|e))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion
    #region STATIC PROPERTIES

#pragma warning disable IDE1006
    public static Scalar π { get; } = new(__scalar.Pi);

    public static Scalar τ { get; } = new(__scalar.Tau);

    public static Scalar e { get; } = new(__scalar.E);
#pragma warning restore IDE1006
    static Scalar num.IFloatingPointConstants<Scalar>.E => e;

    public static Scalar Pi => π;

    public static Scalar Tau => τ;

    public static Scalar PiHalf => π * .5;

    public static Scalar Sqrt2 { get; } = new(__scalar.Sqrt(2));

    public static Scalar GoldenRatio { get; } = 1.618033988749894848204586834;

    public static Scalar IEEE754Epsilon { get; } = new(__scalar.Epsilon);

    static Scalar num.IFloatingPointIeee754<Scalar>.Epsilon => IEEE754Epsilon;

    public static Scalar ComputationalEpsilon
    {
        get => _cepsilon;
        set
        {
            _cepsilon = value * value > IEEE754Epsilon && value < 1 ? value
                : throw new ArgumentOutOfRangeException(nameof(value), $"The computational epislon must be inside the exclusive range ({IEEE754Epsilon.Sqrt():e0}..1).");
        }
    }

    public static Scalar MinValue { get; } = new(__scalar.MinValue);

    public static Scalar MaxValue { get; } = new(__scalar.MaxValue);

    public static Scalar NegativeInfinity { get; } = new(__scalar.NegativeInfinity);

    public static Scalar PositiveInfinity { get; } = new(__scalar.PositiveInfinity);

    public static Scalar NegativeZero { get; } = new(__scalar.NegativeZero);

    public static Scalar NaN { get; } = new(__scalar.NaN);

    /// <summary>
    /// The 1x1 zero matrix. This is euqal to the scalar value of 0.
    /// </summary>
    public static Scalar Zero { get; } = new Scalar((__scalar)0);

    /// <summary>
    /// The 1x1 identity (unit) matrix. This is euqal to the scalar value of 1.
    /// </summary>
    public static Scalar One { get; } = new Scalar((__scalar)1);

    public static Scalar NegativeOne { get; } = new Scalar((__scalar)(-1));

    public static Scalar Two { get; } = new Scalar((__scalar)2);

    /// <summary>
    /// The raw memory size of the <see cref="Scalar"/>-structure in bytes.
    /// </summary>
    public static int BinarySize { get; } = sizeof(Scalar);

    static int num.INumberBase<Scalar>.Radix => 2;

    static Scalar num.IAdditiveIdentity<Scalar, Scalar>.AdditiveIdentity => Zero;

    static Scalar num.IMultiplicativeIdentity<Scalar, Scalar>.MultiplicativeIdentity => One;

    public static ScalarEqualityComparer EqualityComparer { get; } = new ScalarEqualityComparer();

    #endregion
    #region INDEXERS

    public readonly Scalar this[int index] => this[index, 0];

    public readonly Scalar this[int index, Scalar value] => this[index, 0, value];

    public readonly Scalar this[int index, in Scalar value] => this[index, 0, value];

    public readonly Scalar this[int column, int row] => (column, row) == (0, 0) ? this : throw new ArgumentException($"Invalid indices: The type '{typeof(Scalar).FullName}' only holds one value and therefore can only be addressed using the indices (0,0).");

    public readonly Scalar this[int column, int row, Scalar value] => (column, row) == (0, 0) ? value : throw new ArgumentException($"Invalid indices: The type '{typeof(Scalar).FullName}' only holds one value and therefore can only be addressed using the indices (0,0).");

    #endregion
    #region INSTANCE PROPERTIES

    public readonly __scalar Determinant { get; }

    public readonly bool IsNaN => __scalar.IsNaN(Determinant);

    public readonly bool IsNegative => Determinant < 0;

    public readonly bool IsPositive => Determinant > 0;

    public readonly bool IsPositiveDefinite => IsPositive && IsFinite;

    public readonly bool IsFinite => __scalar.IsFinite(Determinant);

    public readonly bool IsInfinity => __scalar.IsInfinity(Determinant);

    public readonly bool IsNegativeInfinity => __scalar.IsNegativeInfinity(Determinant);

    public readonly bool IsPositiveInfinity => __scalar.IsPositiveInfinity(Determinant);

    public readonly Scalar MultiplicativeInverse => Inverse;

    public readonly bool IsInvertible => IsNonZero;

    public readonly bool IsIdentity => Is(One);

    public readonly bool IsOne => Is(One);

    public readonly Scalar AdditiveInverse => Negate();

    public readonly bool IsZero => Is(Zero);

    public readonly bool IsNonZero => !IsZero;

    public readonly bool IsOdd => Decrement().IsEven;

    public readonly bool IsEven => IsMultipleOf(Two);

    public readonly bool IsBinary => IsZero || IsOne;

    public readonly bool IsBetweenZeroAndOne => this >= Zero && this <= One;

    public readonly bool IsInsideUnitSphere => AbsoluteValue.IsBetweenZeroAndOne;

    public readonly Scalar Inverse => new(1 / Determinant);

    public readonly Scalar AbsoluteValue => Abs();

    public readonly Polynomial CharacteristicPolynomial => new(-Determinant, 1);

    public readonly int Sign => CompareTo(Zero);

    public readonly Scalar DecimalPlaces => Subtract(Floor);

    public readonly Scalar Floor => new(__scalar.Floor(Determinant));

    public readonly Scalar Ceiling => new(__scalar.Ceiling(Determinant));

    public readonly Scalar Rounded => new(__scalar.Round(Determinant));

    public readonly bool IsInteger => Is(Floor);

    public readonly bool IsRational => Is(ToFraction(ComputationalEpsilon * ComputationalEpsilon).AsScalar);

    public readonly bool IsPrime => IsPositive && IsInteger && ((bint)Determinant).IsPrime();

    public readonly Scalar[] PrimeFactors => IsInteger && IsPositive ? ((bint)Determinant).PrimeFactorization().Select(b => new Scalar((__scalar)b)).ToArray() : [];

    public readonly Scalar Phi => PrimeFactors is { Length: 2 } f ? (__scalar)((f[0] - 1) * (f[1] - 1)) : throw new InvalidOperationException($"φ({this}) is not defined.");

    #endregion
    #region EXPLICIT INSTANCE PROPERTIES

    readonly Scalar Algebra<Scalar>.IMetricVectorSpace<Scalar>.SquaredNorm => Multiply(this);

    readonly Scalar Algebra<Scalar>.IMetricVectorSpace<Scalar>.Normalized => One;

    readonly bool Algebra<Scalar>.IMetricVectorSpace.IsNormalized => IsOne;

    readonly Scalar Algebra<Scalar>.IMetricVectorSpace.Length => Abs();

    readonly Scalar[] Algebra<Scalar>.IComposite1D.Coefficients => [this];

    readonly Scalar[,] Algebra<Scalar>.IComposite2D.Coefficients => new[,] { { this } };

    readonly int Algebra<Scalar>.IComposite1D.Dimension => 1;

    readonly bool Algebra<Scalar>.IComposite.HasNegatives => IsNegative;

    readonly bool Algebra<Scalar>.IComposite.HasPositives => IsPositive;

    readonly bool Algebra<Scalar>.IComposite.HasNaNs => IsNaN;

    readonly Scalar Algebra<Scalar>.IComposite.CoefficientSum => this;

    readonly Scalar Algebra<Scalar>.IComposite.CoefficientAvg => this;

    readonly Scalar Algebra<Scalar>.IComposite.CoefficientMin => this;

    readonly Scalar Algebra<Scalar>.IComposite.CoefficientMax => this;

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar>.GaussianReduced => IsZero ? Zero : One;

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar>.OrthonormalBasis => IsZero ? Zero : One;

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar>.Transposed => this;

    readonly Scalar Algebra<Scalar>.IMatrix.Determinant => this;

    readonly Scalar Algebra<Scalar>.IMatrix.Trace => this;

    readonly bool Algebra<Scalar>.IMatrix.IsDiagonal => true;

    readonly bool Algebra<Scalar>.IMatrix.IsUpperTriangular => true;

    readonly bool Algebra<Scalar>.IMatrix.IsLowerTriangular => true;

    readonly bool Algebra<Scalar>.IMatrix.IsSymmetric => true;

    readonly bool Algebra<Scalar>.IMatrix.IsProjection => IsOne || IsZero;

    readonly bool Algebra<Scalar>.IMatrix.IsConferenceMatrix => Is(Multiply(this));

    readonly bool Algebra<Scalar>.IMatrix.IsInvolutory => Multiply(Inverse).IsOne;

    readonly bool Algebra<Scalar>.IMatrix.IsHurwitzStable => IsPositive;

    readonly bool Algebra<Scalar>.IMatrix.IsOrthogonal => IsOne;

    readonly bool Algebra<Scalar>.IMatrix.IsSkewSymmetric => IsZero;

    readonly bool Algebra<Scalar>.IMatrix.IsSignMatrix => IsZero || Abs().IsOne;

    readonly bool Algebra<Scalar>.IMatrix.IsSignatureMatrix => Abs().IsOne;

    readonly Scalar[] Algebra<Scalar>.IMatrix.Eigenvalues => IsZero ? [] : [this];

    readonly IEnumerable<Scalar> Algebra<Scalar>.IComposite2D.FlattenedCoefficients => [this];

    readonly (int Columns, int Rows) Algebra<Scalar>.IComposite2D.Dimensions => (1, 1);

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.MainDiagonal => this;

    readonly Scalar[] Algebra<Scalar>.IMatrix<Scalar, Scalar>.Columns => [this];

    readonly Scalar[] Algebra<Scalar>.IMatrix<Scalar, Scalar>.Rows => [this];

    readonly int Algebra<Scalar>.IMatrix.Rank => IsZero ? 0 : 1;

    readonly Scalar Algebra<Scalar>.IVector<Scalar, Scalar>.HouseholderMatrix => IsZero ? throw new InvalidOperationException("The Householder matrix is undefined for zero vectors.") : Two;

    #endregion
    #region .CTOR / .CCTOR / .DTOR

    public Scalar(__scalar value) => Determinant = value;

    public Scalar(params __scalar[] values)
        : this(values[0])
    {
    }

    public Scalar(IEnumerable<__scalar> values)
        : this(values.ToArray())
    {
    }

    public Scalar(Scalar scalar) => Determinant = scalar.Determinant;

    public Scalar(Scalar* scalar)
        : this(*scalar)
    {
    }

    #endregion
    #region INSTANCE METHODS

    public readonly Scalar Abs() => IsNegative ? Negate() : this;

    public readonly Scalar Negate() => new(-Determinant);

    public readonly Scalar Add(Scalar second) => new(Determinant + second.Determinant);

    public readonly Scalar Add(in Scalar second) => new(Determinant + second.Determinant);

    public readonly Scalar Add(params Scalar[] others) => others.Aggregate(this, Add);

    public readonly Scalar Subtract(Scalar second) => new(Determinant - second.Determinant);

    public readonly Scalar Subtract(in Scalar second) => new(Determinant - second.Determinant);

    public readonly Scalar Subtract(params Scalar[] others) => others.Aggregate(this, Subtract);

    public readonly Scalar DistanceTo(Scalar other) => Subtract(other).Abs();

    public readonly Scalar Increment() => Add(One);

    public readonly Scalar Decrement() => Subtract(One);

    public readonly Scalar Dot(in Scalar second) => Multiply(in second);

    public readonly Scalar Multiply(Scalar second) => Multiply(in second);

    public readonly Scalar Multiply(in Scalar second)
    {
        // if (IsZero || second.IsZero)
        //     return Zero;
        // else if (IsOne)
        //     return second;
        // else if (second.IsOne)
        //     return this;
        // else
            return new(Determinant * second.Determinant);
    }

    public readonly Scalar Multiply(params Scalar[] others) => others.Aggregate(this, Multiply);

    public readonly Scalar Divide(Scalar second) => Divide(in second);

    public readonly Scalar Divide(in Scalar second) => /* second.IsOne ? this : */ new(Determinant / second.Determinant);

    public readonly (Scalar Factor, Scalar Remainder) DivideModulus(in Scalar second) => (Divide(in second), Modulus(in second));

    public readonly Scalar Modulus(Scalar second) => Modulus(in second);

    public readonly Scalar Modulus(in Scalar second) => new(Determinant % second.Determinant);

    public readonly Scalar Power(int e) => e switch {
        0 => One,
        1 => this,
        2 => Multiply(in this),
        3 => Power(2).Multiply(in this),
        4 => Power(2).Power(2),
        -1 => MultiplicativeInverse,
        < 0 => Power(-e).MultiplicativeInverse,
        _ => Power(new Scalar((__scalar)e)),
    };

    public readonly Scalar Power(Scalar e) => new(__scalar.Pow(Determinant, e.Determinant));

    public readonly Scalar Factorial()
    {
        if (IsZero)
            return One;
        else if (IsInteger && IsPositive)
        {
            double result = 1;
            ulong num = (ulong)this;

            while (num > 1)
            {
                result *= num;
                --num;
            }

            return result;
        }
        else
            return ScalarFunction.Gamma[this];
    }

    public readonly Scalar Sqrt() => new(__scalar.Sqrt(Determinant));

    public readonly Scalar Cbrt() => new(__scalar.Cbrt(Determinant));

    public readonly Scalar ScaleB(int n) => new(__scalar.ScaleB(Determinant, n));

    public readonly Scalar RootN(int n) => new(__scalar.RootN(Determinant, n));

    public readonly Scalar Exp() => new(__scalar.Exp(Determinant));

    public readonly Scalar Exp2() => new(__scalar.Exp2(Determinant));

    public readonly Scalar Exp10() => new(__scalar.Exp10(Determinant));

    public readonly int ILogB() => __scalar.ILogB(Determinant);

    public readonly Scalar Log() => new(__scalar.Log(Determinant));

    public readonly Scalar Log2() => new(__scalar.Log2(Determinant));

    public readonly Scalar Log10() => new(__scalar.Log10(Determinant));

    public readonly Scalar Sinc() => Sin(Multiply(Pi)).Divide(Tau);

    public readonly Scalar Sin() => new(__scalar.Sin(Determinant));

    public readonly Scalar Cos() => new(__scalar.Cos(Determinant));

    public readonly Scalar Tan() => new(__scalar.Tan(Determinant));

    public readonly Scalar Cot() => Tan().MultiplicativeInverse;

    public readonly Scalar Sec() => Cos().MultiplicativeInverse;

    public readonly Scalar Csc() => Sin().MultiplicativeInverse;

    public readonly Complex Cis() => Complex.Cis(this);

    public readonly Scalar Asin() => new(__scalar.Asin(Determinant));

    public readonly Scalar Acos() => new(__scalar.Acos(Determinant));

    public readonly Scalar Atan() => new(__scalar.Atan(Determinant));

    public readonly Scalar Acot() => MultiplicativeInverse.Atan();

    public readonly Scalar Asec() => MultiplicativeInverse.Acos();

    public readonly Scalar Acsc() => MultiplicativeInverse.Asin();

    public readonly Scalar Sinh() => new(__scalar.Sinh(Determinant));

    public readonly Scalar Cosh() => new(__scalar.Cosh(Determinant));

    public readonly Scalar Tanh() => new(__scalar.Tanh(Determinant));

    public readonly Scalar Coth() => Tanh().MultiplicativeInverse;

    public readonly Scalar Sech() => Cosh().MultiplicativeInverse;

    public readonly Scalar Csch() => Sinh().MultiplicativeInverse;

    public readonly Scalar Asinh() => new(__scalar.Asinh(Determinant));

    public readonly Scalar Acosh() => new(__scalar.Acosh(Determinant));

    public readonly Scalar Atanh() => new(__scalar.Atanh(Determinant));

    public readonly Scalar Acoth() => MultiplicativeInverse.Atanh();

    public readonly Scalar Asech() => MultiplicativeInverse.Acosh();

    public readonly Scalar Acsch() => MultiplicativeInverse.Asinh();

    public readonly Scalar AcosPi() => new(__scalar.AcosPi(Determinant));

    public readonly Scalar AsinPi() => new(__scalar.AsinPi(Determinant));

    public readonly Scalar AtanPi() => new(__scalar.AtanPi(Determinant));

    public readonly Scalar CosPi() => new(__scalar.CosPi(Determinant));

    public readonly Scalar SinPi() => new(__scalar.SinPi(Determinant));

    public readonly Scalar TanPi() => new(__scalar.TanPi(Determinant));

    public readonly Scalar BitDecrement() => new(__scalar.BitDecrement(Determinant));

    public readonly Scalar BitIncrement() => new(__scalar.BitIncrement(Determinant));

    public readonly (Scalar Sin, Scalar Cos) SinCos() => (Sin(), Cos());

    public readonly (Scalar SinPi, Scalar CosPi) SinCosPi() => (SinPi(), CosPi());

    public readonly Scalar Round() => Round(0);

    public readonly Scalar Round(int digits) => Round(digits, default);

    public readonly Scalar Round(MidpointRounding mode) => Round(0, mode);

    public readonly Scalar Round(int digits, MidpointRounding mode) => new(__scalar.Round(Determinant, digits, mode));

    public readonly Scalar Clamp() => Clamp(Zero, One);

    public readonly Scalar Clamp(Scalar low, Scalar high) => __scalar.Clamp(Determinant, low, high);

    public readonly Scalar Min(Scalar second) => CompareTo(second) <= 0 ? this : second;

    public readonly Scalar Max(Scalar second) => CompareTo(second) >= 0 ? this : second;

    public readonly bool IsMultipleOf(Scalar other) => Modulus(other).IsZero;

    public readonly bool IsBetween(Scalar min_inclusive, Scalar max_inclusive) => this >= min_inclusive && this <= max_inclusive;

    public readonly bool Is(Scalar other, Scalar tolerance)
    {
        if ((IsPositiveInfinity && other.IsPositiveInfinity) ||
            (IsNegativeInfinity && other.IsNegativeInfinity) ||
            (IsNaN && other.IsNaN))
            return true;

        // return Subtract(other).Abs().CompareTo(tolerance) <= 0;

        Scalar min = Abs().Min(other.Abs());
        Scalar max = Abs().Max(other.Abs());

        return (max - min) <= tolerance * max;
    }

    public readonly bool Is([MaybeNull] Scalar other) => Is(other, ComputationalEpsilon);

    public readonly bool IsNot([MaybeNull] Scalar other) => !Is(other);

    public readonly bool Equals(Scalar other) => Is(other);

    public readonly override bool Equals(object? other)
    {
        try
        {
            return Equals((Scalar)other!);
        }
        catch
        {
            return false;
        }
    }

    public readonly override int GetHashCode() => Determinant.GetHashCode();

    public readonly override string ToString() => Determinant.ToString(); // TODO : check if pi or e etc.

    /// <inheritdoc cref="__scalar.ToString(string)"/>
    public readonly string ToString(string format) => Determinant.ToString(format);

    /// <inheritdoc cref="__scalar.ToString(IFormatProvider)"/>
    public readonly string ToString(IFormatProvider prov) => Determinant.ToString(prov);

    /// <inheritdoc cref="__scalar.ToString(string,IFormatProvider)"/>
    public readonly string ToString(string? format, IFormatProvider? prov) => Determinant.ToString(format, prov);

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => Determinant.TryFormat(destination, out charsWritten, format, provider);

    public readonly string ToHexString(int digits = 18) => ToString(16, digits);

    public readonly string ToString(int @base, int digits = 18)
    {
        if (IsInteger)
            if (@base == 16)
                return $"{(ulong)this:X}";

        string chars = "0123456789ABCDEF"[..@base];
        StringBuilder acc = new();
        Scalar place = 1;
        Scalar scalar = this;
        bool dot = false;

        if (!IsFinite)
            return ToString();
        else if (IsNegative)
        {
            acc.Append('-');
            scalar = Abs();
        }

        while (scalar >= @base)
        {
            scalar /= @base;
            place *= @base;
        }

        for (int i = 0; i < digits || place > 1; ++i)
        {
            if (scalar.IsZero && (dot || place < 1))
                break;
            else if (place < 1 && !dot)
            {
                acc.Append('.');
                dot = true;
            }

            acc.Append(chars[(int)scalar]);

            scalar = scalar.DecimalPlaces * @base;
            place /= @base;
        }

        return acc.ToString();
    }

    public readonly string ToShortString() => ToShortString(null);

    public readonly string ToShortString(string? format)
    {
        if (IsInfinity)
            return "∞";
        else if (IsNegativeInfinity)
            return "-∞";
        else if (IsNaN)
            return "NaN";

        Scalar abs = Abs();
        string s;

        if (abs.Is(Pi))
            s = "π";
        else if (abs.Is(e))
            s = "e";
        else if (abs.Is(Tau))
            s = "τ";
        else if (abs.Divide(Pi) is { IsInteger: true, IsZero: false } f)
            s = $"{f}π";
        else if (abs.Is(GoldenRatio))
            s = "φ";
        else if (abs.IsPositive && abs < 2 * ComputationalEpsilon)
            s = "ε";
        else
            return format is { } ? ToString(format) : ToString();

        return IsNegative ? $"-{s}" : s;
    }

    public readonly int CompareTo(Scalar other) => Determinant.CompareTo(other.Determinant);

    public readonly int CompareTo(object? obj) => CompareTo((Scalar)obj!);

    public readonly object Clone() => new Scalar(this);

    public readonly Fraction ToFraction() => Fraction.FromScalar(this);

    public readonly Fraction ToFraction(Scalar accuracy) => Fraction.FromScalar(this, accuracy);

    public readonly Scalar[] ToArray() => [this];

    public readonly Polynomial ToPolynomial() => new(this);

    public readonly (Scalar P, Scalar Q)? DecomposePQ(Scalar phi)
    {
        if (new Polynomial(this, phi - this - 1, 1).Roots.ToArray() is { Length: 2 } arr)
            return (arr[0], arr[1]);

        return null;
    }

    public readonly (Scalar U, Scalar D) IwasawaDecompose() => (this, One);

    #endregion
    #region EXPLICIT INSTANCE METHODS

    readonly Scalar Algebra<Scalar>.IVector<Scalar, Scalar>.OuterProduct(in Scalar second) => Multiply(in second);

    readonly Scalar Algebra<Scalar>.IEucledianVectorSpace<Scalar>.AngleTo(in Scalar second) => throw new InvalidOperationException("This operation is undefined for scalar data types.");

    readonly Scalar Algebra<Scalar>.IComposite<Scalar>.ComponentwiseDivide(in Scalar second) => Multiply(in second);

    readonly Scalar Algebra<Scalar>.IComposite<Scalar>.ComponentwiseMultiply(in Scalar second) => Divide(in second);

    readonly Scalar Algebra<Scalar>.IMetricVectorSpace<Scalar>.DistanceTo(in Scalar second) => Subtract(in second).Abs();

    readonly Algebra<Scalar>.IMatrix[] Algebra<Scalar>.IMatrix.GetPrincipalSubmatrices() => [];

    readonly bool Algebra<Scalar>.IVectorSpace<Scalar>.IsLinearDependant(in Scalar other, out Scalar? factor) => (factor = IsZero || other.IsZero ? (Scalar?)null : other / this) != null;

    readonly bool Algebra<Scalar>.IEucledianVectorSpace<Scalar>.IsOrthogonal(in Scalar second) => throw new InvalidOperationException("The 'IsOrthogonal' method is undefined for scalar values.");

    readonly Scalar Algebra<Scalar>.IVectorSpace<Scalar>.LinearInterpolate(in Scalar other, Scalar factor) => new((1 - factor) * Determinant + factor * other.Determinant);

    readonly Scalar Algebra<Scalar>.IEucledianVectorSpace<Scalar>.Reflect(in Scalar normal) => throw new InvalidOperationException("The 'reflect' method is undefined for scalar values.");

    readonly bool Algebra<Scalar>.IEucledianVectorSpace<Scalar>.Refract(in Scalar normal, Scalar eta, out Scalar refracted) => throw new InvalidOperationException("The 'refract' method is undefined for scalar values.");

    readonly bool Algebra<Scalar>.IMatrix<Scalar, Scalar>.Solve(Scalar v, out Scalar solution)
    {
        solution = NaN;

        if (IsZero)
            return false;
        else
        {
            solution = v / this;

            return true;
        }
    }

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.GetRow(int row) => this[0, row];

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.SetRow(int row, in Scalar vector) => this[0, row, vector];

    readonly MatrixNM Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.GetRows(Range rows) => (this as Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>).GetRegion(0..1, rows);

    readonly Scalar Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.SetRows(Range rows, in MatrixNM values) => (this as Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>).SetRegion(0..1, rows, values);

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.GetColumn(int column) => this[column];

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.SetColumn(int column, in Scalar vector) => this[column, vector];

    readonly MatrixNM Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.GetColumns(Range columns) => (this as Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>).GetRegion(columns, 0..1);

    readonly Scalar Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.SetColumns(Range columns, in MatrixNM values) => (this as Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>).SetRegion(columns, 0..1, values);

    readonly MatrixNM Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.GetRegion(Range columns, Range rows)
    {
        if (columns.GetOffsetAndLength(1) is (0, 1))
            if (rows.GetOffsetAndLength(1) is (0, 1))
                return MatrixNM.FromCoefficients(new Scalar[,] { { this } });
            else
                throw new ArgumentException("The row indices must only contain the value [0].", nameof(rows));
        else
            throw new ArgumentException("The column indices must only contain the value [0].", nameof(rows));
    }

    readonly Scalar Algebra<Scalar, Polynomial>.IMatrix<Scalar, MatrixNM>.SetRegion(Range columns, Range rows, in MatrixNM values)
    {
        if (columns.GetOffsetAndLength(1) is (0, 1))
            if (rows.GetOffsetAndLength(1) is (0, 1))
                return values.Size == (1, 1) ? values[0, 0] : throw new ArgumentException("The new values must be a matrix with the dimensions (1, 1).", nameof(values));
            else
                throw new ArgumentException("The row indices must only contain the value [0].", nameof(rows));
        else
            throw new ArgumentException("The column indices must only contain the value [0].", nameof(rows));
    }

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.MultiplyRow(int row, Scalar factor) => row == 0 ? Multiply(in factor) : throw new ArgumentException("The row index must be zero.", nameof(row));

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.SwapRows(int src_row, int dst_row) => (src_row, dst_row) == (0, 0) ? this : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.AddRows(int src_row, int dst_row) => (this as Algebra<Scalar>.IMatrix<Scalar, Scalar>).AddRows(src_row, dst_row, One);

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.AddRows(int src_row, int dst_row, Scalar factor) =>
        (src_row, dst_row) == (0, 0) ? Determinant * (1 + factor) : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.MultiplyColumn(int col, Scalar factor) => col == 0 ? Multiply(in factor) : throw new ArgumentException("The column index must be zero.", nameof(col));

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.SwapColumns(int src_col, int dst_col) => (src_col, dst_col) == (0, 0) ? this : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.AddColumns(int src_col, int dst_col) => (this as Algebra<Scalar>.IMatrix<Scalar, Scalar>).AddColumns(src_col, dst_col, One);

    readonly Scalar Algebra<Scalar>.IMatrix<Scalar, Scalar>.AddColumns(int src_col, int dst_col, Scalar factor) =>
        (src_col, dst_col) == (0, 0) ? Determinant * (1 + factor) : throw new ArgumentException("The source and destination columns must have the index zero.");

    readonly int num.IFloatingPoint<Scalar>.GetExponentByteCount() => ((num.IFloatingPoint<__scalar>)Determinant).GetExponentByteCount();

    readonly int num.IFloatingPoint<Scalar>.GetExponentShortestBitLength() => ((num.IFloatingPoint<__scalar>)Determinant).GetExponentShortestBitLength();

    readonly int num.IFloatingPoint<Scalar>.GetSignificandBitLength() => ((num.IFloatingPoint<__scalar>)Determinant).GetSignificandBitLength();

    readonly int num.IFloatingPoint<Scalar>.GetSignificandByteCount() => ((num.IFloatingPoint<__scalar>)Determinant).GetSignificandByteCount();

    readonly bool num.IFloatingPoint<Scalar>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<__scalar>)Determinant).TryWriteExponentBigEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<__scalar>)Determinant).TryWriteExponentLittleEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<__scalar>)Determinant).TryWriteSignificandBigEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<__scalar>)Determinant).TryWriteSignificandLittleEndian(destination, out bytesWritten);

    #endregion
    #region STATIC METHODS

    public static bool Is(Scalar s1, Scalar s2, Scalar? error = null) => s1.Is(s2, error ?? ComputationalEpsilon);

    public static Scalar LogSumExp(params Scalar[] values) => LogSumExp(values as IEnumerable<Scalar>);

    public static Scalar LogSumExp(IEnumerable<Scalar> values) => values.Select(Exp).Sum().Log();

    public static Scalar Random() => Numerics.Random.XorShift.NextScalar();

    public static Scalar Factorial(Scalar s) => s.Factorial();

    public static Scalar Negate(Scalar s) => s.Negate();

    public static Scalar Abs(Scalar s) => s.Abs();

    public static Scalar Sqrt(Scalar s) => s.Sqrt();

    public static Scalar Cbrt(Scalar s) => s.Cbrt();

    public static Scalar RootN(Scalar s, int n) => s.RootN(n);

    public static Scalar Exp(Scalar s) => s.Exp();

    public static int ILogB(Scalar x) => x.ILogB();

    public static Scalar ScaleB(Scalar x, int n) => x.ScaleB(n);

    public static Scalar Exp10(Scalar x) => x.Exp10();

    public static Scalar Exp2(Scalar x) => x.Exp2();

    public static Scalar Log10(Scalar x) => x.Log10();

    public static Scalar Log2(Scalar x) => x.Log2();

    public static Scalar Hypot(Scalar x, Scalar y) => Sqrt(x * x + y * y);

    public static Scalar Power(Scalar x, Scalar y) => x.Power(y);

    public static Scalar Log(Scalar s) => s.Log();

    public static Scalar Log(Scalar s, Scalar basis) => new(__scalar.Log(s.Determinant, basis.Determinant));

    public static Scalar Sinc(Scalar s) => s.Sinc();

    public static Scalar Sin(Scalar s) => s.Sin();

    public static Scalar Cos(Scalar s) => s.Cos();

    public static Scalar Tan(Scalar s) => s.Tan();

    public static Complex Cis(Scalar s) => s.Cis();

    public static Scalar Cot(Scalar s) => s.Cot();

    public static Scalar Sec(Scalar s) => s.Sec();

    public static Scalar Csc(Scalar s) => s.Csc();

    public static Scalar Asin(Scalar s) => s.Asin();

    public static Scalar Acos(Scalar s) => s.Acos();

    public static Scalar Atan(Scalar s) => s.Atan();

    public static Scalar Atan2(Scalar y, Scalar x) => __scalar.Atan2(y, x);

    public static Scalar Acot(Scalar s) => s.Acot();

    public static Scalar Acsc(Scalar s) => s.Acsc();

    public static Scalar Asec(Scalar s) => s.Asec();

    public static Scalar Sinh(Scalar s) => s.Sinh();

    public static Scalar Cosh(Scalar s) => s.Cosh();

    public static Scalar Tanh(Scalar s) => s.Tanh();

    public static Scalar Coth(Scalar s) => s.Coth();

    public static Scalar Sech(Scalar s) => s.Sech();

    public static Scalar Csch(Scalar s) => s.Csch();

    public static Scalar Asinh(Scalar s) => s.Asinh();

    public static Scalar Acosh(Scalar s) => s.Acosh();

    public static Scalar Atanh(Scalar s) => s.Atanh();

    public static Scalar Acoth(Scalar s) => s.Acoth();

    public static Scalar Acsch(Scalar s) => s.Acsch();

    public static Scalar Asech(Scalar s) => s.Asech();

    public static Scalar Atan2Pi(Scalar y, Scalar x) => __scalar.Atan2Pi(y.Determinant, x.Determinant);

    public static Scalar AcosPi(Scalar x) => x.AcosPi();

    public static Scalar AsinPi(Scalar x) => x.AsinPi();

    public static Scalar AtanPi(Scalar x) => x.AtanPi();

    public static Scalar CosPi(Scalar x) => x.CosPi();

    public static (Scalar Sin, Scalar Cos) SinCos(Scalar x) => x.SinCos();

    public static (Scalar SinPi, Scalar CosPi) SinCosPi(Scalar x) => x.SinCosPi();

    public static Scalar SinPi(Scalar x) => x.SinPi();

    public static Scalar TanPi(Scalar x) => x.TanPi();

    public static Scalar Round(Scalar x) => x.Round();

    public static Scalar Round(Scalar x, int digits) => x.Round(digits);

    public static Scalar Round(Scalar x, MidpointRounding mode) => x.Round(mode);

    public static Scalar Round(Scalar x, int digits, MidpointRounding mode) => x.Round(digits, mode);

    public static Scalar Max(Scalar x, Scalar y) => x.Max(y);

    public static Scalar Min(Scalar x, Scalar y) => x.Min(y);

    public static Scalar MaxMagnitude(Scalar x, Scalar y) => new(__scalar.MaxMagnitude(x.Determinant, y.Determinant));

    public static Scalar MaxMagnitudeNumber(Scalar x, Scalar y) => new(__scalar.MaxMagnitudeNumber(x.Determinant, y.Determinant));

    public static Scalar MinMagnitude(Scalar x, Scalar y) => new(__scalar.MinMagnitude(x.Determinant, y.Determinant));

    public static Scalar MinMagnitudeNumber(Scalar x, Scalar y) => new(__scalar.MinMagnitudeNumber(x.Determinant, y.Determinant));

    public static Scalar BitDecrement(Scalar x) => x.BitDecrement();

    public static Scalar BitIncrement(Scalar x) => x.BitIncrement();

    public static Scalar FusedMultiplyAdd(Scalar left, Scalar right, Scalar addend) => __scalar.FusedMultiplyAdd(left.Determinant, right.Determinant, addend.Determinant);

    public static Scalar Ieee754Remainder(Scalar left, Scalar right) => __scalar.Ieee754Remainder(left.Determinant, right.Determinant);

    public static Scalar Add(Scalar s1, Scalar s2) => s1.Add(s2);

    public static Scalar Subtract(Scalar s1, Scalar s2) => s1.Subtract(s2);

    public static Scalar Multiply(Scalar s1, Scalar s2) => s1.Multiply(s2);

    public static Scalar Divide(Scalar s1, Scalar s2) => s1.Divide(s2);

    public static Scalar Modulus(Scalar s1, Scalar s2) => s1.Modulus(s2);

    public static bool TryParse(string? str, NumberStyles style, IFormatProvider? provider, out Scalar scalar)
    {
        str = str?.Remove("_")?.ToLowerInvariant()?.Trim() ?? "0";
        scalar = Zero;

        bool success = __scalar.TryParse(str, style, provider, out __scalar result);

        if (success)
        {
            scalar = new(result);

            return true;
        }
        else if (str.Match(REGEX_BIN, out ReadOnlyIndexer<string, string>? groups))
            scalar = Convert.ToUInt64(groups["num"].Remove("0b").Remove("b"), 2);
        else if (str.Match(REGEX_OCT, out groups))
            scalar = Convert.ToUInt64(groups["num"].Remove("0o").Remove("o"), 8);
        else if (str.Match(REGEX_HEX, out groups))
            scalar = Convert.ToUInt64(groups["num"].Remove("0x").Remove("x"), 16);
        else if (str.Match(REGEX_DEC1, out groups) || str.Match(REGEX_DEC2, out groups))
        {
            string @const = groups["const"];
            string factor = groups["factor"];
            Scalar f = One;

            if (!string.IsNullOrEmpty(factor))
                f = (Scalar)decimal.Parse(factor, NumberStyles.Any);

            if (string.IsNullOrEmpty(@const))
                scalar = string.IsNullOrEmpty(factor) ? Zero : f;
            else
                scalar = f * (@const switch
                {
                    "" => One,
                    "e" => e,
                    "pi" or "π" => Pi,
                    "tau" or "τ" => Tau,
                    "phi" or "φ" => GoldenRatio,
                });
        }
        else
            success = false;

        if (str.StartsWith("-"))
            scalar = scalar.Negate();

        return success;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Scalar result) => TryParse(s, NumberStyles.Float, provider, out result);

    public static bool TryParse(string? s, out Scalar result) => TryParse(s, null, out result);

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Scalar result) => TryParse(new string(s), style, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Scalar result) => TryParse(s, NumberStyles.Float, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, out Scalar result) => TryParse(s, null, out result);

    public static Scalar Parse(string s, NumberStyles style, IFormatProvider? provider) => TryParse(s, style, provider, out Scalar value) ? value : throw new FormatException();

    public static Scalar Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => TryParse(s, style, provider, out Scalar value) ? value : throw new FormatException();

    public static Scalar Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => TryParse(s, provider, out Scalar value) ? value : throw new FormatException();

    public static Scalar Parse(ReadOnlySpan<char> s) => TryParse(s, out Scalar value) ? value : throw new FormatException();

    public static Scalar Parse(string s, IFormatProvider? provider) => TryParse(s, provider, out Scalar value) ? value : throw new FormatException();

    public static Scalar Parse(string s) => TryParse(s, out Scalar value) ? value : throw new FormatException();

    public static Scalar FromArray(params Scalar[] array) => array[0];

    static Scalar num.IPowerFunctions<Scalar>.Pow(Scalar x, Scalar y) => Power(x, y);

    static bool num.INumberBase<Scalar>.IsCanonical(Scalar value) => true;

    static bool num.INumberBase<Scalar>.IsComplexNumber(Scalar value) => false;

    static bool num.INumberBase<Scalar>.IsEvenInteger(Scalar value) => __scalar.IsEvenInteger(value.Determinant);

    static bool num.INumberBase<Scalar>.IsFinite(Scalar value) => value.IsFinite;

    static bool num.INumberBase<Scalar>.IsImaginaryNumber(Scalar value) => false;

    static bool num.INumberBase<Scalar>.IsInfinity(Scalar value) => value.IsInfinity;

    static bool num.INumberBase<Scalar>.IsInteger(Scalar value) => value.IsInteger;

    static bool num.INumberBase<Scalar>.IsNaN(Scalar value) => value.IsNaN;

    static bool num.INumberBase<Scalar>.IsNegative(Scalar value) => value.IsNegative;

    static bool num.INumberBase<Scalar>.IsNegativeInfinity(Scalar value) => value.IsNegativeInfinity;

    static bool num.INumberBase<Scalar>.IsNormal(Scalar value) => __scalar.IsNormal(value.Determinant);

    static bool num.INumberBase<Scalar>.IsOddInteger(Scalar value) => __scalar.IsOddInteger(value.Determinant);

    static bool num.INumberBase<Scalar>.IsPositive(Scalar value) => value.IsPositive;

    static bool num.INumberBase<Scalar>.IsPositiveInfinity(Scalar value) => value.IsPositiveInfinity;

    static bool num.INumberBase<Scalar>.IsRealNumber(Scalar value) => __scalar.IsRealNumber(value.Determinant);

    static bool num.INumberBase<Scalar>.IsSubnormal(Scalar value) => __scalar.IsSubnormal(value.Determinant);

    static bool num.INumberBase<Scalar>.IsZero(Scalar value) => value.IsZero;





    static bool num.INumberBase<Scalar>.TryConvertFromChecked<TOther>(TOther value, out Scalar result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar>.TryConvertFromSaturating<TOther>(TOther value, out Scalar result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar>.TryConvertFromTruncating<TOther>(TOther value, out Scalar result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar>.TryConvertToChecked<TOther>(Scalar value, out TOther result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar>.TryConvertToSaturating<TOther>(Scalar value, out TOther result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar>.TryConvertToTruncating<TOther>(Scalar value, out TOther result) => throw new NotImplementedException();


    #endregion
    #region OPERATORS

    public static bool operator true(Scalar scalar) => scalar.IsNonZero;

    public static bool operator false(Scalar scalar) => scalar.IsZero;

    /// <summary>
    /// Compares whether the two given scalars are equal regarding their coefficients.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator ==(Scalar m1, Scalar m2) => m1.Is(m2);

    /// <summary>
    /// Compares whether the two given scalars are unequal regarding their coefficients.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator !=(Scalar m1, Scalar m2) => m1.IsNot(m2);

    /// <summary>
    /// Compares whether the first given scalar is smaller than the second one.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator <(Scalar m1, Scalar m2) => m1.CompareTo(m2) < 0;

    /// <summary>
    /// Compares whether the first given scalar is smaller than or equal to the second one.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator <=(Scalar m1, Scalar m2) => m1.CompareTo(m2) <= 0;

    /// <summary>
    /// Compares whether the first given scalar is greater than the second one.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator >(Scalar m1, Scalar m2) => m1.CompareTo(m2) > 0;

    /// <summary>
    /// Compares whether the first given scalar is greater than or equal to the second one.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Comparison result</returns>
    public static bool operator >=(Scalar m1, Scalar m2) => m1.CompareTo(m2) >= 0;

    public static Scalar operator ++(Scalar s) => s.Increment();

    public static Scalar operator ++(in Scalar s) => s.Increment();

    public static Scalar operator --(Scalar s) => s.Decrement();

    public static Scalar operator --(in Scalar s) => s.Decrement();

    /// <summary>
    /// Identity function (returns the given scalar unchanged)
    /// </summary>
    /// <param name="m">Original scalar</param>
    /// <returns>Unchanged scalar</returns>
    public static Scalar operator +(Scalar m) => m;

    /// <summary>
    /// Identity function (returns the given scalar unchanged)
    /// </summary>
    /// <param name="m">Original scalar</param>
    /// <returns>Unchanged scalar</returns>
    public static Scalar operator +(in Scalar m) => m;

    /// <summary>
    /// Negates the given scalar
    /// </summary>
    /// <param name="m">Original scalar</param>
    /// <returns>Negated scalar</returns>
    public static Scalar operator -(Scalar m) => m.Negate();

    /// <summary>
    /// Negates the given scalar
    /// </summary>
    /// <param name="m">Original scalar</param>
    /// <returns>Negated scalar</returns>
    public static Scalar operator -(in Scalar m) => m.Negate();

    /// <summary>
    /// Performs the subtraction of two scalars by subtracting their respective coefficients.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Subtraction result</returns>
    public static Scalar operator -(Scalar m1, Scalar m2) => m1.Subtract(m2);

    /// <summary>
    /// Performs the subtraction of two scalars by subtracting their respective coefficients.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Subtraction result</returns>
    public static Scalar operator -(in Scalar m1, in Scalar m2) => m1.Subtract(in m2);

    public static Scalar operator +(Scalar m1, Scalar m2) => m1.Add(m2);

    /// <summary>
    /// Performs the addition of two scalars by adding their respective coefficients.
    /// </summary>
    /// <param name="m1">First scalar</param>
    /// <param name="m2">Second scalar</param>
    /// <returns>Addition result</returns>
    public static Scalar operator +(in Scalar m1, in Scalar m2) => m1.Add(in m2);

    public static Scalar operator *(Scalar m, Scalar v) => m.Multiply(v);

    public static Scalar operator *(in Scalar m, in Scalar v) => m.Multiply(in v);

    public static Scalar operator ^(in Scalar b, Scalar e) => b.Power(e);

    public static Scalar operator ^(in Scalar b, int e) => b.Power(e);

    public static Scalar operator /(Scalar m, Scalar f) => m.Divide(f);

    public static Scalar operator /(in Scalar m, in Scalar f) => m.Divide(in f);

    public static Scalar operator %(Scalar m, Scalar f) => m.Divide(f);

    public static Scalar operator %(in Scalar m, in Scalar f) => m.Modulus(in f);

    static Scalar Algebra<Scalar>.IVectorSpace<Scalar>.operator *(in Scalar m, Scalar v) => m.Multiply(v);

    static Scalar Algebra<Scalar>.IVectorSpace<Scalar>.operator *(Scalar m, in Scalar v) => m.Multiply(in v);

    static Scalar Algebra<Scalar>.IVectorSpace<Scalar>.operator %(in Scalar m, Scalar v) => m.Modulus(v);

    static Scalar Algebra<Scalar>.IVectorSpace<Scalar>.operator /(in Scalar m, Scalar v) => m.Divide(v);

    public static explicit operator decimal(Scalar m) => (decimal)m.Determinant;

    public static explicit operator Scalar(MatrixNM m) => m[0, 0];

    public static explicit operator Scalar(VectorN m) => m[0];

    public static explicit operator Scalar(string str) => TryParse(str, out Scalar scalar) ? scalar : Zero;

    public static implicit operator bint(Scalar m) => new(m.Determinant);

    public static implicit operator Scalar(byte t) => new((__scalar)t);

    public static implicit operator Scalar(sbyte t) => new((__scalar)t);

    public static implicit operator Scalar(short t) => new((__scalar)t);

    public static implicit operator Scalar(ushort t) => new((__scalar)t);

    public static implicit operator Scalar(int t) => new((__scalar)t);

    public static implicit operator Scalar(uint t) => new((__scalar)t);

    public static implicit operator Scalar(nint t) => new((__scalar)t);

    public static implicit operator Scalar(nuint t) => new((__scalar)t);

    public static implicit operator Scalar(long t) => new((__scalar)t);

    public static implicit operator Scalar(ulong t) => new((__scalar)t);

    public static implicit operator Scalar(float t) => new((__scalar)t);

    public static implicit operator Scalar(double t) => new((__scalar)t);

    public static explicit operator Scalar(decimal t) => new((__scalar)t);

    public static implicit operator double(Scalar m) => (double)m.Determinant;

    public static implicit operator float(Scalar m) => (float)m.Determinant;

    public static explicit operator ulong(Scalar m) => (ulong)m.Determinant;

    public static explicit operator long(Scalar m) => (long)m.Determinant;

    public static explicit operator nuint(Scalar m) => (nuint)m.Determinant;

    public static explicit operator nint(Scalar m) => (nint)m.Determinant;

    public static explicit operator uint(Scalar m) => (uint)m.Determinant;

    public static explicit operator int(Scalar m) => (int)m.Determinant;

    public static explicit operator ushort(Scalar m) => (ushort)m.Determinant;

    public static explicit operator short(Scalar m) => (short)m.Determinant;

    public static explicit operator byte(Scalar m) => (byte)m.Determinant;

    public static explicit operator sbyte(Scalar m) => (sbyte)m.Determinant;

    public static implicit operator Scalar(Scalar<Half> t) => new((__scalar)t.Value);

    public static implicit operator Scalar(Scalar<float> t) => new((__scalar)t.Value);

    public static implicit operator Scalar(Scalar<double> t) => new((__scalar)t.Value);

    public static implicit operator Scalar<Half>(Scalar m) => new((Half)m.Determinant);

    public static implicit operator Scalar<float>(Scalar m) => new((float)m.Determinant);

    public static implicit operator Scalar<double>(Scalar m) => new((double)m.Determinant);

    #endregion
}

[StructLayout(LayoutKind.Sequential), NativeCppClass, Serializable, CLSCompliant(false)]
public unsafe readonly partial struct Scalar<T>
    : IScalar<Scalar<T>>
    , Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>
    , Algebra<Scalar<T>>.IVector<Scalar<T>, Scalar<T>>
    , Algebra<Scalar<T>, Polynomial<T>>.IComposite1D
    , Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>
    , num.IFloatingPointIeee754<Scalar<T>>
    , IComparable<Scalar<T>>
    , IEquatable<Scalar<T>>
    , IComparable
    , ICloneable
    where T : unmanaged, num.IFloatingPointIeee754<T>
{
    #region STATIC FIELDS

    private static readonly Dictionary<OpType, MethodInfo?> _operators = [];
    private static readonly T _zero;
    private static readonly T _one;

    #endregion
    #region STATIC PROPERTIES

#pragma warning disable IDE1006
    public static Scalar<T> π { get; } = new(T.Pi);

    public static Scalar<T> τ { get; } = new(T.Tau);

    public static Scalar<T> e { get; } = new(T.E);
#pragma warning restore IDE1006
    static Scalar<T> num.IFloatingPointConstants<Scalar<T>>.E => e;

    public static Scalar<T> Pi => π;

    public static Scalar<T> Tau => τ;

    public static Scalar<T> PiHalf { get; } = π.Divide(Two);

    public static Scalar<T> Zero { get; }

    public static Scalar<T> NegativeZero { get; } = new(T.NegativeZero);

    public static Scalar<T> One { get; } = new(T.One);

    public static Scalar<T> NegativeOne { get; } = new(T.NegativeOne);

    public static Scalar<T> Two { get; } = One.Add(One);

    public static Scalar<T> PositiveInfinity { get; } = new(T.PositiveInfinity);

    public static Scalar<T> NegativeInfinity { get; } = new(T.NegativeInfinity);

    public static Scalar<T> NaN { get; } = new(T.NaN);

    public static Scalar<T> IEEE754Epsilon { get; } = new(T.Epsilon);

    static Scalar<T> num.IFloatingPointIeee754<Scalar<T>>.Epsilon => IEEE754Epsilon;

    static int num.INumberBase<Scalar<T>>.Radix => T.Radix;

    static Scalar<T> num.IAdditiveIdentity<Scalar<T>, Scalar<T>>.AdditiveIdentity { get; } = new(T.AdditiveIdentity);

    static Scalar<T> num.IMultiplicativeIdentity<Scalar<T>, Scalar<T>>.MultiplicativeIdentity { get; } = new(T.MultiplicativeIdentity);

    public static Scalar<T> MinValue { get; } = new(T.BitIncrement(T.NegativeInfinity));

    public static Scalar<T> MaxValue { get; } = new(T.BitDecrement(T.PositiveInfinity));

    public static ScalarEqualityComparer<T> EqualityComparer { get; } = new ScalarEqualityComparer<T>();

    #endregion
    #region INDEXERS

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite1D.this[int index] =>
        index is 0 ? this : throw new ArgumentOutOfRangeException(nameof(index), "The index must be zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite1D<Scalar<T>>.this[int index, Scalar<T> value] =>
        index is 0 ? value : throw new ArgumentOutOfRangeException(nameof(index), "The index must be zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.this[int column, in Scalar<T> vector] =>
        column is 0 ? vector : throw new ArgumentOutOfRangeException(nameof(column), "The index must be zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite2D.this[int column, int row] =>
        (column, row) is (0, 0) ? this : throw new ArgumentOutOfRangeException($"({nameof(column)}, {nameof(row)})", "The indices must be (0, 0).");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.this[int column] =>
        column is 0 ? this : throw new ArgumentOutOfRangeException(nameof(column), "The index must be zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite2D<Scalar<T>>.this[int column, int row, Scalar<T> value] =>
        (column, row) is (0, 0) ? value : throw new ArgumentOutOfRangeException($"({nameof(column)}, {nameof(row)})", "The indices must be (0, 0).");

    #endregion
    #region INSTANCE PROPERTIES

    public readonly T Value { get; }

    public readonly bool IsNaN => T.IsNaN(Value) || Is(NaN);

    public readonly bool IsNegative => T.IsZero(Value) || this < Zero;

    public readonly bool IsPositive => T.IsPositive(Value) || this > Zero;

    public readonly bool IsPositiveDefinite => IsPositive && IsFinite;

    public readonly bool IsNegativeDefinite => IsNegative && IsFinite;

    public readonly bool IsNegativeInfinity => T.IsNegativeInfinity(Value) || Is(NegativeInfinity);

    public readonly bool IsPositiveInfinity => T.IsPositiveInfinity(Value) || Is(PositiveInfinity);

    public readonly bool IsInfinity => IsNegativeInfinity || IsPositiveInfinity;

    public readonly bool IsFinite => !IsInfinity && !IsNaN;

    public readonly Scalar<T> Inverse => One.Divide(this);

    [DebuggerHidden, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never), EditorBrowsable(EditorBrowsableState.Never)]
    public readonly Scalar<T> MultiplicativeInverse => Inverse;

    public readonly bool IsInvertible => IsNonZero;

    public readonly bool IsOne => Equals(Value, _one);

    public readonly Scalar<T> AdditiveInverse => Negate();

    public readonly bool IsZero => Equals(Value, _zero);

    public readonly bool IsNonZero => !IsZero;

    public readonly bool IsBinary => IsZero || IsOne;

    public readonly int BinarySize => sizeof(T);

    public readonly bool IsNormalized => IsOne;

    public readonly bool IsInsideUnitSphere => Abs().IsBetweenZeroAndOne;

    public readonly bool IsBetweenZeroAndOne => this >= Zero && this <= One;

    public readonly int Sign => CompareTo(Zero);

    public readonly Scalar<T> DecimalPlaces => Modulus(One);

    public readonly Scalar<T> Ceiling => IsInteger ? Floor : Floor.Increment();

    public readonly Scalar<T> Floor => Subtract(DecimalPlaces);

    public readonly Scalar<T> Rounded => (DecimalPlaces * Two) < One ? Floor : Ceiling;

    public readonly bool IsInteger => Is(Rounded);

    public readonly bool IsPrime => IsPositive && IsInteger && ((bint)(dynamic)Value).IsPrime();

    public readonly Scalar<T>[] PrimeFactors => IsInteger && IsPositive ? ((bint)(dynamic)Value).PrimeFactorization().Select(b => new Scalar<T>((T)(dynamic)b)).ToArray() : [];

    public readonly Scalar<T> Phi => PrimeFactors is { Length: 2 } f ? f[0].Decrement().Multiply(f[1].Decrement()) : throw new InvalidOperationException($"φ({this}) is not defined.");

    #endregion
    #region EXPLICIT INSTANCE PROPERTIES

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>>.GaussianReduced => IsZero ? Zero : One;

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>>.OrthonormalBasis => IsZero ? Zero : One;

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>>.Transposed => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix.Determinant => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix.Trace => this;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsDiagonal => true;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsUpperTriangular => true;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsLowerTriangular => true;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsSymmetric => true;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsProjection => IsOne || IsZero;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsConferenceMatrix => Is(Multiply(this));

    readonly bool Algebra<Scalar<T>>.IMatrix.IsInvolutory => Multiply(Inverse).IsOne;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsHurwitzStable => IsPositive;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsOrthogonal => IsOne;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsSkewSymmetric => IsZero;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsSignMatrix => IsZero || Abs().IsOne;

    readonly bool Algebra<Scalar<T>>.IMatrix.IsSignatureMatrix => Abs().IsOne;

    readonly Polynomial<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix.CharacteristicPolynomial => new(this, NegativeOne);

    readonly Scalar<T>[] Algebra<Scalar<T>>.IMatrix.Eigenvalues => IsZero ? [] : [this];

    readonly int Algebra<Scalar<T>>.IMatrix.Rank => IsZero ? 0 : 1;

    readonly Scalar<T>[,] Algebra<Scalar<T>>.IComposite2D.Coefficients => new[,] { { this } };

    readonly IEnumerable<Scalar<T>> Algebra<Scalar<T>>.IComposite2D.FlattenedCoefficients => [this];

    readonly (int Columns, int Rows) Algebra<Scalar<T>>.IComposite2D.Dimensions => (1, 1);

    readonly bool Algebra<Scalar<T>>.IComposite.HasNegatives => IsNegative;

    readonly bool Algebra<Scalar<T>>.IComposite.HasPositives => IsPositive;

    readonly bool Algebra<Scalar<T>>.IComposite.HasNaNs => IsNaN;

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite.CoefficientSum => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite.CoefficientAvg => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite.CoefficientMin => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite.CoefficientMax => this;

    readonly Scalar<T> Algebra<Scalar<T>>.IMetricVectorSpace<Scalar<T>>.SquaredNorm => Multiply(this);

    readonly Scalar<T> Algebra<Scalar<T>>.IMetricVectorSpace<Scalar<T>>.Normalized => One;

    readonly Scalar<T> Algebra<Scalar<T>>.IMetricVectorSpace.Length => this;

    readonly Scalar<T>[] Algebra<Scalar<T>>.IComposite1D.Coefficients => [this];

    readonly int Algebra<Scalar<T>>.IComposite1D.Dimension => 1;

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.MainDiagonal => this;

    readonly Scalar<T>[] Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.Columns => [this];

    readonly Scalar<T>[] Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.Rows => [this];

    readonly Scalar<T> Algebra<Scalar<T>>.IVector<Scalar<T>, Scalar<T>>.HouseholderMatrix => throw new NotImplementedException();

    #endregion
    #region .CTOR / .CCTOR / .DTOR

    public Scalar(T* ptr)
        : this(*ptr)
    {
    }

    public Scalar(T value) => Value = value;

    public Scalar(Scalar<T>* ptr)
        : this(*ptr)
    {
    }

    public Scalar(Scalar<T> scalar)
        : this(scalar.Value)
    {
    }

    #endregion
    #region INSTANCE METHODS

    public readonly Scalar<T> Negate() => new(-Value);

    public readonly Scalar<T> Add(in Scalar<T> second) => new(Value + second.Value);

    public readonly Scalar<T> Add(params Scalar<T>[] others)
    {
        T value = Value;

        foreach (Scalar<T> scalar in others)
            value += scalar.Value;

        return new(value);
    }

    public readonly Scalar<T> Subtract(in Scalar<T> second) => new(Value - second.Value);

    public readonly Scalar<T> Subtract(params Scalar<T>[] others)
    {
        T value = Value;

        foreach (Scalar<T> scalar in others)
            value -= scalar.Value;

        return new(value);
    }

    public readonly Scalar<T> Increment() => Add(One);

    public readonly Scalar<T> Decrement() => Subtract(One);

    public readonly Scalar<T> Dot(in Scalar<T> second) => Multiply(second);

    public readonly Scalar<T> Multiply(Scalar<T> second) => new(Value * second.Value);

    public readonly Scalar<T> Multiply(in Scalar<T> second) => new(Value * second.Value);

    public readonly Scalar<T> Multiply(params Scalar<T>[] others)
    {
        T value = Value;

        foreach (Scalar<T> scalar in others)
            value *= scalar.Value;

        return new(value);
    }

    public readonly Scalar<T> Divide(Scalar<T> second) => new(Value / second.Value);

    public readonly Scalar<T> Divide(in Scalar<T> second) => new(Value / second.Value);

    public readonly (Scalar<T> Factor, Scalar<T> Remainder) DivideModulus(in Scalar<T> second) => (Divide(in second), Modulus(in second));

    public readonly Scalar<T> Modulus(Scalar<T> second) => new(Value % second.Value);

    public readonly Scalar<T> Modulus(in Scalar<T> second) => new(Value % second.Value);

    public readonly Scalar<T> Power(Scalar<T> power) => new(T.Pow(Value, power.Value));

    public readonly Scalar<T> Power(int e)
    {
        if (e == 0)
            return One;
        else if (e < 0)
            return Power(-e).Inverse;

        Scalar<T> r = One;
        Scalar<T> p = this;

        while (e > 0)
            if ((e & 1) == 1)
            {
                --e;
                r = r.Multiply(p);
            }
            else
            {
                e /= 2;
                p = p.Multiply(p);
            }

        return r;
    }

    public readonly Scalar<T> Sqrt() => new(T.Sqrt(Value));

    public readonly Scalar<T> Cbrt() => new(T.Cbrt(Value));

    public readonly Scalar<T> ScaleB(int n) => new(T.ScaleB(Value, n));

    public readonly Scalar<T> RootN(int n) => new(T.RootN(Value, n));

    public readonly Scalar<T> Exp() => new(T.Exp(Value));

    public readonly Scalar<T> Exp2() => new(T.Exp2(Value));

    public readonly Scalar<T> Exp10() => new(T.Exp10(Value));

    public readonly int ILogB() => T.ILogB(Value);

    public readonly Scalar<T> Log() => new(T.Log(Value));

    public readonly Scalar<T> Log2() => new(T.Log2(Value));

    public readonly Scalar<T> Log10() => new(T.Log10(Value));

    public readonly Scalar<T> Sinc() => Sin(Multiply(Pi)).Divide(Tau);

    public readonly Scalar<T> AcosPi() => new(T.AcosPi(Value));

    public readonly Scalar<T> AsinPi() => new(T.AsinPi(Value));

    public readonly Scalar<T> AtanPi() => new(T.AtanPi(Value));

    public readonly Scalar<T> CosPi() => new(T.CosPi(Value));

    public readonly Scalar<T> SinPi() => new(T.SinPi(Value));

    public readonly Scalar<T> TanPi() => new(T.TanPi(Value));

    public readonly Scalar<T> BitDecrement() => new(T.BitDecrement(Value));

    public readonly Scalar<T> BitIncrement() => new(T.BitIncrement(Value));

    public readonly (Scalar<T> Sin, Scalar<T> Cos) SinCos() => (Sin(), Cos());

    public readonly (Scalar<T> SinPi, Scalar<T> CosPi) SinCosPi() => (SinPi(), CosPi());

    public readonly Scalar<T> Round() => Round(0);

    public readonly Scalar<T> Round(int digits) => Round(digits, default);

    public readonly Scalar<T> Round(MidpointRounding mode) => Round(0, mode);

    public readonly Scalar<T> Round(int digits, MidpointRounding mode) => new(T.Round(Value, digits, mode));

    public readonly Scalar<T> Clamp() => Clamp(Zero, One);

    public readonly Scalar<T> Clamp(Scalar<T> low, Scalar<T> high) => Max(low).Min(high);

    public readonly (Scalar<T> P, Scalar<T> Q)? DecomposePQ(Scalar<T> phi) =>
        new Polynomial<T>(this, phi.Subtract(Increment()), One).Roots.ToArray() is { Length: 2 } arr ? ((Scalar<T>, Scalar<T>)?)(arr[0], arr[1]) : null;

    public readonly (Scalar<T> U, Scalar<T> D) IwasawaDecompose() => (this, One);

    public readonly Scalar<T> Min(Scalar<T> second) => this <= second ? this : second;

    public readonly Scalar<T> Max(Scalar<T> second) => this >= second ? this : second;

    public readonly Scalar<T> Abs() => IsNegative ? Negate() : this;

    public readonly Scalar<T> Sin() => new(T.Sin(Value));

    public readonly Scalar<T> Cos() => new(T.Cos(Value));

    public readonly Scalar<T> Tan() => new(T.Tan(Value));

    public readonly Scalar<T> Cot() => Tan().MultiplicativeInverse;

    public readonly Scalar<T> Sec() => Cos().MultiplicativeInverse;

    public readonly Scalar<T> Csc() => Sin().MultiplicativeInverse;

    public readonly Scalar<T> Asin() => new(T.Asin(Value));

    public readonly Scalar<T> Acos() => new(T.Acos(Value));

    public readonly Scalar<T> Atan() => new(T.Atan(Value));

    public readonly Scalar<T> Acot() => MultiplicativeInverse.Atan();

    public readonly Scalar<T> Asec() => MultiplicativeInverse.Acos();

    public readonly Scalar<T> Acsc() => MultiplicativeInverse.Asec();

    public readonly Scalar<T> Sinh() => new(T.Sinh(Value));

    public readonly Scalar<T> Cosh() => new(T.Cosh(Value));

    public readonly Scalar<T> Tanh() => new(T.Tanh(Value));

    public readonly Scalar<T> Coth() => Tanh().MultiplicativeInverse;

    public readonly Scalar<T> Sech() => Cosh().MultiplicativeInverse;

    public readonly Scalar<T> Csch() => Sinh().MultiplicativeInverse;

    public readonly Scalar<T> Asinh() => new(T.Asinh(Value));

    public readonly Scalar<T> Acosh() => new(T.Acosh(Value));

    public readonly Scalar<T> Atanh() => new(T.Atanh(Value));

    public readonly Scalar<T> Acoth() => MultiplicativeInverse.Atanh();

    public readonly Scalar<T> Asech() => MultiplicativeInverse.Acosh();

    public readonly Scalar<T> Acsch() => MultiplicativeInverse.Asinh();

    public readonly bool Is(Scalar<T> o, Scalar<T> tolerance) => Subtract(o).Abs() <= tolerance;

    public readonly bool Is(Scalar<T> o) => Equals(Value, o.Value);

    public readonly bool IsNot(Scalar<T> o) => !Is(o);

    public readonly override bool Equals(object? other)
    {
        if (other is { })
            try
            {
                return Equals((Scalar<T>)other);
            }
            catch
            {
                try
                {
                    return Equals((T)other);
                }
                catch
                {
                }
            }

        return false;
    }

    public readonly bool Equals(Scalar<T> other) => Is(other);

    public readonly int CompareTo(object? other) => other is Scalar<T> s ? CompareTo(s)
                                                 : other is T v ? CompareTo(new Scalar<T>(v)) : throw new ArgumentException($"A value of the type '{other?.GetType()}' cannot be compared to an instance of '{typeof(Scalar<T>)}'.");

    public readonly int CompareTo(Scalar<T> other) => Value.CompareTo(other.Value);

    public readonly override int GetHashCode() => Value.GetHashCode();

    public readonly override string ToString() => Value.ToString()!;

    public readonly object Clone() => new Scalar<T>(this);

    public readonly Scalar<T>[] ToArray() => [this];

    public readonly Polynomial<T> ToPolynomial() => new(this);

    public readonly string ToString(string format) => ToString(format, null);

    public readonly string ToString(IFormatProvider prov) => ToString(null, prov);

    public readonly string ToString(string? format, IFormatProvider? prov) => Value.ToString(format, prov);

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => Value.TryFormat(destination, out charsWritten, format, provider);

    #endregion
    #region EXPLICIT METHODS

    readonly Scalar<T> Algebra<Scalar<T>>.IVector<Scalar<T>, Scalar<T>>.OuterProduct(in Scalar<T> second) => Multiply(in second);

    readonly Algebra<Scalar<T>>.IMatrix[] Algebra<Scalar<T>>.IMatrix.GetPrincipalSubmatrices() => [];

    readonly bool Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.IsLinearDependant(in Scalar<T> other, out Scalar<T>? factor) => (factor = IsNonZero && other.IsNonZero ? (Scalar<T>?)Divide(other) : null) != null;

    readonly Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.LinearInterpolate(in Scalar<T> other, Scalar<T> factor) => Multiply(One.Subtract(factor)).Add(other.Multiply(factor));

    readonly Scalar<T> Algebra<Scalar<T>>.IMetricVectorSpace<Scalar<T>>.DistanceTo(in Scalar<T> second) => Subtract(second).Abs();

    readonly Scalar<T> Algebra<Scalar<T>>.IEucledianVectorSpace<Scalar<T>>.AngleTo(in Scalar<T> second) => throw new InvalidOperationException("This operation is undefined for scalar data types.");

    readonly bool Algebra<Scalar<T>>.IEucledianVectorSpace<Scalar<T>>.IsOrthogonal(in Scalar<T> second) => throw new InvalidOperationException("The 'IsOrthogonal' method is undefined for scalar values.");

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite<Scalar<T>>.ComponentwiseDivide(in Scalar<T> second) => Divide(second);

    readonly Scalar<T> Algebra<Scalar<T>>.IComposite<Scalar<T>>.ComponentwiseMultiply(in Scalar<T> second) => Multiply(second);

    readonly Scalar<T> Algebra<Scalar<T>>.IEucledianVectorSpace<Scalar<T>>.Reflect(in Scalar<T> normal) => throw new InvalidOperationException("The 'reflect' method is undefined for scalar values.");

    readonly bool Algebra<Scalar<T>>.IEucledianVectorSpace<Scalar<T>>.Refract(in Scalar<T> normal, Scalar<T> eta, out Scalar<T> refracted) => throw new InvalidOperationException("The 'refract' method is undefined for scalar values.");

    readonly bool Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.Solve(Scalar<T> v, out Scalar<T> solution) => throw new InvalidOperationException("The 'solve' method is undefined for numeric scalar values.");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.GetRow(int row) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>)[0, row];

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.SetRow(int row, in Scalar<T> vector) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>)[0, row, vector];

    readonly MatrixNM<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.GetRows(Range rows) =>
        (this as Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>).GetRegion(0..1, rows);

    readonly Scalar<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.SetRows(Range rows, in MatrixNM<T> values) =>
        (this as Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>).SetRegion(0..1, rows, values);

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.GetColumn(int column) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>)[column];

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.SetColumn(int column, in Scalar<T> vector) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>)[column, vector];

    readonly MatrixNM<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.GetColumns(Range columns) =>
        (this as Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>).GetRegion(columns, 0..1);

    readonly Scalar<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.SetColumns(Range columns, in MatrixNM<T> values) =>
        (this as Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>).SetRegion(columns, 0..1, values);

    readonly MatrixNM<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.GetRegion(Range columns, Range rows)
    {
        if (columns.GetOffsetAndLength(1) is (0, 1))
            if (rows.GetOffsetAndLength(1) is (0, 1))
                return MatrixNM<T>.FromCoefficients(new Scalar<T>[,] { { this } });
            else
                throw new ArgumentException("The row indices must only contain the value [0].", nameof(rows));
        else
            throw new ArgumentException("The column indices must only contain the value [0].", nameof(rows));
    }

    readonly Scalar<T> Algebra<Scalar<T>, Polynomial<T>>.IMatrix<Scalar<T>, MatrixNM<T>>.SetRegion(Range columns, Range rows, in MatrixNM<T> values)
    {
        if (columns.GetOffsetAndLength(1) is (0, 1))
            if (rows.GetOffsetAndLength(1) is (0, 1))
                return values.Size == (1, 1) ? (Scalar<T>)values[0, 0] : throw new ArgumentException("The new values must be a matrix with the dimensions (1, 1).", nameof(values));
            else
                throw new ArgumentException("The row indices must only contain the value [0].", nameof(rows));
        else
            throw new ArgumentException("The column indices must only contain the value [0].", nameof(rows));
    }

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.MultiplyRow(int row, Scalar<T> factor) => row == 0 ? Multiply(in factor) : throw new ArgumentException("The row index must be zero.", nameof(row));

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.SwapRows(int src_row, int dst_row) => (src_row, dst_row) == (0, 0) ? this : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.AddRows(int src_row, int dst_row) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>).AddRows(src_row, dst_row, One);

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.AddRows(int src_row, int dst_row, Scalar<T> factor) =>
        (src_row, dst_row) == (0, 0) ? Multiply(factor.Increment()) : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.MultiplyColumn(int col, Scalar<T> factor) => col == 0 ? Multiply(in factor) : throw new ArgumentException("The column index must be zero.", nameof(col));

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.SwapColumns(int src_col, int dst_col) => (src_col, dst_col) == (0, 0) ? this : throw new ArgumentException("The source and destination rows must have the index zero.");

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.AddColumns(int src_col, int dst_col) => (this as Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>).AddColumns(src_col, dst_col, One);

    readonly Scalar<T> Algebra<Scalar<T>>.IMatrix<Scalar<T>, Scalar<T>>.AddColumns(int src_col, int dst_col, Scalar<T> factor) =>
        (src_col, dst_col) == (0, 0) ? Multiply( factor.Increment()) : throw new ArgumentException("The source and destination columns must have the index zero.");

    readonly int num.IFloatingPoint<Scalar<T>>.GetExponentByteCount() => ((num.IFloatingPoint<T>)Value).GetExponentByteCount();

    readonly int num.IFloatingPoint<Scalar<T>>.GetExponentShortestBitLength() => ((num.IFloatingPoint<T>)Value).GetExponentShortestBitLength();

    readonly int num.IFloatingPoint<Scalar<T>>.GetSignificandBitLength() => ((num.IFloatingPoint<T>)Value).GetSignificandBitLength();

    readonly int num.IFloatingPoint<Scalar<T>>.GetSignificandByteCount() => ((num.IFloatingPoint<T>)Value).GetSignificandByteCount();

    readonly bool num.IFloatingPoint<Scalar<T>>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<T>)Value).TryWriteExponentBigEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar<T>>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<T>)Value).TryWriteExponentLittleEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar<T>>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<T>)Value).TryWriteSignificandBigEndian(destination, out bytesWritten);

    readonly bool num.IFloatingPoint<Scalar<T>>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => ((num.IFloatingPoint<T>)Value).TryWriteSignificandLittleEndian(destination, out bytesWritten);

    #endregion
    #region PRIVATE METHODS

    //private bool MathFunction(Func<__scalar, bool> func) => func((__scalar)(dynamic)Value);

    //static private Scalar<T> MathFunction(Func<__scalar> func) => new((T)(dynamic)(__scalar)func());

    //private Scalar<T> MathFunction(Func<__scalar, __scalar> func) => new((T)(dynamic)(__scalar)func((__scalar)(dynamic)Value));

    //private Scalar<T> OP(OpType op) => new(OP(op, Value));

    //private Scalar<T> OP(OpType op, Scalar<T> t) => new(OP<T>(op, Value, t.Value));

    //private U OP<U>(OpType op, Scalar<T> t) => OP<U>(op, Value, t.Value);

    //private static T OP(OpType op, T t) => _operators[op].Invoke(null, new object[] { t }) is object result ? (T)result : throw new InvalidProgramException();

    //private static U OP<U>(OpType op, T t1, T t2) => _operators[op].Invoke(null, new object[] { t1, t2 }) is object result ? (U)result : throw new InvalidProgramException();

    #endregion
    #region STATIC METHODS

    public static bool Is(Scalar<T> s1, Scalar<T> s2, Scalar<T>? error = null) => error is null ? s1.Is(s2) : s1.Is(s2, error.Value);

    public static Scalar<T> Negate(Scalar<T> s) => s.Negate();

    public static Scalar<T> Abs(Scalar<T> s) => s.Abs();

    public static Scalar<T> Sqrt(Scalar<T> s) => s.Sqrt();

    public static Scalar<T> Add(Scalar<T> s1, Scalar<T> s2) => s1.Add(s2);

    public static Scalar<T> Subtract(Scalar<T> s1, Scalar<T> s2) => s1.Subtract(s2);

    public static Scalar<T> Multiply(Scalar<T> s1, Scalar<T> s2) => s1.Multiply(s2);

    public static Scalar<T> Divide(Scalar<T> s1, Scalar<T> s2) => s1.Divide(s2);

    public static Scalar<T> Modulus(Scalar<T> s1, Scalar<T> s2) => s1.Modulus(s2);

    public static int ILogB(Scalar<T> x) => x.ILogB();

    public static Scalar<T> Cbrt(Scalar<T> x) => x.Cbrt();

    public static Scalar<T> RootN(Scalar<T> x, int n) => x.RootN(n);

    public static Scalar<T> ScaleB(Scalar<T> x, int n) => x.ScaleB(n);

    public static Scalar<T> Exp10(Scalar<T> x) => x.Exp10();

    public static Scalar<T> Exp2(Scalar<T> x) => x.Exp2();

    public static Scalar<T> Exp(Scalar<T> x) => x.Exp();

    public static Scalar<T> Log10(Scalar<T> x) => x.Log10();

    public static Scalar<T> Log2(Scalar<T> x) => x.Log2();

    public static Scalar<T> Hypot(Scalar<T> x, Scalar<T> y) => Sqrt(x.Multiply(x).Add(y.Multiply(y)));

    public static Scalar<T> Power(Scalar<T> x, Scalar<T> y) => x.Power(y);

    public static Scalar<T> Log(Scalar<T> s) => s.Log();

    public static Scalar<T> Log(Scalar<T> s, Scalar<T> basis) => s.Log().Divide(basis.Log());

    public static Scalar<T> Sinc(Scalar<T> s) => s.Sinc();

    public static Scalar<T> Sin(Scalar<T> s) => s.Sin();

    public static Scalar<T> Cos(Scalar<T> s) => s.Cos();

    public static Scalar<T> Tan(Scalar<T> s) => s.Tan();

    public static Scalar<T> Cot(Scalar<T> s) => s.Cot();

    public static Scalar<T> Sec(Scalar<T> s) => s.Sec();

    public static Scalar<T> Csc(Scalar<T> s) => s.Csc();

    public static Scalar<T> Asin(Scalar<T> s) => s.Asin();

    public static Scalar<T> Acos(Scalar<T> s) => s.Acos();

    public static Scalar<T> Atan(Scalar<T> s) => s.Atan();

    public static Scalar<T> Acot(Scalar<T> s) => s.Acot();

    public static Scalar<T> Acsc(Scalar<T> s) => s.Acsc();

    public static Scalar<T> Asec(Scalar<T> s) => s.Asec();

    public static Scalar<T> Sinh(Scalar<T> s) => s.Sinh();

    public static Scalar<T> Cosh(Scalar<T> s) => s.Cosh();

    public static Scalar<T> Tanh(Scalar<T> s) => s.Tanh();

    public static Scalar<T> Coth(Scalar<T> s) => s.Coth();

    public static Scalar<T> Sech(Scalar<T> s) => s.Sech();

    public static Scalar<T> Csch(Scalar<T> s) => s.Csch();

    public static Scalar<T> Asinh(Scalar<T> s) => s.Asinh();

    public static Scalar<T> Acosh(Scalar<T> s) => s.Acosh();

    public static Scalar<T> Atanh(Scalar<T> s) => s.Atanh();

    public static Scalar<T> Acoth(Scalar<T> s) => s.Acoth();

    public static Scalar<T> Acsch(Scalar<T> s) => s.Acsch();

    public static Scalar<T> Asech(Scalar<T> s) => s.Asech();

    public static Scalar<T> AcosPi(Scalar<T> x) => x.AcosPi();

    public static Scalar<T> AsinPi(Scalar<T> x) => x.AsinPi();

    public static Scalar<T> AtanPi(Scalar<T> x) => x.AtanPi();

    public static Scalar<T> CosPi(Scalar<T> x) => x.CosPi();

    public static Scalar<T> SinPi(Scalar<T> x) => x.SinPi();

    public static Scalar<T> TanPi(Scalar<T> x) => x.TanPi();

    public static Scalar<T> Atan2(Scalar<T> y, Scalar<T> x) => new(T.Atan2(y.Value, x.Value));

    public static Scalar<T> Atan2Pi(Scalar<T> y, Scalar<T> x) => new(T.Atan2Pi(y.Value, x.Value));

    public static (Scalar<T> Sin, Scalar<T> Cos) SinCos(Scalar<T> x) => x.SinCos();

    public static (Scalar<T> SinPi, Scalar<T> CosPi) SinCosPi(Scalar<T> x) => x.SinCosPi();

    public static Scalar<T> Round(Scalar<T> x) => x.Round();

    public static Scalar<T> Round(Scalar<T> x, int digits) => x.Round(digits);

    public static Scalar<T> Round(Scalar<T> x, MidpointRounding mode) => x.Round(mode);

    public static Scalar<T> Round(Scalar<T> x, int digits, MidpointRounding mode) => x.Round(digits, mode);

    public static Scalar<T> Max(Scalar<T> x, Scalar<T> y) => x.Max(y);

    public static Scalar<T> Min(Scalar<T> x, Scalar<T> y) => x.Min(y);

    public static Scalar<T> MaxMagnitude(Scalar<T> x, Scalar<T> y) => new(T.MaxMagnitude(x.Value, y.Value));

    public static Scalar<T> MaxMagnitudeNumber(Scalar<T> x, Scalar<T> y) => new(T.MaxMagnitudeNumber(x.Value, y.Value));

    public static Scalar<T> MinMagnitude(Scalar<T> x, Scalar<T> y) => new(T.MinMagnitude(x.Value, y.Value));

    public static Scalar<T> MinMagnitudeNumber(Scalar<T> x, Scalar<T> y) => new(T.MinMagnitudeNumber(x.Value, y.Value));

    public static Scalar<T> BitDecrement(Scalar<T> x) => x.BitDecrement();

    public static Scalar<T> BitIncrement(Scalar<T> x) => x.BitIncrement();

    public static Scalar<T> FusedMultiplyAdd(Scalar<T> left, Scalar<T> right, Scalar<T> addend) => new(T.FusedMultiplyAdd(left.Value, right.Value, addend.Value));

    public static Scalar<T> Ieee754Remainder(Scalar<T> left, Scalar<T> right) => new(T.Ieee754Remainder(left.Value, right.Value));

    public static bool TryParse(string? str, NumberStyles style, IFormatProvider? provider, out Scalar<T> scalar)
    {
        bool success = T.TryParse(str, style, provider, out T value);

        scalar = success ? new(value) : default;

        return success;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Scalar<T> result) => TryParse(s, NumberStyles.Float, provider, out result);

    public static bool TryParse(string? s, out Scalar<T> result) => TryParse(s, null, out result);

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Scalar<T> result) => TryParse(new string(s), style, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Scalar<T> result) => TryParse(s, NumberStyles.Float, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, out Scalar<T> result) => TryParse(s, null, out result);

    public static Scalar<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => TryParse(s, style, provider, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => TryParse(s, style, provider, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => TryParse(s, provider, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> Parse(ReadOnlySpan<char> s) => TryParse(s, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> Parse(string s, IFormatProvider? provider) => TryParse(s, provider, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> Parse(string s) => TryParse(s, out Scalar<T> value) ? value : throw new FormatException();

    public static Scalar<T> FromArray(params Scalar<T>[] coefficients) =>
        coefficients.Length != 1 ? throw new ArgumentException("Invalid array length.", nameof(coefficients)) : coefficients[0];

    static Scalar<T> num.IPowerFunctions<Scalar<T>>.Pow(Scalar<T> x, Scalar<T> y) => Power(x, y);

    static bool num.INumberBase<Scalar<T>>.IsCanonical(Scalar<T> value) => T.IsCanonical(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsComplexNumber(Scalar<T> value) => T.IsComplexNumber(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsEvenInteger(Scalar<T> value) => T.IsEvenInteger(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsFinite(Scalar<T> value) => T.IsFinite(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsImaginaryNumber(Scalar<T> value) => T.IsImaginaryNumber(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsInfinity(Scalar<T> value) => T.IsInfinity(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsInteger(Scalar<T> value) => T.IsInteger(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsNaN(Scalar<T> value) => T.IsNaN(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsNegative(Scalar<T> value) => T.IsNegative(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsNegativeInfinity(Scalar<T> value) => T.IsNegativeInfinity(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsNormal(Scalar<T> value) => T.IsNormal(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsOddInteger(Scalar<T> value) => T.IsOddInteger(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsPositive(Scalar<T> value) => T.IsPositive(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsPositiveInfinity(Scalar<T> value) => T.IsPositiveInfinity(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsRealNumber(Scalar<T> value) => T.IsRealNumber(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsSubnormal(Scalar<T> value) => T.IsSubnormal(value.Value);

    static bool num.INumberBase<Scalar<T>>.IsZero(Scalar<T> value) => T.IsZero(value.Value);


    static bool num.INumberBase<Scalar<T>>.TryConvertFromChecked<TOther>(TOther value, out Scalar<T> result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar<T>>.TryConvertFromSaturating<TOther>(TOther value, out Scalar<T> result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar<T>>.TryConvertFromTruncating<TOther>(TOther value, out Scalar<T> result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar<T>>.TryConvertToChecked<TOther>(Scalar<T> value, out TOther result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar<T>>.TryConvertToSaturating<TOther>(Scalar<T> value, out TOther result) => throw new NotImplementedException();
    static bool num.INumberBase<Scalar<T>>.TryConvertToTruncating<TOther>(Scalar<T> value, out TOther result) => throw new NotImplementedException();

    #endregion
    #region OPERATORS

    public static bool operator true(Scalar<T> scalar) => scalar.IsNonZero;

    public static bool operator false(Scalar<T> scalar) => scalar.IsZero;

    public static bool operator ==(Scalar<T> s1, Scalar<T> s2) => s1.Is(s2);

    public static bool operator !=(Scalar<T> s1, Scalar<T> s2) => !(s1 == s2);

    public static bool operator <(Scalar<T> s1, Scalar<T> s2) => s1.CompareTo(s2) < 0;

    public static bool operator >(Scalar<T> s1, Scalar<T> s2) => s1.CompareTo(s2) > 0;

    public static bool operator <=(Scalar<T> s1, Scalar<T> s2) => s1.CompareTo(s2) <= 0;

    public static bool operator >=(Scalar<T> s1, Scalar<T> s2) => s1.CompareTo(s2) >= 0;

    public static Scalar<T> operator +(Scalar<T> s) => s;

    public static Scalar<T> operator +(in Scalar<T> s) => s;

    public static Scalar<T> operator -(Scalar<T> s) => s.Negate();

    public static Scalar<T> operator -(in Scalar<T> s) => s.Negate();

    public static Scalar<T> operator ++(Scalar<T> s) => s.Increment();

    public static Scalar<T> operator ++(in Scalar<T> s) => s.Increment();

    public static Scalar<T> operator --(Scalar<T> s) => s.Decrement();

    public static Scalar<T> operator --(in Scalar<T> s) => s.Decrement();

    public static Scalar<T> operator +(Scalar<T> s1, Scalar<T> s2) => s2.Add(s1);

    public static Scalar<T> operator +(in Scalar<T> s1, in Scalar<T> s2) => s2.Add(in s1);

    public static Scalar<T> operator -(Scalar<T> s1, Scalar<T> s2) => s2.Subtract(s1);

    public static Scalar<T> operator -(in Scalar<T> s1, in Scalar<T> s2) => s2.Subtract(in s1);

    public static Scalar<T> operator *(Scalar<T> s1, Scalar<T> s2) => s1.Multiply(s2);

    public static Scalar<T> operator *(in Scalar<T> s1, in Scalar<T> s2) => s1.Multiply(in s2);

    static Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.operator *(Scalar<T> s1, in Scalar<T> s2) => s1.Multiply(in s2);

    static Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.operator *(in Scalar<T> s1, Scalar<T> s2) => s1.Multiply(s2);

    public static Scalar<T> operator ^(in Scalar<T> s, int c) => s.Power(c);

    public static Scalar<T> operator /(Scalar<T> s1, Scalar<T> s2) => s1.Divide(s2);

    public static Scalar<T> operator /(in Scalar<T> s1, in Scalar<T> s2) => s1.Divide(in s2);

    public static Scalar<T> operator /(in Scalar<T> s1, Scalar<T> s2) => s1.Divide(s2);

    public static Scalar<T> operator %(Scalar<T> s1, Scalar<T> s2) => s1.Modulus(s2);

    public static Scalar<T> operator %(in Scalar<T> s1, in Scalar<T> s2) => s1.Modulus(in s2);

    static Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.operator %(in Scalar<T> s1, Scalar<T> s2) => s1.Modulus(s2);

    //static Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.operator *(in Scalar<T> s1, Scalar s2) => s1 * (Scalar<T>)s2;

    //static Scalar<T> Algebra<Scalar<T>>.IVectorSpace<Scalar<T>>.operator *(Scalar s1, in Scalar<T> s2) => (Scalar<T>)s1 * s2;

    public static implicit operator Scalar<T>(T v) => new(v);

    public static implicit operator Scalar<T>(T* ptr) => new(ptr);

    public static implicit operator Scalar<T>(Scalar<T>* ptr) => new(ptr);

    public static implicit operator T(Scalar<T> s) => s.Value;

    public static explicit operator Scalar<T>(Scalar s) => new((T)(dynamic)s.Determinant);

    public static explicit operator Scalar(Scalar<T> s) => (__scalar)(dynamic)s.Value;

    #endregion
    #region NESTED TYPES

    private enum OpType
    {
        op_Equality,
        op_Inequality,
        op_GreaterThan,
        op_LessThan,
        op_GreaterThanOrEqual,
        op_LessThanOrEqual,
        op_Addition,
        op_Subtraction,
        op_Division,
        op_Modulus,
        op_Multiply,
        op_UnaryNegation,
        op_UnaryPlus,
        op_Increment,
        op_Decrement,
    }

    #endregion
}

/// <summary>
/// The equality comparer to be used with the scalar type <see cref="Scalar"/>.
/// </summary>
public sealed class ScalarEqualityComparer
    : IEqualityComparer<Scalar>
    , IEqualityComparer<double>
    , IEqualityComparer<float>
{
    /// <inheritdoc cref="Scalar.Equals(Scalar)"/>
    public bool Equals(Scalar x, Scalar y) => x.Is(y);

    /// <inheritdoc cref="Scalar.GetHashCode"/>
    public int GetHashCode(Scalar obj) => obj.GetHashCode();

    /// <inheritdoc cref="double.Equals(double)"/>
    public bool Equals(double x, double y) => Scalar.Is(x, y);

    /// <inheritdoc cref="double.GetHashCode"/>
    public int GetHashCode(double obj) => obj.GetHashCode();

    /// <inheritdoc cref="float.Equals(float)"/>
    public bool Equals(float x, float y) => Scalar.Is(x, y);

    /// <inheritdoc cref="float.GetHashCode"/>
    public int GetHashCode(float obj) => obj.GetHashCode();
}

/// <summary>
/// The equality comparer to be used with the scalar type <see cref="Scalar{T}"/>.
/// </summary>
public sealed class ScalarEqualityComparer<T>
    : IEqualityComparer<Scalar<T>>
    , IEqualityComparer<T>
    where T : unmanaged, num.IFloatingPointIeee754<T>
{
    /// <inheritdoc cref="Scalar{T}.Equals(Scalar{T})"/>
    public bool Equals(Scalar<T> x, Scalar<T> y) => x.Is(y);

    /// <inheritdoc cref="Scalar{T}.GetHashCode"/>
    public int GetHashCode(Scalar<T> obj) => obj.GetHashCode();

    /// <inheritdoc cref="T.Equals(T)"/>
    public bool Equals(T x, T y) => x.Equals(y) || x.CompareTo(y) == 0;

    /// <inheritdoc cref="T.GetHashCode"/>
    public int GetHashCode(T obj) => obj.GetHashCode();
}
