using System;

using Unknown6656.Mathematics.Analysis;
using Unknown6656.Mathematics.LinearAlgebra;

namespace Unknown6656.Mathematics.Optimization.ParticleSwarmOptimization;

// TODO : maximum / minimum finder
// TODO : complex solver
// TODO : matrixNM solver



public class YValueFinder<Func, Domain, Codomain>
    : PSOProblem<Domain, Scalar, YValueFinder<Func, Domain, Codomain>>
    where Func : Function<Func, Domain, Codomain>
    where Domain : Algebra<Scalar>.IMetricVectorSpace<Domain>
    where Codomain : Algebra<Scalar>.IMetricVectorSpace<Codomain>, IComparable<Codomain>
{
    public Func Function { get; }
    public Codomain YValue { get; }


    public YValueFinder(Func function, Codomain y)
    {
        Function = function;
        YValue = y;
    }

    public override Scalar GetValue(Domain x) => Function.Evaluate(x).DistanceTo(YValue).Abs();

    internal protected override bool IsValidSearchPosition(Domain position)
    {
        try
        {
            return Function.Evaluate(position) is { };
        }
        catch
        {
            return false;
        }
    }

    public override Domain GetZeroVector() => Domain.Zero!;
}

public class YValueFinder<Domain, Codomain>(Function<Domain, Codomain> function, Codomain y)
    : YValueFinder<Function<Domain, Codomain>, Domain, Codomain>(function, y)
    where Domain : Algebra<Scalar>.IMetricVectorSpace<Domain>
    where Codomain : Algebra<Scalar>.IMetricVectorSpace<Codomain>, IComparable<Codomain>;

public class YValueFinder<Domain>(Function<Domain, Domain> function, Domain y)
    : YValueFinder<Domain, Domain>(function, y)
    where Domain : Algebra<Scalar>.IMetricVectorSpace<Domain>, IComparable<Domain>;
