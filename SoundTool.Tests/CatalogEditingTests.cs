using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundTool.Services;
using SoundTool.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using TombLib.Wad;
using TombLib.WPF.Services.Abstract;

namespace SoundTool.Tests;

/// <summary>Protects sound catalog editing and pending-change decisions during WPF command execution.</summary>
[TestClass]
public class CatalogEditingTests
{
    [TestMethod]
    public void CancelledSavePreventsNewAndClose()
    {
        var context = new EditorContext();
        var original = context.AddSound(7);
        context.Messages.Answer = 1;

        context.ViewModel.NewCommand.Execute(null);

        Assert.AreEqual(1, context.ViewModel.Sounds.Count);
        Assert.AreSame(original, context.ViewModel.SelectedSound);
        Assert.IsFalse(context.ViewModel.CanClose());
        Assert.IsTrue(context.ViewModel.Title.EndsWith("*", StringComparison.Ordinal));
        Assert.AreEqual(2, context.Dialogs.SaveRequests);
    }

    [TestMethod]
    public void CancelledSavePreventsOpeningAnotherCatalog()
    {
        var context = new EditorContext();
        var original = context.AddSound(7);
        context.Messages.Answer = 1;

        context.ViewModel.OpenCommand.Execute(null);

        Assert.AreEqual(0, context.Dialogs.OpenRequests);
        Assert.AreEqual(1, context.ViewModel.Sounds.Count);
        Assert.AreSame(original, context.ViewModel.SelectedSound);
        Assert.AreEqual(1, context.Dialogs.SaveRequests);
    }

    [TestMethod]
    public void FailedSavePreservesTheCurrentCatalog()
    {
        var context = new EditorContext();
        var original = context.AddSound(7);
        context.Messages.Answer = 1;
        context.Dialogs.SavePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "catalog.xml");

        context.ViewModel.NewCommand.Execute(null);

