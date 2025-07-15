using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace HuaweiWifiSms.Services;

public class SmsService(ILogger<SmsService> logger, IOptions<SmsServiceOptions> options)
{
    private readonly Uri baseUrl = options.Value.BaseUrl;
    private readonly string username = options.Value.Username;
    private readonly string password = options.Value.Password;

    public async Task<SmsStatus> SendSms(string phoneNumber, string content, CancellationToken cancellationToken)
    {
        using var activity = Otel.StartActivity("SendSms");
        activity?.AddTag("phone-number", phoneNumber);

        try
        {
            logger.LogDebug("Creating playwright context");
            using var playwright = await Playwright.CreateAsync();
            logger.LogDebug("Launching chromium");
            await using var browser = await playwright.Chromium.LaunchAsync();
            logger.LogDebug("Opening new tab");
            var page = await browser.NewPageAsync();
            logger.LogDebug("Navigating to home page");
            await page.GotoAsync($"{baseUrl}html/home.html");

            logger.LogDebug("Clicking SMS tab");
            await page.ClickAsync("#sms");
            logger.LogDebug("Entering username");
            await page.FillAsync("#username", username);
            logger.LogDebug("Entering password");
            await page.FillAsync("#password", password);
            logger.LogDebug("Clicking login");
            await page.ClickAsync("#pop_login");

            logger.LogDebug("Clicking \"Create message\"");
            await page.ClickAsync("#message");
            logger.LogDebug("Entering phone number");
            await page.FillAsync("#recipients_number", phoneNumber);
            logger.LogDebug("Entering content");
            await page.FillAsync("#message_content", content);
            logger.LogDebug("Clicking send");
            await page.ClickAsync("#pop_send");

            logger.LogDebug("Waiting for result");
            await page.WaitForSelectorAsync("#sms_success_info");
            logger.LogDebug("Checking for success table");
            var success = await page.IsVisibleAsync(".send_success_info table");
            logger.LogDebug("Closing tab");
            await page.CloseAsync();
            logger.LogDebug("Returning");

            Otel.SmsCounter.Add(
                1,
                new(phoneNumber, success ? SmsCountTags.StatusTag.Success : SmsCountTags.StatusTag.Failure)
            );
            activity?.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);

            return success ? SmsStatus.Success : SmsStatus.Failure;
        }
        catch
        {
            Otel.SmsCounter.Add(1, new(phoneNumber, SmsCountTags.StatusTag.Failure));
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }
}

public enum SmsStatus
{
    Success,
    Failure,
}

public class SmsServiceOptions
{
    [Required]
    public required Uri BaseUrl { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

[OptionsValidator]
public partial class SmsServiceOptionsValidator : IValidateOptions<SmsServiceOptions>;

public static class SmsServiceServiceCollectionExtensions
{
    public static IServiceCollection AddSmsService(this IServiceCollection services)
    {
        services.AddOptions<SmsServiceOptions>().BindConfiguration("SmsService").ValidateOnStart();
        services.AddTransient<IValidateOptions<SmsServiceOptions>, SmsServiceOptionsValidator>();
        services.AddSingleton<SmsService>();
        return services;
    }
}
