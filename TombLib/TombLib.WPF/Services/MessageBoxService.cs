using CustomMessageBox.WPF;
using System;
using TombLib.WPF.Services.Abstract;

namespace TombLib.WPF.Services;

public sealed class MessageBoxService : IMessageService
{
	public void ShowInformation(string message, string caption = "Information")
		=> CMessageBox.Show(message, caption, CMessageBoxButtons.OK, CMessageBoxIcon.Information);

	public void ShowError(string message, string caption = "Error")
		=> CMessageBox.Show(message, caption, CMessageBoxButtons.OK, CMessageBoxIcon.Error);

	public bool ShowConfirmation(string message, string caption = "Are you sure?", bool? defaultValue = null, bool isRisky = false)
		=> ShowConfirmation(message, caption, yesValue: true, noValue: false, cancelValue: null, defaultValue, isRisky);

	public T ShowConfirmation<T>(string message, string caption, T yesValue, T noValue, T? cancelValue = null, T? defaultValue = null, bool isRisky = false)
		where T : struct
	{
		var buttons = cancelValue == null ? CMessageBoxButtons.YesNo : CMessageBoxButtons.YesNoCancel;
		var icon = isRisky ? CMessageBoxIcon.Warning : CMessageBoxIcon.Question;

		var defaultButton = defaultValue switch
		{
			var v when v.Equals(yesValue) => CMessageBoxDefaultButton.Button1,
			var v when v.Equals(noValue) => CMessageBoxDefaultButton.Button2,
			var v when cancelValue != null && v.Equals(cancelValue) => CMessageBoxDefaultButton.Button3,
			_ => CMessageBoxDefaultButton.None
		};

		var result = CMessageBox.Show(message, caption, buttons, icon, defaultButton);

		return result switch
		{
			CMessageBoxResult.Yes => yesValue,
			CMessageBoxResult.No => noValue,
			_ => cancelValue ?? throw new InvalidOperationException("Cancel option is not available."),
		};
	}
}
