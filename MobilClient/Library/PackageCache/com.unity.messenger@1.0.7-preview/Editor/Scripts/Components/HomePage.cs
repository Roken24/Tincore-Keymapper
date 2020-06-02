using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Models;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using static Unity.Messenger.Elements;
using Color = Unity.UIWidgets.ui.Color;
using Message = Unity.Messenger.Models.Message;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using WebSocketSharp;
using static Unity.Messenger.Utils;

namespace Unity.Messenger.Components
{
    public class HomePage : StatefulWidget
    {
        public HomePage(
            Dictionary<string, User> users,
            Dictionary<string, ReadState> readStates,
            Dictionary<string, List<Message>> messages,
            Dictionary<string, Channel> channels,
            Dictionary<string, List<ChannelMember>> members,
            Dictionary<string, bool> hasMoreMembers,
            Dictionary<string, Group> groups,
            Dictionary<string, bool> pullFlags,
            List<Channel> discoverChannels) : base(Key)
        {
            this.users = users;
            this.readStates = readStates;
            this.messages = messages;
            this.channels = channels;
            this.members = members;
            this.hasMoreMembers = hasMoreMembers;
            this.groups = groups;
            this.pullFlags = pullFlags;
            this.discoverChannels = discoverChannels;
        }

        internal readonly Dictionary<string, User> users;
        internal readonly Dictionary<string, ReadState> readStates;
        internal readonly Dictionary<string, List<Message>> messages;
        internal readonly Dictionary<string, Channel> channels;
        internal readonly Dictionary<string, List<ChannelMember>> members;
        internal readonly Dictionary<string, bool> hasMoreMembers;
        internal readonly Dictionary<string, Group> groups;
        internal readonly List<Channel> discoverChannels;
        internal readonly Dictionary<string, bool> pullFlags;

        public override State createState()
        {
            return new HomePageState();
        }

        private static readonly GlobalKey<HomePageState> Key =
            new GlobalObjectKey<HomePageState>("home-page");

        internal static HomePageState currentState => Key.currentState;

        internal static HomePageState of(BuildContext buildContext)
        {
            return (HomePageState) buildContext.ancestorStateOfType(new TypeMatcher<HomePageState>());
        }
    }

    internal class HomePageState : State<HomePage>
    {
        internal string SelectedChannelId;
        internal bool IsShowChannelInfo;
        internal bool IsShowDiscovery;
        internal string ShowingChannelBriefId;

        public void InsertMessage(string channelId, Message message)
        {
            if (widget.messages.ContainsKey(channelId))
            {
                setState(() => { widget.messages[channelId].Insert(0, message); });
            }
        }

        public void Ack(string channelId)
        {
            if (widget.channels[channelId].lastMessage != null)
            {
                setState(() =>
                {
                    if (!widget.readStates.ContainsKey(channelId))
                    {
                        widget.readStates[channelId] = new ReadState();
                    }

                    widget.readStates[channelId].lastMessageId = widget.channels[channelId].lastMessage.id;
                    widget.readStates[channelId].mentionCount = 0;
                });
                Post<Models.ReadState>(
                    $"/api/messages/{widget.channels[channelId].lastMessage.id}/ack",
                    "{}",
                    readState => { }
                );
            }
        }

        public void Select(string channelId, bool toDiscover = false)
        {
            using (WindowProvider.of(context).getScope())
            {
                MentionPopup.currentState?.StopMention();
                Sender.currentState?.Clear();
                if (Window.NewMessages.isNotEmpty())
                {
                    Window.NewMessages.ForEach(msg => { Window.Messages[msg.channelId].Insert(0, msg); });
                    Window.NewMessages.Clear();
                }

                IsShowDiscovery = channelId == string.Empty && MediaQuery.of(context).size.width >= 750 || toDiscover;
                SelectedChannelId = channelId;
                IsShowChannelInfo = false;
                ShowingChannelBriefId = null;

                setState();
            }
        }

        public void ShowChannelInfo()
        {
            MentionPopup.currentState.StopMention();
            IsShowChannelInfo = true;
            setState();
        }

        public void HideChannelInfo()
        {
            IsShowChannelInfo = false;
            setState();
        }

        public void ShowChannelBrief(string channelId)
        {
            MentionPopup.currentState.StopMention();
            ShowingChannelBriefId = channelId;
            setState();
        }

        public void HideChannelBrief()
        {
            ShowingChannelBriefId = null;
            setState();
        }

