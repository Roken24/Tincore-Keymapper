using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Widgets
{
    public class ChannelsDiscovery : StatelessWidget
    {
        private static readonly EdgeInsets IconPadding = EdgeInsets.only(left: 16);
        private static readonly EdgeInsets TextMargin = EdgeInsets.only(left: 8);
        private static readonly EdgeInsets RowMargin = EdgeInsets.only(bottom: 16);

        private static readonly BoxDecoration IconDecoration = new BoxDecoration(
            borderRadius: BorderRadius.all(3),
            border: Border.all(
                color: new Color(0xffcdcdcd)
            ),
            color: new Color(0xffffffff)
        );

        private static readonly TextStyle TextStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff000000),
            fontFamily: "PingFang"
        );

        private static readonly Color NormalColor = new Color(0x00000000);
        private static readonly Color SelectedColor = new Color(0xff2196f3);

        public ChannelsDiscovery(bool selected)
        {
            m_Selected = selected;
        }

        private readonly bool m_Selected;
        
        public override Widget build(BuildContext context)
        {
            return new GestureDetector(
                onTap: () =>
                {
                    if (!m_Selected)
                    {
                        HomePage.of(context).Select(string.Empty);
                    }
                },
                child: new Container(
                    height: 48,
                    alignment: Alignment.centerLeft,
                    margin: RowMargin,
                    padding: IconPadding,
                    color: m_Selected ? SelectedColor : NormalColor,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.start,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Container(
                                height: 32,
                                width: 32,
                                decoration: IconDecoration,
                                alignment: Alignment.center,
                                child: new Text(
                                    "+"
                                )
                            ),
                            new Container(
                                margin: TextMargin,
                                child: new Text(
                                    "发现频道",
                                    style: TextStyle
                                )
                            )
                        }
                    )
                ) 
            );
        }
    }
}