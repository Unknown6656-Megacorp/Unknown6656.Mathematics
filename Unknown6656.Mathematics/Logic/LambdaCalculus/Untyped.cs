using System.Collections.Generic;
using System.Linq.Expressions;

using Unknown6656.Generics;

namespace Unknown6656.Mathematics.Logic.LambdaCalculus;


public abstract record Expression
{
    public abstract Expression Reduced { get; }

    public bool IsIrreducible => Reduced == this;

    public abstract bool IsClosedUnder(HashSet<Variable> variables);
}

public record Variable(string Name)
    : Expression
{
    public override Variable Reduced => this;


    public Variable(char name)
        : this(name.ToString())
    {
    }

    public override string ToString() => Name;

    public override bool IsClosedUnder(HashSet<Variable> variables) => variables.Contains(this);


    public static implicit operator Variable(char name) => new(name);

    public static implicit operator Variable(string name) => new(name);
}

public record Abstraction(Variable Variable, Expression Body)
    : Expression
{
    public override Abstraction Reduced => new(Variable, Body.Reduced);


    public override string ToString()
    {
        List<Variable> variables = [Variable];
        Expression body = Body;

        while (body is Abstraction(Variable v, Expression e))
        {
            variables.Add(v);
            body = e;
        }

        return $"(λ{variables.StringJoin(".λ")}.{body})";
    }

    public override bool IsClosedUnder(HashSet<Variable> variables) => Body.IsClosedUnder([.. variables, Variable]);
}

public record Application(Expression Left, Expression Right)
    : Expression
{
    public override Expression Reduced => Left is Abstraction(Variable v, Expression e) ? LambdaCalculus.AlphaConversion(e, v, Right).Reduced : new Application(Left.Reduced, Right.Reduced);


    public override string ToString() => (Left, Right) switch
    {
        (_, Application a) => $"{Left} ({a})",
        _ => $"{Left} {Right}"
    };

    public override bool IsClosedUnder(HashSet<Variable> variables) => Left.IsClosedUnder(variables) && Right.IsClosedUnder(variables);
}


public static class LambdaCalculus
{
    public static Variable VAR(char name) => new(name);

    public static Variable VAR(string name) => new(name);

    public static Abstraction ABS(Variable variable, Expression body) => new(variable, body);

    public static Application APP(Expression left, Expression right) => new(left, right);



    public static Expression AlphaConversion(Expression expression, Variable variable, Expression replacement) => expression switch
    {
        Variable v when v == variable => replacement,
        Abstraction(Variable v, Expression e) when v != variable => new Abstraction(v, AlphaConversion(e, variable, replacement)),
        Application(Expression l, Expression r) => new Application(AlphaConversion(l, variable, replacement), AlphaConversion(r, variable, replacement)),
        _ => expression
    };

    public static Expression BetaReduction(Expression expression) => expression is Application(Abstraction(Variable v, Expression e), Expression r) ? AlphaConversion(e, v, r) : expression;

    // TODO : eta conversion
}


// TODO : file:///R:/KIT/Module.Skripte/[S05]%20Programmierparadigmen/slides-4.pdf
// TODO : type systems
// TODO : pi calculus               https://en.wikipedia.org/wiki/%CE%A0-calculus
// TODO : kappa calculus            https://en.wikipedia.org/wiki/Kappa_calculus
// TODO : typed lambda calculus     https://en.wikipedia.org/wiki/Typed_lambda_calculus
// TODO : tromp diagrams            https://tromp.github.io/cl/diagrams.html