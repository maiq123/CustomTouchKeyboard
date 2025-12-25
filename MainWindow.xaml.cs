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
    private void OnKeyDown(object sender, InputEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string keyName)
        {
            // Capture touch to ensure release even if finger slides away
            if (e is TouchEventArgs te) te.TouchDevice.Capture(btn);
            else if (e is MouseEventArgs me) Mouse.Capture(btn);
            
            VirtualKeyCode code = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyName);
            _sim.Keyboard.KeyDown(code);
            btn.Background = System.Windows.Media.Brushes.DarkRed;
        }
    }

    private void OnKeyUp(object sender, InputEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string keyName)
        {
            if (e is TouchEventArgs te) te.TouchDevice.Capture(null);
            else if (e is MouseEventArgs me) Mouse.Capture(null);
            
            VirtualKeyCode code = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), keyName);
            _sim.Keyboard.KeyUp(code);
            btn.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#444444")!;
        }
    }

    // Handles combinations like Shift+Alt
    private void OnComboDown(object sender, InputEventArgs e)
    {
        _sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
        _sim.Keyboard.KeyDown(VirtualKeyCode.MENU); // MENU = Alt
        ((Button)sender).Background = System.Windows.Media.Brushes.DarkRed;
    }

    private void OnComboUp(object sender, InputEventArgs e)
    {
        _sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
        _sim.Keyboard.KeyUp(VirtualKeyCode.MENU);
        ((Button)sender).Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#444444")!;
    }

    // Allows moving the window by dragging the background
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }
     
}