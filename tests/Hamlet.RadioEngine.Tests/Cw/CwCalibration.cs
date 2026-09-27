using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>A decoder's calibration verdict on one condition (HM-REQ-124).</summary>
public enum CwCalibrationVerdict
{
    /// <summary>Every bin of ten or more, and the condition overall, right within 5 points of its mean p, on 30 or more scored characters.</summary>
    Calibrated,

    /// <summary>30 or more scored characters, and a bin of ten or more, or the whole, more than 5 points from its mean p.</summary>
    NotCalibrated,

    /// <summary>Fewer than 30 scored characters: neither calibrated nor not.</summary>
    NotMeasurable,
}

/// <summary>One scored character's stated p and whether the key says it is right.</summary>
/// <param name="P">The probability the decoder attached.</param>
/// <param name="Right">Right by the scorer's verdict; wrong and added are not.</param>
public readonly record struct CwCalibrationPoint(double P, bool Right);

/// <summary>One bin of a reliability table.</summary>
/// <param name="Index">0 to 9: p from Index / 10 to (Index + 1) / 10, the last bin closed.</param>
/// <param name="Count">Characters in it.</param>
/// <param name="MeanP">Their mean p, or NaN where empty.</param>
/// <param name="ShareRight">The share of them right, or NaN where empty.</param>
public readonly record struct CwCalibrationBin(int Index, int Count, double MeanP, double ShareRight)
{
    /// <summary>Share right less mean p, in points.</summary>
    public double DifferencePoints => Count == 0 ? double.NaN : 100 * (ShareRight - MeanP);
}

/// <summary>
/// HM-REQ-124's measure: a logistic fitted by maximum likelihood, the ten-bin
/// reliability table, and the verdict per condition (work instruction 464, tasks 2
/// and 3; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// <para>**A NEW TYPE BESIDE <see cref="CwMetrics"/>, NOT A CHANGE TO IT.** It
/// reads the scorer's verdicts and never moves one.</para>
/// <para>**THE LINES ARE THE INSTRUCTION'S, FIXED BEFORE ANY NUMBER** (464
/// DECIDED (4)): ten bins 0.1 wide; a bin counts when it holds at least 10
/// characters; within 5 points; at least 30 scored characters on a condition, or
/// it is not measurable.</para>
/// </remarks>
public static class CwCalibration
{
    /// <summary>Bins of p, each 0.1 wide.</summary>
    public const int Bins = 10;

    /// <summary>How far a share right may sit from its mean p: 5 points.</summary>
    public const double Tolerance = 0.05;

    /// <summary>The fewest characters a bin holds and still counts.</summary>
    public const int BinFloor = 10;

    /// <summary>The fewest scored characters a condition holds and is measurable.</summary>
    public const int ConditionFloor = 30;

    /// <summary>
    /// Fits p = 1 / (1 + e^-(b0 + b1.x1 + ...)) by maximum likelihood: Newton's
    /// method from zero, at most 100 steps, stopping when no coefficient moves by
    /// 1e-10, with 1e-9 on the Hessian's diagonal only to keep it invertible.
    /// </summary>
    /// <param name="data">Each point's features, the intercept not included, and whether it is right.</param>
    /// <returns>The intercept, then one coefficient per feature.</returns>
    public static double[] Fit(IReadOnlyList<(double[] X, bool Right)> data)
    {
        var n = data.Count == 0 ? 0 : data[0].X.Length;
        var beta = new double[n + 1];

        for (var step = 0; step < 100; step++)
        {
            var gradient = new double[n + 1];
            var hessian = new double[n + 1, n + 1];

            foreach (var (x, right) in data)
            {
                var z = beta[0];

                for (var j = 0; j < n; j++)
                {
                    z += beta[j + 1] * x[j];
                }

                var p = 1 / (1 + Math.Exp(-z));
                var w = p * (1 - p);
                var r = (right ? 1 : 0) - p;

                for (var a = 0; a <= n; a++)
                {
                    var xa = a == 0 ? 1 : x[a - 1];

                    gradient[a] += r * xa;

                    for (var b = 0; b <= n; b++)
                    {
                        hessian[a, b] += w * xa * (b == 0 ? 1 : x[b - 1]);
                    }
                }
            }

            for (var a = 0; a <= n; a++)
            {
                hessian[a, a] += 1e-9;
            }

            var move = Solve(hessian, gradient);
            var largest = 0.0;

            for (var a = 0; a <= n; a++)
            {
                beta[a] += move[a];
                largest = Math.Max(largest, Math.Abs(move[a]));
            }

            if (largest < 1e-10)
            {
                break;
            }
        }

        return beta;
    }

