using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Web.WebView2.Core;
using NasaAstronautTraining.ViewModels;

namespace NasaAstronautTraining.Views;

public partial class MissionControlView : UserControl
{
    private MissionControlViewModel? _subscribedVm;
    private bool _isWebViewInitialized;
    private string? _currentNavigatedUrl;

    private const string GameFullscreenScript = @"
(function() {
    const noopFs = function() { return Promise.resolve(); };
    if (Element.prototype) {
        Element.prototype.requestFullscreen = noopFs;
        Element.prototype.webkitRequestFullscreen = noopFs;
        Element.prototype.mozRequestFullScreen = noopFs;
        Element.prototype.msRequestFullscreen = noopFs;
    }

    if (!window.__doraEscBound) {
        window.__doraEscBound = true;
        window.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage('exit-game');
            }
        }, true);
    }

    function isolateAndAutoStart() {
        const styleId = 'dora-game-pure-style';
        if (!document.getElementById(styleId)) {
            const style = document.createElement('style');
            style.id = styleId;
            style.textContent = `
                html, body {
                    margin: 0 !important;
                    padding: 0 !important;
                    width: 100vw !important;
                    height: 100vh !important;
                    overflow: hidden !important;
                    background-color: #070D1E !important;
                }
                header, footer, nav, #header, #footer, #nav, #site-logo-holder,
                .header, .footer, .breadcrumbs, #breadcrumb, .menu-bar, #menu-wrapper,
                .articleFeatBigButtons, .review-section, .article-body, .related,
                .share-buttons, #utility-bar, .banner, #banner, #unity-footer,
                #unity-fullscreen-button, .fullscreen, [id*=""fullscreen""], [class*=""fullscreen""],
                #page-uri-holder, h1, h2, p, .social-share, [href*=""/menu/play""] {
                    display: none !important;
                    visibility: hidden !important;
                    opacity: 0 !important;
                    pointer-events: none !important;
                }
                #unity-container, #gameContainer, #canvas, canvas, .unity-desktop, .webgl-content {
                    position: fixed !important;
                    top: 0 !important;
                    left: 0 !important;
                    width: 100vw !important;
                    height: 100vh !important;
                    max-width: 100vw !important;
                    max-height: 100vh !important;
                    z-index: 99999999 !important;
                    margin: 0 !important;
                    padding: 0 !important;
                    border: none !important;
                    background: #070D1E !important;
                    visibility: visible !important;
                    opacity: 1 !important;
                    display: block !important;
                }
                #unity-container canvas, #gameContainer canvas {
                    width: 100vw !important;
                    height: 100vh !important;
                    position: absolute !important;
                    top: 0 !important;
                    left: 0 !important;
                }
            `;
            (document.head || document.documentElement).appendChild(style);
        }

        const startSelectors = [
            '#play-button', '.play-button', '#start-button', '.start-button',
            'button.play', '[id*=""start""]', '[id*=""play""]'
        ];
        for (const sel of startSelectors) {
            const btn = document.querySelector(sel);
            if (btn && btn.offsetParent !== null) {
                try { btn.click(); } catch(e) {}
            }
        }

        const canvas = document.querySelector('canvas') || document.getElementById('unity-canvas');
        if (canvas) {
            canvas.focus();
            try {
                canvas.dispatchEvent(new MouseEvent('click', { bubbles: true }));
            } catch(e) {}
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', isolateAndAutoStart);
    } else {
        isolateAndAutoStart();
    }
    window.addEventListener('load', isolateAndAutoStart);
    setInterval(isolateAndAutoStart, 400);
})();
";

    public MissionControlView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _subscribedVm?.IsPlayingGame == true)
        {
            _subscribedVm.StopGame();
            e.Handled = true;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(320))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        RootGrid.BeginAnimation(OpacityProperty, fadeIn);

        var win = Window.GetWindow(this);
        if (win != null)
        {
            win.PreviewKeyDown -= OnPreviewKeyDown;
            win.PreviewKeyDown += OnPreviewKeyDown;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_subscribedVm != null)
        {
            _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _subscribedVm = DataContext as MissionControlViewModel;
        if (_subscribedVm != null)
        {
            _subscribedVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscribedVm == null) return;

        if (e.PropertyName == nameof(MissionControlViewModel.IsPlayingGame) ||
            e.PropertyName == nameof(MissionControlViewModel.SelectedGame))
        {
            if (_subscribedVm.IsPlayingGame && _subscribedVm.SelectedGame != null)
            {
                NavigateWebView(_subscribedVm.SelectedGame.Url);
            }
            else if (!_subscribedVm.IsPlayingGame)
            {
                StopWebView();
            }
        }
    }

    private async Task InitializeWebViewAsync()
    {
        if (_isWebViewInitialized || GameWebView == null) return;
        try
        {
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DORA",
                "WebView2Profile"
            );
            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await GameWebView.EnsureCoreWebView2Async(environment);

            GameWebView.CoreWebView2.ContainsFullScreenElementChanged += async (_, _) =>
            {
                try
                {
                    if (GameWebView.CoreWebView2.ContainsFullScreenElement)
                    {
                        await GameWebView.CoreWebView2.ExecuteScriptAsync(
                            "if (document.exitFullscreen) { document.exitFullscreen(); } else if (document.webkitExitFullscreen) { document.webkitExitFullscreen(); }");
                    }
                }
                catch { }
            };

            GameWebView.CoreWebView2.WebMessageReceived += (_, args) =>
            {
                try
                {
                    if (args.TryGetWebMessageAsString() == "exit-game")
                    {
                        Dispatcher.Invoke(() => _subscribedVm?.StopGame());
                    }
                }
                catch { }
            };

            await GameWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(GameFullscreenScript);

            GameWebView.NavigationStarting += (_, args) =>
            {
                if (args.Uri.Contains("/menu/play") || args.Uri.Contains("/menu/"))
                {
                    args.Cancel = true;
                }
            };

            GameWebView.NavigationCompleted += async (_, _) =>
            {
                try
                {
                    await GameWebView.ExecuteScriptAsync(GameFullscreenScript);
                }
                catch { }
            };

            _isWebViewInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WebView2 init error: {ex.Message}");
        }
    }

    private async void NavigateWebView(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || GameWebView == null) return;
        if (_currentNavigatedUrl == url) return;
        _currentNavigatedUrl = url;

        if (!_isWebViewInitialized)
        {
            await InitializeWebViewAsync();
        }

        try
        {
            if (GameWebView.CoreWebView2 != null)
            {
                GameWebView.CoreWebView2.Navigate(url);
            }
            else
            {
                GameWebView.Source = new Uri(url);
            }
        }
        catch { }
    }

    private void StopWebView()
    {
        _currentNavigatedUrl = null;
        try
        {
            if (GameWebView?.CoreWebView2 != null)
            {
                GameWebView.CoreWebView2.Navigate("about:blank");
            }
        }
        catch { }
    }

    private void OnToggleFullscreen(object sender, RoutedEventArgs e)
    {
        var win = Window.GetWindow(this);
        if (win is null) return;

        if (win.WindowState == WindowState.Maximized && win.WindowStyle == WindowStyle.None)
        {
            win.WindowStyle = WindowStyle.SingleBorderWindow;
            win.WindowState = WindowState.Normal;
        }
        else
        {
            win.WindowStyle = WindowStyle.None;
            win.WindowState = WindowState.Maximized;
        }
    }
}
