using Practice6Sem.Calculus;
using Practice6Sem.Core.GridComponents;
using Practice6Sem.FEM;
using Practice6Sem.GridGenerator;
using Practice6Sem.GridGenerator.Area.Splitting;
using Practice6Sem.SLAE.Preconditions;
using Practice6Sem.SLAE.Solvers;
using Practice6Sem.TwoDimensional;
using Practice6Sem.TwoDimensional.Assembling;
using Practice6Sem.TwoDimensional.Assembling.Boundary;
using Practice6Sem.TwoDimensional.Assembling.Global;
using Practice6Sem.TwoDimensional.Assembling.Local;
using Practice6Sem.TwoDimensional.Parameters;
using System;
using System.Globalization;
using System.Numerics;
using static System.Math;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var gridBuilder2D = new GridBuilder2D();
var grid = gridBuilder2D
    .SetRAxis(new AxisSplitParameter(
            new[] { 1d, 1.1d, 1.101d, 15d },
            new UniformSplitter(2),
            new UniformSplitter(1),
            new UniformSplitter(7)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            new[] { -13d, -10d, -7d, -4d, -1d },
            new ProportionalSplitter(4, 0.7),
            new ProportionalSplitter(16, 0.9),
            new ProportionalSplitter(16, 1.1),
            new ProportionalSplitter(4, 1.3)
        )
    )
    .SetMaterials(new[]
    {
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2, //0
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2, //3
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4, //0
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4,
        0, 0, 1, 3, 3, 4, 4, 4, 4, 4, // 15
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3, //0
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3,
        0, 0, 1, 4, 4, 3, 3, 3, 3, 3, // 15
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2, //0
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 1, 2, 2, 2, 2, 2, 2, 2, //3
    })
    .Build();

//var grid = gridBuilder2D
//    .SetRAxis(new AxisSplitParameter(
//            new[] { 1d, 2d },
//            new UniformSplitter(32)
//        )
//    )
//    .SetZAxis(new AxisSplitParameter(
//            new[] { 1d, 2d },
//            new UniformSplitter(32)
//        )
//    )
//    .Build();

var materialFactory = new MaterialFactory
(
    new List<double> { 1d, 1d, 1d, 1d, 1d },
    new List<double> { 1d, 0.9, 0.5, 0.1, 0.25 }
);
var omega = 100000d;

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

Func<Node2D, Complex> u = p => new Complex((p.R - 1d) * (p.R - 15d) * (p.Z + 1d) * (p.Z + 13d), -(p.R - 1d) * (p.R - 15d) * (p.Z + 1d) * (p.Z + 13d));

var f = new RightPartParameter
(
    (p, mu, sigma) => new Complex(
        (-((p.Z + 13d) * (p.Z + 1d) * (4d * p.R - 16d) / p.R) + u(p).Real / (p.R * p.R) -
         2d * (p.R - 15d) * (p.R - 1d)) / mu - omega * sigma * u(p).Imaginary,
        ((p.Z + 13d) * (p.Z + 1d) * (4d * p.R - 16d) / p.R + u(p).Imaginary / (p.R * p.R) +
         2d * (p.R - 15d) * (p.R - 1d)) / mu + omega * sigma * u(p).Real
    ),
    grid
);

//Func<Node2D, Complex> u = p => new Complex(Exp(p.R), Exp(p.Z));

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        (-Exp(p.R) * (p.R + 1) / p.R + u(p).Real / Pow(p.R, 2)) / mu - omega * sigma * u(p).Imaginary,
//        (-Exp(p.Z) + u(p).Imaginary / Pow(p.R, 2)) / mu + omega * sigma * u(p).Real
//    ),
//    grid
//);

//Func<Node2D, Complex> u = p => new Complex(Pow(p.R, 2), Pow(p.Z, 2));

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        -3d - omega * sigma * u(p).Imaginary,
//        -2d / mu + u(p).Imaginary / (Pow(p.R, 2) * mu) + omega * sigma * u(p).Real
//    ),
//    grid
//);

//Func<Node2D, Complex> u = p => new Complex(Pow(p.R, 2), Pow(p.R, 2));

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        -3d - omega * sigma * u(p).Imaginary,
//        -3d + omega * sigma * u(p).Real
//    ),
//    grid
//);

//Func<Node2D, Complex> u = p => new Complex(p.R, p.Z);

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        -1d / (mu * p.R) + u(p).Real / (Pow(p.R, 2) * mu) - omega * sigma * u(p).Imaginary,
//        u(p).Imaginary / (Pow(p.R, 2) * mu) + omega * sigma * u(p).Real
//    ),
//    grid
//);

//Func<Node2D, Complex> u = p => new Complex(p.R, p.R);

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        -omega * sigma * u(p).Imaginary,
//        omega * sigma * u(p).Real
//    ),
//    grid
//);

var derivativeCalculator = new DerivativeCalculator();

var localAssembler = new LocalAssembler(grid, localBasisFunctionsProvider, materialFactory, f,
    new DoubleIntegralCalculator(), derivativeCalculator, omega);

var inserter = new Inserter();
var globalAssembler = new GlobalAssembler<Node2D>(new MatrixPortraitBuilder(), localAssembler, inserter, new GaussExcluder());

var firstBoundaryProvider = new FirstBoundaryProvider(grid, u);
var conditions = firstBoundaryProvider.GetConditions(10, 40);

var equation = globalAssembler
    .AssembleEquation(grid)
    .ApplyFirstConditions(conditions)
    .BuildEquation();

var preconditionMatrix = globalAssembler.BuildPreconditionMatrix();

var luPreconditioner = new LUPreconditioner();

var los = new LOS(luPreconditioner, new LUSparse(luPreconditioner));
var solution = los.Solve(equation, preconditionMatrix);

var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider, omega);

var fieldValues = femSolution.CalculateField(5d, 1.05d);

foreach (var value in fieldValues)
{
    Console.WriteLine(value);
}