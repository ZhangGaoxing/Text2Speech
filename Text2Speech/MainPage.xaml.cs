using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Text2Speech.Managers;
using Text2Speech.Views;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Text2Speech
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Style transparent = (Style)Application.Current.Resources["TransparentDialog"];

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await SetBarAndStyle();
            RootFrame.Navigate(typeof(ConvertPage));   
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            await MainGrid.Blur(10, 100, 0).StartAsync();

            var contentDialog = new ContentDialog()
            {
                Content = new AboutPage(),
                PrimaryButtonText = "确定",
                FullSizeDesired = true
            };

            contentDialog.Style = transparent;

            contentDialog.Closing += async (_s, _e) =>
            {
                await MainGrid.Blur(0, 0, 0).StartAsync();
            };

            contentDialog.PrimaryButtonClick += async (_s, _e) =>
            {
                await MainGrid.Blur(0, 0, 0).StartAsync();
                contentDialog.Hide();
            };

            await contentDialog.ShowAsync();
        }

        private async void AboutMe_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialog()
            {
                Content = new AboutMePage(),
                PrimaryButtonText = "确定",
                FullSizeDesired = true
            };

            contentDialog.Style = transparent;

            contentDialog.Closing += async (_s, _e) =>
            {
                await MainGrid.Blur(0, 0, 0).StartAsync();
            };

            contentDialog.PrimaryButtonClick += async (_s, _e) =>
            {
                await MainGrid.Blur(0, 0, 0).StartAsync();
                contentDialog.Hide();
            };

            await MainGrid.Blur(10, 100, 0).StartAsync();

            await contentDialog.ShowAsync();
        }

        public async Task SetBarAndStyle()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            if (DeviceInfoManager.GetOsVersion() >= 16299)
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
                {
                    Windows.UI.Xaml.Media.AcrylicBrush myBrush = new Windows.UI.Xaml.Media.AcrylicBrush();
                    myBrush.BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop;
                    myBrush.FallbackColor = Colors.Transparent;
                    myBrush.TintColor = Color.FromArgb(255, 15, 15, 15);
                    myBrush.TintOpacity = 0.3;

                    MainGrid.Background = myBrush;
                    BarGrid.Background = myBrush;

                    Style appbar = (Style)Application.Current.Resources["AppBarButtonRevealStyle"];

                    AboutButton.Style = appbar;
                }
            }

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.Gray;
        }
    }
}
