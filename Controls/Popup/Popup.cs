using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XFFormsControls.Controls
{
    [ContentProperty(nameof(Content))]
    public class Popup : Layout<View>
    {
        public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(Popup), null, propertyChanged: OnContentPropertyChanged);
        public static readonly BindableProperty OnePopupOnlyProperty = BindableProperty.Create(nameof(OnePopupOnly), typeof(bool), typeof(Popup), true);
        public static readonly BindablePropertyKey HideAllCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(HideAllCommand), typeof(ICommand), typeof(Popup), null, defaultValueCreator: (b) =>
        {
            Popup popup = (Popup)b;
            return new Command(popup.HideAll);
        });
        public static readonly BindableProperty HideAllCommandProperty = HideAllCommandPropertyKey.BindableProperty;

        public ObservableCollection<PopupContentView> Popups { get; } = new ObservableCollection<PopupContentView>();

        private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Popup popup = (Popup)bindable;

            if (oldValue != null)
            {
                _ = popup.Children.Remove((View)oldValue);
            }
            if (newValue != null)
            {
                popup.Children.Insert(0, (View)newValue);
            }
        }

        public View Content
        {
            get => (View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public bool OnePopupOnly
        {
            get => (bool)GetValue(OnePopupOnlyProperty);
            set => SetValue(OnePopupOnlyProperty, value);
        }

        public ICommand HideAllCommand { get => (ICommand)GetValue(HideAllCommandProperty); }

        private BoxView _background = new BoxView()
        {
            IsVisible = false,
            Color = Color.Black,
            Opacity = 0.8
        };
        private bool _ignoreStack = false;

        private readonly Lazy<PlatformConfigurationRegistry<Popup>> _platformConfigurationRegistry;

        public Popup()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Popup>>(() => new PlatformConfigurationRegistry<Popup>(this));

            TapGestureRecognizer backgroundGesture = new TapGestureRecognizer()
            {
                NumberOfTapsRequired = 1
            };

            backgroundGesture.Tapped += Background_Tapped;

            _background.GestureRecognizers.Add(backgroundGesture);

            Children.Add(_background);

            Popups.CollectionChanged += Popups_CollectionChanged;
        }

        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        foreach (PopupContentView item in e.NewItems)
                        {
                            if (item.Popup != null && item.Popup != this)
                            {
                                throw new InvalidOperationException("PopupContentView belongs to another popup control!");
                            }

                            Children.Add(item);
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        _ignoreStack = true;
                        foreach (PopupContentView item in e.OldItems)
                        {
                            if (item.IsPresented)
                            {
                                Hide(item);
                            }
                            Children.Remove(item);
                        }
                        _ignoreStack = false;
                        CheckShownPopups();
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        _ignoreStack = true;
                        foreach (PopupContentView item in Children.OfType<PopupContentView>().ToArray())
                        {
                            if (item.IsPresented)
                            {
                                Hide(item);
                            }
                            Children.Remove(item);
                        }
                        _ignoreStack = false;
                        _background.IsVisible = false;
                        break;
                    }
            }
        }

        private double _backgroundFade = 0.8;
        public void Show(PopupContentView popup)
        {
            if (popup is null)
            {
                throw new ArgumentNullException(nameof(popup));
            }
            if (!Popups.Contains(popup))
            {
                throw new InvalidOperationException("Popup is not owned");
            }

            if (OnePopupOnly)
            {
                HideAll(popup, false);
            }

            if (popup.FadeBackground || popup.CloseOnBackgroundTap)
            {
                _background.IsVisible = true;
                RaiseChild(_background);

                if (popup.FadeBackground)
                {
                    if (_background.Opacity < _backgroundFade)
                    {
                        _background.Opacity = 0;
                        if (popup.TransitionAnimation != null)
                        {
                            _background.FadeTo(_backgroundFade, popup.TransitionAnimation.Length);
                        }
                        else
                        {
                            _background.Opacity = _backgroundFade;
                        }
                    }
                }
                else
                {
                    _background.Opacity = 0;
                }
            }

            //if (!_background.IsVisible && (FadeBackground || CloseOnBackgroundTap))
            //{
            //    _background.Opacity = 0;
            //    _background.IsVisible = true;
            //    if (FadeBackground)
            //    {
            //        if (popup.TransitionAnimation != null)
            //        {
            //            _background.FadeTo(_backgroundFade, popup.TransitionAnimation.Length);
            //        }
            //        else
            //        {
            //            _background.Opacity = _backgroundFade;
            //        }
            //    }
            //}

            RaiseChild(popup);
            popup.OnPopupShown();
        }

        public void Hide(PopupContentView popup)
        {
            if (popup is null)
            {
                throw new ArgumentNullException(nameof(popup));
            }
            if (!Popups.Contains(popup))
            {
                throw new InvalidOperationException("Popup is not owned");
            }

            if (!_ignoreStack)
            {
                CheckShownPopups();
            }

            popup.OnPopupHidden();
        }

        private void CheckShownPopups()
        {
            IEnumerable<PopupContentView> popupStack = Children.OfType<PopupContentView>().Where(c => c.IsPresented);

            if (popupStack.Count() == 0)
            {
                _background.IsVisible = false;
                _background.Opacity = 0;
            }
            else
            {
                bool hasFade = popupStack.Any(c => c.FadeBackground);
                bool hasCloseBackground = popupStack.Any(c => c.CloseOnBackgroundTap);

                if (!hasFade && !hasCloseBackground)
                {
                    _background.IsVisible = false;
                }
                else if (!hasFade && _background.Opacity > 0)
                {
                    _background.FadeTo(0);
                }

                PopupContentView lastPopup = popupStack.Last();

                if (lastPopup.CloseOnBackgroundTap || lastPopup.FadeBackground)
                {
                    RaiseChild(_background);
                }

                RaiseChild(lastPopup);
            }
        }

        public void HideAll()
        {
            HideAll(null, true);
        }

        private void HideAll(PopupContentView ignore, bool removeFade = true)
        {
            _ignoreStack = true;
            foreach (PopupContentView view in Children.OfType<PopupContentView>())
            {
                if (view != ignore)
                {
                    view.IsPresented = false;
                }
            }
            if (removeFade)
            {
                _background.IsVisible = false;
            }
            _ignoreStack = false;
        }

        private void Background_Tapped(object sender, EventArgs e)
        {
            PopupContentView lastShownPopup = Children.OfType<PopupContentView>().LastOrDefault(c => c.IsPresented);
            if (lastShownPopup.CloseOnBackgroundTap)
            {
                lastShownPopup.Hide();
            }
        }

        protected override void OnAdded(View view)
        {
            base.OnAdded(view);

            if (view is PopupContentView popup)
            {
                popup.Popup = this;
                popup.IsVisible = false;

                if (popup.IsPresented)
                {
                    Show(popup);
                }
            }
        }

        protected override void OnRemoved(View view)
        {
            base.OnRemoved(view);

            if (view is PopupContentView popup)
            {
                popup.Popup = null;
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size minimum = new Size(40.0, 40.0);
            double width = Math.Min(Device.Info.ScaledScreenSize.Width, widthConstraint);
            double height = Math.Min(Device.Info.ScaledScreenSize.Height, heightConstraint);
            return new SizeRequest(new Size(width, height), minimum);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            Rectangle fullScreen = new Rectangle(x, y, width, height);
            if (Content != null)
            {
                Content.Layout(fullScreen);
            }

            _background.Layout(fullScreen);

            foreach (PopupContentView view in Children.OfType<PopupContentView>())
            {
                SizeRequest size = view.Measure(width, height);

                double neededWidth = Math.Max(size.Minimum.Width, size.Request.Width);
                double neededHeight = Math.Max(size.Minimum.Height, size.Request.Height);

                if (view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
                {
                    neededWidth = width;
                }

                if (view.VerticalOptions.Alignment == LayoutAlignment.Fill)
                {
                    neededHeight = height;
                }

                double actualWidth = Math.Min(neededWidth, width);
                double actualHeight = Math.Min(neededHeight, height);

                //LayoutChildIntoBoundingRegion(view, new Rectangle(width / 2 - actualWidth / 2, height / 2 - actualHeight / 2, actualWidth, actualHeight));
                LayoutChildIntoBoundingRegion(view, fullScreen);
            }
        }
    }
}