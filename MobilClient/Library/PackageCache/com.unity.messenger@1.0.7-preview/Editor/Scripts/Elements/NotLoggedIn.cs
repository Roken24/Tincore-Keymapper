using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger
{
    public partial class Elements
    {
        internal static Widget CreateNotLoggedIn()
        {
            return new Container(
                color: new Color(0xffc2c2c2),
                alignment: Alignment.center,
                child: new Text(
                    "开启Messenger服务需要登录你的Unity ID",
                    style: new TextStyle(
                        color: new Color(0xff000000),
                        fontFamily: "PingFang"
                    )
                )
            );
        }
    }
}