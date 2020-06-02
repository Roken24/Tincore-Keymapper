using System;
using System.Collections.Generic;
using Unity.Messenger.Models;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using static Unity.Messenger.Utils;
using Avatar = Unity.Messenger.Components.Avatar;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;

namespace Unity.Messenger.Widgets
{
    public class Message : StatefulWidget
    {
        internal readonly Unity.Messenger.Models.Message Msg;
        internal readonly Dictionary<string, Models.User> Users;
        internal readonly bool ShowTime;
        internal readonly bool IsNew;
        internal readonly DateTime CreateTime;
        internal readonly Action OnBuild;

        public Message(
            Models.Message Msg,
            Dictionary<string, User> Users,
            bool ShowTime,
            bool IsNew,
            DateTime CreateTime,
            Action OnBuild
        ) : base(key: new ObjectKey(Msg.id.isEmpty() ? Msg.id : Msg.nonce))
        {
            this.Msg = Msg;
            this.Users = Users;
            this.ShowTime = ShowTime;
            this.IsNew = IsNew;
            this.CreateTime = CreateTime;
            this.OnBuild = OnBuild;
        }

        public override State createState()
        {
            return new MessageState();
        }
    }

    internal class MessageState : State<Message>
    {
        private static readonly TextStyle SendingTextStyle = new TextStyle(
            color: new Color(0xff797979),
            fontSize: 16,
            fontFamily: "PingFang"
        );

        private static readonly TextStyle DeletedTextStyle = new TextStyle(
            color: new Color(0xff959595),
            fontSize: 16,
            fontStyle: FontStyle.italic,
            fontFamily: "PingFang"
        );

        private FocusNode _focusNode;
        private FocusNode _rightClickFocus;
        private OverlayEntry _rightClickEntry;
        private bool _everBuild;
        private bool _fetchingUrl;

        public override void initState()
        {
            base.initState();
            _focusNode = new FocusNode();
            _rightClickFocus = new FocusNode();
            _rightClickFocus.addListener(RightClickFocusListener);
            _fetchingUrl = false;
        }

        public override void dispose()
        {
            _rightClickFocus.removeListener(RightClickFocusListener);
            _rightClickFocus.dispose();
            _focusNode.dispose();
            base.dispose();
        }

        private void RightClickFocusListener()
        {
            if (_rightClickFocus.hasFocus)
            {
            }
            else
            {
                _rightClickEntry?.remove();
                _rightClickEntry = null;
            }

            setState();
        }

