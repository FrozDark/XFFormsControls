using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class TreeView : Layout<ScrollView>
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(TreeView), propertyChanged: (b, o, n) =>
        {
            ((TreeView)b).OnItemsSourceChanged((IEnumerable)o, (IEnumerable)n);
        });

        public static readonly BindableProperty IndentationProperty = BindableProperty.Create(nameof(Indentation), typeof(double), typeof(TreeView), 30.0, BindingMode.Default);

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(TreeView), null, BindingMode.Default);

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public double Indentation
        {
            get => (double)GetValue(IndentationProperty);
            set => SetValue(IndentationProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }


        private readonly ScrollView _scrollView;
        private readonly StackLayout _stackLayout;

        public TreeView()
        {
            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.Fill;
            _stackLayout = new StackLayout();
            _scrollView = new ScrollView()
            {
                Orientation = ScrollOrientation.Both,
                Content = _stackLayout
            };

            Children.Add(_scrollView);
        }

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldNotify)
            {
                oldNotify.CollectionChanged -= ItemsSource_CollectionChanged;
            }
            if (newValue is INotifyCollectionChanged newNotify)
            {
                newNotify.CollectionChanged += ItemsSource_CollectionChanged;
            }

            _stackLayout.Children.Clear();
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            _scrollView.Layout(new Rectangle(x, y, width, height));
        }
    }
}
