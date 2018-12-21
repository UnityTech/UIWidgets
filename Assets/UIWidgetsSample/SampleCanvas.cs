using UIWidgets.engine;
using UIWidgets.widgets;

namespace UIWidgetsSample
{
    public class SampleCanvas:WidgetCanvas
    {
        protected override Widget getWidget()
        {
            return new AsScreen();
        }
    }
}
