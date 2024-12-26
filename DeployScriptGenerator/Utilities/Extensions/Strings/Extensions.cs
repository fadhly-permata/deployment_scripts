namespace DeployScriptGenerator.Utilities.Extensions.Strings;

internal static class Extensions
{
    internal static string Format(this string str, params object[] args) =>
        string.Format(str, args);

    internal static void WriteLine(this string str, params object[] args) =>
        Console.WriteLine(str.Format(args));
}
