using System.Windows;
using System.Windows.Input;

namespace StiltzkinsBag.App.Views;

public partial class MainWindow : Window
{
    // Track manual-maximize state so we can restore properly.
    private bool _isManuallyMaximized = false;
    private Rect _restoreBounds;

    public MainWindow()
    {
        InitializeComponent();
    }

    // ── Custom chrome handlers ────────────────────────────────────────────────

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        Close();

    // ── Maximize / restore ───────────────────────────────────────────────────
    // AllowsTransparency=True + WindowStyle=None causes WPF to add an 8px offset
    // when using WindowState=Maximized (it compensates for non-existent chrome).
    // Fix: manually set Left/Top/Width/Height to the work area, never use
    // WindowState=Maximized on a transparent frameless window.

    private void ToggleMaximize()
    {
        if (_isManuallyMaximized)
        {
            // Restore previous size and position
            Left = _restoreBounds.Left;
            Top = _restoreBounds.Top;
            Width = _restoreBounds.Width;
            Height = _restoreBounds.Height;
            _isManuallyMaximized = false;
        }
        else
        {
            // Save current bounds before maximizing
            _restoreBounds = new Rect(Left, Top, Width, Height);

            // Use WorkArea (excludes taskbar) and set position manually
            var wa = SystemParameters.WorkArea;
            Left = wa.Left;
            Top = wa.Top;
            Width = wa.Width;
            Height = wa.Height;
            _isManuallyMaximized = true;
        }
    }
}