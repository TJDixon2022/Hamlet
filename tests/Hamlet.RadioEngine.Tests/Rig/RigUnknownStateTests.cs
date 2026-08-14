using System.Reflection;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Unknown as a first-class state: no default ever stands in for a real
/// reading, and the four ways of not knowing stay apart (HM-DEC-009,
/// HM-DEC-030, HM-DEC-050).
/// </summary>
public sealed class RigUnknownStateTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    /// <remarks>
    /// THE STRUCTURAL GUARANTEE. Every way of building a value that is not a
    /// reading leaves the number null, so there is no path by which a caller
    /// reaches a zero and mistakes it for a measurement. This is the subsystem
    /// where that would be easiest to slip, because a resting needle and a quiet
    /// band look identical.
    /// </remarks>
    [Fact]
    public void NothingButARealReadingEverCarriesANumber()
    {
        var notReadings = new[]
        {
            RigValue.Unknown(RigField.SMeter),
            RigValue.Unsupported(RigField.SMeter, "training radio"),
            RigValue.Undocumented(RigField.Vfo, "no command"),
        };

        Assert.All(notReadings, v =>
        {
            Assert.Null(v.Number);
            Assert.Null(v.AtUtc);
            Assert.False(v.IsKnown);
        });
    }

    /// <remarks>
    /// Proves each of the four states reads differently on screen. A UI that
    /// collapsed them would either keep waiting for something that will never
    /// arrive, or blame the radio for a gap in Hamlet.
    /// </remarks>
    [Fact]
    public void TheFourStatesEachSaySomethingDifferent()
    {
        var texts = new[]
        {
            RigDisplay.Describe(RigValue.Unknown(RigField.Agc), Now).Text,
            RigDisplay.Describe(RigValue.Unsupported(RigField.Agc, "x"), Now).Text,
            RigDisplay.Describe(RigValue.Undocumented(RigField.Vfo, "x"), Now).Text,
            RigDisplay.Describe(
                RigValue.Known(RigField.Agc, 3, "SLOW", Now, "CI-V 16 12"), Now).Text,
        };

        Assert.Equal(4, texts.Distinct().Count());
        Assert.All(texts, t => Assert.False(string.IsNullOrWhiteSpace(t)));
    }

    /// <remarks>
    /// Proves a stale reading keeps its number and gains its age, rather than
    /// being blanked or shown as current. "S7, read four minutes ago" is useful
    /// and honest; a bare "S7" would be a claim about now.
    /// </remarks>
    [Fact]
    public void AStaleReadingKeepsItsNumberAndGainsItsAge()
    {
        var old = RigValue.Known(
            RigField.SMeter, 90, "S7", Now.AddMinutes(-4), "CI-V 15 02");

        var readout = RigDisplay.Describe(old, Now);

        Assert.Equal(RigFreshness.Stale, readout.Freshness);
        Assert.False(readout.IsCurrent);
        Assert.Equal("S7", readout.Text);
        Assert.Equal("about 4 minutes ago", readout.AgeText);
    }

    /// <remarks>
    /// Proves a fresh reading is marked current, and that the same value goes
    /// stale as it ages. Freshness is a fact about when, not about the value.
    /// </remarks>
    [Fact]
    public void TheSameReadingGoesStaleAsItAges()
    {
        var reading = RigValue.Known(RigField.SMeter, 90, "S7", Now, "CI-V 15 02");

        Assert.Equal(RigFreshness.Fresh, RigDisplay.Describe(reading, Now).Freshness);

        var later = Now + RigPollPlan.LiveFreshFor + TimeSpan.FromSeconds(1);
        Assert.Equal(RigFreshness.Stale, RigDisplay.Describe(reading, later).Freshness);
    }

    /// <remarks>
    /// Proves a setting is allowed to be much older than a meter before anybody
    /// calls it stale. Nobody changes their AGC twice a minute, and marking it
    /// stale after a second would train the operator to ignore the marking.
    /// </remarks>
    [Fact]
    public void ASettingStaysCurrentFarLongerThanAMeter()
    {
        var when = Now.AddSeconds(-20);
        var meter = RigValue.Known(RigField.SMeter, 90, "S7", when, "CI-V 15 02");
        var agc = RigValue.Known(RigField.Agc, 3, "SLOW", when, "CI-V 16 12");

        Assert.Equal(RigFreshness.Stale, RigDisplay.Describe(meter, Now).Freshness);
        Assert.Equal(RigFreshness.Fresh, RigDisplay.Describe(agc, Now).Freshness);
    }

    /// <remarks>
    /// Proves ages are spoken rather than counted (§0.7). Nobody reading a
    /// diagnostics screen at two in the morning wants a stopwatch reading.
    /// </remarks>
    [Theory]
    [InlineData(0.5, "a moment ago")]
    [InlineData(8, "8 seconds ago")]
    [InlineData(75, "about a minute ago")]
    [InlineData(600, "about 10 minutes ago")]
    [InlineData(4000, "over an hour ago")]
    public void AgesAreSpokenRatherThanCounted(double seconds, string expected)
        => Assert.Equal(expected, RigDisplay.Age(TimeSpan.FromSeconds(seconds)));

    /// <remarks>
    /// Proves something never read has no age at all, which is not the same as
    /// being very old.
    /// </remarks>
    [Fact]
    public void SomethingNeverReadHasNoAge()
    {
        Assert.Equal("", RigDisplay.Age(null));

        var readout = RigDisplay.Describe(RigValue.Unknown(RigField.SMeter), Now);
        Assert.Equal(RigFreshness.None, readout.Freshness);
        Assert.Equal("", readout.AgeText);
    }

    /// <remarks>
    /// Proves every field has a label a person would recognize, so a new field
    /// added next month cannot reach the diagnostics screen as an identifier.
    /// </remarks>
    [Fact]
    public void EveryFieldHasALabelInOrdinaryWords()
    {
        foreach (var field in Enum.GetValues<RigField>())
        {
            var label = RigDisplay.Label(field);

            Assert.False(string.IsNullOrWhiteSpace(label));
            Assert.DoesNotContain("_", label, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// THE GUARD AGAINST DRIFT. The state's typed accessors must answer null
    /// rather than a default when nothing has been read, and a reflection sweep
    /// catches a new one added later that forgets. A property returning a
    /// non-nullable number would be a default standing in for a reading, which
    /// is the exact failure this whole model prevents.
    /// </remarks>
    [Fact]
    public void EveryTypedAccessorOnTheStateCanSayItDoesNotKnow()
    {
        var accessors = typeof(RigState)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            // Two "known to be" accessors are deliberately not nullable, and
            // the test below says what each of them means when nothing has been
            // read (§0.2, HM-DEC-056).
            .Where(p => p.Name is not (nameof(RigState.RigName)
                or nameof(RigState.KnownCount)
                or nameof(RigState.IsTransmitting)
                or nameof(RigState.IsDataMode)))
            .ToList();

        Assert.NotEmpty(accessors);

        foreach (var property in accessors)
        {
            var type = property.PropertyType;
            var nullable = !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

            Assert.True(
                nullable,
                $"{property.Name} returns {type.Name}, which cannot say it does not know");

            Assert.Null(property.GetValue(RigState.Empty));
        }
    }

    /// <remarks>
    /// Proves the accessors that are deliberately not nullable say what they
    /// mean. "Known to be transmitting" is false when nothing has been read, and
    /// anything that keys a transmitter later must treat unread and receiving as
    /// different things (§0.2). "Known to be in the data variant" is the same
    /// shape, and false there is safe in the other direction: its only caller is
    /// the automation deciding whether to write, and an unread data setting is a
    /// reason to set it rather than a reason to leave it (HM-DEC-056).
    /// </remarks>
    [Fact]
    public void KnownToBeTransmittingIsFalseWhenNothingHasBeenRead()
    {
        Assert.False(RigState.Empty.IsTransmitting);
        Assert.Equal(RigValueState.Unknown, RigState.Empty[RigField.TransmitStatus].State);

        var receiving = RigState.Empty.With(
            RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"));

        Assert.False(receiving.IsTransmitting);
        Assert.True(receiving[RigField.TransmitStatus].IsKnown);

        // The same shape for the data variant, and the field itself still
        // carries the unknown state for anything that displays it.
        Assert.False(RigState.Empty.IsDataMode);
        Assert.Equal(RigValueState.Unknown, RigState.Empty[RigField.DataMode].State);

        var dataOn = RigState.Empty.With(
            RigValue.Known(RigField.DataMode, 1, "on", Now, "CI-V 26 00"));

        Assert.True(dataOn.IsDataMode);
    }
}
