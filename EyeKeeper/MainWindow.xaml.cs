using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EyeKeeper;
public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();
  }

  private void navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {

  }

  private void contentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
  {

  }
}