        public void AddChannel(Channel channel)
        {
            setState(() =>
            {
                widget.channels[channel.id] = channel;
                if (channel.lastMessage != null)
                {
                    widget.users[channel.lastMessage.author.id] = channel.lastMessage.author;
                }
            });
        }

        public void RemoveChannel(Channel channel)
        {
            if (widget.channels.ContainsKey(channel.id))
            {
                widget.channels.Remove(channel.id);
                setState();
            }
        }

        public void Clear()
        {
            SelectedChannelId = string.Empty;
            IsShowChannelInfo = false;
        }

        public override void initState()
        {
            base.initState();
            SelectedChannelId = string.Empty;
            IsShowChannelInfo = false;
            IsShowDiscovery = false;

            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                var overlayEntry = new OverlayEntry(
                    ctx => new MentionPopup(
                        widget.users,
                        widget.members,
                        widget.hasMoreMembers,
                        () => SelectedChannelId
                    )
                );
                Overlay.of(context).insert(overlayEntry);
            });
        }

        public Widget BuildNarrow(BuildContext context)
        {
            if (!Window.socketConnected)
            {
                return CreateNarrowSkeleton();
            }

            var stacked = new List<Widget>
            {
                new Container(
                    decoration: new BoxDecoration(
                        color: new Color(0xfff9f9f9),
                        border: new Border(
                            right: new BorderSide(
                                color: new Color(0xffd8d8d8)
                            )
                        )
                    ),
                    child: new Scroller(
                        child: new CustomScrollView(
                            slivers: BuildNavigations()
                        )
                    )
                )
            };

            if (IsShowDiscovery)
            {
                stacked.Add(
                    new Discovery(widget.channels, widget.groups)
                );
            }

            if (!SelectedChannelId.IsNullOrEmpty())
            {
                var channel = widget.channels[SelectedChannelId];
                var previousLastMsgId = string.Empty;
                if (widget.readStates.ContainsKey(SelectedChannelId)) {
                    previousLastMsgId = widget.readStates[SelectedChannelId].lastMessageId;
                    if (previousLastMsgId == channel.lastMessage?.id)
                    {
                        previousLastMsgId = null;
                    }
                }
                else {
                    previousLastMsgId = null;
                }

                stacked.Add(
                    new ChattingWindow(
                        channel,
                        widget.messages,
                        widget.users,
                        widget.members,
                        widget.hasMoreMembers,
                        widget.pullFlags,
                        previousLastMsgId
                    )
                );

                if (IsShowChannelInfo)
                {
                    stacked.Add(
                        new ChannelInfo(
                            widget.channels,
                            widget.groups,
                            widget.members,
                            widget.hasMoreMembers,
                            widget.users,
                            SelectedChannelId
                        )
                    );
                }

                if (ShowingChannelBriefId != null)
                {
                    stacked.Add(
                        new ChannelBrief(
                            ShowingChannelBriefId
                        )
                    );
                }
            }

            return new Stack(
                children: stacked
            );
        }

        public List<Widget> BuildNavigations()
        {
            var navigations = new List<Widget>
            {
                new SliverToBoxAdapter(
                    child: CreateChannelsListHeader()
                ),
            };

            if (Window.socketConnected)
            {
                if (widget.channels.Count > 0)
                {
                    var sortedChannels = widget.channels.Values.ToList();
                    sortedChannels.Sort((c1, c2) =>
                    {
                        if (!c1.stickTime.IsNullOrEmpty() && c2.stickTime.IsNullOrEmpty())
                        {
                            return -1;
                        }

                        if (c1.stickTime.IsNullOrEmpty() && !c2.stickTime.IsNullOrEmpty())
                        {
                            return 1;
                        }

                        var d2 = ExtractTimeFromSnowflakeId(c2.lastMessage?.id ?? c2.id);
                        var d1 = ExtractTimeFromSnowflakeId(c1.lastMessage?.id ?? c1.id);
                        return d2.CompareTo(d1);
                    });
                    navigations.Add(
                        new SliverList(
                            del: new SliverChildBuilderDelegate(
                                builder: (buildContext, index) => new LobbyChannelCard(
                                    sortedChannels[index],
                                    widget.users,
                                    widget.readStates,
                                    SelectedChannelId == sortedChannels[index].id
                                ),
                                childCount: widget.channels.Count
                            )
                        )
                    );
                }
                else
                {
                    navigations.Add(
                        new SliverToBoxAdapter(
                            child: new Container(
                                margin: EdgeInsets.only(left: 16, right: 16, top: 32),
                                child: new Text(
                                    "你还没有加入任何群聊，快加入感兴趣的群聊频道和大家一起讨论吧！",
                                    style: new TextStyle(
                                        fontSize: 14,
                                        color: new Color(0xff797979),
                                        fontFamily: "PingFang"
                                    )
                                )
                            )
                        )
                    );
                }

                if (widget.discoverChannels.Count > 0)
                {
                    navigations.Add(
                        new SliverToBoxAdapter(
                            child: new Container(
                                height: 82,
                                decoration: new BoxDecoration(
                                    border: new Border(
                                        bottom: new BorderSide(
                                            color: new Color(0xffd8d8d8),
                                            width: 1
                                        )
                                    )
                                ),
                                padding: EdgeInsets.only(left: 16, right: 16, bottom: 8),
                                alignment: Alignment.bottomCenter,
                                child: new Container(
                                    height: 24,
                                    alignment: Alignment.center,
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget>
                                        {
                                            new Text(
                                                "发现群聊",
                                                style: new TextStyle(
                                                    fontSize: 16,
                                                    color: new Color(0xff000000),
                                                    fontWeight: FontWeight.w500,
                                                    fontFamily: "PingFang"
                                                )
                                            ),
                                            new GestureDetector(
                                                onTap: () => Select(string.Empty, true),
                                                child: new Container(
                                                    color: new Color(0x00000000),
                                                    child: new Text(
                                                        "查看全部",
                                                        style: new TextStyle(
                                                            fontSize: 16,
                                                            color: new Color(0xff2196f3),
                                                            fontFamily: "PingFang"
                                                        )
                                                    )
                                                )
                                            )
                                        }
                                    )
                                )
                            )
                        )
                    );
                    navigations.Add(
                        new SliverList(
                            del: new SliverChildBuilderDelegate(
                                builder: (buildContext, index) => new DiscoverChannelCard(
                                    widget.discoverChannels[index],
                                    widget.channels
                                ),
                                childCount: widget.discoverChannels.Count
                            )
                        )
                    );
                }
            }
            else
            {
                navigations.Add(
                    new SliverToBoxAdapter(
                        child: new Container(
                            height: MediaQuery.of(context).size.height - 64,
                            alignment: Alignment.center,
                            child: new Loading(size: 64)
                        )
                    )
                );
            }

            return navigations;
        }

        public override Widget build(BuildContext context)
        {
            if (!Window.loggedIn)
            {
                return CreateNotLoggedIn();
            }

            var width = MediaQuery.of(context).size.width;
            if (width < 750)
            {
                return new SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: new SizedBox(
                        width: width.clamp(375, 750),
                        child: BuildNarrow(context)
                    )
                );
            }

            Widget content = null;
            if (!Window.socketConnected)
            {
                return CreateSkeleton(context);
            }

            if (IsShowDiscovery)
            {
                content = new Discovery(
                    widget.channels,
                    widget.groups
                );
            }
            else if (ShowingChannelBriefId != null)
            {
                content = new ChannelBrief(
                    ShowingChannelBriefId
                );
            }
            else if (!SelectedChannelId.IsNullOrEmpty())
            {
                var channel = widget.channels[SelectedChannelId];
                if (IsShowChannelInfo)
                {
                    content = new ChannelInfo(
                        widget.channels,
                        widget.groups,
                        widget.members,
                        widget.hasMoreMembers,
                        widget.users,
                        SelectedChannelId);
                }
                else
                {
                    var previousLastMsgId = string.Empty;
                    if (widget.readStates.ContainsKey(SelectedChannelId))
                    {
                        previousLastMsgId = widget.readStates[SelectedChannelId].lastMessageId;
                        if (previousLastMsgId == channel.lastMessage?.id)
                        {
                            previousLastMsgId = null;
                        }
                    }
                    else
                    {
                        previousLastMsgId = null;
                    }

                    

                    content = new ChattingWindow(
                        channel,
                        widget.messages,
                        widget.users,
                        widget.members,
                        widget.hasMoreMembers,
                        widget.pullFlags,
                        previousLastMsgId);
                }
            }
            else
            {
                content = new Discovery(widget.channels, widget.groups);
            }

            return new Row(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: new List<Widget>
                {
                    new Container(
                        width: 375,
                        decoration: new BoxDecoration(
                            color: new Color(0xfff9f9f9),
                            border: new Border(
                                right: new BorderSide(
                                    color: new Color(0xffd8d8d8)
                                )
                            )
                        ),
                        child: new Scroller(
                            child: new CustomScrollView(
                                slivers: BuildNavigations()
                            )
                        )
                    ),
                    new Expanded(
                        child: content
                    ),
                }
            );
        }
    }
}