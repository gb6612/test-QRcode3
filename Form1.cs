using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using ZXing; // for decoding QR code
using ZXing.QrCode;

using AForge.Video;
using AForge.Video.DirectShow;

namespace test_QRcode3
{
    public partial class Form1 : Form
    {
        private Stopwatch stopWatch = null;
        private BarcodeFormat myBarcodeFormat = BarcodeFormat.QR_CODE;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            barcodeformat_comboBox.Items.Add(BarcodeFormat.QR_CODE);
            barcodeformat_comboBox.Items.Add(BarcodeFormat.DATA_MATRIX);
            barcodeformat_comboBox.Items.Add(BarcodeFormat.AZTEC);
            barcodeformat_comboBox.Items.Add(BarcodeFormat.UPC_A);
            barcodeformat_comboBox.Text = BarcodeFormat.QR_CODE.ToString();
        }

        public void LogMessage(string msg)
        {
            richTextBox1.AppendText(DateTime.Now.ToString("HH:mm:ss ") + msg + "\n");
            // set the current caret position to the end
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            richTextBox1.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            stopWatch = null;

            // start timer
            timer1.Start();

            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                videoSourcePlayer.WaitForStop();

                // wait ~ 3 seconds
                //for (int i = 0; i < 30; i++)
                //{
                //    if (!videoSourcePlayer.IsRunning)
                //        break;
                //    System.Threading.Thread.Sleep(100);
                //}

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }


        // On timer event - gather statistics
        private void timer1_Tick(object sender, EventArgs e)
        {
            IVideoSource videoSource = videoSourcePlayer.VideoSource;

            if (videoSource != null)
            {
                // Try decoding QR
                var qrCodeBitmap = (Bitmap)videoSourcePlayer.GetCurrentVideoFrame();
                var qrCodeReader = new BarcodeReader();
                if (qrCodeBitmap != null)
                {
                    var qrCodeResult = qrCodeReader.Decode(qrCodeBitmap);
                    if (qrCodeResult != null)
                    {
                        // success
                        // update text only if different from previous
                        if (qrCodeResult.Text != textBox1.Text)
                        {
                            textBox1.Text = qrCodeResult.Text;
                            LogMessage(textBox1.Text);
                        }
                    }
                    else
                    {
                        // Fail
                    }
                    qrCodeBitmap.Dispose();
                }


                // get number of frames since the last timer tick
                int framesReceived = videoSource.FramesReceived;

                if (stopWatch == null)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }
                else
                {
                    stopWatch.Stop();

                    float fps = 1000.0f * framesReceived / stopWatch.ElapsedMilliseconds;
                    fpsLabel.Text = fps.ToString("F2") + " fps";

                    stopWatch.Reset();
                    stopWatch.Start();
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseCurrentVideoSource();
        }


        private void create_button_Click(object sender, EventArgs e)
        {
            do {

                if (create_textBox.Text == "") { MessageBox.Show("write some text to encode"); break; }
                if (myBarcodeFormat == BarcodeFormat.UPC_A) {
                    if (create_textBox.Text.Length < 12)
                        create_textBox.Text = create_textBox.Text.PadLeft(12, '0');
                    else if (create_textBox.Text.Length > 12)
                        create_textBox.Text = create_textBox.Text.Substring(0,12);
                }

                var qrcodeWriter = new BarcodeWriter();

                qrcodeWriter.Format = myBarcodeFormat; //BarcodeFormat.QR_CODE;
                qrcodeWriter.Options.Width = (int)width_qr.Value;
                qrcodeWriter.Options.Height = (int)height_qr.Value;
                //qrcodeWriter.ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
                //EncodeHintType.ERROR_CORRECTION  ???

                pictureBox1.Image = qrcodeWriter.Write(create_textBox.Text);

                if (create_and_save.Checked)
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        qrcodeWriter.Write(create_textBox.Text).Save(saveFileDialog1.FileName);
                    }
                }

            } while (false);
        }

        private void barcodeformat_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (barcodeformat_comboBox.Text == BarcodeFormat.QR_CODE.ToString()) myBarcodeFormat = BarcodeFormat.QR_CODE;
            else if (barcodeformat_comboBox.Text == BarcodeFormat.DATA_MATRIX.ToString()) myBarcodeFormat = BarcodeFormat.DATA_MATRIX;
            else if (barcodeformat_comboBox.Text == BarcodeFormat.AZTEC.ToString()) myBarcodeFormat = BarcodeFormat.AZTEC;
            else if (barcodeformat_comboBox.Text == BarcodeFormat.UPC_A.ToString()) myBarcodeFormat = BarcodeFormat.UPC_A;
        }
    }
}
