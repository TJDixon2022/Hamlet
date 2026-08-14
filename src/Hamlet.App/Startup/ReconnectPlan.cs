namespace Hamlet.App.Startup;

/// <summary>What the startup reconnect should do.</summary>
public enum ReconnectStep
{
    /// <summary>Leave the radio alone: the setting is off, or something is
    /// already connected.</summary>
    Nothing,

    /// <summary>Open the training radio.</summary>
    TrainingRadio,

    /// <summary>Try the port the operator was last using.</summary>
    RememberedPort,
}

/// <summary>
/// The startup reconnect decision, as a value (HM-DEC-052).
/// </summary>
/// <remarks>
/// Separated from the ViewModel so the rule can be proved without a window, a
/// serial port or a settings file. What Hamlet does when the radio is not there
/// is the whole point of the feature, and it is exactly the case that never
/// gets exercised by hand.
/// </remarks>
/// <param name="Step">What to do.</param>
/// <param name="Port">The port to open, empty when there is nothing to do.</param>
/// <param name="Explanation">
/// What to say afterwards, or null when the plain connect message says enough.
/// </param>
public sealed record ReconnectPlan(ReconnectStep Step, string Port, string? Explanation)
{
    /// <summary>Works out what to do when the window opens.</summary>
    /// <param name="enabled">The operator's setting.</param>
    /// <param name="alreadyConnected">Whether a rig is already open.</param>
    /// <param name="lastPort">The remembered port, or null.</param>
    /// <param name="available">Ports this machine has right now.</param>
    /// <param name="trainingRadio">The training radio's entry in the port list.</param>
    public static ReconnectPlan Decide(
        bool enabled,
        bool alreadyConnected,
        string? lastPort,
        IReadOnlyCollection<string> available,
        string trainingRadio)
    {
        if (!enabled || alreadyConnected)
        {
            return new ReconnectPlan(ReconnectStep.Nothing, string.Empty, null);
        }

        if (string.IsNullOrWhiteSpace(lastPort) || lastPort == trainingRadio)
        {
            return new ReconnectPlan(ReconnectStep.TrainingRadio, trainingRadio, null);
        }

        // A PORT THAT IS GONE IS NAMED, not folded into a general failure.
        // Windows hands a USB radio whichever COM number is free at the time,
        // and it changes it after an update or a different socket. Somebody who
        // is told "could not connect" checks the cable, the radio, the baud
        // rate and their own sanity before they think to look at the port list.
        if (!available.Contains(lastPort))
        {
            return new ReconnectPlan(
                ReconnectStep.TrainingRadio,
                trainingRadio,
                $"{lastPort} isn't on this computer any more, so Hamlet is on the "
                + "training radio. Windows moves a USB radio to a different COM port "
                + "now and then, so it is worth a look in the port list.");
        }

        return new ReconnectPlan(ReconnectStep.RememberedPort, lastPort, null);
    }

    /// <summary>
    /// What to say when the port is there and the radio is not answering.
    /// </summary>
    /// <remarks>
    /// Almost always a radio that is switched off, which is not a fault and is
    /// not written like one. It says what happened, where Hamlet ended up, and
    /// how to change its mind, and then it stops talking.
    /// </remarks>
    /// <param name="port">The port that did not answer.</param>
    public static string NoAnswer(string port)
        => $"{port} isn't answering, so Hamlet is on the training radio. Switch the "
         + "radio on and click Connect whenever you are ready.";

    /// <summary>What to say when opening the port threw.</summary>
    /// <remarks>
    /// Usually another program holding the port open. Hamlet cannot tell which
    /// one from here, so it does not guess at a cause it does not have (§0.0).
    /// </remarks>
    public static string CouldNotOpen()
        => "Hamlet could not open the radio this time, so it is on the training "
         + "radio. Click Connect to try again.";
}
