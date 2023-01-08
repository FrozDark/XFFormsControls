using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    [ContentProperty(nameof(InputView))]
    public class TextInputLayout : Layout, IPaddingElement
    {
        public static readonly BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(TextInputLayout), propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).HintTextPropertyChanged((string)o, (string)n);
        });
        public static readonly BindableProperty HintScaleProperty = BindableProperty.Create(nameof(HintScale), typeof(double), typeof(TextInputLayout), 0.9);
        public static readonly BindableProperty HintLabelStyleProperty = BindableProperty.Create(nameof(HintLabelStyle), typeof(TextInputLayoutLabelStyle), typeof(TextInputLayout), new TextInputLayoutLabelStyle() { FontAttributes = FontAttributes.Bold }, propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).StyleChangedProperty((TextInputLayoutLabelStyle)o, (TextInputLayoutLabelStyle)n);
        });
        public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(nameof(HelperText), typeof(string), typeof(TextInputLayout), propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).HelperTextPropertyChanged((string)o, (string)n);
        });
        public static readonly BindableProperty HelperTextColorProperty = BindableProperty.Create(nameof(HelperTextColor), typeof(Color), typeof(TextInputLayout), Color.Gray, propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).HelperTextColorPropertyChanged((Color)o, (Color)n);
        });
        public static readonly BindableProperty HelperLabelStyleProperty = BindableProperty.Create(nameof(HelperLabelStyle), typeof(TextInputLayoutLabelStyle), typeof(TextInputLayout), new TextInputLayoutLabelStyle() {  FontSize = 11 }, propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).StyleChangedProperty((TextInputLayoutLabelStyle)o, (TextInputLayoutLabelStyle)n);
        });
        public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(TextInputLayout), propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).ErrorTextPropertyChanged((string)o, (string)n);
        });
        public static readonly BindableProperty ErrorTextColorProperty = BindableProperty.Create(nameof(ErrorTextColor), typeof(Color), typeof(TextInputLayout), Color.Red, propertyChanged: (b, o, n) =>
        {
            ((TextInputLayout)b).ErrorTextColorPropertyChanged((Color)o, (Color)n);
        });
        public static readonly BindableProperty FocusedColorProperty = BindableProperty.Create(nameof(FocusedColor), typeof(Color), typeof(TextInputLayout), Color.FromHex("#00AFA0"));
        public static readonly BindableProperty UnfocusedColorProperty = BindableProperty.Create(nameof(UnfocusedColor), typeof(Color), typeof(TextInputLayout), Color.Silver);

        private void HintTextPropertyChanged(string oldValue, string newValue)
        {
            _hintTextLabel.Value.Text = newValue;
        }

        private void HelperTextPropertyChanged(string oldValue, string newValue)
        {
            UpdateHelperText();
        }

        private void HelperTextColorPropertyChanged(Color oldValue, Color newValue)
        {
            UpdateHelperText();
        }

        private void ErrorTextPropertyChanged(string oldValue, string newValue)
        {
            UpdateHelperText();
        }

        private void ErrorTextColorPropertyChanged(Color oldValue, Color newValue)
        {
            UpdateHelperText();
        }

        private void UpdateHelperText()
        {
            if (string.IsNullOrEmpty(ErrorText))
            {
                _helperTextLabel.Value.Text = HelperText;
                _helperTextLabel.Value.TextColor = HelperTextColor;
            }
            else
            {
                _helperTextLabel.Value.Text = ErrorText;
                _helperTextLabel.Value.TextColor = ErrorTextColor;
            }
        }

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public TextInputLayoutLabelStyle HintLabelStyle
        {
            get => (TextInputLayoutLabelStyle)GetValue(HintLabelStyleProperty);
            set => SetValue(HintLabelStyleProperty, value);
        }

        public double HintScale
        {
            get => (double)GetValue(HintScaleProperty);
            set => SetValue(HintScaleProperty, value);
        }

        public string HelperText
        {
            get => (string)GetValue(HelperTextProperty);
            set => SetValue(HelperTextProperty, value);
        }

        public Color HelperTextColor
        {
            get => (Color)GetValue(HelperTextColorProperty);
            set => SetValue(HelperTextColorProperty, value);
        }

        public TextInputLayoutLabelStyle HelperLabelStyle
        {
            get => (TextInputLayoutLabelStyle)GetValue(HelperLabelStyleProperty);
            set => SetValue(HelperLabelStyleProperty, value);
        }

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        public Color ErrorTextColor
        {
            get => (Color)GetValue(ErrorTextColorProperty);
            set => SetValue(ErrorTextColorProperty, value);
        }

        public Color FocusedColor
        {
            get => (Color)GetValue(FocusedColorProperty);
            set => SetValue(FocusedColorProperty, value);
        }
        public Color UnfocusedColor
        {
            get => (Color)GetValue(UnfocusedColorProperty);
            set => SetValue(UnfocusedColorProperty, value);
        }

        private ObservableCollection<Element> ChildsInternal { get => (ObservableCollection<Element>)Children; }

        private InputView _inputView = null;
        private Lazy<Label> _hintTextLabel;
        private Lazy<Label> _helperTextLabel;
        private Lazy<Label> _textLengthLabel;

        public InputView InputView
        {
            get => _inputView;
            set
            {
                SetInputView(value);
            }
        }

        public TextInputLayout()
        {
            _hintTextLabel = new Lazy<Label>(() =>
            {
                Label label = new Label()
                {
                    InputTransparent = true,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.NoWrap
                };
                label.SetBinding(Label.FontAttributesProperty, new Binding()
                {
                    Path = $"{nameof(HintLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontAttributes)}",
                    Source = this
                });
                label.SetBinding(Label.FontFamilyProperty, new Binding()
                {
                    Path = $"{nameof(HintLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontFamily)}",
                    Source = this
                });
                label.SetBinding(Label.FontSizeProperty, new Binding()
                {
                    Path = $"{nameof(HintLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontSize)}",
                    Source = this
                });

                if (_inputView is null)
                {
                    label.TextColor = UnfocusedColor;
                }
                else if (_inputView.IsFocused)
                {
                    label.TextColor = FocusedColor.IsDefault ? _inputView.TextColor : FocusedColor;
                }
                else
                {
                    label.TextColor = UnfocusedColor.IsDefault ? _inputView.PlaceholderColor : UnfocusedColor;
                }

                return label;
            });

            _helperTextLabel = new Lazy<Label>(() =>
            {
                Label label = new Label()
                {
                    InputTransparent = true,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.NoWrap
                };
                label.SetBinding(Label.FontAttributesProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontAttributes)}",
                    Source = this
                });
                label.SetBinding(Label.FontFamilyProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontFamily)}",
                    Source = this
                });
                label.SetBinding(Label.FontSizeProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontSize)}",
                    Source = this
                });

                return label;
            });

            _textLengthLabel = new Lazy<Label>(() =>
            {
                Label label = new Label()
                {
                    InputTransparent = true,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.NoWrap,
                    TextColor = Color.Gray
                };
                label.SetBinding(Label.FontAttributesProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontAttributes)}",
                    Source = this
                });
                label.SetBinding(Label.FontFamilyProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontFamily)}",
                    Source = this
                });
                label.SetBinding(Label.FontSizeProperty, new Binding()
                {
                    Path = $"{nameof(HelperLabelStyle)}.{nameof(TextInputLayoutLabelStyle.FontSize)}",
                    Source = this
                });
                //label.SetBinding(Label.TextColorProperty, new Binding()
                //{
                //    Path = nameof(HelperTextColor),
                //    Source = this
                //});

                return label;
            });
        }

        private void SetInputView(InputView view)
        {
            if (view is null)
            {
                throw new ArgumentNullException(nameof(view), "InputView can not be null");
            }

            if (_inputView != null)
            {
                _inputView.TextChanged -= InputView_TextChanged;
                _inputView.Focused -= InputView_Focused;
                _inputView.Unfocused -= InputView_Unfocused;
                _ = ChildsInternal.Remove(_inputView);
            }
            else
            {
                ChildsInternal.Add(_hintTextLabel.Value);
                ChildsInternal.Add(_helperTextLabel.Value);
                ChildsInternal.Add(_textLengthLabel.Value);
            }

            _inputView = view;
            _textLengthLabel.Value.IsVisible = _inputView.MaxLength < int.MaxValue;
            ChildsInternal.Insert(0, _inputView);

            _inputView.TextChanged += InputView_TextChanged;
            _inputView.Focused += InputView_Focused;
            _inputView.Unfocused += InputView_Unfocused;
            _inputView.PropertyChanged += InputView_PropertyChanged;

            InvalidateLayout();
            UpdateHintFloat();
            UpdateTextLengthText();
        }

        private void InputView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InputView.MaxLength))
            {
                _textLengthLabel.Value.IsVisible = InputView.MaxLength < int.MaxValue;
                UpdateTextLengthText();
            }
        }

        private void InputView_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateHintFloat();
            UpdateTextLengthText();
        }

        private void UpdateTextLengthText()
        {
            _textLengthLabel.Value.Text = $"{_inputView?.Text?.Length ?? 0} / {_inputView?.MaxLength ?? 0}";
        }

        private void UpdateHintFloat()
        {
            if (_inputView is null || _inputView.IsFocused)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_inputView.Text))
            {
                _hintTextLabel.Value.TranslateTo(-_hintTextLabel.Value.X, -_hintTextLabel.Value.Height * HintScale);
                _hintTextLabel.Value.ScaleTo(HintScale);
            }
            else
            {
                _hintTextLabel.Value.TranslateTo(0, 0);
                _hintTextLabel.Value.ScaleTo(1);
            }
        }

        private void InputView_Focused(object sender, FocusEventArgs e)
        {
            _hintTextLabel.Value.TextColor = FocusedColor.IsDefault ? _inputView.TextColor : FocusedColor;
            _hintTextLabel.Value.TranslateTo(-_hintTextLabel.Value.X, -_hintTextLabel.Value.Height * HintScale);
            _hintTextLabel.Value.ScaleTo(HintScale);
        }

        private void InputView_Unfocused(object sender, FocusEventArgs e)
        {
            _hintTextLabel.Value.TextColor = UnfocusedColor.IsDefault ? _inputView.TextColor : UnfocusedColor;
            if (string.IsNullOrWhiteSpace(_inputView.Text))
            {
                _hintTextLabel.Value.TranslateTo(0, 0);
                _hintTextLabel.Value.ScaleTo(1);
            }
        }

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return false;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return false;
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (_inputView is null)
            {
                return base.OnMeasure(widthConstraint, heightConstraint);
            }

            double width = Math.Min(Device.Info.ScaledScreenSize.Width, widthConstraint);

            //if (_inputView.HorizontalOptions.Alignment == LayoutAlignment.Fill && _inputView.VerticalOptions.Alignment == LayoutAlignment.Fill)
            //{
            //    return new SizeRequest(new Size(width, Math.Min(Device.Info.ScaledScreenSize.Height, heightConstraint)));
            //}

            SizeRequest size = _inputView.Measure(widthConstraint, heightConstraint);

            double height = Math.Min(size.Request.Height, heightConstraint);

            if (!string.IsNullOrEmpty(HelperText))
            {
                SizeRequest _helperSize = _helperTextLabel.Value.Measure(widthConstraint, heightConstraint);
                height += _helperSize.Request.Height;
            }

            if (!string.IsNullOrEmpty(Hint))
            {
                SizeRequest _hintSize = _hintTextLabel.Value.Measure(widthConstraint, heightConstraint);
                height += _hintSize.Request.Height * HintScale;
            }

            SizeRequest resulted = new SizeRequest(new Size(
                width, 
                Math.Min(height, heightConstraint)
            ));

            return resulted;
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (_inputView != null)
            {
                SizeRequest size = _inputView.Measure(width, height);

                double hintHeight = 0;
                {
                    Label _hintLabel = _hintTextLabel.Value;
                    SizeRequest _hintSize = _hintLabel.Measure(width, height, MeasureFlags.IncludeMargins);
                    hintHeight = _hintSize.Request.Height * HintScale;

                    _hintLabel.Layout(new Rectangle(x + 10, y + hintHeight, _hintSize.Request.Width, Math.Min(size.Request.Height, _hintSize.Request.Height)));
                }
                {
                    Label _helperLabel = _helperTextLabel.Value;
                    SizeRequest _helperSize = _helperLabel.Measure(width, height);

                    double _helperWidth = Math.Min(_helperSize.Request.Width, width);
                    double _helperHeight = Math.Min(_helperSize.Request.Height, height);

                    _helperLabel.Layout(new Rectangle(x + 10, size.Request.Height + hintHeight, _helperWidth, _helperHeight));
                }
                {
                    Label _lengthLabel = _textLengthLabel.Value;
                    SizeRequest _size = _lengthLabel.Measure(width, height);

                    double _width = Math.Min(_size.Request.Width, width);
                    double _height = Math.Min(_size.Request.Height, height);

                    _lengthLabel.Layout(new Rectangle(x + width - _width, size.Request.Height + hintHeight, _width, _height));
                }

                _inputView.Layout(new Rectangle(x, y + hintHeight, width, Math.Min(size.Request.Height, height)));

                UpdateHintFloat();
                UpdateTextLengthText();
            }
        }

        private void StyleChangedProperty(TextInputLayoutLabelStyle oldStyle, TextInputLayoutLabelStyle newStyle)
        {
            if (oldStyle != null)
            {
                oldStyle.PropertyChanged -= TextStyle_PropertyChanged;
            }
            if (newStyle != null)
            {
                newStyle.PropertyChanged += TextStyle_PropertyChanged;
            }
        }

        private void TextStyle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TextInputLayoutLabelStyle.FontSize))
            {
                InvalidateLayout();
            }
        }
    }
}
