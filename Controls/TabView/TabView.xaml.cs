using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XFFormsControls.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [ContentProperty(nameof(TabItems))]
    public partial class TabView : ContentView
    {
        public static readonly BindableProperty TabStripHeightProperty = BindableProperty.Create(nameof(TabStripHeight), typeof(GridLength), typeof(TabView), new GridLength(40, GridUnitType.Absolute), BindingMode.OneWay);
        public static readonly BindableProperty TabStripBackgroundColorProperty = BindableProperty.Create(nameof(TabStripBackgroundColor), typeof(Color), typeof(TabView), Color.Default, BindingMode.OneWay);

        public static readonly BindableProperty TabIndicatorColorProperty = BindableProperty.Create("TabIndicatorColor", typeof(Color), typeof(TabView), Color.Default, BindingMode.OneWay);

        public static readonly BindableProperty TabIndicatorHeightProperty = BindableProperty.Create("TabIndicatorHeight", typeof(double), typeof(TabView), 3.0, BindingMode.OneWay);

        public Color TabIndicatorColor
        {
            get
            {
                return (Color)GetValue(TabIndicatorColorProperty);
            }
            set
            {
                SetValue(TabIndicatorColorProperty, value);
            }
        }
        public double TabIndicatorHeight
        {
            get
            {
                return (double)GetValue(TabIndicatorHeightProperty);
            }
            set
            {
                SetValue(TabIndicatorHeightProperty, value);
            }
        }



        public static readonly BindableProperty IsSwipeEnabledProperty = BindableProperty.Create("IsSwipeEnabled", typeof(bool), typeof(TabView), true, BindingMode.OneWay);

        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create("SelectedIndex", typeof(int), typeof(TabView), -1, BindingMode.TwoWay);
        public ObservableCollection<TabViewItem> TabItems { get; } = new ObservableCollection<TabViewItem>();

        public GridLength TabStripHeight
        {
            get => (GridLength)GetValue(TabStripHeightProperty);
            set => SetValue(TabStripHeightProperty, value);
        }
        public Color TabStripBackgroundColor
        {
            get => (Color)GetValue(TabStripBackgroundColorProperty);
            set => SetValue(TabStripBackgroundColorProperty, value);
        }
        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }

        public bool IsSwipeEnabled
        {
            get
            {
                return (bool)GetValue(IsSwipeEnabledProperty);
            }
            set
            {
                SetValue(IsSwipeEnabledProperty, value);
            }
        }

        public Command<TabViewItem> SelectCommand { get; }

        readonly Lazy<PlatformConfigurationRegistry<TabView>> _platformConfigurationRegistry;
        public TabView()
        {
            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TabView>>(() => new PlatformConfigurationRegistry<TabView>(this));

            InitializeComponent();

            SelectCommand = new Command<TabViewItem>((tabitem) =>
            {
                int index = TabItems.IndexOf(tabitem);
                if (index != -1)
                {
                    SelectedIndex = index;
                }
            });

            TabItems.CollectionChanged += TabItems_CollectionChanged;
        }

        private void TabItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TabViewItem item in e.NewItems)
                {
                    SetInheritedBindingContext(item, BindingContext);
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            foreach (TabViewItem item in TabItems)
            {
                SetInheritedBindingContext(item, BindingContext);
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(SelectedIndex))
            {
                for (int i = 0; i < TabItems.Count; i++)
                {
                    TabViewItem item = TabItems[i];
                    if (i == SelectedIndex)
                    {
                        item.IsSelected = true;

                        tabStripScroller.ScrollToAsync(stripElementContent.Children[SelectedIndex], ScrollToPosition.MakeVisible, true);
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void Frame_SizeChanged(object sender, EventArgs e)
        {
            if (sender is Frame frame)
            {
                if (frame.Width < frame.Height)
                {
                    frame.WidthRequest = frame.Height;
                }
            }
        }
    }
}