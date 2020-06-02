using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using WebSocketSharp;
using Color = Unity.UIWidgets.ui.Color;
using static Unity.Messenger.Elements;
using Scroller = Unity.Messenger.Widgets.Scroller;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.Messenger.Components
{
    public class ChannelBrief : StatefulWidget
    {
        public ChannelBrief(
            string channelId
        ) : base(key: new GlobalObjectKey<ChannelBriefState>(channelId))
        {
            this.channelId = channelId;
        }

        internal readonly string channelId;

        public override State createState()
        {
            return new ChannelBriefState();
        }
    }

    internal class ChannelBriefState : SingleTickerProviderStateMixin<ChannelBrief>
    {
        private AnimationController m_AnimationController;
        private Channel channel;
        private Group group;
        private bool m_Joining;

        public override void initState()
        {
            base.initState();
            m_Joining = false;
            m_AnimationController = new AnimationController(
                vsync: this,
                duration: new TimeSpan(0, 0, 0, 0, milliseconds: 240)
            );
            m_AnimationController.addListener(() => setState());
            m_AnimationController.addStatusListener(AnimationStatusListener);
            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                if (MediaQuery.of(context).size.width < 750)
                {
                    m_AnimationController.forward();
                }
            });
            Utils.Get<GetChannelResponse>(
                    $"/api/connectapp/v1/channels/{widget.channelId}")
                .Then(response =>
                {
                    using (WindowProvider.of(context).getScope())
                    {
                        channel = response.channel;
                        group = response.groupFull;
                        setState();
                    }
                });
        }

        public override void dispose()
        {
            m_AnimationController.dispose();
            base.dispose();
        }

        private void AnimationStatusListener(AnimationStatus status)
        {
            if (status == AnimationStatus.dismissed)
            {
                HomePage.of(this.context).HideChannelBrief();
            }
        }

        public override void didChangeDependencies()
        {
            base.didChangeDependencies();

            var screenWidth = MediaQuery.of(context).size.width;
            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                if (screenWidth < 750 && !m_AnimationController.isAnimating)
                {
                    m_AnimationController.animateTo(1, duration: TimeSpan.Zero);
                }
            });
        }

        public override Widget build(BuildContext context)
        {
            var screenSize = MediaQuery.of(context).size;
            var screenWidth = screenSize.width;
            var screenHeight = screenSize.height;

            var headerChildren = new List<Widget>
            {
                new Expanded(
                    child: new Text(
                        $"{channel?.name ?? string.Empty}",
                        style: new TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w500,
                            color: new Color(0xff212121),
                            fontFamily: "PingFang"
                        )
                    )
                ),
            };

            headerChildren.Add(
                new GestureDetector(
                    onTap: () =>
                    {
                        if (screenWidth < 750)
                        {
                            m_AnimationController.reverse();
                        }
                        else
                        {
                            HomePage.of(this.context).HideChannelInfo();
                        }
                    },
                    child: new Icon(
                        IconFont.IconFontClose,
                        color: new Color(0xff979a9e),
                        size: 28
                    )
                )
            );

            var children = new List<Widget> { };
            if (channel == null)
            {
                children.Add(
                    new Loading(size: 56)
                );
            }
            else
            {
                children.Add(
                    CreateLobbyIcon(
                        channel.thumbnail,
                        size: 184,
                        radius: 4
                    )
                );
                children.Add(
                    new Container(
                        margin: EdgeInsets.only(top: 32),
                        child: new Text(
                            channel.name,
                            style: new TextStyle(
                                color: new Color(0xff000000),
                                fontSize: 24,
                                fontWeight: FontWeight.bold
                            )
                        )
                    )
                );
                children.Add(
                    new Container(
                        margin: EdgeInsets.only(top: 8),
                        child: new Text(
                            $"{channel.memberCount}成员",
                            style: new TextStyle(
                                color: new Color(0xff797979),
                                fontSize: 14
                            )
                        )
                    )
                );
                var topic = channel.topic;
                if (!channel.groupId.IsNullOrEmpty() && group != null)
                {
                    topic = group.description;
                }

                children.Add(
                    new Container(
                        margin: EdgeInsets.only(top: 32),
                        child: new Text(
                            topic,
                            style: new TextStyle(
                                color: new Color(0xff000000),
                                fontSize: 14
                            )
                        )
                    )
                );
                var buttonChildren = new List<Widget>();
                if (m_Joining)
                {
                    buttonChildren.Add(
                        new Text(
                            "加入群聊",
                            style: new TextStyle(
                                color: new Color(0x00000000),
                                fontSize: 18,
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
                            "加入群聊",
                            style: new TextStyle(
                                fontSize: 18,
                                color: new Color(0xff2196f3),
                                fontFamily: "PingFang"
                            )
                        )
                    );
                }

                children.Add(
                    new GestureDetector(
                        onTap: () =>
                        {
                            if (m_Joining)
                            {
                                return;
                            }
                            setState(() => m_Joining = true);
                            var requestUrl = string.IsNullOrEmpty(channel.groupId)
                                ? $"/api/connectapp/v1/channels/{channel.id}/join"
                                : $"/api/connectapp/v1/groups/{channel.groupId}/join";
                            Utils.Post<JoinChannelResponse>(
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
                        },
                        child: new Container(
                            height: 56,
                            width: 234,
                            margin: EdgeInsets.only(top: 48),
                            decoration: new BoxDecoration(
                                color: new Color(0xffffffff),
                                borderRadius: BorderRadius.circular(6),
                                border: Border.all(
                                    color: new Color(0xff2196f3)
                                )
                            ),
                            alignment: Alignment.center,
                            child: new Stack(
                                alignment: Alignment.center,
                                children: buttonChildren
                            )
                        )
                    )
                );
            }

            Widget all = new Container(
                color: new Color(0xffffffff),
                child: new Scroller(
                    child: new SingleChildScrollView(
                        child: new Column(
                            children: new List<Widget>
                            {
                                new Container(
                                    height: 64,
                                    decoration: new BoxDecoration(
                                        border: new Border(
                                            bottom: new BorderSide(
                                                color: new Color(0xffd8d8d8)
                                            )
                                        )
                                    ),
                                    padding: EdgeInsets.symmetric(horizontal: 24),
                                    alignment: Alignment.center,
                                    child: new Row(
                                        crossAxisAlignment: CrossAxisAlignment.center,
                                        children: headerChildren
                                    )
                                ),
                                new Container(
                                    constraints: new BoxConstraints(
                                        minHeight: screenHeight - 64
                                    ),
                                    width: screenWidth < 750 ? (float?) null : 450,
                                    margin: EdgeInsets.symmetric(horizontal: 24),
                                    alignment: Alignment.center,
                                    child: new Column(
                                        children: children
                                    )
                                ),
                            }
                        )
                    )
                )
            );

            var stacked = new List<Widget> {all};

            all = new Stack(
                children: stacked
            );

            if (screenWidth < 750)
            {
                all = Transform.translate(
                    offset: new Offset(0, (1 - m_AnimationController.value) * screenHeight),
                    child: all
                );
            }

            return all;
        }
    }
}