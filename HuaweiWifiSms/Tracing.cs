using System.Diagnostics;

namespace HuaweiWifiSms;

public static class Tracing
{
    public const string SourceName = "HuaweiWifiSms";
    public const string SourceVersion = "1.0.0";

    internal static ActivitySource Source { get; } = new(SourceName, SourceVersion);

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);
}
