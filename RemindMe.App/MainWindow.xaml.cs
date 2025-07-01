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
        private WebApplication _serverHost;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _serverHost = CreateServer();
            await _serverHost.StartAsync();
            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.Navigate(_serverHost.Urls.First());
        }

        private static WebApplication CreateServer()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddControllers();
            builder.Services.AddSingleton<TaskStore>();

            var app = builder.Build();

            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            return app;
        }
    }
}
