using System;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class ScaleAnimation : AnimationControl
    {
        public double From { get; set; } = 0;
        public double To { get; set; } = 1;

        public ScaleAnimation()
        {

        }

        public ScaleAnimation(double from, double to)
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
                    target.Scale = f;
                }
            }
        }

        public override void OnFinished(View slide)
        {
            slide.Scale = 1;
        }
    }
}
