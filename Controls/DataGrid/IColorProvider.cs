using Xamarin.Forms;

namespace XFFormsControls.Controls
{
	public interface IColorProvider
	{
		Color GetColor(int rowIndex, object item);
	}
}
