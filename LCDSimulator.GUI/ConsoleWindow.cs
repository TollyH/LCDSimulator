using System.Runtime.InteropServices;

namespace LCDSimulator.GUI
{
    public static class ConsoleWindow
    {
        public enum Visibility
        {
            Hidden = SW_HIDE,
            Visible = SW_SHOW
        }

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("Kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Create()
        {
            _ = AllocConsole();
        }

        public static void SetVisibility(Visibility visibility)
        {
            _ = ShowWindow(GetConsoleWindow(), (int)visibility);
        }
    }
}
