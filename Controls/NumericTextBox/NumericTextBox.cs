using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public enum FocuslAllPosition
    {
        AnyPosition,
        StartLine,
        EndLine,
        NonStartLine,
        NonEndLine
    }
    public class NumericTextBox : Entry, IElementConfiguration<NumericTextBox>
    {
        public static readonly Regex NumberValidation = new Regex("^-?\\d*[,.]?(\\d+)?$");
        public static readonly char[] DecimalSeparators = ",.".ToCharArray();

        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(decimal?), typeof(NumericTextBox), null, BindingMode.TwoWay, propertyChanged: OnValueChanged);
        public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(decimal?), typeof(NumericTextBox), null, propertyChanged: OnMinMaxChanged);
        public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(decimal?), typeof(NumericTextBox), null, propertyChanged: OnMinMaxChanged);

        public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(NumericTextBox), null, propertyChanged: OnFormatChanged);
        public static readonly BindableProperty AllowNullProperty = BindableProperty.Create(nameof(AllowNull), typeof(bool), typeof(NumericTextBox), false);
        public static readonly BindableProperty SelectAllFocusPositionProperty = BindableProperty.Create(nameof(SelectAllFocusPosition), typeof(FocuslAllPosition), typeof(NumericTextBox), FocuslAllPosition.AnyPosition);
        public static readonly BindableProperty SelectAllOnFocusProperty = BindableProperty.Create(nameof(SelectAllOnFocus), typeof(bool), typeof(NumericTextBox), true);

        public decimal? Value
        {
            get => (decimal?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public decimal? Maximum
        {
            get => (decimal?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public decimal? Minimum
        {
            get => (decimal?)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }
        public bool AllowNull
        {
            get => (bool)GetValue(AllowNullProperty);
            set => SetValue(AllowNullProperty, value);
        }
        public FocuslAllPosition SelectAllFocusPosition
        {
            get => (FocuslAllPosition)GetValue(SelectAllFocusPositionProperty);
            set => SetValue(SelectAllFocusPositionProperty, value);
        }
        public bool SelectAllOnFocus
        {
            get => (bool)GetValue(SelectAllOnFocusProperty);
            set => SetValue(SelectAllOnFocusProperty, value);
        }
        public new Keyboard Keyboard
        {
            get => base.Keyboard;
        }

        public new string Text { get => base.Text; }

        private int numberOfDecimals = -1;

        private bool _isFocused = false;
        private CancellationTokenSource tokenSourceUnfocus;

        private readonly Lazy<PlatformConfigurationRegistry<NumericTextBox>> _platformConfigurationRegistry;
        public NumericTextBox() : base()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NumericTextBox>>(() => new PlatformConfigurationRegistry<NumericTextBox>(this));

            base.Keyboard = Keyboard.Numeric;
            base.IsSpellCheckEnabled = false;
            base.IsTextPredictionEnabled = false;

            Completed += NumericTextBox_Completed;
            Focused += NumericTextBox_Focused;
            Unfocused += NumericTextBox_Unfocused;
        }

        private void NumericTextBox_Completed(object sender, EventArgs e)
        {
            UpdateText();
        }
        private void NumericTextBox_Focused(object sender, FocusEventArgs e)
        {
            if (tokenSourceUnfocus != null)
            {
                tokenSourceUnfocus.Cancel();
                tokenSourceUnfocus.Dispose();
                tokenSourceUnfocus = null;
            }
            if (_isFocused)
            {
                return;
            }
            _isFocused = true;
            if (!Value.HasValue)
            {
                //_ignoreTextChange = true;
                base.Text = string.Empty;
            }
            else if (!string.IsNullOrEmpty(Format))
            {
                Match regex = Regex.Match(Format.Replace("#", "0"), "\\d+?[,.]\\d+");

                string result;

                if (regex.Success)
                {
                    result = Value.Value.ToString(regex.Value, CultureInfo.InvariantCulture);

                    if (!NumberValidation.IsMatch(result))
                    {
                        result = Value.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    result = Value.Value.ToString(CultureInfo.InvariantCulture);
                }

                //_ignoreTextChange = true;
                base.Text = result;
            }
            else
            {
                //_ignoreTextChange = true;
                base.Text = Value.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (SelectAllOnFocus)
            {
                SelectionOnFocus();
            }
        }
        private void NumericTextBox_Unfocused(object sender, FocusEventArgs e)
        {
            if (tokenSourceUnfocus != null)
            {
                tokenSourceUnfocus.Cancel();
                tokenSourceUnfocus.Dispose();
            }

            tokenSourceUnfocus = new CancellationTokenSource();
            Task.Delay(100, tokenSourceUnfocus.Token).ContinueWith(delegate (Task task)
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    throw task.Exception;
                }

                if (task.Status != TaskStatus.Canceled && _isFocused)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _isFocused = false;
                        UpdateText();
                    });
                }
            }).ConfigureAwait(false);
        }

        private void SelectionOnFocus(int delay = 1)
        {
            Task.Delay(delay).ContinueWith(delegate (Task task)
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    throw task.Exception;
                }

                if (task.Status != TaskStatus.Canceled)
                {
                    switch (SelectAllFocusPosition)
                    {
                        case FocuslAllPosition.StartLine:
                            {
                                if (CursorPosition == 0)
                                {
                                    SelectAll();
                                }
                                break;
                            }
                        case FocuslAllPosition.EndLine:
                            {
                                if (CursorPosition == Text.Length)
                                {
                                    SelectAll();
                                }
                                break;
                            }
                        case FocuslAllPosition.NonStartLine:
                            {
                                if (CursorPosition != 0)
                                {
                                    SelectAll();
                                }
                                break;
                            }
                        case FocuslAllPosition.NonEndLine:
                            {
                                if (CursorPosition != Text.Length)
                                {
                                    SelectAll();
                                }
                                break;
                            }
                        default:
                            {
                                SelectAll();
                                break;
                            }
                    }
                }
            }).ConfigureAwait(false);
        }

        private void SelectAll(int delay = 1)
        {
            Task.Delay(delay).ContinueWith(delegate (Task task)
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    throw task.Exception;
                }

                if (task.Status != TaskStatus.Canceled)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_isFocused)
                        {
                            CursorPosition = 0;
                            SelectionLength = Text.Length;
                        }
                    });
                }
            }).ConfigureAwait(false);
        }

        private void UpdateText(int delay = 1)
        {
            Task.Delay(delay).ContinueWith(delegate (Task task)
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    throw task.Exception;
                }

                if (task.Status != TaskStatus.Canceled)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!_isFocused)
                        {
                            if (Value.HasValue)
                            {
                                if (string.IsNullOrEmpty(Format))
                                {
                                    base.Text = Value.Value.ToString(CultureInfo.CurrentCulture);
                                }
                                else
                                {
                                    base.Text = Value.Value.ToString(Format, CultureInfo.CurrentCulture);
                                }
                            }
                            else
                            {
                                base.Text = string.Empty;
                            }
                        }
                    });
                }
            }).ConfigureAwait(false);
        }

        protected override void OnTextChanged(string oldValue, string newValue)
        {
            //if (_ignoreTextChange)
            //{
            //    _ignoreTextChange = false;
            //    base.OnTextChanged(oldValue, newValue);
            //    return;
            //}

            if (string.IsNullOrEmpty(newValue))
            {
                if (AllowNull)
                {
                    Value = null;
                }
                else
                {
                    Value = 0;
                }
                base.OnTextChanged(oldValue, newValue);
            }
            else if (!_isFocused)
            {
                base.OnTextChanged(oldValue, newValue);
            }
            else if (!NumberValidation.IsMatch(newValue) ||
                numberOfDecimals == 0 && newValue.IndexOfAny(DecimalSeparators) != -1 ||
                numberOfDecimals != -1 && GetDecimalsFromString(newValue) > numberOfDecimals)
            {
                //_ignoreTextChange = true;
                if (!NumberValidation.IsMatch(oldValue) || numberOfDecimals != -1 && GetDecimalsFromString(oldValue) > numberOfDecimals)
                {
                    base.Text = string.Empty;
                }
                else
                {
                    base.Text = oldValue;
                }
            }
            else if (decimal.TryParse(newValue.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal newNumber))
            {
                if (Minimum.HasValue && newNumber < Minimum.Value)
                {
                    //_ignoreTextChange = true;
                    base.Text = Minimum.Value.ToString(CultureInfo.CurrentCulture);

                    SelectAll();
                }
                else if (Maximum.HasValue && newNumber > Maximum.Value)
                {
                    //_ignoreTextChange = true;
                    base.Text = Maximum.Value.ToString(CultureInfo.CurrentCulture);

                    SelectAll();
                }
                else
                {
                    Value = newNumber;
                    base.OnTextChanged(oldValue, newValue);
                }
            }
            else if (newValue != "-")
            {
                //_ignoreTextChange = true;
                base.Text = oldValue;
                //Value = 0;
            }
        }

        private int GetDecimalsFromString(string text)
        {
            Match regex = Regex.Match(text, "[,.]\\d+");
            if (regex.Success)
            {
                return regex.Value.Length - 1;
            }

            return 0;
        }

        private void UpdateFormatState()
        {
            if (!string.IsNullOrEmpty(Format))
            {
                numberOfDecimals = GetDecimalsFromString(Format.Replace("#", "0"));
            }
            else
            {
                numberOfDecimals = -1;
            }
            UpdateText();
        }

        private static void OnFormatChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is NumericTextBox tx)
            {
                tx.UpdateFormatState();
            }
        }

        private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is NumericTextBox tx)
            {
                tx.UpdateText();
            }
        }

        private static void OnMinMaxChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is NumericTextBox tx)
            {
                if (tx.Minimum.HasValue && tx.Value < tx.Minimum.Value)
                {
                    tx.Value = tx.Minimum.Value;
                }
                else if (tx.Maximum.HasValue && tx.Value > tx.Maximum.Value)
                {
                    tx.Value = tx.Maximum.Value;
                }
            }
        }

        public new IPlatformElementConfiguration<T, NumericTextBox> On<T>() where T : IConfigPlatform
        {
            return _platformConfigurationRegistry.Value.On<T>();
        }
    }
}
