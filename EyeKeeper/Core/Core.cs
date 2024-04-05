using CommunityToolkit.WinUI.Notifications;
using EyeKeeper.Utils;
using Microsoft.Win32;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Notifications;

namespace EyeKeeper.Core;

[DebuggerDisplay("Time {Minutes.ToString(\"00\")}:{Seconds.ToString(\"00\")}")]
public class Time
{
  public Time() { }
  public Time(int minutes, int seconds)
  {
    Minutes = minutes;
    Seconds = seconds;
  }
  public Time(int minutes)
  {
    Minutes = minutes;
  }
  public Time(TimeSpan timeSpan)
  {
    Minutes = timeSpan.Minutes;
    Seconds = timeSpan.Seconds;
  }

  private int minutes = 0;
  public int Minutes
  {
    get => minutes;
    set => minutes = value switch
    {
      < 0 => throw new ArgumentOutOfRangeException(nameof(Minutes), "Minutes must be greater than or equal to 0"),
      _ => value
    };
  }
  private int seconds = 0;
  public int Seconds
  {
    get => seconds;
    set
    {
      if (value < 0)
      {
        seconds = 60 + value;
        Minutes--;
      }
      else if (value >= 60)
      {
        seconds = value % 60;
        Minutes += value / 60;
      }
      else
      {
        seconds = value;
      }
    }
  }

  public int TotalSeconds => Minutes * 60 + Seconds;

  public TimeSpan ToTimeSpan() => new(0, Minutes, Seconds);

  public static Time operator +(Time time1, Time time2)
  {
    return new Time
    {
      Minutes = time1.Minutes + time2.Minutes,
      Seconds = time1.Seconds + time2.Seconds,
    };
  }
  public static Time operator +(Time time1, TimeSpan time2)
  {
    return new Time
    {
      Minutes = time1.Minutes + time2.Minutes,
      Seconds = time1.Seconds + time2.Seconds,
    };
  }
  public static Time operator -(Time time1, Time time2)
  {
    return new Time
    {
      Minutes = time1.Minutes - time2.Minutes,
      Seconds = time1.Seconds - time2.Seconds,
    };
  }
  public static Time operator -(Time time1, TimeSpan time2)
  {
    return new Time
    {
      Minutes = time1.Minutes - time2.Minutes,
      Seconds = time1.Seconds - time2.Seconds,
    };
  }
  public static bool operator >(Time time1, Time time2)
  {
    return time1.TotalSeconds > time2.TotalSeconds;
  }
  public static bool operator <(Time time1, Time time2)
  {
    return time1.TotalSeconds < time2.TotalSeconds;
  }
  public static bool operator ==(Time time1, Time time2)
  {
    return time1.Minutes == time2.Minutes && time1.Seconds == time2.Seconds;
  }
  public static bool operator !=(Time time1, Time time2)
  {
    return time1.Minutes != time2.Minutes || time1.Seconds != time2.Seconds;
  }

  public override bool Equals(object obj)
  {
    return obj is Time time &&
           Minutes == time.Minutes &&
           Seconds == time.Seconds;
  }
  public override int GetHashCode()
  {
    return HashCode.Combine(Minutes, Seconds);
  }
}

public class TickEventArgs(State state, bool isStateChanged, Time remainingTime) : EventArgs
{
  public State State { get; } = state;
  public bool IsStateChanged { get; } = isStateChanged;
  public Time RemainingTime { get; } = remainingTime;
}

public enum State
{
  Work,
  Break,
}

public static class Core
{
  private static readonly ResourceLoader loader = new();

  private readonly static ObservableTimer timer = new() { Interval = 1000 };
  public static EventHandler<ObservableTimerEnabledChangedEventArgs> TimerEnabledChanged { get; set; }

  public static bool IsTimerRunning => timer.Enabled;

