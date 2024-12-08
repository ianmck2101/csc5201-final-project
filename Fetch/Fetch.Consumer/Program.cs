using Fetch.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConsumerDAL, ConsumerDAL>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
