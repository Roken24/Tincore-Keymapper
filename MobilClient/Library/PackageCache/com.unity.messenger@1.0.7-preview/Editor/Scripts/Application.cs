using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.Messenger.Models;
using Unity.Messenger.Style;
using Unity.Messenger.Widgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using static Unity.Messenger.Elements;
using Message = Unity.Messenger.Widgets.Message;

namespace Unity.Messenger
{
    public class Application
    {
        public void OnEnable()
        {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/PingFangHeiTC-W4"), "PingFang");
            FontManager.instance.addFont(Resources.Load<Font>("fonts/PingFangHeiTC-W6"), "PingFang", FontWeight.w500);
            IconFont.Load();
        }

        public Widget CreateWidget(
            Dictionary<string, User> users,
            Dictionary<string, ReadState> readStates,
            Dictionary<string, List<Models.Message>> messages,
            Dictionary<string, Channel> channels,
            Dictionary<string, List<ChannelMember>> members,
            Dictionary<string, bool> hasMoreMembers,
            Dictionary<string, Group> groups,
            Dictionary<string, bool> pullFlags,
            List<Channel> discoverChannels)
        {
            return new WidgetsApp(
                textStyle: new TextStyle(
                    fontSize: 14,
                    fontFamily: "PingFang",
                    color: new Color(0xff000000),
                    textBaseline: TextBaseline.ideographic
                ),
                onGenerateRoute:
                settings =>
                {
                    return new MaterialPageRoute(
                        builder: context => new HomePage(
                            users: users,
                            readStates: readStates,
                            messages: messages,
                            channels: channels,
                            members: members,
                            hasMoreMembers: hasMoreMembers,
                            groups,
                            pullFlags: pullFlags,
                            discoverChannels: discoverChannels
                        )
                    );
                }
            );
        }
    }
}