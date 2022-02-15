using System.Collections.Generic;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class CarouselSlideAnimation : Element
    {
        public static readonly BindableProperty SlideInRateProperty = BindableProperty.Create(nameof(SlideInRate), typeof(uint), typeof(CarouselSlideAnimation), 16u);
        public static readonly BindableProperty SlideOutRateProperty = BindableProperty.Create(nameof(SlideOutRate), typeof(uint), typeof(CarouselSlideAnimation), 16u);
        public static readonly BindableProperty SlideInLengthProperty = BindableProperty.Create(nameof(SlideInLength), typeof(uint), typeof(CarouselSlideAnimation), 250u);
        public static readonly BindableProperty SlideOutLengthProperty = BindableProperty.Create(nameof(SlideOutLength), typeof(uint), typeof(CarouselSlideAnimation), 250u);

        public uint SlideInRate
        {
            get => (uint)GetValue(SlideInRateProperty);
            set => SetValue(SlideInRateProperty, value);
        }
        public uint SlideOutRate
        {
            get => (uint)GetValue(SlideOutRateProperty);
            set => SetValue(SlideOutRateProperty, value);
        }
        public uint SlideInLength
        {
            get => (uint)GetValue(SlideInLengthProperty);
            set => SetValue(SlideInLengthProperty, value);
        }
        public uint SlideOutLength
        {
            get => (uint)GetValue(SlideOutLengthProperty);
            set => SetValue(SlideOutLengthProperty, value);
        }

        private readonly List<AnimationControl> _slideIn = new List<AnimationControl>();
        private readonly List<AnimationControl> _slideOut = new List<AnimationControl>();

        public IList<AnimationControl> SlideIn { get => _slideIn; }
        public IList<AnimationControl> SlideOut { get => _slideOut; }
    }
}
