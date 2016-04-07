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
    public sealed partial class HueRotationPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private CompositionImageFactory _imageFactory;
        private SpriteVisual _spriteVisual;
        private CompositionEffectBrush _brush;

        public HueRotationPage()
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
            _spriteVisual = _compositor.CreateSpriteVisual();
            _root.Children.InsertAtTop(_spriteVisual);

            // Create the visual and add it to the composition tree
            var side = (float)Math.Min(Presenter.ActualWidth, Presenter.ActualHeight);
            _spriteVisual.Size = new Vector2(side, side);

            // Create the effect, but don't specify the Angle yet.
            var hueRotationEffect = new HueRotationEffect
            {
                Name = "hueRotation",
                Source = new CompositionEffectSourceParameter("source")
            };

            // Compile the effect
            var effectFactory = _compositor.CreateEffectFactory(hueRotationEffect, new[] { "hueRotation.Angle" });

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

            RotateHue(0);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Normalize to Angle value.
            float angle = (float)(Math.PI * 2 * e.NewValue / 100);

            RotateHue(angle);
        }

        private void RotateHue(float angle)
        {
            // Apply parameter to brush.
            _brush.Properties.InsertScalar("hueRotation.Angle", angle);
        }

        private void ColorWheel_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage(new Uri("ms-appx:///Assets/colorwheel.png"));
        }

        private void BlackAndWhite_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage(new Uri("ms-appx:///Assets/blackandwhitegradient.png"));
        }

        private void Saturation_Checked(object sender, RoutedEventArgs e)
        {
            LoadImage(new Uri("ms-appx:///Assets/saturationandbrightness.png"));
        }

        private void Presenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // This code actually belongs in a behavior, or in a custom control ("CompositionPanel").
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

    public static class UIElementExtensions
    {
        public static ContainerVisual GetVisual(this UIElement element)
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(element);
            var root = hostVisual.Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(element, root);
            return root;
        }
    }
}
