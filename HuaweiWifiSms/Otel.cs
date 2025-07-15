using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace HuaweiWifiSms;

public static class Otel
{
    public const string SourceName = "HuaweiWifiSms";
    public const string SourceVersion = "1.0.0";

    internal static ActivitySource Source { get; } = new(SourceName, SourceVersion);

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);

    internal static Meter Meter { get; } = new(SourceName, SourceVersion);

    internal static SmsCount SmsCounter { get; } = Counters.CreateSmsCount(Meter);
}

internal static partial class Counters
{
    [Counter<int>(typeof(SmsCountTags))]
    public static partial SmsCount CreateSmsCount(Meter meter);
}

internal readonly record struct SmsCountTags(string PhoneNumber, SmsCountTags.StatusTag Status)
{
    public enum StatusTag
    {
        Success,
        Failure,
    }
}
