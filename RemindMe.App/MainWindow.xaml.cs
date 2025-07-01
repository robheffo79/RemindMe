using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace RemindMe
{
    public partial class MainWindow : Window
    {
        private IHost? _serverHost;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _serverHost = CreateServer();
            await _serverHost.StartAsync();
            var addresses = _serverHost.Services.GetRequiredService<IWebHost>().ServerFeatures
                .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
            var address = addresses.Addresses.First();
            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.Navigate(address);
        }

        private static IHost CreateServer()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddControllers();
            builder.Services.AddSingleton<TaskStore>();

            var app = builder.Build();

            app.UseStaticFiles();
            app.MapGet("/{file}.woff2", async (string file, IWebHostEnvironment env) =>
            {
                var b64Path = Path.Combine(env.WebRootPath, $"{file}.woff2.b64");
                if (!File.Exists(b64Path))
                    return Results.NotFound();
                var base64 = await File.ReadAllTextAsync(b64Path);
                var bytes = Convert.FromBase64String(base64);
                return Results.File(bytes, "font/woff2");
            });
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            return app;
        }
    }
}
