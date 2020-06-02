using Unity.UIWidgets.ui;
using UnityEngine;
using Unity.UIWidgets.widgets;

namespace Unity.Messenger.Style
{
    public class IconFont
    {
        public static void Load()
        {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/IconFont"), "IconFont");
        }
        
        
        public static readonly IconData IconFontArrowBack = new IconData(0xf10e, fontFamily: "IconFont");
        
        public static readonly IconData IconFontArrowLeft = new IconData(0xf10b, fontFamily: "IconFont");
        
        public static readonly IconData IconFontArrowUp = new IconData(0xf10d, fontFamily: "IconFont");
        
        public static readonly IconData IconFontBell = new IconData(0xf10a, fontFamily: "IconFont");
        
        public static readonly IconData IconFontChevronRight = new IconData(0xf109, fontFamily: "IconFont");
        
        public static readonly IconData IconFontClose = new IconData(0xf100, fontFamily: "IconFont");
        
        public static readonly IconData IconFontDelete = new IconData(0xf10c, fontFamily: "IconFont");
        
        public static readonly IconData IconFontError = new IconData(0xf101, fontFamily: "IconFont");
        
        public static readonly IconData IconFontJoin = new IconData(0xf108, fontFamily: "IconFont");
        
        public static readonly IconData IconFontLoading = new IconData(0xf107, fontFamily: "IconFont");
        
        public static readonly IconData IconFontOpenEye = new IconData(0xf102, fontFamily: "IconFont");
        
        public static readonly IconData IconFontRight = new IconData(0xf106, fontFamily: "IconFont");
        
        public static readonly IconData IconFontSendPic = new IconData(0xf103, fontFamily: "IconFont");
        
        public static readonly IconData IconFontSettings = new IconData(0xf104, fontFamily: "IconFont");
        
        public static readonly IconData IconFontSettingsHoriz = new IconData(0xf10f, fontFamily: "IconFont");
        
        public static readonly IconData IconFontShare = new IconData(0xf110, fontFamily: "IconFont");
        
        public static readonly IconData IconFontUser = new IconData(0xf105, fontFamily: "IconFont");
        
    }
}