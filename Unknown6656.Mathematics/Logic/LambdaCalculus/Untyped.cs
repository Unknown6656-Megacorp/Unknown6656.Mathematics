using System.Collections.Generic;
using System.Linq;
using System;

using Unknown6656.Generics;

namespace Unknown6656.Mathematics.Logic.LambdaCalculus;

using static LambdaCalculus;


public abstract record Expression
{
    public bool IsIrreducible => Reduced == this;

    public abstract Expression Reduced { get; }

    public virtual Expression this[params Expression[] expression] => expression switch
    {
        [] => this,
        [Expression e, ..Expression[] es] => new Application(this, e).Reduced[es]
    };


    public abstract bool IsClosedUnder(HashSet<Variable> variables);

    public Expression BetaReduction() => this is Application(Abstraction(Variable v, Expression e), Expression r) ? e.AlphaConversion(v, r) : this;

    public Expression AlphaConversion(Variable variable, Expression replacement) => this switch
    {
        Variable v when v == variable => replacement,
        Abstraction(Variable v, Expression e) when v != variable => new Abstraction(v, e.AlphaConversion(variable, replacement)),
        Application(Expression l, Expression r) => new Application(l.AlphaConversion(variable, replacement), r.AlphaConversion(variable, replacement)),
        _ => this
    };

    // TODO : eta conversion
}

public record Variable(int ID)
    : Expression
{
    public string? DisplayName { get; set; }

    public override Variable Reduced => this;


    public Variable(char name)
        : this(name.ToString())
    {
    }

    public Variable(string name)
        : this(Random.Shared.Next()) => DisplayName = name;

    public override int GetHashCode() => ID;

    public override string ToString() => DisplayName ?? $"${ID:x8}";

    public override bool IsClosedUnder(HashSet<Variable> variables) => variables.Contains(this);

    public static Variable CreateNew() => new(Random.Shared.Next());

    public static implicit operator Variable(char name) => new(name);

    public static implicit operator Variable(string name) => new(name);
}

public record Abstraction(Variable Variable, Expression Body)
    : Expression
{
    public override Abstraction Reduced => new(Variable, Body.Reduced);

    public override Expression this[params Expression[] expression] => expression switch
    {
        [] => this,
        [Expression e] => Body.AlphaConversion(Variable, e).Reduced,
        [Expression e, ..Expression[] es] => Body.AlphaConversion(Variable, e).Reduced[es],
    };


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
    public override Expression Reduced => Left is Abstraction(Variable v, Expression e) ? e.AlphaConversion(v, Right).Reduced : new Application(Left.Reduced, Right.Reduced);


    public override string ToString() => (Left, Right) switch
    {
        (_, Application a) => $"{Left} ({a})",
        _ => $"{Left} {Right}"
    };

    public override bool IsClosedUnder(HashSet<Variable> variables) => Left.IsClosedUnder(variables) && Right.IsClosedUnder(variables);
}

public static class LambdaCalculus
{
    public static Abstraction YCombinator { get; } = λ('f', 'x', (f, x) => f[x, x[x]]);

    public static Abstraction IdentityFunction { get; } = λ('x', x => x);

    public static Abstraction TuringFixedPointCombinator { get; } = λ('f', 'x', 'y', (f, x, y) => y[x[x, y], λ("x'", x => f[x[x]])]);


    public static Abstraction λ(Func<Variable, Expression> closure)
    {
        Variable variable = Variable.CreateNew();

        return new(variable, closure(variable));
    }

    public static Abstraction λ(Union<char, string>? name, Func<Variable, Expression> closure)
    {
        if (name is null)
            return λ(closure);
        else
        {
            Variable variable = new(name.Match(c => c.ToString(), LINQ.id));

            return new(variable, closure(variable));
        }
    }

    public static Abstraction λ(Union<char, string>? var1, Union<char, string>? var2, Func<Variable, Variable, Expression> closure) =>
        λ(var1, v1 => λ(var2, v2 => closure(v1, v2)));

    public static Abstraction λ(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Func<Variable, Variable, Variable, Expression> closure) =>
        λ(var1, v1 => λ(var2, v2 => λ(var3, v3 => closure(v1, v2, v3))));

