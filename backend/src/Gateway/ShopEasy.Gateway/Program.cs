const string DevCorsPolicy = "DevCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Permissive by design: this gateway only ever runs on localhost or inside the Docker network
// for this demo, and the web build of the mobile app (Expo for web) needs to call it directly
// from a browser origin. Never carry this policy into a real deployment.
builder.Services.AddCors(options => options.AddPolicy(
    DevCorsPolicy,
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors(DevCorsPolicy);
app.MapHealthChecks("/health");
app.MapReverseProxy();

await app.RunAsync();
