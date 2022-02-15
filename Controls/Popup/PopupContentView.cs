using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class PopupContentView : Frame
    {
        public const string AnimationName = "TransitionAnimation";

        public static readonly BindableProperty IsPresentedProperty = BindableProperty.Create(nameof(IsPresented), typeof(bool), typeof(PopupContentView), false, BindingMode.TwoWay, propertyChanged: OnIsPresentedChanged);
        public static readonly BindableProperty TransitionAnimationProperty = BindableProperty.Create(nameof(TransitionAnimation), typeof(AnimationControl), typeof(PopupContentView), new FadeAnimation(0, 1));
        public static readonly BindableProperty OnShowCommandProperty = BindableProperty.Create(nameof(OnShowCommand), typeof(ICommand), typeof(PopupContentView));
        public static readonly BindableProperty OnShowCommandParameterProperty = BindableProperty.Create(nameof(OnShowCommandParameter), typeof(object), typeof(PopupContentView));
        public static readonly BindableProperty OnHideCommandProperty = BindableProperty.Create(nameof(OnHideCommand), typeof(ICommand), typeof(PopupContentView));
        public static readonly BindableProperty OnHideCommandParameterProperty = BindableProperty.Create(nameof(OnHideCommandParameter), typeof(object), typeof(PopupContentView));

        public bool IsPresented
        {
            get => (bool)GetValue(IsPresentedProperty);
            set => SetValue(IsPresentedProperty, value);
        }

        public ICommand OnShowCommand
        {
            get => (ICommand)GetValue(OnShowCommandProperty);
            set => SetValue(OnShowCommandProperty, value);
        }

        public object OnShowCommandParameter
        {
            get => (ICommand)GetValue(OnShowCommandParameterProperty);
            set => SetValue(OnShowCommandParameterProperty, value);
        }

        public ICommand OnHideCommand
        {
            get => (ICommand)GetValue(OnHideCommandProperty);
            set => SetValue(OnHideCommandProperty, value);
        }

        public object OnHideCommandParameter
        {
            get => (ICommand)GetValue(OnHideCommandParameterProperty);
            set => SetValue(OnHideCommandParameterProperty, value);
        }

        public AnimationControl TransitionAnimation
        {
            get => (AnimationControl)GetValue(TransitionAnimationProperty);
            set => SetValue(TransitionAnimationProperty, value);
        }

        public Popup Popup { get; internal set; } = null;
        public Command ShowCommand { get; }
        public Command HideCommand { get; }
        public Command ToggleCommand { get; }


        public event EventHandler OnPopupShow;
        public event EventHandler OnPopupHide;

        public PopupContentView() : base()
        {
            ShowCommand = new Command(Show);
            HideCommand = new Command(Hide);
            ToggleCommand = new Command(Toggle);

            IsClippedToBounds = true;
        }

        public void Show()
        {
            IsPresented = true;
        }

        public void Hide()
        {
            IsPresented = false;
        }

        public void Toggle()
        {
            IsPresented = !IsPresented;
        }

        public bool StopAnimation()
        {
            return this.AbortAnimation(AnimationName);
        }

        internal void OnPopupShown()
        {
            if (OnShowCommand != null && OnShowCommand.CanExecute(OnShowCommandParameter))
            {
                OnShowCommand.Execute(null);
            }
            OnPopupShow?.Invoke(this, EventArgs.Empty);
            if (TransitionAnimation != null)
            {
                TransitionAnimation.GetAnimation(this).Commit(this, AnimationName);
            }
        }

        internal void OnPopupHidden()
        {
            if (OnHideCommand != null && OnHideCommand.CanExecute(OnHideCommandParameter))
            {
                OnHideCommand.Execute(null);
            }
            OnPopupHide?.Invoke(this, EventArgs.Empty);
        }

        private static void OnIsPresentedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            PopupContentView popup = (PopupContentView)bindable;

            if (popup.Popup != null)
            {
                if ((bool)newValue)
                {
                    popup.Popup.Show(popup);
                }
                else
                {
                    popup.Popup.Hide(popup);
                }
            }
        }
    }
}
