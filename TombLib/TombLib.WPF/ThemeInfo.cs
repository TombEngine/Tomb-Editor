using System.Windows;

// This class library hosts WPF windows (e.g. AnimatedTexturesWindow). Without ThemeInfo, WPF does
// not establish the theme/resource context for this assembly's visuals, which (among other things)
// makes ComboBox dropdown popups lose their placement reference and open in the top-left screen
// corner. Declaring it fixes that.
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,        // no theme-specific (Aero/Luna/...) resource dictionaries
    ResourceDictionaryLocation.SourceAssembly)] // generic resource dictionary lives in this assembly