  public static EventHandler<TickEventArgs> Tick { get; set; }
  public static EventHandler<bool> StartStopChanged { get; set; }
  public static State State { get; set; } = State.Work;

  public static Time WorkTime { get; set; } = new(ApplicationData.Current.LocalSettings.Values["TimeWork"] is TimeSpan timeSpan ? timeSpan : new TimeSpan(0, 20, 0));
  public static Time BreakTime { get; set; } = new(ApplicationData.Current.LocalSettings.Values["TimeBreak"] is TimeSpan timeSpan ? timeSpan : new TimeSpan(0, 0, 20));

  public static Time RemainingTime { get; set; } = WorkTime;

  private static bool isTimerStoppedBySuspend = false;

  public static void Initialize()
  {
    SystemEvents.PowerModeChanged += (sender, e) =>
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          isTimerStoppedBySuspend = true;
          timer.Stop();
          StartStopChanged?.Invoke(null, false);
          break;
        case PowerModes.Resume:
          if (IsTimerRunning && isTimerStoppedBySuspend)
          {
            timer.Start();
            StartStopChanged?.Invoke(null, true);
          }
          break;
      }
    };

    timer.Elapsed += (sender, e) =>
    {
      isTimerStoppedBySuspend = false;

      bool isStateChanged = false;
      if (RemainingTime.Minutes == 0 && RemainingTime.Seconds == 0)
      {
        isStateChanged = true;
        timer.Stop();
        if (State == State.Work)
        {
          State = State.Break;
          RemainingTime = BreakTime;
        }
        else
        {
          State = State.Work;
          RemainingTime = WorkTime;
          isNotificationShown = false;
        }
        StateChanged(State);
      }
      else
      {
        RemainingTime -= TimeSpan.FromSeconds(1);
      }
      if (isNotificationShown)
        UpdateProgressNotification();

      Tick?.Invoke(null, new(State, isStateChanged, RemainingTime));
    };
    timer.EnabledChanged += (sender, e) =>
    {
      TimerEnabledChanged?.Invoke(null, e);
    };
  }

  public static void StartTimer()
  {
    timer.Start();
    ToastNotificationManager.GetDefault().History.Remove(progressNotificationTag, progressNotificationGroup);
    if (previousTakeBreakNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousTakeBreakNotification);
      previousTakeBreakNotification = null;
    }
  }
  public static void StopTimer()
  {
    timer.Stop();
  }
  public static void ResetTimer()
  {
    RemainingTime = State switch
    {
      State.Work => WorkTime,
      State.Break => BreakTime,
      _ => throw new NotImplementedException(),
    };
    RestartTimer();
    ToastNotificationManager.GetDefault().History.Remove(progressNotificationTag, progressNotificationGroup);
    if (previousTakeBreakNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousTakeBreakNotification);
      previousTakeBreakNotification = null;
    }
    if (previousProgressNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousProgressNotification);
      previousProgressNotification = null;
    }
  }
  public static void ChangeState(State state)
  {
    State = state;
    RemainingTime = state switch
    {
      State.Work => WorkTime,
      State.Break => BreakTime,
      _ => throw new NotImplementedException(),
    };
    RestartTimer();
  }

  private static void RestartTimer()
  {
    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
      timer.Start();
      StartStopChanged?.Invoke(null, true);
    });
  }

  private static ToastNotification previousTakeBreakNotification = null;
  private static void StateChanged(State state)
  {
    if (previousTakeBreakNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousTakeBreakNotification);
      previousTakeBreakNotification = null;
    }
    if (previousProgressNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousProgressNotification);
      previousProgressNotification = null;
    }

    if (state == State.Break && App.MainWindow != null && !(App.MainWindow as MainWindow).IsActivated)
    {
      var content = new ToastContentBuilder()
        .SetToastScenario(ToastScenario.Alarm)
        .AddAudio(new ToastAudio() { Silent = true })
        .AddText(loader.GetString("Notification_Title"))
        .AddButton(new ToastButton(loader.GetString("Notification_TakeBreak"), "TakeBreak")
        {
          ActivationType = ToastActivationType.Background,
          ActivationOptions = new ToastActivationOptions() { AfterActivationBehavior = ToastAfterActivationBehavior.PendingUpdate }
        })
        .AddButton(new ToastButton(loader.GetString("Notification_Skip"), "Skip")
        {
          ActivationType = ToastActivationType.Background
        });

      var toast = new ToastNotification(content.GetXml());
      ToastNotificationManager.CreateToastNotifier().Show(toast);
      previousTakeBreakNotification = toast;

      Tick?.Invoke(null, new(State.Break, false, BreakTime));
      timer.Stop();
      StartStopChanged?.Invoke(null, false);
    }
    else
    {
      RestartTimer();
    }
  }
  private const string progressNotificationTag = "Progress";
  private const string progressNotificationGroup = "Progress";
  private static bool isNotificationShown = false;
  private static ToastNotification previousProgressNotification = null;
  public static void CreateProgressNotification()
  {
    ToastNotificationManager.GetDefault().History.Clear();
    if (previousProgressNotification != null)
    {
      ToastNotificationManager.CreateToastNotifier().Hide(previousProgressNotification);
      previousProgressNotification = null;
    }

    var content = new ToastContentBuilder()
      .SetToastScenario(ToastScenario.Alarm)
      .AddAudio(new ToastAudio() { Silent = true })
      .AddText(loader.GetString("Notification_Breaking"))
      .AddVisualChild(new AdaptiveProgressBar()
      {
        Value = new BindableProgressBarValue("ProgressValue"),
        ValueStringOverride = new BindableString("ProgressValueString"),
        Status = new BindableString("ProgressStatus"),
      })
      .GetToastContent();

    var toast = new ToastNotification(content.GetXml())
    {
      Tag = progressNotificationTag,
      Group = progressNotificationGroup,

      Data = new NotificationData(),
    };
    toast.Data.Values["ProgressValue"] = "0";
    toast.Data.Values["ProgressValueString"] = $"{loader.GetString("Notification_SecondsPrefix")} {RemainingTime.TotalSeconds} {loader.GetString("Notification_SecondsSuffix")}";
    toast.Data.Values["ProgressStatus"] = "";
    toast.Data.SequenceNumber = 1;

    ToastNotificationManager.CreateToastNotifier().Show(toast);
    previousProgressNotification = toast;

    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
      timer.Start();
      StartStopChanged?.Invoke(null, true);
    });

    isNotificationShown = true;
  }
  private static void UpdateProgressNotification()
  {
    var data = new NotificationData
    {
      SequenceNumber = 1,
    };
    data.Values["ProgressValue"] = ((double)(BreakTime - RemainingTime).TotalSeconds / BreakTime.TotalSeconds).ToString();
    data.Values["ProgressValueString"] = $"{loader.GetString("Notification_SecondsPrefix")} {RemainingTime.TotalSeconds} {loader.GetString("Notification_SecondsSuffix")}";
    if (RemainingTime.TotalSeconds == 0)
      data.Values["ProgressStatus"] = loader.GetString("Notification_Completed");
    else
      data.Values["ProgressStatus"] = "";

    ToastNotificationManager.CreateToastNotifier().Update(data, progressNotificationTag, progressNotificationGroup);
  }
  public static void SkipBreak()
  {
    State = State.Work;
    RemainingTime = WorkTime;
    RestartTimer();

    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
      Tick?.Invoke(null, new(State.Work, true, RemainingTime));
    });
  }

  public static void ChangeTime(State state, Time time)
  {
    if (state == State.Work)
    {
      WorkTime = time;
    }
    else if (state == State.Break)
    {
      BreakTime = time;
    }
    Tick?.Invoke(null, new(state, false, RemainingTime));
  }
}
