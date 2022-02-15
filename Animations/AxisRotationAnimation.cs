using System;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class AxisRotationAnimation : AnimationControl
    {
        public Point From { get; set; } = new Point(180, 0);
        public Point To { get; set; } = new Point(0, 0);

        public AxisRotationAnimation()
        {

        }

        public AxisRotationAnimation(Point from, Point to)
        {
            From = from;
            To = to;
        }

        public AxisRotationAnimation(double fromX, double fromY, double toX, double toY)
        {
            From = new Point(fromX, fromY);
            To = new Point(toX, toY);
        }

        public override Animation GetAnimation(View slide)
        {
            WeakReference<View> weakView = new WeakReference<View>(slide);
            Action<double> callback = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target2))
                {
                    target2.RotationX = f;
                }
            };
            Action<double> callback2 = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    target.RotationY = f;
                }
            };
            return new Animation
                {
                    { 0.0, 1.0, new Animation(callback, From.X, To.X, Easing ?? Easing.Linear) },
                    { 0.0, 1.0, new Animation(callback2, From.Y, To.Y, Easing ?? Easing.Linear) }
                };
        }

        public override void OnFinished(View slide)
        {
            slide.RotationX = 0;
            slide.RotationY = 0;
        }
    }
}
