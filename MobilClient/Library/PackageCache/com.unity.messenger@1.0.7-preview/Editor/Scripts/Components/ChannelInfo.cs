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
    public class ChannelInfo : StatefulWidget
    {
        public ChannelInfo(
            Dictionary<string, Channel> channels,
            Dictionary<string, Group> groups,
            Dictionary<string, List<ChannelMember>> members,
            Dictionary<string, bool> hasMoreMembers,
            Dictionary<string, User> users,
            string selectedChannelId
        ) : base(key: new GlobalObjectKey<ChannelInfoState>(selectedChannelId))
        {
            this.channels = channels;
            this.groups = groups;
            this.members = members;
            this.hasMoreMembers = hasMoreMembers;
            this.users = users;
            this.selectedChannelId = selectedChannelId;
        }

        internal readonly Dictionary<string, Channel> channels;
        internal readonly Dictionary<string, Group> groups;
        internal readonly Dictionary<string, List<ChannelMember>> members;
        internal readonly Dictionary<string, bool> hasMoreMembers;
        internal readonly Dictionary<string, User> users;
        internal readonly string selectedChannelId;

        public override State createState()
        {
            return new ChannelInfoState();
        }
    }

    internal class ChannelInfoState : SingleTickerProviderStateMixin<ChannelInfo>
    {
        private AnimationController m_AnimationController;
        private SwitchController m_MuteController;
        private SwitchController m_PinController;
        private bool m_Quiting;
        private bool m_AmIOwner;
        private OverlayEntry _copiedTipEntry;
        private bool _showCopiedTip;

        public override void initState()
        {
            base.initState();
            var channel = widget.channels[widget.selectedChannelId];
            m_MuteController = new SwitchController(channel.isMute);
            m_PinController = new SwitchController(!string.IsNullOrEmpty(channel.stickTime));
            m_Quiting = false;
            m_AmIOwner = widget.members.ContainsKey(channel.id) &&
                         widget.members[channel.id].Any(member =>
                             member.user.id == Window.currentUserId && member.role == "owner");
            m_MuteController.addListener(MuteControllerListener);
            m_PinController.addListener(PinControllerListener);
            m_AnimationController = new AnimationController(
                vsync: this,
                duration: new TimeSpan(0, 0, 0, 0, milliseconds: 240)
            );
            m_AnimationController.addListener(() => setState());
            m_AnimationController.addStatusListener(AnimationStatusListener);
            _showCopiedTip = false;

            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                if (MediaQuery.of(context).size.width < 750)
                {
                    m_AnimationController.forward();
                }

                _copiedTipEntry = new OverlayEntry(buildContext =>
                {
                    var left = 0.0f;
                    if (MediaQuery.of(buildContext).size.width >= 750)
                    {
                        left = 375;
                    }

                    return new Positioned(
                        top: 64 + 40,
                        left: left,
                        right: 0,
                        child: new IgnorePointer(
                            child: new Container(
                                height: 40,
                                alignment: Alignment.center,
                                child: new Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: new List<Widget>
                                    {
                                        new AnimatedOpacity(
                                            opacity: _showCopiedTip ? 1 : 0,
                                            duration: new TimeSpan(0, 0, 0, 0, 240),
                                            curve: Curves.fastOutSlowIn,
                                            child: new Container(
                                                decoration: new BoxDecoration(
                                                    borderRadius: BorderRadius.circular(20),
                                                    color: new Color(0x99000000)
                                                ),
                                                padding: EdgeInsets.symmetric(horizontal: 24),
                                                alignment: Alignment.center,
                                                child: new Text(
                                                    "已复制群聊链接，请前往粘贴分享",
                                                    style: new TextStyle(
                                                        fontSize: 16,
                                                        color: new Color(0xffffffff),
                                                        fontFamily: "PingFang"
                                                    )
                                                )
                                            )
                                        ),
                                    }
                                )
                            )
                        )
                    );
                });
                Overlay.of(context).insert(_copiedTipEntry);
            });
        }

        public override void dispose()
        {
            m_AnimationController.dispose();
            _copiedTipEntry.remove();
            base.dispose();
        }

        private void AnimationStatusListener(AnimationStatus status)
        {
            if (status == AnimationStatus.dismissed)
            {
                HomePage.of(this.context).HideChannelInfo();
            }
        }

        public void MuteControllerListener()
        {
            var url = m_MuteController.value
                ? $"/api/connectapp/v1/channels/{HomePage.currentState.SelectedChannelId}/mute"
                : $"/api/connectapp/v1/channels/{HomePage.currentState.SelectedChannelId}/unMute";
            widget.channels[widget.selectedChannelId].isMute = m_MuteController.value;
            Utils.Post<Channel>(
                url,
                null).Then(channel => { });
        }

        public void PinControllerListener()
        {
            var url = m_PinController.value
                ? $"/api/connectapp/v1/channels/{HomePage.currentState.SelectedChannelId}/stick"
                : $"/api/connectapp/v1/channels/{HomePage.currentState.SelectedChannelId}/unStick";
            widget.channels[widget.selectedChannelId].stickTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Utils.Post<Channel>(
                url,
                null).Then(channel => { });
        }

        public override void didUpdateWidget(StatefulWidget oldWidget)
        {
            m_MuteController.removeListener(MuteControllerListener);
            m_PinController.removeListener(PinControllerListener);
            base.didUpdateWidget(oldWidget);
            var channel = widget.channels[widget.selectedChannelId];
            m_MuteController.value = channel.isMute;
            m_PinController.value = !string.IsNullOrEmpty(channel.stickTime);
            m_MuteController.addListener(MuteControllerListener);
            m_PinController.addListener(PinControllerListener);
        }

        private Widget BuildNarrowInfo()
        {
            var channel = widget.channels[widget.selectedChannelId];
            var topic = channel.topic ?? string.Empty;
            var canBeShared = true; 
            if (!channel.groupId.IsNullOrEmpty())
            {
                topic = widget.groups[channel.groupId].description;
                canBeShared = widget.groups[channel.groupId].privacy.ToLower() == "public";
            }

            var children = new List<Widget>
            {
                new Row(
                    children: new List<Widget>
                    {
                        CreateLobbyIcon(
                            channel.thumbnail,
                            size: 48,
                            radius: 4
                        ),
                        new Expanded(
                            child: new Container(
                                margin: EdgeInsets.only(left: 12),
                                child: new Text(
                                    channel.name,
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                    style: new TextStyle(
                                        color: new Color(0xff212121),
                                        fontSize: 16,
                                        height: 24 / 22.4f,
                                        fontWeight: FontWeight.w500,
                                        fontFamily: "PingFang"
                                    )
                                )
                            )
                        )
                    }
                ),
                new Container(
                    margin: EdgeInsets.only(top: 16),
                    child: new Text(
                        topic,
                        style: new TextStyle(
                            fontSize: 14,
                            color: new Color(0xff212121),
                            height: 22 * 16.0f / 14 / 22.4f,
                            fontFamily: "PingFang"
                        )
                    )
                ),
            };

            if (canBeShared)
            {
                children.Add(BuildShareLink());
            }

            return new SliverToBoxAdapter(
                child: new Container(
                    padding: EdgeInsets.all(24)
                        .copyWith(top: 16),
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: children
                    )
                )
            );
        }

        private Widget BuildShareLink()
        {
            var channel = widget.channels[widget.selectedChannelId];
            return new GestureDetector(
                onTap: () =>
                {
                    if (_showCopiedTip)
                    {
                        return;
                    }
                    Clipboard.setData(
                        new ClipboardData($"https://connect.unity.com/mconnect/channels/{channel.id}")
                    );
                    _showCopiedTip = true;
                    _copiedTipEntry.markNeedsBuild();
                    UIWidgets.ui.Window.instance.run(
                        new TimeSpan(0, 0, 0, 0, 2000),
                        () =>
                        {
                            if (mounted)
                            {
                                using (WindowProvider.of(context).getScope())
                                {

                                    _showCopiedTip = false;
                                    _copiedTipEntry.markNeedsBuild();

                                }
                            }
                        });
                },
                child: new Container(
                    height: 24,
                    color: new Color(0x00000000),
                    margin: EdgeInsets.only(top: 16),
                    child: new Row(
                        children: new List<Widget>
                        {
                            new Icon(
                                IconFont.IconFontShare,
                                size: 24,
                                color: new Color(0xff2196f3)
                            ),
                            new Container(
                                margin: EdgeInsets.only(left: 8),
                                child: new Text(
                                    "分享群聊",
                                    style: new TextStyle(
                                        fontSize: 14,
                                        color: new Color(0xff2196f3)
                                    )
                                )
                            ),
                        }
                    )
                )
            );
        }

        private Widget BuildWideInfo()
        {
            var channel = widget.channels[widget.selectedChannelId];
            var topic = channel.topic ?? string.Empty;
            var canBeShared = true;
            if (!channel.groupId.IsNullOrEmpty())
            {
                topic = widget.groups[channel.groupId].description;
                canBeShared = widget.groups[channel.groupId].privacy.ToLower() == "public";
            }

            var rightColumnChildren = new List<Widget>
            {
                new Text(
                    channel.name,
                    style: new TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.w400,
                        fontFamily: "PingFang"
                    )
                ),
                new Container(
                    margin: EdgeInsets.only(top: 12),
                    child: new RichText(
                        text: Utils.ParseMessage(
                            topic,
                            null,
                            new TextStyle(
                                fontSize: 14,
                                height: 20 * 16.0f / 14 / 22.4f,
                                fontFamily: "PingFang"
                            )
                        )
                    )
                ),
            };

            if (canBeShared)
            {
                rightColumnChildren.Add(
                    BuildShareLink()
                );
            }

            return new SliverToBoxAdapter(
                child: new Container(
                    margin: EdgeInsets.only(bottom: 24),
                    padding: EdgeInsets.only(top: 24, left: 24, right: 24),
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget>
                        {
                            new Container(
                                margin: EdgeInsets.only(right: 16),
                                child: CreateLobbyIcon(
                                    channel.thumbnail,
                                    size: 184,
                                    radius: 4
                                )
                            ),
                            new Expanded(
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: rightColumnChildren
                                )
                            )
                        }
                    )
                )
            );
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
            var channel = widget.channels[widget.selectedChannelId];

            Widget members;

            var screenSize = MediaQuery.of(context).size;
            var screenWidth = screenSize.width;
            var screenHeight = screenSize.height;
            var containerWidth = screenWidth - 48;
            if (screenWidth >= 750)
            {
                containerWidth -= 375;
            }

            var countPerRow = (containerWidth / 240).floor();
            var rowCount = ((widget.members.ContainsKey(channel.id) ? widget.members[channel.id].Count : 0) /
                            (float) countPerRow).ceil();
            if (!widget.hasMoreMembers.ContainsKey(widget.selectedChannelId))
            {
                widget.hasMoreMembers[widget.selectedChannelId] = true;
            }
            if (widget.hasMoreMembers[widget.selectedChannelId])
            {
                rowCount += 1;
            }

            members = new SliverList(
                del: new SliverChildBuilderDelegate(
                    builder: (buildContext, index) =>
                    {
                        if (widget.hasMoreMembers[widget.selectedChannelId] && index == rowCount - 1)
                        {
                            return new Container(
                                height: 78,
                                alignment: Alignment.center,
                                child: new LoadTrigger(
                                    onLoad: () =>
                                    {
                                        var offset = widget.members.ContainsKey(channel.id)
                                            ? widget.members[channel.id].Count
                                            : 0;
                                        Utils.Get<GetMembersResponse>(
                                            $"/api/connectapp/channels/{channel.id}/members?offset={offset}"
                                        ).Then(response =>
                                        {
                                            response.list.ForEach(member =>
                                            {
                                                if (widget.members[channel.id].All(m => m.user.id != member.user.id))
                                                {
                                                    widget.members[channel.id].Add(member);
                                                }

                                                widget.users.putIfAbsent(member.user.id, () => member.user);
                                            });

                                            m_AmIOwner = widget.members[channel.id].Any(member =>
                                                member.user.id == Window.currentUserId && member.role == "owner");
                                            widget.hasMoreMembers[widget.selectedChannelId] = response.total > widget.members[channel.id].Count;
                                            if (mounted)
                                            {
                                                using (WindowProvider.of(context).getScope())
                                                {
                                                    setState(() => { });
                                                }
                                            }
                                        });
                                    }
                                )
                            );
                        }

                        var children = new List<Widget> { };
                        for (var i = 0; i < countPerRow; ++i)
                        {
                            if (index * countPerRow + i < widget.members[channel.id].Count)
                            {
                                var member = widget.members[channel.id][index * countPerRow + i];
                                children.Add(
                                    new Expanded(
                                        child: new Container(
                                            height: 78,
                                            width: 240,
                                            alignment: Alignment.center,
                                            child: new Container(
                                                height: 50,
                                                padding: EdgeInsets.only(
                                                    top: 6,
                                                    bottom: 4
                                                ),
                                                child: new Row(
                                                    children: new List<Widget>
                                                    {
                                                        new Container(
                                                            margin: EdgeInsets.only(right: 8),
                                                            child: new Avatar(member.user)
                                                        ),
                                                        new Expanded(
                                                            child: new Column(
                                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                                children: new List<Widget>
                                                                {
                                                                    new Text(
                                                                        member.user.fullName,
                                                                        style: new TextStyle(
                                                                            fontSize: 14,
                                                                            fontWeight: FontWeight.w500,
                                                                            fontFamily: "PingFang"
                                                                        ),
                                                                        overflow: TextOverflow.ellipsis
                                                                    ),
                                                                    new Text(
                                                                        member.user.title ?? string.Empty,
                                                                        style: new TextStyle(
                                                                            color: new Color(0xff5a5a5b),
                                                                            fontSize: 14,
                                                                            fontFamily: "PingFang"
                                                                        ),
                                                                        overflow: TextOverflow.ellipsis
                                                                    )
                                                                }
                                                            )
                                                        )
                                                    }
                                                )
                                            )
                                        )
                                    )
                                );
                            }
                            else
                            {
                                children.Add(
                                    new Expanded(
                                        child: new Container(
                                            height: 78,
                                            width: 240
                                        )
                                    )
                                );
                            }
                        }

                        return new Container(
                            margin: EdgeInsets.symmetric(horizontal: 24),
                            decoration: new BoxDecoration(
                                border: new Border(
                                    top: new BorderSide(
                                        color: new Color(0xfff0f0f0)
                                    )
                                )
                            ),
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: children
                            )
                        );
                    },
                    childCount: rowCount
                )
            );

            var headerChildren = new List<Widget>
            {
                new Expanded(
                    child: new Text(
                        $"{channel.name}",
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

            Widget all = new Container(
                color: new Color(0xffffffff),
                child: new Scroller(
                    child: new CustomScrollView(
                        slivers: new List<Widget>
                        {
                            new SliverToBoxAdapter(
                                child: new Container(
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
                                )
                            ),
                            MediaQuery.of(context).size.width < 750 ? BuildNarrowInfo() : BuildWideInfo(),
                            new SliverToBoxAdapter(
                                child: new Container(
                                    decoration: new BoxDecoration(
                                        border: new Border(
                                            top: new BorderSide(
                                                color: new Color(0xffd8d8d8)
                                            ),
                                            bottom: new BorderSide(
                                                color: new Color(0xffd8d8d8)
                                            )
                                        )
                                    ),
                                    padding: EdgeInsets.symmetric(vertical: 18),
                                    margin: EdgeInsets.symmetric(horizontal: 24),
                                    height: 78,
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget>
                                        {
                                            new Column(
                                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget>
                                                {
                                                    new Text(
                                                        "消息免打扰",
                                                        style: new TextStyle(
                                                            fontSize: 14,
                                                            color: new Color(0xff212121),
                                                            fontFamily: "PingFang"
                                                        )
                                                    ),
                                                    new Text(
                                                        "打开后，将不会收到消息提醒",
                                                        style: new TextStyle(
                                                            fontSize: 14,
                                                            color: new Color(0xff797979),
                                                            fontFamily: "PingFang"
                                                        )
                                                    ),
                                                }
                                            ),
                                            new Switch(
                                                m_MuteController
                                            )
                                        }
                                    )
                                )
                            ),
                            new SliverToBoxAdapter(
                                child: new Container(
                                    decoration: new BoxDecoration(
                                        border: new Border(
                                            bottom: new BorderSide(
                                                color: new Color(0xffd8d8d8)
                                            )
                                        )
                                    ),
                                    padding: EdgeInsets.symmetric(vertical: 18),
                                    margin: EdgeInsets.symmetric(horizontal: 24),
                                    height: 78,
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget>
                                        {
                                            new Column(
                                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget>
                                                {
                                                    new Text(
                                                        "设为置顶",
                                                        style: new TextStyle(
                                                            fontSize: 14,
                                                            color: new Color(0xff212121),
                                                            fontFamily: "PingFang"
                                                        )
                                                    ),
                                                    new Text(
                                                        "打开后，当前群聊将会被置顶在群聊列表",
                                                        style: new TextStyle(
                                                            fontSize: 14,
                                                            color: new Color(0xff797979),
                                                            fontFamily: "PingFang"
                                                        )
                                                    )
                                                }
                                            ),
                                            new Switch(
                                                m_PinController
                                            )
                                        }
                                    )
                                )
                            ),
                            new SliverToBoxAdapter(
                                child: new Container(
                                    margin: EdgeInsets.only(top: 24, left: 24, right: 24),
                                    padding: EdgeInsets.only(bottom: 8),
                                    child: new Text(
                                        $"群聊成员（{channel.memberCount}）",
                                        style: new TextStyle(
                                            fontSize: 18,
                                            fontWeight: FontWeight.w500,
                                            fontFamily: "PingFang"
                                        )
                                    )
                                )
                            ),
                            members,
                            new SliverToBoxAdapter(
                                child: new Container(
                                    height: 56
                                )
                            )
                        }
                    )
                )
            );

            var stacked = new List<Widget> {all};
            if (!m_AmIOwner)
            {
                var quitButtonChildren = new List<Widget>();
                if (m_Quiting)
                {
                    quitButtonChildren.Add(
                        new Text(
                            "退出群聊",
                            style: new TextStyle(
                                fontSize: 18,
                                color: new Color(0x00000000),
                                fontFamily: "PingFang"
                            )
                        )
                    );
                    quitButtonChildren.Add(
                        new Loading(
                            size: 24
                        )
                    );
                }
                else
                {
                    quitButtonChildren.Add(new Text(
                            "退出群聊",
                            style: new TextStyle(
                                fontSize: 18,
                                color: new Color(0xfff44336),
                                fontFamily: "PingFang"
                            )
                        )
                    );
                }

                stacked.Add(
                    new Positioned(
                        bottom: 0,
                        left: 0,
                        right: 0,
                        child: new GestureDetector(
                            onTap: () =>
                            {
                                if (m_Quiting)
                                {
                                    return;
                                }
                                setState(() => m_Quiting = true);
                                var requestUrl = string.IsNullOrEmpty(channel.groupId)
                                    ? $"/api/connectapp/v1/channels/{channel.id}/leave"
                                    : $"/api/connectapp/v1/groups/{channel.groupId}/leave";
                                var state = HomePage.of(context);
                                Utils.Post<Models.Channel>(
                                    requestUrl,
                                    "{}"
                                ).Then(c =>
                                {
                                    state.Select(string.Empty);
                                    state.RemoveChannel(channel);
                                });
                            },
                            child: new Container(
                                height: 56,
                                decoration: new BoxDecoration(
                                    color: new Color(0xffffffff),
                                    border: new Border(
                                        top: new BorderSide(
                                            color: new Color(0xffd8d8d8)
                                        )
                                    )
                                ),
                                alignment: Alignment.center,
                                child: new Stack(
                                    children: quitButtonChildren
                                )
                            )
                        )
                    )
                );
            }

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