using System.Windows.Media;
using TombLib.WPF.Features.ColorPicker;

namespace TombLib.Test.ViewModels;

[TestClass]
public class ColorPickerViewModelTests
{
    [TestMethod]
    public void SelectingCustomColorsPreservesThePaletteUntilExplicitlySaved()
    {
        var viewModel = new ColorPickerViewModel(Colors.White, [Colors.Red, Colors.Green, Colors.Blue]);

        viewModel.SelectedSlot = 1;
        viewModel.SelectedColor = Colors.Yellow;

        Assert.AreEqual(Colors.Green, viewModel.CustomColors[1].Color);
        viewModel.SelectedSlot = 2;
        Assert.AreEqual(Colors.Blue, viewModel.SelectedColor);
        Assert.AreEqual(Colors.Green, viewModel.CustomColors[1].Color);
    }

    [TestMethod]
    public void SavingCustomColorOnlyReplacesTheSelectedSlot()
    {
        var viewModel = new ColorPickerViewModel(Colors.White, [Colors.Red, Colors.Green, Colors.Blue]);
        viewModel.SelectedSlot = 1;
        viewModel.SelectedColor = Colors.Yellow;

        viewModel.SaveCustomCommand.Execute(null);

        Assert.AreEqual(Colors.Red, viewModel.CustomColors[0].Color);
        Assert.AreEqual(Colors.Yellow, viewModel.CustomColors[1].Color);
        Assert.AreEqual(Colors.Blue, viewModel.CustomColors[2].Color);
        Assert.AreEqual(Colors.Yellow, viewModel.SelectedColor);
    }
}
