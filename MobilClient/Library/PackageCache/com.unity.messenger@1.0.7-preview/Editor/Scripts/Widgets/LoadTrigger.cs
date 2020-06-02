using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using static Unity.Messenger.Utils;

namespace Unity.Messenger.Widgets
{
    public class LoadTrigger : StatefulWidget
    {
        internal readonly Action onLoad;

        public LoadTrigger(Action onLoad) :
            base(key: new ObjectKey(CreateNonce()))
        {
            this.onLoad = onLoad;
        }

        public override State createState()
        {
            return new LoadTriggerState();
        }
    }

    internal class LoadTriggerState : State<LoadTrigger>
    {
        private bool m_Loading;

        public override void initState()
        {
            base.initState();
            m_Loading = false;
        }

        public override Widget build(BuildContext context)
        {
            if (m_Loading == false)
            {
                SchedulerBinding.instance.addPostFrameCallback(value =>
                {
                    setState(() =>
                    {
                        m_Loading = true;
                        widget.onLoad?.Invoke();
                    });
                });
            }

            return new Container(
                height: 56,
                alignment: Alignment.center,
                child: Transform.scale(
                    scale: 0.9f,
                    alignment: Alignment.center,
                    child: new Loading(
                        size: 40
                    )
                )
            );
        }
    }
}