using Xamarin.Forms;

namespace XFFormsControls.Controls
{
    public sealed class Slide : ContentView
    {
        internal object Context { get; }
        public CarouselView CarouselView { get; internal set; } = null;

        public Slide()
        {

        }

        internal Slide(View view, object context)
        {
            Content = view;
            Context = context;
        }
    }
}
