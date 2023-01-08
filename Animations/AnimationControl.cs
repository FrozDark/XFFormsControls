using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public abstract class AnimationControl : Element
    {
        public virtual Easing Easing { get; set; } = Easing.Linear;
        public virtual uint Length { get; set; } = 250u;

        public abstract Animation GetAnimation(View slide);
        public virtual Animation GetBackwardAnimation(View slide) => null;

        public abstract void OnFinished(View slide);
    }
}
