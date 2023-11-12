using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Forms;

namespace BingDesk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static private string backgroundName = "nan";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)sender;
            w.Left = Screen.PrimaryScreen.Bounds.Width-this.Width;

            try
            {
                CopyRightData.Text = "必应图片正在下载";
                BingWallPaperClient client = new BingWallPaperClient();
                await client.DownloadAsync();

                //BingPhoto.Source = client.WPFPhotoOfTheDay;

                CopyRightData.Text = client.CoppyRightData;
                backgroundName = client.BackGroundName;
                SaveBackground(client.WFPhotoOfTheDay);
                SetBackground();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static string GetBackgroundImagePath()
        {
            string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Bing Backgrounds", DateTime.Now.Year.ToString());
            System.IO.Directory.CreateDirectory(directory);
            backgroundName=backgroundName.Replace("/","-");
            backgroundName=backgroundName.Replace("(","-");
            backgroundName=backgroundName.Replace(")","-");
            return System.IO.Path.Combine(directory, DateTime.Now.ToString("M-d-yyyy")+ backgroundName + ".bmp");
        }

        private static void SaveBackground(System.Drawing.Image background)
        {
            Console.WriteLine("Saving background...");
            background.Save(GetBackgroundImagePath(), System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private static void SetBackground()
        {
            const int SetDesktopBackground = 20;
            const int UpdateIniFile = 1;
            const int SendWindowsIniChange = 2;
            NativeMethods.SystemParametersInfo(SetDesktopBackground, 0, GetBackgroundImagePath(), UpdateIniFile | SendWindowsIniChange);
        }

        internal sealed class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            internal static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.BingDeskNotifyIcon.Dispose();
            this.Close();
        }
    }
}
