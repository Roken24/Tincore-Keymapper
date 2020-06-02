using System.Collections.Generic;
using Unity.Messenger.Components;
using Unity.UIWidgets.widgets;
using static Unity.Messenger.Utils;

namespace Unity.Messenger.Widgets
{
    public class AckTrigger : StatefulWidget
    {
        public AckTrigger(
            Dictionary<string, Models.Channel> channels,
            Dictionary<string, Models.ReadState> readStates
        ) : base(key: new ObjectKey(CreateNonce()))
        {
            this.channels = channels;
            this.readStates = readStates;
        }

        internal readonly Dictionary<string, Models.Channel> channels;
        internal readonly Dictionary<string, Models.ReadState> readStates;

        public override State createState()
        {
            return new AckTriggerState();
        }
    }

    internal class AckTriggerState : State<AckTrigger>
    {
        public override Widget build(BuildContext context)
        {
            var rootState = HomePage.of(context);
            var channel = widget.channels[rootState.SelectedChannelId];
            var readState = widget.readStates[rootState.SelectedChannelId];
            if (channel.lastMessage != null &&
                channel.lastMessage.id != readState.lastMessageId)
            {
                rootState.Ack(rootState.SelectedChannelId);
            }

            return new Container();
        }
    }
}