#nullable enable

namespace TombLib.Services;

/// <summary>
/// Describes DialogButtons values independently of the window framework.
/// </summary>
public enum DialogButtons
{
    OK = 0,
    OKCancel = 1,
    AbortRetryIgnore = 2,
    YesNoCancel = 3,
    YesNo = 4,
    RetryCancel = 5,
    CancelTryContinue = 6,
}
