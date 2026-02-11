var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8080"));

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
