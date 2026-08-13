using Hamlet.App.Licensing;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Licensing;

/// <summary>
/// Lazy licence resolution (HM-DEC-028) and the status line it feeds
/// (HM-DEC-029).
/// </summary>
public sealed class LicenceResolverTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);

    private static CwBand Forty => BandPlan.Bands.First(b => b.Name == "40 m");

    /// <summary>A lookup that answers from a script, without a network.</summary>
    private sealed class StubLookup : ICallsignLookup
    {
        private readonly Func<string, CallsignLookupResult?> _answer;

        public StubLookup(Func<string, CallsignLookupResult?> answer) => _answer = answer;

        public string SourceName => "stub-registry";

        public int Calls { get; private set; }

        public Task<CallsignLookupResult?> LookupAsync(
            string callsign, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(_answer(callsign));
        }
    }

    private static StubLookup Answering(LicenceClass cls)
        => new(call => new CallsignLookupResult(call, cls, "stub-registry", Now));

    /// <remarks>
    /// Proves the trigger is the fact, not a wizard step: a callsign present
    /// and a class missing is all it takes. People skip wizards, and the
    /// callsign can arrive from Settings or a hand-edited file.
    /// </remarks>
    [Fact]
    public void NeedsResolution_TracksTheFactRatherThanAScreen()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        Assert.True(LicenceResolver.NeedsResolution(profile));

        profile.SetLicenceClass(
            LicenceClass.General, LicenceClassSource.LookedUp, "x", Now);
        Assert.False(LicenceResolver.NeedsResolution(profile));

        Assert.False(LicenceResolver.NeedsResolution(new OperatorProfile { Callsign = "  " }));
    }

    /// <remarks>
    /// Proves a missing class is filled in with its provenance, which is what
    /// the status bar narrates and Settings shows.
    /// </remarks>
    [Fact]
    public async Task Resolve_FillsAnUnknownClassWithItsProvenance()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        var resolver = new LicenceResolver(Answering(LicenceClass.General), () => Now);

        var result = await resolver.ResolveAsync(profile);

        Assert.Equal(LicenceResolutionOutcome.Resolved, result.Outcome);
        Assert.Equal(LicenceClass.General, profile.LicenceClass);
        Assert.Equal(LicenceClassSource.LookedUp, profile.LicenceClassSource);
        Assert.Equal("stub-registry", profile.LicenceClassSourceName);
        Assert.Equal("2026-08-13", profile.LicenceClassSetOn);
        Assert.Contains("General", result.Narration, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the ruling that matters most (HM-DEC-028): a lookup NEVER
    /// silently overwrites a class the operator set by hand. Both values come
    /// back with the source, and the profile is untouched until they choose.
    /// It is their licence.
    /// </remarks>
    [Fact]
    public async Task Resolve_NeverOverwritesAHandSetClass()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        profile.SetLicenceClass(
            LicenceClass.General, LicenceClassSource.EnteredByOperator, "", Now);

        var resolver = new LicenceResolver(Answering(LicenceClass.Extra), () => Now);
        var result = await resolver.ResolveAsync(profile);

        Assert.Equal(LicenceResolutionOutcome.Mismatch, result.Outcome);
        Assert.True(result.NeedsOperatorDecision);
        Assert.Equal(LicenceClass.Extra, result.Found);
        Assert.Equal(LicenceClass.General, result.Existing);

        // The profile is exactly as the operator left it.
        Assert.Equal(LicenceClass.General, profile.LicenceClass);
        Assert.Equal(LicenceClassSource.EnteredByOperator, profile.LicenceClassSource);

        // And the operator is shown both, plainly.
        Assert.Contains("Extra", result.Narration, StringComparison.Ordinal);
        Assert.Contains("General", result.Narration, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves agreement is quiet. A lookup that confirms the operator's own
    /// answer should not produce a decision to make.
    /// </remarks>
    [Fact]
    public async Task Resolve_IsSilentWhenTheLookupAgrees()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        profile.SetLicenceClass(
            LicenceClass.General, LicenceClassSource.EnteredByOperator, "", Now);

        var result = await new LicenceResolver(Answering(LicenceClass.General), () => Now)
            .ResolveAsync(profile);

        Assert.Equal(LicenceResolutionOutcome.NotNeeded, result.Outcome);
        Assert.False(result.NeedsOperatorDecision);
        Assert.Equal(LicenceClassSource.EnteredByOperator, profile.LicenceClassSource);
    }

    /// <remarks>
    /// Proves a service being down blocks nobody. The class stays unknown,
    /// the narration says what to do, and Settings still takes a hand-picked
    /// answer — the bottom of the fallback ladder.
    /// </remarks>
    [Fact]
    public async Task Resolve_FallsBackWithoutBlocking()
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS" };
        var broken = new StubLookup(_ => throw new HttpRequestException("down"));

        var result = await new LicenceResolver(broken, () => Now).ResolveAsync(profile);

        Assert.Equal(LicenceResolutionOutcome.Unavailable, result.Outcome);
        Assert.Equal(LicenceClass.Unknown, profile.LicenceClass);
        Assert.Contains("Settings", result.Narration, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves an unknown callsign is an answer rather than a fault, and says
    /// where to go instead.
    /// </remarks>
    [Fact]
    public async Task Resolve_HandlesAnUnknownCallsign()
    {
        var profile = new OperatorProfile { Callsign = "XX0XXX" };
        var result = await new LicenceResolver(new StubLookup(_ => null), () => Now)
            .ResolveAsync(profile);

        Assert.Equal(LicenceResolutionOutcome.NotFound, result.Outcome);
        Assert.Equal(LicenceClass.Unknown, profile.LicenceClass);
        Assert.Contains("Settings", result.Narration, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the narration exists for the status bar. Visible competence is
    /// the point: the operator sees Hamlet work rather than wondering whether
    /// it did.
    /// </remarks>
    [Fact]
    public void Narration_NamesTheCallsignBeingLookedUp()
        => Assert.Equal("Looking up KC3QIS…", LicenceResolver.LookingUpNarration(" kc3qis "));

    /// <remarks>
    /// Proves provenance is stated differently for a lookup and a hand-set
    /// value — the operator is entitled to know which they are looking at.
    /// </remarks>
    [Fact]
    public void Provenance_DistinguishesLookedUpFromHandSet()
    {
        var looked = new OperatorProfile();
        looked.SetLicenceClass(LicenceClass.General, LicenceClassSource.LookedUp, "callook.info", Now);
        Assert.Contains("callook.info", LicenceResolver.DescribeProvenance(looked), StringComparison.Ordinal);

        var typed = new OperatorProfile();
        typed.SetLicenceClass(LicenceClass.General, LicenceClassSource.EnteredByOperator, "", Now);
        Assert.Contains("you set this", LicenceResolver.DescribeProvenance(typed), StringComparison.OrdinalIgnoreCase);

        var none = new OperatorProfile();
        Assert.Contains("not set", LicenceResolver.DescribeProvenance(none), StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the class survives a restart with its provenance, through the
    /// real loader.
    /// </remarks>
    [Fact]
    public void LicenceClass_RoundTripsThroughSettingsJson()
    {
        var folder = Path.Combine(Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "settings.json");

        try
        {
            var written = new AppSettings();
            written.Operator.SetLicenceClass(
                LicenceClass.Extra, LicenceClassSource.EnteredByOperator, "", Now);

            SettingsStore.SaveTo(written, path);
            var read = SettingsStore.LoadFrom(path);

            Assert.Equal(LicenceClass.Extra, read.Operator.LicenceClass);
            Assert.Equal(LicenceClassSource.EnteredByOperator, read.Operator.LicenceClassSource);
            Assert.True(read.Operator.LicenceClassWasSetByHand);
            Assert.Equal("2026-08-13", read.Operator.LicenceClassSetOn);
        }
        finally
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not a test failure.
            }
        }
    }

    /// <remarks>
    /// Proves the class is written as a readable name and a hand-edited one
    /// is honoured. This was found by hand-editing the real settings file:
    /// the class serialised as a bare number, and editing it to "General"
    /// threw, was caught by the never-throw loader, and silently reverted
    /// EVERY setting to defaults. HM-DEC-028 expects the callsign and class
    /// to arrive from hand-edited files, so that had to work.
    /// </remarks>
    [Fact]
    public void LicenceClass_IsWrittenAsAName_AndHandEditsAreHonoured()
    {
        var folder = Path.Combine(Path.GetTempPath(), "hamlet-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "settings.json");

        try
        {
            var written = new AppSettings { LastBand = "20 m" };
            written.Operator.SetLicenceClass(
                LicenceClass.General, LicenceClassSource.LookedUp, "callook.info", Now);
            SettingsStore.SaveTo(written, path);

            var json = File.ReadAllText(path);
            Assert.Contains("\"General\"", json, StringComparison.Ordinal);

            // Somebody edits it by hand to a different class.
            File.WriteAllText(path, json.Replace("\"General\"", "\"Extra\""));
            var read = SettingsStore.LoadFrom(path);

            Assert.Equal(LicenceClass.Extra, read.Operator.LicenceClass);

            // And nothing else was lost on the way through.
            Assert.Equal("20 m", read.LastBand);
        }
        finally
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not a test failure.
            }
        }
    }

    /// <remarks>
    /// Proves a fresh profile claims nothing. Defaulting to the commonest
    /// class would be the one guess with legal consequences (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void FreshProfile_HasNoLicenceClass()
    {
        var profile = new OperatorProfile();

        Assert.Equal(LicenceClass.Unknown, profile.LicenceClass);
        Assert.Equal(LicenceClassSource.Unset, profile.LicenceClassSource);
        Assert.False(profile.LicenceClassWasSetByHand);
    }

    /// <remarks>
    /// Proves the guard rail ships on, and that it is transmit-only — no
    /// setting anywhere restricts listening.
    /// </remarks>
    [Fact]
    public void GuardRail_ShipsOnAndGovernsTransmitOnly()
    {
        Assert.True(new AppSettings().RestrictTransmitToPrivileges);

        var suspicious = typeof(AppSettings)
            .GetProperties()
            .Where(p => p.Name.Contains("Receive", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Contains("Listen", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Contains("Tuning", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Name)
            .ToList();

        Assert.True(
            suspicious.Count == 0,
            "no setting may restrict listening or tuning; found: " + string.Join(", ", suspicious));
    }
}
