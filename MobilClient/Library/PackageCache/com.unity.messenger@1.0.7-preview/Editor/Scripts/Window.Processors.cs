using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Notification;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WebSocketSharp;
using static Unity.Messenger.Utils;
using Message = Unity.Messenger.Models.Message;

namespace Unity.Messenger
{
    public partial class Window
    {
        private static void ProcessReadyFrame(ReadyFrame readyFrame)
        {
            var lastMessagesMapping = new Dictionary<string, Message>();
            readyFrame.data.lastMessages.ForEach(message =>
                lastMessagesMapping.Add(message.id, message));
            var lobbyChannels = readyFrame.data.lobbyChannels
                .Where(c => c.liveChannelId == null || c.liveChannelId.isEmpty())
                .Where(c => c.workspaceId == DefaultWorkspaceId)
                .ToDictionary(c => c.id);
            readyFrame.data.users.ForEach(user => Users[user.id] = user);
            readyFrame.data.readStates?.ForEach(state => ReadStates[state.channelId] = state);

            foreach (var c in lobbyChannels.Values)
            {
                if (c.lastMessageId != null)
                {
                    c.lastMessage = lastMessagesMapping[c.lastMessageId];
                }

                if (OverrideNames.ContainsKey(c.id))
                {
                    c.name = OverrideNames[c.id];
                }
            }
            foreach (var pair in readyFrame.data.groupMap)
            {
                Groups.putIfAbsent(pair.Key, () => pair.Value);                
            }

            foreach (var keyValuePair in lobbyChannels)
            {
                Channels[keyValuePair.Key] = keyValuePair.Value;
                PullFlags[keyValuePair.Key] = true;
            }

            _pingTimer = new Timer(15000);
            _timeoutTimer = new Timer(15000);
            _pingTimer.Elapsed += (sender, args) =>
            {
                var ts = (long) DateTime.UtcNow.Subtract(Epoch).TotalMilliseconds;
                var frameSz = "{\"op\":9,\"d\": {\"ts\":" + ts + "}}";
                _lastPingTimestamp = ts;
                _timeoutTimer.Enabled = true;
                _client.Send(frameSz);
            };
            _timeoutTimer.Elapsed += (sender, args) => { _client.Close(); };
            _timeoutTimer.AutoReset = false;
            _pingTimer.AutoReset = false;

            _pingTimer.Enabled = true;
            socketConnected = true;
            reconnecting = false;
        }

        private static void ProcessMessageCreateFrame(MessageCreateFrame messageCreateFrame)
        {
            var frameMessage = messageCreateFrame.data;
            if (frameMessage.type == "normal")
            {
                Users[frameMessage.author.id] = frameMessage.author;
            }

            if (Messages.ContainsKey(frameMessage.channelId))
            {
                var idx = -1;
                for (var i = 0; i < Messages[frameMessage.channelId].Count; ++i)
                {
                    if (Messages[frameMessage.channelId][i].nonce == frameMessage.nonce)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx > -1)
                {
                    Messages[frameMessage.channelId][idx] = frameMessage;
                }
                else
                {
                    var addLately = false;
                    if (_instance != null)
                    {
                        using (_instance.window.getScope())
                        {
                            addLately = HomePage.currentState.SelectedChannelId == frameMessage.channelId
                                        && ChattingWindow.currentState.m_ScrollController.offset > 50f;
                        }
                    }

                    if (addLately)
                    {
                        NewMessages.Add(frameMessage);
                    }
                    else
                    {
                        Messages[frameMessage.channelId].Insert(0, frameMessage);
                    }
                }
            }

            string selectedChannelId = null;
            if (_instance != null)
            {
                using (_instance.window.getScope())
                {
                    if (HomePage.currentState != null)
                    {
                        selectedChannelId = HomePage.currentState.SelectedChannelId;
                    }
                }
            }

            if (selectedChannelId != null)
            {
                if (frameMessage.channelId == selectedChannelId &&
                    ReadStates.ContainsKey(selectedChannelId))
                {
                    ReadStates[selectedChannelId].lastMessageId = frameMessage.id;
                }
            }

            if (frameMessage.mentionEveryone ||
                frameMessage.mentions.Any(m => m.id == currentUserId))
            {
                ReadStates[frameMessage.channelId].lastMentionId = frameMessage.id;
                if (selectedChannelId != null)
                {
                    if (frameMessage.channelId != selectedChannelId)
                    {
                        ReadStates[frameMessage.channelId].mentionCount += 1;
                    }
                }
            }

            if (Channels.ContainsKey(messageCreateFrame.data.channelId))
            {
                Channels[messageCreateFrame.data.channelId].lastMessage =
                    messageCreateFrame.data;
            }
            
            if (Channels.ContainsKey(frameMessage.channelId) &&
                !Channels[frameMessage.channelId].isMute &&
                frameMessage.author.id != currentUserId)
            {
                var userId = messageCreateFrame.data.author.id;
                var username = messageCreateFrame.data.author.fullName;
                var avatar = messageCreateFrame.data.author.avatar;
                var content = messageCreateFrame.data.content;
                var msgId = messageCreateFrame.data.id;
                var channelId = messageCreateFrame.data.channelId;
                var inChannel = messageCreateFrame.data.channelId == selectedChannelId;
                var notificationContent = "";
                
                if (messageCreateFrame.data.type == "voice") {
                    notificationContent = $"{username}: [语音]";
                } else if (messageCreateFrame.data.attachments != null && messageCreateFrame.data.attachments.Count > 0)
                {
                    var contentType = messageCreateFrame.data.attachments.first().contentType;
                    if (contentType == "image/png" || contentType == "image/jpg" || contentType == "image/jpeg" ||
                        contentType == "image/gif")
                    {
                        notificationContent = $"{username}: [图片]";
                    }
                    else
                    {
                        notificationContent = $"{username}: [文件]";
                    }
                } else {
                    notificationContent = $"{username}: {ParseMessageToString(content, Users)}";
                }
                
                UnityMainThreadDispatcher.Instance().Enqueue(() => NotificationManager.instance.ShowNotification(
                    inChannel,
                    userId,
                    $"{Channels[channelId].name}",
                    notificationContent,
                    msgId,
                    channelId,
                    avatar,
                    id =>
                    {
                        var instance = _instance == null
                            ? GetWindow<Window>("Messenger", typeof(SceneView))
                            : _instance;

                        using (instance.window.getScope())
                        {
                            HomePage.currentState.Select(id);
                        }
                    }));
            }
        }

