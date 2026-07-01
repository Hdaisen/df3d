using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DF3D.Models;
using DF3D.Services;

namespace DF3D.Windows;

/// <summary>
/// Code-behind for the settings window.
/// Handles UI initialization, event wiring, and bidirectional data flow.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly ProfileService _profileService;
    private bool _isLoading;

    public SettingsWindow(SettingsService settingsService, ProfileService profileService)
    {
        _settingsService = settingsService;
        _profileService = profileService;

        InitializeComponent();
        LoadEnumComboBoxes();
        LoadSettingsToUI();
        UpdateHotkeyButtons();
        LoadProfiles();

        // Restore window position
        if (_settingsService.Settings.SettingsWindowLeft >= 0)
        {
            Left = _settingsService.Settings.SettingsWindowLeft;
            Top = _settingsService.Settings.SettingsWindowTop;
        }
    }

    #region Initialization

    private void LoadEnumComboBoxes()
    {
        // Overlay enums
        CmbOverlayShape.ItemsSource = Enum.GetValues<VignetteShape>();
        CmbOverlaySize.ItemsSource = Enum.GetValues<VignetteSize>();
        CmbOverlayLength.ItemsSource = Enum.GetValues<VignetteLength>();
        CmbOverlayColor.ItemsSource = Enum.GetValues<OverlayColor>();
        CmbOverlayAspect.ItemsSource = Enum.GetValues<AspectRatio>();
        CmbOverlayDisplayMode.ItemsSource = Enum.GetValues<DisplayMode>();
        CmbOverlaySplit.ItemsSource = Enum.GetValues<SplitScreenMode>();

        // Crosshair enums
        CmbCrossShape.ItemsSource = Enum.GetValues<CrosshairShape>();
        CmbCrossSize.ItemsSource = Enum.GetValues<CrosshairSize>();
        CmbCrossColor.ItemsSource = Enum.GetValues<OverlayColor>();
    }

    private void LoadSettingsToUI()
    {
        _isLoading = true;
        try
        {
            var s = _settingsService.Settings;

            // Overlay
            ChkOverlayVisible.IsChecked = s.Overlay.IsVisible;
            CmbOverlayShape.SelectedItem = s.Overlay.Shape;
            CmbOverlaySize.SelectedItem = s.Overlay.Size;
            CmbOverlayLength.SelectedItem = s.Overlay.Length;
            CmbOverlayColor.SelectedItem = s.Overlay.Color;
            TxtOverlayCustomColor.Text = s.Overlay.CustomColorHex;
            PanelOverlayCustomColor.Visibility = s.Overlay.Color == OverlayColor.Custom
                ? Visibility.Visible : Visibility.Collapsed;
            SliderOverlayOpacity.Value = s.Overlay.Opacity;
            TxtOverlayOpacity.Text = $"{(int)(s.Overlay.Opacity * 100)}%";
            CmbOverlayAspect.SelectedItem = s.Overlay.AspectRatio;
            CmbOverlayDisplayMode.SelectedItem = s.Overlay.DisplayMode;
            CmbOverlaySplit.SelectedItem = s.Overlay.SplitScreen;

            // Crosshair
            ChkCrosshairVisible.IsChecked = s.Crosshair.IsVisible;
            CmbCrossShape.SelectedItem = s.Crosshair.Shape;
            CmbCrossSize.SelectedItem = s.Crosshair.Size;
            SliderCrossThickness.Value = s.Crosshair.Thickness;
            TxtCrossThickness.Text = s.Crosshair.Thickness.ToString();
            CmbCrossColor.SelectedItem = s.Crosshair.Color;
            TxtCrossCustomColor.Text = s.Crosshair.CustomColorHex;
            PanelCrossCustomColor.Visibility = s.Crosshair.Color == OverlayColor.Custom
                ? Visibility.Visible : Visibility.Collapsed;
            SliderCrossOpacity.Value = s.Crosshair.Opacity;
            TxtCrossOpacity.Text = $"{(int)(s.Crosshair.Opacity * 100)}%";
            SliderCrossX.Value = s.Crosshair.OffsetX;
            TxtCrossX.Text = ((int)s.Crosshair.OffsetX).ToString();
            SliderCrossY.Value = s.Crosshair.OffsetY;
            TxtCrossY.Text = ((int)s.Crosshair.OffsetY).ToString();

            // General
            ChkStartup.IsChecked = StartupService.IsStartupEnabled();
            ChkShowOverlayOnStart.IsChecked = s.ShowOverlayOnStartup;
            ChkShowCrosshairOnStart.IsChecked = s.ShowCrosshairOnStartup;
            ChkMinimizeToTray.IsChecked = s.MinimizeToTray;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateHotkeyButtons()
    {
        var h = _settingsService.Settings.Hotkeys;
        BtnHotkeyOverlay.Content = h.ToggleOverlay.DisplayString;
        BtnHotkeyCrosshair.Content = h.ToggleCrosshair.DisplayString;
        BtnHotkeySplit.Content = h.ToggleSplitScreen.DisplayString;
        BtnHotkeyDisplay.Content = h.ToggleDisplayMode.DisplayString;
        BtnHotkeySize.Content = h.CycleOverlaySize.DisplayString;
        BtnHotkeyProfile.Content = h.CycleProfile.DisplayString;
    }

    private void LoadProfiles()
    {
        CmbProfiles.Items.Clear();
        foreach (var p in _profileService.Profiles)
            CmbProfiles.Items.Add(p.Name);

        CmbProfiles.SelectedItem = _profileService.ActiveProfile.Name;
    }

    #endregion

    #region Overlay Events

    private void OnOverlaySettingChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var s = _settingsService.Settings.Overlay;

        s.IsVisible = ChkOverlayVisible.IsChecked ?? true;
        if (CmbOverlayShape.SelectedItem is VignetteShape shape) s.Shape = shape;
        if (CmbOverlaySize.SelectedItem is VignetteSize size) s.Size = size;
        if (CmbOverlayLength.SelectedItem is VignetteLength len) s.Length = len;
        if (CmbOverlayColor.SelectedItem is OverlayColor color)
        {
            s.Color = color;
            PanelOverlayCustomColor.Visibility = color == OverlayColor.Custom
                ? Visibility.Visible : Visibility.Collapsed;
        }
        if (CmbOverlayAspect.SelectedItem is AspectRatio ar) s.AspectRatio = ar;
        if (CmbOverlayDisplayMode.SelectedItem is DisplayMode dm) s.DisplayMode = dm;
        if (CmbOverlaySplit.SelectedItem is SplitScreenMode split) s.SplitScreen = split;

        s.CustomColorHex = TxtOverlayCustomColor.Text;
        _settingsService.Save();
        UpdateStatus("叠加层设置已更新");
    }

    private void OnOverlayOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading) return;
        _settingsService.Settings.Overlay.Opacity = SliderOverlayOpacity.Value;
        TxtOverlayOpacity.Text = $"{(int)(SliderOverlayOpacity.Value * 100)}%";
        _settingsService.Save();
    }

    #endregion

    #region Crosshair Events

    private void OnCrosshairSettingChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var s = _settingsService.Settings.Crosshair;

        s.IsVisible = ChkCrosshairVisible.IsChecked ?? true;
        if (CmbCrossShape.SelectedItem is CrosshairShape shape) s.Shape = shape;
        if (CmbCrossSize.SelectedItem is CrosshairSize size) s.Size = size;
        if (CmbCrossColor.SelectedItem is OverlayColor color)
        {
            s.Color = color;
            PanelCrossCustomColor.Visibility = color == OverlayColor.Custom
                ? Visibility.Visible : Visibility.Collapsed;
        }
        s.CustomColorHex = TxtCrossCustomColor.Text;
        _settingsService.Save();
        UpdateStatus("十字线设置已更新");
    }

    private void OnCrosshairThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading) return;
        _settingsService.Settings.Crosshair.Thickness = (int)SliderCrossThickness.Value;
        TxtCrossThickness.Text = ((int)SliderCrossThickness.Value).ToString();
        _settingsService.Save();
    }

    private void OnCrosshairOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading) return;
        _settingsService.Settings.Crosshair.Opacity = SliderCrossOpacity.Value;
        TxtCrossOpacity.Text = $"{(int)(SliderCrossOpacity.Value * 100)}%";
        _settingsService.Save();
    }

    private void OnCrosshairOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading) return;
        _settingsService.Settings.Crosshair.OffsetX = SliderCrossX.Value;
        _settingsService.Settings.Crosshair.OffsetY = SliderCrossY.Value;
        TxtCrossX.Text = ((int)SliderCrossX.Value).ToString();
        TxtCrossY.Text = ((int)SliderCrossY.Value).ToString();
        _settingsService.Save();
    }

    #endregion

    #region Hotkey Events

    private void OnHotkeyButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tag) return;

        // Simple hotkey recording: show a dialog asking the user to press a key combination
        var dialog = new HotkeyRecordWindow(this)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            var h = _settingsService.Settings.Hotkeys;
            switch (tag)
            {
                case "Overlay":   h.ToggleOverlay = dialog.Result; break;
                case "Crosshair": h.ToggleCrosshair = dialog.Result; break;
                case "Split":     h.ToggleSplitScreen = dialog.Result; break;
                case "Display":   h.ToggleDisplayMode = dialog.Result; break;
                case "Size":      h.CycleOverlaySize = dialog.Result; break;
                case "Profile":   h.CycleProfile = dialog.Result; break;
            }
            _settingsService.Save();
            UpdateHotkeyButtons();
            UpdateStatus("热键已更新（重启后生效）");
        }
    }

    #endregion

    #region Profile Events

    private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || CmbProfiles.SelectedItem is not string name) return;
        _profileService.SetActiveProfile(name);
        LoadSettingsToUI();
        UpdateStatus($"已切换到预设: {name}");
    }

    private void OnSaveProfileClick(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog(this, "保存预设", "预设名称:", _profileService.ActiveProfile.Name)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
        {
            _profileService.AddProfile(dialog.InputText.Trim());
            LoadProfiles();
            UpdateStatus($"预设已保存: {dialog.InputText.Trim()}");
        }
    }

    private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
    {
        if (CmbProfiles.SelectedItem is not string name) return;

        if (_profileService.ActiveProfile.IsBuiltIn)
        {
            MessageBox.Show(this, "内置预设无法删除。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(this, $"确定删除预设 \"{name}\"？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _profileService.RemoveProfile(name);
            LoadProfiles();
            LoadSettingsToUI();
            UpdateStatus($"预设已删除: {name}");
        }
    }

    #endregion

    #region General Events

    private void OnStartupChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        StartupService.SetStartup(ChkStartup.IsChecked ?? false);
    }

    private void OnGeneralSettingChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var s = _settingsService.Settings;
        s.ShowOverlayOnStartup = ChkShowOverlayOnStart.IsChecked ?? true;
        s.ShowCrosshairOnStartup = ChkShowCrosshairOnStart.IsChecked ?? true;
        s.MinimizeToTray = ChkMinimizeToTray.IsChecked ?? true;
        _settingsService.Save();
    }

    #endregion

    #region Window Events

    protected override void OnClosing(CancelEventArgs e)
    {
        // Save window position
        _settingsService.Settings.SettingsWindowLeft = (int)Left;
        _settingsService.Settings.SettingsWindowTop = (int)Top;
        _settingsService.Save();

        base.OnClosing(e);
    }

    #endregion

    private void UpdateStatus(string message)
    {
        TxtStatus.Text = message;
    }
}

