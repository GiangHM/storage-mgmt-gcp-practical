using Google.Api.Gax;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using storageapi.Extensions;
using storageapi.Infra.gcp;
using storagedal.Infra.efcore;
using StorageManagementAPI;
using StorageManagementAPI.Services;
using System;


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

// Configure OpenTelemetry
var otel = builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        //metrics.AddMeter("Microsoft.AspNetCore.Hosting");
        //metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
    })
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                var stateDisplayName = $"{command.CommandType} operation";
                activity.DisplayName = stateDisplayName;
                activity.SetTag("db.name", stateDisplayName);
            };
        }));

var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (OtlpEndpoint != null)
{
    otel.UseOtlpExporter();
}
// ── Firestore ────────────────────────────────────────────────────────────────
// FirestoreDb is thread-safe and intended to be reused; register as singleton.
// Application Default Credentials (ADC) are used automatically.
// In local development set FIRESTORE_EMULATOR_HOST=localhost:8080 to use the emulator.
builder.Services.AddGcpConfiguration();

var gcpProjectId = builder.Configuration["GCP:ProjectId"]
    ?? throw new InvalidOperationException(
        "GCP:ProjectId is required. Add it to appsettings.json or set the GCP__ProjectId environment variable.");

// builder.Services.AddSingleton(FirestoreDb.Create(gcpProjectId));
builder.Services.AddSingleton(new FirestoreDbBuilder{
    ProjectId = gcpProjectId,
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction
}.Build());
builder.Services.AddScoped<IDocTypeService, DocTypeFirestoreService>();

// ── Pub/Sub ──────────────────────────────────────────────────────────────────
// Registers PubSubPublisher as singleton + IHostedService (StartAsync / StopAsync).
// In local development set PUBSUB_EMULATOR_HOST=localhost:8085 to use the emulator.
builder.Services.AddPubSubPublisher();
builder.Services.AddSingleton<ICloudStorageService, CloudStorageService>();
builder.Services.AddPostgresDatabaseServer(options => builder.Configuration.GetSection("Storage:PostgresDb").Bind(options));

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile(new AutoMapperProfile());
});

var frontUrl = builder.Configuration.GetValue<string>("FrontUrl");
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(frontUrl!)
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI support
builder.Services.AddOpenApi();

var app = builder.Build();

// Log resolved GCP configuration (no secrets — credentials come from GOOGLE_APPLICATION_CREDENTIALS).
app.Services.LogGcpConfiguration();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.MapScalarApiReference();
//}

// TLS is terminated by Cloud Run's load balancer; the container receives plain HTTP on port 8080.
// UseHttpsRedirection is intentionally omitted — it would redirect CORS preflight requests
// before the CORS middleware runs, causing "No Access-Control-Allow-Origin" errors in production.

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
