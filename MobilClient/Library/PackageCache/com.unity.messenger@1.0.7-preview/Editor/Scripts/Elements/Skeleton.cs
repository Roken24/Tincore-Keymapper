using System.Collections.Generic;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;

namespace Unity.Messenger
{
    public partial class Elements
    {
        public static Widget CreateNarrowSkeleton()
        {
            return new Container(
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
                        slivers: new List<Widget>
                        {
                            new SliverToBoxAdapter(
                                child: CreateChannelsListHeader()
                            ),
                            new SliverToBoxAdapter(
                                child: CreateSkeletonChannel()
                            ),
                            new SliverToBoxAdapter(
                                child: CreateSkeletonChannel()
                            ),
                            new SliverToBoxAdapter(
                                child: CreateSkeletonChannel()
                            ),
                            new SliverToBoxAdapter(
                                child: CreateSkeletonChannel()
                            ),
                        }
                    )
                )
            );
        }
        public static Widget CreateSkeleton(BuildContext context)
        {
            return new Container(
                child: new Row(
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
                                    slivers: new List<Widget>
                                    {
                                        new SliverToBoxAdapter(
                                            child: CreateChannelsListHeader()
                                        ),
                                        new SliverToBoxAdapter(
                                            child: CreateSkeletonChannel()
                                        ),
                                        new SliverToBoxAdapter(
                                            child: CreateSkeletonChannel()
                                        ),
                                        new SliverToBoxAdapter(
                                            child: CreateSkeletonChannel()
                                        ),
                                        new SliverToBoxAdapter(
                                            child: CreateSkeletonChannel()
                                        ),
                                    }
                                )
                            )
                        ),
                        new Expanded(
                            child: new Container(
                                color: new Color(0xffffffff),
                                child: new Stack(
                                    fit: StackFit.expand,
                                    children: new List<Widget>
                                    {
                                        new Positioned(
                                            top: 0,
                                            left: 0,
                                            right: 0,
                                            child: new Container(
                                                height: 64,
                                                decoration: new BoxDecoration(
                                                    border: new Border(
                                                        bottom: new BorderSide(
                                                            color: new Color(0xffd8d8d8)
                                                        )
                                                    )
                                                ),
                                                padding: EdgeInsets.only(top: 12, bottom: 12, left: 24),
                                                child: new Column(
                                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                    crossAxisAlignment: CrossAxisAlignment.start,
                                                    children: new List<Widget>
                                                    {
                                                        new Container(
                                                            height: 20,
                                                            width: 146,
                                                            color: new Color(0xfff8f8f8)
                                                        ),
                                                        new Container(
                                                            height: 16,
                                                            width: 80,
                                                            color: new Color(0xfff8f8f8)
                                                        )
                                                    }
                                                )
                                            )
                                        ),
                                        new Container(
                                            alignment: Alignment.center,
                                            child: new Image(
                                                image: new AssetImage("Images/Loading")
                                            )
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }
    }
}