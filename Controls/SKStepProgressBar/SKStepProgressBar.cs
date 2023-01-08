using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace XFFormsControls.Controls
{
    public enum StepMode
    {
        None,
        Add,
        Remove
    };

    public class SKStepProgressBar : SKCanvasView, IDisposable
    {
        public enum StepState
        {
            None,
            Finished,
            Skipped,
            Disabled
        }

        internal sealed class StepItemHandler : BindableObject, IDisposable
        {
            internal struct CircleHandler
            {
                public SKPoint Center;
                public float Radius;
                public CircleHandler(SKPoint center, float radius)
                {
                    Center = center;
                    Radius = radius;
                }

                public bool IsWithinBounds(float x, float y)
                {
                    var distance = Math.Sqrt((Math.Pow(Center.X - x, 2) + Math.Pow(Center.Y - y, 2)));

                    return distance <= Radius;
                }

                public bool IsWithinBounds(SKPoint p)
                {
                    return IsWithinBounds(p.X, p.Y);
                }
            }

            #region Properties

            #region Bindable properties
            public static readonly BindableProperty CaptionProperty =
                BindableProperty.Create(nameof(Caption), typeof(string), typeof(StepItemHandler), null, propertyChanged: (b, o, n) =>
                {
                    ((StepItemHandler)b).OnCaptionPropertyChanged((string)o, (string)n);
                });

            public static readonly BindableProperty StateProperty =
                BindableProperty.Create(nameof(State), typeof(StepState), typeof(StepItemHandler), null, propertyChanged: (b, o, n) =>
                {
                    ((StepItemHandler)b).OnStatePropertyChanged((StepState)o, (StepState)n);
                });

            public static readonly BindableProperty CanBeDeletedProperty =
                BindableProperty.Create(nameof(CanBeDeleted), typeof(bool), typeof(StepItemHandler), true, propertyChanged: (b, o, n) =>
                {
                    ((StepItemHandler)b).OnCanBeDeletedPropertyChanged((bool)o, (bool)n);
                });

            public static readonly BindableProperty IsMandatoryProperty =
                BindableProperty.Create(nameof(IsMandatory), typeof(bool), typeof(StepItemHandler), false, propertyChanged: (b, o, n) =>
                {
                    ((StepItemHandler)b).OnIsMandatoryPropertyChanged((bool)o, (bool)n);
                });
            #endregion

            public string Caption
            {
                get => (string)GetValue(CaptionProperty);
                set => SetValue(CaptionProperty, value);
            }
            public StepState State
            {
                get => (StepState)GetValue(StateProperty);
                set => SetValue(StateProperty, value);
            }
            public bool CanBeDeleted
            {
                get => (bool)GetValue(CanBeDeletedProperty);
                set => SetValue(CanBeDeletedProperty, value);
            }
            public bool IsMandatory
            {
                get => (bool)GetValue(IsMandatoryProperty);
                set => SetValue(IsMandatoryProperty, value);
            }

            public bool IsCompleted => State == StepState.Finished;
            public bool IsSkipped => State == StepState.Skipped;
            public bool IsDisabled => State == StepState.Disabled;

            private string _captionBindingPath = null;
            public string CaptionBindingPath
            {
                get => _captionBindingPath;
                set
                {
                    _captionBindingPath = value;
                    if (string.IsNullOrEmpty(_captionBindingPath))
                    {
                        RemoveBinding(CaptionProperty);
                    }
                    else
                    {
                        SetBinding(CaptionProperty, new Binding()
                        {
                            Source = Item,
                            Path = _captionBindingPath
                        });
                    }
                    _stepProgressBar?.RequestInvalidateSurface();
                }
            }

            private IValueConverter _captionPropertyConverter;
            public IValueConverter CaptionPropertyConverter
            {
                get => _captionPropertyConverter;
                set
                {
                    if (value != _captionPropertyConverter)
                    {
                        _captionPropertyConverter = value;

                        if (!string.IsNullOrEmpty(CaptionBindingPath))
                        {
                            SetBinding(CaptionProperty, new Binding()
                            {
                                Source = Item,
                                Path = CaptionBindingPath,
                                Converter = _captionPropertyConverter
                            });

                            _stepProgressBar?.RequestInvalidateSurface();
                        }
                    }
                }
            }

            private string _canBeDeletedBindingPath = null;
            public string CanBeDeletedBindingPath
            {
                get => _canBeDeletedBindingPath;
                set
                {
                    _canBeDeletedBindingPath = value;
                    if (string.IsNullOrEmpty(_canBeDeletedBindingPath))
                    {
                        RemoveBinding(CanBeDeletedProperty);
                    }
                    else
                    {
                        SetBinding(CanBeDeletedProperty, new Binding()
                        {
                            Source = Item,
                            Path = _canBeDeletedBindingPath,
                            Converter = CanBeDeletedConverter
                        });
                    }
                    _stepProgressBar?.RequestInvalidateSurface();
                }
            }

            private IValueConverter _canBeDeletedConverter;
            public IValueConverter CanBeDeletedConverter
            {
                get => _canBeDeletedConverter;
                set
                {
                    if (value != _canBeDeletedConverter)
                    {
                        _canBeDeletedConverter = value;

                        if (!string.IsNullOrEmpty(CanBeDeletedBindingPath))
                        {
                            SetBinding(CanBeDeletedProperty, new Binding()
                            {
                                Source = Item,
                                Path = CanBeDeletedBindingPath,
                                Converter = _canBeDeletedConverter
                            });

                            _stepProgressBar?.RequestInvalidateSurface();
                        }
                    }
                }
            }

            private string _isMandatoryBindingPath = null;
            public string IsMandatoryBindingPath
            {
                get => _isMandatoryBindingPath;
                set
                {
                    _isMandatoryBindingPath = value;
                    if (string.IsNullOrEmpty(_isMandatoryBindingPath))
                    {
                        RemoveBinding(IsMandatoryProperty);
                    }
                    else
                    {
                        SetBinding(IsMandatoryProperty, new Binding()
                        {
                            Source = Item,
                            Path = _isMandatoryBindingPath,
                            Converter = IsMandatoryConverter
                        });
                    }
                    _stepProgressBar?.RequestInvalidateSurface();
                }
            }

            private IValueConverter _isMandatoryConverter;
            public IValueConverter IsMandatoryConverter
            {
                get => _isMandatoryConverter;
                set
                {
                    if (value != _isMandatoryConverter)
                    {
                        _isMandatoryConverter = value;

                        if (!string.IsNullOrEmpty(IsMandatoryBindingPath))
                        {
                            SetBinding(IsMandatoryProperty, new Binding()
                            {
                                Source = Item,
                                Path = IsMandatoryBindingPath,
                                Converter = _isMandatoryConverter
                            });

                            _stepProgressBar?.RequestInvalidateSurface();
                        }
                    }
                }
            }

            private bool isHovered = false;
            public bool IsHovered
            {
                get => isHovered;
                set
                {
                    if (isHovered != value)
                    {
                        if (value && !IsClickable())
                        {
                            return;
                        }
                        isHovered = value;
                        _stepProgressBar?.RequestInvalidateSurface();
                    }
                }
            }
            #endregion

            internal SKStepProgressBar _stepProgressBar;

            public SKPoint Center;
            public SKRect Bounds;
            private CircleHandler _addStepHandler;
            public object Item { get; private set; }
            public int Index;

            public StepItemHandler(object item)
            {
                Item = item;
            }

            public void Draw(SKCanvas canvas)
            {
                SKColor textColor = SKColors.Black;
                SKColor captionColor = SKColors.Black;

                var text = (Index + 1).ToString();

                switch (State)
                {
                    case StepState.Disabled:
                        {
                            _stepProgressBar.circleBorder.Color = _stepProgressBar.disabledColor;
                            _stepProgressBar.circleFill.Color = _stepProgressBar.disabledColor;
                            break;
                        }
                    case StepState.Finished:
                        {
                            _stepProgressBar.circleBorder.Color = _stepProgressBar.successColor;
                            _stepProgressBar.circleFill.Color = _stepProgressBar.successFillColor;
                            textColor = _stepProgressBar.successTextColor;
                            captionColor = _stepProgressBar.successCaptionColor;

                            break;
                        }
                    case StepState.Skipped:
                        {
                            if (IsHovered && _stepProgressBar.Mode == StepMode.None)
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.successColor;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.successFillColor;
                                textColor = _stepProgressBar.successTextColor;
                                captionColor = _stepProgressBar.successCaptionColor;
                            }
                            else if (IsMandatory)
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.skipMandatoryColor;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.skipMandatoryFillColor;
                                textColor = _stepProgressBar.skipMandatoryTextColor;
                                text = "!";
                            }
                            else
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.skipColor;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.skipFillColor;
                                textColor = _stepProgressBar.skipTextColor;
                            }
                            captionColor = _stepProgressBar.successCaptionColor;
                            break;
                        }
                    default:
                        {
                            if (_stepProgressBar.hoverIndex > Index && _stepProgressBar.Mode == StepMode.None)
                            {
                                goto case StepState.Skipped;
                            }

                            if (isHovered &&  _stepProgressBar.Mode == StepMode.None)
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.successColor;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.successFillColor;
                                textColor = _stepProgressBar.successTextColor;
                                captionColor = _stepProgressBar.successCaptionColor;
                            }
                            else
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.color;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.fillColor;
                                textColor = _stepProgressBar.textColor;
                                captionColor = _stepProgressBar.captionColor;
                            }
                            if (_stepProgressBar.Mode == StepMode.Remove && State == StepState.None && IsDeleteAvailable())
                            {
                                _stepProgressBar.circleBorder.Color = _stepProgressBar.color;
                                _stepProgressBar.circleFill.Color = _stepProgressBar.color;
                                textColor = _stepProgressBar.deleteTextColor;
                            }
                            break;
                        }
                }

                if (!_stepProgressBar.IsEnabled)
                {
                    _stepProgressBar.circleBorder.Color = _stepProgressBar.disabledColor;
                    _stepProgressBar.circleFill.Color = _stepProgressBar.disabledColor.WithAlpha(_stepProgressBar.circleFill.Color.Alpha);
                    var color = _stepProgressBar.circleFill.Color;

                    if (color.Alpha > 128)
                    {
                        var luma = 0.2126 * color.Red + 0.7152 * color.Green + 0.0722 * color.Blue;
                        if (luma > 128)
                        {
                            textColor = SKColors.Black;
                        }
                        else
                        {
                            textColor = SKColors.White;
                        }
                    }
                    else
                    {
                        textColor = _stepProgressBar.disabledColor;
                    }

                    captionColor = _stepProgressBar.disabledColor;
                }

                canvas.DrawCircle(Center, _stepProgressBar.Radius, _stepProgressBar.circleBorder);
                canvas.DrawCircle(Center, _stepProgressBar.Radius, _stepProgressBar.circleFill);

                if (IsHovered && IsClickable() && _stepProgressBar.Mode != StepMode.Add)
                {
                    canvas.DrawCircle(Center, _stepProgressBar.Radius, _stepProgressBar.circleHover);
                }

                SKRect textBounds = new SKRect();

                if (_stepProgressBar.Mode == StepMode.Remove && State == StepState.None && IsDeleteAvailable())
                {
                    text = "_";
                    _stepProgressBar.textPaint.Color = textColor;
                    _stepProgressBar.textPaint.TextSize = _stepProgressBar._stepSequenceTextSize;
                    var textWidth = _stepProgressBar.textPaint.MeasureText(text, ref textBounds);
                    var textHeight = textBounds.Size.Height;

                    canvas.DrawText(text, Center.X - textWidth / 2, Center.Y + textHeight / 2, _stepProgressBar.textPaint);
                }
                else if (_stepProgressBar.hoverIndex == Index && _stepProgressBar.SelectedIndex > Index && (State == StepState.Finished || State == StepState.Skipped) && _stepProgressBar.Mode == StepMode.None)
                {
                    _stepProgressBar.svgPaint.Color = textColor;
                    SvgImage.DrawImage(canvas, _stepProgressBar.svgBack, _stepProgressBar.svgPaint, Bounds, _stepProgressBar.Radius / 2);
                }
                else if (_stepProgressBar.hoverIndex == Index && _stepProgressBar.SelectedIndex < Index && State == StepState.None && _stepProgressBar.Mode == StepMode.None)
                {
                    _stepProgressBar.svgPaint.Color = textColor;
                    SvgImage.DrawImage(canvas, _stepProgressBar.svgCheck, _stepProgressBar.svgPaint, Bounds, _stepProgressBar.Radius / 2);
                }
                else
                {
                    _stepProgressBar.textPaint.Color = textColor;
                    _stepProgressBar.textPaint.TextSize = _stepProgressBar._stepSequenceTextSize;
                    var textWidth = _stepProgressBar.textPaint.MeasureText(text, ref textBounds);
                    var textHeight = textBounds.Size.Height;

                    canvas.DrawText(text, Center.X - textWidth / 2, Center.Y + textHeight / 2, _stepProgressBar.textPaint);
                }


                text = Caption;
                if (!string.IsNullOrEmpty(text))
                {
                    _stepProgressBar.textPaint.TextSize = _stepProgressBar._stepCaptionTextSize;
                    _stepProgressBar.textPaint.Color = captionColor;

                    var y = Center.Y + _stepProgressBar.Radius + _stepProgressBar.textPaint.FontSpacing + 2f;
                    int index = 0;
                    do
                    {
                        _stepProgressBar.textPaint.BreakText(text.Substring(index), Bounds.Width, out float textWidth, out string textToPrint);

                        canvas.DrawText(textToPrint, Center.X - textWidth / 2, y, _stepProgressBar.textPaint);

                        index += textToPrint.Length;

                        y += _stepProgressBar.textPaint.FontSpacing;
                    }
                    while (index < text.Length);
                }
            }

            public void DrawPipeline(SKCanvas canvas, StepItemHandler item)
            {
                var color = SKColors.White;
                switch (State)
                {
                    case StepState.Disabled:
                        {
                            color = _stepProgressBar.disabledColor;

                            break;
                        }
                    case StepState.Finished:
                        {
                            color = _stepProgressBar.successColor;

                            break;
                        }
                    case StepState.Skipped:
                        {
                            color = _stepProgressBar.skipColor;
                            break;
                        }
                    default:
                        {
                            if (_stepProgressBar.Mode == StepMode.None)
                            {
                                if (!_stepProgressBar.IsEnabled)
                                {
                                    goto case StepState.Disabled;
                                }
                                if (_stepProgressBar.hoverIndex > Index)
                                {
                                    goto case StepState.Skipped;
                                }
                                if (_stepProgressBar.hoverIndex == Index)
                                {
                                    goto case StepState.Finished;
                                }
                            }
                            color = _stepProgressBar.color;
                            break;
                        }
                }

                if (!_stepProgressBar.IsEnabled)
                {
                    color = _stepProgressBar.disabledColor;
                }

                _stepProgressBar.pathStroke.Color = color;

                var firstPoint = new SKPoint(item.Center.X + _stepProgressBar.Radius, item.Center.Y);
                var secondPoint = new SKPoint(Center.X - _stepProgressBar.Radius, Center.Y);

                canvas.DrawLine(firstPoint, secondPoint, _stepProgressBar.pathStroke);

                if (_stepProgressBar.Mode == StepMode.Add && IsNewStepAvailable())
                {
                    var radius = _stepProgressBar.Radius / 2;
                    var diameter = radius * 2;
                    var center = new SKPoint((firstPoint.X + secondPoint.X) / 2, (firstPoint.Y + secondPoint.Y) / 2);
                    _stepProgressBar.circleFill.Color = _stepProgressBar.newStepColor;
                    canvas.DrawCircle(center, radius, _stepProgressBar.circleFill);

                    if (IsHovered && IsClickable())
                    {
                        canvas.DrawCircle(center, radius, _stepProgressBar.circleHover);
                    }

                    _stepProgressBar.textPaint.Color = _stepProgressBar.newStepTextColor;
                    _stepProgressBar.textPaint.TextSize = _stepProgressBar._stepSequenceTextSize;

                    SKRect textBounds = new SKRect();

                    var text = "+";
                    _stepProgressBar.textPaint.MeasureText(text, ref textBounds);

                    canvas.DrawText(text, center.X - Math.Abs(textBounds.MidX), center.Y + Math.Abs(textBounds.MidY), _stepProgressBar.textPaint);

                    _addStepHandler = new CircleHandler(center, radius);
                }
            }

            public void OnSizeAllocated(SKRect rect)
            {
                Bounds = rect;
                Center = new SKPoint(rect.MidX, rect.MidY);
            }

            #region Methods

            public bool IsClickable()
            {
                if (IsDisabled || !_stepProgressBar.IsEnabled || _stepProgressBar.SelectedIndex == Index)
                {
                    return false;
                }

                switch (_stepProgressBar.Mode)
                {
                    case StepMode.Add:
                        {
                            return IsNewStepAvailable();
                        }
                    case StepMode.Remove:
                        {
                            return IsDeleteAvailable() && State == StepState.None;
                        }
                    default:
                        {
                            return (_stepProgressBar.Command?.CanExecute(Item) ?? true);
                        }
                }
            }

            public bool IsNewStepAvailable()
            {
                return _stepProgressBar.SelectedIndex < Index && _stepProgressBar.IsEnabled && (_stepProgressBar.NewStepCommand?.CanExecute(Index) ?? true);
            }

            public bool IsDeleteAvailable()
            {
                return CanBeDeleted && _stepProgressBar.SelectedIndex < Index && _stepProgressBar.IsEnabled && (_stepProgressBar.DeleteCommand?.CanExecute(Item) ?? true);
            }

            public void Clicked()
            {
                if (!IsClickable())
                {
                    return;
                }

                switch (_stepProgressBar.Mode)
                {
                    case StepMode.None:
                        {
                            if (_stepProgressBar.SelectedIndex != Index)
                            {
                                _stepProgressBar.Command?.Execute(Item);
                            }
                            break;
                        }
                    case StepMode.Add:
                        {
                            if (IsNewStepAvailable())
                            {
                                _stepProgressBar.NewStepCommand?.Execute(Item);
                            }

                            break;
                        }
                    case StepMode.Remove:
                        {
                            if (IsDeleteAvailable())
                            {
                                _stepProgressBar.DeleteCommand?.Execute(Item);
                            }

                            break;
                        }
                }
            }

            public bool IsWithinRadius(float x, float y)
            {
                if (_stepProgressBar.Mode == StepMode.Add)
                {
                    if (!IsNewStepAvailable())
                    {
                        return false;
                    }

                    return _addStepHandler.IsWithinBounds(x, y);
                }
                
                var distance = Math.Sqrt((Math.Pow(Center.X - x, 2) + Math.Pow(Center.Y - y, 2)));

                return distance <= _stepProgressBar.Radius;
            }

            public bool IsWithinRadius(SKPoint p)
            {
                return IsWithinRadius(p.X, p.Y);
            }
            #endregion

            #region Property Changes
            private void OnCaptionPropertyChanged(string oldValue, string newValue)
            {
                _stepProgressBar?.RequestInvalidateSurface();
            }

            private void OnStatePropertyChanged(StepState o, StepState n)
            {
                _stepProgressBar?.RequestInvalidateSurface();
            }

            private void OnCanBeDeletedPropertyChanged(bool o, bool n)
            {
                _stepProgressBar?.RequestInvalidateSurface();
            }

            private void OnIsMandatoryPropertyChanged(bool o, bool n)
            {
                _stepProgressBar?.RequestInvalidateSurface();
            }
            #endregion

            public void Dispose()
            {
                RemoveBinding(CaptionProperty);
                RemoveBinding(StateProperty);
                RemoveBinding(CanBeDeletedProperty);
                CaptionPropertyConverter = null;
                CanBeDeletedConverter = null;
                _stepProgressBar = null;
                Item = null;
            }
        }

        internal sealed class StepItemCollection : Collection<StepItemHandler>, IDisposable
        {
            public double ContentWidth { get; set; } = 0;
            public double ContentHeight { get; set; } = 0;

            internal SKStepProgressBar _stepProgressBar;

            public StepItemCollection(SKStepProgressBar stepProgressBar)
            {
                _stepProgressBar = stepProgressBar;
            }

            public void MeasureAll()
            {
                var widthSegment = Math.Max((float)_stepProgressBar.Width / Count, _stepProgressBar.Diameter * 1.5f);

                float x = 0;
                foreach (var item in this)
                {
                    item.OnSizeAllocated(SKRect.Create(x, _stepProgressBar.circleBorder.StrokeWidth, widthSegment, _stepProgressBar.Diameter));
                    x += widthSegment;
                }
            }

            public void Draw(SKCanvas canvas)
            {
                for (int i = 1; i < Count; i++)
                {
                    var prevItem = this[i - 1];
                    var item = this[i];
                    
                    item.DrawPipeline(canvas, prevItem);
                }
                foreach (var item in this)
                {
                    item.Draw(canvas);
                }
            }

            #region Overrides
            protected override void ClearItems()
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
                base.ClearItems();

                ContentWidth = 0;
                ContentHeight = 0;
            }

            protected override void InsertItem(int index, StepItemHandler item)
            {
                if (Contains(item))
                {
                    throw new InvalidOperationException("Duplicate");
                }

                double width = 0;
                if (Count > 0)
                {
                    width = 40;
                }

                for (int i = index; i < Count; i++)
                {
                    var oldItem = this[i];
                    oldItem.Index = oldItem.Index + 1;
                }

                item.Index = index;
                item._stepProgressBar = _stepProgressBar;
                base.InsertItem(index, item);

                ContentWidth += _stepProgressBar.Diameter + width;
                ContentHeight = Math.Max(_stepProgressBar.Diameter, ContentHeight);
            }

            protected override void RemoveItem(int index)
            {
                if (Count > 0)
                {
                    ContentWidth -= _stepProgressBar.Diameter;
                    ContentHeight = Math.Max(_stepProgressBar.Diameter, ContentHeight);
                }
                else
                {
                    ContentWidth = 0;
                    ContentHeight = 10;
                }

                for (int i = index; i < Count; i++)
                {
                    var oldItem = this[i];
                    oldItem.Index = oldItem.Index - 1;
                }

                var item = this[index];
                item.Dispose();

                base.RemoveItem(index);
            }

            protected override void SetItem(int index, StepItemHandler item)
            {
                var oldItem = this[index];
                if (oldItem != item)
                {
                    oldItem.Dispose();

                    ContentWidth -= _stepProgressBar.Diameter;

                    item.Index = index;
                    base.SetItem(index, item);

                    ContentWidth += _stepProgressBar.Diameter;
                    ContentHeight = Math.Max(_stepProgressBar.Diameter, ContentHeight);
                }
            }
            #endregion

            public void Dispose()
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
                Clear();
                _stepProgressBar = null;
            }
        }

        #region Properties

        #region Color bindable properties
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color), typeof(Color), typeof(SKStepProgressBar), Color.FromRgb(128, 144, 160), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty NewStepColorProperty =
            BindableProperty.Create(nameof(NewStepColor), typeof(Color), typeof(SKStepProgressBar), Color.FromRgb(128, 144, 160), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnNewStepColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty NewStepTextColorProperty =
            BindableProperty.Create(nameof(NewStepTextColor), typeof(Color), typeof(SKStepProgressBar), Color.White, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnNewStepTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty FillColorProperty =
            BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(SKStepProgressBar), Color.Transparent, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnFillColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(SKStepProgressBar), Color.FromRgb(128, 144, 160), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty CaptionColorProperty =
            BindableProperty.Create(nameof(CaptionColor), typeof(Color), typeof(SKStepProgressBar), Color.FromRgb(128, 144, 160), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCaptionColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SuccessColorProperty =
            BindableProperty.Create(nameof(SuccessColor), typeof(Color), typeof(SKStepProgressBar), Color.LimeGreen, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSuccessColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SuccessFillColorProperty =
            BindableProperty.Create(nameof(SuccessFillColor), typeof(Color), typeof(SKStepProgressBar), Color.LimeGreen, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSuccessFillColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SuccessTextColorProperty =
            BindableProperty.Create(nameof(SuccessTextColor), typeof(Color), typeof(SKStepProgressBar), Color.White, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSuccessTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SuccessCaptionColorProperty =
            BindableProperty.Create(nameof(SuccessCaptionColor), typeof(Color), typeof(SKStepProgressBar), Color.LimeGreen, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSuccessCaptionColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty DeleteTextColorProperty =
            BindableProperty.Create(nameof(DeleteTextColor), typeof(Color), typeof(SKStepProgressBar), Color.White, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnDeleteTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipColorProperty =
            BindableProperty.Create(nameof(SkipColor), typeof(Color), typeof(SKStepProgressBar), Color.LimeGreen, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipFillColorProperty =
            BindableProperty.Create(nameof(SkipFillColor), typeof(Color), typeof(SKStepProgressBar), Color.Transparent, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipFillColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipTextColorProperty =
            BindableProperty.Create(nameof(SkipTextColor), typeof(Color), typeof(SKStepProgressBar), Color.LimeGreen, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipMandatoryColorProperty =
            BindableProperty.Create(nameof(SkipMandatoryColor), typeof(Color), typeof(SKStepProgressBar), Color.Red, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipMandatoryColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipMandatoryFillColorProperty =
            BindableProperty.Create(nameof(SkipMandatoryFillColor), typeof(Color), typeof(SKStepProgressBar), Color.Transparent, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipMandatoryFillColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty SkipMandatoryTextColorProperty =
            BindableProperty.Create(nameof(SkipMandatoryTextColor), typeof(Color), typeof(SKStepProgressBar), Color.Red, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSkipMandatoryTextColorChanged((Color)o, (Color)n);
            });

        public static readonly BindableProperty DisabledColorProperty =
            BindableProperty.Create(nameof(DisabledColor), typeof(Color), typeof(SKStepProgressBar), Color.Gray, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnDisabledColorChanged((Color)o, (Color)n);
            });
        #endregion

        #region Binding bindable path properties
        public static readonly BindableProperty CaptionPropertyProperty =
            BindableProperty.Create(nameof(CaptionProperty), typeof(string), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCaptionPropertyChanged((string)o, (string)n);
            });

        public static readonly BindableProperty CaptionPropertyConverterProperty =
            BindableProperty.Create(nameof(CaptionPropertyConverter), typeof(IValueConverter), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCaptionPropertyConverterPropertyChanged((IValueConverter)o, (IValueConverter)n);
            });

        public static readonly BindableProperty CanBeDeletedPropertyProperty =
            BindableProperty.Create(nameof(CanBeDeletedProperty), typeof(string), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCanBeDeletedPropertyChanged((string)o, (string)n);
            });

        public static readonly BindableProperty CanBeDeletedPropertyConverterProperty =
            BindableProperty.Create(nameof(CanBeDeletedPropertyConverter), typeof(IValueConverter), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCanBeDeletedPropertyConverterPropertyChanged((IValueConverter)o, (IValueConverter)n);
            });

        public static readonly BindableProperty IsMandatoryPropertyProperty =
            BindableProperty.Create(nameof(IsMandatoryProperty), typeof(string), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnIsMandatoryPropertyChanged((string)o, (string)n);
            });

        public static readonly BindableProperty IsMandatoryPropertyConverterProperty =
            BindableProperty.Create(nameof(IsMandatoryPropertyConverter), typeof(IValueConverter), typeof(SKStepProgressBar), null, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnIsMandatoryPropertyConverterPropertyChanged((IValueConverter)o, (IValueConverter)n);
            });
        #endregion

        #region Etc bindable
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SKStepProgressBar), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnItemsSourceChanged((IEnumerable)o, (IEnumerable)n);
            });

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SKStepProgressBar), -1, BindingMode.TwoWay, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSelectedIndexChanged((int)o, (int)n);
            });

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SKStepProgressBar), -1, BindingMode.TwoWay, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnSelectedItemChanged(o, n);
            });

        public static readonly BindableProperty ModeProperty =
            BindableProperty.Create(nameof(Mode), typeof(StepMode), typeof(SKStepProgressBar), StepMode.None, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnModeChanged((StepMode)o, (StepMode)n);
            });

        public static readonly BindableProperty RadiusProperty =
            BindableProperty.Create(nameof(Radius), typeof(float), typeof(SKStepProgressBar), 32f, propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnRadiusChanged((float)o, (float)n);
            });
        #endregion

        #region Bindable commands
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SKStepProgressBar), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnCommandChanged((ICommand)o, (ICommand)n);
            });

        public static readonly BindableProperty NewStepCommandProperty =
            BindableProperty.Create(nameof(NewStepCommand), typeof(ICommand), typeof(SKStepProgressBar), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnNewStepCommandChanged((ICommand)o, (ICommand)n);
            });

        public static readonly BindableProperty DeleteCommandProperty =
            BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(SKStepProgressBar), propertyChanged: (b, o, n) =>
            {
                ((SKStepProgressBar)b).OnDeleteCommandChanged((ICommand)o, (ICommand)n);
            });
        #endregion

        #region Color properties
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public Color NewStepColor
        {
            get => (Color)GetValue(NewStepColorProperty);
            set => SetValue(NewStepColorProperty, value);
        }

        public Color NewStepTextColor
        {
            get => (Color)GetValue(NewStepTextColorProperty);
            set => SetValue(NewStepTextColorProperty, value);
        }

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        public Color SuccessColor
        {
            get => (Color)GetValue(SuccessColorProperty);
            set => SetValue(SuccessColorProperty, value);
        }

        public Color SuccessFillColor
        {
            get => (Color)GetValue(SuccessFillColorProperty);
            set => SetValue(SuccessFillColorProperty, value);
        }

        public Color CaptionColor
        {
            get => (Color)GetValue(CaptionColorProperty);
            set => SetValue(CaptionColorProperty, value);
        }

        public Color SuccessCaptionColor
        {
            get => (Color)GetValue(SuccessCaptionColorProperty);
            set => SetValue(SuccessCaptionColorProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public Color DeleteTextColor
        {
            get => (Color)GetValue(DeleteTextColorProperty);
            set => SetValue(DeleteTextColorProperty, value);
        }

        public Color SuccessTextColor
        {
            get => (Color)GetValue(SuccessTextColorProperty);
            set => SetValue(SuccessTextColorProperty, value);
        }

        public Color SkipColor
        {
            get => (Color)GetValue(SkipColorProperty);
            set => SetValue(SkipColorProperty, value);
        }

        public Color SkipFillColor
        {
            get => (Color)GetValue(SkipFillColorProperty);
            set => SetValue(SkipFillColorProperty, value);
        }

        public Color SkipTextColor
        {
            get => (Color)GetValue(SkipTextColorProperty);
            set => SetValue(SkipTextColorProperty, value);
        }

        public Color SkipMandatoryColor
        {
            get => (Color)GetValue(SkipMandatoryColorProperty);
            set => SetValue(SkipMandatoryColorProperty, value);
        }

        public Color SkipMandatoryFillColor
        {
            get => (Color)GetValue(SkipMandatoryFillColorProperty);
            set => SetValue(SkipMandatoryFillColorProperty, value);
        }

        public Color SkipMandatoryTextColor
        {
            get => (Color)GetValue(SkipMandatoryTextColorProperty);
            set => SetValue(SkipMandatoryTextColorProperty, value);
        }

        public Color DisabledColor
        {
            get => (Color)GetValue(DisabledColorProperty);
            set => SetValue(DisabledColorProperty, value);
        }
        #endregion

        #region Bindable path properties
        public string CaptionProperty
        {
            get => (string)GetValue(CaptionPropertyProperty);
            set => SetValue(CaptionPropertyProperty, value);
        }

        public IValueConverter CaptionPropertyConverter
        {
            get => (IValueConverter)GetValue(CaptionPropertyConverterProperty);
            set => SetValue(CaptionPropertyConverterProperty, value);
        }

        public string CanBeDeletedProperty
        {
            get => (string)GetValue(CanBeDeletedPropertyProperty);
            set => SetValue(CanBeDeletedPropertyProperty, value);
        }

        public IValueConverter CanBeDeletedPropertyConverter
        {
            get => (IValueConverter)GetValue(CanBeDeletedPropertyConverterProperty);
            set => SetValue(CanBeDeletedPropertyConverterProperty, value);
        }

        public string IsMandatoryProperty
        {
            get => (string)GetValue(IsMandatoryPropertyProperty);
            set => SetValue(IsMandatoryPropertyProperty, value);
        }

        public IValueConverter IsMandatoryPropertyConverter
        {
            get => (IValueConverter)GetValue(IsMandatoryPropertyConverterProperty);
            set => SetValue(IsMandatoryPropertyConverterProperty, value);
        }
        #endregion

        #region Etc
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => (object)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public StepMode Mode
        {
            get => (StepMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public float Radius
        {
            get => (float)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public float Diameter
        {
            get;
            private set;
        }
        #endregion

        #region Commands
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public ICommand NewStepCommand
        {
            get => (ICommand)GetValue(NewStepCommandProperty);
            set => SetValue(NewStepCommandProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }
        #endregion
        #endregion

        #region Skia paints & properties
        // create the paint for the filled circle
        internal SKPaint circleFill = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue
        };

        // create the paint for the circle border
        internal SKPaint circleBorder = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 2
        };

        internal SKPaint circleHover = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 0, 0, 50)
        };

        // create the paint for the path
        internal SKPaint pathStroke = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 10
        };

        // create the paint for the path
        internal SKPaint svgPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 20
        };

        internal SKPaint textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName(null, (int)SKFontStyleWeight.Normal, (int)SKFontStyleWidth.SemiExpanded, SKFontStyleSlant.Upright),
            TextSize = 14F,
            //LcdRenderText=true,
        };

        internal float _stepSequenceTextSize = 14f;
        internal float _stepCaptionTextSize = 14f;

        internal SKColor color;
        internal SKColor newStepColor;
        internal SKColor newStepTextColor;
        internal SKColor fillColor;
        internal SKColor textColor;
        internal SKColor deleteTextColor;
        internal SKColor captionColor;
        internal SKColor successCaptionColor;
        internal SKColor successColor;
        internal SKColor successFillColor;
        internal SKColor successTextColor;
        internal SKColor hoverColor = new SKColor(0, 0, 0, 50);
        internal SKColor skipColor;
        internal SKColor skipFillColor;
        internal SKColor skipTextColor;
        internal SKColor skipMandatoryColor;
        internal SKColor skipMandatoryFillColor;
        internal SKColor skipMandatoryTextColor;
        internal SKColor disabledColor;
        #endregion

        internal StepItemCollection _items;

        internal int hoverIndex = -1;

        SvgImage svgBack;
        SvgImage svgCheck;

        protected float xscale = (float)Device.Info.ScalingFactor;
        protected float yscale = (float)Device.Info.ScalingFactor;
        private int _batchInvalidation = 0;
        private bool invalidationRequested = false;

        public SKStepProgressBar()
        {
            _items = new StepItemCollection(this);

            HorizontalOptions = LayoutOptions.FillAndExpand;
            EnableTouchEvents = true;
            //IgnorePixelScaling = true;

            Diameter = Radius * 2;
            svgPaint.StrokeWidth = Radius / 2;

            svgBack = SvgImage.GetCache(typeof(SKStepProgressBar).Assembly, "back");
            svgCheck = SvgImage.GetCache(typeof(SKStepProgressBar).Assembly, "check");

            _stepSequenceTextSize = (float)Device.GetNamedSize(NamedSize.Title, typeof(Label));
            _stepCaptionTextSize = (float)Device.GetNamedSize(NamedSize.Caption, typeof(Label));

            color = Color.ToSKColor();
            newStepColor = NewStepColor.ToSKColor();
            newStepTextColor = NewStepTextColor.ToSKColor();
            fillColor = FillColor.ToSKColor();
            textColor = TextColor.ToSKColor();
            deleteTextColor = DeleteTextColor.ToSKColor();
            captionColor = CaptionColor.ToSKColor();
            successCaptionColor = SuccessCaptionColor.ToSKColor();
            successColor = SuccessColor.ToSKColor();
            successFillColor = SuccessFillColor.ToSKColor();
            successTextColor = SuccessTextColor.ToSKColor();
            skipColor = SkipColor.ToSKColor();
            skipFillColor = SkipFillColor.ToSKColor();
            skipTextColor = SkipTextColor.ToSKColor();
            skipMandatoryColor = SkipMandatoryColor.ToSKColor();
            skipMandatoryFillColor = SkipMandatoryFillColor.ToSKColor();
            skipMandatoryTextColor = SkipMandatoryTextColor.ToSKColor();
            disabledColor = DisabledColor.ToSKColor();
        }


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var surface = e.Surface;

            var canvas = surface.Canvas;
            xscale = (float)(e.Info.Width / Width);
            yscale = (float)(e.Info.Height / Height);

            canvas.Scale(xscale, yscale);

            canvas.Clear();

            _items.Draw(canvas);

            //StepItemHandler prevItem = null;
            //foreach (var item in _items)
            //{
            //    var _hoveredIndex = _hoveredItem.Index;

            //    if (prevItem != null)
            //    {
            //        if (SelectedIndex >= item.Index)
            //        {
            //            pathStroke.Color = successColor;
            //        }
            //        else if (item.Index <= _hoveredIndex)
            //        {
            //            pathStroke.Color = skipColor;
            //        }
            //        else
            //        {
            //            pathStroke.Color = color;
            //        }
            //        canvas.DrawLine(new SKPoint(item.Center.X - Radius, item.Center.Y), new SKPoint(prevItem.Center.X + Radius, prevItem.Center.Y), pathStroke);
            //    }

            //    bool fill = true;
            //    if (SelectedIndex >= item.Index || item.Index == _hoveredIndex)
            //    {
            //        circleBorder.Color = circleFill.Color = successColor;
            //        textPaint.Color = successTextColor;

            //    }
            //    else if (item.Index < _hoveredIndex)
            //    {
            //        circleBorder.Color = circleFill.Color = skipColor;
            //        textPaint.Color = skipTextColor;
            //    }
            //    else
            //    {
            //        fill = false;
            //        circleBorder.Color = color;
            //        textPaint.Color = textColor;
            //    }

            //    if (fill)
            //    {
            //        canvas.DrawCircle(item.Center, Radius, circleFill);
            //    }
            //    canvas.DrawCircle(item.Center, Radius, circleBorder);

            //    if (item.Index == _hoveredIndex)
            //    {
            //        circleFill.Color = hoverColor;
            //        canvas.DrawCircle(item.Center, Radius, circleFill);
            //    }

            //    SKRect textBounds = new SKRect();

            //    var text = (item.Index + 1).ToString();
            //    textPaint.TextSize = _stepSequenceTextSize;
            //    var textWidth = textPaint.MeasureText(text, ref textBounds);
            //    var textHeight = textBounds.Size.Height;

            //    canvas.DrawText(text, item.Center.X - textWidth / 2, item.Center.Y + textHeight / 2, textPaint);



            //    text = item.Item.ToString();
            //    textPaint.TextSize = _stepDescriptionTextSize;
            //    if (SelectedIndex >= item.Index)
            //    {
            //        textPaint.Color = successCaptionColor;
            //    }
            //    else
            //    {
            //        textPaint.Color = captionColor;
            //    }
            //    textWidth = textPaint.MeasureText(text, ref textBounds);
            //    textHeight = textBounds.Size.Height;
            //    canvas.DrawText(text, item.Center.X - textWidth / 2, item.Center.Y + textHeight + Radius + 10, textPaint);

            //    prevItem = item;
            //}
        }

        StepItemHandler _pressedItem = null;

        protected override void OnTouch(SKTouchEventArgs e)
        {
            //base.OnTouch(e);

            if (!IsEnabled)
            {
                return;
            }

            switch (e.ActionType)
            {
                case SKTouchAction.Moved:
                    {
                        if (!e.InContact)
                        {
                            CheckHover(e.Location);
                        }
                        break;
                    }
                case SKTouchAction.Pressed:
                    {
                        foreach (var item in _items)
                        {
                            if (item.IsWithinRadius(e.Location.X / xscale, e.Location.Y / yscale))
                            {
                                _pressedItem = item;
                                e.Handled = true;
                                break;
                            }
                        }
                        break;
                    }
                case SKTouchAction.Released:
                    {
                        BatchInvalidationBegin();

                        CheckHover(e.Location);
                        if (_pressedItem != null && _pressedItem.IsWithinRadius(e.Location.X / xscale, e.Location.Y / yscale))
                        {
                            e.Handled = true;

                            _pressedItem.Clicked();
                        }

                        _pressedItem = null;

                        BatchInvalidationCommit();

                        break;
                    }
            }
        }

        private void CheckHover(SKPoint p)
        {
            BatchInvalidationBegin();

            hoverIndex = -1;

            foreach (var item in _items)
            {
                var isHovered = item.IsWithinRadius(p.X / xscale, p.Y / yscale);

                if (isHovered != item.IsHovered)
                {
                    item.IsHovered = isHovered;
                    if (isHovered)
                    {
                        hoverIndex = item.Index;
                    }
                }
            }

            BatchInvalidationCommit();
        }

        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldCol)
            {
                oldCol.CollectionChanged -= ItemSource_CollectionChanged;
            }
            if (newValue is INotifyCollectionChanged newCol)
            {
                newCol.CollectionChanged += ItemSource_CollectionChanged;
            }

            BatchInvalidationBegin();

            _items.Clear();
            if (!(newValue is null))
            {
                foreach (var item in newValue)
                {
                    var handler = new StepItemHandler(item)
                    {
                        CaptionBindingPath = CaptionProperty,
                        CanBeDeletedBindingPath = CanBeDeletedProperty,
                        IsMandatoryBindingPath = IsMandatoryProperty,
                    };
                    _items.Add(handler);
                }
            }
            _items.MeasureAll();

            RequestInvalidateSurface();
            BatchInvalidationCommit();
        }

        private void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        BatchInvalidationBegin();

                        var index = e.NewStartingIndex;
                        foreach (var item in e.NewItems)
                        {
                            _items.Insert(index, new StepItemHandler(item)
                            {
                                CaptionBindingPath = CaptionProperty,
                                CanBeDeletedBindingPath = CanBeDeletedProperty,
                                IsMandatoryBindingPath = IsMandatoryProperty,
                            });
                            index++;
                        }

                        RequestInvalidateSurface();
                        BatchInvalidationCommit();
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        BatchInvalidationBegin();

                        foreach (var item in e.OldItems)
                        {
                            var handler = _items.FirstOrDefault(c => c.Item == item);
                            if (handler != null)
                            {
                                _items.Remove(handler);
                            }
                        }

                        RequestInvalidateSurface();
                        BatchInvalidationCommit();
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        BatchInvalidationBegin();
                        _items.Clear();
                        RequestInvalidateSurface();
                        BatchInvalidationCommit();
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        BatchInvalidationBegin();

                        _items.Clear();
                        foreach (var item in ItemsSource)
                        {
                            var handler = new StepItemHandler(item)
                            {
                                CaptionBindingPath = CaptionProperty,
                                CanBeDeletedBindingPath = CanBeDeletedProperty,
                                IsMandatoryBindingPath = IsMandatoryProperty,
                            };
                            _items.Add(handler);
                        }
                        _items.MeasureAll();

                        RequestInvalidateSurface();
                        BatchInvalidationCommit();

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        BatchInvalidationBegin();

                        _items.Clear();
                        foreach (var item in ItemsSource)
                        {
                            var handler = new StepItemHandler(item)
                            {
                                CaptionBindingPath = CaptionProperty,
                                CanBeDeletedBindingPath = CanBeDeletedProperty,
                                IsMandatoryBindingPath = IsMandatoryProperty,
                            };
                            _items.Add(handler);
                        }
                        _items.MeasureAll();

                        RequestInvalidateSurface();
                        BatchInvalidationCommit();

                        break;
                    }
            }
            _items.MeasureAll();
        }

        #region Property Changes

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsEnabled))
            {
                BatchInvalidationBegin();
                Mode = StepMode.None;
                if (!IsEnabled)
                {
                    _pressedItem = null;
                    hoverIndex = -1;
                }
                RequestInvalidateSurface();
                BatchInvalidationCommit();
            }
        }

        #region Color changes
        protected void OnColorChanged(Color oldValue, Color newValue)
        {
            color = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnNewStepColorChanged(Color oldValue, Color newValue)
        {
            newStepColor = newValue.ToSKColor();
            if (Mode == StepMode.Add)
            {
                RequestInvalidateSurface();
            }
        }

        private void OnNewStepTextColorChanged(Color oldValue, Color newValue)
        {
            newStepTextColor = newValue.ToSKColor();
            if (Mode == StepMode.Add)
            {
                RequestInvalidateSurface();
            }
        }

        private void OnFillColorChanged(Color oldValue, Color newValue)
        {
            fillColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnSuccessColorChanged(Color oldValue, Color newValue)
        {
            successColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnSuccessFillColorChanged(Color oldValue, Color newValue)
        {
            successFillColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnTextColorChanged(Color oldValue, Color newValue)
        {
            textColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnDeleteTextColorChanged(Color oldValue, Color newValue)
        {
            deleteTextColor = newValue.ToSKColor();
            if (Mode == StepMode.Remove)
            {
                RequestInvalidateSurface();
            }
        }

        protected void OnSuccessTextColorChanged(Color oldValue, Color newValue)
        {
            successTextColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnSkipColorChanged(Color oldValue, Color newValue)
        {
            skipColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnSkipFillColorChanged(Color oldValue, Color newValue)
        {
            skipFillColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnSkipTextColorChanged(Color oldValue, Color newValue)
        {
            skipTextColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnSkipMandatoryColorChanged(Color oldValue, Color newValue)
        {
            skipMandatoryColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnSkipMandatoryFillColorChanged(Color oldValue, Color newValue)
        {
            skipMandatoryFillColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnSkipMandatoryTextColorChanged(Color oldValue, Color newValue)
        {
            skipMandatoryTextColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnCaptionColorChanged(Color oldValue, Color newValue)
        {
            captionColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        protected void OnSuccessCaptionColorChanged(Color oldValue, Color newValue)
        {
            successCaptionColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }

        private void OnDisabledColorChanged(Color oldValue, Color newValue)
        {
            disabledColor = newValue.ToSKColor();
            RequestInvalidateSurface();
        }
        #endregion

        #region Bindable paths changes

        protected void OnCaptionPropertyChanged(string oldValue, string newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.CaptionBindingPath = newValue;
            }
            BatchInvalidationCommit();
        }

        private void OnCaptionPropertyConverterPropertyChanged(IValueConverter oldValue, IValueConverter newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.CaptionPropertyConverter = newValue;
            }
            BatchInvalidationCommit();
        }

        private void OnCanBeDeletedPropertyChanged(string oldValue, string newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.CanBeDeletedBindingPath = newValue;
            }
            BatchInvalidationCommit();
        }

        private void OnCanBeDeletedPropertyConverterPropertyChanged(IValueConverter oldValue, IValueConverter newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.CanBeDeletedConverter = newValue;
            }
            BatchInvalidationCommit();
        }

        private void OnIsMandatoryPropertyChanged(string oldValue, string newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.IsMandatoryBindingPath = newValue;
            }
            BatchInvalidationCommit();
        }

        private void OnIsMandatoryPropertyConverterPropertyChanged(IValueConverter oldValue, IValueConverter newValue)
        {
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                item.IsMandatoryConverter = newValue;
            }
            BatchInvalidationCommit();
        }
        #endregion

        #region Etc property changes
        private void OnIsReadOnlyChanged(bool o, bool n)
        {
            RequestInvalidateSurface();
        }

        protected void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            if (newValue > _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(SelectedIndex), SelectedIndex, "SelectedIndex is out of range");
            }
            BatchInvalidationBegin();
            foreach (var item in _items)
            {
                if (item.Index < newValue)
                {
                    if (item.State == StepState.None)
                    {
                        item.State = StepState.Skipped;
                    }
                }
                else if (item.Index == newValue)
                {
                    item.State = StepState.Finished;
                    SelectedItem = item.Item;
                }
                else
                {
                    item.State = StepState.None;
                }
            }
            BatchInvalidationCommit();
        }

        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            var item = _items.FirstOrDefault(c => c.Item == newValue);
            var index = _items.IndexOf(item);

            if (index != SelectedIndex)
            {
                SelectedIndex = index;
            }
        }

        protected void OnModeChanged(StepMode oldValue, StepMode newValue)
        {
            RequestInvalidateSurface();
        }

        protected void OnRadiusChanged(float oldValue, float newValue)
        {
            Diameter = newValue * 2;
            svgPaint.StrokeWidth = newValue / 2;

            _items.MeasureAll();
            RequestInvalidateSurface();
        }
        #endregion

        #region Commands changes
        private void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= Command_CanExecuteChanged;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += Command_CanExecuteChanged;
            }
        }
        private void OnNewStepCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= NewStepCommand_CanExecuteChanged;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += NewStepCommand_CanExecuteChanged;
            }
        }

        private void OnDeleteCommandChanged(ICommand oldValue, ICommand newValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= DeleteCommand_CanExecuteChanged;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += DeleteCommand_CanExecuteChanged;
            }
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            if (Mode == StepMode.None)
            {
                RequestInvalidateSurface();
            }
        }

        private void NewStepCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            if (Mode == StepMode.Add)
            {
                RequestInvalidateSurface();
            }
        }

        private void DeleteCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            if (Mode == StepMode.Remove)
            {
                RequestInvalidateSurface();
            }
        }
        #endregion
        #endregion

        #region Measurements
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            _items.MeasureAll();
            RequestInvalidateSurface();
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return new SizeRequest(new Size(Math.Max(widthConstraint, _items.ContentWidth), _items.ContentHeight));
        }
        #endregion

        #region Batch processes
        protected void BatchInvalidationBegin()
        {
            _batchInvalidation++;
        }

        protected void BatchInvalidationCommit()
        {
            _batchInvalidation--;
            if (_batchInvalidation < 0)
            {
                throw new InvalidOperationException("Batch out of commit");
            }
            else if (_batchInvalidation == 0 && invalidationRequested)
            {
                invalidationRequested = false;
                InvalidateSurface();
            }
        }

        public void RequestInvalidateSurface()
        {
            if (_batchInvalidation == 0)
            {
                InvalidateSurface();
            }
            else
            {
                invalidationRequested = true;
            }
        }
        #endregion

        public void Dispose()
        {
            _items.Dispose();
            _items = null;
        }
    }
}