        private static void ProcessChannelDeleteFrame(ChannelDeleteFrame channelDeleteFrame)
        {
            var exist = Channels.Remove(channelDeleteFrame.data.id);
            if (exist)
            {
                if (_instance != null)
                {
                    using (_instance.window.getScope())
                    {
                        if (HomePage.currentState.SelectedChannelId == channelDeleteFrame.data.id)
                        {
                            HomePage.currentState.SelectedChannelId = string.Empty;
                            HomePage.currentState.IsShowChannelInfo = false;
                        }
                    }
                }
            }
        }

        private static void ProcessChannelCreateFrame(ChannelCreateFrame channelCreateFrame)
        {
            var frameData = channelCreateFrame.data;
            if (frameData.type == "lobby" &&
                frameData.liveChannelId.IsNullOrEmpty() &&
                frameData.workspaceId == DefaultWorkspaceId
            )
            {
                Channels[frameData.id] = frameData;
                ReadStates[frameData.id] = new ReadState
                {
                    lastMessageId = frameData.lastMessageId,
                    mentionCount = 0,
                };
                if (frameData.lastMessage != null)
                {
                    Users[frameData.lastMessage.author.id] = frameData.lastMessage.author;
                }

                if (!Members.ContainsKey(frameData.id))
                {
                    Members[frameData.id] = new List<ChannelMember>();
                }

                if (!frameData.groupId.IsNullOrEmpty())
                {
                    Groups[frameData.groupId] = frameData.groupMap[frameData.groupId];
                }
            }
        }

        private static void ProcessMessageDeleteFrame(MessageDeleteFrame messageDeleteFrame)
        {
            var frameMessage = messageDeleteFrame.data;
            if (Channels.ContainsKey(frameMessage.channelId) &&
                Channels[frameMessage.channelId].lastMessage.id == frameMessage.id)
            {
                Channels[frameMessage.channelId].lastMessage = frameMessage;
            }

            if (Messages.ContainsKey(frameMessage.channelId))
            {
                for (var i = 0; i < Messages[frameMessage.channelId].Count; ++i)
                {
                    if (Messages[frameMessage.channelId][i].id == frameMessage.id)
                    {
                        Messages[frameMessage.channelId][i] = frameMessage;
                    }
                }
            }

            for (var i = 0; i < NewMessages.Count; ++i)
            {
                if (NewMessages[i].id == frameMessage.id)
                {
                    NewMessages[i] = frameMessage;
                }
            }
        }

        private static void ProcessChannelUpdateFrame(ChannelUpdateFrame channelUpdateFrame)
        {
            if (Channels.ContainsKey(channelUpdateFrame.data.id))
            {
                Channels[channelUpdateFrame.data.id] = channelUpdateFrame.data;
            }
        }

        private static void ProcessMessageUpdateFrame(MessageUpdateFrame messageUpdateFrame)
        {
            var frameMessage = messageUpdateFrame.data;
            if (Channels[frameMessage.channelId].lastMessage.id == frameMessage.id)
            {
                Channels[frameMessage.channelId].lastMessage = frameMessage;
            }

            for (var i = 0; i < Messages[frameMessage.channelId].Count; ++i)
            {
                if (Messages[frameMessage.channelId][i].id == frameMessage.id)
                {
                    Messages[frameMessage.channelId][i] = frameMessage;
                    break;
                }
            }

            for (var i = 0; i < NewMessages.Count; ++i)
            {
                if (NewMessages[i].id == frameMessage.id)
                {
                    NewMessages[i] = frameMessage;
                    break;
                }
            }
        }

        private static void ProcessPingFrame(PingFrame pingFrame)
        {
            _timeoutTimer.Enabled = false;
            _pingTimer.Enabled = true;
        }
    }
}