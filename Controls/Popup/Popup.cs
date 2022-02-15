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
    public class Popup : Layout
    {
        public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(Popup), null, propertyChanged: OnContentPropertyChanged);
        public static readonly BindableProperty CloseOnBackgroundTapProperty = BindableProperty.Create(nameof(CloseOnBackgroundTap), typeof(bool), typeof(Popup), true);

        public ObservableCollection<PopupContentView> Popups { get; } = new ObservableCollection<PopupContentView>();

        private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Popup popup = (Popup)bindable;

            if (oldValue != null)
            {
                _ = popup.ChildsInternal.Remove((View)oldValue);
            }
            if (newValue != null)
            {
                popup.ChildsInternal.Insert(0, (View)newValue);
            }
        }


        public View Content
        {
            get => (View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public bool CloseOnBackgroundTap
        {
            get => (bool)GetValue(CloseOnBackgroundTapProperty);
            set => SetValue(CloseOnBackgroundTapProperty, value);
        }
        public Command HideAllCommand { get; }

        private IList<Element> ChildsInternal { get => (IList<Element>)Children; }

        private BoxView _background;

        public Popup()
        {
            HideAllCommand = new Command(HideAll);

            _background = new BoxView()
            {
                IsVisible = false,
                Color = Color.Black,
                Opacity = 0.8
            };

            TapGestureRecognizer backgroundGesture = new TapGestureRecognizer()
            {
                NumberOfTapsRequired = 1
            };

            backgroundGesture.Tapped += Background_Tapped;

            _background.GestureRecognizers.Add(backgroundGesture);

            ChildsInternal.Add(_background);

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

                            item.IsVisible = false;
                            ChildsInternal.Add(item);

                            item.Popup = this;

                            if (item.IsPresented)
                            {
                                Show(item);
                            }
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        foreach (PopupContentView item in e.OldItems)
                        {
                            if (item.IsPresented)
                            {
                                Hide(item);
                            }
                            ChildsInternal.Remove(item);
                            item.Popup = null;
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        foreach (PopupContentView item in ChildsInternal.OfType<PopupContentView>().ToArray())
                        {
                            if (item.IsPresented)
                            {
                                Hide(item);
                            }
                            item.Popup = null;
                            ChildsInternal.Remove(item);
                        }
                        break;
                    }
            }
        }

        public void Show(PopupContentView popup)
        {
            if (popup == null)
            {
                throw new ArgumentNullException(nameof(popup));
            }
            if (!Popups.Contains(popup))
            {
                throw new InvalidOperationException("Popup is not owned");
            }

            popup.IsVisible = true;
            _background.IsVisible = true;

            RaiseChild(popup);
            popup.OnPopupShown();
        }

        public void Hide(PopupContentView popup)
        {
            if (popup == null)
            {
                throw new ArgumentNullException(nameof(popup));
            }
            if (!Popups.Contains(popup))
            {
                throw new InvalidOperationException("Popup is not owned");
            }

            popup.IsVisible = false;

            IEnumerable<PopupContentView> popupStack = ChildsInternal.OfType<PopupContentView>().Where(c => c.IsPresented);

            if (popupStack.Count() == 0)
            {
                _background.IsVisible = false;
            }
            else
            {
                RaiseChild(popupStack.Last());
            }

            popup.OnPopupHidden();
        }

        public void Toggle(PopupContentView popup)
        {
            if (popup == null)
            {
                throw new ArgumentNullException(nameof(popup));
            }
            if (!Popups.Contains(popup))
            {
                throw new InvalidOperationException("Popup is not owned");
            }

            popup.IsPresented = !popup.IsPresented;
        }

        public void HideAll()
        {
            foreach (PopupContentView view in ChildsInternal.OfType<PopupContentView>())
            {
                view.IsPresented = false;
            }
        }

        private void Background_Tapped(object sender, EventArgs e)
        {
            if (CloseOnBackgroundTap)
            {
                foreach (PopupContentView view in ChildsInternal.OfType<PopupContentView>())
                {
                    view.IsPresented = false;
                }
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

            foreach (PopupContentView view in ChildsInternal.OfType<PopupContentView>())
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

                LayoutChildIntoBoundingRegion(view, new Rectangle(width / 2 - actualWidth / 2, height / 2 - actualHeight / 2, actualWidth, actualHeight));
            }
        }
    }
}