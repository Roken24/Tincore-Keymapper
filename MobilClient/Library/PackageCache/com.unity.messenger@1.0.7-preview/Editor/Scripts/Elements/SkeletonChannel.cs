using System.Collections.Generic;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger
{
    public partial class Elements
    {
        private static class SkeletonChannelConstants
        {
            public static readonly EdgeInsets rowPadding = EdgeInsets.only(top: 12, bottom: 12, left: 16);

            public static readonly BoxDecoration rowDecoration = new BoxDecoration(
                color: new Color(0xffffffff),
                border: new Border(
                    bottom: new BorderSide(
                        color: new Color(0xffd8d8d8)
                    )
                )
            );

            public static readonly BoxDecoration squareDecoration = new BoxDecoration(
                color: new Color(0xfff8f8f8),
                borderRadius: BorderRadius.circular(4)
            );

            public static readonly EdgeInsets squareMargin = EdgeInsets.only(
                right: 16
            );

            public static readonly Color color = new Color(0xfff8f8f8);
        }

        public static Widget CreateSkeletonChannel()
        {
            return new Container(
                height: 72,
                padding: SkeletonChannelConstants.rowPadding,
                decoration: SkeletonChannelConstants.rowDecoration,
                child: new Row(
                    children: new List<Widget>
                    {
                        new Container(
                            width: 48,
                            height: 48,
                            decoration: SkeletonChannelConstants.squareDecoration,
                            margin: SkeletonChannelConstants.squareMargin
                        ),
                        new Column(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget>
                            {
                                new Container(
                                    height: 22,
                                    width: 120,
                                    color: SkeletonChannelConstants.color
                                ),
                                new Container(
                                    height: 22,
                                    width: 250,
                                    color: SkeletonChannelConstants.color
                                )
                            }
                        )
                    }
                )
            );
        }
    }
}