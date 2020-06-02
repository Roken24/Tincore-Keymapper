using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Color = Unity.UIWidgets.ui.Color;
using static Unity.Messenger.Utils;
using File = UnityEngine.Windows.File;

namespace Unity.Messenger.Widgets
{
    public class Sender : StatefulWidget
    {
        internal readonly FocusNode focusNode;
        internal readonly Dictionary<string, User> users;
        private static readonly GlobalKey<SenderState> Key = new GlobalObjectKey<SenderState>("sender");

        public Sender(
            FocusNode focusNode,
            Dictionary<string, User> users
        ) : base(key: Key)
        {
            this.focusNode = focusNode;
            this.users = users;
        }

        public override State createState()
        {
            return new SenderState();
        }

        internal static SenderState currentState => Key.currentState;
    }

    internal class SenderState : State<Sender>
    {
        private static readonly string[] FileFilters =
        {
            "图片（*.png, *.jpg, *.jpeg）",
            "png,jpg,jpeg",
        };

        private static readonly string[] EmptyFilters = { };

        private TextEditingController m_TextEditingController;
        private HashSet<string> m_MentionCandidates;
        private DateTime? m_LastTypingTimestamp;

        public void AppendString(string sz)
        {
            var start = m_TextEditingController.selection.start;
            var end = m_TextEditingController.selection.end;
            if (start == -1)
            {
                start = m_TextEditingController.text.Length;
                end = start - 1 > 0 ? start - 1 : 0;
            }

            var newSz = m_TextEditingController.text.Substring(0, start) + sz;
            var newStart = newSz.Length;
            newSz += m_TextEditingController.text.Substring(end);
            m_TextEditingController.value = new TextEditingValue(
                newSz,
                selection: new TextSelection(
                    baseOffset: newStart,
                    extentOffset: newStart
                )
            );
        }

        public void AddCandidate(string id, string prevText, int beginIndex, int endIndex)
        {
            m_MentionCandidates.Add(id);
            var newText = $"{prevText.Substring(0, beginIndex)}@{widget.users[id].fullName}  ";
            var curPos = newText.Length;
            newText = $"{newText}{prevText.Substring(endIndex)}";
            m_TextEditingController.value = new TextEditingValue(
                newText,
                selection: new TextSelection(
                    curPos,
                    curPos,
                    Unity.UIWidgets.ui.TextAffinity.downstream,
                    false
                )
            );
        }

        public void DirectlyAddCandidate(string id)
        {
            m_MentionCandidates.Add(id);
            var newText = "";
            if (m_TextEditingController.selection.start == -1)
            {
                newText = $"{m_TextEditingController.text}";
                FocusScope.of(context).requestFocus(widget.focusNode);
            }
            else
            {
                var prev = m_TextEditingController.text.Substring(
                    0,
                    m_TextEditingController.selection.start);
                if (prev.Length != 0)
                {
                    newText += prev;
                }
            }

            newText += $"@{widget.users[id].fullName} ";
            var curPos = newText.Length;
            if (m_TextEditingController.selection.end != -1)
            {
                var suf = m_TextEditingController.text.Substring(m_TextEditingController.selection.end);
                newText += suf;
            }

            m_TextEditingController.value = new TextEditingValue(
                newText,
                selection: new TextSelection(
                    curPos,
                    curPos,
                    Unity.UIWidgets.ui.TextAffinity.downstream,
                    false
                )
            );
        }

        public void AddMentionEveryone(string prevText, int beginIndex, int endIndex)
        {
            var newText = $"{prevText.Substring(0, beginIndex)}@所有人 ";
            var curPos = newText.Length;
            newText = $"{newText}{prevText.Substring(endIndex)}";
            m_TextEditingController.value = new TextEditingValue(
                newText,
                selection: new TextSelection(
                    curPos,
                    curPos,
                    Unity.UIWidgets.ui.TextAffinity.downstream,
                    false
                )
            );
        }

        public void UnFocus()
        {
            FocusScope.of(context).requestFocus(new FocusNode());
        }

        public void Clear()
        {
            m_MentionCandidates.Clear();
            m_TextEditingController.value = new TextEditingValue();
            widget.focusNode.unfocus();
        }

        public override void initState()
        {
            base.initState();
            m_MentionCandidates = new HashSet<string>();
            m_TextEditingController = new TextEditingController();
            widget.focusNode.addListener(OnFocusChanged);
            m_TextEditingController.addListener(TextControllerListener);
        }

        private void OnFocusChanged()
        {
            if (!widget.focusNode.hasFocus)
            {
                MentionPopup.currentState.StopMention();
            }
            else
            {
                MentionPopup.currentState?.StartMention();
            }

            this.setState();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget)
        {
            ((Sender) oldWidget).focusNode.removeListener(OnFocusChanged);
            base.didUpdateWidget(oldWidget);
            widget.focusNode.addListener(OnFocusChanged);
        }

