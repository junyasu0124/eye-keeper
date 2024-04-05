using System;
using System.Timers;

namespace EyeKeeper.Utils;

public class ObservableTimerEnabledChangedEventArgs : EventArgs
{
  public required bool Enabled { get; set; }
}


public class ObservableTimer : Timer
{

  public EventHandler<ObservableTimerEnabledChangedEventArgs> EnabledChanged { get; set; }

  public new bool Enabled
  {
    get => base.Enabled;
    set
    {
      base.Enabled = value;
      EnabledChanged?.Invoke(this, new ObservableTimerEnabledChangedEventArgs { Enabled = value });
    }
  }

  public new void Start()
  {
    base.Start();
    EnabledChanged?.Invoke(this, new ObservableTimerEnabledChangedEventArgs { Enabled = true });
  }

  public new void Stop()
  {
    base.Stop();
    EnabledChanged?.Invoke(this, new ObservableTimerEnabledChangedEventArgs { Enabled = false });
  }
}
