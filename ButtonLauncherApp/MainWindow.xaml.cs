using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ButtonLauncherApp.Models;
using ButtonLauncherApp.Services;
using ButtonLauncherApp.Views;

namespace ButtonLauncherApp;

public partial class MainWindow : Window
{
    private const int MaxLabelLength = 16;
    private readonly string _configPath;
    private readonly ILogService _logger;
    private readonly ConfigurationLoader _configurationLoader;
    private readonly ConfigurationSaver _configurationSaver;
    private ButtonActionExecutor? _actionExecutor;
    private FileSystemWatcher? _fileWatcher;
    private readonly DispatcherTimer _reloadThrottleTimer;
    private bool _reloadPending;

    public MainWindow()
    {
        InitializeComponent();
        _configPath = Path.Combine(AppContext.BaseDirectory, "buttons.json");
        _logger = new FileLogService();
        _configurationLoader = new ConfigurationLoader(_logger);
        _configurationSaver = new ConfigurationSaver(_logger);
        _reloadThrottleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _reloadThrottleTimer.Tick += ReloadThrottleTimerOnTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _actionExecutor = new ButtonActionExecutor(this, _logger);
        TryReloadConfiguration();
        SetupWatcher();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DisposeWatcher();
    }

    private void SetupWatcher()
    {
        DisposeWatcher();

        var directory = Path.GetDirectoryName(_configPath);
        var fileName = Path.GetFileName(_configPath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            return;
        }

        _fileWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };

        _fileWatcher.Changed += OnConfigurationFileChanged;
        _fileWatcher.Created += OnConfigurationFileChanged;
        _fileWatcher.Renamed += OnConfigurationFileChanged;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void DisposeWatcher()
    {
        if (_fileWatcher is null)
        {
            return;
        }

        _fileWatcher.EnableRaisingEvents = false;
        _fileWatcher.Changed -= OnConfigurationFileChanged;
        _fileWatcher.Created -= OnConfigurationFileChanged;
        _fileWatcher.Renamed -= OnConfigurationFileChanged;
        _fileWatcher.Dispose();
        _fileWatcher = null;
    }

    private void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        _reloadPending = true;
        _reloadThrottleTimer.Stop();
        _reloadThrottleTimer.Start();
    }

    private void ReloadThrottleTimerOnTick(object? sender, EventArgs e)
    {
        _reloadThrottleTimer.Stop();

        if (!_reloadPending)
        {
            return;
        }

        _reloadPending = false;
        Dispatcher.InvokeAsync(() => TryReloadConfiguration());
    }

    private void TryReloadConfiguration()
    {
        try
        {
            var configuration = _configurationLoader.Load(_configPath);
            ApplyConfiguration(configuration);
            StatusTextBlock.Text = $"Konfiguration geladen ({configuration.Meta.Version}) um {DateTime.Now:T}";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Konfiguration", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyConfiguration(ButtonLauncherConfiguration configuration)
    {
        ButtonGrid.Children.Clear();
        var columns = Math.Max(1, configuration.Meta?.GridColumns ?? 4);
        ButtonGrid.Columns = columns;

        var buttons = configuration.Buttons
            .Where(b => !string.IsNullOrWhiteSpace(b.Label))
            .ToList();

        if (buttons.Count == 0)
        {
            StatusTextBlock.Text = "Keine Buttons in der Konfiguration gefunden.";
            return;
        }

        foreach (var buttonConfig in buttons)
        {
            var button = new Button
            {
                Content = TruncateLabel(buttonConfig.Label),
                Tag = buttonConfig,
                Margin = new Thickness(4),
                Padding = new Thickness(12, 8, 12, 8),
                MinWidth = 120,
                MinHeight = 60,
                IsEnabled = buttonConfig.Enabled
            };

            button.Click += ActionButtonOnClick;
            ButtonGrid.Children.Add(button);
        }
    }

    private static string TruncateLabel(string label)
    {
        if (label.Length <= MaxLabelLength)
        {
            return label;
        }

        return label.Substring(0, MaxLabelLength);
    }

    private void ActionButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (_actionExecutor is null)
        {
            return;
        }

        if (sender is not Button button || button.Tag is not ButtonConfig config)
        {
            return;
        }

        _actionExecutor.Execute(config);
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        TryReloadConfiguration();
    }

    private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
    {
        var editor = new ButtonEditorWindow(_configPath, _configurationLoader, _configurationSaver, _logger)
        {
            Owner = this
        };

        var result = editor.ShowDialog();

        if (result == true)
        {
            TryReloadConfiguration();
        }
    }
}
