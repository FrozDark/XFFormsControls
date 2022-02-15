using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public abstract class AnimationControl : Element
    {
        public virtual Easing Easing { get; set; } = Easing.Linear;

        public abstract Animation GetAnimation(View slide);

        public abstract void OnFinished(View slide);
    }
}
