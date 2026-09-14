using System.Net.Http;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Tests;

/// <summary>
/// **A license lookup with the answer written in**, for a test that needs the operator's class to
/// land the same way on every run (work instruction 355 task 4).
/// </summary>
/// <remarks>
/// **NO REQUEST, NO CLIENT, AND A SOURCE NAME THAT SAYS SO.** Unit 354 found the plain layout fixture
/// asking callook.info for KC3QIS at construction, so *General* landed or did not depending on the
/// network, and the layout tests measured whichever window that made.
/// </remarks>
internal sealed class FixedLicenseLookup : ICallsignLookup
{
    /// <summary>The source name a fixed answer carries onto the profile.</summary>
    internal const string Name = "a fixed answer under test";

    private readonly IReadOnlyDictionary<string, LicenseClass> _answers;

    /// <summary>Creates the lookup.</summary>
    /// <param name="answers">Each callsign's class; any other callsign is not known.</param>
    internal FixedLicenseLookup(IReadOnlyDictionary<string, LicenseClass> answers) => _answers = answers;

    /// <summary>How many times it was asked.</summary>
    internal int Asked { get; private set; }

    /// <summary>**General for KC3QIS**, the answer both layout fixtures supply explicitly.</summary>
    internal static FixedLicenseLookup GeneralForKc3qis()
        => new(new Dictionary<string, LicenseClass>(StringComparer.OrdinalIgnoreCase) { ["KC3QIS"] = LicenseClass.General });

    /// <inheritdoc/>
    public string SourceName => Name;

    /// <inheritdoc/>
    public Task<CallsignLookupResult?> LookupAsync(string callsign, CancellationToken cancellationToken = default)
    {
        Asked++;

        var call = (callsign ?? "").Trim().ToUpperInvariant();

        return Task.FromResult(_answers.TryGetValue(call, out var found)
            ? new CallsignLookupResult(call, found, Name, new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc))
            : null);
    }
}

/// <summary>
/// **A license lookup with the network denied**: every request fails as a transport failure does, and
/// nothing leaves the machine.
/// </summary>
/// <remarks>
/// **WHAT A VIEW MODEL A TEST BUILDS WITHOUT A LOOKUP OF ITS OWN GETS** (work instruction 355 task 4).
/// A failed request is a state the application already handles - the class stays as it was and the
/// status bar says the service could not be reached - so a test sees what an offline machine sees.
/// </remarks>
internal sealed class NetworkDeniedLookup : ICallsignLookup
{
    private int _attempts;

    /// <summary>How many requests were refused.</summary>
    internal int Attempts => _attempts;

    /// <inheritdoc/>
    public string SourceName => CallookCallsignLookup.ServiceName;

    /// <inheritdoc/>
    public Task<CallsignLookupResult?> LookupAsync(string callsign, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _attempts);

        return Task.FromException<CallsignLookupResult?>(
            new HttpRequestException("the network is denied under test; " + SourceName + " was not asked"));
    }
}

/// <summary>
/// **The network denied for the whole test assembly**, before its first test builds a view model (work
/// instruction 355 task 4).
/// </summary>
/// <remarks>
/// A module initializer for <c>TheOperatorsFolderGuard</c>'s reason: a fixture binds only the classes that
/// ask for it, and the reach to callook.info is through a constructor 148 files in this project call.
/// </remarks>
internal static class TheNetworkIsDeniedGuard
{
    /// <summary>Every view model built here without a lookup of its own gets the network denied.</summary>
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void DenyTheNetwork()
        => Hamlet.App.ViewModels.MainWindowViewModel.DefaultLicenseLookup = () => new NetworkDeniedLookup();
}
