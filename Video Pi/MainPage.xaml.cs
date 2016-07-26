using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Video_Pi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            MainMenuFrame.Navigate(typeof(Views.MainMenuHome));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void IconsListBox_SelctionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainMenuFrame != null)
            {
                if (HomeListBoxItem.IsSelected) MainMenuFrame.Navigate(typeof(Views.MainMenuHome));
                else if (NewsListBoxItem.IsSelected) MainMenuFrame.Navigate(typeof(Views.MainMenuNews));
                else if (FeedbackListBoxItem.IsSelected) MainMenuFrame.Navigate(typeof(Views.MainMenuFeedback));
                else if (SettingsListBoxItem.IsSelected) MainMenuFrame.Navigate(typeof(Views.MainMenuSettings));
            }
        }
    }
}
