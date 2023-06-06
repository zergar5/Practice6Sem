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
//var grid = gridBuilder2D
//    .SetRAxis(new AxisSplitParameter(
//            new[] { 1d, 1.1d, 1.101d, 25d },
//            new UniformSplitter(1),
//            new UniformSplitter(1),
//            new UniformSplitter(1)
//        )
//    )
//    .SetZAxis(new AxisSplitParameter(
//            new[] { 1d, 7d, 13d },
//            new ProportionalSplitter(10, 0.56),
//            new ProportionalSplitter(10, 1.44)
//        )
//    )
//    .Build();

var grid = gridBuilder2D
    .SetRAxis(new AxisSplitParameter(
            new[] { 1d, 2d },
            new UniformSplitter(2)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            new[] { 1d, 2d },
            new UniformSplitter(2)
        )
    )
    .Build();

var materialFactory = new MaterialFactory
(
    new List<double> { 1d },
    new List<double> { 1d }
);
var omega = 1d;

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

//Func<Node2D, Complex> u = p => new Complex((p.R - 1d) * (p.R - 25d) * (p.Z - 1d) * (p.Z - 13d), -(p.R - 1d) * (p.R - 25d) * (p.Z - 1d) * (p.Z - 13d));

//var f = new RightPartParameter
//(
//    (p, mu, sigma) => new Complex(
//        (-((p.Z - 13d) * (p.Z - 1d) * (4d * p.R - 26d) / p.R) + u(p).Real / (p.R * p.R) -
//         2d * (p.R - 25d) * (p.R - 1d)) / mu - omega * sigma * u(p).Imaginary,
//        ((p.Z - 13d) * (p.Z - 1d) * (4d * p.R - 26d) / p.R + u(p).Imaginary / (p.R * p.R) +
//         2d * (p.R - 25d) * (p.R - 1d)) / mu + omega * sigma * u(p).Real
//    ),
//    grid
//);

Func<Node2D, Complex> u = p => new Complex(p.R, p.R);

var f = new RightPartParameter
(
    (p, mu, sigma) => new Complex(
        - omega * sigma * u(p).Imaginary,
        omega * sigma * u(p).Real
    ),
    grid
);

var derivativeCalculator = new DerivativeCalculator();

var localAssembler = new LocalAssembler(grid, localBasisFunctionsProvider, materialFactory, f,
    new DoubleIntegralCalculator(), derivativeCalculator, omega);

var inserter = new Inserter();
var globalAssembler = new GlobalAssembler<Node2D>(new MatrixPortraitBuilder(), localAssembler, inserter, new GaussExcluder());

var firstBoundaryProvider = new FirstBoundaryProvider(grid, u);
var conditions = firstBoundaryProvider.GetConditions(2, 2);

var equation = globalAssembler
    .AssembleEquation(grid)
    .ApplyFirstConditions(conditions)
    .BuildEquation();

var preconditionMatrix = globalAssembler.BuildPreconditionMatrix();

var luPreconditioner = new LUPreconditioner();

var los = new LOS(luPreconditioner, new LUSparse(luPreconditioner));
var solution = los.Solve(equation, preconditionMatrix);

var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider, omega);

var error = femSolution.CalcError(u);
var fieldValues = femSolution.Calculate(1d, 1.05d);

Console.WriteLine(error);