/// <summary>
/// A simple input dialog for entering text (e.g., profile name).
/// </summary>
public class InputDialog : Window
{
    public string InputText { get; private set; } = "";

    // Hardcoded colors (same palette as SettingsWindow, but local to avoid FindResource failures)
    private static readonly System.Windows.Media.SolidColorBrush BgDark = new(System.Windows.Media.Color.FromRgb(24, 24, 37));
    private static readonly System.Windows.Media.SolidColorBrush BgCard = new(System.Windows.Media.Color.FromRgb(49, 50, 68));
    private static readonly System.Windows.Media.SolidColorBrush TextPrimary = new(System.Windows.Media.Color.FromRgb(205, 214, 244));
    private static readonly System.Windows.Media.SolidColorBrush Border = new(System.Windows.Media.Color.FromRgb(69, 71, 90));

    public InputDialog(Window owner, string title, string prompt, string defaultValue = "")
    {
        Title = title;
        Width = 350;
        Height = 160;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = BgDark;
        ResizeMode = ResizeMode.NoResize;

        var stack = new StackPanel { Margin = new Thickness(16) };
        stack.Children.Add(new TextBlock
        {
            Text = prompt,
            Foreground = TextPrimary,
            Margin = new Thickness(0, 0, 0, 8)
        });

        var textBox = new TextBox
        {
            Text = defaultValue,
            Background = BgCard,
            Foreground = TextPrimary,
            BorderBrush = Border,
            Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 0, 12)
        };
        stack.Children.Add(textBox);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var okBtn = new Button
        {
            Content = "确定",
            Padding = new Thickness(16, 4, 16, 4),
            Margin = new Thickness(0, 0, 8, 0)
        };
        okBtn.Click += (_, _) => { InputText = textBox.Text; DialogResult = true; };
        var cancelBtn = new Button
        {
            Content = "取消",
            Padding = new Thickness(16, 4, 16, 4)
        };
        cancelBtn.Click += (_, _) => { DialogResult = false; };
        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        stack.Children.Add(btnPanel);

        Content = stack;
        textBox.SelectAll();
        textBox.Focus();
    }
}
