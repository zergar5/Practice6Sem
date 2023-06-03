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
            new[] { 0d, 0.1d, 0.101d, 10d },
            new UniformSplitter(1),
            new UniformSplitter(1),
            new UniformSplitter(1)
        )
    )
    .SetZAxis(new AxisSplitParameter(
            new[] { 0d, 6d, 12d },
            new ProportionalSplitter(10, 0.9d),
            new ProportionalSplitter(10, 1.1d)
        )
    )
    .Build();

var materialFactory = new MaterialFactory
(
    new List<double> { 1d },
    new List<double> { 1d }
);
var omega = 100000d;

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

Func<Node2D, Complex> u = p => new Complex(p.R * (p.R - 10) * p.Z * (p.Z - 12), -p.R * (p.R - 10) * p.Z * (p.Z - 12));

var f = new RightPartParameter(
        (p, mu, sigma) => new Complex(
            (-(p.Z * (p.Z - 12) * (4 * p.R - 10) / p.R) + u(p).Real / (p.R * p.R) -
            2 * p.R * (p.R - 10)) / mu - omega * sigma * u(p).Imaginary,
            (p.Z * (p.Z - 12) * (4 * p.R - 10) / p.R + u(p).Imaginary / (p.R * p.R) +
             2 * p.R * (p.R - 10)) / mu + omega * sigma * u(p).Real
            ), grid);

var derivativeCalculator = new DerivativeCalculator();

var localAssembler = new LocalAssembler(grid, localBasisFunctionsProvider, materialFactory, f,
    new DoubleIntegralCalculator(), derivativeCalculator, omega);

var inserter = new Inserter();
var globalAssembler = new GlobalAssembler<Node2D>(new MatrixPortraitBuilder(), localAssembler, inserter, new GaussExcluder());

var firstBoundaryProvider = new FirstBoundaryProvider(grid);
var conditions = firstBoundaryProvider.GetConditions(3, 20);

var equation = globalAssembler
    .AssembleEquation(grid)
    .ApplyFirstConditions(conditions)
    .BuildEquation();

var luPreconditioner = new LUPreconditioner();

var los = new LOS(luPreconditioner, new LUSparse(luPreconditioner));
var solution = los.Solve(equation);

var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider, omega);
var error = femSolution.CalcError(u);

Console.WriteLine(error);