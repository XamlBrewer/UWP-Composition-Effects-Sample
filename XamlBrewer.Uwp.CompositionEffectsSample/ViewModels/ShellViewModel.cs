using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.CompositionEffects;

namespace Mvvm
{
    class ShellViewModel : ViewModelBase
    {
        public ShellViewModel()
        {
            // Build the menu
            // Symbol enumeration is here: https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.symbol.aspx
            Menu.Add(new MenuItem() { Glyph = Symbol.Rotate, Text = "Hue Rotation", NavigationDestination = typeof(HueRotationPage) });
        }
    }
}
