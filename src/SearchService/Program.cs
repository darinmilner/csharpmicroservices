using Polly;
using Polly.Extensions.Http;
using SearchService;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddMassTransit(x => {
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) => {
        cfg.ConfigureEndpoints(context);
    });  
});

var app = builder.Build();

app.UseAuthentication();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () => {
  try{
    await DbInitializer.InitDb(app);
}
catch (Exception e) {
    Console.WriteLine(e);
}
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
  => HttpPolicyExtensions.HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
     .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(10));

