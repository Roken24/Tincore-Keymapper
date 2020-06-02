using System;
using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using WebSocketSharp;
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
    public class LobbyChannelCard : StatelessWidget
    {
        private static readonly Decoration NormalDecoration = new BoxDecoration(
            color: new Color(0xffffffff),
            border: new Border(
                bottom: new BorderSide(
                    color: new Color(0x15000000),
                    width: 1
                )
            )
        );

        private static readonly Decoration StickDecoration = new BoxDecoration(
            color: new Color(0xfff6fbff),
            border: new Border(
                bottom: new BorderSide(
                    color: new Color(0x15000000),
                    width: 1
                )
            )
        );

        private static readonly Decoration SelectedDecoration = new BoxDecoration(
            color: new Color(0xff2196f3),
            border: new Border(
                bottom: new BorderSide(
                    color: new Color(0x15000000)
                )
            )
        );

        private static readonly EdgeInsets ThumbnailMargin = EdgeInsets.only(left: 16, right: 16);
        private static readonly EdgeInsets ChannelNameMargin = EdgeInsets.only(right: 16);

        private static readonly TextStyle SelectedChannelNameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xffffffff),
            fontFamily: "PingFang",
            fontWeight: FontWeight.w500
        );

        private static readonly TextStyle NormalChannelNameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff212121),
            fontFamily: "PingFang",
            fontWeight: FontWeight.w500
        );

        private static readonly TextStyle SelectedLastMessageStyle = new TextStyle(
            color: new Color(0xffffffff),
            fontSize: 14,
            fontFamily: "PingFang"
        );

        private static readonly TextStyle NormalLastMessageStyle = new TextStyle(
            color: new Color(0xff797979),
            fontSize: 14,
            fontFamily: "PingFang"
        );

        private static readonly EdgeInsets LastMessageMargin = EdgeInsets.only(right: 16);
        private static readonly EdgeInsets LastMessageTimestampMargin = EdgeInsets.only(right: 16, top: 12);

        private static readonly TextStyle SelectedLastMessageTimeStampStyle = new TextStyle(
            fontSize: 12,
            color: new Color(0xffffffff),
            fontFamily: "PingFang"
        );

        private static readonly TextStyle NormalLastMessageTimeStampStyle = new TextStyle(
            fontSize: 12,
            color: new Color(0xff797979),
            fontFamily: "PingFang"
        );

        public LobbyChannelCard(
            Channel channel,
            Dictionary<string, Models.User> users,
            Dictionary<string, Models.ReadState> readStates,
            bool selected = false) : base(key: new ObjectKey(channel.id))
        {
            m_Channel = channel;
            m_Selected = selected;
            m_Users = users;
            m_ReadStates = readStates;
        }

        private readonly Channel m_Channel;
        private readonly bool m_Selected;
        private readonly Dictionary<string, Models.User> m_Users;
        private readonly Dictionary<string, Models.ReadState> m_ReadStates;

        public override Widget build(BuildContext context)
        {
            var lastMessageContent = "";
            if (m_Channel.lastMessage != null)
            {
                if (m_Channel.lastMessage.content != null && m_Channel.lastMessage.content.isNotEmpty())
                {
                    lastMessageContent =
                        $"{m_Users[m_Channel.lastMessage.author.id].fullName}: {ParseMessageToString(m_Channel.lastMessage?.content, m_Users)}";
                }
                else if (m_Channel.lastMessage.attachments != null &&
                         m_Channel.lastMessage.attachments.Count > 0)
                {
                    var contentType = m_Channel.lastMessage.attachments.first().contentType;
                    if (contentType == "image/png" || contentType == "image/jpg" || contentType == "image/jpeg" ||
                        contentType == "image/gif")
                    {
                        lastMessageContent = $"{m_Users[m_Channel.lastMessage.author.id].fullName}: [图片]";
                    }
                    else
                    {
                        lastMessageContent = $"{m_Users[m_Channel.lastMessage.author.id].fullName}: [文件]";
                    }
                }
                else if (m_Channel.lastMessage.type == "voice")
                {
                    lastMessageContent = $"{m_Users[m_Channel.lastMessage.author.id].fullName}: [语音]";
                }
                else if (m_Channel.lastMessage.deletedTime != null && m_Channel.lastMessage.deletedTime.isNotEmpty())
                {
                    lastMessageContent = $"{m_Users[m_Channel.lastMessage.author.id].fullName}: 此条消息已被删除";
                }
            }
            else
            {
                lastMessageContent = "快来开始聊天吧";
            }

            var lastMessage = new List<TextSpan>
            {
                new TextSpan(
                    text: lastMessageContent,
                    style: m_Selected
                        ? SelectedLastMessageStyle
                        : NormalLastMessageStyle
                ),
            };
            if (!m_ReadStates.ContainsKey(m_Channel.id))
            {
                m_ReadStates[m_Channel.id] = new ReadState
                {
                    lastMessageId = m_Channel.lastMessage.id,
                };
            }
            if (m_ReadStates[m_Channel.id].mentionCount > 0)
            {
                lastMessage.Insert(
                    0,
                    new TextSpan(
                        text: "[有人@我] ",
                        style: new TextStyle(
                            color: new Color(0xfff44336),
                            fontSize: 14,
                            fontFamily: "PingFang"
                        )
                    )
                );
            }

            var rightColumn = new List<Widget>
            {
                new Expanded(
                    child: new Container(
                        margin: LastMessageTimestampMargin,
                        child: new Column(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget>
                            {
                                new Container(
                                    alignment: Alignment.center,
                                    child: new Text(
                                        FormatDateTime(
                                            ExtractTimeFromSnowflakeId(
                                                m_Channel.lastMessage?.id ?? m_Channel.id).ToLocalTime()
                                        ),
                                        style: m_Selected
                                            ? SelectedLastMessageTimeStampStyle
                                            : NormalLastMessageTimeStampStyle
                                    )
                                )
                            }
                        )
                    )
                ),
            };
            if (m_ReadStates[m_Channel.id].mentionCount > 0)
            {
                var count = m_ReadStates[m_Channel.id].mentionCount > 99 ? 99 : m_ReadStates[m_Channel.id].mentionCount;
                rightColumn.Add(
                    new Container(
                        constraints: new BoxConstraints(minWidth: 16),
                        padding: EdgeInsets.symmetric(horizontal: 4),
                        decoration: new BoxDecoration(
                            borderRadius: BorderRadius.all(8),
                            color: new Color(0xfff44336)
                        ),
                        margin: EdgeInsets.only(right: 16, bottom: 16),
                        alignment: Alignment.center,
                        child: new Text(
                            $"{count}",
                            style: new TextStyle(
                                fontSize: 12,
                                color: new Color(0xffffffff),
                                fontFamily: "PingFang"
                            )
                        )
                    )
                );
            }
            else if (m_Channel.lastMessage != null && !m_ReadStates[m_Channel.id].lastMessageId.isEmpty() &&
                     string.CompareOrdinal(m_Channel.lastMessage.id, m_ReadStates[m_Channel.id].lastMessageId) > 0)
            {
                rightColumn.Add(
                    new Container(
                        height: 10,
                        width: 10,
                        margin: EdgeInsets.only(bottom: 16, right: 16),
                        decoration: new BoxDecoration(
                            borderRadius: BorderRadius.all(5),
                            color: new Color(0xfff44336)
                        )
                    )
                );
            }

            return new GestureDetector(
                onTap: () => HomePage.of(context).Select(m_Channel.id),
                child: new Container(
                    height: 72,
                    alignment: Alignment.centerLeft,
                    decoration: m_Selected
                        ? SelectedDecoration
                        : (m_Channel.stickTime.IsNullOrEmpty() ? NormalDecoration : StickDecoration),
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Container(
                                margin: ThumbnailMargin,
                                child: CreateLobbyIcon(
                                    m_Channel.thumbnail,
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
                                                alignment: Alignment.centerLeft,
                                                child: new Text(
                                                    m_Channel.name,
                                                    style: m_Selected
                                                        ? SelectedChannelNameStyle
                                                        : NormalChannelNameStyle,
                                                    overflow: TextOverflow.ellipsis
                                                )
                                            ),
                                            new Container(
                                                margin: LastMessageMargin,
                                                child: new RichText(
                                                    text: new TextSpan(
                                                        children: lastMessage
                                                    ),
                                                    overflow: TextOverflow.ellipsis
                                                )
                                            )
                                        }
                                    )
                                )
                            ),
                            new Column(
                                crossAxisAlignment: CrossAxisAlignment.end,
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: rightColumn
                            ),
                        }
                    )
                )
            );
        }
    }
}