using System;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class FadeAnimation : AnimationControl
    {
        public double From { get; set; } = 0;
        public double To { get; set; } = 1;

        public FadeAnimation()
        {

        }

        public FadeAnimation(double from, double to)
        {
            From = from;
            To = to;
        }

        public override Animation GetAnimation(View slide)
        {
            WeakReference<View> weakView = new WeakReference<View>(slide);
            return new Animation(UpdateProperty, From, To, Easing ?? Easing.Linear);
            void UpdateProperty(double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    target.Opacity = f;
                }
            }
        }

        public override void OnFinished(View slide)
        {
            slide.Opacity = 1;
        }
    }
}
