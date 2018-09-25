using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using TextStyle = UIWidgets.painting.TextStyle;

namespace UIWidgets.widgets
{
    public class DefaultTextStyle : InheritedWidget
    {
        public DefaultTextStyle(TextStyle style, Widget child,
            TextAlign textAlign, int maxLines = 0,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip, Key key = null) : base(key, child)
        {
            D.assert(style != null);
            D.assert(child != null);
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.maxLines = maxLines;
            this.overflow = overflow;
        }

        public static DefaultTextStyle fallback()
        {
            return new DefaultTextStyle(new TextStyle(), null, TextAlign.left, 0, true,
                TextOverflow.clip, null);
        }

        public static Widget merge(Key key, TextStyle style, TextAlign? textAlign,
            bool? softWrap, TextOverflow? overflow, int? maxLines, Widget child)
        {
            D.assert(child != null);
            return new Builder(builder: (context =>
            {
                var parent = DefaultTextStyle.of(context);
                return new DefaultTextStyle(
                    key: key,
                    style: parent.style.merge(style),
                    textAlign: textAlign ?? parent.textAlign,
                    softWrap: softWrap ?? parent.softWrap,
                    overflow: overflow ?? parent.overflow,
                    maxLines: maxLines ?? parent.maxLines,
                    child: child
                );
            }));
        }

        public readonly TextStyle style;
        public readonly TextAlign textAlign;
        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly int maxLines;

        public static DefaultTextStyle of(BuildContext context)
        {
            var inherit = (DefaultTextStyle) context.inheritFromWidgetOfExactType(typeof(DefaultTextStyle));
            return inherit ?? DefaultTextStyle.fallback();
        }

        public override bool updateShouldNotify(InheritedWidget w)
        {
            var oldWidget = (DefaultTextStyle) w;
            return style != oldWidget.style ||
                   textAlign != oldWidget.textAlign ||
                   softWrap != oldWidget.softWrap ||
                   overflow != oldWidget.overflow ||
                   maxLines != oldWidget.maxLines;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            if (style != null)
            {
                style.debugFillProperties(properties);
            }

            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", overflow, defaultValue: null));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: null));
        }
    }

    public class Text : StatelessWidget
    {
        public Text(string data, Key key = null, TextStyle style = null,
            TextAlign? textAlign = null, bool? softWrap = null,
            TextOverflow? overflow = null, double? textScaleFactor = null) : base(key)
        {
            D.assert(data != null);
            this.textSpan = null;
            this.data = data;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
        }

        public Text(TextSpan textSpan, Key key = null, TextStyle style = null,
            TextAlign? textAlign = null, bool? softWrap = null,
            TextOverflow? overflow = null, double? textScaleFactor = null) : base(key)
        {
            D.assert(textSpan != null);
            this.textSpan = textSpan;
            this.data = null;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
        }

        public readonly string data;

        public readonly TextSpan textSpan;

        public readonly TextStyle style;

        public readonly TextAlign? textAlign;

        public readonly TextDirection? textDirection;

        public readonly bool? softWrap;

        public readonly TextOverflow? overflow;

        public readonly double? textScaleFactor;

        public readonly int? maxLines;

        public override Widget build(BuildContext context)
        {
            DefaultTextStyle defaultTextStyle = DefaultTextStyle.of(context);
            TextStyle effectiveTextStyle = style;
            if (style == null || style.inherit)
            {
                effectiveTextStyle = defaultTextStyle.style.merge(style);
            }

            return new RichText(
                textAlign: textAlign ?? defaultTextStyle.textAlign,
                softWrap: softWrap ?? defaultTextStyle.softWrap,
                overflow: overflow ?? defaultTextStyle.overflow,
                textScaleFactor: textScaleFactor ?? 1.0, // MediaQuery.textScaleFactorOf(context), todo
                maxLines: maxLines ?? defaultTextStyle.maxLines,
                text: new TextSpan(
                    style: effectiveTextStyle,
                    text: data,
                    children: textSpan != null ? new List<TextSpan>() {textSpan} : null
                )
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("data", data, showName: false));
            if (textSpan != null)
            {
                properties.add(textSpan.toDiagnosticsNode(name: "textSpan", style: DiagnosticsTreeStyle.transition));
            }

            if (style != null)
            {
                style.debugFillProperties(properties);
            }

            properties.add(new EnumProperty<TextAlign?>("textAlign", textAlign, defaultValue: null));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new FlagProperty("softWrap", value: softWrap ?? false, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true)); // todo ObjectFlagProperty
            properties.add(new EnumProperty<TextOverflow?>("overflow", overflow, defaultValue: null));
            properties.add(new DoubleProperty("textScaleFactor", textScaleFactor, defaultValue: null));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: null));
        }
    }
}