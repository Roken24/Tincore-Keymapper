using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Components
{
    public class SwitchController : ValueNotifier<bool>
    {
        public SwitchController(bool value = false) : base(value)
        {
        }
    }

    public class Switch : StatefulWidget
    {
        public Switch(SwitchController controller)
        {
            this.controller = controller;
        }

        internal readonly SwitchController controller;

        public override State createState()
        {
            return new SwitchState();
        }
    }

    internal class SwitchState : TickerProviderStateMixin<Switch>
    {
        private AnimationController m_AnimationController;
        private Animation<Color> m_ColorAnimation;
        private Animation<float> m_PositionAnimation;

        public override void initState()
        {
            base.initState();
            m_AnimationController = new AnimationController(
                vsync: this,
                duration: TimeSpan.FromMilliseconds(180)
            );
            m_ColorAnimation = new ColorTween(
                begin: new Color(0xffe6e6e6),
                end: new Color(0xff2196f3)
            ).chain(new CurveTween(curve: Curves.linear)).animate(m_AnimationController);
            m_PositionAnimation = new FloatTween(
                begin: 2,
                end: 22
            ).chain(new CurveTween(curve: Curves.linear)).animate(m_AnimationController);
            m_AnimationController.addListener(() =>
            {
                if (mounted)
                {
                    setState(() => { });
                }
            });
            widget.controller.addListener(StatusListener);
            if (widget.controller.value)
            {
                m_AnimationController.animateTo(22, duration: TimeSpan.Zero);
            }
        }

        public void StatusListener()
        {
            if (widget.controller.value)
            {
                m_AnimationController.forward();
            }
            else
            {
                m_AnimationController.reverse();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget)
        {
            ((Switch) oldWidget).controller.removeListener(StatusListener);
            base.didUpdateWidget(oldWidget);
            widget.controller.addListener(StatusListener);
        }

        public override void dispose()
        {
            widget.controller.removeListener(StatusListener);
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            return new GestureDetector(
                onTap: () => { widget.controller.value ^= true; },
                child: new Stack(
                    children: new List<Widget>
                    {
                        new Container(
                            height: 32,
                            width: 52,
                            decoration: new BoxDecoration(
                                borderRadius: BorderRadius.circular(16),
                                color: m_ColorAnimation.value
                            )
                        ),
                        new Positioned(
                            top: 2,
                            left: m_PositionAnimation.value,
                            child: new Container(
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.circular(14),
                                    color: new Color(0xffffffff),
                                    border: Border.all(
                                        color: new Color(0x1a000000),
                                        width: 1
                                    ),
                                    boxShadow: new List<BoxShadow>
                                    {
                                        new BoxShadow(
                                            color: new Color(0x1a000000),
                                            offset: new Offset(0, 3),
                                            blurRadius: 1,
                                            spreadRadius: 0
                                        ),
                                        new BoxShadow(
                                            color: new Color(0x29000000),
                                            offset: new Offset(0, 1),
                                            blurRadius: 1,
                                            spreadRadius: 0
                                        ),
                                        new BoxShadow(
                                            color: new Color(0x26000000),
                                            offset: new Offset(0, 3),
                                            blurRadius: 8,
                                            spreadRadius: 0
                                        )
                                    }
                                ),
                                width: 28,
                                height: 28
                            )
                        )
                    }
                )
            );
        }
    }
}