using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapleRobots
{
    /// <summary>
    /// WindowCheckVersion.xaml 的互動邏輯
    /// </summary>
    public partial class WindowCheckVersion : Window
    {
        public WindowCheckVersion()
        {
            InitializeComponent();
        }
        private void DownLoadFile()
        {
            lbProgress.Content = "正在更新中..";

            string UName = "anonymous", UPWord = "";
            FtpWebRequest ftpReq;
            //宣告FTP連線
            ftpReq = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://maplerobots.no-ip.org/MapleRoyals/MapleRobots.exe"));
            //取得欲下載檔案的大小(位元)存至 fiesize
            ftpReq.Method = WebRequestMethods.Ftp.GetFileSize;
            //認證
            ftpReq.Credentials = new NetworkCredential(UName, UPWord);
            int filesize = (int)ftpReq.GetResponse().ContentLength;

            //宣告FTP連線
            ftpReq = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://maplerobots.no-ip.org/MapleRoyals/MapleRobots.exe"));
            //下載
            ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
            //認證
            ftpReq.Credentials = new NetworkCredential(UName, UPWord);

            //binaary
            ftpReq.UseBinary = true;

            //支援續傳
            FileInfo fi = new FileInfo(".\\MapleRobots.exe");
            FileStream fs = null;
            //檢測是否已有相同檔名的存在於client端
            if (fi.Exists)
            {
                File.Delete("_MapleRobots.exe");
                File.Move("MapleRobots.exe", "_MapleRobots.exe");
                FileInfo fi2 = new FileInfo(".\\_MapleRobots.exe");
                fi2.Attributes = FileAttributes.Hidden;
            }
            //client端無檔案 則重新建立新檔
            fs = new FileStream(".\\MapleRobots.exe", FileMode.Create);
            //建立ftp連線
            FtpWebResponse ftpResp = (FtpWebResponse)ftpReq.GetResponse();

            bool bfinish = false;
            try
            {
                //取得下載用的stream物件
                //ftpResp.GetResponseStream()--->擷取包含從FTP server傳送之回應資料的資料流
                using (Stream stm = ftpResp.GetResponseStream())
                {
                    //以block方式多批寫入
                    byte[] buff = new byte[5];
                    //讀data
                    int len = 0;

                    while (fs.Length < filesize)
                    {
                        //取得長度
                        len = stm.Read(buff, 0, buff.Length);
                        fs.Write(buff, 0, len);

                        //傳完
                        if (fs.Length == filesize)
                        {
                            //File.Delete("_MapleRobots.exe");
                            lbProgress.Content = "更新已完成";
                            
                        }
                    }

                    fs.Flush();
                    //傳完，bfinish = true
                    //清除資料流的緩衝區
                    bfinish = (fs.Length == filesize);
                    fs.Close();
                    stm.Close();
                }
            }
            catch (WebException we)
            {
                //若未傳完才要觸發 exception
                if (!bfinish)
                    throw we;
            }
            ftpResp.Close();
            Hack.ShowMessageBox("更新完成，請重啟程式");
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int nowVersion = 0;
            using (var conn = new SqlConnection("Server=tcp:MapleRobots.no-ip.org,1433;Database=MapleRobots;User ID=sa;Password=753951;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"))
            {
                var cmd = conn.CreateCommand();
                try
                {
                    conn.Open();
                }
                catch
                {
                    Hack.ShowMessageBox("無法連接伺服器");
                    this.Close();
                }
                try
                {
                    cmd.CommandText = @"
                    SELECT NowVersion
                    FROM dbo.RobotsVersion";

                    nowVersion = (int)cmd.ExecuteScalar();
                    
                }
                catch
                {
                    
                }
            }
            
            if (nowVersion == 1)
            {
                File.Delete("_MapleRobots.exe");
                MainWindow mainwindow = new MainWindow();
                mainwindow.Show();
                Close();
            }
            else
                DownLoadFile();
        }
    }
}
