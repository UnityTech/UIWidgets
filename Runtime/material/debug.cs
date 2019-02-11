using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public static class MaterialDebug {
        public static bool debugCheckHasMaterial(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Material) && context.ancestorWidgetOfExactType(typeof(Material)) == null) {
                    string message = "No Material widget found.";
                    message += context.widget.GetType() + " widgets require a Material widget ancestor.";

                    message += "In material design, most widgets are conceptually \"printed\" on " +
                               "a sheet of material. In Flutter\'s material library, that " +
                               "material is represented by the Material widget. It is the " +
                               "Material widget that renders ink splashes, for instance. " +
                               "Because of this, many material library widgets require that " +
                               "there be a Material widget in the tree above them.";

                    message += "To introduce a Material widget, you can either directly " +
                               "include one, or use a widget that contains Material itself, " +
                               "such as a Card, Dialog, Drawer, or Scaffold.";

                    message += "The specific widget that could not find a Material ancestor was:";

                    message += context.widget.ToString();
                    List<Widget> ancestors = new List<Widget>();

                    context.visitAncestorElements((Element element) => {
                        ancestors.Add(element.widget);
                        return true;
                    });
                    if (ancestors.isNotEmpty()) {
                        message += "The ancestors of this widget were:";
                        foreach (Widget ancestor in ancestors) {
                            message += "\n  $ancestor";
                        }
                    }
                    else {
                        message += "This widget is the root of the tree, so it has no " +
                                   "ancestors, let alone a \"Material\" ancestor.";
                    }

                    throw new UIWidgetsError(message);
                }

                return true;
            });
            return true;
        }
    }
}