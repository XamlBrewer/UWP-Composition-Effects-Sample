using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition.Toolkit;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

// More info here:
// https://msdn.microsoft.com/en-us/windows/uwp/graphics/composition-effects

namespace XamlBrewer.Uwp.CompositionEffects
{
    public sealed partial class TemperatureAndTintPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private CompositionImageFactory _imageFactory;
        private SpriteVisual _spriteVisual;
        private CompositionEffectBrush _brush;

        private string _temperatureParameter;

        public TemperatureAndTintPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize Composition UI infrastructure.
            _root = Container.GetVisual();
            _compositor = _root.Compositor;
            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);

            // Hook the sprite visual into the XAML visual tree.
            _spriteVisual = _compositor.CreateSpriteVisual();
            var side = (float)Math.Min(Presenter.ActualWidth, Presenter.ActualHeight);
            _spriteVisual.Size = new Vector2(side, side);
            _root.Children.InsertAtTop(_spriteVisual);

            // Create the effect, but don't specify the properties yet.
            var temperatureAndTintEffect = new TemperatureAndTintEffect
            {
                Name = "temperatureAndtint",
                Source = new CompositionEffectSourceParameter("source")
            };

            // Strongly typed version of the "temperatureAndtint.Temperature" string
            _temperatureParameter = temperatureAndTintEffect.Name + "." + nameof(temperatureAndTintEffect.Temperature);

            // Compile the effect
            var effectFactory = _compositor.CreateEffectFactory(
                temperatureAndTintEffect,
                new[] { _temperatureParameter, "temperatureAndtint.Tint" });

            // Create and apply the brush.
            _brush = effectFactory.CreateBrush();
            _spriteVisual.Brush = _brush;

            ColorWheelButton.IsChecked = true;
        }

        private void LoadImage(Uri uri)
        {
            // Create CompositionSurfaceBrush
            var surfaceBrush = _compositor.CreateSurfaceBrush();

            // Create an image source to load
            CompositionImage imageSource = _imageFactory.CreateImageFromUri(uri);
            surfaceBrush.Surface = imageSource.Surface;

            _brush.SetSourceParameter("source", surfaceBrush);

            ChangeTemperature(0);
        }

        private void Temperature_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ChangeTemperature((float)e.NewValue);
        }

        private void Tint_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ChangeTint((float)e.NewValue);
        }

        private void ChangeTemperature(float temperature)
        {
            // Apply parameter to brush.
            _brush.Properties.InsertScalar(_temperatureParameter, temperature);
        }

        private void ChangeTint(float tint)
        {
            // Apply parameter to brush.
            _brush.Properties.InsertScalar("temperatureAndtint.Tint", tint);
        }

        private void ColorWheel_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage(new Uri("ms-appx:///Assets/colorwheel.png"));
            Image_Changed();
        }

        private void White_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage(new Uri("ms-appx:///Assets/whitegradient.jpg"));
            Image_Changed();
        }

        private void Colorful_Checked(object sender, RoutedEventArgs e)
        {
            // LoadImage(new Uri("ms-appx:///Assets/flowers.jpg"));
            // LoadImage(new Uri("ms-appx:///Assets/people.jpg"));
            LoadImage(new Uri("ms-appx:///Assets/colorTheme.jpg"));
            Image_Changed();
        }

        private void Image_Changed()
        {
            TemperatureSlider.Value = 0;
            TintSlider.Value = 0;
        }

        private void Presenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var side = (float)Math.Min(Presenter.ActualWidth, Presenter.ActualHeight);
            if (_spriteVisual != null)
            {
                _spriteVisual.Size = new Vector2(side, side);
            }

            var horizontalMargin = (Presenter.ActualWidth - side) / 2;
            var verticalMargin = (Presenter.ActualHeight - side) / 2;
            if (horizontalMargin > 0 || verticalMargin > 0)
            {
                Presenter.Padding = new Thickness(horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
            }
        }
    }
}
