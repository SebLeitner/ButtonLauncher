using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ButtonLauncherApp.Models;
using ButtonLauncherApp.Services;

namespace ButtonLauncherApp.Views;

public partial class ButtonEditorWindow : Window
{
    private readonly string _configPath;
    private readonly ConfigurationLoader _configurationLoader;
    private readonly ConfigurationSaver _configurationSaver;
    private readonly ILogService _logger;
    private ButtonLauncherConfiguration _configuration = new();

    public ObservableCollection<ButtonConfig> Buttons { get; } = new();

    public ButtonEditorWindow(
        string configPath,
        ConfigurationLoader configurationLoader,
        ConfigurationSaver configurationSaver,
        ILogService logger)
    {
        InitializeComponent();
        DataContext = this;
        _configPath = configPath;
        _configurationLoader = configurationLoader;
        _configurationSaver = configurationSaver;
        _logger = logger;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        try
        {
            _configuration = _configurationLoader.Load(_configPath);
            Buttons.Clear();

            foreach (var button in _configuration.Buttons)
            {
                Buttons.Add(CloneButton(button));
            }

            VersionTextBox.Text = _configuration.Meta.Version;
            GridColumnsTextBox.Text = _configuration.Meta.GridColumns.ToString();
            EditorStatusTextBlock.Text = $"Konfiguration geladen ({_configuration.Meta.Version})";
        }
        catch (Exception ex)
        {
            EditorStatusTextBlock.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Button-Editor", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        LoadConfiguration();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        Buttons.Add(new ButtonConfig
        {
            Id = Guid.NewGuid().ToString("N"),
            Label = "Neuer Button",
            ActionType = "open_explorer",
            Target = string.Empty,
            Confirm = "none",
            RunAsAdmin = false,
            Enabled = true
        });
        ButtonDataGrid.SelectedIndex = Buttons.Count - 1;
        ButtonDataGrid.ScrollIntoView(ButtonDataGrid.SelectedItem);
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ButtonDataGrid.SelectedItem is not ButtonConfig selected)
        {
            return;
        }

        Buttons.Remove(selected);
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(GridColumnsTextBox.Text, out var gridColumns) || gridColumns <= 0)
        {
            MessageBox.Show(this, "Bitte eine gültige Spaltenanzahl eingeben.", "Button-Editor", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _configuration.Meta.Version = VersionTextBox.Text;
        _configuration.Meta.GridColumns = gridColumns;
        _configuration.Buttons = Buttons.Select(CloneButton).ToList();

        try
        {
            _configurationSaver.Save(_configPath, _configuration);
            EditorStatusTextBlock.Text = $"Konfiguration gespeichert ({DateTime.Now:T})";
        }
        catch (Exception ex)
        {
            EditorStatusTextBlock.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Button-Editor", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static ButtonConfig CloneButton(ButtonConfig button)
    {
        return new ButtonConfig
        {
            Id = button.Id,
            Label = button.Label,
            ActionType = button.ActionType,
            Target = button.Target,
            Confirm = button.Confirm,
            RunAsAdmin = button.RunAsAdmin,
            Enabled = button.Enabled
        };
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
