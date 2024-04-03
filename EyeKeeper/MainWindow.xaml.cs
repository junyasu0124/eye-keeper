using EyeKeeper.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EyeKeeper;
public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

  private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
  }

  private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
  {

  }
}
