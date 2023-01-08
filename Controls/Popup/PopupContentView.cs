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
        public static readonly BindablePropertyKey ShowCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(ShowCommand), typeof(ICommand), typeof(PopupContentView), null, defaultValueCreator: (b) =>
        {
            PopupContentView popupContentView = (PopupContentView)b;
            return new Command(popupContentView.Show);
        });
        public static readonly BindableProperty ShowCommandProperty = ShowCommandPropertyKey.BindableProperty;
        public static readonly BindablePropertyKey HideCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(HideCommand), typeof(ICommand), typeof(PopupContentView), null, defaultValueCreator: (b) =>
        {
            PopupContentView popupContentView = (PopupContentView)b;
            return new Command(popupContentView.Hide);
        });
        public static readonly BindableProperty HideCommandProperty = HideCommandPropertyKey.BindableProperty;
        public static readonly BindablePropertyKey ToggleCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(ToggleCommand), typeof(ICommand), typeof(PopupContentView), null, defaultValueCreator: (b) =>
        {
            PopupContentView popupContentView = (PopupContentView)b;
            return new Command(popupContentView.Toggle);
        });
        public static readonly BindableProperty ToggleCommandProperty = ToggleCommandPropertyKey.BindableProperty;
        public static readonly BindableProperty FadeBackgroundProperty = BindableProperty.Create(nameof(FadeBackground), typeof(bool), typeof(PopupContentView), true);
        public static readonly BindableProperty CloseOnBackgroundTapProperty = BindableProperty.Create(nameof(CloseOnBackgroundTap), typeof(bool), typeof(PopupContentView), true);

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
        public ICommand ShowCommand { get => (ICommand)GetValue(ShowCommandProperty); }
        public ICommand HideCommand { get => (ICommand)GetValue(HideCommandProperty); }
        public ICommand ToggleCommand { get => (ICommand)GetValue(ToggleCommandProperty); }

        public bool CloseOnBackgroundTap
        {
            get => (bool)GetValue(CloseOnBackgroundTapProperty);
            set => SetValue(CloseOnBackgroundTapProperty, value);
        }

        public bool FadeBackground
        {
            get => (bool)GetValue(FadeBackgroundProperty);
            set => SetValue(FadeBackgroundProperty, value);
        }


        public event EventHandler OnPopupShow;
        public event EventHandler OnPopupHide;

        private readonly Lazy<PlatformConfigurationRegistry<PopupContentView>> _platformConfigurationRegistry;

        public PopupContentView() : base()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<PopupContentView>>(() => new PlatformConfigurationRegistry<PopupContentView>(this));
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
                TransitionAnimation.GetAnimation(this)?.Commit(this, AnimationName, length: TransitionAnimation.Length, finished: (f, b) =>
                {
                    TransitionAnimation.OnFinished(this);
                });
            }
        }

        internal void OnPopupHidden()
        {
            if (OnHideCommand != null && OnHideCommand.CanExecute(OnHideCommandParameter))
            {
                OnHideCommand.Execute(null);
            }
            OnPopupHide?.Invoke(this, EventArgs.Empty);
            Animation anim = TransitionAnimation?.GetBackwardAnimation(this);
            if (anim != null)
            {
                anim.Commit(this, AnimationName, length: TransitionAnimation.Length, finished: (f, b) =>
                {
                    IsVisible = false;
                    TransitionAnimation.OnFinished(this);
                });
            }
            else
            {
                IsVisible = false;
            }
        }

        private static void OnIsPresentedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            PopupContentView popup = (PopupContentView)bindable;

            if (popup.Popup != null)
            {
                if ((bool)newValue)
                {
                    popup.IsVisible = true;
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
