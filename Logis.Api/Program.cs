using Logis.Api.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiControllers(builder.Configuration);
builder.Services.AddApiOptions(builder.Configuration);
builder.Services.AddApiDependences(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiAuthorization(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    //options.SingleLine = true;
    //options.IncludeScopes = true;
    options.TimestampFormat = "HH:mm:ss ";
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseApiPipeline();

app.Run();
