using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using WebSocketSharp;
using static Unity.Messenger.Utils;
using Color = Unity.UIWidgets.ui.Color;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.Messenger.Widgets
{
    public class ChattingWindow : StatefulWidget
    {
        internal readonly Channel channel;
        internal readonly Dictionary<string, Models.User> users;
        internal readonly Dictionary<string, List<Models.Message>> messages;
        internal readonly Dictionary<string, List<ChannelMember>> members;
        internal readonly Dictionary<string, bool> hasMoreMembers;
        internal readonly Dictionary<string, bool> pullFlags;
        private static GlobalObjectKey<ChattingWindowState> _key;
        internal readonly string previousLastMsgId;

        public ChattingWindow(
            Channel channel,
            Dictionary<string, List<Models.Message>> messages,
            Dictionary<string, User> users,
            Dictionary<string, List<ChannelMember>> members,
            Dictionary<string, bool> hasMoreMembers,
            Dictionary<string, bool> pullFlags,
            string previousLastMsgId) :
            base(key: _key = new GlobalObjectKey<ChattingWindowState>(channel.id))
        {
            this.channel = channel;
            this.messages = messages;
            this.users = users;
            this.members = members;
            this.hasMoreMembers = hasMoreMembers;
            this.previousLastMsgId = previousLastMsgId;
            this.pullFlags = pullFlags;
        }

        public override State createState()
        {
            return new ChattingWindowState(previousLastMsgId);
        }

        internal static ChattingWindowState currentState => _key.currentState;
    }

    internal class ChattingWindowState : SingleTickerProviderStateMixin<ChattingWindow>
    {
        public ChattingWindowState(string previousLastMsgId)
        {
            m_PreviousLastMsgId = previousLastMsgId;
        }

        private int m_MsgsUnreads;
        private bool m_HasMoreUnreads;

        private string m_PreviousLastMsgId;
        private bool m_HasMoreOld;
        private bool m_Initialized;
        private FocusNode m_EmptyFocusNode;
        private FocusNode m_SenderFocusNode;
        internal ScrollController m_ScrollController;
        private AnimationController m_AnimationController;

        private void AnimationStatusListener(AnimationStatus status)
        {
            if (status == AnimationStatus.dismissed)
            {
                HomePage.of(context).Select(string.Empty);
            }
        }

        public float CalculateMessageHeight(
            Models.Message message,
            bool showTime,
            float width)
        {
            var height = 0.0f;
            if (message.attachments.isNotEmpty())
            {
                var attachment = message.attachments.first();
                var contentType = attachment.contentType;
                if (contentType == "image/png" ||
                    contentType == "image/jpg" ||
                    contentType == "image/jpeg" ||
                    contentType == "image/gif")
                {
                    height = 42 + 282 * attachment.height / attachment.width + (showTime ? 36 : 0);
                }
                else
                {
                    var textPainter = new TextPainter(
                        textDirection: TextDirection.ltr,
                        text: new TextSpan(
                            text: attachment.filename,
                            style: new TextStyle(
                                fontSize: 16,
                                fontFamily: "PingFang"
                            )
                        )
                    );
                    textPainter.layout(maxWidth: 172);
                    height = 86 + textPainter.getLineCount() * 22;
                    height = height.clamp(114, height);
                    if (showTime)
                    {
                        height += 36;
                    }
                }
            }
            else
            {
                height = 66 + (showTime ? 36 : 0); // Name + Internal + Bottom padding + time
                var textPainter = new TextPainter(
                    textDirection: TextDirection.ltr,
                    text: ParseMessage(
                        message.content ?? "此条消息已被删除",
                        widget.users
                    )
                );
                textPainter.layout(maxWidth: width);
                height += textPainter.getLineCount() * 22;
                height += MessageEmbed.CalculateTextHeight(message, width);
            }

            return height;
        }

        public void PullIfNeeded()
        {
            if (widget.pullFlags.ContainsKey(widget.channel.id) && widget.pullFlags[widget.channel.id])
            {
                widget.pullFlags[widget.channel.id] = false;
                SchedulerBinding.instance.addPostFrameCallback(value =>
                {
                    Utils.Get<GetMessagesResponse>(
                        $"/api/connectapp/v1/channels/{widget.channel.id}/messages"
                    ).Then(getMessagesResponse =>
                    {
                        if (mounted)
                        {
                            m_HasMoreOld = getMessagesResponse.hasMore;
                            m_Initialized = true;
                        }

                        var needMerge = true;
                        if (!widget.messages.ContainsKey(widget.channel.id) ||
                            widget.messages[widget.channel.id].isEmpty())
                        {
                            needMerge = false;
                        }
                        else if (string.CompareOrdinal(getMessagesResponse.items.last().id,
                                     widget.messages[widget.channel.id].first().id) > 0)
                        {
                            needMerge = false;
                        }

                        if (needMerge)
                        {
                            for (var i = getMessagesResponse.items.Count - 1; i >= 0; i--)
                            {
                                if (string.CompareOrdinal(getMessagesResponse.items[i].id,
                                        widget.messages[widget.channel.id].first().id) <= 0)
                                {
                                    continue;
                                }

                                widget.messages[widget.channel.id].Insert(0, getMessagesResponse.items[i]);
                            }
                        }
                        else
                        {
                            widget.messages[widget.channel.id]?.Clear();
                            widget.messages[widget.channel.id] = getMessagesResponse.items;
                        }
                    }).Catch(exception => { }).Then(() =>
                    {
                        using (WindowProvider.of(context).getScope())
                        {
                            setState();
                            FocusScope.of(context).requestFocus(m_SenderFocusNode);
                        }
                    });
                });
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

        public override void initState()
        {
            base.initState();
            m_HasMoreOld = true;
            m_EmptyFocusNode = new FocusNode();
            m_SenderFocusNode = new FocusNode();
            m_ScrollController = new ScrollController();
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

            m_ScrollController.addListener(() =>
            {
                if (m_ScrollController.offset < 50f && Window.NewMessages.isNotEmpty())
                {
                    var message = Window.NewMessages[0];
                    Window.NewMessages.RemoveAt(0);
                    var showTime = true;
                    if (!widget.messages[message.channelId].isEmpty())
                    {
                        var preMsg = widget.messages[message.channelId].first();
                        var preTime = ExtractTimeFromSnowflakeId(preMsg.id ?? preMsg.nonce);
                        var curTime = ExtractTimeFromSnowflakeId(message.id ?? message.nonce);
                        showTime = curTime - preTime > TimeSpan.FromMinutes(5);
                    }

                    var height = CalculateMessageHeight(
                        message,
                        showTime,
                        MediaQuery.of(context).size.width * 0.7f - 286.5f);
                    Window.Messages[message.channelId].Insert(0, message);
                    m_ScrollController.jumpTo(m_ScrollController.offset + height);
                    HomePage.currentState.setState();
                }
            });
            if (!widget.messages.ContainsKey(widget.channel.id) || widget.messages[widget.channel.id].isEmpty())
            {
                m_Initialized = false;
                widget.messages[widget.channel.id] = new List<Models.Message>();
                if (widget.pullFlags.ContainsKey(widget.channel.id) && widget.pullFlags[widget.channel.id])
                {
                    widget.pullFlags[widget.channel.id] = false;
                }

                SchedulerBinding.instance.addPostFrameCallback(value =>
                {
                    Promise.All(new List<IPromise>
                    {
                        Get<GetMembersResponse>(
                            $"/api/connectapp/v1/channels/{widget.channel.id}/members"
                        ).Then(response =>
                        {
                            if (!widget.members.ContainsKey(widget.channel.id))
                            {
                                widget.members[widget.channel.id] = new List<ChannelMember>();
                            }

                            response.list.ForEach(member =>
                            {
                                if (!widget.users.ContainsKey(member.user.id))
                                {
                                    widget.users.Add(member.user.id, member.user);
                                }

                                if (widget.members[widget.channel.id].All(m => m.user.id != member.user.id))
                                {
                                    widget.members[widget.channel.id].Add(member);
                                }
                            });
                            widget.hasMoreMembers[widget.channel.id] =
                                response.total > widget.members[widget.channel.id].Count;
                        }).Catch(exception => { }),
                        Utils.Get<GetMessagesResponse>(
                            $"/api/connectapp/v1/channels/{widget.channel.id}/messages"
                        ).Then(getMessagesResponse =>
                        {
                            if (mounted)
                            {
                                m_HasMoreOld = getMessagesResponse.hasMore;
                                m_Initialized = true;
                            }

                            widget.messages[widget.channel.id] = getMessagesResponse.items;
                            m_HasMoreUnreads = true;
                            if (m_PreviousLastMsgId == null)
                            {
                                m_HasMoreUnreads = false;
                            }
                            else
                            {
                                foreach (var m in widget.messages[widget.channel.id])
                                {
                                    if (string.Compare(m.id, m_PreviousLastMsgId) > 0)
                                    {
                                        ++m_MsgsUnreads;
                                    }
                                    else
                                    {
                                        m_HasMoreUnreads = false;
                                        break;
                                    }
                                }
                            }
                        }).Catch(exception => { }),
                    }).Then(() =>
                    {
                        if (mounted)
                        {
                            using (WindowProvider.of(context).getScope())
                            {
                                setState();
                                FocusScope.of(context).requestFocus(m_SenderFocusNode);
                            }
                        }
                    });
                });
            }
            else
            {
                PullIfNeeded();
                m_Initialized = true;
                m_HasMoreUnreads = true;
                if (m_PreviousLastMsgId == null)
                {
                    m_HasMoreUnreads = false;
                }
                else
                {
                    foreach (var m in widget.messages[widget.channel.id])
                    {
                        if (string.Compare(m.id, m_PreviousLastMsgId) > 0)
                        {
                            ++m_MsgsUnreads;
                        }
                        else
                        {
                            m_HasMoreUnreads = false;
                            break;
                        }
                    }
                }

                SchedulerBinding.instance.addPostFrameCallback(value =>
                {
                    using (WindowProvider.of(context).getScope())
                    {
                        FocusScope.of(context).requestFocus(m_SenderFocusNode);
                    }
                });
            }

            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                var rootState = HomePage.of(context);
                rootState.Ack(widget.channel.id);
            });
        }

        public override void dispose()
        {
            m_AnimationController.dispose();
            m_ScrollController.dispose();
            m_EmptyFocusNode.dispose();
            m_SenderFocusNode.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            var screenWidth = MediaQuery.of(context).size.width;
            var windowChildren = new List<Widget>
            {
                new Scroller(
                    child: ListView.builder(
                        controller: m_ScrollController,
                        reverse: true,
                        itemCount: m_HasMoreOld
                            ? widget.messages[widget.channel.id].Count + 1
                            : widget.messages[widget.channel.id].Count,
                        itemBuilder: (ctx, index) =>
                        {
                            if (!m_Initialized)
                            {
                                return new Container();
                            }

                            if (index == widget.messages[widget.channel.id].Count)
                            {
                                return new LoadTrigger(
                                    () =>
                                    {
                                        var lastMessageId =
                                            widget.messages[widget.channel.id].last().id;
                                        Get(
                                            $"/api/connectapp/v1/channels/{widget.channel.id}/messages?before={lastMessageId}",
                                            (GetMessagesResponse getMessagesResponse) =>
                                            {
                                                if (mounted)
                                                {
                                                    using (WindowProvider.of(context)
                                                        .getScope())
                                                    {
                                                        setState(() =>
                                                        {
                                                            widget.messages[widget.channel.id]
                                                                .AddRange(
                                                                    getMessagesResponse.items
                                                                        .Where(
                                                                            item =>
                                                                                item.id !=
                                                                                lastMessageId
                                                                        )
                                                                );
                                                            m_MsgsUnreads = 0;
                                                            m_HasMoreUnreads = true;
                                                            if (m_PreviousLastMsgId == null)
                                                            {
                                                                m_HasMoreUnreads = false;
                                                            }
                                                            else
                                                            {
                                                                foreach (var m in widget.messages[widget.channel.id])
                                                                {
                                                                    if (string.Compare(m.id, m_PreviousLastMsgId) > 0)
                                                                    {
                                                                        ++m_MsgsUnreads;
                                                                    }
                                                                    else
                                                                    {
                                                                        m_HasMoreUnreads = false;
                                                                        break;
                                                                    }
                                                                }
                                                            }

                                                            m_HasMoreOld =
                                                                getMessagesResponse.hasMore;
                                                        });
                                                    }
                                                }
                                            });
                                    }
                                );
                            }

                            var currentMessage = widget.messages[widget.channel.id][index];
                            var msgTime =
                                ExtractTimeFromSnowflakeId(currentMessage.id.IsNullOrEmpty()
                                    ? currentMessage.nonce
                                    : currentMessage.id);
                            bool showTime;
                            var isNew = false;
                            if (index == widget.messages[widget.channel.id].Count - 1)
                            {
                                showTime = true;
                            }
                            else
                            {
                                var nextMessage = widget.messages[widget.channel.id][index + 1];
                                showTime = msgTime - ExtractTimeFromSnowflakeId(
                                               nextMessage.id.IsNullOrEmpty()
                                                   ? nextMessage.nonce
                                                   : nextMessage.id) >
                                           TimeSpan.FromMinutes(5);
                                if (nextMessage.id != null && nextMessage.id == m_PreviousLastMsgId)
                                {
                                    isNew = true;
                                }
                            }

                            Action onBuild = null;
                            if (currentMessage.id == m_PreviousLastMsgId)
                            {
                                onBuild = () =>
                                {
                                    SchedulerBinding.instance.addPostFrameCallback(value =>
                                    {
                                        setState(() =>
                                        {
                                            m_MsgsUnreads = 0;
                                            m_HasMoreUnreads = false;
                                        });
                                    });
                                };
                            }

                            return new Message(
                                widget.messages[widget.channel.id][index],
                                widget.users,
                                showTime,
                                m_PreviousLastMsgId != widget.channel.lastMessage.id && isNew,
                                msgTime,
                                onBuild
                            );
                        }
                    )
                ),
            };
            if (Window.reconnecting)
            {
                windowChildren.Add(
                    new Positioned(
                        left: 0,
                        top: 0,
                        right: 0,
                        child: new Container(
                            color: new Color(0xfffde1df),
                            height: 48,
                            alignment: Alignment.center,
                            child: new Text(
                                "网络未连接，正在连接中",
                                style: new TextStyle(
                                    fontSize: 16,
                                    color: new Color(0xfff44336),
                                    fontFamily: "PingFang"
                                )
                            )
                        )
                    )
                );
            }

            var newMsgsCount = Window.NewMessages.Count(msg => msg.author.id != Window.currentUserId);
            if (newMsgsCount != 0)
            {
                windowChildren.Add(
                    new Positioned(
                        bottom: 24,
                        child: new GestureDetector(
                            onTap: () =>
                            {
                                Window.NewMessages.ForEach(msg => { Window.Messages[msg.channelId].Insert(0, msg); });
                                Window.NewMessages.Clear();
                                setState();
                                m_ScrollController.animateTo(
                                    0,
                                    new TimeSpan(0, 0, 0, 0, 480),
                                    Curves.easeInOut
                                );
                            },
                            child: new Container(
                                height: 40,
                                padding: EdgeInsets.symmetric(horizontal: 16),
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(20),
                                    boxShadow: new List<BoxShadow>
                                    {
                                        new BoxShadow(
                                            offset: new Offset(0, 1),
                                            blurRadius: 6,
                                            color: new Color(0x19000000)
                                        ),
                                    },
                                    color: new Color(0xffffffff)
                                ),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget>
                                    {
                                        new Text(
                                            $"{newMsgsCount}条新消息未读",
                                            style: new TextStyle(
                                                color: new Color(0xff2196f3),
                                                fontSize: 14,
                                                fontFamily: "PingFang"
                                            )
                                        ),
                                    }
                                )
                            )
                        )
                    )
                );
            }

            if (m_MsgsUnreads > 0)
            {
                var text = $"{m_MsgsUnreads}";
                if (m_HasMoreUnreads)
                {
                    text += "+";
                }

                text += "条新消息";
                windowChildren.Add(
                    new Positioned(
                        top: 24,
                        child: new GestureDetector(
                            onTap: () =>
                            {
                                var totalHeight = 0.0f;
                                for (var index = 0; index < widget.messages[widget.channel.id].Count; ++index)
                                {
                                    var message = widget.messages[widget.channel.id][index];
                                    if (string.Compare(message.id, m_PreviousLastMsgId) > 0)
                                    {
                                        var showTime = true;
                                        var msgTime =
                                            ExtractTimeFromSnowflakeId(message.id.IsNullOrEmpty()
                                                ? message.nonce
                                                : message.id);
                                        if (index != widget.messages[widget.channel.id].Count - 1)
                                        {
                                            var nextMessage = widget.messages[widget.channel.id][index + 1];
                                            showTime = msgTime - ExtractTimeFromSnowflakeId(
                                                           nextMessage.id.IsNullOrEmpty()
                                                               ? nextMessage.nonce
                                                               : nextMessage.id) >
                                                       TimeSpan.FromMinutes(5);
                                        }

                                        var layoutWidth = screenWidth * 0.7f;
                                        if (screenWidth >= 750)
                                        {
                                            layoutWidth -= 286.5f;
                                        }
                                        else
                                        {
                                            layoutWidth -= 24f;
                                        }

                                        totalHeight += CalculateMessageHeight(
                                            message,
                                            showTime,
                                            layoutWidth
                                        );
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (m_HasMoreUnreads)
                                {
                                    totalHeight += 40;
                                }
                                else
                                {
                                    totalHeight += 36;
                                }

                                m_ScrollController.animateTo(
                                    totalHeight - MediaQuery.of(context).size.height + 184,
                                    new TimeSpan(0, 0, 0, 0, 480),
                                    Curves.easeInOut
                                );
                                if (!m_HasMoreUnreads)
                                {
                                    setState(() =>
                                    {
                                        m_MsgsUnreads = 0;
                                        m_HasMoreUnreads = false;
                                    });
                                }
                            },
                            child: new Container(
                                height: 40,
                                padding: EdgeInsets.symmetric(horizontal: 16),
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(20),
                                    boxShadow: new List<BoxShadow>
                                    {
                                        new BoxShadow(
                                            offset: new Offset(0, 1),
                                            blurRadius: 6,
                                            color: new Color(0x19000000)
                                        ),
                                    },
                                    color: new Color(0xffffffff)
                                ),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget>
                                    {
                                        new Text(
                                            text,
                                            style: new TextStyle(
                                                color: new Color(0xff2196f3),
                                                fontSize: 14,
                                                fontFamily: "PingFang"
                                            )
                                        ),
                                        new Container(
                                            margin: EdgeInsets.only(left: 4),
                                            child: Transform.rotate(
                                                child: new Icon(
                                                    IconFont.IconFontArrowUp,
                                                    size: 24,
                                                    color: new Color(0xff2196f3)
                                                )
                                            )
                                        )
                                    }
                                )
                            )
                        )
                    )
                );
            }

            var rootState = HomePage.of(context);

            var children = new List<Widget>
            {
                new Container(
                    color: new Color(0xffffffff),
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: new List<Widget>
                        {
                            new ChattingWindowHeader(
                                widget.channel, () =>
                                {
                                    if (screenWidth < 750)
                                    {
                                        m_AnimationController.reverse();
                                    }
                                    else
                                    {
                                        HomePage.of(context).Select(string.Empty);
                                    }
                                }),
                            new Expanded(
                                child: new Stack(
                                    alignment: Alignment.center,
                                    children: windowChildren
                                )
                            ),
                            new Sender(
                                m_SenderFocusNode,
                                widget.users),
                        }
                    )
                ),
            };
            if (!m_Initialized)
            {
                children.Add(
                    new Container(
                        alignment: Alignment.center,
                        child: new Loading(size: 56)
                    )
                );
            }

            Widget all = new GestureDetector(
                onTap: () => { FocusScope.of(context).requestFocus(m_EmptyFocusNode); },
                child: new Stack(
                    children: children
                )
            );
            if (screenWidth < 750)
            {
                all = Transform.translate(
                    offset: new Offset((1 - m_AnimationController.value) * screenWidth, 0),
                    child: all
                );
            }

            return all;
        }
    }
}