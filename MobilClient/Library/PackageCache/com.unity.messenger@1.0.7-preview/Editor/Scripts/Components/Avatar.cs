using System.Collections.Generic;
using System.Text;
using Unity.Messenger.Models;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using static Unity.Messenger.Utils;

namespace Unity.Messenger.Components
{
    public class Avatar : StatefulWidget
    {
        public Avatar(
            User user,
            float size = 40
        )
        {
            this.user = user;
            this.size = size;
        }

        internal readonly float size;
        internal readonly User user;

        public override State createState()
        {
            return new AvatarState();
        }
    }

    internal class AvatarState : State<Avatar>
    {
        private void OnTapUp(TapUpDetails details)
        {
            if (details.device == InputUtils.MouseLeftKeyDevice &&
                details.kind == PointerDeviceKind.mouse)
            {
                Launch($"https://connect.unity.com/u/{widget.user.id}");
            }
        } 
        
        public override Widget build(BuildContext context)
        {
            Widget avatar = null;
            if (widget.user.avatar == null || widget.user.avatar.isEmpty())
            {
                avatar = new Container(
                    color: CColorUtils.GetAvatarBackgroundColor(id: widget.user.id),
                    alignment: Alignment.center,
                    child: new Text(
                        Utils.ExtractName(widget.user.fullName),
                        style: new TextStyle(
                            fontSize: widget.size / 2,
                            color: new Color(0xffffffff),
                            fontFamily: "PingFang"
                        )
                    )
                );
            }
            else
            {
                avatar = new Image(
                    image: ProxiedImage(widget.user.avatar),
                    fit: BoxFit.cover
                );
            }

            return new GestureDetector(
                onTapUp: OnTapUp,
                onLongPress: () =>
                {
                    if (Window.currentUserId != widget.user.id)
                    {
                        var senderState = Sender.currentState;
                        senderState.DirectlyAddCandidate(widget.user.id);
                    }
                },
                child: new ClipRRect(
                    borderRadius: BorderRadius.circular(widget.size / 2),
                    child: new Container(
                        width: widget.size,
                        height: widget.size,
                        child: avatar
                    )
                )
            );
        }
    }
    
    internal static class CColorUtils {
        private static readonly Color Gerakdine = new Color(0xFFFF8686);
        private static readonly Color Tan = new Color(0xFFFFAB6D);
        private static readonly Color Mustard = new Color(0xFFFFDB55);
        private static readonly Color Feijoa = new Color(0xFFADE376);
        private static readonly Color Riptide = new Color(0xFF80E5D7);
        private static readonly Color SkyBlue = new Color(0xFF86D9ED);
        private static readonly Color Portage = new Color(0xFF8DA8F2);
        private static readonly Color DullLavender = new Color(0xFF9E91F8);
        private static readonly Color BrightLavender = new Color(0xFFC586F3);
        private static readonly Color Comet = new Color(0xFF636672);

        private static readonly List<Color> ColorsList = new List<Color> {
            Gerakdine,
            Tan,
            Mustard,
            Feijoa,
            Riptide,
            SkyBlue,
            Portage,
            DullLavender,
            BrightLavender,
            Comet
        };

        public static Color GetAvatarBackgroundColor(string id) {
            return ColorsList[GetStableHash(s: id) % ColorsList.Count];
        }

        const int MUST_BE_LESS_THAN = 10000; // 4 decimal digits

        private static int GetStableHash(string s) {
            uint hash = 0;
            // if you care this can be done much faster with unsafe 
            // using fixed char* reinterpreted as a byte*
            foreach (byte b in Encoding.Unicode.GetBytes(s)) {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            // final avalanche
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            // helpfully we only want positive integer < MUST_BE_LESS_THAN
            // so simple truncate cast is ok if not perfect
            return (int) (hash % MUST_BE_LESS_THAN);
        }
    }
}