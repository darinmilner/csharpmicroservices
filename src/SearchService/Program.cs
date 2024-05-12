using SearchService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();

app.MapControllers();

try{
    await DbInitializer.InitDb(app);
}
catch (Exception e) {
    Console.WriteLine(e);
}

app.Run();