    public static Abstraction λ(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Union<char, string>? var4, Func<Variable, Variable, Variable, Variable, Expression> closure) =>
        λ(var1, v1 => λ(var2, v2 => λ(var3, v3 => λ(var4, v4 => closure(v1, v2, v3, v4)))));

    public static Abstraction λ(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Union<char, string>? var4, Union<char, string>? var5, Func<Variable, Variable, Variable, Variable, Variable, Expression> closure) =>
        λ(var1, v1 => λ(var2, v2 => λ(var3, v3 => λ(var4, v4 => λ(var5, v5 => closure(v1, v2, v3, v4, v5))))));



    // only alias for λ
    public static Abstraction LAMBDA(Func<Variable, Expression> closure) => λ(closure);

    // only alias for λ
    public static Abstraction LAMBDA(Union<char, string>? name, Func<Variable, Expression> closure) => λ(name, closure);

    // only alias for λ
    public static Abstraction LAMBDA(Union<char, string>? var1, Union<char, string>? var2, Func<Variable, Variable, Expression> closure) => LAMBDA(var1, var2, closure);

    // only alias for λ
    public static Abstraction LAMBDA(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Func<Variable, Variable, Variable, Expression> closure) => LAMBDA(var1, var2, var3, closure);

    // only alias for λ
    public static Abstraction LAMBDA(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Union<char, string>? var4, Func<Variable, Variable, Variable, Variable, Expression> closure) => LAMBDA(var1, var2, var3, var4, closure);

    // only alias for λ
    public static Abstraction LAMBDA(Union<char, string>? var1, Union<char, string>? var2, Union<char, string>? var3, Union<char, string>? var4, Union<char, string>? var5, Func<Variable, Variable, Variable, Variable, Variable, Expression> closure) => LAMBDA(var1, var2, var3, var4, var5, closure);
}

public static class ChurchNumeral
{
    public static Abstraction True { get; } = λ('t', 'f', (t, f) => t);
    public static Abstraction False { get; } = λ('t', 'f', (t, f) => f);
    public static Abstraction And { get; } = λ('p', 'q', (p, q) => p[q, p]);
    public static Abstraction Or { get; } = λ('p', 'q', (p, q) => p[p, q]);
    public static Abstraction Not { get; } = λ('p', p => p[False, True]);
    public static Abstraction IfThenElse { get; } = λ('p', 'a', 'b', (p, a, b) => p[a, b]);

    public static Abstraction IsZero { get; } = λ('n', n => n[λ('x', _ => False), True]);

    public static Abstraction Zero { get; } = λ('f', 'x', (f, x) => x);
    public static Abstraction One { get; } = λ('f', 'x', (f, x) => f[x]);
    public static Abstraction Two { get; } = λ('f', 'x', (f, x) => f[f[x]]);

    public static Abstraction Predecessor { get; } = λ('n', 'f', 'x', (n, f, x) => n[λ('g', 'h', (g, h) => h[g[f]]), λ('u', u => x), λ('u', u => u)]);
    public static Abstraction Subtract { get; } = λ('m', 'n', (m, n) => n[Predecessor, m]);
    public static Abstraction Successor { get; } = λ('n', 'f', 'x', (n, f, x) => f[n[f, x]]);
    public static Abstraction Plus { get; } = λ('m', 'n', 'f', 'x', (m, n, f, x) => m[f, n[f, x]]);
    public static Abstraction Multiply { get; } = λ('m', 'n', 'f', (m, n, f) => m[n[f]]);
    public static Abstraction Power { get; } = λ('b', 'e', (b, e) => e[b]);


    public static Expression ToChurchNumeral(uint number) => number switch
    {
        0 => Zero,
        1 => One,
        2 => Two,
        _ => Successor[ToChurchNumeral(number - 1)],
    };
}


// TODO : file:///R:/KIT/Module.Skripte/[S05]%20Programmierparadigmen/slides-4.pdf
// TODO : type systems
// TODO : implement shadowing
// TODO : correct handling of free variables / bounded variables
// TODO : pi calculus               https://en.wikipedia.org/wiki/%CE%A0-calculus
// TODO : kappa calculus            https://en.wikipedia.org/wiki/Kappa_calculus
// TODO : typed lambda calculus     https://en.wikipedia.org/wiki/Typed_lambda_calculus
// TODO : tromp diagrams            https://tromp.github.io/cl/diagrams.html