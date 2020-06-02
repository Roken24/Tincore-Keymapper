using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;

namespace Unity.Messenger.Widgets
{
    public class ImageWithProgress : StatefulWidget
    {
        internal readonly ImageProvider image;
        internal readonly BoxFit? fit;
        internal readonly Func<float> progress;

        public ImageWithProgress(
            ImageProvider image,
            Func<float> progress,
            BoxFit? fit = null)
        {
            this.image = image;
            this.progress = progress;
            this.fit = fit;
        }

        public override State createState()
        {
            return new ImageWithProgressState();
        }
    }

    internal class ImageWithProgressState : SingleTickerProviderStateMixin<ImageWithProgress>
    {
        private Ticker m_Ticker;

        public override void initState()
        {
            base.initState();
            m_Ticker = this.createTicker(value => { setState(null); });
            m_Ticker.start();
        }

        public override void dispose()
        {
            m_Ticker.stop();
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            return new Stack(
                children: new List<Widget>
                {
                    new Image(
                        image: widget.image,
                        fit: widget.fit
                    ),
                    new ClipRRect(
                        borderRadius: BorderRadius.all(2),
                        child: new Container(
                            color: new Color(0xb2000000),
                            alignment: Alignment.center,
                            child: new Container(
                                height: 8,
                                margin: EdgeInsets.symmetric(horizontal: 32),
                                decoration: new BoxDecoration(
                                    color: new Color(0xb2000000)
                                ),
                                alignment: Alignment.centerLeft,
                                child: new Container(
                                    width: 218 * widget.progress(),
                                    color: new Color(0xff00cccc)
                                )
                            )
                        )
                    ),
                }
            );
        }
    }
}