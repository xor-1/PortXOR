using System.Runtime.InteropServices;

namespace PortXOR.Utils;

/// <summary>
/// Cross-platform colored console output utility
/// </summary>
public static class ColorConsole
{
    private static readonly bool _supportsColors;
    private static readonly bool _isWindows;

    static ColorConsole()
    {
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _supportsColors = Environment.GetEnvironmentVariable("NO_COLOR") == null;

        // Enable virtual terminal processing on Windows 10+ for better color support
        if (_isWindows && _supportsColors)
        {
            try
            {
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                GetConsoleMode(handle, out uint mode);
                SetConsoleMode(handle, mode | 0x0004); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
            }
            catch
            {
                // Fallback to basic colors if virtual terminal processing fails
            }
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    // ANSI color codes
    private const string Reset = "\x1b[0m";
    private const string Bold = "\x1b[1m";
    private const string Dim = "\x1b[2m";
    
    // Colors
    private const string Black = "\x1b[30m";
    private const string Red = "\x1b[31m";
    private const string Green = "\x1b[32m";
    private const string Yellow = "\x1b[33m";
    private const string Blue = "\x1b[34m";
    private const string Magenta = "\x1b[35m";
    private const string Cyan = "\x1b[36m";
    private const string White = "\x1b[37m";
    
    // Bright colors
    private const string BrightRed = "\x1b[91m";
    private const string BrightGreen = "\x1b[92m";
    private const string BrightYellow = "\x1b[93m";
    private const string BrightBlue = "\x1b[94m";
    private const string BrightMagenta = "\x1b[95m";
    private const string BrightCyan = "\x1b[96m";

    private static string Colorize(string text, string colorCode)
    {
        return _supportsColors ? $"{colorCode}{text}{Reset}" : text;
    }

    public static void WriteSuccess(string text) => Console.Write(Colorize(text, BrightGreen));
    public static void WriteError(string text) => Console.Write(Colorize(text, BrightRed));
    public static void WriteWarning(string text) => Console.Write(Colorize(text, BrightYellow));
    public static void WriteInfo(string text) => Console.Write(Colorize(text, BrightCyan));
    public static void WriteBanner(string text) => Console.Write(Colorize(text, BrightMagenta));
    public static void WritePort(string text) => Console.Write(Colorize(text, BrightBlue));
    public static void WriteDim(string text) => Console.Write(Colorize(text, Dim));

    public static void WriteLineSuccess(string text) => Console.WriteLine(Colorize(text, BrightGreen));
    public static void WriteLineError(string text) => Console.WriteLine(Colorize(text, BrightRed));
    public static void WriteLineWarning(string text) => Console.WriteLine(Colorize(text, BrightYellow));
    public static void WriteLineInfo(string text) => Console.WriteLine(Colorize(text, BrightCyan));
    public static void WriteLineBanner(string text) => Console.WriteLine(Colorize(text, BrightMagenta));
    public static void WriteLinePort(string text) => Console.WriteLine(Colorize(text, BrightBlue));
    public static void WriteLineDim(string text) => Console.WriteLine(Colorize(text, Dim));

    public static void WriteHeader()
    {
        if (_supportsColors)
        {
            Console.WriteLine(Colorize(@"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   ██████╗  ██████╗ ██████╗ ████████╗██╗  ██╗ ██████╗ ██████╗ 
║   ██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝╚██╗██╔╝██╔═══██╗██╔══██╗
║   ██████╔╝██║   ██║██████╔╝   ██║    ╚███╔╝ ██║   ██║██████╔╝
║   ██╔═══╝ ██║   ██║██╔══██╗   ██║    ██╔██╗ ██║   ██║██╔══██╗
║   ██║     ╚██████╔╝██║  ██║   ██║   ██╔╝ ██╗╚██████╔╝██║  ██║
║   ╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝
║                                                           ║
║          Lightweight Port Scanner v1.0.0                  ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝", BrightCyan));
        }
        else
        {
            Console.WriteLine("PortXOR - Lightweight Port Scanner v1.0.0");
            Console.WriteLine("==========================================\n");
        }
    }
}

