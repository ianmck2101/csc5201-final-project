using Fetch.Api;
using Fetch.Api.Data;
using Fetch.Api.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services
builder.Services.AddTransient<IRequestService, RequestService>();
builder.Services.AddTransient<IRequestDAL, RequestDAL>();
builder.Services.AddSingleton<KafkaProducer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
