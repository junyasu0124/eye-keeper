using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using static EyeKeeper.Core.Core;

namespace EyeKeeper.Tasktray;

[ObservableObject]
public sealed partial class Tasktray : UserControl
{
  public Tasktray()
  {
    InitializeComponent();

    TimerEnabledChanged += (sender, e) =>
    {
      SetPauseResumeVisibility(e.Enabled);
    };
    SetPauseResumeVisibility(IsTimerRunning);
  }

  private void SetPauseResumeVisibility(bool enabled)
  {
    if (enabled)
    {
      Pause.Visibility = Visibility.Visible;
      Resume.Visibility = Visibility.Collapsed;
    }
    else
    {
      Pause.Visibility = Visibility.Collapsed;
      Resume.Visibility = Visibility.Visible;
    }
  }

  [RelayCommand]
#pragma warning disable CA1822
  public void ShowMainWindow()
#pragma warning restore CA1822
  {
    if (App.MainWindow == null)
    {
      App.ShowNewMainWindow();
    }
    else
    {
      App.MainWindow.Show();
    }
  }

  private void Open_Click(object sender, RoutedEventArgs e)
  {
    // NO ShowMainWindow();
    if (App.MainWindow == null)
    {
      App.ShowNewMainWindow();
    }
    else
    {
      App.MainWindow.Show();
    }
  }

  private void Pause_Click(object sender, RoutedEventArgs e)
  {
    StopTimer();
    StartStopChanged?.Invoke(this, false);
    SetPauseResumeVisibility(false);
  }

  private void Resume_Click(object sender, RoutedEventArgs e)
  {
    StartTimer();
    StartStopChanged?.Invoke(this, true);
    SetPauseResumeVisibility(true);
  }

  private void Exit_Click(object sender, RoutedEventArgs e)
  {
    App.ExitApplication();
  }

#pragma warning disable CA1822
  private BitmapImage GetIconSource()
#pragma warning restore CA1822
  {
    return new BitmapImage(new Uri("ms-appx:///Images/TasktrayIcon.ico"));
  }
}
