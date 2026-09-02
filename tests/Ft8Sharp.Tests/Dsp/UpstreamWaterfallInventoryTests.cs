using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The sanctioned read of the pinned clone for unit 213: what upstream's monitor does to turn audio
/// into a waterfall, and under whose copyright the FFT it uses to do it is published.
/// </summary>
/// <remarks>
/// <para>
/// <b>Read it before porting it, and leave behind something that fails loudly if a re-pin changes
/// it.</b> Every shape asserted here is a shape the spectrogram in <c>src/Ft8Sharp/Dsp/</c> was
/// written against. If upstream's receive front end is ever re-pinned to a different one, this goes
/// red beside the waterfall rather than the waterfall drifting quietly.
/// </para>
/// <para>
/// <b>Shapes and counts, never values.</b> The same discipline as
/// <see cref="Ft8Sharp.Tests.Encode.UpstreamSynthesisInventoryTests"/>: identifiers, presences,
/// counts and structural facts are printed; nothing from the clone is committed. The protocol's
/// published facts — the 0.160 s symbol period, the 15 s slot, 6.25 Hz tone spacing — come from the
/// QEX paper the NOTICE already cites and are free.
/// </para>
/// <para>
/// <b>The vendored FFT is not read.</b> The pin carries a third party's transform in its own folder.
/// <see cref="TheVendoredFftsCopyrightAndLicenceAreOnTheRecord"/> reads that folder's licence header
/// and its copyright line and <em>nothing else in it</em>, because the answer to <i>whose licence is
/// it</i> is the record that unit 213's decision to write its own FFT from the mathematics stands on
/// a measurement rather than on an assumption. No structure, no algorithm and no line of it was read.
/// </para>
/// <para>
/// <b>Strong and weak anchoring is reported, not blurred.</b> A shape that is a macro in a header
/// cannot be misread; a shape that is an expression inside a function body can be, and every unit of
/// this phase from 209 onward has been required to say which it had. The split is printed by
/// <see cref="TheAnchoringOfEachGeometryShapeIsReported"/>.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone stays green.</para>
/// </remarks>
public class UpstreamWaterfallInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamWaterfallInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 213 is licensed to read for shapes, and no others.</summary>
    private static readonly string[] ReceiveSources =
    {
        @"common\monitor.c", @"common\monitor.h", @"ft8\decode.h", @"ft8\constants.h",
        @"demo\decode_ft8.c",
    };

    /// <summary>
    /// Discovery, and it runs because assuming which file holds the monitor is exactly the mistake
    /// this project has paid for before. Task 2 says <i>find the files first and say which they
    /// are</i>.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheReceivePathsFilesAreFoundRatherThanAssumed()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone: {location}");

        foreach (var folder in new[] { "ft8", "common", "fft", "demo" })
        {
            var full = Path.Combine(location, folder);
            Assert.True(Directory.Exists(full), $"the pin no longer has a {folder}/ folder.");

            _output.WriteLine($"[{folder}]");
            foreach (var entry in Directory.EnumerateFileSystemEntries(full).OrderBy(e => e))
            {
                var name = Path.GetFileName(entry);
                var mark = Directory.Exists(entry) ? "<dir>" : $"{new FileInfo(entry).Length} bytes";
                _output.WriteLine($"    {name,-24} {mark}");
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("The receive front end is common/monitor.{c,h}. The waterfall structure and");
        _output.WriteLine("its element type are in ft8/decode.h. The monitor's configuration values are");
        _output.WriteLine("chosen by the application in demo/decode_ft8.c. The FFT is a third party's,");
        _output.WriteLine("vendored whole in fft/, and only its licence header is read.");

        foreach (var relative in ReceiveSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"the pin no longer holds {relative}; the read has no source.");
        }
    }

    /// <summary>
    /// Task 2 question 1 — the transform. Size, input rate, advance between blocks, and whether it
    /// is real-input or complex.
    /// </summary>
    /// <remarks>
    /// <b>The size is the finding.</b> Upstream's transform length is the samples in one symbol
    /// multiplied by the frequency oversampling factor. At the rate FT8 is decoded at that is 3840,
    /// and <b>3840 is not a power of two</b> — it is 2^8 × 3 × 5. The instruction for this unit
    /// expected a power-of-two size and said so; it is wrong about the tree, and the mismatch is
    /// reported rather than repaired. A radix-2 transform alone cannot compute this length, which is
    /// why <c>Ft8Fft</c> is a general mixed-radix Cooley–Tukey with a radix-2 core.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheTransformsSizeRateAndAdvanceAreRead()
    {
        var monitor = ReadSource(@"common\monitor.c");
        var init = ExtractFunctionBody(monitor, "monitor_init");
        var process = ExtractFunctionBody(monitor, "monitor_process");
        var app = ReadSource(@"demo\decode_ft8.c");

        Shows(init, @"block_size\s*=\s*\(int\)\(\s*cfg->sample_rate\s*\*\s*symbol_period\s*\)",
            "the block is the samples in one symbol at the configured rate",
            "the block size is what one waterfall row covers in time");
        Shows(init, @"subblock_size\s*=\s*me->block_size\s*/\s*cfg->time_osr",
            "the analysis advance is the block divided by the time oversampling factor",
            "the advance is what decides how many transforms a symbol costs");
        Shows(init, @"nfft\s*=\s*me->block_size\s*\*\s*cfg->freq_osr",
            "the transform length is the block times the frequency oversampling factor",
            "this is the length the port's FFT must be able to compute");
        Shows(process, @"kiss_fftr\s*\(",
            "the transform is the REAL-input entry point, not the complex one",
            "a real-input transform returns nfft/2+1 bins and a complex one returns nfft");
        Shows(process, @"freqdata\s*\[\s*me->nfft\s*/\s*2\s*\+\s*1\s*\]",
            "the output buffer is nfft/2+1 complex bins",
            "that count is the one-sided spectrum and the port must produce the same extent");
        Shows(app, @"sample_rate\s*=\s*12000",
            "the application decodes at 12000 Hz",
            "the geometry below is only arithmetic once the rate is known");

        // Computed here from the shapes above and the protocol's published symbol period, so the
        // numbers are derived rather than transcribed out of the pin.
        const int rate = 12000;
        const double symbolPeriod = 0.160;
        const int timeOsr = 2;
        const int freqOsr = 2;
        var blockSize = (int)(rate * symbolPeriod);
        var subblock = blockSize / timeOsr;
        var nfft = blockSize * freqOsr;

        _output.WriteLine($"sample rate         : {rate} Hz");
        _output.WriteLine($"block size          : {blockSize} samples (one symbol)");
        _output.WriteLine($"advance per transform: {subblock} samples (block / time_osr)");
        _output.WriteLine($"transform length    : {nfft} samples (block * freq_osr)");
        _output.WriteLine($"transform kind      : REAL input, {nfft / 2 + 1} complex bins out");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"THE FINDING: {nfft} is NOT a power of two. {nfft} = {Factorise(nfft)}.");
        _output.WriteLine("A radix-2 Cooley-Tukey cannot compute this length. Ft8Fft is therefore a");
        _output.WriteLine("general mixed-radix Cooley-Tukey with a radix-2 core, which is the same");
        _output.WriteLine("textbook decomposition and is still written from the mathematics.");

        Assert.Equal(1920, blockSize);
        Assert.Equal(960, subblock);
        Assert.Equal(3840, nfft);
        Assert.False(IsPowerOfTwo(nfft), "3840 was a power of two after all; the port's premise changes.");
    }

    /// <summary>Task 2 question 2 — the window, its length, and how it is normalised.</summary>
    [RequiresReferenceCloneFact]
    public void TheWindowAndItsNormalisationAreRead()
    {
        var monitor = ReadSource(@"common\monitor.c");
        var init = ExtractFunctionBody(monitor, "monitor_init");

        Shows(monitor, @"static\s+float\s+hann_i\s*\(",
            "the window function is a Hann window",
            "the window decides the leakage between neighbouring bins");
        Shows(monitor, @"hann_i[\s\S]{0,200}?sinf\s*\([^;]*?M_PI\s*\*\s*i\s*/\s*N\s*\)\s*;[\s\S]{0,80}?x\s*\*\s*x",
            "the Hann window is written as the SQUARE OF A SINE, sin(pi i / N) squared",
            "sin^2 and the 0.5-0.5cos form agree exactly, but the pin's arithmetic is the sine one");
        Shows(monitor, @"//\s*static\s+float\s+hamming_i",
            "a Hamming window is present but COMMENTED OUT",
            "an alternative left commented is not the window in force and must not be ported as one");
        Shows(monitor, @"//\s*static\s+float\s+blackman_i",
            "a Blackman window is present but COMMENTED OUT",
            "same reason");
        Shows(init, @"window\s*=\s*\(float\*\)malloc\s*\(\s*me->nfft\s*\*",
            "the window is nfft samples long, the whole transform",
            "a window shorter than the transform would be a different analysis");
        Shows(init, @"//\s*const\s+int\s+len_window\s*=\s*1\.8f\s*\*\s*me->block_size",
            "a shorter hand-picked window is present but COMMENTED OUT",
            "so the window in force spans the whole transform and is not the 1.8-block one");
        Shows(init, @"fft_norm\s*=\s*2\.0f\s*/\s*me->nfft",
            "the normalisation factor is two divided by the transform length",
            "this is the scale that makes a full-scale sinusoid read as its own amplitude");
        Shows(init, @"me->window\s*\[\s*i\s*\]\s*=\s*me->fft_norm\s*\*\s*hann_i\s*\(\s*i\s*,\s*me->nfft\s*\)",
            "THE NORMALISATION IS FOLDED INTO THE WINDOW, not applied to the transform output",
            "this is the answer to 'is there a scale factor applied to the output' and it is NO — "
            + "the samples are scaled going in, which is arithmetically different in the last place "
            + "from scaling the bins coming out");

        _output.WriteLine(string.Empty);
        _output.WriteLine("window          : Hann, sin(pi i / N) squared, over all nfft samples");
        _output.WriteLine("normalisation   : 2 / nfft, MULTIPLIED INTO THE WINDOW COEFFICIENTS");
        _output.WriteLine("                  and therefore into the time samples, not into the bins");
        _output.WriteLine("computed from   : the transform length alone; it does not see the signal");
    }

    /// <summary>
    /// Task 2 question 3 — the oversampling. How many time offsets, how many frequency offsets, and
    /// how the extra ones are produced.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheOversamplingFactorsAndHowTheyAreProducedAreRead()
    {
        var monitor = ReadSource(@"common\monitor.c");
        var process = ExtractFunctionBody(monitor, "monitor_process");
        var app = ReadSource(@"demo\decode_ft8.c");

        Shows(app, @"const\s+int\s+kTime_osr\s*=\s*2\s*;",
            "the time oversampling factor is 2",
            "each symbol is analysed at two time offsets");
        Shows(app, @"const\s+int\s+kFreq_osr\s*=\s*2\s*;",
            "the frequency oversampling factor is 2",
            "each 6.25 Hz tone slot is analysed at two frequency offsets");

        Shows(process, @"for\s*\(\s*int\s+time_sub\s*=\s*0\s*;\s*time_sub\s*<\s*me->wf\.time_osr",
            "the time offsets are a loop over time_sub inside one block",
            "so a block is not one transform, it is time_osr of them");
        Shows(process, @"me->last_frame\s*\[\s*pos\s*\]\s*=\s*me->last_frame\s*\[\s*pos\s*\+\s*me->subblock_size\s*\]",
            "THE EXTRA TIME OFFSETS COME FROM SHIFTING THE INPUT, by subblock_size samples",
            "a sliding analysis frame, not a second transform of the same samples");
        Shows(process, @"int\s+src_bin\s*=\s*\(\s*bin\s*\*\s*me->wf\.freq_osr\s*\)\s*\+\s*freq_sub",
            "THE EXTRA FREQUENCY OFFSETS COME FROM THE TRANSFORM BEING freq_osr TIMES LONGER, "
            + "and are read out by striding the bins",
            "nfft = block_size * freq_osr is what buys the finer bins; there is no zero padding "
            + "and no second finer transform");

        _output.WriteLine(string.Empty);
        _output.WriteLine("time_osr  : 2 — produced by SHIFTING THE INPUT FRAME by block/time_osr samples");
        _output.WriteLine("            between transforms, so 2 transforms are run per symbol block");
        _output.WriteLine("freq_osr  : 2 — produced by making the TRANSFORM ITSELF freq_osr times longer");
        _output.WriteLine("            than the symbol, so bins are 1/freq_osr of a tone spacing apart;");
        _output.WriteLine("            the two sub-offsets are then read out by stride, src_bin =");
        _output.WriteLine("            bin * freq_osr + freq_sub");
        _output.WriteLine("NOT by     : zero padding, and not by a separate finer transform.");
    }

    /// <summary>
    /// Task 2 question 4 — the waterfall's storage. Element type, logarithm base and scale,
    /// normalisation, extents and axis order.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheWaterfallsStorageAndAxisOrderAreRead()
    {
        var decode = ReadSource(@"ft8\decode.h");
        var monitor = ReadSource(@"common\monitor.c");
        var process = ExtractFunctionBody(monitor, "monitor_process");

        Shows(decode, @"^\s*//\s*#define\s+WATERFALL_USE_PHASE\s*$",
            "the phase-carrying waterfall is COMPILED OUT — the define is commented",
            "the element type depends on it, so which branch is live decides everything below");
        Shows(decode, @"#define\s+WF_ELEM_T\s+uint8_t",
            "the live element type is an UNSIGNED 8-BIT INTEGER, one byte per magnitude",
            "not a float; the port stores bytes or it is not storing what upstream stores");
        Shows(decode, @"#define\s+WF_ELEM_MAG\(x\)\s*\(\(float\)\(x\)\s*\*\s*0\.5f\s*-\s*120\.0f\)",
            "a stored byte reads back as half its value minus 120",
            "that is the inverse of the storage scale and it fixes the step at half a decibel");
        Shows(process, @"float\s+db\s*=\s*10\.0f\s*\*\s*log10f\s*\(\s*1E-12f\s*\+\s*mag2\s*\)",
            "the magnitude is a LOGARITHM, base ten, times ten — decibels of POWER",
            "and a floor of 1e-12 is added inside the logarithm so a silent bin is finite");
        Shows(process, @"mag2\s*=\s*\(\s*freqdata\s*\[\s*src_bin\s*\]\.i\s*\*[\s\S]{0,80}?\.r\s*\*",
            "the quantity logged is the SQUARED magnitude, imaginary part first",
            "10*log10 of a squared magnitude is 20*log10 of a magnitude; the port must not do both");
        Shows(process, @"int\s+scaled\s*=\s*\(int\)\s*\(\s*2\s*\*\s*db\s*\+\s*240\s*\)",
            "decibels become a byte as twice the decibels plus 240",
            "half-decibel steps over a 120 dB span, and the cast truncates toward zero");
        Shows(process, @"me->wf\.mag\s*\[\s*offset\s*\]\s*=\s*\(\s*scaled\s*<\s*0\s*\)\s*\?\s*0\s*:\s*\(\s*\(\s*scaled\s*>\s*255\s*\)\s*\?\s*255\s*:\s*scaled\s*\)",
            "the byte is CLAMPED to 0..255 rather than wrapping",
            "a clamp and a wrap differ by 256 at the ends, which is the whole dynamic range");
        Shows(decode, @"uint8_t\s*\[\s*blocks\s*\]\s*\[\s*time_osr\s*\]\s*\[\s*freq_osr\s*\]\s*\[\s*num_bins\s*\]",
            "the axis order is documented as [blocks][time_osr][freq_osr][num_bins]",
            "the next unit's correlator indexes this array directly and a transposed port is silent");
        Shows(monitor, @"block_stride\s*=\s*\(\s*time_osr\s*\*\s*freq_osr\s*\*\s*num_bins\s*\)",
            "the stride from one block to the next is time_osr * freq_osr * num_bins",
            "which is the same statement as the axis order, made arithmetically");

        var noNormalisation = !Regex.IsMatch(process, @"max_mag[\s\S]{0,200}?wf\.mag\s*\[")
            && !Regex.IsMatch(process, @"/\s*(me->max_mag|block_max|slot_max)");
        _output.WriteLine($"    {(noNormalisation ? "yes" : "NO "),-4} nothing divides a stored magnitude by any maximum");
        Assert.True(noNormalisation, "a normalisation appeared in monitor_process; the storage is no longer absolute.");

        Shows(process, @"if\s*\(\s*db\s*>\s*me->max_mag\s*\)",
            "max_mag is tracked but is only a running maximum",
            "it is written after the byte is stored and never divides anything — it is debug stats, "
            + "which is what makes the storage ABSOLUTE rather than per-block or per-slot normalised");

        _output.WriteLine(string.Empty);
        _output.WriteLine("element type    : uint8_t, one byte");
        _output.WriteLine("logarithm       : yes — 10 * log10(1e-12 + |X|^2), i.e. dB of power");
        _output.WriteLine("storage scale   : byte = clamp(0, 255, (int)(2 * dB + 240)), 0.5 dB per count,");
        _output.WriteLine("                  covering -120 dB .. +7.5 dB");
        _output.WriteLine("normalisation   : NONE — not per block, not per slot. Absolute.");
        _output.WriteLine("axis order      : [block][time_sub][freq_sub][bin], bin varying fastest");
        _output.WriteLine("block stride    : time_osr * freq_osr * num_bins");
    }

    /// <summary>
    /// Task 2 question 5 — the strong/weak anchoring split, as units 209 and 211 reported it. A
    /// macro in a header cannot be misread; an expression inside a function body can.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheAnchoringOfEachGeometryShapeIsReported()
    {
        var constants = ReadSource(@"ft8\constants.h");
        var decode = ReadSource(@"ft8\decode.h");
        var monitor = ReadSource(@"common\monitor.c");
        var app = ReadSource(@"demo\decode_ft8.c");

        var strong = 0;
        var weak = 0;

        void Strong(string source, string pattern, string what)
        {
            Assert.True(Regex.IsMatch(source, pattern), $"the strong anchoring for {what} is gone.");
            _output.WriteLine($"    STRONG  {what}");
            strong++;
        }

        void Weak(string source, string pattern, string what)
        {
            Assert.True(Regex.IsMatch(source, pattern), $"the weak anchoring for {what} is gone.");
            _output.WriteLine($"    weak    {what}");
            weak++;
        }

        _output.WriteLine("STRONG — a macro or a typedef in a header. Cannot be misread.");
        Strong(constants, @"#define\s+FT8_SYMBOL_PERIOD\s+\(0\.160f\)", "the symbol period, a macro in ft8/constants.h");
        Strong(constants, @"#define\s+FT8_SLOT_TIME\s+\(15\.0f\)", "the slot duration, a macro in ft8/constants.h");
        Strong(decode, @"#define\s+WF_ELEM_T\s+uint8_t", "the waterfall element type, a macro in ft8/decode.h");
        Strong(decode, @"#define\s+WF_ELEM_MAG\(x\)", "the byte-to-decibel reading, a macro in ft8/decode.h");
        Strong(decode, @"typedef\s+struct[\s\S]{0,900}?\}\s*ftx_waterfall_t", "the waterfall structure and its fields, a typedef in ft8/decode.h");
        Strong(decode, @"uint8_t\s*\[\s*blocks\s*\]\s*\[\s*time_osr\s*\]\s*\[\s*freq_osr\s*\]\s*\[\s*num_bins\s*\]", "the axis order, documented on the field in ft8/decode.h");

        _output.WriteLine(string.Empty);
        _output.WriteLine("WEAK — an expression inside a function body, or a value chosen by the");
        _output.WriteLine("application rather than declared by the library. These are the ones that");
        _output.WriteLine("can be misread, and the port names each of them where it uses them.");
        Weak(monitor, @"block_size\s*=\s*\(int\)\(\s*cfg->sample_rate\s*\*\s*symbol_period", "block size, an expression in monitor_init");
        Weak(monitor, @"subblock_size\s*=\s*me->block_size\s*/\s*cfg->time_osr", "the analysis advance, an expression in monitor_init");
        Weak(monitor, @"nfft\s*=\s*me->block_size\s*\*\s*cfg->freq_osr", "the transform length, an expression in monitor_init");
        Weak(monitor, @"fft_norm\s*=\s*2\.0f\s*/\s*me->nfft", "the normalisation factor, an expression in monitor_init");
        Weak(monitor, @"hann_i\s*\(\s*i\s*,\s*me->nfft\s*\)", "the window and its length, an expression in monitor_init");
        Weak(monitor, @"max_blocks\s*=\s*\(int\)\(\s*slot_time\s*/\s*symbol_period\s*\)", "the block count, an expression in monitor_init");
        Weak(monitor, @"min_bin\s*=\s*\(int\)\(\s*cfg->f_min\s*\*\s*symbol_period\s*\)", "the first kept bin, an expression in monitor_init");
        Weak(monitor, @"max_bin\s*=\s*\(int\)\(\s*cfg->f_max\s*\*\s*symbol_period\s*\)\s*\+\s*1", "the last kept bin, an expression in monitor_init");
        Weak(monitor, @"10\.0f\s*\*\s*log10f", "the decibel conversion, an expression in monitor_process");
        Weak(monitor, @"\(int\)\s*\(\s*2\s*\*\s*db\s*\+\s*240\s*\)", "the byte scaling, an expression in monitor_process");
        Weak(monitor, @"src_bin\s*=\s*\(\s*bin\s*\*\s*me->wf\.freq_osr\s*\)\s*\+\s*freq_sub", "the frequency sub-offset stride, an expression in monitor_process");
        Weak(app, @"const\s+int\s+kTime_osr\s*=\s*2", "time_osr = 2, a file-scope constant in the APPLICATION, not the library");
        Weak(app, @"const\s+int\s+kFreq_osr\s*=\s*2", "freq_osr = 2, a file-scope constant in the APPLICATION, not the library");
        Weak(app, @"\.f_min\s*=\s*200", "f_min = 200 Hz, a designated initialiser inside main()");
        Weak(app, @"\.f_max\s*=\s*3000", "f_max = 3000 Hz, a designated initialiser inside main()");

        _output.WriteLine(string.Empty);
        _output.WriteLine($"anchoring split : {strong} strong, {weak} weak, {strong + weak} shapes read, 0 unread");
        _output.WriteLine("The four passband and oversampling values are the WEAKEST of all: they are");
        _output.WriteLine("not the library's at all, they are one application's choices. A different");
        _output.WriteLine("caller of monitor_init would get a different waterfall from the same code.");

        Assert.Equal(6, strong);
        Assert.Equal(15, weak);
    }

    /// <summary>
    /// Task 2 question 6 — the licence question, and the one that must not be skipped.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Only the licence header is read.</b> This test opens the vendored FFT's public header and
    /// looks at its first lines for a copyright holder and a licence identifier. It reads no
    /// algorithm, no structure and nothing below the comment block. That restriction is the point:
    /// the arbiter's decision to write this library's FFT from the mathematics rather than port the
    /// vendored one has to stand on a measurement of whose licence it is, and measuring that must
    /// not become a reason to read the code.
    /// </para>
    /// <para>
    /// <b>What the finding means.</b> <c>Ft8Sharp</c> carries one LICENSE — Tim's MIT — and a NOTICE
    /// crediting Goba. A second copyright holder under a separate licence would add that holder's
    /// obligations to a library headed for publication, and what may be published is the owner's
    /// under ARBITER.md §6, not a unit's.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheVendoredFftsCopyrightAndLicenceAreOnTheRecord()
    {
        var location = RequireReachableClone();

        var folder = Path.Combine(location, "fft");
        Assert.True(Directory.Exists(folder), "the pin no longer vendors an FFT in fft/.");

        var names = Directory.EnumerateFiles(folder).Select(Path.GetFileName).OrderBy(n => n).ToList();
        _output.WriteLine($"vendored FFT folder : fft/  ({names.Count} files)");
        foreach (var name in names)
        {
            _output.WriteLine($"    {name}");
        }

        // The header only, and only its leading comment block. Nothing below it is read.
        var header = Path.Combine(folder, "kiss_fft.h");
        Assert.True(File.Exists(header), "fft/kiss_fft.h is gone; the licence cannot be read.");

        var leading = new StringBuilder();
        foreach (var line in File.ReadLines(header))
        {
            if (line.Contains("#ifndef", StringComparison.Ordinal)
                || line.Contains("#define", StringComparison.Ordinal)
                || line.Contains("#include", StringComparison.Ordinal))
            {
                break;
            }

            leading.AppendLine(line.TrimEnd('\r'));
        }

        var text = leading.ToString();
        _output.WriteLine(string.Empty);
        _output.WriteLine("Leading comment block of fft/kiss_fft.h, and nothing else in the folder:");
        _output.WriteLine(text);

        var copyright = Regex.Match(text, @"Copyright[^\n]*");
        var spdx = Regex.Match(text, @"SPDX-License-Identifier:\s*(\S+)");
        var project = Regex.Match(text, @"part of\s+([^\n]*)");

        Assert.True(copyright.Success, "the vendored FFT states no copyright holder; the finding cannot be made.");
        Assert.True(spdx.Success, "the vendored FFT states no SPDX licence identifier.");

        _output.WriteLine($"project           : {(project.Success ? project.Groups[1].Value.Trim() : "unstated")}");
        _output.WriteLine($"copyright         : {copyright.Value.Trim()}");
        _output.WriteLine($"licence           : {spdx.Groups[1].Value}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE DECISION THIS RECORDS: the vendored FFT is a SECOND COPYRIGHT HOLDER");
        _output.WriteLine("under a SEPARATE LICENCE from the one Ft8Sharp carries. Ft8Sharp has one");
        _output.WriteLine("LICENSE, Tim's MIT, and a NOTICE crediting Goba. Adding this would add a");
        _output.WriteLine("third party's obligations to a library headed for publication, which is");
        _output.WriteLine("owner-class under ARBITER.md section 6 and is not authored around by a unit.");
        _output.WriteLine("So src/Ft8Sharp/Dsp/Ft8Fft.cs is written from the mathematics. Nothing in");
        _output.WriteLine("this folder was read beyond the comment block printed above.");

        Assert.Contains("Borgerding", copyright.Value, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("BSD-3-Clause", spdx.Groups[1].Value);
    }

    /// <summary>
    /// Everything task 2 could NOT answer, said out loud. A shape guessed at is worse than a shape
    /// reported as unread, because the next unit builds a correlator on it.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void WhatCouldNotBeReadIsNamed()
    {
        RequireReachableClone();

        _output.WriteLine("Unanswered by task 2, and named rather than guessed:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("1. Nothing upstream EMITS a spectrum, a waterfall or a candidate list, so");
        _output.WriteLine("   no number in this file was checked against upstream's own output. Every");
        _output.WriteLine("   shape here is read from source. decode_ft8.exe is not built on this");
        _output.WriteLine("   machine and a unit may not build one.");
        _output.WriteLine("2. The first block's analysis frame is prefilled with zeros by calloc, so");
        _output.WriteLine("   the earliest blocks see a partly empty window. Upstream's own resynth");
        _output.WriteLine("   comment calls this a 3-subblock loading offset. WHAT THE EXACT ALIGNMENT");
        _output.WriteLine("   BETWEEN A BLOCK INDEX AND A SAMPLE OFFSET IS was NOT settled by reading;");
        _output.WriteLine("   the port reproduces the same prefill and the same shift, so it inherits");
        _output.WriteLine("   whatever alignment upstream has, but it is not asserted as a number.");
        _output.WriteLine("3. The rounding of the float-to-byte cast is C's truncation toward zero on");
        _output.WriteLine("   a value that is always non-negative after the clamp comparison, but the");
        _output.WriteLine("   clamp is applied to the SIGNED int before the cast to the byte, so a");
        _output.WriteLine("   negative db truncates toward zero first. The port does the same in the");
        _output.WriteLine("   same order; it was not independently verified against a running monitor.");
    }

    /// <summary>
    /// Prints the receive-path source so it can be read for shapes, and only when explicitly asked
    /// to. Off by default and keyed on its own variable, exactly as unit 212's dump is. Nothing it
    /// prints reaches a committed file, and it will not open the vendored FFT.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void EmitReceiveSourceForReading()
    {
        if (Environment.GetEnvironmentVariable("FT8_RX_SOURCE_DUMP") != "1")
        {
            _output.WriteLine("Not asked. Set FT8_RX_SOURCE_DUMP=1 to emit the source for reading.");
            return;
        }

        var location = RequireReachableClone();
        var only = Environment.GetEnvironmentVariable("FT8_RX_SOURCE_FILE");
        if (only is not { Length: > 0 })
        {
            _output.WriteLine("Set FT8_RX_SOURCE_FILE to one of: " + string.Join(", ", ReceiveSources));
            return;
        }

        // The vendored FFT is out of bounds for this route by construction, not by good intentions.
        if (!ReceiveSources.Contains(only, StringComparer.OrdinalIgnoreCase))
        {
            _output.WriteLine(
                $"REFUSED: {only} is not one of unit 213's licensed reads. The vendored FFT in fft/ "
                + "is read for its licence header only, by "
                + nameof(TheVendoredFftsCopyrightAndLicenceAreOnTheRecord) + ".");
            return;
        }

        var path = Path.Combine(location, only);
        if (!File.Exists(path))
        {
            _output.WriteLine($"ABSENT: {path}");
            return;
        }

        var lines = File.ReadAllText(path).Split('\n');
        _output.WriteLine($"===== {only} : {lines.Length} lines =====");

        var from = int.TryParse(Environment.GetEnvironmentVariable("FT8_RX_FROM"), out var f) ? f : 1;
        var to = int.TryParse(Environment.GetEnvironmentVariable("FT8_RX_TO"), out var t) ? t : lines.Length;
        var pattern = Environment.GetEnvironmentVariable("FT8_RX_GREP");

        var builder = new StringBuilder();
        for (var i = Math.Max(1, from); i <= Math.Min(to, lines.Length); i++)
        {
            var line = lines[i - 1].TrimEnd('\r');
            if (pattern is { Length: > 0 } && !Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
            {
                continue;
            }

            builder.Append(i).Append('\t').Append(line).Append('\n');
        }

        _output.WriteLine(builder.ToString());
    }

    private string ReadSource(string relative)
    {
        var path = Path.Combine(RequireReachableClone(), relative);
        Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Pulls one function's body out by brace matching, so an assertion aimed at
    /// <c>monitor_init</c> cannot be satisfied by a line in <c>monitor_process</c>. Unit 209 was
    /// caught by exactly that and the habit is kept.
    /// </summary>
    private static string ExtractFunctionBody(string source, string name)
    {
        var head = Regex.Match(source, $@"^[A-Za-z_][A-Za-z0-9_ \t\*]*\b{Regex.Escape(name)}\s*\([^;{{]*\)\s*\{{", RegexOptions.Multiline);
        Assert.True(head.Success, $"{name} is no longer defined in the source read.");

        var depth = 0;
        var start = head.Index + head.Length - 1;
        for (var i = start; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(i + 1)];
                }
            }
        }

        Assert.Fail($"{name}'s body is unbalanced; the brace match ran off the end.");
        return string.Empty;
    }

    private void Shows(string source, string pattern, string what, string why)
    {
        var present = Regex.IsMatch(source, pattern, RegexOptions.Multiline);
        _output.WriteLine($"    {(present ? "yes" : "NO "),-4} {what}");
        Assert.True(present, $"the pin no longer shows that {what} — {why}.");
    }

    private static bool IsPowerOfTwo(int n) => n > 0 && (n & (n - 1)) == 0;

    private static string Factorise(int n)
    {
        var parts = new List<string>();
        var twos = 0;
        while (n % 2 == 0)
        {
            n /= 2;
            twos++;
        }

        if (twos > 0)
        {
            parts.Add($"2^{twos}");
        }

        for (var p = 3; p * p <= n; p += 2)
        {
            while (n % p == 0)
            {
                n /= p;
                parts.Add(p.ToString());
            }
        }

        if (n > 1)
        {
            parts.Add(n.ToString());
        }

        return string.Join(" x ", parts);
    }

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so nothing can be read tonight.");
        }

        return ReferenceClone.Location;
    }
}
