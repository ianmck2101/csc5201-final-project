using Fetch.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection
builder.Services.AddSingleton<IConsumerDAL, ConsumerDAL>();
builder.Services.AddSingleton<MessageConsumer>();
builder.Services.AddHostedService<KafkaConsumerHostedService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
