using API.CMS.Models;
using API.CMS.Services;

var builder = WebApplication.CreateBuilder(args);

//Bind MongoDbSettings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

//Register StudentMongoService and CourseMongoService as a Singleton
builder.Services.AddSingleton<StudentMongoService>();
builder.Services.AddSingleton<CourseMongoService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();