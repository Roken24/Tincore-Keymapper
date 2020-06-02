using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Widgets
{
    public class MentionUser : StatefulWidget
    {
        internal readonly User user;
        internal readonly System.Action onTap;
        internal readonly bool selected;

        public MentionUser(
            User user,
            System.Action onTap,
            bool selected
        )
        {
            this.user = user;
            this.onTap = onTap;
            this.selected = selected;
        }

        public override State createState()
        {
            return new MentionUserState();
        }
    }

    internal class MentionUserState : State<MentionUser>
    {
        private static readonly TextStyle UsernameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff000000),
            fontFamily: "PingFang"
        );

        private static readonly TextStyle HoverUsernameStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xffffffff),
            fontFamily: "PingFang"
        );

        private static readonly EdgeInsets AvatarMargin = EdgeInsets.only(right: 10);
        private static readonly EdgeInsets ContainerPadding = EdgeInsets.symmetric(horizontal: 16);
        private static readonly Color NormalBgColor = new Color(0xffffffff);
        private static readonly Color HoverBgColor = new Color(0xff6ec6ff);
        
        public override Widget build(BuildContext context)
        {
            return new GestureDetector(
                onTap: () => widget.onTap?.Invoke(),
                child: new Container(
                    height: 36,
                    color: widget.selected ? HoverBgColor : NormalBgColor,
                    padding: ContainerPadding,
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Container(
                                margin: AvatarMargin,
                                child: new Avatar(
                                    widget.user,
                                    size: 24
                                )
                            ),
                            new Expanded(
                                child: new Text(
                                    widget.user.fullName,
                                    style: widget.selected ? HoverUsernameStyle : UsernameStyle,
                                    overflow: TextOverflow.ellipsis
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}