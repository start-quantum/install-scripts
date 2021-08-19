namespace StartQuantum;

public static class Extensions
{
    public static Func<InstallStatus> And(this Func<InstallStatus> func, Func<InstallStatus> then) =>
        () => {
            var baseResult = func();
            if (baseResult != InstallStatus.Installed)
            {
                return baseResult;
            }
            else
            {
                return then();
            }
        };

    public static string[] SplitLines(this string str) =>
        str.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );
}
