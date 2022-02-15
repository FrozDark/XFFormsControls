using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    [ContentProperty(nameof(Content))]
    public class TabViewItem : TemplatedView
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(TabViewItem), string.Empty);

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(TabViewItem), Color.Default, BindingMode.OneWay);

        public static readonly BindableProperty TextColorSelectedProperty = BindableProperty.Create("TextColorSelected", typeof(Color), typeof(TabViewItem), Color.Default);

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create("FontSize", typeof(double), typeof(TabViewItem), Device.GetNamedSize(NamedSize.Small, typeof(Label)), BindingMode.OneWay);

        public static readonly BindableProperty FontSizeSelectedProperty = BindableProperty.Create("FontSizeSelected", typeof(double), typeof(TabViewItem), Device.GetNamedSize(NamedSize.Small, typeof(Label)));

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(TabViewItem), string.Empty, BindingMode.OneWay);

        public static readonly BindableProperty FontFamilySelectedProperty = BindableProperty.Create("FontFamilySelected", typeof(string), typeof(TabViewItem), string.Empty, BindingMode.OneWay);

        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(TabViewItem), FontAttributes.None, BindingMode.OneWay);

        public static readonly BindableProperty FontAttributesSelectedProperty = BindableProperty.Create("FontAttributesSelected", typeof(FontAttributes), typeof(TabViewItem), FontAttributes.Bold, BindingMode.OneWay);

        public static readonly BindableProperty ContentProperty = BindableProperty.Create("Content", typeof(View), typeof(TabViewItem), propertyChanged: OnContentChanged);

        private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            //bindable.SetBinding(BindingContextProperty, new Binding(nameof(BindingContext), BindingMode.OneWay, source: bindable));

            SetInheritedBindingContext((View)newValue, bindable.BindingContext);
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create("IsSelected", typeof(bool), typeof(TabViewItem), false, BindingMode.OneWay);

        public static readonly BindableProperty BadgeTextProperty = BindableProperty.Create("BadgeText", typeof(string), typeof(TabViewItem), string.Empty);

        public static readonly BindableProperty BadgeTextColorProperty = BindableProperty.Create("BadgeTextColor", typeof(Color), typeof(TabViewItem), Color.Default);

        public static readonly BindableProperty BadgeBackgroundColorProperty = BindableProperty.Create("BadgeBackgroundColor", typeof(Color), typeof(TabViewItem), Color.Transparent, BindingMode.OneWay);

        public static readonly BindableProperty BadgeBackgroundColorSelectedProperty = BindableProperty.Create("BadgeBackgroundColorSelected", typeof(Color), typeof(TabViewItem), Color.Transparent, BindingMode.OneWay);

        public static readonly BindableProperty BadgeBorderColorProperty = BindableProperty.Create("BadgeBorderColor", typeof(Color), typeof(TabViewItem), Color.Default, BindingMode.OneWay);

        public static readonly BindableProperty BadgeBorderColorSelectedProperty = BindableProperty.Create("BadgeBorderColorSelected", typeof(Color), typeof(TabViewItem), Color.Default, BindingMode.OneWay);

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        public Color BadgeTextColor
        {
            get => (Color)GetValue(BadgeTextColorProperty);
            set => SetValue(BadgeTextColorProperty, value);
        }

        public Color BadgeBackgroundColor
        {
            get => (Color)GetValue(BadgeBackgroundColorProperty);
            set => SetValue(BadgeBackgroundColorProperty, value);
        }

        public Color BadgeBackgroundColorSelected
        {
            get => (Color)GetValue(BadgeBackgroundColorSelectedProperty);
            set => SetValue(BadgeBackgroundColorSelectedProperty, value);
        }

        public Color BadgeBorderColor
        {
            get
            {
                return (Color)GetValue(BadgeBorderColorProperty);
            }
            set
            {
                SetValue(BadgeBorderColorProperty, value);
            }
        }

        public Color BadgeBorderColorSelected
        {
            get
            {
                return (Color)GetValue(BadgeBorderColorSelectedProperty);
            }
            set
            {
                SetValue(BadgeBorderColorSelectedProperty, value);
            }
        }


        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public Color TextColor
        {
            get
            {
                return (Color)GetValue(TextColorProperty);
            }
            set
            {
                SetValue(TextColorProperty, value);
            }
        }

        public Color TextColorSelected
        {
            get
            {
                return (Color)GetValue(TextColorSelectedProperty);
            }
            set
            {
                SetValue(TextColorSelectedProperty, value);
            }
        }

        public double FontSize
        {
            get
            {
                return (double)GetValue(FontSizeProperty);
            }
            set
            {
                SetValue(FontSizeProperty, value);
            }
        }

        public double FontSizeSelected
        {
            get
            {
                return (double)GetValue(FontSizeSelectedProperty);
            }
            set
            {
                SetValue(FontSizeSelectedProperty, value);
            }
        }

        public string FontFamily
        {
            get
            {
                return (string)GetValue(FontFamilyProperty);
            }
            set
            {
                SetValue(FontFamilyProperty, value);
            }
        }

        public string FontFamilySelected
        {
            get
            {
                return (string)GetValue(FontFamilySelectedProperty);
            }
            set
            {
                SetValue(FontFamilySelectedProperty, value);
            }
        }

        public FontAttributes FontAttributes
        {
            get
            {
                return (FontAttributes)GetValue(FontAttributesProperty);
            }
            set
            {
                SetValue(FontAttributesProperty, value);
            }
        }

        public FontAttributes FontAttributesSelected
        {
            get
            {
                return (FontAttributes)GetValue(FontAttributesSelectedProperty);
            }
            set
            {
                SetValue(FontAttributesSelectedProperty, value);
            }
        }

        public View Content
        {
            get
            {
                return (View)GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (Content != null)
            {
                SetInheritedBindingContext(Content, BindingContext);
            }
        }
    }
}
