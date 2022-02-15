using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    [ContentProperty(nameof(Slides))]
    public class CarouselView : Layout
    {
        public const string SlideAnimationName = "CarouselSlide";

        public class SlideCollection : ObservableCollection<Slide>
        {

        }

        public static readonly BindableProperty LoopProperty = BindableProperty.Create(nameof(Loop), typeof(bool), typeof(CarouselView), true);
        public static readonly BindableProperty AllowDefaultAnimationsProperty = BindableProperty.Create(nameof(AllowDefaultAnimations), typeof(bool), typeof(CarouselView), true);
        public static readonly BindableProperty AllowSwipeGesturesProperty = BindableProperty.Create(nameof(AllowSwipeGestures), typeof(bool), typeof(CarouselView), true, propertyChanged: OnAllowSwipeChanged);
        public static readonly BindableProperty CurrentItemProperty = BindableProperty.Create(nameof(CurrentItem), typeof(object), typeof(CarouselView), null, BindingMode.TwoWay, propertyChanged: OnCurrentItemChanged);
        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(int), typeof(CarouselView), -1, BindingMode.TwoWay, propertyChanged: OnPositionPropertyChanged);
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CarouselView), propertyChanged: OnItemsSourceChanged);
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CarouselView), propertyChanged: OnItemTemplateChanged);
        public static readonly BindableProperty ForwardAnimationsProperty = BindableProperty.Create(nameof(ForwardAnimations), typeof(CarouselSlideAnimation), typeof(CarouselView));
        public static readonly BindableProperty BackwardAnimationsProperty = BindableProperty.Create(nameof(BackwardAnimations), typeof(CarouselSlideAnimation), typeof(CarouselView));
        public static readonly BindableProperty EmptyViewTemplateProperty = BindableProperty.Create(nameof(EmptyViewTemplate), typeof(DataTemplate), typeof(CarouselView), propertyChanged: OnEmptyViewPropertyChanged);


        public DataTemplate EmptyViewTemplate
        {
            get => (DataTemplate)GetValue(EmptyViewTemplateProperty);
            set => SetValue(EmptyViewTemplateProperty, value);
        }
        public bool AllowDefaultAnimations
        {
            get => (bool)GetValue(AllowDefaultAnimationsProperty);
            set => SetValue(AllowDefaultAnimationsProperty, value);
        }

        public CarouselSlideAnimation ForwardAnimations
        {
            get => (CarouselSlideAnimation)GetValue(ForwardAnimationsProperty);
            set => SetValue(ForwardAnimationsProperty, value);
        }
        public CarouselSlideAnimation BackwardAnimations
        {
            get => (CarouselSlideAnimation)GetValue(BackwardAnimationsProperty);
            set => SetValue(BackwardAnimationsProperty, value);
        }

        private Lazy<CarouselSlideAnimation> _fallbackForwardAnimations = new Lazy<CarouselSlideAnimation>(() =>
        {
            CarouselSlideAnimation animation = new CarouselSlideAnimation();

            animation.SlideIn.Add(new TranslationAnimation(1, 0, 0, 0));
            animation.SlideIn.Add(new ScaleAnimation(0, 1));

            animation.SlideOut.Add(new TranslationAnimation(0, 0, -1, 0));
            animation.SlideOut.Add(new ScaleAnimation(1, 0));

            return animation;
        });

        private Lazy<CarouselSlideAnimation> _fallbackBackwardAnimations = new Lazy<CarouselSlideAnimation>(() =>
        {
            CarouselSlideAnimation animation = new CarouselSlideAnimation();

            animation.SlideIn.Add(new TranslationAnimation(-1, 0, 0, 0));
            animation.SlideIn.Add(new ScaleAnimation(0, 1));

            animation.SlideOut.Add(new TranslationAnimation(0, 0, 1, 0));
            animation.SlideOut.Add(new ScaleAnimation(1, 0));

            return animation;
        });

        private ObservableCollection<Element> ChildsInternal { get => (ObservableCollection<Element>)Children; }

        private SlideCollection __slides = new SlideCollection();

        private int _currentPosition = -1;

        public IList<Slide> Slides { get => __slides; }

        private Lazy<View> _emptyView;

        public object CurrentItem
        {
            get => GetValue(CurrentItemProperty);
            set => SetValue(CurrentItemProperty, value);
        }

        public int Position
        {
            get => (int)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }

        public bool AllowSwipeGestures
        {
            get => (bool)GetValue(AllowSwipeGesturesProperty);
            set => SetValue(AllowSwipeGesturesProperty, value);
        }
        public IndicatorView IndicatorView
        {
            set
            {
                LinkToIndicatorView(this, value);
            }
        }

        private readonly SwipeGestureRecognizer leftGestureRecognizer;
        private readonly SwipeGestureRecognizer rightGestureRecognizer;

        public CarouselView()
        {
            IsClippedToBounds = true;

            _emptyView = new Lazy<View>(() =>
            {
                if (EmptyViewTemplate != null)
                {
                    return EmptyViewTemplate.CreateContent() as View;
                }
                return null;
            });

            leftGestureRecognizer = GetSwipeGestureRecognizer(SwipeDirection.Left);
            rightGestureRecognizer = GetSwipeGestureRecognizer(SwipeDirection.Right);

            UpdateSwipeGestures();

            __slides.CollectionChanged += Slides_CollectionChanged;

            VisualStateManager.GoToState(this, "Normal");
        }

        private void Slides_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Slide slide in e.NewItems)
                        {
                            if (slide.CarouselView is null)
                            {
                                slide.CarouselView = this;
                                slide.IsVisible = false;
                                ChildsInternal.Add(slide);
                            }
                            else
                            {
                                throw new InvalidOperationException("Slide of item is already set to another CarouselView");
                            }
                        if (__slides.Count > 0 && _currentPosition == -1)
                        {
                            SlideToPosition(0, true, false);
                        }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Slide slide in e.OldItems)
                        {
                            slide.CarouselView = null;
                            _ = ChildsInternal.Remove(slide);
                        }

                        if (Position > e.OldStartingIndex)
                        {
                            int newPosition = Position - e.OldItems.Count;
                            if (newPosition < 0)
                            {
                                newPosition = 0;
                            }
                            else if (newPosition >= __slides.Count)
                            {
                                newPosition = __slides.Count - 1;
                            }

                            Position = newPosition;
                        }
                        else if (Position == e.OldStartingIndex)
                        {
                            _currentPosition = -1;
                            if (__slides.Count == 0)
                            {
                                Position = -1;
                                CurrentItem = null;
                            }
                            else
                            {
                                int newPosition = Position - 1;
                                if (newPosition < 0)
                                {
                                    newPosition = 0;
                                }
                                SlideToPosition(newPosition);
                            }
                        }

                        //if (e.OldItems.Contains(__slides[Position]))
                        //{
                        //    if (_currentPosition == Position)
                        //    {
                        //        _currentPosition = -1;
                        //    }

                        //    int newPosition = Position - 1;
                        //    if (newPosition < 0)
                        //    {
                        //        newPosition = 0;
                        //    }
                        //    else if (newPosition >= __slides.Count)
                        //    {
                        //        newPosition = __slides.Count - 1;
                        //    }

                        //    if (Position == newPosition)
                        //    {
                        //        CurrentItem = __slides[newPosition].Context ?? __slides[newPosition];
                        //    }
                        //    else
                        //    {
                        //        Position = newPosition;
                        //    }
                        //}
                        break;
                    }
                default:
                    {
                        if (__slides.Count > 0)
                        {
                            SlideToPosition(0, true, false);
                        }
                        break;
                    }
            }
        }

        private void Col_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        AddSlides(e.NewItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (object oldItem in e.OldItems)
                        {
                            Slide slide = __slides.FirstOrDefault(c => c.Context.Equals(oldItem));
                            if (slide != null)
                            {
                                _ = __slides.Remove(slide);
                            }
                        }

                        break;
                    }
                default:
                    {
                        RefillViews();
                        break;
                    }
            }
            UpdateEmptyView();
        }

        private DateTime _swipeDatetime;
        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            if (DateTime.Now.Subtract(_swipeDatetime) <= TimeSpan.FromMilliseconds(100))
            {
                return;
            }

            _swipeDatetime = DateTime.Now;

            if (e.Direction == SwipeDirection.Right)
            {
                _ = SwipePrev();
            }
            else if (e.Direction == SwipeDirection.Left)
            {
                _ = SwipeNext();
            }
        }

        public void SwipeToPosition(int position, bool animate = true)
        {
            SlideToPosition(position, null, animate);
        }

        private async void SlideToPosition(int position, bool? isSlidingFoward = null, bool animate = true)
        {
            if (position == _currentPosition)
            {
                return;
            }

            if (!isSlidingFoward.HasValue)
            {
                isSlidingFoward = position > _currentPosition;
            }

            Slide fromSlide = _currentPosition != -1 ? __slides[_currentPosition] : null;
            Slide toSlide = position != -1 ? __slides[position] : null;

            _currentPosition = position;
            Position = _currentPosition;
            CurrentItem = __slides[_currentPosition].Context;

            if (fromSlide == toSlide)
            {
                return;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();

            if (fromSlide != null)
            {
                _ = fromSlide.AbortAnimation(SlideAnimationName);

                if (animate)
                {
                    if (isSlidingFoward.Value)
                    {
                        tasks.Add(CommitNextOutAnimation(fromSlide));
                    }
                    else
                    {
                        tasks.Add(CommitPrevOutAnimation(fromSlide));
                    }
                }
                else
                {
                    //_ = ChildsInternal.Remove(fromSlide);
                    fromSlide.IsVisible = false;
                }
            }
            if (toSlide != null)
            {
                _ = toSlide.AbortAnimation(SlideAnimationName);

                //if (!ChildsInternal.Contains(toSlide))
                //{
                //    ChildsInternal.Add(toSlide);
                //}
                toSlide.IsVisible = true;

                if (animate)
                {
                    if (isSlidingFoward.Value)
                    {
                        tasks.Add(CommitNextInAnimation(toSlide));
                    }
                    else
                    {
                        tasks.Add(CommitPrevInAnimation(toSlide));
                    }
                }
            }

            if (tasks.Count > 0)
            {
                VisualStateManager.GoToState(this, isSlidingFoward.Value ? "SlidingForward" : "SlidingBackward");

                _ = await Task.WhenAll(tasks);

                foreach (Slide child in ChildsInternal.Where(c => c != __slides[_currentPosition]).ToArray())
                {
                    if (!child.AnimationIsRunning(SlideAnimationName))
                    {
                        //_ = ChildsInternal.Remove(child);
                        child.IsVisible = false;
                    }
                }

                VisualStateManager.GoToState(this, "Default");
            }
        }

        private bool Swipe(bool swipeNext, bool animate)
        {
            if (__slides.Count == 0)
            {
                return false;
            }

            int next = swipeNext ? _currentPosition + 1 : _currentPosition - 1;
            if (next < 0)
            {
                if (!Loop)
                {
                    return false;
                }

                next = __slides.Count - 1;
            }
            else if (next >= __slides.Count)
            {
                if (!Loop)
                {
                    return false;
                }

                next = 0;
            }

            SlideToPosition(next, swipeNext, animate);

            return true;
        }

        public bool SwipePrev(bool animate = true)
        {
            return Swipe(false, animate);
        }

        public bool SwipeNext(bool animate = true)
        {
            return Swipe(true, animate);
        }

        private void RefillViews()
        {
            for (int i = 0; i < ChildsInternal.Count; i++)
            {
                ChildsInternal.RemoveAt(0);
            }

            __slides.Clear();
            _currentPosition = -1;
            CurrentItem = null;
            Position = -1;

            if (ItemsSource != null)
            {
                AddSlides(ItemsSource);
            }
        }

        private void AddSlides(IEnumerable slides)
        {
            if (ItemTemplate != null)
            {
                foreach (object item in slides)
                {
                    DataTemplate template = ItemTemplate;

                    if (ItemTemplate is DataTemplateSelector templateSelector)
                    {
                        template = templateSelector.SelectTemplate(item, this);
                    }

                    if (!template.Values.ContainsKey(BindingContextProperty))
                    {
                        template.Values.Add(BindingContextProperty, item);
                    }
                    else
                    {
                        template.Values[BindingContextProperty] = item;
                    }

                    View result = template.CreateContent() as View;

                    //result.SetValue(BindingContextProperty, item);

                    __slides.Add(new Slide(result, item));
                }
            }
            else if (slides is IEnumerable<View> views)
            {
                foreach (View view in views)
                {
                    __slides.Add(new Slide(view, view));
                }
            }
            else if (slides is IEnumerable<DataTemplate> templates)
            {
                foreach (DataTemplate template in templates)
                {
                    Slide result = new Slide((View)template.CreateContent(), template);

                    __slides.Add(result);
                }
            }
        }

        private Task<bool> CommitAnimation(Slide view, IList<AnimationControl> animations, uint rate, uint length)
        {
            if (animations is null || animations.Count == 0)
            {
                return Task.FromResult(false);
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            WeakReference<Slide> weakView = new WeakReference<Slide>(view);

            Animation anim = new Animation();
            foreach (AnimationControl carouselAnimation in animations)
            {
                anim.Add(0.0, 1.0, carouselAnimation.GetAnimation(view));
            }

            IEnumerable<AnimationControl> _lateHandler = animations.AsEnumerable();

            anim.Commit(view, SlideAnimationName, rate, length, null, delegate (double f, bool a)
            {
                tcs.SetResult(a);

                if (weakView.TryGetTarget(out Slide target))
                {
                    foreach (AnimationControl carouselAnimation in _lateHandler)
                    {
                        carouselAnimation.OnFinished(target);
                    }
                }
            });

            return tcs.Task;
        }

        protected virtual Task<bool> CommitPrevInAnimation(Slide view)
        {
            CarouselSlideAnimation slideAnimations = BackwardAnimations;
            if (slideAnimations is null)
            {
                if (!AllowDefaultAnimations)
                {
                    return Task.FromResult(false);
                }
                slideAnimations = _fallbackBackwardAnimations.Value;
            }
            return CommitAnimation(view, slideAnimations.SlideIn, slideAnimations.SlideInRate, slideAnimations.SlideInLength);
        }

        protected virtual Task<bool> CommitPrevOutAnimation(Slide view)
        {
            CarouselSlideAnimation slideAnimations = BackwardAnimations;
            if (slideAnimations is null)
            {
                if (!AllowDefaultAnimations)
                {
                    return Task.FromResult(false);
                }
                slideAnimations = _fallbackBackwardAnimations.Value;
            }
            return CommitAnimation(view, slideAnimations.SlideOut, slideAnimations.SlideOutRate, slideAnimations.SlideOutLength);
        }

        protected virtual Task<bool> CommitNextInAnimation(Slide view)
        {
            CarouselSlideAnimation slideAnimations = ForwardAnimations;
            if (slideAnimations is null)
            {
                if (!AllowDefaultAnimations)
                {
                    return Task.FromResult(false);
                }
                slideAnimations = _fallbackForwardAnimations.Value;
            }
            return CommitAnimation(view, slideAnimations.SlideIn, slideAnimations.SlideInRate, slideAnimations.SlideInLength);
        }

        protected virtual Task<bool> CommitNextOutAnimation(Slide view)
        {
            CarouselSlideAnimation slideAnimations = ForwardAnimations;
            if (slideAnimations is null)
            {
                if (!AllowDefaultAnimations)
                {
                    return Task.FromResult(false);
                }
                slideAnimations = _fallbackForwardAnimations.Value;
            }
            return CommitAnimation(view, slideAnimations.SlideOut, slideAnimations.SlideOutRate, slideAnimations.SlideOutLength);
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
            foreach (View view in Children)
            {
                //SizeRequest size = view.Measure(width, height);

                //double actualWidth = Math.Min(Math.Max(size.Minimum.Width, size.Request.Width), width);
                //double actualHeight = Math.Min(Math.Max(size.Minimum.Height, size.Request.Height), height);

                //LayoutChildIntoBoundingRegion(view, new Rectangle(width / 2 - actualWidth / 2, height / 2 - actualHeight / 2, actualWidth, actualHeight));
                LayoutChildIntoBoundingRegion(view, new Rectangle(x, y, width, height));
            }
        }

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return true;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return true;
        }

        private static void OnAllowSwipeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CarouselView view)
            {
                view.UpdateSwipeGestures();
            }
        }

        private void UpdateSwipeGestures()
        {
            if (AllowSwipeGestures)
            {
                leftGestureRecognizer.Swiped += SwipeGestureRecognizer_Swiped;
                rightGestureRecognizer.Swiped += SwipeGestureRecognizer_Swiped;

                GestureRecognizers.Add(leftGestureRecognizer);
                GestureRecognizers.Add(rightGestureRecognizer);
            }
            else
            {
                leftGestureRecognizer.Swiped -= SwipeGestureRecognizer_Swiped;
                rightGestureRecognizer.Swiped -= SwipeGestureRecognizer_Swiped;

                _ = GestureRecognizers.Remove(leftGestureRecognizer);
                _ = GestureRecognizers.Remove(rightGestureRecognizer);
            }
        }

        private void OnEmptyViewTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
        {
            if (_emptyView.IsValueCreated && _emptyView.Value != null)
            {
                _ = ChildsInternal.Remove(_emptyView.Value);
            }

            _emptyView = new Lazy<View>(() =>
            {
                if (newTemplate is null)
                {
                    return null;
                }

                return newTemplate.CreateContent() as View;
            });

            UpdateEmptyView();
        }

        private void UpdateEmptyView()
        {
            if (__slides.Count == 0)
            {
                if (_emptyView.Value != null && !ChildsInternal.Contains(_emptyView.Value))
                {
                    ChildsInternal.Add(_emptyView.Value);
                }
            }
            else if (_emptyView.IsValueCreated && _emptyView.Value != null)
            {
                _ = ChildsInternal.Remove(_emptyView.Value);
            }
        }

        private static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null && bindable is CarouselView view)
            {
                if (view.__slides.Count == 0)
                {
                    view.CurrentItem = null;
                    return;
                }

                if (newValue is null)
                {
                    throw new ArgumentNullException(nameof(CurrentItem));
                }

                Slide item = view.__slides.First(c => c.Context.Equals(newValue));

                int pos = view.__slides.IndexOf(item);
                if (pos != view._currentPosition)
                {
                    view.SlideToPosition(pos);
                }
            }
        }
        private static void OnEmptyViewPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CarouselView view)
            {
                view.OnEmptyViewTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
            }
        }
        private static void OnPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CarouselView view)
            {
                if (view.__slides.Count == 0)
                {
                    view.Position = 0;
                    return;
                }

                if ((int)oldValue == -1)
                {
                    view.SlideToPosition((int)newValue, true, false);
                }
                else if ((int)newValue != view._currentPosition)
                {
                    if ((int)newValue < 0 || (int)newValue != 0 && (int)newValue >= view.__slides.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(Position));
                    }
                    view.SlideToPosition((int)newValue);
                }
            }
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CarouselView view)
            {
                view.RefillViews();
                view.UpdateEmptyView();

                if (oldValue is INotifyCollectionChanged oldCol)
                {
                    oldCol.CollectionChanged -= view.Col_CollectionChanged;
                }
                if (newValue is INotifyCollectionChanged col)
                {
                    col.CollectionChanged += view.Col_CollectionChanged;
                }
            }
        }

        private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CarouselView view)
            {
                view.RefillViews();
            }
        }
        private static void LinkToIndicatorView(CarouselView carouselView, IndicatorView indicatorView)
        {
            if (indicatorView != null)
            {
                indicatorView.SetBinding(IndicatorView.PositionProperty, new Binding
                {
                    Path = nameof(Position),
                    Source = carouselView
                });
                indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, new Binding
                {
                    Path = nameof(ItemsSource),
                    Source = carouselView
                });
            }
        }
        private SwipeGestureRecognizer GetSwipeGestureRecognizer(SwipeDirection direction)
        {
            SwipeGestureRecognizer swipe = new SwipeGestureRecognizer { Direction = direction };
            swipe.Swiped += SwipeGestureRecognizer_Swiped;
            return swipe;
        }
    }
}
