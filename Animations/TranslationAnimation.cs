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
                if (weakView.TryGetTarget(out View target))
                {
                    //double result = 0;

                    //if (f < 0)
                    //{
                    //    result = Math.Abs(0 - target.X - target.Width) * f;
                    //}
                    //else if (f > 0)
                    //{
                    //    if (target.Parent is VisualElement parent)
                    //    {
                    //        result = (parent.Width - target.X) * f;
                    //    }
                    //    else
                    //    {
                    //        result = target.Width * f;
                    //    }
                    //}

                    target.TranslationX = f;
                }
            };
            Action<double> callback2 = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    //double result = 0;

                    //if (f < 0)
                    //{
                    //    result = Math.Abs(0 - target.Y - target.Height) * f;
                    //}
                    //else if (f > 0)
                    //{
                    //    if (target.Parent is VisualElement parent)
                    //    {
                    //        result = (parent.Height - target.Y) * f;
                    //    }
                    //    else
                    //    {
                    //        result = target.Height * f;
                    //    }
                    //}

                    target.TranslationY = f;
                }
            };
            return new Animation
                {
                    { 0.0, 1.0, new Animation(callback, GetTranslationXRatio(slide, From.X), GetTranslationXRatio(slide, To.X), Easing ?? Easing.Linear) },
                    { 0.0, 1.0, new Animation(callback2, GetTranslationYRatio(slide, From.Y), GetTranslationYRatio(slide, To.Y), Easing ?? Easing.Linear) }
                };
        }

        public override Animation GetBackwardAnimation(View slide)
        {
            WeakReference<View> weakView = new WeakReference<View>(slide);
            Action<double> callback = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    //double result = 0;

                    //if (f < 0)
                    //{
                    //    result = Math.Abs(0 - target.X - target.Width) * f;
                    //}
                    //else if (f > 0)
                    //{
                    //    if (target.Parent is VisualElement parent)
                    //    {
                    //        result = (parent.Width - target.X) * f;
                    //    }
                    //    else
                    //    {
                    //        result = target.Width * f;
                    //    }
                    //}

                    target.TranslationX = f;
                }
            };
            Action<double> callback2 = delegate (double f)
            {
                if (weakView.TryGetTarget(out View target))
                {
                    //double result = 0;

                    //if (f < 0)
                    //{
                    //    result = Math.Abs(0 - target.Y - target.Height) * f;
                    //}
                    //else if (f > 0)
                    //{
                    //    if (target.Parent is VisualElement parent)
                    //    {
                    //        result = (parent.Height - target.Y) * f;
                    //    }
                    //    else
                    //    {
                    //        result = target.Height * f;
                    //    }
                    //}

                    target.TranslationY = f;
                }
            };
            return new Animation
                {
                    { 0.0, 1.0, new Animation(callback, GetTranslationXRatio(slide, To.X), GetTranslationXRatio(slide, From.X), Easing ?? Easing.Linear) },
                    { 0.0, 1.0, new Animation(callback2, GetTranslationYRatio(slide, To.Y), GetTranslationYRatio(slide, From.Y), Easing ?? Easing.Linear) }
                };
        }

        public override void OnFinished(View slide)
        {
            slide.TranslationX = 0;
            slide.TranslationY = 0;
        }

        protected virtual double GetTranslationXRatio(View target, double f)
        {
            double result = 0;

            if (f < 0)
            {
                result = Math.Abs(0 - target.X - target.Width) * f;
            }
            else if (f > 0)
            {
                if (target.Parent is VisualElement parent)
                {
                    result = (parent.Width - target.X) * f;
                }
                else
                {
                    result = target.Width * f;
                }
            }
            return result + target.TranslationX;
        }

        protected virtual double GetTranslationYRatio(View target, double f)
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
            return result + target.TranslationY;
        }
    }
}
