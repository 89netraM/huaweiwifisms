using HuaweiWifiSms;
using HuaweiWifiSms.Grpc;
using HuaweiWifiSms.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOpenTelemetry();

builder.Services.AddSmsService();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<SmsGrpc>();

app.Run();
