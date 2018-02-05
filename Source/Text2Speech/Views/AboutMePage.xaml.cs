using Microsoft.Toolkit.Uwp.UI.Animations;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Text2Speech.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutMePage : Page
    {
        public AboutMePage()
        {
            this.InitializeComponent();
        }

        private void ZhifubaoConfirm_Click(object sender, RoutedEventArgs e)
        {
            Zhifubao.Hide();
        }

        private void WeixinConfirm_Click(object sender, RoutedEventArgs e)
        {
            Weixin.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FlyoutBase.ShowAttachedFlyout(element);
        }

        private async void BackgroundImage_Loading(FrameworkElement sender, object args)
        {
            await BackgroundImage.Blur(15, 0, 0).StartAsync();
        }
    }
}