        public override void dispose()
        {
            m_TextEditingController.dispose();
            widget.focusNode.removeListener(OnFocusChanged);
            base.dispose();
        }

        private void TextControllerListener()
        {
            MentionPopup.currentState?.UpdateQueryAndCursor(m_TextEditingController.value.text,
                m_TextEditingController.selection.start);
        }

        private RawInputKeyResponse GlobalKeyEventHandler(
            RawKeyEvent rawKeyEvent,
            bool enableCustomAction)
        {
            var keyCode = rawKeyEvent.data.unityEvent.keyCode;

            if (keyCode == KeyCode.UpArrow)
            {
                if (enableCustomAction)
                {
                    MentionPopup.currentState.SelectPrev();
                }

                return RawInputKeyResponse.swallowResponse;
            }

            if (keyCode == KeyCode.DownArrow)
            {
                if (enableCustomAction)
                {
                    MentionPopup.currentState.SelectNext();
                }

                return RawInputKeyResponse.swallowResponse;
            }

            if (rawKeyEvent.data.unityEvent.character == '\n' ||
                rawKeyEvent.data.unityEvent.character == '\r' ||
                rawKeyEvent.data.unityEvent.character == 3 ||
                rawKeyEvent.data.unityEvent.character == 10)
            {
                if (MentionPopup.currentState.selectedIndex == -1 ||
                    !MentionPopup.currentState.isMentioning)
                {
                    if (rawKeyEvent.data.unityEvent.shift)
                    {
                        return new RawInputKeyResponse(true, '\n', TextInputAction.newline);
                    }
                    else
                    {
                        if (enableCustomAction)
                        {
                            OnSubmitted(m_TextEditingController.text);
                            return RawInputKeyResponse.swallowResponse;
                        }
                    }
                }
                else
                {
                    if (enableCustomAction)
                    {
                        MentionPopup.currentState.Select();
                    }

                    return RawInputKeyResponse.swallowResponse;
                }
            }

            return RawInputKeyResponse.convert(rawKeyEvent);
        }

        private void OnSubmitted(string value)
        {
            if (value.Trim() != string.Empty)
            {
                var rootState = HomePage.of(context);
                var nonce = CreateNonce();
                var content = value.Trim();
                content = content.Replace("@所有人", "@everyone");
                var mentions = new List<User>();
                var list = m_MentionCandidates.ToList();
                for (var i = 0; i < list.Count; ++i)
                {
                    for (var j = i + 1; j < list.Count; ++j)
                    {
                        var uni = widget.users[list[i]].fullName;
                        var unj = widget.users[list[j]].fullName;
                        if (unj.StartsWith(uni))
                        {
                            var tmp = list[i];
                            list[i] = list[j];
                            list[j] = tmp;
                        }
                    }
                }

                foreach (var mentionCandidate in list)
                {
                    var candidateUser = widget.users[mentionCandidate];
                    if (candidateUser != null &&
                        content.IndexOf($"@{candidateUser.fullName}",
                            StringComparison.Ordinal) > -1)
                    {
                        mentions.Add(candidateUser);
                        content = content.Replace(
                            $"@{candidateUser.fullName}",
                            $"<@{candidateUser.id}>");
                    }
                }

                Window.NewMessages.ForEach(msg =>
                    Window.Messages[msg.channelId].Insert(0, msg));
                Window.NewMessages.Clear();
                setState();
                SchedulerBinding.instance.addPostFrameCallback(timeSpan =>
                {
                    ChattingWindow.currentState?.m_ScrollController.animateTo(
                        0,
                        new TimeSpan(0, 0, 0, 0, 480),
                        Curves.easeInOut
                    );
                });
                rootState.InsertMessage(
                    rootState.SelectedChannelId,
                    new Models.Message
                    {
                        channelId = rootState.SelectedChannelId,
                        content = content,
                        nonce = nonce,
                        embeds = new List<Embed>(),
                        attachments = new List<Attachment>(),
                        author = widget.users[Window.currentUserId],
                        mentions = mentions,
                    }
                );
                m_MentionCandidates.Clear();
                Post<Models.Message>(
                    $"/api/channels/{rootState.SelectedChannelId}/messages",
                    new Models.SendMessageRequest(
                        content,
                        nonce,
                        null
                    ).ToJson().ToString()
                ).Then((message) => { });
                m_TextEditingController.value = new TextEditingValue(string.Empty);
            }

            SchedulerBinding.instance.addPostFrameCallback(_ =>
            {
                FocusScope.of(context).requestFocus(widget.focusNode);
            });
        }

