using ConsulTest;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddConsul(options => builder.Configuration.GetSection(ConsulOptions.Consul).Bind(options));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddConsul(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHealthChecks(Consts.HealthAddress);

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.Urls.Add(UriTool.BuildRunUri(app.Configuration));
    app.Urls.Add(UriTool.BuildRunLocalUri(app.Configuration));
}

app.Run();