        Assert.AreEqual(1, context.Messages.Errors.Count);
        Assert.AreEqual(1, context.ViewModel.Sounds.Count);
        Assert.AreSame(original, context.ViewModel.SelectedSound);
        Assert.IsTrue(context.ViewModel.Title.EndsWith("*", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ResetPreservesTheSoundIdentityAndSamples()
    {
        var context = new EditorContext();
        var original = context.AddSound(42);
        original.Name = "CUSTOM_SOUND";
        original.Volume = 23;
        original.RandomizePitch = true;
        original.Samples.Add(new SampleViewModel("custom.wav"));

        context.ViewModel.ResetCommand.Execute(null);

        var reset = context.ViewModel.SelectedSound ?? throw new AssertFailedException("Reset did not select the updated sound.");
        Assert.AreEqual(42, reset.Id);
        Assert.AreEqual("CUSTOM_SOUND", reset.Name);
        Assert.AreEqual("custom.wav", reset.Samples[0].FileName);
        Assert.AreEqual(100, reset.Volume);
        Assert.IsFalse(reset.RandomizePitch);
        Assert.AreEqual(1, context.ViewModel.Sounds.Count);
    }

    [TestMethod]
    public void SearchWrapsToTheFirstSoundAndMatchesIds()
    {
        var context = new EditorContext();
        var first = context.AddSound(10);
        first.Name = "FIRST_SOUND";
        var second = context.AddSound(20);
        context.AddSound(30);
        context.ViewModel.SearchText = "first";

        context.ViewModel.SearchCommand.Execute(null);

        Assert.AreSame(first, context.ViewModel.SelectedSound);
        context.ViewModel.SearchText = "20";
        context.ViewModel.SearchCommand.Execute(null);
        Assert.AreSame(second, context.ViewModel.SelectedSound);
    }

    [TestMethod]
    public void ResolvedSamplePathsDoNotMarkTheSoundChanged()
    {
        var sound = new SoundInfoViewModel(new WadSoundInfo(12));
        var sample = new SampleViewModel("sample.wav");
        sound.Samples.Add(sample);
        int changes = 0;
        sound.Changed += (_, _) => changes++;

        sample.ResolvedPath = @"C:\Samples\sample.wav";

        Assert.AreEqual(0, changes);
        sample.FileName = "renamed.wav";
        Assert.AreEqual(1, changes);
        Assert.AreEqual("renamed.wav", sound.ToSound().Samples[0].FileName);
    }

    [TestMethod]
    public void NewSoundUsesTheLegacyDefaultsAndSelectsTheNewEntry()
    {
        var context = new EditorContext();

        var sound = context.AddSound(5);

        Assert.AreEqual("NEW_SOUND", sound.Name);
        Assert.AreEqual(100, sound.Volume);
        Assert.AreEqual(10, sound.RangeInSectors);
        Assert.AreEqual(100, sound.Chance);
        Assert.IsTrue(context.ViewModel.Title.EndsWith("*", StringComparison.Ordinal));
    }

    [TestMethod]
    public void IndexAllAndUnindexAllMarkTheCatalogChanged()
    {
        var context = new EditorContext();
        var first = context.AddSound(1);
        var second = context.AddSound(2);
        first.Indexed = false;
        second.Indexed = false;

        context.ViewModel.IndexAllCommand.Execute(null);

        Assert.IsTrue(first.Indexed);
        Assert.IsTrue(second.Indexed);

        context.ViewModel.UnindexAllCommand.Execute(null);

        Assert.IsFalse(first.Indexed);
        Assert.IsFalse(second.Indexed);
    }

    [TestMethod]
    public void DeleteRemovesTheSelectedSoundsAndSelectsTheFirstRemaining()
    {
        var context = new EditorContext();
        var first = context.AddSound(1);
        var second = context.AddSound(2);
        context.ViewModel.SelectedSounds = [second];
        context.ViewModel.SelectedSound = second;

        context.ViewModel.DeleteSoundsCommand.Execute(null);

        Assert.AreEqual(1, context.ViewModel.Sounds.Count);
        Assert.AreSame(first, context.ViewModel.SelectedSound);
    }

    [TestMethod]
    public void PlaybackModesExposeLegacyNamesAndHints()
    {
        Assert.AreEqual(4, LoopBehaviourOption.All.Count);
        Assert.AreEqual("Loop", LoopBehaviourOption.All[3].Name);
        Assert.AreEqual("Play in any case", LoopBehaviourOption.GetHint(WadSoundLoopBehaviour.None));
        Assert.AreEqual("Ignore if already playing", LoopBehaviourOption.GetHint(WadSoundLoopBehaviour.OneShotWait));
        Assert.AreEqual("Rewind if already playing", LoopBehaviourOption.GetHint(WadSoundLoopBehaviour.OneShotRewound));
        Assert.AreEqual("Loop until stopped by engine", LoopBehaviourOption.GetHint(WadSoundLoopBehaviour.Looped));
    }

    [TestMethod]
    public void ChangingThePlaybackModeUpdatesTheHint()
    {
        var sound = new SoundInfoViewModel(new WadSoundInfo(3));
        var changed = new List<string?>();
        sound.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        sound.LoopBehaviour = WadSoundLoopBehaviour.OneShotRewound;

        CollectionAssert.Contains(changed, nameof(SoundInfoViewModel.LoopBehaviourHint));
        Assert.AreEqual("Rewind if already playing", sound.LoopBehaviourHint);
    }
    private sealed class EditorContext
    {
        public MainViewModel ViewModel { get; }
        public TestDialogs Dialogs { get; } = new();
        public TestMessages Messages { get; } = new();

        public EditorContext() => ViewModel = new MainViewModel(new Configuration(), Dialogs, Messages);

        public SoundInfoViewModel AddSound(int id)
        {
            Dialogs.Prompts.Enqueue(id.ToString());
            ViewModel.AddSoundCommand.Execute(null);
            return ViewModel.SelectedSound ?? throw new AssertFailedException("The added sound was not selected.");
        }
    }

    private sealed class TestDialogs : ISoundToolDialogs
    {
        public Queue<string?> Prompts { get; } = new();
        public string? SavePath { get; set; }
        public int OpenRequests { get; private set; }
        public int SaveRequests { get; private set; }

        public string? OpenFile(string title, string filter, string? initialPath = null)
        {
            OpenRequests++;
            return null;
        }

        public string[] OpenSamples() => [];

        public string? SaveFile(string title, string filter, string? initialPath = null)
        {
            SaveRequests++;
            return SavePath;
        }

        public string? SelectFolder(string title, string? initialPath = null) => null;
        public string? Prompt(string title, string label, string value = "") => Prompts.Dequeue();
        public void ShowOptions(Configuration configuration) { }
        public void ShowAbout() { }
        public void Close() { }
    }

    private sealed class TestMessages : IMessageService
    {
        public int Answer { get; set; }
        public List<string> Errors { get; } = [];

        public void ShowInformation(string message, string caption = "Information") { }
        public void ShowError(string message, string caption = "Error") => Errors.Add(message);
        public bool ShowConfirmation(string message, string caption = "Are you sure?", bool? defaultValue = null, bool isRisky = false) => true;

        public T ShowConfirmation<T>(string message, string caption, T yesValue, T noValue,
            T? cancelValue = null, T? defaultValue = null, bool isRisky = false) where T : struct => (T)(object)Answer;
    }
}
