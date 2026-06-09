internal static class BoxStackRunStatusText
{
    internal const string Ready = "READY";
    internal const string Drop = "DROP";
    internal const string Verifying = "VERIFYING";
    internal const string StackComplete = "STACK COMPLETE";

    internal static string Run(int attempts)
    {
        return $"RUN {attempts}";
    }
}
