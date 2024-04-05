using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Storage;

namespace EyeKeeper.Pages;

public sealed partial class SettingsPage : Page
{
  public static EventHandler<ElementTheme> ThemeChanged { get; set; }

  private readonly bool isInitialized = false;

  public SettingsPage()
  {
    InitializeComponent();

    switch (GetApplicationTheme())
    {
      case ElementTheme.Light:
        ThemeSelections.SelectedIndex = 1;
        break;
      case ElementTheme.Dark:
        ThemeSelections.SelectedIndex = 2;
        break;
      case ElementTheme.Default:
        ThemeSelections.SelectedIndex = 0;
        break;
    }

    if (ApplicationData.Current.LocalSettings.Values["TimeWork"] == null)
      ApplicationData.Current.LocalSettings.Values["TimeWork"] = new TimeSpan(0, 20, 0);
    if (ApplicationData.Current.LocalSettings.Values["TimeBreak"] == null)
      ApplicationData.Current.LocalSettings.Values["TimeBreak"] = new TimeSpan(0, 0, 20);

    TimeWorkSelector.Minutes = ((TimeSpan)ApplicationData.Current.LocalSettings.Values["TimeWork"]).Minutes;
    TimeWorkSelector.Seconds = ((TimeSpan)ApplicationData.Current.LocalSettings.Values["TimeWork"]).Seconds;

    TimeBreakSelector.Minutes = ((TimeSpan)ApplicationData.Current.LocalSettings.Values["TimeBreak"]).Minutes;
    TimeBreakSelector.Seconds = ((TimeSpan)ApplicationData.Current.LocalSettings.Values["TimeBreak"]).Seconds;

    TimeWorkSelector.TimeChanged += (sender, args) =>
    {
      ApplicationData.Current.LocalSettings.Values["TimeWork"] = args;
      Core.Core.ChangeTime(Core.State.Work, new(args));
    };
    TimeBreakSelector.TimeChanged += (sender, args) =>
    {
      ApplicationData.Current.LocalSettings.Values["TimeBreak"] = args;
      Core.Core.ChangeTime(Core.State.Break, new(args));
    };

    isInitialized = true;
  }

  private void ThemeSelections_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (isInitialized && e.AddedItems.Count == 1 && e.AddedItems[0] != null)
    {
      var newTheme = (e.AddedItems[0] as RadioButton).Name switch
      {
        nameof(ThemeSelectionsDefault) => ElementTheme.Default,
        nameof(ThemeSelectionsLight) =>ElementTheme.Light,
        nameof(ThemeSelectionsDark) => ElementTheme.Dark,
        _ => throw new NotImplementedException(),
      };
      SetApplicationTheme(newTheme);
      ThemeChanged?.Invoke(this, ActualTheme);
    }
  }
  public static ElementTheme GetApplicationTheme()
  {
    return ApplicationData.Current.LocalSettings.Values["Theme"] switch
    {
      "Light" => ElementTheme.Light,
      "Dark" => ElementTheme.Dark,
      _ => ElementTheme.Default
    };
  }
  private void SetApplicationTheme(ElementTheme theme)
  {
    if (isInitialized)
    {
      (App.MainWindow.Content as Grid).RequestedTheme = theme;
      ((App.MainWindow.Content as Grid).Children.First(x => x is NavigationView) as NavigationView).RequestedTheme = theme;
      ApplicationData.Current.LocalSettings.Values["Theme"] = theme.ToString();
    }
  }
}
