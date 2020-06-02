using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.Messenger.Style;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Widgets
{
    public class ChattingWindowHeader : StatelessWidget
    {
        public ChattingWindowHeader(
            Models.Channel channel,
            System.Action onBack
        )
        {
            m_Channel = channel;
            m_OnBack = onBack;
        }

        private readonly Models.Channel m_Channel;
        private readonly System.Action m_OnBack;

        private static readonly EdgeInsets Padding = EdgeInsets.only(
            left: 24,
            right: 24
        );

        private static readonly BoxDecoration Decoration = new BoxDecoration(
            color: new Color(0xffffffff),
            border: new Border(
                bottom: new BorderSide(
                    color: new Color(0xffd8d8d8)
                )
            )
        );

        private static readonly TextStyle ChannelNameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff000000),
            fontWeight: FontWeight.bold,
            fontFamily: "PingFang"
        );

        private static readonly Container UserIcon = new Container(
            margin: EdgeInsets.only(right: 4),
            child: new Icon(
                IconFont.IconFontUser,
                size: 12,
                color: new Color(0xff231916)
            )
        );

        private static readonly TextStyle MemberCountStyle = new TextStyle(
            fontSize: 14,
            color: new Color(0xff797979),
            fontFamily: "PingFang"
        );

        private static readonly Icon SettingsIcon = new Icon(
            IconFont.IconFontSettingsHoriz,
            color: new Color(0xff979a9e),
            size: 28
        );

        public override Widget build(BuildContext context)
        {
            var firstRowChildren = new List<Widget>
            {
                new Container(
                    height: 24,
                    alignment: Alignment.centerLeft,
                    child: new Text(
                        m_Channel.name,
                        style: ChannelNameStyle
                    )
                ),
            };
            if (m_Channel.isMute)
            {
                firstRowChildren.Add(
                    new Container(
                        margin: EdgeInsets.only(left: 6),
                        child: new Icon(
                            IconFont.IconFontBell,
                            size: 16,
                            color: new Color(0xffc7cbcf)
                        )
                    )
                );
            }

            var leftChildren = new List<Widget>
            {
                new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget>
                    {
                        new Row(
                            children: firstRowChildren
                        ),
                        new Container(
                            height: 22,
                            alignment: Alignment.centerLeft,
                            child: new Text(
                                $"{m_Channel.memberCount}成员",
                                style: MemberCountStyle
                            )
                        ),
                    }
                ),
            };
            if (MediaQuery.of(context).size.width < 750)
            {
                leftChildren.Insert(
                    0,
                    new GestureDetector(
                        onTap: () => m_OnBack(),
                        child: new Container(
                            margin: EdgeInsets.only(right: 12),
                            child: new Icon(
                                IconFont.IconFontArrowBack,
                                size: 28,
                                color: new Color(0xff979a9e)
                            )
                        )
                    )
                );
            }

            var children = new List<Widget>
            {
                new Expanded(
                    child: new Row(
                        children: leftChildren
                    )
                ),
                new GestureDetector(
                    child: new Container(
                        width: 28,
                        height: 28,
                        alignment: Alignment.center,
                        child: SettingsIcon
                    ),
                    onTap: () => HomePage.of(context).ShowChannelInfo()
                ),
            };

            return new Container(
                padding: Padding,
                decoration: Decoration,
                height: 64,
                child: new Row(
                    children: children
                )
            );
        }
    }
}