using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using static System.Net.Http.HttpClient;


namespace dwonvid
{
    public partial class DownVid : Form
    {
        public DownVid()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.BackColor= Color.Green;
            var youtube = new YoutubeClient();
            var VideoUrl = textBox1.Text;
            var palce = textBox2.Text;
            
            //Ensure that url and place of saving is not null
            if (string.IsNullOrWhiteSpace(VideoUrl) || string.IsNullOrWhiteSpace(palce))
            {
                button1.BackColor = Color.IndianRed;

                MessageBox.Show("Please enter both the video URL and the save path.");
                button1.BackColor = Color.Gray;

                return;
            }
            //get the info aboute video
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(VideoUrl);
            //choose the highst quality from the manifest file 
            
            
           var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
           //save video in the path with the type
            var savePath = Path.Combine(palce, $"video{RandomString(12)}.{streamInfo.Container}");
            
            await DownloadVideoWithProgressAsync(streamInfo, savePath);
            button1.Enabled = true;
        }




        private async Task DownloadVideoWithProgressAsync(IStreamInfo streamInfo, string savePath)
        {
            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(streamInfo.Url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var contentLength = response.Content.Headers.ContentLength ?? 0;
                progressBar1.Maximum = 100;
                progressBar1.Value = 0;

                var inputStream = await response.Content.ReadAsStreamAsync();
                using (var outputStream = File.Create(savePath))
                {
                    var buffer = new byte[81920];
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        int progress = (int)((totalBytesRead * 100) / contentLength);
                        progressBar1.Value = progress;
                    }
                }
            }

            MessageBox.Show("Video downloaded successfully!");
            this.Close();

        }


    }
}
