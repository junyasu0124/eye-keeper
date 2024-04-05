using EyeKeeper.Pages;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using System;
using WinRT.Interop;
using static EyeKeeper.Core.Core;

namespace EyeKeeper;

public sealed partial class MainWindow : Window
{
  private readonly static string sKeyName = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
  private readonly static string sSubkeyName = "SystemUsesLightTheme";

  public IntPtr HWnd => WindowNative.GetWindowHandle(this);

  public bool IsActivated { get; private set; } = true;

  public MainWindow()
  {
    InitializeComponent();

    App.Tasktray = Tasktray.Icon;

    ContentFrame.Navigate(typeof(HomePage));

    Activated += (sender, e) =>
    {
      IsActivated = e.WindowActivationState != WindowActivationState.Deactivated;
    };

    var appWindow = GetAppWindowForCurrentWindow();

    SettingsPage.ThemeChanged += (sender, theme) =>
    {
      appWindow.TitleBar.ButtonForegroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.Black;
    };

    if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
    {
      TitleBar.Visibility = Visibility.Visible;
      appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

      RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset);
      LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset);

      appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
      appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

      var appTheme = SettingsPage.GetApplicationTheme();

      ParentSpace.RequestedTheme = appTheme;
      Navigation.RequestedTheme = appTheme;

      if (appTheme == ElementTheme.Default)
      {
        try
        {
          using var rKey = Registry.CurrentUser.OpenSubKey(sKeyName);
          var nResult = (int)rKey.GetValue(sSubkeyName);
          if (nResult == 1)
            appTheme = ElementTheme.Light;
          else
            appTheme = ElementTheme.Dark;
        }
        catch { }
      }
      appWindow.TitleBar.ButtonForegroundColor = appTheme == ElementTheme.Dark ? Colors.White : Colors.Black;
      appWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(50, 100, 100, 100);

      ParentSpace.ActualThemeChanged += (sender, e) =>
      {
        var currentTheme = SettingsPage.GetApplicationTheme();
        if (currentTheme == ElementTheme.Default)
          appWindow.TitleBar.ButtonForegroundColor = currentTheme == ElementTheme.Dark ? Colors.White : Colors.Black;
      };
    }
    else
    {
      TitleBar.Visibility = Visibility.Collapsed;
    }
  }

  private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
  {
    var wndId = Win32Interop.GetWindowIdFromWindow(HWnd);
    return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
  }

  private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.IsSettingsSelected)
    {
      ContentFrame.Navigate(typeof(SettingsPage));
    }
    else
    {
      if (State == Core.State.Work && RemainingTime > WorkTime)
        RemainingTime = WorkTime;
      else if (State == Core.State.Break && RemainingTime > BreakTime)
        RemainingTime = BreakTime;

      ContentFrame.Navigate(typeof(HomePage));
    }
  }
}
