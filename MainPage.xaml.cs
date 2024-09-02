using Microsoft.Maui.Handlers;

namespace MauiTheme202409;

public partial class MainPage : ContentPage
{
	int count = 0;

#if WINDOWS
    /// <summary>Current theme of the application.</summary>
    private static Microsoft.UI.Xaml.ElementTheme currentAppTheme = Microsoft.UI.Xaml.ElementTheme.Default;

    static MainPage()
    {
        ConfigurePickerBehavior();
    }

    private static void ConfigurePickerBehavior()
    {
        PickerHandler.Mapper.AppendToMapping("MAUI.Pickers.Customization", (IPickerHandler handler, IPicker view) =>
        {
            if (view is not Picker)
                return;

            if (handler.PlatformView is not Microsoft.UI.Xaml.Controls.ComboBox nativeView)
                return;

            // Optimization to avoid refreshing element's theme every time it appears in the UI tree.
            Microsoft.UI.Xaml.ElementTheme? lastSetTheme = null;

            // Once the native view appears in the UI tree, set the correct theme.
            nativeView.Loaded += (s, e) =>
            {
                if ((lastSetTheme is null) || (lastSetTheme != currentAppTheme))
                {
                    lastSetTheme = currentAppTheme;

                    // Make sure the theme of the control is set right even if it is not in the UI tree.
                    nativeView.RequestedTheme = currentAppTheme;
                }
            };
        });
    }

#endif

    public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
#if WINDOWS
        count++;

        AppTheme appTheme = count % 2 == 0
            ? AppTheme.Dark
            : AppTheme.Light;

        pickerColors.IsVisible = count % 2 == 0;
        pickerMonkeys.IsVisible = count % 2 == 1;

        Application.Current!.UserAppTheme = appTheme;
        SetWindowTheme(appTheme);

        // In practice, you should iterate over all your app's windows.
        MauiWinUIWindow platformWindow = (MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!;
        ApplyTheme(platformWindow.Content);
#endif
    }

#if WINDOWS

    public static void SetWindowTheme(AppTheme appTheme)
    {
        Microsoft.UI.Xaml.ElementTheme elementTheme = appTheme switch
        {
            AppTheme.Light => Microsoft.UI.Xaml.ElementTheme.Light,
            AppTheme.Dark => Microsoft.UI.Xaml.ElementTheme.Dark,
            AppTheme.Unspecified => Microsoft.UI.Xaml.ElementTheme.Default,
            _ => throw new NotSupportedException($"Invalid application theme value '{appTheme}'."),
        };

        currentAppTheme = elementTheme;
    }

    public static void ApplyTheme(Microsoft.UI.Xaml.DependencyObject obj)
    {
        // Only combo boxes for simplicity (called Pickers in MAUI).
        foreach (Microsoft.UI.Xaml.Controls.ComboBox comboBox in FindDescendants<Microsoft.UI.Xaml.Controls.ComboBox>(obj))
        {
            comboBox.RequestedTheme = currentAppTheme;

            // To see what this method does, uncomment this line.
            // comboBox.Background = new global::Microsoft.UI.Xaml.Media.SolidColorBrush(global::Windows.UI.Color.FromArgb(128, 255, 0, 0));
        }
    }

    /// <remarks>This method iterates only over UI tree elements. If an element is not attached to the UI tree, it won't be found!</remarks>
    public static IEnumerable<T> FindDescendants<T>(Microsoft.UI.Xaml.DependencyObject dobj)
        where T : Microsoft.UI.Xaml.DependencyObject
    {
        int count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(dobj);
        for (int i = 0; i < count; i++)
        {
            Microsoft.UI.Xaml.DependencyObject element = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(dobj, i);
            if (element is T t)
                yield return t;

            foreach (T descendant in FindDescendants<T>(element))
                yield return descendant;
        }
    }
#endif
}