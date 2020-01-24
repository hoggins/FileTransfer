using InteractiveConsole;

namespace FtApp
{
  class FtConsole : ConsoleManager
  {
    public static readonly FtConsole Instance = new FtConsole();
    
    public static RawConsoleCommand AddCommand(string name, string description = null)
    {
      return Instance.AddCommand(new RawConsoleCommand(name, description));
    }
    
    public new static void WriteLine()
    {
      ((ConsoleCore) Instance).WriteLine();
    }

    public new static void WriteLine(string value)
    {
      ((ConsoleCore) Instance).WriteLine(value);
    }

    public new static void WriteLine(string format, params object[] args)
    {
      ((ConsoleCore) Instance).WriteLine(format, args);
    }
  }
}