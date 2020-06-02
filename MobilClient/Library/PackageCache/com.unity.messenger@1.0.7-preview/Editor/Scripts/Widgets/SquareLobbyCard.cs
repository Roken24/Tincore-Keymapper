using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using WebSocketSharp;
using static Unity.Messenger.Elements;
using static Unity.Messenger.Utils;

namespace Unity.Messenger.Widgets
{
    public class SquareLobbyCard : StatefulWidget
    {
        internal readonly Channel channel;
        internal readonly Dictionary<string, Channel> channels;
        internal readonly Dictionary<string, Group> groups;
        internal readonly float width;

        public SquareLobbyCard(
            Channel channel,
            Dictionary<string, Channel> channels,
            Dictionary<string, Group> groups,
            float width)
        {
            this.channels = channels;
            this.channel = channel;
            this.groups = groups;
            this.width = width;
        }

        public override State createState()
        {
            return new SquareLobbyCardState();
        }
    }

    internal class SquareLobbyCardState : State<SquareLobbyCard>
    {
        private static readonly BoxDecoration Decoration = new BoxDecoration(
            borderRadius: BorderRadius.circular(8),
            color: new Color(0xffffffff),
            border: Border.all(
                color: new Color(0xffd8d8d8),
                width: 1
            )
        );

        private static readonly TextStyle NameTextStyle = new TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            fontFamily: "PingFang"
        );

        private static readonly TextStyle MemberCountTextStyle = new TextStyle(
            fontSize: 12,
            color: new Color(0x80000000),
            fontFamily: "PingFang"
        );

        private static readonly EdgeInsets TopicMargin = EdgeInsets.symmetric(vertical: 24);

        private static readonly TextStyle TopicTextStyle = new TextStyle(
            fontSize: 14,
            height: 22 / 22.4f,
            fontFamily: "PingFang"
        );

        private bool m_Joining;

        public override void initState()
        {
            base.initState();
            m_Joining = false;
        }

        public override Widget build(BuildContext context)
        {
            var topic = widget.channel.topic;
            if (!widget.channel.groupId.IsNullOrEmpty())
            {
                topic = widget.groups[widget.channel.groupId].description;
            }

            var buttonChildren = new List<Widget>();
            if (m_Joining)
            {
                buttonChildren.Add(
                    new Text(
                        widget.channels.ContainsKey(widget.channel.id) ? "查看群聊" : "立即加入",
                        style: new TextStyle(
                            color: new Color(0x00000000),
                            fontSize: 14,
                            fontFamily: "PingFang"
                        )
                    )
                );
                buttonChildren.Add(
                    new Loading(
                        size: 24
                    )
                );
            }
            else
            {
                buttonChildren.Add(
                    new Text(
                        widget.channels.ContainsKey(widget.channel.id) ? "查看群聊" : "立即加入",
                        style: new TextStyle(
                            color: new Color(0xff2196f3),
                            fontSize: 14,
                            fontFamily: "PingFang"
                        )
                    )
                );
            }

            return new Container(
                decoration: Decoration,
                height: 280,
                width: widget.width,
                margin: EdgeInsets.all(8),
                padding: EdgeInsets.only(top: 24, left: 16, right: 16, bottom: 16),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>
                    {
                        new Row(
                            children: new List<Widget>
                            {
                                CreateLobbyIcon(
                                    widget.channel.thumbnail,
                                    radius: 4,
                                    size: 48
                                ),
                                new Expanded(
                                    child: new Container(
                                        height: 48,
                                        margin: EdgeInsets.only(left: 16),
                                        child: new Column(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget>
                                            {
                                                new Container(
                                                    height: 24,
                                                    alignment: Alignment.centerLeft,
                                                    child: new Text(
                                                        widget.channel.name,
                                                        style: NameTextStyle,
                                                        overflow: TextOverflow.ellipsis
                                                    )
                                                ),
                                                new Container(
                                                    height: 22,
                                                    alignment: Alignment.centerLeft,
                                                    child: new Text(
                                                        $"{widget.channel.memberCount}成员",
                                                        style: MemberCountTextStyle
                                                    )
                                                )
                                            }
                                        )
                                    )
                                ),
                            }
                        ),
                        new Expanded(
                            child: new Container(
                                margin: TopicMargin,
                                child: new RichText(
                                    text: ParseMessage(topic, null, TopicTextStyle),
                                    maxLines: 5,
                                    overflow: TextOverflow.ellipsis
                                )
                            )
                        ),
                        new GestureDetector(
                            onTap: () =>
                            {
                                if (widget.channels.ContainsKey(widget.channel.id))
                                {
                                    HomePage.of(context).Select(widget.channel.id);
                                }
                                else
                                {
                                    setState(() => m_Joining = true);
                                    var requestUrl = string.IsNullOrEmpty(widget.channel.groupId)
                                        ? $"/api/connectapp/v1/channels/{widget.channel.id}/join"
                                        : $"/api/connectapp/v1/groups/{widget.channel.groupId}/join";
                                    Post<JoinChannelResponse>(
                                        requestUrl,
                                        "{}"
                                    ).Then(response =>
                                    {
                                        using (WindowProvider.of(context).getScope())
                                        {
                                            if (mounted)
                                            {
                                                var state = HomePage.of(context);
                                                var responseChannel = response.channel;
                                                state.AddChannel(responseChannel);
                                                state.Select(responseChannel.id);
                                                state.Ack(responseChannel.id);
                                                setState(() => m_Joining = false);
                                            }
                                        }
                                    });
                                }
                            },
                            child: new Container(
                                height: 32,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.circular(8),
                                    color: new Color(0xffffffff),
                                    border: Border.all(
                                        color: new Color(0xffd8d8d8)
                                    )
                                ),
                                alignment: Alignment.center,
                                child: new Stack(
                                    alignment: Alignment.center,
                                    children: buttonChildren
                                )
                            )
                        )
                    }
                )
            );
        }
    }
}