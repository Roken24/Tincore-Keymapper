using System;
using System.Diagnostics.Eventing.Reader;
using Unity.Messenger.Style;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.Messenger.Widgets
{
    public class Loading : StatefulWidget
    {
        internal readonly float size;
        internal readonly bool isWhite;

        public Loading(float size, bool isWhite = false)
        {
            this.size = size;
            this.isWhite = isWhite;
        }

        public override State createState()
        {
            return new LoadingState();
        }
    }

    internal class LoadingState : SingleTickerProviderStateMixin<Loading>
    {
        private const float AnimationStart = Mathf.PI / 4;
        private const float AnimationEnd = Mathf.PI * 2 + AnimationStart;
        private static readonly Color IconColor = new Color(0xffd8d8d8);

        private AnimationController m_Controller;
        private Animation<float> m_Animation;

        public override void initState()
        {
            base.initState();
            m_Controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 2),
                vsync: this
            );
            m_Animation =
                m_Controller.drive(
                    new FloatTween(AnimationStart, AnimationEnd).chain(new CurveTween(Curves.linear)));
            m_Controller.addListener(() => setState(() => { }));
            m_Controller.repeat();
        }

        public override void dispose()
        {
            m_Controller.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            return Transform.rotate(
                degree: m_Animation.value,
                alignment: Alignment.center,
                child: new Container(
                    width: widget.size,
                    height: widget.size,
                    child: Image.asset(
                        widget.isWhite ? "Images/white-loading" : "Images/black-loading"
                    )
                )
            );
        }
    }
}