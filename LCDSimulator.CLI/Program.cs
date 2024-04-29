namespace LCDSimulator.CLI
{
    internal static class Program
    {
        private static void Main()
        {
            DisplayController controller = new()
            {
                IsPowered = true
            };
            new CommandLine(new DisplayInterface(controller)).StartCLI();
        }
    }
}
