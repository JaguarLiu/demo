using Microsoft.AspNetCore.StaticFiles;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);
// </snippet_UseWebSockets>

app.UseDefaultFiles();
var provider = new FileExtensionContentTypeProvider();
// Add new mappings
provider.Mappings[".data"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.MapControllers();

app.Run();
