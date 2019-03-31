using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public static class MaterialD {
        public static bool debugCheckHasMaterial(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Material) && context.ancestorWidgetOfExactType(typeof(Material)) == null) {
                    string message = "No Material widget found.";
                    message += context.widget.GetType() + " widgets require a Material widget ancestor.";

                    message += "In material design, most widgets are conceptually \"printed\" on " +
                               "a sheet of material. In UIWidgets\'s material library, that " +
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
                            message += "\n  " + ancestor;
                        }
                    } else {
                        message += "This widget is the root of the tree, so it has no " +
                                   "ancestors, let alone a \"Material\" ancestor.";
                    }

                    throw new UIWidgetsError(message);
                }

                return true;
            });
            return true;
        }

        public static bool debugCheckHasMaterialLocalizations(BuildContext context) {
            D.assert(() => {
                if (Localizations.of<MaterialLocalizations>(context, typeof(MaterialLocalizations)) == null) {
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("No MaterialLocalizations found.");
                    message.AppendLine(
                        context.widget.GetType() + " widgets require MaterialLocalizations " +
                        "to be provided by a Localizations widget ancestor.");
                    message.AppendLine(
                        "Localizations are used to generate many different messages, labels," +
                        "and abbreviations which are used by the material library. ");
                    message.AppendLine(
                        "To introduce a MaterialLocalizations, either use a " +
                        " MaterialApp at the root of your application to include them " +
                        "automatically, or add a Localization widget with a " +
                        "MaterialLocalizations delegate.");
                    message.AppendLine(
                        "The specific widget that could not find a MaterialLocalizations ancestor was:"
                    );
                    message.AppendLine("  " + context.widget);
                    List<Widget> ancestors = new List<Widget>();
                    context.visitAncestorElements((Element element) => {
                        ancestors.Add(element.widget);
                        return true;
                    });
                    if (ancestors.isNotEmpty()) {
                        message.Append("The ancestors of this widget were:");
                        foreach (Widget ancestor in ancestors) {
                            message.Append("\n  " + ancestor);
                        }
                    } else {
                        message.AppendLine(
                            "This widget is the root of the tree, so it has no " +
                            "ancestors, let alone a \"Localizations\" ancestor."
                        );
                    }
                    throw new UIWidgetsError(message.ToString());
                }
                return true;
            });
            return true;
        }
    }
}