using Unity.Messenger.Style;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using static Unity.Messenger.Utils;

namespace Unity.Messenger
{
    internal static class LobbyIconConstants
    {
        public static Color BackgroundColor = new Color(0xffffb40d);
        public static Color IconColor = new Color(0xffffffff);
    }

    public partial class Elements
    {
        public static Widget CreateLobbyIcon(
            string url,
            float size = 32.0f,
            float radius = 3.0f
        )
        {
            Widget inner = null;
            if (url != null && url.isNotEmpty())
            {
                inner = new Image(
                    image: ProxiedImage(url),
                    fit: BoxFit.cover,
                    width: size,
                    height: size
                );
            }
            else
            {
                inner = new Container(
                    width: size,
                    height: size,
                    decoration: new BoxDecoration(
                        color: LobbyIconConstants.BackgroundColor
                    ),
                    alignment: Alignment.center,
                    child: new Icon(
                        IconFont.IconFontOpenEye,
                        size: size * 2 / 3,
                        color: LobbyIconConstants.IconColor
                    )
                );
            }

            return new ClipRRect(
                borderRadius: BorderRadius.circular(radius),
                child: inner
            );
        }
    }
}