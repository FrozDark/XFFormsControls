using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class StringHighlighter : FormattedString
    {
        //public static readonly BindableProperty StyleProperty = BindableProperty.Create(nameof(Style), typeof(Style), typeof(StringHighlighter), default(Style),
        //    propertyChanged: (bindable, oldvalue, newvalue) => ((Span)bindable)._mergedStyle.Style = (Style)newvalue, defaultBindingMode: BindingMode.OneWay);

        public static readonly BindableProperty TextProperty 
            = BindableProperty.Create(nameof(Text), typeof(string), typeof(StringHighlighter), default(string), defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnTextPatternPropertyChanged);

        public static readonly BindableProperty PatternProperty 
            = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(StringHighlighter), default(string), defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnTextPatternPropertyChanged);

        public static readonly BindableProperty PatternWordSeparatorProperty
            = BindableProperty.Create(nameof(PatternWordSeparator), typeof(string), typeof(StringHighlighter), default(string), defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnTextPatternPropertyChanged);

        public static readonly BindableProperty TextColorProperty 
            = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(StringHighlighter), Color.Default, defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty BackgroundColorProperty
            = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(StringHighlighter), default(Color), defaultBindingMode: BindingMode.OneWay,
                                    propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty CharacterSpacingProperty =
            BindableProperty.Create(nameof(CharacterSpacing), typeof(double), typeof(StringHighlighter), 0.0d, defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(StringHighlighter), default(string), defaultBindingMode: BindingMode.OneWay,
                                    propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(StringHighlighter), -1.0, defaultBindingMode: BindingMode.OneWay,
                defaultValueCreator: FontSizeDefaultValueCreator,
                                    propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty FontAttributesProperty =
            BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(StringHighlighter), FontAttributes.None, defaultBindingMode: BindingMode.OneWay,
                                    propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty LineHeightProperty =
            BindableProperty.Create(nameof(LineHeight), typeof(double), typeof(StringHighlighter), -1.0d, defaultBindingMode: BindingMode.OneWay,
                                    propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty TextTransformProperty =
            BindableProperty.Create(nameof(TextTransform), typeof(TextTransform), typeof(StringHighlighter), TextTransform.Default, defaultBindingMode: BindingMode.OneWay,
                            propertyChanged: OnPatternTransformationPropertyChanged);

        public static readonly BindableProperty TextDecorationsProperty 
            = BindableProperty.Create(nameof(IDecorableTextElement.TextDecorations), typeof(TextDecorations), typeof(StringHighlighter), TextDecorations.None, defaultBindingMode: BindingMode.OneWay,
                propertyChanged: OnPatternTransformationPropertyChanged);


        private static object FontSizeDefaultValueCreator(BindableObject bindable)
        {
            return Device.GetNamedSize(NamedSize.Default, typeof(Label));
        }

        private static void OnTextPatternPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((StringHighlighter)bindable).OnTextPatternChanged();
        }

        private static void OnPatternTransformationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((StringHighlighter)bindable).OnPatternTransformationChanged();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Pattern
        {
            get => (string)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        public string PatternWordSeparator
        {
            get => (string)GetValue(PatternWordSeparatorProperty);
            set => SetValue(PatternWordSeparatorProperty, value);
        }

        public double CharacterSpacing
        {
            get { return (double)GetValue(CharacterSpacingProperty); }
            set { SetValue(CharacterSpacingProperty, value); }
        }

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        public TextTransform TextTransform
        {
            get => (TextTransform)GetValue(TextTransformProperty);
            set => SetValue(TextTransformProperty, value);
        }

        public FontAttributes FontAttributes
        {
            get { return (FontAttributes)GetValue(FontAttributesProperty); }
            set { SetValue(FontAttributesProperty, value); }
        }

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public TextDecorations TextDecorations
        {
            get { return (TextDecorations)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }


        public StringHighlighter() : base()
        {
            
        }

        private void OnTextPatternChanged()
        {
            SetupText();
        }

        private void OnPatternTransformationChanged()
        {
            foreach (var span in Spans.Where(c => string.Equals(c.Text, Pattern, StringComparison.OrdinalIgnoreCase)))
            {
                span.CharacterSpacing = CharacterSpacing;
                span.TextColor = TextColor;
                span.BackgroundColor = BackgroundColor;
                span.LineHeight = LineHeight;
                span.TextTransform = TextTransform;
                span.FontAttributes = FontAttributes;
                span.FontFamily = FontFamily;
                span.FontSize = FontSize;
                span.TextDecorations = TextDecorations;
            }
        }

        private void SetupText()
        {
            if (Spans.Count > 0)
            {
                Spans.Clear();
            }

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                Spans.Add(new Span()
                {
                    Text = Text
                });

                return;
            }

            string text = Text;

            string[] patterns = new[] { Pattern };

            if (!string.IsNullOrEmpty(PatternWordSeparator))
            {
                patterns = Pattern.Split(new string[] { PatternWordSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (patterns.Length < 1)
                {
                    patterns = new[] { Pattern };
                }
            }

            int pos = 0;
            while (pos != -1)
            {
                int selected_pos = -1;
                string selected_pattern = null;

                foreach (var subj in patterns)
                {
                    var dum = text.IndexOf(subj, StringComparison.OrdinalIgnoreCase);
                    if (dum != -1 && (selected_pos == -1 || dum < selected_pos))
                    {
                        selected_pos = dum;
                        selected_pattern = subj;
                    }
                }
                if (selected_pos == -1)
                {
                    break;
                }
                pos = selected_pos;
                var pattern = text.Substring(pos, selected_pattern.Length);

                if (pos > 0)
                {
                    var pretext = text.Substring(0, pos);
                    if (!string.IsNullOrEmpty(pretext))
                    {
                        Spans.Add(new Span()
                        {
                            Text = pretext,
                        });
                    }
                }

                if (!string.IsNullOrEmpty(pattern))
                {
                    Spans.Add(new Span()
                    {
                        Text = pattern,
                        TextColor = TextColor,
                        CharacterSpacing = CharacterSpacing,
                        BackgroundColor = BackgroundColor,
                        LineHeight = LineHeight,
                        TextTransform = TextTransform,
                        FontAttributes = FontAttributes,
                        FontFamily = FontFamily,
                        FontSize = FontSize,
                        TextDecorations = TextDecorations,
                    });
                }

                int nextIndex = pos + selected_pattern.Length;
                if (nextIndex < Text.Length)
                {
                    text = text.Substring(nextIndex);
                }
                else
                {
                    text = null;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(text))
            {
                Spans.Add(new Span()
                {
                    Text = text,
                });
            }
        }
    }
}
