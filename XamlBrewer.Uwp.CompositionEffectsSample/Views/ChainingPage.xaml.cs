using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            var firstEffect = new InvertEffect
            {
                Name = "firstEffect",
                Source = new CompositionEffectSourceParameter("source") 
            };
            var firstEffectFactory = _compositor.CreateEffectFactory(firstEffect);
            _effectBrush1 = firstEffectFactory.CreateBrush();
            _leftSpriteVisual.Brush = _effectBrush1;

            // Create and apply the second effect.
            var secondEffect = new SaturationEffect
            {
                Name = "secondEffect",
                Source = new CompositionEffectSourceParameter("source"),
                Saturation = 0
            };
            var secondEffectFactory = _compositor.CreateEffectFactory(secondEffect);
            _effectBrush2 = secondEffectFactory.CreateBrush();
            _middleSpriteVisual.Brush = _effectBrush2;

            // Create and apply the combined effect.
            var combinedEffect = new SaturationEffect
            {
                Name = "chained",
                Source = firstEffect,
                Saturation = 0
            };
            var combinedEffectFactory = _compositor.CreateEffectFactory(combinedEffect);
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
    }
}
