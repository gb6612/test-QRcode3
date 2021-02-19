QR code in Visual Studio(VS)

Packages needed:
AForge.Net = access video
ZXing = encode/decode barcodes

Create a project under VS
In the solution explorer right-click the project -> manage NuGet packages -> Browse
Search & Install ZXing.Net + AForge

In solution explorer check under References that the following was added:
zxing
zxing.presentation
AForge.Video
AForge.Video.DirectShow
AForge.Video.Controls

In the Toolbox window, check that a AForge.NET form container was added.

In your form, add toolbox AForge.NET - VideoSourcePlayer. (into a Panel?)

In the code add:
using ZXing; // for decoding QR code
using AForge.Video;
using AForge.Video.DirectShow;



To open a video device:
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }

