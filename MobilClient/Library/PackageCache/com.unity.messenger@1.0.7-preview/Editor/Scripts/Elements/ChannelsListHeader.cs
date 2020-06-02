using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger
{
    internal static class ChannelsListHeaderConstants
    {
        public static readonly TextStyle headerTextStyle = new TextStyle(
            fontSize: 24,
            color: new Color(0xff000000),
            fontFamily: "PingFang"
        );
        
        public static readonly EdgeInsets padding = EdgeInsets.only(left: 16);
    }
    public partial class Elements
    {
        public static Widget CreateChannelsListHeader() {
            return new Container(
                height: 64,
                alignment: Alignment.centerLeft,
                padding: ChannelsListHeaderConstants.padding,
                decoration: new BoxDecoration(
                    color: new Color(0xffffffff),
                    border: new Border(
                        bottom: new BorderSide(
                            color: new Color(0xffd8d8d8)
                        )
                    )
                ),
                child: new Text(
                    "群聊",
                    style: ChannelsListHeaderConstants.headerTextStyle
                )
            );
        }
    }
}
