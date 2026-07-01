using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using DF3D.Helpers;
using DF3D.Models;

namespace DF3D.Windows;

/// <summary>
/// A small dialog that captures the next key combination pressed by the user.
/// Used for recording custom hotkey bindings.
/// </summary>
public class HotkeyRecordWindow : Window
{
    public HotkeyBinding? Result { get; private set; }

    private readonly System.Windows.Controls.TextBlock _promptText;

    public HotkeyRecordWindow(Window owner)
    {
        Owner = owner;
        Title = "录制热键";
        Width = 300;
        Height = 120;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = System.Windows.Media.Brushes.DarkSlateGray;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;

        var stack = new System.Windows.Controls.StackPanel
        {
            Margin = new Thickness(16),
            VerticalAlignment = VerticalAlignment.Center
        };

        _promptText = new System.Windows.Controls.TextBlock
        {
            Text = "按下想要使用的热键组合...\n\n（按 Esc 取消）",
            Foreground = System.Windows.Media.Brushes.White,
            TextAlignment = TextAlignment.Center,
            FontSize = 14
        };
        stack.Children.Add(_promptText);
        Content = stack;

        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Escape cancels
        if (key == Key.Escape)
        {
            DialogResult = false;
            return;
        }

        // Ignore modifier-only keys
        if (key is Key.LeftCtrl or Key.RightCtrl or
            Key.LeftAlt or Key.RightAlt or
            Key.LeftShift or Key.RightShift or
            Key.LWin or Key.RWin)
        {
            return;
        }

        // Build modifiers
        uint modifiers = 0;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            modifiers |= Win32Helper.MOD_CONTROL;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= Win32Helper.MOD_ALT;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            modifiers |= Win32Helper.MOD_SHIFT;

        // Require at least one modifier to avoid conflicts
        if (modifiers == 0)
        {
            _promptText.Text = "请至少按住一个修饰键\n（Ctrl/Alt/Shift）";
            return;
        }

        // Convert WPF Key to virtual key code
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

        Result = new HotkeyBinding(modifiers, vk);
        _promptText.Text = $"已录制: {Result.DisplayString}\n\n按 Enter 确认，Esc 取消";

        // On Enter, confirm; on another key, re-record
        PreviewKeyDown -= OnPreviewKeyDown;
        PreviewKeyDown += OnConfirmKeyDown;
    }

    private void OnConfirmKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            DialogResult = true;
        }
        else if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
        else
        {
            // Re-record with new key
            PreviewKeyDown -= OnConfirmKeyDown;
            PreviewKeyDown += OnPreviewKeyDown;
            _promptText.Text = "按下想要使用的热键组合...\n\n（按 Esc 取消）";

            // Trigger the normal recording handler
            OnPreviewKeyDown(sender, e);
        }
    }
}