        public override Widget build(BuildContext context)
        {
            var stacked = new List<Widget>
            {
                new Row(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Container(
                                alignment: Alignment.centerLeft,
                                padding: EdgeInsets.only(left: 24, bottom: 2),
                                child: new EditableText(
                                    globalKeyEventHandler: GlobalKeyEventHandler,
                                    controller: m_TextEditingController,
                                    focusNode: widget.focusNode,
                                    style: new TextStyle(
                                        color: new Color(0xff000000),
                                        fontSize: 16,
                                        height: 28 / 22.4f,
                                        fontFamily: "PingFang"
                                    ),
                                    selectionColor: new Color(0xff2199f4),
                                    cursorColor: new Color(0xa0000000),
                                    minLines: 1,
                                    maxLines: 8,
                                    onSubmitted: OnSubmitted,
                                    backgroundCursorColor: new Color(0xffffffff)
                                )
                            )
                        ),
                        new GestureDetector(
                            onTap: () =>
                            {
                                var filePath = EditorUtility.OpenFilePanelWithFilters(
                                    "",
                                    "",
                                    EmptyFilters);
                                if (filePath != null && filePath.isNotEmpty())
                                {
                                    var rootState = HomePage.of(context);
                                    var bytes = File.ReadAllBytes(filePath);
                                    var fileName = System.IO.Path.GetFileName(filePath);
                                    var ext = System.IO.Path.GetExtension(fileName);
                                    var nonce = CreateNonce();
                                    var mimeType = GetMimeType(ext);
                                    var form = new List<IMultipartFormSection>
                                    {
                                        new MultipartFormDataSection("channel", rootState.SelectedChannelId),
                                        new MultipartFormDataSection("nonce", nonce),
                                        new MultipartFormDataSection("size", $"{bytes.Length}"),
                                        new MultipartFormFileSection("file", bytes, fileName, mimeType)
                                    };

                                    PostForm<Models.Message>(
                                        $"/api/channels/{rootState.SelectedChannelId}/messages/attachments",
                                        form,
                                        message => { },
                                        out var progress
                                    );
                                    Window.NewMessages.ForEach(msg => Window.Messages[msg.channelId].Insert(0, msg));
                                    Window.NewMessages.Clear();
                                    setState();
                                    SchedulerBinding.instance.addPostFrameCallback(value =>
                                    {
                                        ChattingWindow.currentState?.m_ScrollController.animateTo(
                                            0,
                                            new TimeSpan(0, 0, 0, 0, 480),
                                            Curves.easeInOut
                                        );
                                    });
                                    
                                    var attachment = new Attachment
                                    {
                                        local = true,
                                        url = filePath,
                                        contentType = mimeType,
                                        progress = progress,
                                        filename = fileName,
                                        size = bytes.Length,
                                    };
                                    if (mimeType.StartsWith("image/"))
                                    {
                                        var texture = new Texture2D(1, 1);
                                        texture.LoadImage(bytes);
                                        attachment.width = texture.width;
                                        attachment.height = texture.height;
                                    }
                                    rootState.InsertMessage(
                                        rootState.SelectedChannelId,
                                        new Models.Message
                                        {
                                            channelId = rootState.SelectedChannelId,
                                            content = string.Empty,
                                            nonce = nonce,
                                            embeds = new List<Embed>(),
                                            attachments = new List<Attachment>
                                            {
                                                attachment,
                                            },
                                            author = widget.users[Window.currentUserId],
                                        }
                                    );
                                }
                            },
                            child: new Container(
                                margin: EdgeInsets.only(
                                    right: 24,
                                    left: 16
                                ),
                                child: new Icon(
                                    IconFont.IconFontSendPic,
                                    size: 28,
                                    color: new Color(0xff979a9e)
                                )
                            )
                        ),
                    }
                ),
            };
            if (!widget.focusNode.hasFocus && m_TextEditingController.text.isEmpty())
            {
                stacked.Add(
                    new IgnorePointer(
                        child: new Container(
                            alignment: Alignment.centerLeft,
                            margin: EdgeInsets.only(left: 24),
                            height: 28,
                            child: new Text(
                                "说点想法…",
                                style: new TextStyle(
                                    color: new Color(0xff797979),
                                    fontSize: 16,
                                    fontFamily: "PingFang"
                                )
                            )
                        )
                    )
                );
            }

            var outterStacked = new List<Widget>
            {
                new GestureDetector(
                    child: new Container(
                        decoration: new BoxDecoration(
                            color: new Color(0xffffffff),
                            border: Border.all(
                                color: new Color(0xffd8d8d8),
                                width: 2
                            ),
                            borderRadius: BorderRadius.circular(10)
                        ),
                        padding: EdgeInsets.symmetric(vertical: 16),
                        margin: EdgeInsets.all(24),
                        child: new Stack(
                            children: stacked
                        )
                    ),
                    onTap: () =>
                    {
                        FocusScope.of(context).requestFocus(widget.focusNode);
                    }
                ),
            };

            var result = new Stack(
                children: outterStacked
            );
            return result;
        }
    }
}