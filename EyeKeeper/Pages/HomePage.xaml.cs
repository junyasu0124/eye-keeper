using EyeKeeper.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static EyeKeeper.Core.Core;

namespace EyeKeeper.Pages;

public sealed partial class HomePage : Page
{
  public HomePage()
  {
    InitializeComponent();

    if (IsTimerRunning && ((Core.Core.State == Core.State.Work && RemainingTime == WorkTime) || (Core.Core.State == Core.State.Work && RemainingTime == BreakTime)))
    {
      StartStop.Tag = true;
      StartStop.Content = StartStopIcon(true);

      Reset.IsEnabled = true;
      Break.IsEnabled = Core.Core.State == Core.State.Work;
    }
    else
    {
      StartStop.Tag = IsTimerRunning;
      StartStop.Content = StartStopIcon(IsTimerRunning);

      if (!((Core.Core.State == Core.State.Work && RemainingTime == WorkTime) || (Core.Core.State == Core.State.Work && RemainingTime == BreakTime)))
        Reset.IsEnabled = true;
      if (IsTimerRunning)
        Break.IsEnabled = Core.Core.State == Core.State.Work;
    }
    SetClockText(RemainingTime);

    Tick += (sender, e) =>
    {
      DispatcherQueue.TryEnqueue(() =>
      {
        SetClockText(e.RemainingTime);

        if (e.IsStateChanged)
        {
          if (e.State == Core.State.Break)
          {
            Break.IsEnabled = false;
            SetBreakMode(true);
          }
          else
          {
            Break.IsEnabled = true;
            SetBreakMode(false);
            StartStop.Tag = true;
            StartStop.Content = StartStopIcon(true);
          }
        }
      });
    };
    StartStopChanged += (sender, e) =>
    {
      DispatcherQueue.TryEnqueue(() =>
      {
        StartStop.Tag = e;
        StartStop.Content = StartStopIcon(e);
        Reset.IsEnabled = true;
        if (Core.Core.State == Core.State.Work)
          Break.IsEnabled = true;
      });
    };
  }

  private void SetClockText(Time time)
  {
    Clock_Minutes.Text = time.Minutes.ToString("00");
    Clock_Seconds.Text = time.Seconds.ToString("00");
  }

  private void SetBreakMode(bool isDisplay)
  {
    if (isDisplay)
    {
      ParentSpace.BorderThickness = new Thickness(4);
      BreakText.Visibility = Visibility.Visible;
    }
    else
    {
      ParentSpace.BorderThickness = new Thickness(0);
      BreakText.Visibility = Visibility.Collapsed;
    }
  }

  static SymbolIcon StartStopIcon(bool isPlaying) => isPlaying ? new SymbolIcon(Symbol.Pause) : new SymbolIcon(Symbol.Play);

  private void StartStop_Click(object sender, RoutedEventArgs e)
  {
    var tag = (sender as Button).Tag;

    var isRunning = tag is not null && (bool)tag;

    ((Button)sender).Tag = !isRunning;
    ((Button)sender).Content = StartStopIcon(!isRunning);
    if (!isRunning)
      StartTimer();
    else
      StopTimer();

    Reset.IsEnabled = true;
    if (Core.Core.State == Core.State.Work)
      Break.IsEnabled = true;
  }

  private void Reset_Click(object sender, RoutedEventArgs e)
  {
    ResetTimer();
    SetClockText(RemainingTime);
  }

  private void Break_Click(object sender, RoutedEventArgs e)
  {
    ChangeState(Core.State.Break);
    SetClockText(RemainingTime);
    SetBreakMode(true);
    StartStop.Tag = true;
    StartStop.Content = StartStopIcon(true);
  }
}
