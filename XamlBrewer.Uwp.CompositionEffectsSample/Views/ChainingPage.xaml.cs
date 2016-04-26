using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition.Toolkit;
using System;
using System.Linq;
using System.Numerics;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

namespace XamlBrewer.Uwp.CompositionEffects
{
    public sealed partial class ChainingPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _leftRoot;
        private ContainerVisual _middleRoot;
        private ContainerVisual _rightRoot;
        private CompositionImageFactory _imageFactory;
        private SpriteVisual _leftSpriteVisual;
        private SpriteVisual _middleSpriteVisual;
        private SpriteVisual _rightSpriteVisual;
        private CompositionSurfaceBrush _imageBrush;
        private CompositionEffectBrush _effectBrush1;
        private CompositionEffectBrush _effectBrush2;
        private CompositionEffectBrush _combinedBrush;
        private IGraphicsEffect _firstEffect;
        private InvertEffect _combinedEffect;

        public ChainingPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize Composition UI infrastructure.
            _leftRoot = LeftContainer.GetVisual();
            _middleRoot = MiddleContainer.GetVisual();
            _rightRoot = RightContainer.GetVisual();
            _compositor = _leftRoot.Compositor;

            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);

            // Hook the sprite visuals into the XAML visual tree.
            _leftSpriteVisual = _compositor.CreateSpriteVisual();
            var side = (float)Math.Min(LeftPresenter.ActualWidth, LeftPresenter.ActualHeight);
            _leftSpriteVisual.Size = new Vector2(side, side);
            _leftRoot.Children.InsertAtTop(_leftSpriteVisual);
            _middleSpriteVisual = _compositor.CreateSpriteVisual();
            side = (float)Math.Min(MiddlePresenter.ActualWidth, MiddlePresenter.ActualHeight);
            _middleSpriteVisual.Size = new Vector2(side, side);
            _middleRoot.Children.InsertAtTop(_middleSpriteVisual);
            _rightSpriteVisual = _compositor.CreateSpriteVisual();
            side = (float)Math.Min(RightPresenter.ActualWidth, RightPresenter.ActualHeight);
            _rightSpriteVisual.Size = new Vector2(side, side);
            _rightRoot.Children.InsertAtTop(_rightSpriteVisual);

            // Create CompositionSurfaceBrush
            _imageBrush = _compositor.CreateSurfaceBrush();

            // Create an image source to load
            var imageSource = _imageFactory.CreateImageFromUri(new Uri("ms-appx:///Assets/flowers.jpg"));
            _imageBrush.Surface = imageSource.Surface;

            // Create and apply the first effect.
            _firstEffect = new SaturationEffect
            {
                Name = "firstEffect",
                Source = new CompositionEffectSourceParameter("source")
            };
            var firstEffectFactory = _compositor.CreateEffectFactory(_firstEffect, new[] { "firstEffect.Saturation" });
            _effectBrush1 = firstEffectFactory.CreateBrush();
            _leftSpriteVisual.Brush = _effectBrush1;

            // Create and apply the second effect.
            var secondEffect = new InvertEffect
            {
                Name = "secondEffect",
                Source = new CompositionEffectSourceParameter("source")
            };
            var secondEffectFactory = _compositor.CreateEffectFactory(secondEffect);
            _effectBrush2 = secondEffectFactory.CreateBrush();
            _middleSpriteVisual.Brush = _effectBrush2;

            // Create and apply the combined effect.
            _combinedEffect = new InvertEffect
            {
                Name = "chained",
                Source = _firstEffect
            };
            var combinedEffectFactory = _compositor.CreateEffectFactory(_combinedEffect, new[] { "firstEffect.Saturation" });
            _combinedBrush = combinedEffectFactory.CreateBrush();
            _rightSpriteVisual.Brush = _combinedBrush;

            // Chain the brushes
            _effectBrush1.SetSourceParameter("source", _imageBrush);
            _effectBrush2.SetSourceParameter("source", _imageBrush);
            _combinedBrush.SetSourceParameter("source", _imageBrush);
        }

        private void Presenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var presenter = sender as ContentPresenter;

            var side = (float)Math.Min(presenter.ActualWidth, presenter.ActualHeight);
            var containerVisual = ElementCompositionPreview.GetElementChildVisual(presenter.Content as UIElement) as ContainerVisual;

            if (containerVisual != null)
            {
                var spriteVisual = containerVisual.Children.First();
                spriteVisual.Size = new Vector2(side, side);
            }

            var horizontalMargin = (presenter.ActualWidth - side) / 2;
            var verticalMargin = (presenter.ActualHeight - side) / 2;
            if (horizontalMargin > 0 || verticalMargin > 0)
            {
                presenter.Padding = new Thickness(horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_effectBrush1 != null)
            {
                // Apply parameter to brushes.
                _effectBrush1.Properties.InsertScalar("firstEffect.Saturation", (float)e.NewValue);
                _combinedBrush.Properties.InsertScalar("firstEffect.Saturation", (float)e.NewValue);
            }
        }
    }
}
