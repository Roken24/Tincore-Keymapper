using System;
using System.Collections.Generic;
using RSG;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using static Unity.Messenger.Elements;
using Color = Unity.UIWidgets.ui.Color;
using static Unity.Messenger.Utils;

/**
 * Rather than pure build function, wrapped in Stateless
 * widget would provide separated StatelessElement which
 * also is buildContext.
 */
namespace Unity.Messenger.Widgets
{
    public class DiscoverChannelCard : StatefulWidget
    {
        internal readonly Channel channel;
        internal readonly Dictionary<string, Channel> channels;

        public DiscoverChannelCard(
            Channel channel,
            Dictionary<string, Channel> channels)
        {
            this.channel = channel;
            this.channels = channels;
        }

        public override State createState()
        {
            return new DiscoverChannelCardState();
        }
    }
    public class DiscoverChannelCardState : State<DiscoverChannelCard>
    {
        private static readonly Decoration NormalDecoration = new BoxDecoration(
            color: new Color(0xffffffff),
            border: new Border(
                bottom: new BorderSide(
                    color: new Color(0x15000000)
                )
            )
        );

        private static readonly EdgeInsets ThumbnailMargin = EdgeInsets.only(left: 16, right: 16);
        private static readonly EdgeInsets ChannelNameMargin = EdgeInsets.only(right: 16);

        private static readonly TextStyle NormalChannelNameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff212121),
            fontFamily: "PingFang",
            fontWeight: FontWeight.w500
        );

        private static readonly TextStyle NormalLastMessageStyle = new TextStyle(
            color: new Color(0xff797979),
            fontSize: 14,
            fontFamily: "PingFang",
            fontWeight: FontWeight.w500
        );

        private static readonly EdgeInsets LastMessageMargin = EdgeInsets.only(right: 16);
        private static readonly EdgeInsets LastMessageTimestampMargin = EdgeInsets.only(right: 16);
        private bool m_Joining;

        public override void initState()
        {
            base.initState();
            m_Joining = false;
        }

        public override Widget build(BuildContext context)
        {
            Widget iconWidgetChildren;
            if (m_Joining)
            {
                iconWidgetChildren = new Loading(size: 24);
            }
            else
            {
                iconWidgetChildren = new Icon(
                    IconFont.IconFontChevronRight,
                    size: 24,
                    color: new Color(0xff959595)
                );
            }
            return new GestureDetector(
                onTap: () =>
                {
                    if (widget.channels.ContainsKey(widget.channel.id))
                    {
                        var rootState = HomePage.of(context);
                        rootState.Select(widget.channel.id);
                    }
                    else
                    {
                        setState(() => m_Joining = true);
                        var requestUrl = string.IsNullOrEmpty(widget.channel.groupId) ? 
                            $"/api/connectapp/v1/channels/{widget.channel.id}/join" :
                            $"/api/connectapp/v1/groups/{widget.channel.groupId}/join";
                        
                        Post<JoinChannelResponse>(
                            requestUrl,
                            "{}"
                        ).Then(response =>
                        {
                            using (WindowProvider.of(context).getScope())
                            {
                                var state = HomePage.of(context);
                                var responseChannel = response.channel;
                                state.AddChannel(responseChannel);
                                state.Select(responseChannel.id);
                                state.Ack(responseChannel.id);
                                setState(() => m_Joining = false);
                            }
                        });
                    }
                },
                child: new Container(
                    height: 72,
                    alignment: Alignment.centerLeft,
                    decoration: NormalDecoration,
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Container(
                                margin: ThumbnailMargin,
                                child: CreateLobbyIcon(
                                    widget.channel.thumbnail,
                                    size: 48,
                                    radius: 4
                                )
                            ),
                            new Expanded(
                                child: new Container(
                                    height: 48,
                                    child: new Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget>
                                        {
                                            new Container(
                                                margin: ChannelNameMargin,
                                                height: 24,
                                                alignment: Alignment.centerLeft,
                                                child: new Text(
                                                    widget.channel.name,
                                                    style: NormalChannelNameStyle,
                                                    overflow: TextOverflow.ellipsis
                                                )
                                            ),
                                            new Container(
                                                height: 22,
                                                margin: LastMessageMargin,
                                                child: new Text(
                                                    $"{widget.channel.memberCount}成员",
                                                    style: NormalLastMessageStyle,
                                                    overflow: TextOverflow.ellipsis
                                                )
                                            )
                                        }
                                    )
                                )
                            ),
                            new Container(
                                margin: LastMessageTimestampMargin,
                                child: iconWidgetChildren
                            ),
                        }
                    )
                )
            );
        }
    }
}