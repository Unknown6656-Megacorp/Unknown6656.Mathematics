using System;
using System.Collections.Generic;
using System.Linq;

using Unknown6656.Generics;

namespace Unknown6656.Mathematics.Logic.LambdaCalculus;


public abstract record Expression
{
    public abstract bool IsClosedUnder(HashSet<Variable> variables);
}

public record Variable(string Name)
    : Expression
{
    public override bool IsClosedUnder(HashSet<Variable> variables) => variables.Contains(this);

    public override string ToString() => Name;

    public static implicit operator Variable(char name) => new(name.ToString());

    public static implicit operator Variable(string name) => new(name);
}

public record Abstraction(Variable Variable, Expression Body)
    : Expression
{
    public override bool IsClosedUnder(HashSet<Variable> variables) => Body.IsClosedUnder([.. variables, Variable]);

    public override string ToString()
    {
        List<Variable> variables = [Variable];
        Expression body = Body;

        while (body is Abstraction(Variable v, Expression e))
        {
            variables.Add(v);
            body = e;
        }

        return $"(λ{variables.StringJoin(".λ")}.{Body})";
        //return $"(λ{variables.StringJoin(" ")}.{Body})";
    }
}

public record Application(Expression Left, Expression Right)
    : Expression
{
    public override bool IsClosedUnder(HashSet<Variable> variables) => Left.IsClosedUnder(variables) && Right.IsClosedUnder(variables);

    public override string ToString() => (Left, Right) switch
    {
        (_, Application a) => $"{Left} ({a})",
        _ => $"{Left} {Right}"
    };
}


public static class LambdaCalculus
{
    public static Expression AlphaConversion(Expression expression, Variable variable, Expression replacement) => expression switch
    {
        Variable v when v == variable => replacement,
        Abstraction(Variable v, Expression e) when v != variable => new Abstraction(v, AlphaConversion(e, variable, replacement)),
        Application(Expression l, Expression r) => new Application(AlphaConversion(l, variable, replacement), AlphaConversion(r, variable, replacement)),
        _ => expression
    };

    public static Expression BetaReduction(Expression expression) => expression is Application(Abstraction(Variable v, Expression e), Expression r) ? AlphaConversion(e, v, r) : expression;
}
