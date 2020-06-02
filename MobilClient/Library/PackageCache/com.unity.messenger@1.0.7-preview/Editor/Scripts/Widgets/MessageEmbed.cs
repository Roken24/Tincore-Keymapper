using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;
using static Unity.Messenger.Utils;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.Messenger.Widgets {
    public class MessageEmbed : StatelessWidget {
        public MessageEmbed(
            Unity.Messenger.Models.Message message,
            Action<TapUpDetails, string> onClickUrl,
            Key key = null
        ) : base(key: key) {
            this.message = message;
            this.onClickUrl = onClickUrl;
        }

        readonly Unity.Messenger.Models.Message message;
        readonly Action<TapUpDetails, string> onClickUrl;

        static readonly TextStyle _embedTitleStyle = new TextStyle(
            fontSize: 16,
            fontFamily: "PingFang",
            color: new Color(0xFF2196F3)
        );

        static readonly TextStyle _embedDescriptionStyle = new TextStyle(
            fontSize: 14,
            fontFamily: "PingFang",
            color: new Color(0xFF616161)
        );

        public override Widget build(BuildContext context) {
            if (this.message == null) {
                return new Container();
            }

            if (this.message.embeds.isEmpty()) {
                return new Container();
            }

            return new Container(
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(height: 6),
                        this._buildEmbedExternal()
                    }
                )
            );
        }

        Widget _buildEmbedExternal() {
            var embedData = this.message.embeds[0].embedData;

            return new GestureDetector(
                onTapUp: details => this.onClickUrl(details, embedData.url),
                child: new Container(
                    padding: EdgeInsets.all(6),
                    decoration: new BoxDecoration(
                        color: new Color(0xFFFFFFFF),
                        borderRadius: BorderRadius.all(4)
                    ),
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            embedData.title.isEmpty()
                                ? new Container()
                                : new Container(
                                    padding: EdgeInsets.only(bottom: 4),
                                    child: new Text(
                                        data: embedData.title,
                                        style: _embedTitleStyle
                                    )
                                ),
                            embedData.description.isEmpty()
                                ? new Container()
                                : new Container(
                                    padding: EdgeInsets.only(bottom: 4),
                                    child: new Text(
                                        data: embedData.description,
                                        style: _embedDescriptionStyle,
                                        maxLines: 4,
                                        overflow: TextOverflow.ellipsis
                                    )
                                ),
                            _buildEmbeddedName(image: embedData.image, name: embedData.name)
                        }
                    )
                )
            );
        }

        static Widget _buildEmbeddedName(string image, string name) {
            if (image.isEmpty() && name.isEmpty()) {
                return new Container();
            }

            return new Row(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: new List<Widget> {
                    image == null
                        ? (Widget) new Container(width: 14, height: 14)
                        : new Container(
                            width: 14,
                            height: 14,
                            child: new Image(
                                image: ProxiedImage(image),
                                width: 14,
                                height: 14,
                                fit: BoxFit.cover
                            )),
                    new Container(width: 4),
                    new Expanded(
                        child: new Text(
                            name ?? "",
                            style: new TextStyle(
                                height: 1.46f,
                                fontSize: 14,
                                fontFamily: "Roboto-Medium",
                                color: new Color(0xFF212121)
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis
                        )
                    )
                }
            );
        }

        public static float CalculateTextHeight(Unity.Messenger.Models.Message message, float width) {
            if (message == null || message.embeds.isEmpty()) {
                return 0;
            }

            var innerWidth = width - 2 * 6;
            var embedData = message.embeds[0].embedData;

            var textPainter = new TextPainter(
                textDirection: TextDirection.ltr,
                text: new TextSpan(
                    text: embedData.title,
                    style: _embedTitleStyle
                )
            );
            textPainter.layout(maxWidth: innerWidth);
            var embedDataTitleHeight = textPainter.getLineCount() * 23;

            textPainter = new TextPainter(
                textDirection: TextDirection.ltr,
                text: new TextSpan(
                    text: embedData.description,
                    style: _embedDescriptionStyle
                )
            );
            textPainter.layout(maxWidth: innerWidth);
            var embedDescriptionHeight = textPainter.getLineCount() * 23;

            float embedNameHeight;
            if (embedData.image.isEmpty() && embedData.name.isEmpty()) {
                embedNameHeight = 0;
            }
            else {
                embedNameHeight = 23;
            }

            var descriptionHeight = embedDataTitleHeight + 4 + embedDescriptionHeight + 4 + embedNameHeight;

            return descriptionHeight + 24;
        }
    }
}