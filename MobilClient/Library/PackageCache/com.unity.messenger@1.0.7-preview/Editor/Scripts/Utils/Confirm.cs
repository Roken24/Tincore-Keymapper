using System.Collections.Generic;
using Unity.Messenger.Style;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger
{
    public partial class Utils
    {
        public static void Confirm(
            BuildContext buildContext,
            string title,
            string msg,
            System.Action onConfirmed,
            System.Action onCanceled
        )
        {
            var focusNode = new FocusNode();
            FocusScope.of(buildContext).requestFocus(focusNode);
            OverlayEntry entry = null;
            entry = new OverlayEntry(
                context =>
                {
                    return new Positioned(
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 0,
                        child: new GestureDetector(
                            onTap: () =>
                            {
                                entry.remove();
                                onCanceled();
                            },
                            child: new Container(
                                color: new Color(0x00000000),
                                alignment: Alignment.center,
                                child: new GestureDetector(
                                    onTap: () => { },
                                    child: new Container(
                                        width: 327,
                                        height: 201,
                                        decoration: new BoxDecoration(
                                            borderRadius: BorderRadius.all(6),
                                            color: new Color(0xffffffff),
                                            boxShadow: new List<BoxShadow>
                                            {
                                                new BoxShadow(
                                                    blurRadius: 16,
                                                    color: new Color(0x33000000)
                                                )
                                            }
                                        ),
                                        child: new Column(
                                            children: new List<Widget>
                                            {
                                                new Container(
                                                    height: 56,
                                                    padding: EdgeInsets.symmetric(horizontal: 24),
                                                    child: new Row(
                                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                        children: new List<Widget>
                                                        {
                                                            new Text(
                                                                title,
                                                                style: new TextStyle(
                                                                    fontSize: 16,
                                                                    color: new Color(0xff212121),
                                                                    fontFamily: "PingFang"
                                                                )
                                                            ),
                                                            new GestureDetector(
                                                                onTap: () =>
                                                                {
                                                                    entry.remove();
                                                                    onCanceled();
                                                                },
                                                                child: new Icon(
                                                                    IconFont.IconFontClose,
                                                                    size: 24,
                                                                    color: new Color(0xff979a9e)
                                                                )
                                                            )
                                                        }
                                                    )
                                                ),
                                                new Container(
                                                    height: 1,
                                                    color: new Color(0xfff0f0f0)
                                                ),
                                                new Expanded(
                                                    child: new Container(
                                                        padding: EdgeInsets.only(
                                                            top: 24,
                                                            left: 24,
                                                            right: 24,
                                                            bottom: 16),
                                                        child: new Column(
                                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                            children: new List<Widget>
                                                            {
                                                                new Row(
                                                                    children: new List<Widget>
                                                                    {
                                                                        new Container(
                                                                            width: 32,
                                                                            height: 32,
                                                                            margin: EdgeInsets.only(right: 8),
                                                                            decoration: new BoxDecoration(
                                                                                borderRadius: BorderRadius.all(16),
                                                                                color: new Color(0x992196f3)
                                                                            ),
                                                                            child: new Icon(
                                                                                IconFont.IconFontDelete,
                                                                                size: 32,
                                                                                color: new Color(0x992196f3)
                                                                            )
                                                                        ),
                                                                        new Text(
                                                                            msg,
                                                                            style: new TextStyle(
                                                                                fontSize: 14,
                                                                                color: new Color(0xff797979),
                                                                                fontFamily: "PingFang"
                                                                            )
                                                                        )
                                                                    }
                                                                ),
                                                                new Row(
                                                                    mainAxisAlignment: MainAxisAlignment.end,
                                                                    children: new List<Widget>
                                                                    {
                                                                        new GestureDetector(
                                                                            onTap: () =>
                                                                            {
                                                                                entry.remove();
                                                                                onCanceled();
                                                                            },
                                                                            child: new Container(
                                                                                width: 72,
                                                                                height: 40,
                                                                                decoration: new BoxDecoration(
                                                                                    borderRadius: BorderRadius.all(3),
                                                                                    border: Border.all(
                                                                                        color: new Color(0xffd8d8d8)
                                                                                    ),
                                                                                    color: new Color(0xffffffff)
                                                                                ),
                                                                                alignment: Alignment.center,
                                                                                child: new Text(
                                                                                    "取消"
                                                                                ),
                                                                                margin: EdgeInsets.only(right: 16)
                                                                            )
                                                                        ),
                                                                        new GestureDetector(
                                                                            onTap: () =>
                                                                            {
                                                                                entry.remove();
                                                                                onConfirmed();
                                                                            },
                                                                            child: new Container(
                                                                                width: 72,
                                                                                height: 40,
                                                                                decoration: new BoxDecoration(
                                                                                    borderRadius: BorderRadius.all(3),
                                                                                    color: new Color(0xff2196f3)
                                                                                ),
                                                                                alignment: Alignment.center,
                                                                                child: new Text(
                                                                                    "确认",
                                                                                    style: new TextStyle(
                                                                                        fontSize: 14,
                                                                                        color: new Color(0xffffffff),
                                                                                        fontFamily: "PingFang"
                                                                                    )
                                                                                )
                                                                            )
                                                                        )
                                                                    }
                                                                )
                                                            }
                                                        )
                                                    )
                                                )
                                            }
                                        )
                                    )
                                )
                            )
                        )
                    );
                }
            );
            Overlay.of(buildContext).insert(entry);
        }
    }
}