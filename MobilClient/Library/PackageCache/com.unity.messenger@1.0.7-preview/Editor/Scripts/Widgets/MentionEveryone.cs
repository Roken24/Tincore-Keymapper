using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Widgets
{
    public class MentionEveryone : StatefulWidget
    {
        internal readonly System.Action onTap;

        public MentionEveryone(
            System.Action onTap
        )
        {
            this.onTap = onTap;
        }

        public override State createState()
        {
            return new MentionEveryoneState();
        }
    }

    internal class MentionEveryoneState : State<MentionEveryone>
    {
        public override Widget build(BuildContext context)
        {
            return new GestureDetector(
                onTap: () => widget.onTap(),
                child: new Container(
                    height: 40,
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    color: new Color(0xfff0f0f0),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Text(
                                "@所有人",
                                style: new TextStyle(
                                    color: new Color(0xff000000),
                                    fontSize: 16,
                                    fontFamily: "PingFang"
                                )
                            ),
                            new Text(
                                "群聊中所有人",
                                style: new TextStyle(
                                    color: new Color(0xff797979),
                                    fontSize: 14,
                                    fontFamily: "PingFang"
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}