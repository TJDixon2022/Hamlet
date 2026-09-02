using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The proof that <see cref="Ft8Fft"/> and <see cref="Ft8RealFft"/> compute the discrete Fourier
/// transform, held against the defining sum computed independently in <see cref="NaiveDft"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every measurement is printed before any bound is asserted.</b> Unit 212's most portable lesson:
/// a tolerance chosen before the measurement is where a laundered failure gets in. Each test here
/// writes the maximum it saw to the test output, and the assertion that follows is a number chosen
/// after reading it, with the gap between the two stated.
/// </para>
/// <para>
/// <b>What the bounds are relative to.</b> An absolute error means nothing without a scale. The
/// scale used throughout is the largest bin magnitude of the transform in question, so a bound reads
/// as <i>parts in the largest thing the answer contains</i>. Where a raw absolute number is also
/// useful it is printed beside the relative one.
/// </para>
/// <para>
/// <b>The reference has error too, and it is the larger of the two.</b> The naive sum accumulates N
/// terms in one running total; a Cooley–Tukey accumulates about log2(N) levels. So the difference
/// measured here is dominated by the reference, not by the thing under test, and the bounds grow
/// with N for that reason and not because the transform is getting worse.
/// </para>
/// <para>
/// <b>Nothing here reads the clone.</b> These tests run on any machine.
/// </para>
/// </remarks>
public class Ft8FftTests
{
    private readonly ITestOutputHelper _output;

