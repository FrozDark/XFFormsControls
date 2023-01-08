using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace XFFormsControls.Controls
{
    public class Tag : Frame
    {
        protected static PathGeometryConverter _geometryConverter = new PathGeometryConverter();

        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(Tag), string.Empty, BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnTextChanged((string)o, (string)n);
        });

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(Tag), Color.White, BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnTextColorChanged((Color)o, (Color)n);
        });

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create("FontSize", typeof(double), typeof(Tag), Device.GetNamedSize(NamedSize.Default, typeof(Label)), BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnFontSizeChanged((double)o, (double)n);
        });

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(Tag), string.Empty, BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnFontFamilyChanged((string)o, (string)n);
        });

        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(Tag), FontAttributes.None, BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnFontAttributesChanged((FontAttributes)o, (FontAttributes)n);
        });

        public static readonly BindableProperty ShowCloseButtonProperty = BindableProperty.Create("ShowCloseButton", typeof(bool), typeof(Tag), true, propertyChanged: (b, o, n) =>
        {
            ((Tag)b).OnShowCloseButtonChanged((bool)o, (bool)n);
        });

        public static readonly BindableProperty SelectableProperty = BindableProperty.Create("Selectable", typeof(bool), typeof(Tag), false, BindingMode.OneWay, null, (b, o, n) =>
        {
            ((Tag)b).OnSelectableChanged((bool)o, (bool)n);
        });

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create("IsSelected", typeof(bool), typeof(Tag), false, BindingMode.TwoWay, propertyChanged: (b, o, n) =>
        {
            ((Tag)b).OnIsSelectedChanged((bool)o, (bool)n);
        });

        public static readonly BindableProperty NumberOfTapsRequiredProperty = BindableProperty.Create("NumberOfTapsRequired", typeof(int), typeof(Tag), 1, propertyChanged: (b, o, n) =>
        {
            ((Tag)b).OnNumberOfTapsRequiredChanged((int)o, (int)n);
        });

        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(Tag), propertyChanged: (b, o, n) =>
        {
            ((Tag)b).OnCommandChanged((ICommand)o, (ICommand)n);
        });

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(Tag), propertyChanged: (b, o, n) =>
        {
            ((Tag)b).OnCommandParameterChanged(o, n);
        });

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public string FontFamily
        {
            get => (string)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public FontAttributes FontAttributes
        {
            get => (FontAttributes)GetValue(FontAttributesProperty);
            set => SetValue(FontAttributesProperty, value);
        }

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        public bool Selectable
        {
            get => (bool)GetValue(SelectableProperty);
            set => SetValue(SelectableProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public int NumberOfTapsRequired
        {
            get { return (int)GetValue(NumberOfTapsRequiredProperty); }
            set { SetValue(NumberOfTapsRequiredProperty, value); }
        }
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public event EventHandler Selected
        {
            add { weakEventManager.AddEventHandler(value); }
            remove { weakEventManager.RemoveEventHandler(value); }
        }

        public event EventHandler Closed
        {
            add { weakEventManager.AddEventHandler(value); }
            remove { weakEventManager.RemoveEventHandler(value); }
        }

        protected readonly WeakEventManager weakEventManager = new WeakEventManager();
        private Path _checkBox;
        private Label _label;
        private Button _button;
        private Path _path;
        private TapGestureRecognizer _tapGestureRecognizer;

        public Tag() : base()
        {
            Padding = 2;
            BackgroundColor = Color.Gray;
            BorderColor = Color.Silver;
            CornerRadius = 2;
            VerticalOptions = LayoutOptions.Start;

            Grid grid = new Grid()
            {
                ColumnSpacing = 0,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto,
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto,
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto,
            });

            Geometry pathCheckData = (Geometry)_geometryConverter.ConvertFromInvariantString("M23.25.749,8.158,22.308a2.2,2.2,0,0,1-3.569.059L.75,17.249");
            _checkBox = new Path()
            {
                HeightRequest = 12,
                WidthRequest = 12,
                InputTransparent = true,
                Aspect = Stretch.Fill,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Stroke = Color.LimeGreen,
                StrokeThickness = 2,
                IsVisible = Selectable && IsSelected,
                Data = pathCheckData
            };
            Grid.SetColumn(_checkBox, 0);
            grid.Children.Add(_checkBox);

            _label = new Label()
            {
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(2,0,2,0),
                Text = Text,
                TextColor = TextColor,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontAttributes = FontAttributes,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.NoWrap
            };
            Grid.SetColumn(_label, 1);
            grid.Children.Add(_label);

            _button = new Button()
            {
                Padding = 0,
                BackgroundColor = BorderColor,
                CornerRadius = 99,
                HeightRequest = 25,
                WidthRequest = 25,
                TextColor = BackgroundColor,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = ShowCloseButton,
            };
            _button.Clicked += CloseButton_Clicked;
            Grid.SetColumn(_button, 2);
            grid.Children.Add(_button);

            Geometry pathData = (Geometry)_geometryConverter.ConvertFromInvariantString("M7.4038057,6.4038057 C7.1478834,6.4038057 6.8919611,6.5014372 6.6966991,6.6966991 6.3061748,7.0872235 6.3061748,7.7203884 6.6966991,8.1109123 L10.585787,12 6.6966991,15.889088 C6.3061748,16.279612 6.3061748,16.912777 6.6966991,17.303301 7.0872235,17.693825 7.7203879,17.693825 8.1109123,17.303301 L12,13.414213 15.889088,17.303301 C16.279612,17.693825 16.912777,17.693825 17.303301,17.303301 17.693825,16.912777 17.693825,16.279612 17.303301,15.889088 L13.414213,12 17.303301,8.1109123 C17.693825,7.7203884 17.693825,7.0872235 17.303301,6.6966991 16.912777,6.3061748 16.279612,6.3061748 15.889088,6.6966991 L12,10.585787 8.1109123,6.6966991 C7.9156504,6.5014372 7.6597281,6.4038057 7.4038057,6.4038057 z M12,0 C18.627417,0 24,5.3725829 24,12 24,18.627417 18.627417,24 12,24 5.3725829,24 0,18.627417 0,12 0,5.3725829 5.3725829,0 12,0 z");
            _path = new Path()
            {
                InputTransparent = true,
                Aspect = Stretch.Fill,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Fill = BackgroundColor,
                IsVisible = _button.IsVisible,
                Data = pathData
            };
            Grid.SetColumn(_path, 2);
            grid.Children.Add(_path);

            Content = grid;

            _tapGestureRecognizer = new TapGestureRecognizer()
            {
                NumberOfTapsRequired = 1,
            };
            _tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            GestureRecognizers.Add(_tapGestureRecognizer);
        }

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(Closed));
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (Selectable)
            {
                IsSelected = !IsSelected;
                weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(Selected));
            }
        }

        private void OnTextChanged(string oldValue, string newValue)
        {
            _label.Text = newValue;
        }

        private void OnTextColorChanged(Color oldValue, Color newValue)
        {
            _label.TextColor = newValue;
        }

        private void OnFontSizeChanged(double oldValue, double newValue)
        {
            _label.FontSize = newValue;
        }

        private void OnFontFamilyChanged(string oldValue, string newValue)
        {
            _label.FontFamily = newValue;
        }

        private void OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
        {
            _label.FontAttributes = newValue;
        }

        private void OnShowCloseButtonChanged(bool o, bool n)
        {
            _button.IsVisible = n;
            _path.IsVisible = n;
        }

        private void OnSelectableChanged(bool o, bool n)
        {
            if (!n)
            {
                if (IsSelected)
                {
                    IsSelected = false;
                }
            }
        }

        private void OnIsSelectedChanged(bool o, bool n)
        {
            _checkBox.IsVisible = n && Selectable;

            if (n)
            {
                VisualStateManager.GoToState(this, "Selected");
            }
            else
            {
                VisualStateManager.GoToState(this, "Default");
            }
        }

        private void OnNumberOfTapsRequiredChanged(int o, int n)
        {
            _tapGestureRecognizer.NumberOfTapsRequired = n;
        }

        private void OnCommandChanged(ICommand o, ICommand n)
        {
            _button.Command = n;
        }

        private void OnCommandParameterChanged(object o, object n)
        {
            _button.CommandParameter = n;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!(_checkBox is null))
            {
                _checkBox.HeightRequest = height - 10;
                _checkBox.WidthRequest = height - 10;
            }
        }
    }
}
