using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace XFFormsControls.Controls
{
    public enum GradientColorStyle
    {
        ColorsOnlyStyle,
        ColorsToDarkStyle,
        DarkToColorsStyle,
        ColorsToLightStyle,
        LightToColorsStyle,
        LightToColorsToDarkStyle,
        DarkToColorsToLightStyle
    }

    public enum ColorListDirection
    {
        Horizontal,
        Vertical
    }

    public class SKColorPicker : SKCanvasView
    {
        /// <summary>
        /// Occurs when the Picked Color changes
        /// </summary>
        public event EventHandler<Color> PickedColorChanged;

        public static readonly BindableProperty PickedColorProperty
            = BindableProperty.Create(
                nameof(PickedColor),
                typeof(Color),
                typeof(SKColorPicker));

        /// <summary>
        /// Get the current Picked Color
        /// </summary>
        public Color PickedColor
        {
            get { return (Color)GetValue(PickedColorProperty); }
            private set { SetValue(PickedColorProperty, value); }
        }


        public static readonly BindableProperty GradientColorStyleProperty
            = BindableProperty.Create(
                nameof(GradientColorStyle),
                typeof(GradientColorStyle),
                typeof(SKColorPicker),
                GradientColorStyle.ColorsToDarkStyle,
                BindingMode.OneTime, null);

        /// <summary>
        /// Set the Color Spectrum Gradient Style
        /// </summary>
        public GradientColorStyle GradientColorStyle
        {
            get { return (GradientColorStyle)GetValue(GradientColorStyleProperty); }
            set { SetValue(GradientColorStyleProperty, value); }
        }


        public static readonly BindableProperty ColorListProperty
            = BindableProperty.Create(
                nameof(ColorList),
                typeof(string[]),
                typeof(SKColorPicker),
                new string[]
                {
                    new Color(255, 0, 0).ToHex(), // Red
					new Color(255, 255, 0).ToHex(), // Yellow
					new Color(0, 255, 0).ToHex(), // Green (Lime)
					new Color(0, 255, 255).ToHex(), // Aqua
					new Color(0, 0, 255).ToHex(), // Blue
					new Color(255, 0, 255).ToHex(), // Fuchsia
					new Color(255, 0, 0).ToHex(), // Red
				},
                BindingMode.OneTime, null, propertyChanged: (b, o, n) =>
                {
                    ((SKColorPicker)b).RefillGradientColors();
                });

        /// <summary>
        /// Sets the Color List
        /// </summary>
        public string[] ColorList
        {
            get { return (string[])GetValue(ColorListProperty); }
            set { SetValue(ColorListProperty, value); }
        }


        public static readonly BindableProperty ColorListDirectionProperty
            = BindableProperty.Create(
                nameof(ColorListDirection),
                typeof(ColorListDirection),
                typeof(SKColorPicker),
                ColorListDirection.Horizontal,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Color List flow Direction
        /// </summary>
        public ColorListDirection ColorListDirection
        {
            get { return (ColorListDirection)GetValue(ColorListDirectionProperty); }
            set { SetValue(ColorListDirectionProperty, value); }
        }


        public static readonly BindableProperty PointerCircleDiameterUnitsProperty
            = BindableProperty.Create(
                nameof(PointerCircleDiameterUnits),
                typeof(double),
                typeof(SKColorPicker),
                0.6,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Picker Pointer Size
        /// Value must be between 0-1
        /// Calculated against the View Canvas size
        /// </summary>
        public double PointerCircleDiameterUnits
        {
            get { return (double)GetValue(PointerCircleDiameterUnitsProperty); }
            set { SetValue(PointerCircleDiameterUnitsProperty, value); }
        }


        public static readonly BindableProperty PointerCircleBorderUnitsProperty
            = BindableProperty.Create(
                nameof(PointerCircleBorderUnits),
                typeof(double),
                typeof(SKColorPicker),
                0.3,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Picker Pointer Border Size
        /// Value must be between 0-1
        /// Calculated against pixel size of Picker Pointer
        /// </summary>
        public double PointerCircleBorderUnits
        {
            get { return (double)GetValue(PointerCircleBorderUnitsProperty); }
            set { SetValue(PointerCircleBorderUnitsProperty, value); }
        }

        private SKColor[] gradientColorsArray;
        private SKPoint _lastTouchPoint = new SKPoint();

        private SKPaint sKPaint = new SKPaint()
        {
            IsAntialias = true,
        };

        private SKPaint paintTouchPoint = new SKPaint()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        public SKColorPicker()
        {
            EnableTouchEvents = true;
            VerticalOptions = LayoutOptions.Start;
            HorizontalOptions = LayoutOptions.Start;

            RefillGradientColors();
        }

        private void RefillGradientColors()
        {
            gradientColorsArray = ColorList.Select(c => Color.FromHex(c).ToSKColor()).ToArray();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo skImageInfo = e.Info;
            SKSurface skSurface = e.Surface;
            SKCanvas skCanvas = skSurface.Canvas;

            int skCanvasWidth = skImageInfo.Width;
            int skCanvasHeight = skImageInfo.Height;

            skCanvas.Clear(SKColors.White);

            // Draw gradient rainbow Color spectrum
            // create the gradient shader between Colors
            using (SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                ColorListDirection == ColorListDirection.Horizontal ?
                    new SKPoint(skCanvasWidth, 0) : new SKPoint(0, skCanvasHeight),
                gradientColorsArray,
                null,
                SKShaderTileMode.Clamp))
            {
                sKPaint.Shader = shader;
                skCanvas.DrawPaint(sKPaint);
            }

            // Draw darker gradient spectrum
            // create the gradient shader 
            using (SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                ColorListDirection == ColorListDirection.Horizontal ?
                    new SKPoint(0, skCanvasHeight) : new SKPoint(skCanvasWidth, 0),
                GetGradientOrder(),
                null,
                SKShaderTileMode.Clamp))
            {
                sKPaint.Shader = shader;
                skCanvas.DrawPaint(sKPaint);
            }

            // Picking the Pixel Color values on the Touch Point

            // Represent the color of the current Touch point
            SKColor touchPointColor;

            // Efficient and fast
            // https://forums.xamarin.com/discussion/92899/read-a-pixel-info-from-a-canvas
            // create the 1x1 bitmap (auto allocates the pixel buffer)
            using (SKBitmap bitmap = new SKBitmap(skImageInfo))
            {
                // get the pixel buffer for the bitmap
                IntPtr dstpixels = bitmap.GetPixels();

                // read the surface into the bitmap
                skSurface.ReadPixels(skImageInfo,
                    dstpixels,
                    skImageInfo.RowBytes,
                    (int)_lastTouchPoint.X, (int)_lastTouchPoint.Y);

                // access the color
                touchPointColor = bitmap.GetPixel(0, 0);
            }

            // Painting the Touch point
            paintTouchPoint.Color = SKColors.White;

            int valueToCalcAgainst = (skCanvasWidth > skCanvasHeight) ? skCanvasWidth : skCanvasHeight;

            double pointerCircleDiameterUnits = PointerCircleDiameterUnits; // 0.6 (Default)
            pointerCircleDiameterUnits = (float)pointerCircleDiameterUnits / 10f; //  calculate 1/10th of that value
            float pointerCircleDiameter = (float)(valueToCalcAgainst * pointerCircleDiameterUnits);

            // Outer circle of the Pointer (Ring)
            skCanvas.DrawCircle(
                _lastTouchPoint.X,
                _lastTouchPoint.Y,
                pointerCircleDiameter / 2, paintTouchPoint);

            // Draw another circle with picked color
            paintTouchPoint.Color = touchPointColor;

            double pointerCircleBorderWidthUnits = PointerCircleBorderUnits; // 0.3 (Default)
            float pointerCircleBorderWidth = (float)pointerCircleDiameter *
                                                    (float)pointerCircleBorderWidthUnits; // Calculate against Pointer Circle

            // Inner circle of the Pointer (Ring)
            skCanvas.DrawCircle(
                _lastTouchPoint.X,
                _lastTouchPoint.Y,
                ((pointerCircleDiameter - pointerCircleBorderWidth) / 2), paintTouchPoint);

            // Set selected color
            PickedColor = touchPointColor.ToFormsColor();
            PickedColorChanged?.Invoke(this, PickedColor);
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            if (e.InContact && _lastTouchPoint != e.Location)
            {
                _lastTouchPoint = e.Location;

                // Check for each touch point XY position to be inside Canvas
                // Ignore any Touch event ocurred outside the Canvas region 
                if ((e.Location.X > 0 && e.Location.X < CanvasSize.Width) &&
                    (e.Location.Y > 0 && e.Location.Y < CanvasSize.Height))
                {
                    e.Handled = true;

                    // update the Canvas as you wish
                    InvalidateSurface();
                }
            }
        }

        private SKColor[] GetGradientOrder()
        {
            switch (GradientColorStyle)
            {
                case GradientColorStyle.ColorsOnlyStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.Transparent
                        };
                    }
                case GradientColorStyle.ColorsToDarkStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.Transparent,
                            SKColors.Black
                        };
                    }
                case GradientColorStyle.DarkToColorsStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.Black,
                            SKColors.Transparent
                        };
                    }
                case GradientColorStyle.ColorsToLightStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.Transparent,
                            SKColors.White
                        };
                    }
                case GradientColorStyle.LightToColorsStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.White,
                            SKColors.Transparent
                        };
                    }
                case GradientColorStyle.LightToColorsToDarkStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.White,
                            SKColors.Transparent,
                            SKColors.Black
                        };
                    }
                case GradientColorStyle.DarkToColorsToLightStyle:
                    {
                        return new SKColor[]
                        {
                            SKColors.Black,
                            SKColors.Transparent,
                            SKColors.White
                        };
                    }
                default:
                    {
                        return new SKColor[]
                        {
                            SKColors.Transparent,
                            SKColors.Black
                        };
                    }
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            double size = Math.Min(widthConstraint, heightConstraint);
            return new SizeRequest(new Size(size, size), new Size(40.0, 40.0));
        }
    }
}
