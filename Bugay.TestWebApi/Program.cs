using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights();
// builder.Logging.AddConsole();

var app = builder.Build();

// The name of the state store found in ./resources/statestore.yaml
const string DAPR_STORE_NAME = "statestore";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/neworder", async (Order order) =>
{
    app.Logger.LogDebug($"Incoming order Id: {order.Id}");
    var client = new DaprClientBuilder().Build();

    await client.SaveStateAsync(DAPR_STORE_NAME, order.Id.ToString(), order.ToString());
    app.Logger.LogDebug("Saving order to store: " + order);

    // Get the state from the state store
    var storeResult = await client.GetStateAsync<string>(DAPR_STORE_NAME, order.Id.ToString());
    app.Logger.LogDebug("Getting Order: " + storeResult);


    var result = new OrderResult { Id = order.Id };

    return Results.Ok(new { result = "success", value = result });
}).WithName("CreateNewOrder");

app.MapPost("/manyneworders", async () =>
{
    for (int i = 0; i < 1000; i++)
    {
        var order = new Order { Id = i };
        app.Logger.LogDebug($"Incoming order Id: {order.Id}");
        var client = new DaprClientBuilder().Build();

        await client.SaveStateAsync(DAPR_STORE_NAME, order.Id.ToString(), order.ToString());
        app.Logger.LogDebug("Saving order to store: " + order);

        // Get the state from the state store
        var storeResult = await client.GetStateAsync<string>(DAPR_STORE_NAME, order.Id.ToString());
        app.Logger.LogDebug("Getting Order: " + storeResult);

    }

    return Results.Ok(new { result = "success", value = "Completed" });
}).WithName("CreateManyNewOrders");

app.Run();

record Order()
{
    public int Id { get; set; }
    public string Item { get; set; } = "Default Item";
}

record OrderResult()
{
    public int Id { get; set; }
    public string IsSuccess { get; set; } = "Created a new Order Successfully!";
}
