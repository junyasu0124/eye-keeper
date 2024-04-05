using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace EyeKeeper.Controls;

public sealed partial class TimeSelector : UserControl
{
  public EventHandler<TimeSpan> TimeChanged;

  public int Minutes
  {
    get => (int)MinutesCombo.SelectedItem;
    set
    {
      MinutesCombo.SelectedItem = value;
      if (value == 0)
      {
        if (!isZeroEnabled)
        {
          if (Seconds == 0)
            Seconds = 1;
          RemoveZeroFromSeconds();
        }
      }
    }
  }
  public int Seconds
  {
    get => (int)SecondsCombo.SelectedItem;
    set
    {
      SecondsCombo.SelectedItem = value;
      if (value == 0)
      {
        if (!isZeroEnabled)
        {
          if (Minutes == 0)
            Minutes = 1;
          RemoveZeroFromMinutes();
        }
      }
    }
  }

  private bool isZeroEnabled = true;
  public bool IsZeroEnabled
  {
    get => isZeroEnabled;
    set
    {
      isZeroEnabled = value;
      if (!value && Minutes == 0 && Seconds == 0)
      {
        Minutes = 1;
        MinutesCombo.SelectedItem = 1;
        TimeChanged?.Invoke(this, new TimeSpan(0, 1, (int)SecondsCombo.SelectedItem));
        RemoveZeroFromMinutes();
      }
      else if (value)
      {
        AddZeroToMinutes();
        AddZeroToSeconds();
      }
    }
  }

  private void AddZeroToMinutes()
  {
    var selected = MinutesCombo.SelectedItem;
    MinutesCombo.ItemsSource = Enumerable.Range(0, 100).ToList();
    MinutesCombo.SelectedItem = selected;
  }
  private void RemoveZeroFromMinutes()
  {
    var selected = MinutesCombo.SelectedItem;
    MinutesCombo.ItemsSource = Enumerable.Range(1, 100).ToList();
    MinutesCombo.SelectedItem = selected;
  }
  private void AddZeroToSeconds()
  {
    var selected = SecondsCombo.SelectedItem;
    SecondsCombo.ItemsSource = Enumerable.Range(0, 60).ToList();
    SecondsCombo.SelectedItem = selected;
  }
  private void RemoveZeroFromSeconds()
  {
    var selected = SecondsCombo.SelectedItem;
    SecondsCombo.ItemsSource = Enumerable.Range(1, 60).ToList();
    SecondsCombo.SelectedItem = selected;
  }

  public TimeSelector()
  {
    InitializeComponent();

    AddZeroToMinutes();
    AddZeroToSeconds();

    MinutesCombo.SelectedItem = 1;
    SecondsCombo.SelectedItem = 0;
  }

  private int minutesSelectedWhenGotFocus = -1;
  private void MinutesCombo_DropDownOpened(object sender, object e)
  {
    minutesSelectedWhenGotFocus = (int)MinutesCombo.SelectedItem;
  }
  private void MinutesCombo_DropDownClosed(object sender, object e)
  {
    if (MinutesCombo.SelectedItem != null && SecondsCombo.SelectedItem != null && (int)MinutesCombo.SelectedItem != minutesSelectedWhenGotFocus)
    {
      TimeChanged?.Invoke(this, new TimeSpan(0, (int)MinutesCombo.SelectedItem, (int)SecondsCombo.SelectedItem));
      if (!isZeroEnabled && (int)MinutesCombo.SelectedItem == 0)
      {
        RemoveZeroFromSeconds();
      }
      else
      {
        AddZeroToMinutes();
        AddZeroToSeconds();
      }
    }
  }

  private int secondsSelectedWhenGotFocus = -1;
  private void SecondsCombo_DropDownOpened(object sender, object e)
  {
    secondsSelectedWhenGotFocus = (int)SecondsCombo.SelectedItem;
  }

  private void SecondsCombo_DropDownClosed(object sender, object e)
  {
    if (MinutesCombo.SelectedItem != null && SecondsCombo.SelectedItem != null && (int)SecondsCombo.SelectedItem != secondsSelectedWhenGotFocus)
    {
      TimeChanged?.Invoke(this, new TimeSpan(0, (int)MinutesCombo.SelectedItem, (int)SecondsCombo.SelectedItem));
      if (!isZeroEnabled && (int)SecondsCombo.SelectedItem == 0)
      {
        RemoveZeroFromMinutes();
      }
      else
      {
        AddZeroToMinutes();
        AddZeroToSeconds();
      }
    }
  }
}
