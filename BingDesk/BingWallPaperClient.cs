using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace BingDesk
{

    class BingWallPaperClient
    {
        private readonly string _feed;
        private readonly string _tempfilename;
        private readonly string _tempcoppyright;
        private bool _loadcalled;


        /// <summary>
        /// Creates a new instance of the Bing photo downloader
        /// </summary>
        public BingWallPaperClient()
        {
            var tempdir = Environment.ExpandEnvironmentVariables("%temp%");
            _tempfilename = Path.Combine(tempdir, "bingphotooftheday.jpg");
            _tempcoppyright = Path.Combine(tempdir, "bingphotooftheday.txt");
            _loadcalled = false;

            //photo of the day data in xml format
            _feed = "http://www.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=1&mkt=zh-CN";
        }

        /// <summary>
        /// Downloades the photo of the day syncronously
        /// </summary>
        public void DownLoad()
        {
            bool downloadneeded = true;
            if (File.Exists(_tempfilename))
            {
                FileInfo fi = new FileInfo(_tempfilename);
                if ((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalHours < 24)
                {
                    downloadneeded = false;
                }
            }

            if (File.Exists(_tempcoppyright))
            {
                CoppyRightData = File.ReadAllText(_tempcoppyright);
                downloadneeded = false;
            }
            else downloadneeded = true;

            downloadneeded = true;

            _loadcalled = true;
            if (!downloadneeded) return;

            bool downloadOk = false;
            XElement document;
            while (!downloadOk)
            {
                try
                {
                    document = XDocument.Load(_feed).Elements().Elements().FirstOrDefault();
                    var url = (from i in document.Elements()
                               where i.Name == "url"
                               select i.Value.ToString()).FirstOrDefault();

                    var imgurl = "http://www.bing.com" + url;

                    var copyright = (from i in document.Elements()
                                     where i.Name == "copyright"
                                     select i.Value.ToString()).FirstOrDefault();
                    var cplink = (from i in document.Elements()
                                  where i.Name == "copyrightlink"
                                  select i.Value.ToString()).FirstOrDefault();
                    BackGroundName = copyright;
                    CoppyRightData = copyright + "\r\n" + cplink;
                    File.WriteAllText(_tempcoppyright, CoppyRightData);

                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(imgurl, _tempfilename);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Error新野");
                    }
                    downloadOk = true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(3000);
                }

            }

        }

        /// <summary>
        /// Asyncronous & awaitable version of the download routine
        /// </summary>
        /// <returns>An awaitable task</returns>
        public Task DownloadAsync()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send("baidu.com", timeout, buffer, options);
            while (reply.Status != IPStatus.Success)
            {
                reply = pingSender.Send("baidu.com", timeout, buffer, options);
                Thread.Sleep(3000);
            }
            return Task.Run(() =>
        {
            DownLoad();
        });
        }

        /// <summary>
        /// Gets the Photo of the day in a WPF compiliant ImageSource
        /// </summary>
        public ImageSource WPFPhotoOfTheDay
        {
            get
            {
                if (!_loadcalled) throw new InvalidOperationException("Call the DownLoad() methood first");
                return new BitmapImage(new Uri(_tempfilename));
            }
        }
        /// <summary>
        /// Gets the Photo of the day in a Windows Forms compiliant ImageSource
        /// </summary>
        public Bitmap WFPhotoOfTheDay
        {
            get
            {
                if (!_loadcalled) throw new InvalidOperationException("Call the DownLoad() methood first");
                return new Bitmap(_tempfilename);
            }
        }

        /// <summary>
        /// CoppyRight data information
        /// </summary>
        public string CoppyRightData
        {
            get;
            private set;
        }

        /// <summary>
        /// CoppyRight data information
        /// </summary>
        public string BackGroundName
        {
            get;
            private set;
        }

    }
}
