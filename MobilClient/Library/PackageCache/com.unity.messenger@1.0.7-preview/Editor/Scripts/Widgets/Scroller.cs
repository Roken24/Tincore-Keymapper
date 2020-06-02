using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.async;

namespace Unity.Messenger.Widgets
{
    public class Scroller : StatefulWidget
    {
        public Scroller(Widget child)
        {
            this.child = child;
        }

        internal readonly Widget child;

        public override State createState()
        {
            return new ScrollerState();
        }
    }

    internal class ScrollerState : TickerProviderStateMixin<Scroller>
    {
        private static readonly Color ScrollbarColor = new Color(0x66000000);
        private static readonly float ScrollbarMinLength = 36;
        private static readonly float ScrollbarMinOverscrollLength = 8;
        private static readonly Radius ScrollbarRadius = Radius.circular(4f);
        private static readonly TimeSpan ScrollbarTimeToFade = TimeSpan.FromMilliseconds(50);
        private static readonly TimeSpan ScrollbarFadeDuration = TimeSpan.FromMilliseconds(250);

        private const float ScrollbarThickness = 8f;
        private const float ScrollbarMainAxisMargin = 3;
        private const float ScrollbarCrossAxisMargin = 3;

        private ScrollbarPainter m_ScrollbarPainter;
        private AnimationController m_FadeoutAnimationController;
        private Animation<float> m_FadeoutOpacityAnimation;
        private Timer m_FadeoutTimer;

        public override void initState()
        {
            base.initState();
            m_FadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: ScrollbarFadeDuration
            );
            m_FadeoutOpacityAnimation = new CurvedAnimation(
                parent: this.m_FadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
        }

        public override void didChangeDependencies()
        {
            base.didChangeDependencies();
            m_ScrollbarPainter = new ScrollbarPainter(
                ScrollbarColor,
                textDirection: TextDirection.ltr,
                ScrollbarThickness,
                fadeoutOpacityAnimation: this.m_FadeoutOpacityAnimation,
                ScrollbarMainAxisMargin,
                ScrollbarCrossAxisMargin,
                ScrollbarRadius,
                ScrollbarMinLength,
                ScrollbarMinOverscrollLength
            );
        }

        public override void dispose()
        {
            m_FadeoutAnimationController.dispose();
            m_FadeoutTimer?.cancel();
            m_ScrollbarPainter?.dispose();
            base.dispose();
        }

        bool _handleScrollNotification(ScrollNotification notification)
        {
            if (notification is ScrollUpdateNotification || notification is OverscrollNotification)
            {
                if (this.m_FadeoutAnimationController.status != AnimationStatus.forward)
                {
                    this.m_FadeoutAnimationController.forward();
                }

                this.m_FadeoutTimer?.cancel();
                this.m_ScrollbarPainter.update(
                    metrics: notification.metrics,
                    axisDirection: notification.metrics.axisDirection
                );
            }
            else if (notification is ScrollEndNotification)
            {
                this.m_FadeoutTimer?.cancel();
                this.m_FadeoutTimer = UIWidgets.ui.Window.instance.run(ScrollbarTimeToFade, () =>
                {
                    this.m_FadeoutAnimationController.reverse();
                    this.m_FadeoutTimer = null;
                });
            }

            return false;
        }

        public override Widget build(BuildContext context)
        {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: this.m_ScrollbarPainter,
                        child: new RepaintBoundary(
                            child: this.widget.child
                        )
                    )
                )
            );
        }
    }
}