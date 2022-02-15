using System;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class RotationAnimation : AnimationControl
    {
        public double From { get; set; } = 180;
        public double To { get; set; } = 0;

        public RotationAnimation()
        {

        }

        public RotationAnimation(double from, double to)
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
                    target.Rotation = f;
                }
            }
        }

        public override void OnFinished(View slide)
        {
            slide.Rotation = 0;
        }
    }
}
