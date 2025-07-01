using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Windows.Forms;
using Screen = System.Windows.Forms.Screen;
using System.Windows.Controls;


namespace RemindMe
{
    public partial class MainWindow : Window
    {
        private WebApplication? _serverHost;

        private const double CollapsedWidth = 20;
        private const double ExpandedWidth = 300;
        private bool _isPinned;
        private bool _isDragging;
        private bool _dockedLeft = true;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Width = ExpandedWidth;
            Height = 400; // temporary until docked
            Topmost = true;
            Collapse();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _serverHost = CreateServer();
            await _serverHost.StartAsync();
            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.Navigate(_serverHost.Urls.First());
            SnapToEdge();
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

        private void DragHandle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            _isDragging = true;
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }

        private void DragHandle_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
            SnapToEdge();
        }

        private void RootGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Expand();
        }

        private void RootGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isPinned && !_isDragging)
                Collapse();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            PinButton.Content = _isPinned ? "📌" : "📍";
            if (_isPinned)
                Expand();
            else
                Collapse();
        }

        private void Expand()
        {
            Width = ExpandedWidth;
        }

        private void Collapse()
        {
            if (!_isPinned)
                Width = CollapsedWidth;
        }

        private void SnapToEdge()
        {
            var center = new System.Drawing.Point((int)(Left + Width / 2), (int)(Top + Height / 2));
            var screen = Screen.FromPoint(center);
            var wa = screen.WorkingArea;

            Height = wa.Height;
            Top = wa.Top;

            var distLeft = Math.Abs(Left - wa.Left);
            var distRight = Math.Abs((Left + Width) - wa.Right);

            if (distLeft < distRight)
            {
                Left = wa.Left;
                _dockedLeft = true;
            }
            else
            {
                Left = wa.Right - Width;
                _dockedLeft = false;
            }

            Grid.SetColumn(DragHandle, _dockedLeft ? 0 : 1);
            Grid.SetColumn(ContentGrid, _dockedLeft ? 1 : 0);
        }
    }
}
