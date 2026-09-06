using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 252, task 4: the two tooltips meet the panel, and nothing
/// else on it moves.
/// </summary>
/// <remarks>
/// **THE HALF OF THIS TASK THAT IS A GUARD RATHER THAN A FEATURE.** Two tooltips
/// gained something; the rest of the closed table gained nothing, and a test that
/// only checked the new sentences would let the old ones drift without anybody
/// noticing until an operator read one.
/// </remarks>
public sealed class TheSenderTooltipNamesTheEntityTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheSenderTooltipNamesTheEntityTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The sender's tooltip names the entity where it is certain.</summary>
    [Theory]
    [InlineData("CQ HA1BF JN86", "HA1BF is a callsign from Hungary.")]
    [InlineData("CQ IS0AAA JM49", "IS0AAA is a callsign from Sardinia.")]
    [InlineData("KE9COB N5CH R+14", "N5CH is a callsign from United States of America.")]
    public void TheSenderTooltipNamesTheEntity(string message, string expected)
    {
        var help = Row(message).SenderHelp;

        _output.WriteLine(message + " -> " + help);

        Assert.StartsWith("Who sent it.", help, StringComparison.Ordinal);
        Assert.Contains(expected, help, StringComparison.Ordinal);
    }

    /// <summary>
    /// Where the entity is not certain, the tooltip is exactly what it was.
    /// </summary>
    /// <remarks>
    /// **NO HEDGE IN PLACE OF THE ANSWER** (Tim's ruling). Not *unknown*, not
    /// *probably*, not a shortened sentence that reads as a failure. The line is
    /// byte for byte the structural wording unit 241 wrote, so a reader cannot
    /// tell a declined lookup from a panel that never looked — which is correct,
    /// because the country is not part of what this field promises.
    /// </remarks>
    [Theory]
    [InlineData("CQ VK9XYZ QG44")]
    [InlineData("CQ 3D2AB RH91")]
    // `Q` is reserved for Q codes and is allocated to nobody, so it is
    // the honest "not in the table at all" case. `ZZZZZ` was tried first and
    // is Brazil: ZV-ZZ is a real Brazilian series, so that input was wrong and
    // the code was right.
    [InlineData("CQ QQ1ABC AA00")]
    public void AnUncertainEntityLeavesTheTooltipExactlyAsItWas(string message)
    {
        var help = Row(message).SenderHelp;

        _output.WriteLine(message + " -> " + help);

        Assert.Equal("Who sent it.", help);
    }

    /// <summary>No sender tooltip ever hedges.</summary>
    [Fact]
    public void NoSenderTooltipEverHedges()
    {
        string[] messages =
        {
            "CQ HA1BF JN86", "CQ VK9XYZ QG44", "CQ 3D2AB RH91",
            "KE9COB N5CH R+14", "CQ IS0AAA JM49", "CQ EA8ABC IL18",
            "CQ QQ1ABC AA00", "TNX FER QSO OM",
        };

        string[] hedges =
        {
            "probably", "possibly", "maybe", "unknown", "likely", "perhaps",
            "uncertain", "guess", "may be", "could be",
        };

        foreach (var message in messages)
        {
            var help = Row(message).SenderHelp;

            _output.WriteLine(message + " -> " + help);

            foreach (var hedge in hedges)
            {
                Assert.DoesNotContain(hedge, help, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>The addressee tooltip did not change.</summary>
    /// <remarks>
    /// **ONLY THE `from` FIELD GAINED THE COUNTRY**, which is the instruction's
    /// own scope. The addressee is who a message is aimed at, and where that
    /// station is licensed is not what the operator is asking when he hovers it.
    /// </remarks>
    [Theory]
    [InlineData("CQ HA1BF JN86", "Who this is addressed to. CQ means anyone.")]
    [InlineData("KE9COB N5CH R+14", "Who this is addressed to.")]
    public void TheAddresseeTooltipIsUnchanged(string message, string expected)
        => Assert.Equal(expected, Row(message).AddresseeHelp);

    /// <summary>A message with no three fields still has no field tooltips.</summary>
    [Fact]
    public void FreeTextStillHasNoFields()
    {
        var row = Row("TNX FER QSO OM");

        Assert.Equal("", row.Sender);
        Assert.Equal("", row.PayloadHelp);

        // No sender means nothing to name an entity for, and the structural line
        // is what is left.
        Assert.Equal("Who sent it.", row.SenderHelp);
    }

    /// <summary>A row built the way the panel builds one.</summary>
    private static DigitalDecodeRow Row(string message)
    {
        var settings = new AppSettings();

        settings.Operator.GridSquare = "FN00DJ";

        return new MainWindowViewModel(settings, null)
            .AddDecodeRowForTests("214135", "-11", "0.2", "1240", message);
    }
}
