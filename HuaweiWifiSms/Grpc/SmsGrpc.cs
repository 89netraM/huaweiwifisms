using System.Threading.Tasks;
using Grpc.Core;
using HuaweiWifiSms.Services;

namespace HuaweiWifiSms.Grpc;

public class SmsGrpc(SmsService smsService) : SmsSender.SmsSenderBase
{
    public override async Task<SmsResponse> SendSms(SmsRequest request, ServerCallContext context)
    {
        if (!request.HasRecipientPhoneNumber || !request.HasContent)
        {
            throw new RpcException(
                new(
                    StatusCode.InvalidArgument,
                    $"Both {nameof(request.RecipientPhoneNumber)} and {nameof(request.Content)} are required."
                )
            );
        }

        var smsStatus = await smsService.SendSms(
            request.RecipientPhoneNumber,
            request.Content,
            context.CancellationToken
        );

        return new() { Status = smsStatus is Services.SmsStatus.Success ? SmsStatus.Success : SmsStatus.Failure };
    }
}
