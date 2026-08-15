using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.App.ViewModels;

/// <summary>One line of what Hamlet would change, and why (HM-DEC-084).</summary>
public sealed partial class AdviceRowViewModel : ObservableObject
{
    /// <summary>Wraps one suggestion.</summary>
    /// <param name="suggestion">The suggestion.</param>
    public AdviceRowViewModel(ReceiveSuggestion suggestion) => Suggestion = suggestion;

    /// <summary>The suggestion behind it.</summary>
    public ReceiveSuggestion Suggestion { get; }

    /// <summary>What would change and why.</summary>
    public string Says => Suggestion.Says;

    /// <summary>True when pressing the button would do this one.</summary>
    public bool WouldChange => Suggestion.WouldChange;

    /// <summary>
    /// A mark for the row, so the list reads at a glance.
    /// </summary>
    /// <remarks>
    /// Rows already correct stay visible and say so (HM-DEC-084). Hiding them is
    /// tidier and teaches nothing; showing them is the app proving what it
    /// checked, which is the difference between being trusted and being
    /// second-guessed.
    /// </remarks>
    public string Mark => Suggestion.Unreadable
        ? "?"
        : Suggestion.AlreadyRight ? "✓" : "→";
}

/// <summary>
/// "I can hear it and Hamlet can't" (HM-DEC-084).
/// </summary>
/// <remarks>
/// <para>**SETTINGS ARE CONSEQUENCES OF INTENT, NEVER THINGS THE OPERATOR
/// OPERATES.** There is no Noise Blanker toggle here and there never will be.
/// There is one button that names a problem the operator has, and behind it the
/// handful of changes that usually cause it, each announced in plain words with
/// a way to put it back.</para>
/// <para>**DO ALL FOUR IS ONE PRESS.** Not four confirmations. None of these can
/// put anything on the air, and asking permission four times is exactly the
/// protectiveness this ruling exists to remove: it trains somebody to click
/// through prompts, which is worse than not having them.</para>
/// </remarks>
public sealed partial class ReceiveHelpViewModel : ObservableObject
{
    private readonly Func<RigState> _state;
    private readonly Func<CivWrite, int, Task<RigWriteResult>> _write;
    private readonly Action<SettingChange>? _announced;

    /// <summary>Creates the panel over the radio.</summary>
    /// <param name="state">How to read what the radio is doing now.</param>
    /// <param name="write">How to set one documented setting.</param>
    /// <param name="announced">Called for each change, so it can be recorded.</param>
    public ReceiveHelpViewModel(
        Func<RigState> state,
        Func<CivWrite, int, Task<RigWriteResult>> write,
        Action<SettingChange>? announced = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _write = write ?? throw new ArgumentNullException(nameof(write));
        _announced = announced;

        Refresh();
    }

    /// <summary>What Hamlet would change, one line each.</summary>
    public ObservableCollection<AdviceRowViewModel> Rows { get; } = new();

    /// <summary>What has been changed this session, newest first.</summary>
    public ObservableCollection<SettingChange> Changes { get; } = new();

    /// <summary>True when anything has been changed.</summary>
    public bool HasChanges => Changes.Count > 0;

    /// <summary>True while writes are going out.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FixAllCommand))]
    private bool _isWorking;

    /// <summary>What the button says.</summary>
    public string ActionLabel
    {
        get
        {
            var count = Rows.Count(r => r.WouldChange);

            return count switch
            {
                0 => "Nothing to change",
                1 => "Do that one thing",
                _ => $"Do all {count}",
            };
        }
    }

    /// <summary>True when there is anything to do.</summary>
    public bool CanFix => !IsWorking && Rows.Any(r => r.WouldChange);

    /// <summary>Recompute the list from what the radio is doing now.</summary>
    /// <remarks>
    /// From live state and never hardcoded (HM-DEC-084). A list that did not
    /// move when the radio did would be describing a radio nobody owns.
    /// </remarks>
    public void Refresh()
    {
        var advice = ReceiveAdvice.For(_state());

        Rows.Clear();
        foreach (var one in advice)
        {
            Rows.Add(new AdviceRowViewModel(one));
        }

        OnPropertyChanged(nameof(ActionLabel));
        OnPropertyChanged(nameof(CanFix));
        FixAllCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Do everything on the list that needs doing, in one press.
    /// </summary>
    /// <returns>A task that completes when the writes have gone.</returns>
    [RelayCommand(CanExecute = nameof(CanFix))]
    private async Task FixAllAsync()
    {
        IsWorking = true;

        try
        {
            foreach (var row in Rows.Where(r => r.WouldChange).ToList())
            {
                await ApplyAsync(row.Suggestion);
            }
        }
        finally
        {
            IsWorking = false;
            Refresh();
        }
    }

    /// <summary>Do one of them, for somebody who wants them one at a time.</summary>
    /// <param name="row">Which one.</param>
    /// <returns>A task.</returns>
    [RelayCommand]
    private async Task ApplyOneAsync(AdviceRowViewModel? row)
    {
        if (row is null || !row.WouldChange || IsWorking)
        {
            return;
        }

        IsWorking = true;

        try
        {
            await ApplyAsync(row.Suggestion);
        }
        finally
        {
            IsWorking = false;
            Refresh();
        }
    }

    /// <summary>
    /// Put one change back.
    /// </summary>
    /// <param name="change">Which one.</param>
    /// <returns>A task.</returns>
    /// <remarks>
    /// Only where the prior value was actually read. An undo that invented one
    /// would be a write decided by a guess wearing the most reassuring word in
    /// the application (HM-DEC-084).
    /// </remarks>
    [RelayCommand]
    private async Task UndoAsync(SettingChange? change)
    {
        if (change is not { CanUndo: true, Was: { } was } || IsWorking)
        {
            return;
        }

        IsWorking = true;

        try
        {
            var result = await _write(change.Write, was);

            if (result.Worked)
            {
                Changes.Remove(change);
                OnPropertyChanged(nameof(HasChanges));
            }
        }
        finally
        {
            IsWorking = false;
            Refresh();
        }
    }

    /// <summary>Put everything back that can be put back.</summary>
    /// <returns>A task.</returns>
    [RelayCommand]
    private async Task UndoAllAsync()
    {
        foreach (var change in Changes.Where(c => c.CanUndo).ToList())
        {
            await UndoAsync(change);
        }
    }

    /// <summary>Read before, write, and record what happened.</summary>
    private async Task ApplyAsync(ReceiveSuggestion suggestion)
    {
        // READ BEFORE WRITE. The state model already holds it, so the prior
        // value is free; what matters is that it is null when it was never read
        // rather than a plausible number (HM-DEC-050, HM-DEC-084).
        var before = _state()[suggestion.Write.Field];
        var was = before.IsKnown ? (int?)before.Number : null;

        var result = await _write(suggestion.Write, suggestion.Value);

        var change = new SettingChange(
            suggestion.Write, was, suggestion.Value, suggestion.Says,
            result.Outcome, DateTime.UtcNow);

        Changes.Insert(0, change);
        OnPropertyChanged(nameof(HasChanges));

        _announced?.Invoke(change);
    }
}
