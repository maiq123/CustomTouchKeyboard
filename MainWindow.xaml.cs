using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using MouseButton = System.Windows.Input.MouseButton;

namespace CustomTouchKeyboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    
    private readonly InputSimulator _sim = new();
    
    // Define these once at the top of your class
    private readonly System.Windows.Media.Brush _activeBrush = System.Windows.Media.Brushes.DarkRed;
    private readonly System.Windows.Media.Brush _defaultBrush = 
        (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#444444")!;
    private readonly HashSet<VirtualKeyCode> _lockedKeys = new();
    
    // Win32 Constants
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // This makes the window "un-focusable" so your 3D app stays active
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
    }

    // Handles Shift, Ctrl, Alt, H, G
    // private void OnKeyDown(object sender, InputEventArgs e)
    // {
    //     if (sender is Button btn)
    //     {
    //         // Change color immediately
    //         btn.Background = _activeBrush;
    //
    //         if (btn.Tag is string keyName && Enum.TryParse(keyName, out VirtualKeyCode code))
    //         {
    //             _sim.Keyboard.KeyDown(code);
    //         }
    //
    //         // Capture logic to prevent "stuck" keys
    //         if (e is TouchEventArgs te) te.TouchDevice.Capture(btn);
    //         else if (e is MouseEventArgs me) Mouse.Capture(btn);
    //     
    //         e.Handled = true;
    //     }
    // }
    //
    // private void OnKeyUp(object sender, InputEventArgs e)
    // {
    //     if (sender is Button btn)
    //     {
    //         // Revert color
    //         btn.Background = _defaultBrush;
    //
    //         if (btn.Tag is string keyName && Enum.TryParse(keyName, out VirtualKeyCode code))
    //         {
    //             _sim.Keyboard.KeyUp(code);
    //         }
    //
    //         // Release capture
    //         if (e is TouchEventArgs te) te.TouchDevice.Capture(null);
    //         else if (e is MouseEventArgs me) Mouse.Capture(null);
    //     
    //         e.Handled = true;
    //     }
    // }
    
    private void OnKeyDown(object sender, InputEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string keyName)
        {
            if (Enum.TryParse(keyName, out VirtualKeyCode code))
            {
                if (!_lockedKeys.Contains(code))
                {
                    // KEY DOWN (Lock it)
                    _sim.Keyboard.KeyDown(code);
                    _lockedKeys.Add(code);
                    btn.Background = System.Windows.Media.Brushes.DarkRed;
                    btn.BorderBrush = System.Windows.Media.Brushes.Gold; // Visual indicator it's locked
                    btn.BorderThickness = new Thickness(3);
                }
                else
                {
                    // KEY UP (Release it)
                    _sim.Keyboard.KeyUp(code);
                    _lockedKeys.Remove(code);
                    btn.Background = _defaultBrush;
                    btn.BorderThickness = new Thickness(1);
                }
            }
            e.Handled = true;
        }
    }

    // Handles combinations like Shift+Alt
    private void OnComboDown(object sender, InputEventArgs e)
    {
        var btn = (Button)sender;
        btn.Background = _activeBrush;
        _sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
        _sim.Keyboard.KeyDown(VirtualKeyCode.MENU); // Alt
    }

    private void OnComboUp(object sender, InputEventArgs e)
    {
        var btn = (Button)sender;
        btn.Background = _defaultBrush;
        _sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
        _sim.Keyboard.KeyUp(VirtualKeyCode.MENU);
    }

    // Allows moving the window by dragging the background
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }
     
}