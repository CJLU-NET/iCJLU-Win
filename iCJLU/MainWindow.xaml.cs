using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = HandyControl.Controls.MessageBox;
using iCJLU.Utils;

namespace iCJLU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            User.Text = ConfigurationManager.AppSettings["User"];
            Pass.Password = ConfigurationManager.AppSettings["Pass"];

            CheckLogin();

        }
        private async void CheckLogin()
        {
            string ssid = Wifi.getWlanName();
            if (ssid == "iCJLU" || ssid == "iCJLU2" || Helper.Ping("10.253.0.100"))
            {
                bool success = await Srun.isLogin();
                if (success)
                {
                    Btn_Login.IsEnabled = false;
                    Btn_Login.Content = "退出登录";
                }
            }
        }
        private async void Btn_Login_Click(object sender, RoutedEventArgs e)
        {

            if (User.Text == "")
                Pass.Focus();
            else if (Pass.Password == "")
                Pass.Focus();

            else
            {
                if (Btn_Login.Content == "退出登录")
                {
                    Btn_Login.IsEnabled = false;
                    await Srun.Logout(User.Text);
                    MessageBox.Show("退出登录成功", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Btn_Login.IsEnabled = true;
                    Btn_Login.Content = "登录";
                } else
                {
                    Btn_Login.IsEnabled = false;
                    Btn_Login.Content = "登录中";

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["User"].Value = User.Text;
                    config.AppSettings.Settings["Pass"].Value = Pass.Password;

                    config.Save();

                    string ssid = Wifi.getWlanName();
                    if (string.IsNullOrEmpty(ssid) || (ssid != "iCJLU" && ssid != "iCJLU2"))
                    {
                        MessageBox.Show("未连接至教学区WiFi", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Btn_Login.IsEnabled = true;
                        Btn_Login.Content = "登录";

                    }
                    else if (!Helper.Ping("10.253.0.100"))
                    {
                        MessageBox.Show("未连接至教学区WiFi", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Btn_Login.IsEnabled = true;
                        Btn_Login.Content = "登录";
                    }
                    else
                    {

                        bool success = await Srun.Login(User.Text, Pass.Password);
                        if (success)
                        {
                            MessageBox.Show("登录成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            Btn_Login.Content = "退出登录";
                            Btn_Login.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("认证失败", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Btn_Login.IsEnabled = true;
                            Btn_Login.Content = "登录";
                        }
                    }
                }
                
            }


        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;

            if (link.NavigateUri.AbsoluteUri == "self://donate/")
            {
                DonateWindow window = new DonateWindow();
                window.Show();
            }
            else if (link.NavigateUri.AbsoluteUri == "self://about/")
            {
                AboutWindow window = new AboutWindow();
                window.Show();
            }
            else
            {
                Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri) { UseShellExecute = true });
            }

        }
    }
}