        private void OnTapUp(TapUpDetails details)
        {
            if (_rightClickFocus.hasFocus)
            {
                _rightClickEntry?.remove();
                _rightClickEntry = null;
            }

            FocusScope.of(context).requestFocus(new FocusNode());

            if (details.device == InputUtils.MouseLeftKeyDevice &&
                details.kind == PointerDeviceKind.mouse &&
                widget.Msg.attachments.Count > 0 &&
                !widget.Msg.attachments.first().local &&
                !_fetchingUrl)
            {
                _fetchingUrl = true;
                setState();
                var attachment = widget.Msg.attachments.first();
                Get<Attachment>(
                    url: $"/api/cdn-signed/message-attachments/{widget.Msg.id}/{attachment.id}"
                ).Then(atch =>
                {
                    _fetchingUrl = false;
                    setState();
                    Launch(atch.signedUrl);
                });
            }
            else if (details.device == InputUtils.MouseRightKeyDevice &&
                     details.kind == PointerDeviceKind.mouse)
            {
                var children = new List<Widget>();
                if (widget.Msg.content.isNotEmpty() &&
                    widget.Msg.id.isNotEmpty())
                {
                    children.Add(
                        new GestureDetector(
                            onTap: () =>
                            {
                                Clipboard.setData(
                                    new ClipboardData(text: ParseMessageToString(widget.Msg.content, widget.Users)));
                                FocusScope.of(context).requestFocus(new FocusNode());
                            },
                            child: new Container(
                                height: 40,
                                color: new Color(0x00000000),
                                alignment: Alignment.center,
                                child: new Text(
                                    "复制",
                                    style: new TextStyle(
                                        fontSize: 16,
                                        color: new Color(0xff000000),
                                        fontFamily: "PingFang"
                                    )
                                )
                            )
                        )
                    );
                    children.Add(
                        new Container(
                            height: 1,
                            color: new Color(0xffd8d8d8)
                        )
                    );
                    children.Add(
                        new GestureDetector(
                            onTap: () =>
                            {
                                Sender.currentState.AppendString(
                                    $"「{widget.Msg.author.fullName}: {ParseMessageToString(widget.Msg.content, widget.Users)}」\n- - - - - - - - - - - - - - -\n"
                                );
                                SchedulerBinding.instance.addPostFrameCallback(value =>
                                    FocusScope.of(context).requestFocus(Sender.currentState.widget.focusNode));
                            },
                            child: new Container(
                                height: 40,
                                color: new Color(0x00000000),
                                alignment: Alignment.center,
                                child: new Text(
                                    "引用",
                                    style: new TextStyle(
                                        fontSize: 16,
                                        color: new Color(0xff000000),
                                        fontFamily: "PingFang"
                                    )
                                )
                            )
                        )
                    );
                }

                if (widget.Msg.author.id == Window.currentUserId &&
                    widget.Msg.id.isNotEmpty() &&
                    (widget.Msg.attachments.isEmpty() || !widget.Msg.attachments.first().local))
                {
                    if (children.isNotEmpty())
                    {
                        children.Add(
                            new Container(
                                height: 1,
                                color: new Color(0xffd8d8d8)
                            )
                        );
                    }

                    children.Add(
                        new GestureDetector(
                            onTap: () =>
                            {
                                FocusScope.of(context).requestFocus(new FocusNode());
                                Confirm(
                                    context,
                                    "删除消息",
                                    "确认删除此条消息？",
                                    () =>
                                    {
                                        Post<Models.Message>(
                                            $"/api/connectapp/v1/messages/{widget.Msg.id}/delete",
                                            null
                                        ).Then(m => { });
                                    },
                                    () => { }
                                );
                            },
                            child: new Container(
                                height: 40,
                                color: new Color(0x00000000),
                                alignment: Alignment.center,
                                child: new Text(
                                    "删除",
                                    style: new TextStyle(
                                        fontSize: 16,
                                        color: new Color(0xff000000),
                                        fontFamily: "PingFang"
                                    )
                                )
                            )
                        )
                    );
                }

                if (children.Count == 0)
                {
                    return;
                }

                var focusScopeNode = FocusScope.of(context);
                _rightClickEntry = new OverlayEntry(buildContext =>
                {
                    return new GestureDetector(
                        onTap: () => { focusScopeNode.requestFocus(new FocusNode()); },
                        child: new Container(
                            color: new Color(0x00000000),
                            child: new Stack(
                                children: new List<Widget>
                                {
                                    new Positioned(
                                        left: details.globalPosition.dx,
                                        top: details.globalPosition.dy,
                                        child: new Container(
                                            width: 80,
                                            decoration: new BoxDecoration(
                                                borderRadius: BorderRadius.all(6),
                                                boxShadow: new List<BoxShadow>
                                                {
                                                    new BoxShadow(
                                                        blurRadius: 16,
                                                        color: new Color(0x33000000)
                                                    )
                                                },
                                                color: new Color(0xffffffff)
                                            ),
                                            child: new Column(
                                                children: children
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    );
                });
                FocusScope.of(context).requestFocus(_rightClickFocus);
                Overlay.of(context).insert(_rightClickEntry);
            }
        }

        public override Widget build(BuildContext context)
        {
            if (!_everBuild)
            {
                _everBuild = true;
                widget.OnBuild?.Invoke();
            }

            var sendByMe = widget.Msg.author.id == Window.currentUserId;

            Widget content = null;
            var mediaQueryData = MediaQuery.of(context);
            var maxWidth = mediaQueryData.size.width * 0.7f;
            if (mediaQueryData.size.width > 750)
            {
                maxWidth -= 262.5f;
            }

            if (!string.IsNullOrEmpty(widget.Msg.deletedTime))
            {
                content = new Container(
                    padding: EdgeInsets.all(12),
                    constraints: new BoxConstraints(
                        maxWidth: maxWidth
                    ),
                    decoration: new BoxDecoration(
                        color: sendByMe ? new Color(0xffc5e8ff) : new Color(0xfff0f0f0),
                        borderRadius: BorderRadius.circular(10)
                    ),
                    child: new SelectableText(
                        textSpan: new TextSpan(
                            text: "此条消息已被删除",
                            style: DeletedTextStyle
                        ),
                        focusNode: _focusNode,
                        selectionColor: new Color(0xffd8d8d8)
                    )
                );
            }
            else if (widget.Msg.content != null && widget.Msg.content.isNotEmpty())
            {
                var contentChildren = new List<Widget>
                {
                    new SelectableText(
                        textSpan: ParseMessage(
                            widget.Msg.content ?? "",
                            widget.Users,
                            textStyle: widget.Msg.id == null ? SendingTextStyle : null,
                            defaultOnTapUp: OnTapUp
                        ),
                        focusNode: _focusNode,
                        selectionColor: new Color(0xffd8d8d8),
                        onTapUp: OnTapUp
                    ),
                };
                if (!widget.Msg.embeds.isEmpty())
                {
                    contentChildren.Add(new Container(height: 6));
                    contentChildren.Add(new MessageEmbed(
                        message: widget.Msg,
                        onClickUrl: (details, url) =>
                        {
                            OnTapUp(details);
                            if (details.kind == PointerDeviceKind.mouse &&
                                details.device == InputUtils.MouseLeftKeyDevice)
                            {
                                Launch(url);
                            }
                        }));
                }

                content = new GestureDetector(
                    onTapUp: OnTapUp,
                    child: new Container(
                        padding: EdgeInsets.all(12),
                        constraints: new BoxConstraints(
                            maxWidth: maxWidth
                        ),
                        decoration: new BoxDecoration(
                            color: sendByMe ? new Color(0xffc5e8ff) : new Color(0xfff0f0f0),
                            borderRadius: BorderRadius.circular(10)
                        ),
                        child: new Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: contentChildren
                        )
                    )
                );
            }
            else if (widget.Msg.attachments.Count > 0)
            {
                var attachment = widget.Msg.attachments.first();
                var contentType = attachment.contentType;
                if (contentType == "image/png" ||
                    contentType == "image/jpg" ||
                    contentType == "image/jpeg" ||
                    contentType == "image/gif")
                {
                    Widget image = null;
                    if (attachment.local)
                    {
                        image = new ImageWithProgress(
                            image: new FileImage(
                                attachment.url
                            ),
                            attachment.progress
                        );
                    }
                    else
                    {
                        image = new Image(
                            image: ProxiedImage(
                                $"{attachment.url}.200x0x1.jpg",
                                cookie: $"LS={Window.loginSession};"
                            ),
                            fit: BoxFit.cover
                        );
                    }

                    content = new GestureDetector(
                        onTapUp: OnTapUp,
                        child: new Container(
                            constraints: new BoxConstraints(
                                maxWidth: 282
                            ),
                            child: new ClipRRect(
                                borderRadius: BorderRadius.circular(5),
                                child: new AspectRatio(
                                    aspectRatio: attachment.width == 0 || attachment.height == 0
                                        ? 1
                                        : (float) attachment.width / attachment.height,
                                    child: image
                                )
                            )
                        )
                    );
                }
                else
                {
                    AssetImage image;
                    if (contentType == "application/pdf")
                    {
                        image = new AssetImage("Images/FilePdf@4x");
                    }
                    else if (contentType.StartsWith("video/"))
                    {
                        image = new AssetImage("Images/FileVideo@4x");
                    }
                    else
                    {
                        image = new AssetImage("Images/FileGeneral@4x");
                    }

                    content = new GestureDetector(
                        onTapUp: OnTapUp,
                        child: new Container(
                            padding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            decoration: new BoxDecoration(
                                color: new Color(0xfff0f0f0),
                                borderRadius: BorderRadius.all(10)
                            ),
                            width: 262,
                            child: new Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>
                                {
                                    new Expanded(
                                        child: new Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget>
                                            {
                                                new Text(
                                                    attachment.filename,
                                                    style: new TextStyle(
                                                        fontSize: 16,
                                                        color: new Color(0xff000000),
                                                        fontFamily: "PingFang"
                                                    )
                                                ),
                                                new Container(
                                                    margin: EdgeInsets.only(top: 4),
                                                    child: new Text(
                                                        ReadableSize(attachment.size),
                                                        style: new TextStyle(
                                                            fontSize: 12,
                                                            color: new Color(0xff797979),
                                                            fontFamily: "PingFang"
                                                        )
                                                    )
                                                )
                                            }
                                        )
                                    ),
                                    new Container(
                                        margin: EdgeInsets.only(left: 16),
                                        width: 42,
                                        height: 48,
                                        child: new Image(
                                            image: image,
                                            width: 42,
                                            height: 48,
                                            fit: BoxFit.cover
                                        )
                                    )
                                }
                            )
                        )
                    );
                }
            }
            else
            {
                return new Container(height: 0, width: 0);
            }

            var decoratedContent = new List<Widget>
            {
                content,
            };
            if (_rightClickFocus.hasFocus)
            {
                decoratedContent.Add(
                    new Positioned(
                        top: 0,
                        right: 0,
                        bottom: 0,
                        left: 0,
                        child: new Container(
                            decoration: new BoxDecoration(
                                borderRadius: BorderRadius.all(10),
                                color: new Color(0x1a000000)
                            )
                        )
                    )
                );
            }

            if (_fetchingUrl)
            {
                decoratedContent.Add(
                    new Positioned(
                        top: 0,
                        right: 0,
                        bottom: 0,
                        left: 0,
                        child: new Container(
                            decoration: new BoxDecoration(
                                borderRadius: BorderRadius.all(10),
                                color: new Color(0x1a000000)
                            ),
                            alignment: Alignment.center,
                            child: new Loading(
                                size: 24,
                                isWhite: true
                            )
                        )
                    )
                );
            }

            var children = new List<Widget>
            {
                new Container(
                    margin: EdgeInsets.only(
                        left: sendByMe ? 10 : 24,
                        right: sendByMe ? 24 : 10
                    ),
                    child: new Avatar(
                        widget.Msg.author,
                        size: 40
                    )
                ),
                new Expanded(
                    child: new Column(
                        crossAxisAlignment: sendByMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: new List<Widget>
                        {
                            new Container(
                                height: 20,
                                alignment: sendByMe ? Alignment.centerRight : Alignment.centerLeft,
                                margin: EdgeInsets.only(bottom: 6),
                                child: new Text(
                                    widget.Msg.author.fullName,
                                    style: new TextStyle(
                                        fontSize: 12,
                                        fontWeight: FontWeight.w500,
                                        color: new Color(0xff797979),
                                        fontFamily: "PingFang"
                                    )
                                )
                            ),
                            new Stack(
                                children: decoratedContent
                            ),
                        }
                    )
                ),
            };

            if (sendByMe)
            {
                children.Reverse();
            }

            Widget result = new Container(
                margin: EdgeInsets.symmetric(vertical: 8),
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: children
                )
            );

            var resultChildren = new List<Widget> { };

            if (widget.IsNew)
            {
                resultChildren.Add(
                    new Container(
                        height: 36,
                        alignment: Alignment.center,
                        child: new Row(
                            crossAxisAlignment: CrossAxisAlignment.center,
                            children: new List<Widget>
                            {
                                new Expanded(
                                    child: new Container(
                                        margin: EdgeInsets.only(right: 8),
                                        height: 1,
                                        color: new Color(0xffd8d8d8)
                                    )
                                ),
                                new Container(
                                    alignment: Alignment.center,
                                    child: new Text(
                                        "以下为新消息",
                                        style: new TextStyle(
                                            fontSize: 12,
                                            fontWeight: FontWeight.w500,
                                            color: new Color(0xff959595),
                                            fontFamily: "PingFang"
                                        )
                                    )
                                ),
                                new Expanded(
                                    child: new Container(
                                        margin: EdgeInsets.only(left: 8),
                                        height: 1,
                                        color: new Color(0xffd8d8d8)
                                    )
                                ),
                            }
                        )
                    )
                );
            }

            if (widget.ShowTime)
            {
                resultChildren.Add(
                    new Container(
                        height: 36,
                        child: new Center(
                            child: new Text(
                                DateTimeString(widget.CreateTime),
                                style: new TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.w500,
                                    color: new Color(0xff797979),
                                    fontFamily: "PingFang"
                                )
                            )
                        )
                    )
                );
            }

            resultChildren.Add(result);

            return new Column(
                children: resultChildren
            );
        }
    }
}