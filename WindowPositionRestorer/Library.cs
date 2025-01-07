using System;

namespace WindowPositionRestorer
{
    /// <summary>Origin is the top left corner of the main monitor. If there's another monitor to the left, windows in it will have negative coordinates.</summary>
    public struct Rect
    {
        /// <summary>Distance between origin and left border.</summary>
        public int Left { get; set; }

        /// <summary>Distance between origin and right border.</summary>
        public int Top { get; set; }

        /// <summary>Distance between origin and top border.</summary>
        public int Right { get; set; }

        /// <summary>Distance between origin and bottom border.</summary>
        public int Bottom { get; set; }

        public override string ToString()
        {
            return $"{{ \"Left\": {Left}, \"Top\": {Top}, \"Right\": {Right}, \"Bottom\": {Bottom} }}";
        }
    }

    public struct WindowsWindow
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public IntPtr Handle { get; set; }

        public WindowsWindow(IntPtr handle, Rect rect)
        {
            Handle = handle;
            Left = rect.Left;
            Top = rect.Top;
            Right = rect.Right;
            Bottom = rect.Bottom;
        }
    }
}
