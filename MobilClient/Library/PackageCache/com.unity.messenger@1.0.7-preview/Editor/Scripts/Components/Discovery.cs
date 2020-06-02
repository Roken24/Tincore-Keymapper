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
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.Messenger.Components
{
    public class Discovery : StatefulWidget
    {
        public Discovery(
            Dictionary<string, Channel> channels,
            Dictionary<string, Group> groups
        ) : base(key: new GlobalObjectKey<DiscoveryState>("discovery"))
        {
            this.channels = channels;
            this.groups = groups;
        }

        internal readonly Dictionary<string, Channel> channels;
        internal readonly Dictionary<string, Group> groups;

        public override State createState()
        {
            return new DiscoveryState();
        }
    }

    internal class DiscoveryState : SingleTickerProviderStateMixin<Discovery>
    {
        private static readonly EdgeInsets HeaderTextPadding = EdgeInsets.only(
            left: 24
        );

        private static readonly Color HeaderBackgroundColor = new Color(0xffffffff);

        private static readonly TextStyle HeaderTextStyle = new TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            color: new Color(0xff212121),
            fontFamily: "PingFang"
        );

        private AnimationController m_AnimationController;
        private int? m_Total;

        public override void initState()
        {
            base.initState();
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
        }

        public override void didChangeDependencies()
        {
            base.didChangeDependencies();

            var screenWidth = MediaQuery.of(context).size.width;
            SchedulerBinding.instance.addPostFrameCallback(value =>
            {
                if (screenWidth < 750 && !m_AnimationController.isAnimating && mounted)
                {
                    m_AnimationController.animateTo(1, duration: TimeSpan.Zero);
                }
            });
        }

        private void AnimationStatusListener(AnimationStatus status)
        {
            if (status == AnimationStatus.dismissed)
            {
                var state = HomePage.of(context);
                state.IsShowDiscovery = false;
                state.setState();
            }
        }

        public override Widget build(BuildContext context)
        {
            var screenWidth = MediaQuery.of(context).size.width;
            var headerChildren = new List<Widget>
            {
                new Text(
                    "发现群聊",
                    style: HeaderTextStyle
                )
            };
            if (screenWidth < 750)
            {
                headerChildren.Insert(
                    0,
                    new GestureDetector(
                        onTap: () => m_AnimationController.reverse(),
                        child: new Container(
                            width: 28,
                            height: 28,
                            margin: EdgeInsets.only(right: 12),
                            child: new Icon(
                                IconFont.IconFontArrowBack,
                                size: 28,
                                color: new Color(0xff979a9e)
                            )
                        )
                    )
                );
            }

            var children = new List<Widget>
            {
                new Container(
                    height: 64,
                    padding: HeaderTextPadding,
                    alignment: Alignment.centerLeft,
                    decoration: new BoxDecoration(
                        border: new Border(
                            bottom: new BorderSide(
                                color: new Color(0xffd8d8d8),
                                width: 1
                            )
                        )
                    ),
                    child: new Row(
                        children: headerChildren
                    )
                ),
            };
            if (widget.channels == null || widget.channels.Count == 0)
            {
                children.Add(
                    new Expanded(
                        child: new Container(
                            alignment: Alignment.center,
                            child: new Loading(
                                size: 56
                            )
                        )
                    )
                );
            }
            else
            {
                var canvasWidth = screenWidth - 64;
                if (screenWidth >= 750)
                {
                    canvasWidth -= 375;
                }

                var countPerRow = (canvasWidth / 316).floor();
                var itemWidth = canvasWidth / (float) countPerRow - 16;
                var rowCount = (widget.channels.Count / (float) countPerRow).ceil();
                var rows = new List<Widget> { };
                for (var i = 0; i < rowCount; ++i)
                {
                    var items = new List<Widget> { };
                    for (var j = 0; j < countPerRow && i * countPerRow + j < widget.channels.Count; ++j)
                    {
                        items.Add(
                            new SquareLobbyCard(
                                widget.channels.Values.ToArray()[i * countPerRow + j],
                                widget.channels,
                                widget.groups,
                                itemWidth
                            )
                        );
                    }

                    rows.Add(
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.start,
                            children: items
                        )
                    );
                }

                children.Add(
                    new Expanded(
                        child: new Scroller(
                            child: new SingleChildScrollView(
                                child: new Container(
                                    padding: EdgeInsets.all(32),
                                    child: new Column(
                                        children: rows
                                    )
                                )
                            )
                        )
                    )
                );
            }

            Widget all = new Container(
                color: HeaderBackgroundColor,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
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