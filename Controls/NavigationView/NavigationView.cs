using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class NavigationView : Layout<View>, IElementConfiguration<NavigationView>
	{
		private readonly List<View> _navList = new List<View>();
		private readonly Lazy<PlatformConfigurationRegistry<NavigationView>> _platformConfigurationRegistry;

        static readonly BindablePropertyKey BackCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(BackCommand), typeof(ICommand), typeof(NavigationView), null);
        public static readonly BindableProperty BackCommandProperty = BackCommandPropertyKey.BindableProperty;

		static readonly BindablePropertyKey BackToRootCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(BackToRootCommand), typeof(ICommand), typeof(NavigationView), null);
		public static readonly BindableProperty BackToRootCommandProperty = BackToRootCommandPropertyKey.BindableProperty;

		static readonly BindablePropertyKey PushCommandPropertyKey = BindableProperty.CreateReadOnly(nameof(PushCommand), typeof(ICommand), typeof(NavigationView), null);
		public static readonly BindableProperty PushCommandProperty = PushCommandPropertyKey.BindableProperty;

		static readonly BindablePropertyKey CurrentViewPropertyKey = BindableProperty.CreateReadOnly(nameof(CurrentView), typeof(View), typeof(NavigationView), null);
        public static readonly BindableProperty CurrentViewProperty = CurrentViewPropertyKey.BindableProperty;

		static readonly BindablePropertyKey CanGoBackPropertyKey = BindableProperty.CreateReadOnly(nameof(CanGoBack), typeof(bool), typeof(NavigationView), false, propertyChanged: OnCanGoBackPropertyChanged);
        public static readonly BindableProperty CanGoBackProperty = CanGoBackPropertyKey.BindableProperty;
		private static void OnCanGoBackPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			NavigationView navView = (NavigationView)bindable;
			((Command)navView.BackCommand).ChangeCanExecute();
			((Command)navView.BackToRootCommand).ChangeCanExecute();
		}

		public static readonly BindableProperty TitleViewProperty = BindableProperty.Create(nameof(TitleView), typeof(View), typeof(NavigationView), null, propertyChanged: OnTItleViewChanged);

		public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(NavigationView), 100u);

		private static void OnTItleViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
			NavigationView navView = bindable as NavigationView;

			if (oldValue != null)
			{
				navView.Children.Remove((View)oldValue);
			}
			if (newValue != null)
            {
				navView.Children.Insert(0, (View)newValue);
            }
		}

        public new bool IsClippedToBounds => base.IsClippedToBounds;

        public ICommand BackCommand => (ICommand)GetValue(BackCommandProperty);
        public ICommand BackToRootCommand => (ICommand)GetValue(BackToRootCommandProperty);
		public ICommand PushCommand => (ICommand)GetValue(PushCommandProperty);

		public uint AnimationDuration
		{
			get => (uint)GetValue(AnimationDurationProperty);
			set => SetValue(AnimationDurationProperty, value);
		}

		public View TitleView
		{
			get => (View)GetValue(TitleViewProperty);
			set => SetValue(TitleViewProperty, value);
		}

		public View CurrentView
        {
			get => (View)GetValue(CurrentViewProperty);
			private set => SetValue(CurrentViewPropertyKey, value);
		}

		public View RootView => _navList.FirstOrDefault();

		public bool CanGoBack
		{
			get => (bool)GetValue(CanGoBackProperty);
			private set => SetValue(CanGoBackPropertyKey, value);
		}

        public NavigationView()
        {
            SetValue(BackCommandPropertyKey, new Command(async () =>
			{
				await PopAsync();
			}, () => CanGoBack));

			SetValue(BackToRootCommandPropertyKey, new Command(async () =>
			{
				await PopToRootAsync();
			}, () => CanGoBack));

			SetValue(PushCommandPropertyKey, new Command(async (obj) =>
			{
				if (obj is null)
                {
					throw new ArgumentNullException(nameof(obj), "Invalid parameter passed as CommandParameter to PushCommand");
                }
				if (obj is Type type)
				{
					await PushAsync(type);
				}
                else
                {
					throw new ArgumentException($"Invalid parameter passed as CommandParameter to PushCommand ({obj}). Parameter should be passed as reference of View object or a type derived from View");
                }
			}));


			base.IsClippedToBounds = true;

            _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NavigationView>>(() => new PlatformConfigurationRegistry<NavigationView>(this));
        }

		public Task<View> PopAsync()
		{
			return PopAsync(true);
		}

		public async Task PopToRootAsync(bool animated = true)
        {
			if (!CanGoBack)
            {
				return;
            }

			View[] _viewToRemove = _navList.Skip(1).ToArray();

			await NavigateToView(RootView, animated);

			foreach (View view in _viewToRemove)
            {
				_navList.Remove(view);
				Children.Remove(view);
            }
        }

		private Task currentNavTask;
		public async Task<View> PopAsync(bool animated)
		{
			if (!CanGoBack)
            {
				return null;
			}

			View prevView = _navList[_navList.Count - 2];

			if (_navList.Remove(CurrentView))
			{
				CanGoBack = _navList.Count > 1;
			}

			var tcs = new TaskCompletionSource<bool>();
			try
			{
				if (currentNavTask != null && !currentNavTask.IsCompleted)
				{
					var oldTask = currentNavTask;
					currentNavTask = tcs.Task;
					await oldTask;
				}
				else
				{
					currentNavTask = tcs.Task;
				}

				View currentView = await NavigateToView(prevView, animated);

				Children.Remove(currentView);

				tcs.SetResult(true);

				return prevView;

				//tcs.SetResult(true);
				//return null;
			}
			catch (Exception)
			{
				currentNavTask = null;
				tcs.SetCanceled();

				throw;
			}
		}

		private Dictionary<Type, WeakReference<View>> _memCache = new Dictionary<Type, WeakReference<View>>();
		public T GetCacheTypedView<T>(bool create) where T : View
		{
			return (T)GetCacheTypedView(typeof(T), create);
		}

		public View GetCacheTypedView(Type type, bool create)
		{
			if (type.IsSubclassOf(typeof(View)))
			{
				if (!_memCache.TryGetValue(type, out WeakReference<View> weak) || !weak.TryGetTarget(out View view))
				{
					if (create)
					{
						view = (View)Activator.CreateInstance(type);

						_memCache[type] = new WeakReference<View>(view);
					}
					else
					{
						view = null;
					}
				}

				return view;
			}

			throw new ArgumentException("Type must be a subclass of View");
		}

		public async Task<View> PushAsync(Type type, bool animated = true)
		{
			if (type.IsSubclassOf(typeof(View)))
			{
				if (!_memCache.TryGetValue(type, out WeakReference<View> weak) || !weak.TryGetTarget(out View view))
				{
					view = (View)Activator.CreateInstance(type);

					_memCache[type] = new WeakReference<View>(view);
				}

				await PushAsync(view, animated);

				return view;
			}

			throw new ArgumentException("Type must be a subclass of View");
		}

		public async Task<T> PushAsync<T>(bool animated = true) where T : View
		{
			if (!_memCache.TryGetValue(typeof(T), out WeakReference<View> weak) || !weak.TryGetTarget(out View view))
			{
				view = (T)Activator.CreateInstance(typeof(T));

				_memCache[typeof(T)] = new WeakReference<View>(view);
			}

			await PushAsync(view, animated);

			return (T)view;
		}

		public Task PushAsync(View view)
		{
			return PushAsync(view, true);
		}

		private View pushingView = null;
		public async Task PushAsync(View view, bool animated)
		{
			if (view is null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			if (pushingView == view)
            {
                return;
			}

			if (!_navList.Contains(view))
			{
				_navList.Add(view);
				CanGoBack = _navList.Count > 1;
			}

			if (currentNavTask != null && !currentNavTask.IsCompleted)
			{
				//var tcs = new TaskCompletionSource<bool>();
				//Task oldTask = CurrentNavigationTask;
				//CurrentNavigationTask = tcs.Task;
				//await oldTask;

				//pushingView = view;
				//await NavigateToView(view, animated);
				//pushingView = null;
				//tcs.SetResult(true);

				return;
			}

			pushingView = view;

			currentNavTask = NavigateToView(view, animated);
			await currentNavTask;
			pushingView = null;
		}

		private async Task<View> NavigateToView(View view, bool animated)
        {
			if (CurrentView == view)
            {
                if (AnimationDuration > 0)
                {
                    await view.FadeTo(0, AnimationDuration);
                    await view.FadeTo(1, AnimationDuration);
                }
                OnPropertyChanged(nameof(CurrentView));

                return CurrentView;
			}

			View oldView = CurrentView;

			if (animated && AnimationDuration > 0)
			{
				view.Opacity = 0;
				view.IsVisible = true;

				if (!Children.Contains(view))
				{
					Children.Add(view);

					//if (view.BindingContext is null)
					//{
					//	SetInheritedBindingContext(view, BindingContext);
					//}
				}
                else
                {
                    //RaiseChild(view);
					_navList.Remove(view);
					_navList.Add(view);
				}

				if (oldView != null)
				{
					await oldView.FadeTo(0, AnimationDuration);
					oldView.IsVisible = false;
				}

				CurrentView = view;

				await view.FadeTo(1, AnimationDuration);
            }
            else
			{
				if (oldView != null)
				{
					oldView.IsVisible = false;
				}
				view.Opacity = 1;
				view.IsVisible = true;
				if (!Children.Contains(view))
				{
					Children.Add(view);

					//if (view.BindingContext is null)
					//{
					//	SetInheritedBindingContext(view, BindingContext);
					//}
				}
				else
				{
					//RaiseChild(view);
					_navList.Remove(view);
					_navList.Add(view);
				}
				CurrentView = view;
			}

			return oldView;
		}

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
			return child.IsVisible;
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
			Rectangle occupiedSize = Rectangle.Zero;
			if (TitleView != null)
            {
				Size measure = TitleView.Measure(width, height, MeasureFlags.None).Request;

				occupiedSize = new Rectangle(x, y, width, measure.Height);
				LayoutChildIntoBoundingRegion(TitleView, occupiedSize);
				//TitleView.Layout(occupiedSize);
            }

			Rectangle contentPlace = new Rectangle(x, y + occupiedSize.Height, width, height - occupiedSize.Height);
			foreach (View child in _navList)
            {
				LayoutChildIntoBoundingRegion(child, contentPlace);
            }
        }

        public IPlatformElementConfiguration<T, NavigationView> On<T>() where T : IConfigPlatform
        {
            return _platformConfigurationRegistry.Value.On<T>();
        }
    }
}