    public Ft8FftTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The lengths swept. Powers of two from the smallest a human can check by hand up to past what
    /// the monitor wants, and the two lengths the monitor actually wants — <b>1920 and 3840, neither
    /// of them a power of two</b> — together with the small non-power-of-two sizes that exercise the
    /// radix-3, radix-5 and prime-radix combines on their own.
    /// </summary>
    private static readonly int[] Lengths =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 16, 24, 30, 32, 60, 64, 120, 128, 240, 256,
        480, 512, 960, 1024, 1920, 2048, 3840, 4096,
    };

    /// <summary>The same lengths, as xunit wants them.</summary>
    public static TheoryData<int> SweptLengths
    {
        get
        {
            var data = new TheoryData<int>();
            foreach (var length in Lengths)
            {
                data.Add(length);
            }

            return data;
        }
    }

    /// <summary>
    /// <b>The load-bearing test.</b> The transform against the defining sum, at every swept length,
    /// on random input.
    /// </summary>
    [Theory]
    [MemberData(nameof(SweptLengths))]
    public void TheTransformAgreesWithTheDefiningSumOnRandomInput(int length)
    {
        var plan = new Ft8Fft(length);
        var random = new Random(21301 + length);

        var real = new double[length];
        var imaginary = new double[length];
        for (var i = 0; i < length; i++)
        {
            real[i] = (random.NextDouble() * 2) - 1;
            imaginary[i] = (random.NextDouble() * 2) - 1;
        }

        var fastReal = new double[length];
        var fastImaginary = new double[length];
        plan.Transform(real, imaginary, fastReal, fastImaginary);

        var slowReal = new double[length];
        var slowImaginary = new double[length];
        NaiveDft.Forward(real, imaginary, slowReal, slowImaginary);

        var (absolute, scale, relative) = Compare(fastReal, fastImaginary, slowReal, slowImaginary);

        _output.WriteLine($"length {length,5}  radices {string.Join("x", plan.Factors),-24} "
            + $"pure radix-2 {(plan.IsPureRadix2 ? "yes" : "no "),-3} "
            + $"max |diff| {absolute:E3}  largest bin {scale:F4}  relative {relative:E3}");

        // The bound was set after reading the line above across every length in the sweep. See
        // TheWorstErrorAcrossTheWholeSweepIsMeasuredBeforeTheBoundIsAsserted for the measurement that
        // chose it and for the margin between the two.
        Assert.True(
            relative < 1e-13,
            $"length {length}: relative error {relative:E3} exceeded 1e-13. This is a measurement to "
            + "reason about, not a bound to widen.");
    }

    /// <summary>
    /// The whole sweep in one place, so the worst case over all of it is a printed number rather
    /// than something a reader has to assemble from thirty lines.
    /// </summary>
    /// <remarks>
    /// <b>This is the test the asserted bound was chosen from.</b> It prints the maximum first and
    /// asserts afterwards, and the gap between the measurement and the bound is stated in the output
    /// rather than left to be inferred.
    /// </remarks>
    [Fact]
    public void TheWorstErrorAcrossTheWholeSweepIsMeasuredBeforeTheBoundIsAsserted()
    {
        double worstRelative = 0;
        double worstAbsolute = 0;
        var worstLength = 0;

        foreach (var length in Lengths)
        {
            var plan = new Ft8Fft(length);
            var random = new Random(90210 + length);

            var real = new double[length];
            var imaginary = new double[length];
            for (var i = 0; i < length; i++)
            {
                real[i] = (random.NextDouble() * 2) - 1;
                imaginary[i] = (random.NextDouble() * 2) - 1;
            }

            var fastReal = new double[length];
            var fastImaginary = new double[length];
            plan.Transform(real, imaginary, fastReal, fastImaginary);

            var slowReal = new double[length];
            var slowImaginary = new double[length];
            NaiveDft.Forward(real, imaginary, slowReal, slowImaginary);

            var (absolute, _, relative) = Compare(fastReal, fastImaginary, slowReal, slowImaginary);
            if (relative > worstRelative)
            {
                worstRelative = relative;
                worstAbsolute = absolute;
                worstLength = length;
            }
        }

        const double bound = 1e-13;

        _output.WriteLine("MEASURED FIRST, ASSERTED AFTER.");
        _output.WriteLine($"worst relative error over the sweep : {worstRelative:E6}");
        _output.WriteLine($"  at length                         : {worstLength}");
        _output.WriteLine($"  absolute difference there         : {worstAbsolute:E6}");
        _output.WriteLine($"bound asserted                      : {bound:E6}");
        _output.WriteLine($"headroom                            : {bound / worstRelative:F1}x");
        _output.WriteLine(string.Empty);
        _output.WriteLine("Why the gap is what it is: double precision carries about 2.2e-16 per");
        _output.WriteLine("operation, and the NAIVE side accumulates N terms into one running sum");
        _output.WriteLine("while the transform under test accumulates about log2(N) levels. So the");
        _output.WriteLine("difference measured is mostly the reference's own error, which grows");
        _output.WriteLine("roughly with N. At the longest length swept that is a few thousand times");
        _output.WriteLine("2.2e-16, which is where the measurement above sits. The bound is one");
        _output.WriteLine("round order of magnitude above the worst measurement and no more; it is");
        _output.WriteLine("not a tolerance that would absorb a real defect, because a transposed");
        _output.WriteLine("index or a sign error moves a bin by order one, not by order 1e-13.");

        Assert.True(worstRelative < bound, $"worst relative error {worstRelative:E6} exceeded {bound:E6}.");
    }

    /// <summary>
    /// A length small enough to check on paper, and checked against the arithmetic rather than
    /// against the reference. <b>The one place tonight where a human is the oracle.</b>
    /// </summary>
    /// <remarks>
    /// x = [1, 2, 3, 4] with zero imaginary parts. By hand, with W = exp(-i*pi/2) = -i:
    /// X[0] = 1+2+3+4 = 10;
    /// X[1] = 1 + 2(-i) + 3(-1) + 4(i) = -2 + 2i;
    /// X[2] = 1 - 2 + 3 - 4 = -2;
    /// X[3] = 1 + 2(i) + 3(-1) + 4(-i) = -2 - 2i.
    /// </remarks>
    [Fact]
    public void AFourPointTransformMatchesTheOneWorkedOutByHand()
    {
        var plan = new Ft8Fft(4);
        var real = new double[] { 1, 2, 3, 4 };
        var imaginary = new double[4];
        var outReal = new double[4];
        var outImaginary = new double[4];

        plan.Transform(real, imaginary, outReal, outImaginary);

        for (var k = 0; k < 4; k++)
        {
            _output.WriteLine($"X[{k}] = {outReal[k]:F12} + {outImaginary[k]:F12}i");
        }

        var expectedReal = new double[] { 10, -2, -2, -2 };
        var expectedImaginary = new double[] { 0, 2, 0, -2 };

        double worst = 0;
        for (var k = 0; k < 4; k++)
        {
            worst = Math.Max(worst, Math.Abs(outReal[k] - expectedReal[k]));
            worst = Math.Max(worst, Math.Abs(outImaginary[k] - expectedImaginary[k]));
        }

        _output.WriteLine($"worst difference from the hand result: {worst:E3}");
        Assert.True(worst < 1e-14, $"the four-point transform is off the hand result by {worst:E3}.");
    }

    /// <summary>Structured input rather than random: a chirp, which fills every bin unevenly.</summary>
    [Theory]
    [InlineData(256)]
    [InlineData(1920)]
    [InlineData(3840)]
    public void TheTransformAgreesWithTheDefiningSumOnAChirp(int length)
    {
        var plan = new Ft8Fft(length);

        var real = new double[length];
        var imaginary = new double[length];
        for (var i = 0; i < length; i++)
        {
            var phase = Math.PI * i * i / length;
            real[i] = Math.Cos(phase);
            imaginary[i] = Math.Sin(phase);
        }

        var fastReal = new double[length];
        var fastImaginary = new double[length];
        plan.Transform(real, imaginary, fastReal, fastImaginary);

        var slowReal = new double[length];
        var slowImaginary = new double[length];
        NaiveDft.Forward(real, imaginary, slowReal, slowImaginary);

        var (absolute, scale, relative) = Compare(fastReal, fastImaginary, slowReal, slowImaginary);
        _output.WriteLine($"chirp, length {length}: max |diff| {absolute:E3}, largest bin {scale:F4}, relative {relative:E3}");

        Assert.True(relative < 1e-13, $"chirp at length {length}: relative error {relative:E3}.");
    }

    /// <summary>The real-input path against the defining sum taken over real input.</summary>
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(30)]
    [InlineData(256)]
    [InlineData(1920)]
    [InlineData(3840)]
    public void TheRealTransformAgreesWithTheDefiningSum(int length)
    {
        var plan = new Ft8RealFft(length);
        Assert.Equal((length / 2) + 1, plan.BinCount);

        var random = new Random(4004 + length);
        var samples = new double[length];
        for (var i = 0; i < length; i++)
        {
            samples[i] = (random.NextDouble() * 2) - 1;
        }

        var fastReal = new double[plan.BinCount];
        var fastImaginary = new double[plan.BinCount];
        plan.Transform(samples, fastReal, fastImaginary);

        var slowReal = new double[plan.BinCount];
        var slowImaginary = new double[plan.BinCount];
        NaiveDft.ForwardReal(samples, slowReal, slowImaginary);

        var (absolute, scale, relative) = Compare(fastReal, fastImaginary, slowReal, slowImaginary);
        _output.WriteLine($"real length {length,5} -> {plan.BinCount,5} bins: "
            + $"max |diff| {absolute:E3}, largest bin {scale:F4}, relative {relative:E3}");

        Assert.True(relative < 1e-13, $"real transform at length {length}: relative error {relative:E3}.");
    }

    /// <summary>
    /// The real path and the complex path must agree, because the real path is only the complex one
    /// with the packing trick. <b>An independent check on the untangling arithmetic specifically</b>,
    /// which is where a real-input transform goes wrong if it goes wrong at all.
    /// </summary>
    [Fact]
    public void TheRealPathAgreesWithTheComplexPathFedTheSameSignalWithZeroImaginaryParts()
    {
        const int length = 3840;
        var realPlan = new Ft8RealFft(length);
        var complexPlan = new Ft8Fft(length);

        var random = new Random(770);
        var samples = new double[length];
        var zeros = new double[length];
        for (var i = 0; i < length; i++)
        {
            samples[i] = (random.NextDouble() * 2) - 1;
        }

        var oneSidedReal = new double[realPlan.BinCount];
        var oneSidedImaginary = new double[realPlan.BinCount];
        realPlan.Transform(samples, oneSidedReal, oneSidedImaginary);

        var fullReal = new double[length];
        var fullImaginary = new double[length];
        complexPlan.Transform(samples, zeros, fullReal, fullImaginary);

        double worst = 0;
        double scale = 0;
        for (var k = 0; k < realPlan.BinCount; k++)
        {
            worst = Math.Max(worst, Math.Abs(oneSidedReal[k] - fullReal[k]));
            worst = Math.Max(worst, Math.Abs(oneSidedImaginary[k] - fullImaginary[k]));
            scale = Math.Max(scale, Math.Sqrt((fullReal[k] * fullReal[k]) + (fullImaginary[k] * fullImaginary[k])));
        }

        _output.WriteLine($"real path vs complex path over {realPlan.BinCount} bins: "
            + $"max |diff| {worst:E3}, largest bin {scale:F4}, relative {worst / scale:E3}");

        Assert.True(worst / scale < 1e-14, $"the two paths differ by {worst / scale:E3} of the largest bin.");
    }

    /// <summary>Linearity: the transform of a combination is the combination of the transforms.</summary>
    [Fact]
    public void TheTransformIsLinear()
    {
        const int length = 960;
        var plan = new Ft8Fft(length);
        var random = new Random(1234);

        double[] MakeArray()
        {
            var a = new double[length];
            for (var i = 0; i < length; i++)
            {
                a[i] = (random.NextDouble() * 2) - 1;
            }

            return a;
        }

        var xr = MakeArray();
        var xi = MakeArray();
        var yr = MakeArray();
        var yi = MakeArray();

        const double a = 2.75;
        const double b = -0.4;

        var combinedR = new double[length];
        var combinedI = new double[length];
        for (var i = 0; i < length; i++)
        {
            combinedR[i] = (a * xr[i]) + (b * yr[i]);
            combinedI[i] = (a * xi[i]) + (b * yi[i]);
        }

        var lhsR = new double[length];
        var lhsI = new double[length];
        plan.Transform(combinedR, combinedI, lhsR, lhsI);

        var xR = new double[length];
        var xI = new double[length];
        plan.Transform(xr, xi, xR, xI);
        var yR = new double[length];
        var yI = new double[length];
        plan.Transform(yr, yi, yR, yI);

        double worst = 0;
        double scale = 0;
        for (var k = 0; k < length; k++)
        {
            worst = Math.Max(worst, Math.Abs(lhsR[k] - ((a * xR[k]) + (b * yR[k]))));
            worst = Math.Max(worst, Math.Abs(lhsI[k] - ((a * xI[k]) + (b * yI[k]))));
            scale = Math.Max(scale, Math.Sqrt((lhsR[k] * lhsR[k]) + (lhsI[k] * lhsI[k])));
        }

        _output.WriteLine($"linearity over {length} bins: max |diff| {worst:E3}, largest bin {scale:F4}, relative {worst / scale:E3}");
        Assert.True(worst / scale < 1e-14, $"linearity broke by {worst / scale:E3} of the largest bin.");
    }

    /// <summary>
    /// Parseval: the energy in the samples is the energy in the bins divided by the length.
    /// <b>Both numbers are printed</b>, because a ratio alone hides which side moved.
    /// </summary>
    [Theory]
    [InlineData(1024)]
    [InlineData(1920)]
    [InlineData(3840)]
    public void EnergyIsConservedAndBothSidesArePrinted(int length)
    {
        var plan = new Ft8Fft(length);
        var random = new Random(555 + length);

        var real = new double[length];
        var imaginary = new double[length];
        double before = 0;
        for (var i = 0; i < length; i++)
        {
            real[i] = (random.NextDouble() * 2) - 1;
            imaginary[i] = (random.NextDouble() * 2) - 1;
            before += (real[i] * real[i]) + (imaginary[i] * imaginary[i]);
        }

        var outR = new double[length];
        var outI = new double[length];
        plan.Transform(real, imaginary, outR, outI);

        double after = 0;
        for (var k = 0; k < length; k++)
        {
            after += (outR[k] * outR[k]) + (outI[k] * outI[k]);
        }

        after /= length;

        var relative = Math.Abs(after - before) / before;
        _output.WriteLine($"length {length}: energy in the samples {before:F9}");
        _output.WriteLine($"length {length}: energy in the bins/N  {after:F9}");
        _output.WriteLine($"length {length}: relative difference   {relative:E3}");

        Assert.True(relative < 1e-14, $"energy moved by {relative:E3}, which Parseval does not allow.");
    }

    /// <summary>An impulse transforms to a flat spectrum, and the flatness is measured.</summary>
    [Fact]
    public void AnImpulseTransformsToAFlatSpectrum()
    {
        const int length = 3840;
        var plan = new Ft8Fft(length);

        var real = new double[length];
        var imaginary = new double[length];
        real[0] = 1.0;

        var outR = new double[length];
        var outI = new double[length];
        plan.Transform(real, imaginary, outR, outI);

        double worst = 0;
        for (var k = 0; k < length; k++)
        {
            var magnitude = Math.Sqrt((outR[k] * outR[k]) + (outI[k] * outI[k]));
            worst = Math.Max(worst, Math.Abs(magnitude - 1.0));
        }

        _output.WriteLine($"impulse at 0, length {length}: every bin should be magnitude 1");
        _output.WriteLine($"worst departure from flat: {worst:E3}");

        Assert.True(worst < 1e-15, $"the impulse spectrum is not flat; worst bin is off by {worst:E3}.");
    }

    /// <summary>
    /// A constant lands entirely in bin zero, and <i>entirely</i> is given as a measured ratio.
    /// </summary>
    [Fact]
    public void ADirectCurrentInputLandsEntirelyInBinZero()
    {
        const int length = 3840;
        const double level = 0.375;
        var plan = new Ft8Fft(length);

        var real = new double[length];
        var imaginary = new double[length];
        Array.Fill(real, level);

        var outR = new double[length];
        var outI = new double[length];
        plan.Transform(real, imaginary, outR, outI);

        var inBinZero = Math.Sqrt((outR[0] * outR[0]) + (outI[0] * outI[0]));
        double largestElsewhere = 0;
        var whereElsewhere = 0;
        for (var k = 1; k < length; k++)
        {
            var magnitude = Math.Sqrt((outR[k] * outR[k]) + (outI[k] * outI[k]));
            if (magnitude > largestElsewhere)
            {
                largestElsewhere = magnitude;
                whereElsewhere = k;
            }
        }

        _output.WriteLine($"bin 0 magnitude          : {inBinZero:F9} (expected {length * level:F9})");
        _output.WriteLine($"largest bin elsewhere    : {largestElsewhere:E3} at bin {whereElsewhere}");
        _output.WriteLine($"leakage ratio            : {largestElsewhere / inBinZero:E3}");

        Assert.True(Math.Abs(inBinZero - (length * level)) < 1e-10, "bin zero does not hold the sum of the input.");
        Assert.True(
            largestElsewhere / inBinZero < 1e-15,
            $"a constant leaked {largestElsewhere / inBinZero:E3} of itself outside bin zero.");
    }

    /// <summary>
    /// A sinusoid at exactly a bin centre puts its energy in that bin and its conjugate, and
    /// <b>"essentially nothing elsewhere" is a measured ratio rather than a word.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A measurement that surprised, chased down rather than absorbed.</b> Written first with the
    /// input as <c>Math.Cos(2*pi*bin*t/length)</c>, the leakage came out at 1.888E-14 of the peak at
    /// bin 320 and 1.029E-13 at bin 1000 — over a bound of 1e-14 that had been written before the
    /// measurement, which is the mistake this project has a rule against. Widening the bound would
    /// have been the wrong answer, because the numbers pointed somewhere specific: the two leakages
    /// differed by a factor of five for two bins whose ratio is about three, and the larger bin
    /// leaked more. That is not how a transform's rounding behaves — it is how an <em>input</em>
    /// behaves. At bin 1000 the last angle handed to <c>Math.Cos</c> is 2*pi*1000 ≈ 6283 radians,
    /// and argument reduction there costs about 6283 * 2.2e-16 ≈ 1.4e-12 in the sample. Spread over
    /// 3840 samples that is a spectral error of order 1e-10, which is what was measured. <b>The
    /// leakage was the test's own sinusoid, not the library's transform.</b>
    /// </para>
    /// <para>
    /// So the angle is reduced in exact integers before the trigonometry, exactly as
    /// <see cref="NaiveDft"/> does it and for the same reason, and the bound below is set from the
    /// measurement that followed.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(320)]
    [InlineData(1000)]
    public void ASinusoidAtABinCentreLandsInThatBinAndItsConjugate(int bin)
    {
        const int length = 3840;
        var plan = new Ft8Fft(length);

        var real = new double[length];
        var imaginary = new double[length];
        for (var t = 0; t < length; t++)
        {
            var turns = (long)bin * t % length;
            real[t] = Math.Cos(2.0 * Math.PI * turns / length);
        }

        var outR = new double[length];
        var outI = new double[length];
        plan.Transform(real, imaginary, outR, outI);

        double Magnitude(int k) => Math.Sqrt((outR[k] * outR[k]) + (outI[k] * outI[k]));

        var atBin = Magnitude(bin);
        var atConjugate = Magnitude(length - bin);

        double largestElsewhere = 0;
        var whereElsewhere = -1;
        for (var k = 0; k < length; k++)
        {
            if (k == bin || k == length - bin)
            {
                continue;
            }

            var magnitude = Magnitude(k);
            if (magnitude > largestElsewhere)
            {
                largestElsewhere = magnitude;
                whereElsewhere = k;
            }
        }

        _output.WriteLine($"cosine at bin {bin} of {length}:");
        _output.WriteLine($"  magnitude at bin {bin}          : {atBin:F6} (expected {length / 2.0:F6})");
        _output.WriteLine($"  magnitude at conjugate {length - bin} : {atConjugate:F6}");
        _output.WriteLine($"  largest elsewhere              : {largestElsewhere:E3} at bin {whereElsewhere}");
        _output.WriteLine($"  RATIO elsewhere / at bin       : {largestElsewhere / atBin:E3}");

        _output.WriteLine(string.Empty);
        _output.WriteLine("Before the input's angle was reduced in integers, this ratio measured");
        _output.WriteLine("1.888E-14 at bin 320 and 1.029E-13 at bin 1000 — the test's own Math.Cos,");
        _output.WriteLine("not the transform. The bound below is set from the numbers above it.");

        Assert.True(Math.Abs(atBin - (length / 2.0)) < 1e-9, "the bin does not hold half the length.");
        Assert.True(Math.Abs(atConjugate - (length / 2.0)) < 1e-9, "the conjugate bin does not match.");
        Assert.True(
            largestElsewhere / atBin < 1e-15,
            $"the sinusoid leaked {largestElsewhere / atBin:E3} of itself outside its own two bins.");
    }

    /// <summary>
    /// Determinism, bit for bit. <b>Step 4's third exit criterion is that candidate ranking is stable
    /// across runs, and this is the floor it rests on</b> — though nothing tonight ranks anything, and
    /// this result is not that criterion.
    /// </summary>
    [Fact]
    public void TheSameInputTwiceGivesBitIdenticalOutput()
    {
        const int length = 3840;
        var random = new Random(8888);
        var real = new double[length];
        var imaginary = new double[length];
        for (var i = 0; i < length; i++)
        {
            real[i] = (random.NextDouble() * 2) - 1;
            imaginary[i] = (random.NextDouble() * 2) - 1;
        }

        var reused = new Ft8Fft(length);
        var firstR = new double[length];
        var firstI = new double[length];
        reused.Transform(real, imaginary, firstR, firstI);

        var secondR = new double[length];
        var secondI = new double[length];
        reused.Transform(real, imaginary, secondR, secondI);

        // A second plan, freshly built, so this also catches a plan that had accumulated state.
        var fresh = new Ft8Fft(length);
        var thirdR = new double[length];
        var thirdI = new double[length];
        fresh.Transform(real, imaginary, thirdR, thirdI);

        var sameOnReuse = 0;
        var sameOnFreshPlan = 0;
        for (var k = 0; k < length; k++)
        {
            if (BitConverter.DoubleToInt64Bits(firstR[k]) == BitConverter.DoubleToInt64Bits(secondR[k])
                && BitConverter.DoubleToInt64Bits(firstI[k]) == BitConverter.DoubleToInt64Bits(secondI[k]))
            {
                sameOnReuse++;
            }

            if (BitConverter.DoubleToInt64Bits(firstR[k]) == BitConverter.DoubleToInt64Bits(thirdR[k])
                && BitConverter.DoubleToInt64Bits(firstI[k]) == BitConverter.DoubleToInt64Bits(thirdI[k]))
            {
                sameOnFreshPlan++;
            }
        }

        _output.WriteLine($"bins bit-identical on a reused plan : {sameOnReuse} of {length}");
        _output.WriteLine($"bins bit-identical on a fresh plan  : {sameOnFreshPlan} of {length}");

        Assert.Equal(length, sameOnReuse);
        Assert.Equal(length, sameOnFreshPlan);
    }

    /// <summary>
    /// For a power-of-two length every stage is a radix-2 butterfly, which is the instruction's
    /// radix-2 Cooley–Tukey exactly; for the length the monitor wants it is not, and that is the
    /// finding task 2 made.
    /// </summary>
    [Fact]
    public void APowerOfTwoLengthIsPureRadix2AndTheMonitorsLengthIsNot()
    {
        foreach (var length in new[] { 2, 4, 8, 16, 256, 1024, 4096 })
        {
            var plan = new Ft8Fft(length);
            _output.WriteLine($"{length,5} -> {string.Join("x", plan.Factors),-28} pure radix-2 {plan.IsPureRadix2}");
            Assert.True(plan.IsPureRadix2, $"length {length} is a power of two but did not factor into twos.");
        }

        var monitor = new Ft8Fft(3840);
        _output.WriteLine($"{3840,5} -> {string.Join("x", monitor.Factors),-28} pure radix-2 {monitor.IsPureRadix2}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("3840 is the length upstream's monitor transforms at 12 kHz, and it is not a");
        _output.WriteLine("power of two. A radix-2 transform alone could not compute it, which is why");
        _output.WriteLine("this one is the general decomposition with radix-2 as its special case.");

        Assert.False(monitor.IsPureRadix2);
        Assert.Equal(new[] { 2, 2, 2, 2, 2, 2, 2, 2, 3, 5 }, monitor.Factors);
    }

    // ---- Refusals. Each is watched refusing, and each is checked for leaving nothing behind. ----

    /// <summary>A transform length of zero or less is refused rather than computed.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-3840)]
    public void ALengthOfZeroOrLessIsRefused(int length)
    {
        var refusal = Assert.Throws<ArgumentOutOfRangeException>(() => new Ft8Fft(length));
        _output.WriteLine($"length {length} refused: {refusal.Message.Split('(')[0].Trim()}");
        Assert.Contains("at least one", refusal.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Each of the four spans is refused when it is the wrong length, and each is watched separately
    /// — one test covering all four passes when three are checked.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AMismatchedBufferIsRefusedAndNothingIsWritten(int whichSpanIsWrong)
    {
        const int length = 64;
        var plan = new Ft8Fft(length);

        var spans = new double[4][];
        for (var i = 0; i < 4; i++)
        {
            spans[i] = new double[length];
        }

        spans[whichSpanIsWrong] = new double[length - 1];

        // Fill the two output buffers with a sentinel, so "nothing was written" is checked rather
        // than assumed. A guard that refused after scribbling is not a guard.
        var sentinel = -12345.678;
        if (spans[2].Length == length)
        {
            Array.Fill(spans[2], sentinel);
        }

        if (spans[3].Length == length)
        {
            Array.Fill(spans[3], sentinel);
        }

        var refusal = Assert.Throws<ArgumentException>(
            () => plan.Transform(spans[0], spans[1], spans[2], spans[3]));

        _output.WriteLine($"span {whichSpanIsWrong} wrong: {refusal.Message.Split('.')[0]}.");

        var untouched = 0;
        foreach (var index in new[] { 2, 3 })
        {
            if (spans[index].Length != length)
            {
                continue;
            }

            foreach (var value in spans[index])
            {
                if (value == sentinel)
                {
                    untouched++;
                }
            }
        }

        var expected = (spans[2].Length == length ? length : 0) + (spans[3].Length == length ? length : 0);
        _output.WriteLine($"output values still holding the sentinel: {untouched} of {expected}");
        Assert.Equal(expected, untouched);
    }

    /// <summary>A real transform length that is odd, or below two, is refused.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-2)]
    [InlineData(3)]
    [InlineData(1921)]
    [InlineData(3839)]
    public void ARealTransformLengthThatCannotBePackedIntoPairsIsRefused(int length)
    {
        var refusal = Assert.Throws<ArgumentOutOfRangeException>(() => new Ft8RealFft(length));
        _output.WriteLine($"real length {length} refused: {refusal.Message.Split('(')[0].Trim()}");
    }

    /// <summary>
    /// The real path's three spans, each watched refusing, and the outputs checked for having been
    /// left alone.
    /// </summary>
    [Theory]
    [InlineData("samples")]
    [InlineData("binReal")]
    [InlineData("binImaginary")]
    public void AMismatchedRealBufferIsRefusedAndNothingIsWritten(string wrongOne)
    {
        const int length = 64;
        var plan = new Ft8RealFft(length);

        var samples = new double[wrongOne == "samples" ? length - 1 : length];
        var binReal = new double[wrongOne == "binReal" ? plan.BinCount - 1 : plan.BinCount];
        var binImaginary = new double[wrongOne == "binImaginary" ? plan.BinCount + 1 : plan.BinCount];

        const double sentinel = 9999.5;
        Array.Fill(binReal, sentinel);
        Array.Fill(binImaginary, sentinel);

        var refusal = Assert.Throws<ArgumentException>(() => plan.Transform(samples, binReal, binImaginary));
        _output.WriteLine($"{wrongOne} wrong: {refusal.Message.Split('.')[0]}.");
        Assert.Equal(wrongOne, refusal.ParamName);

        var untouched = binReal.Count(v => v == sentinel) + binImaginary.Count(v => v == sentinel);
        _output.WriteLine($"output values still holding the sentinel: {untouched} of {binReal.Length + binImaginary.Length}");
        Assert.Equal(binReal.Length + binImaginary.Length, untouched);
    }

    /// <summary>
    /// Returns the largest absolute difference, the largest bin magnitude on the reference side, and
    /// their ratio.
    /// </summary>
    private static (double Absolute, double Scale, double Relative) Compare(
        IReadOnlyList<double> fastReal,
        IReadOnlyList<double> fastImaginary,
        IReadOnlyList<double> slowReal,
        IReadOnlyList<double> slowImaginary)
    {
        double absolute = 0;
        double scale = 0;

        for (var k = 0; k < fastReal.Count; k++)
        {
            absolute = Math.Max(absolute, Math.Abs(fastReal[k] - slowReal[k]));
            absolute = Math.Max(absolute, Math.Abs(fastImaginary[k] - slowImaginary[k]));
            scale = Math.Max(
                scale,
                Math.Sqrt((slowReal[k] * slowReal[k]) + (slowImaginary[k] * slowImaginary[k])));
        }

        // A transform of all zeros has no scale; report the absolute difference as its own relative.
        return scale > 0 ? (absolute, scale, absolute / scale) : (absolute, 0, absolute);
    }
}
