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
        public static readonly BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(TextInputLayout), propertyChanged: TextPropertyChanged);
        public static readonly BindableProperty HintScaleProperty = BindableProperty.Create(nameof(HintScale), typeof(double), typeof(TextInputLayout), 0.9);
        public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(nameof(HelperText), typeof(string), typeof(TextInputLayout), propertyChanged: TextPropertyChanged);
        public static readonly BindableProperty FocusedColorProperty = BindableProperty.Create(nameof(FocusedColor), typeof(Color), typeof(TextInputLayout), Color.FromHex("#00AFA0"));
        public static readonly BindableProperty UnfocusedColorProperty = BindableProperty.Create(nameof(UnfocusedColor), typeof(Color), typeof(TextInputLayout), Color.Silver);
        public static readonly BindableProperty HelperTextColorProperty = BindableProperty.Create(nameof(HelperTextColor), typeof(Color), typeof(TextInputLayout));
        public static readonly BindableProperty HintLabelStyleProperty = BindableProperty.Create(nameof(HintLabelStyle), typeof(TextInputLayoutLabelStyle), typeof(TextInputLayout), new TextInputLayoutLabelStyle() { FontAttributes = FontAttributes.Bold }, propertyChanged: TextStylePropertyChanged);

        public static readonly BindableProperty HelperLabelStyleProperty = BindableProperty.Create(nameof(HelperLabelStyle), typeof(TextInputLayoutLabelStyle), typeof(TextInputLayout), new TextInputLayoutLabelStyle() {  FontSize = 11 }, propertyChanged: TextStylePropertyChanged);

        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TextInputLayout inputLayout)
            {
                if (string.IsNullOrEmpty((string)oldValue) || string.IsNullOrEmpty((string)newValue))
                {
                    inputLayout.InvalidateLayout();
                }
            }
        }
        private static void TextStylePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TextInputLayout inputLayout)
            {
                inputLayout.StyleChangedProperty((TextInputLayoutLabelStyle)oldValue, (TextInputLayoutLabelStyle)newValue);
            }
        }


        public double HintScale
        {
            get => (double)GetValue(HintScaleProperty);
            set => SetValue(HintScaleProperty, value);
        }

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }
        public string HelperText
        {
            get => (string)GetValue(HelperTextProperty);
            set => SetValue(HelperTextProperty, value);
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
        public Color HelperTextColor
        {
            get => (Color)GetValue(HelperTextColorProperty);
            set => SetValue(HelperTextColorProperty, value);
        }

        public TextInputLayoutLabelStyle HintLabelStyle
        {
            get => (TextInputLayoutLabelStyle)GetValue(HintLabelStyleProperty);
            set => SetValue(HintLabelStyleProperty, value);
        }

        public TextInputLayoutLabelStyle HelperLabelStyle
        {
            get => (TextInputLayoutLabelStyle)GetValue(HelperLabelStyleProperty);
            set => SetValue(HelperLabelStyleProperty, value);
        }

        private ObservableCollection<Element> ChildsInternal { get => (ObservableCollection<Element>)Children; }

        private InputView _inputView = null;
        private Lazy<Label> _hintTextLabel;
        private Lazy<Label> _helperTextLabel;

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
                    VerticalTextAlignment = TextAlignment.Center
                };
                label.SetBinding(Label.TextProperty, new Binding()
                {
                    Path = nameof(Hint),
                    Source = this
                });
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
                    VerticalTextAlignment = TextAlignment.Center
                };
                label.SetBinding(Label.TextProperty, new Binding()
                {
                    Path = nameof(HelperText),
                    Source = this
                });
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
                label.SetBinding(Label.TextColorProperty, new Binding()
                {
                    Path = nameof(HelperTextColor),
                    Source = this
                });

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
            }

            _inputView = view;
            ChildsInternal.Insert(0, _inputView);

            _inputView.TextChanged += InputView_TextChanged;
            _inputView.Focused += InputView_Focused;
            _inputView.Unfocused += InputView_Unfocused;

            InvalidateLayout();
            UpdateHintFloat();
        }

        private void InputView_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateHintFloat();
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
                if (!string.IsNullOrEmpty(Hint))
                {
                    Label _hintLabel = _hintTextLabel.Value;
                    SizeRequest _hintSize = _hintLabel.Measure(width, height);
                    hintHeight = _hintSize.Request.Height * HintScale;

                    _hintLabel.Layout(new Rectangle(x + 10, y + hintHeight, _hintSize.Request.Width, Math.Max(size.Request.Height, _hintSize.Request.Height)));
                }
                if (!string.IsNullOrEmpty(HelperText))
                {
                    Label _helperLabel = _helperTextLabel.Value;
                    SizeRequest _helperSize = _helperLabel.Measure(width, height);

                    double _helperWidth = Math.Min(_helperSize.Request.Width, width);
                    double _helperHeight = Math.Min(_helperSize.Request.Height, height);

                    _helperLabel.Layout(new Rectangle(x + 10, size.Request.Height + hintHeight, _helperWidth, _helperHeight));
                }

                _inputView.Layout(new Rectangle(x, y + hintHeight, width, size.Request.Height));

                UpdateHintFloat();
            }
        }

        private void StyleChangedProperty(TextInputLayoutLabelStyle oldStyle, TextInputLayoutLabelStyle newStyle)
        {
            if (oldStyle != null)
            {
                oldStyle.PropertyChanged += TextStyle_PropertyChanged;
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
