namespace LCDSimulator
{
    public class CursorBlink : IDisposable
    {
        public const double BlinkIntervalMilliseconds = 409.6;

        public bool Blink { get; private set; }

        private readonly Timer blinkTimer;

        public CursorBlink()
        {
            blinkTimer = new Timer(ToggleBlink, null, TimeSpan.Zero,
                TimeSpan.FromMilliseconds(BlinkIntervalMilliseconds));
        }

        ~CursorBlink()
        {
            Dispose();
        }

        public void Dispose()
        {
            blinkTimer.Dispose();

            GC.SuppressFinalize(this);
        }

        private void ToggleBlink(object? state)
        {
            Blink = !Blink;
        }
    }
}