    /// <summary>The logistic at one point.</summary>
    /// <param name="beta">The intercept, then one coefficient per feature.</param>
    /// <param name="x">The features.</param>
    /// <returns>p.</returns>
    public static double Logistic(IReadOnlyList<double> beta, IReadOnlyList<double> x)
    {
        var z = beta[0];

        for (var j = 0; j < x.Count; j++)
        {
            z += beta[j + 1] * x[j];
        }

        return 1 / (1 + Math.Exp(-z));
    }

    /// <summary>Which bin a p falls in.</summary>
    /// <param name="p">The probability.</param>
    /// <returns>0 to 9.</returns>
    public static int BinOf(double p) => Math.Clamp((int)Math.Floor(p * Bins), 0, Bins - 1);

    /// <summary>The ten-bin reliability table.</summary>
    /// <param name="points">The scored characters.</param>
    /// <returns>Ten bins, empty ones included.</returns>
    public static IReadOnlyList<CwCalibrationBin> Table(IReadOnlyList<CwCalibrationPoint> points)
        => Enumerable.Range(0, Bins)
            .Select(b =>
            {
                var here = points.Where(p => BinOf(p.P) == b).ToList();

                return here.Count == 0
                    ? new CwCalibrationBin(b, 0, double.NaN, double.NaN)
                    : new CwCalibrationBin(b, here.Count, here.Average(p => p.P), here.Count(p => p.Right) / (double)here.Count);
            })
            .ToList();

    /// <summary>The verdict on one condition (464 DECIDED (4)).</summary>
    /// <param name="points">The condition's scored characters.</param>
    /// <returns>The verdict.</returns>
    public static CwCalibrationVerdict Judge(IReadOnlyList<CwCalibrationPoint> points)
    {
        if (points.Count < ConditionFloor)
        {
            return CwCalibrationVerdict.NotMeasurable;
        }

        var overall = points.Count(p => p.Right) / (double)points.Count - points.Average(p => p.P);
        var binsHold = Table(points).Where(b => b.Count >= BinFloor).All(b => Within(b.ShareRight - b.MeanP));

        return binsHold && Within(overall) ? CwCalibrationVerdict.Calibrated : CwCalibrationVerdict.NotCalibrated;
    }

    /// <summary>The largest difference in points of any bin holding at least <see cref="BinFloor"/>, sign kept; NaN where none does.</summary>
    /// <param name="points">The scored characters.</param>
    /// <returns>Share right less mean p, in points.</returns>
    public static double WorstPopulatedBin(IReadOnlyList<CwCalibrationPoint> points)
    {
        var populated = Table(points).Where(b => b.Count >= BinFloor).ToList();

        return populated.Count == 0
            ? double.NaN
            : populated.OrderByDescending(b => Math.Abs(b.DifferencePoints)).First().DifferencePoints;
    }

    // 5 points inclusive; 1e-12 only so a hand-built 0.05 is not lost to rounding.
    private static bool Within(double difference) => Math.Abs(difference) <= Tolerance + 1e-12;

    // Gaussian elimination with partial pivoting.
    private static double[] Solve(double[,] a, double[] b)
    {
        var n = b.Length;
        var m = (double[,])a.Clone();
        var v = (double[])b.Clone();

        for (var c = 0; c < n; c++)
        {
            var pivot = c;

            for (var r = c + 1; r < n; r++)
            {
                if (Math.Abs(m[r, c]) > Math.Abs(m[pivot, c]))
                {
                    pivot = r;
                }
            }

            for (var k = 0; k < n; k++)
            {
                (m[c, k], m[pivot, k]) = (m[pivot, k], m[c, k]);
            }

            (v[c], v[pivot]) = (v[pivot], v[c]);

            for (var r = c + 1; r < n; r++)
            {
                var f = m[r, c] / m[c, c];

                for (var k = c; k < n; k++)
                {
                    m[r, k] -= f * m[c, k];
                }

                v[r] -= f * v[c];
            }
        }

        var x = new double[n];

        for (var r = n - 1; r >= 0; r--)
        {
            var s = v[r];

            for (var k = r + 1; k < n; k++)
            {
                s -= m[r, k] * x[k];
            }

            x[r] = s / m[r, r];
        }

        return x;
    }
}
