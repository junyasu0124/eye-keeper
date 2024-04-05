using CommunityToolkit.WinUI.Notifications;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using System;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace EyeKeeper;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
  public static TaskbarIcon Tasktray { get; set; }
  public static Window MainWindow { get; set; }
  public static bool HandleClosedEvents { get; set; } = true;

  /// <summary>
  /// Initializes the singleton application object.  This is the first line of authored code
  /// executed, and as such is the logical equivalent of main() or WinMain().
  /// </summary>
  public App()
  {
    InitializeComponent();

    Core.Core.Initialize();

    ToastNotificationManagerCompat.OnActivated += NotificationOnActivated;
  }

  private void NotificationOnActivated(ToastNotificationActivatedEventArgsCompat e)
  {
    if (e.Argument == "TakeBreak")
      Core.Core.CreateProgressNotification();
    else if (e.Argument == "Skip")
      Core.Core.SkipBreak();
  }

  /// <summary>
  /// Invoked when the application is launched.
  /// </summary>
  /// <param name="args">Details about the launch request and process.</param>
  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    ShowNewMainWindow();
  }

  internal static void ShowNewMainWindow()
  {
    MainWindow = new MainWindow();
    MainWindow.Closed += (sender, args) =>
    {
      if (HandleClosedEvents)
      {
        args.Handled = true;
        MainWindow.Hide();
      }
    };
    MainWindow.Activate();
  }

  internal static void ExitApplication()
  {
    HandleClosedEvents = false;
    Tasktray?.Dispose();
    MainWindow?.Close();

    if (MainWindow == null)
    {
      Environment.Exit(0);
    }
  }
}
