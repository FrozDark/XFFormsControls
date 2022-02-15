using System;
using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public class TranslationAnimation : AnimationControl
    {
        public Point From { get; set; } = new Point(-1, 0);
        public Point To { get; set; } = Point.Zero;

        public TranslationAnimation()
        {

        }

        public TranslationAnimation(Point from, Point to)
        {
            From = from;
            To = to;
        }

        public TranslationAnimation(double fromX, double fromY, double toX, double toY)
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
                    double result = 0;

                    if (f < 0)
                    {
                        result = Math.Abs(0 - target2.X - target2.Width) * f;
                    }
                    else if (f > 0)
                    {
                        if (target2.Parent is VisualElement parent)
                        {
                            result = (parent.Width - target2.X) * f;
                        }
                        else
                        {
                            result = target2.Width * f;
                        }
                    }

                    target2.TranslationX = result;
                }
            };
            Action<double> callback2 = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    double result = 0;

                    if (f < 0)
                    {
                        result = Math.Abs(0 - target.Y - target.Height) * f;
                    }
                    else if (f > 0)
                    {
                        if (target.Parent is VisualElement parent)
                        {
                            result = (parent.Height - target.Y) * f;
                        }
                        else
                        {
                            result = target.Height * f;
                        }
                    }

                    target.TranslationY = result;
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
            slide.TranslationX = 0;
            slide.TranslationY = 0;
        }
    }
}
