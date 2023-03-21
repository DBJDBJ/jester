
/* 

2023MAR21   dbj@dbj.org     Meandering arround .NET Core 
******************************************************************************************

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0

The ASPNETCORE_URLS environment variable is available to set the port:

ASPNETCORE_URLS=http://localhost:3000

ASPNETCORE_URLS supports multiple URLs:

ASPNETCORE_URLS=http://localhost:3000;https://localhost:5000

IMPORTANT! this is not mandatory! 
if you seen container run using env vars that does not mean that is mandatory!
that is not safe as anybody can see your run parameters and bause them

you can always use you special not known env var:

var port = Environment.GetEnvironmentVariable("MY_SPECIAL_PORT_ENV_VAR") ?? "3000";
app.Run($"http://localhost:{port}");

or simply read from the configuration hidden inside a container ;)

For HTTPS, I recommend not using config or envvar or dev cert, instead use Certificate APIs
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#use-the-certificate-apis

Are we in dev mode: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#read-the-environment

*/

using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environments.Staging,
    WebRootPath = "jesterwwwroot"
    // this is waste of time: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#change-the-content-root-app-name-and-environment-by-environment-variables-or-command-line
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-6.0#directory-browsing
builder.Services.AddDirectoryBrowser();


// BIG WARNING: do not waste your life on loggin libraries
// always write code to run in a container
// where this is the only logging required
// Console.WriteLine($"Application Name: {builder.Environment.ApplicationName}");
// Console.WriteLine($"Environment Name: {builder.Environment.EnvironmentName}");
// Console.WriteLine($"ContentRoot Path: {builder.Environment.ContentRootPath}");
// Console.WriteLine($"WebRootPath: {builder.Environment.WebRootPath}");

// json config is overengineering, use ini
var config_file_name = "jester.ini";
builder.Configuration.AddIniFile(config_file_name);
var jester_config_name = builder.Configuration["name"] ?? $"Config file {config_file_name}, not found";

///////////////////////////////////////////////////////////////////////////////////////////////
var app = builder.Build();
///////////////////////////////////////////////////////////////////////////////////////////////
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-6.0#serve-default-documents
app.UseDefaultFiles();
app.UseStaticFiles();
///////////////////////////////////////////////////////////////////////////////////////////////
var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "media"));
var requestPath = "/media";

// Enable displaying browser links.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});
///////////////////////////////////////////////////////////////////////////////////////////////
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapRazorPages();

StringBuilder banner_ = new StringBuilder();

banner_.AppendFormat("\n\nApplication Name: '{0}'\n Environment Name '{1}'\n Content Root Path: '{2}'\n Web Root Path: '{3}'\n",
        builder.Environment.ApplicationName,
        builder.Environment.EnvironmentName,
        builder.Environment.ContentRootPath,
        builder.Environment.WebRootPath
);

banner_.AppendFormat("\n\nconfg file {1}, name = {0}", jester_config_name, config_file_name);
// se the BIG WARNING above
app.Logger.LogInformation(banner_.ToString());
app.Logger.LogInformation("\n\nThe app started");
// this will overrired the automagic <web root path>/index.html
// app.MapGet("/", () => banner_.ToString());

// no config, use code
// app.Urls.Add("http://localhost:3000");
// app.Urls.Add("http://localhost:4000");

// or for a single port
app.Run("http://localhost:3000");


