using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESBasic;
using Oraycn.MCapture;
using Oraycn.MFile;

namespace RecordVideo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void 录制视频ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int audioSampleRate = 16000;
                int channelCount = 1;
                System.Drawing.Size videoSize = Screen.PrimaryScreen.Bounds.Size;

                {
                    videoSize = new System.Drawing.Size(640, 480);
                    this.cameraCapturer = CapturerFactory.CreateCameraCapturer(0, videoSize, frameRate);
                    this.cameraCapturer.ImageCaptured += new CbGeneric<Bitmap>(this.Form1_ImageCaptured);
                }


                {
                    this.microphoneCapturer = CapturerFactory.CreateMicrophoneCapturer(0);
                    this.microphoneCapturer.CaptureError += new CbGeneric<Exception>(this.CaptureError);
                }

                this.microphoneCapturer.AudioCaptured += audioMixter_AudioMixed;

                this.microphoneCapturer.Start();

                this.cameraCapturer.Start();

                this.sizeRevised = (videoSize.Width % 4 != 0) || (videoSize.Height % 4 != 0);
                if (this.sizeRevised)
                {
                    videoSize = new System.Drawing.Size(videoSize.Width / 4 * 4, videoSize.Height / 4 * 4);
                }

                this.videoFileMaker = new VideoFileMaker();
                this.videoFileMaker.Initialize(video_save_path, VideoCodecType.H264, videoSize.Width, videoSize.Height, frameRate, VideoQuality.High, AudioCodecType.AAC, audioSampleRate, channelCount, true);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void 停止录制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.microphoneCapturer.Stop();
            this.cameraCapturer.Stop();
            videoFileMaker.Close(true);
            MessageBox.Show("录制完成！");
        }

        void CaptureError(Exception obj)
        {

        }

        void Form1_ImageCaptured(Bitmap img)
        {
            Bitmap imgRecorded = img;
            if (this.sizeRevised) // 对图像进行裁剪，  MFile要求录制的视频帧的长和宽必须是4的整数倍。
            {
                imgRecorded = ESBasic.Helpers.ImageHelper.RoundSizeByNumber(img, 4);
                img.Dispose();
            }
            this.pictureBox1.Image = imgRecorded;
            this.pictureBox1.Refresh();
            this.videoFileMaker.AddVideoFrame(imgRecorded);
        }

        void audioMixter_AudioMixed(byte[] audioData)
        {
            this.videoFileMaker.AddAudioFrame(audioData);
        }
        private string video_save_path = "test.mp4";
        private IMicrophoneCapturer microphoneCapturer;
        private ICameraCapturer cameraCapturer;
        private VideoFileMaker videoFileMaker;
        private int frameRate = 15; // 采集视频的帧频
        private bool sizeRevised = false;// 是否需要将图像帧的长宽裁剪为4的整数倍
    }
}
