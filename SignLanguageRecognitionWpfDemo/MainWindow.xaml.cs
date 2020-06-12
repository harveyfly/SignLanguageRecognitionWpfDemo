using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.WPF;
using System.Diagnostics;
using Microsoft.Azure.Kinect.BodyTracking;
using System.Globalization;
using System.Threading;

namespace SignLanguageRecognitionWpfDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Device KinectDevice = null;
        private SLRHttpClient client = null;
        private SysParameter sysParameter = null;
        private PostSkeletonData postSkeletonData = null;
        private RecognitionResult recResult = null;
        private SystemState sysState = null;
        private VisualFrameData visualFrameData = null;
        private SynchronizationContext syncContext = null;

        private utils utils = new utils();

        public MainWindow()
        {
            InitializeComponent();
            this.sysState = new SystemState();
            this.sysParameter = new SysParameter();
            this.recResult = new RecognitionResult();
            this.postSkeletonData = new PostSkeletonData(sysParameter);
            this.visualFrameData = new VisualFrameData();
            
            labResult.DataContext = recResult;
            sldKeyFrameThreshold.DataContext = sysParameter;
            Logger.LogMessage += this.Logger_LogMessage;
        }

        //Kinect Log Message
        private void Logger_LogMessage(LogMessage logMessage)
        {
            if (logMessage.LogLevel < LogLevel.Information)
            {
                this.utils.AddLogInfo(this.rtbLoggerInfo, String.Format("{0} [{1}] {2}@{3}: {4}\r\n", logMessage.Time, logMessage.LogLevel, logMessage.FileName, logMessage.Line, logMessage.Message));
            }
        }
        
        //Window Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Check devices
            int connectDeviceNum = Device.GetInstalledCount();
            List<string> deviceIdList = new List<string>();
            for (int i = 0;i < connectDeviceNum; i++)
            {
                deviceIdList.Add(i.ToString());
            }
            this.cbxDeviceName.ItemsSource = deviceIdList;
            //this.tbxServerSocket.Text = "127.0.0.1:5000";
            this.tbxServerSocket.Text = "47.103.52.232:60504";
        }

        //Wondow Close
        private void Window_Closed(object sender, EventArgs e)
        {
            Logger.LogMessage -= this.Logger_LogMessage;
        }

        //Connect Server
        private void btnConnectServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.client = new SLRHttpClient(this.tbxServerSocket.Text);
                UpdataSysParameter();
            }
            catch(Exception ex)
            {
                rtbLoggerInfo.AppendText(utils.GenLogString("Server Socket Type Error"));
                utils.ShowErrorMsg(ex.Message);
            }
        }

        //Updata system parameter
        private async void UpdataSysParameter()
        {
            try
            {
                this.sysParameter = await this.client.getSysParameterAsync();
                this.postSkeletonData.parameter = sysParameter;
                rtbLoggerInfo.AppendText(utils.GenLogString(this.sysParameter.ToString()));
            }
            catch(Exception ex)
            {
                rtbLoggerInfo.AppendText(utils.GenLogString("Server Connect Error"));
                utils.ShowErrorMsg(ex.Message);
            }
            
        }
        
        //Connect Device
        private void btnConnectDevice_Click(object sender, RoutedEventArgs e)
        {
            int deviceId = this.cbxDeviceName.SelectedIndex;
            if(deviceId < 0)
            {
                utils.ShowErrorMsg("Device is not selected");
                return;
            }
            if (this.sysState.DeviceConnected)
            {
                this.sysState.DeviceConnected = false;
                return;
            }
            OpenKinectDevice(deviceId);
        }

        //Open Kinect device and show videos
        private async void OpenKinectDevice(int deviceId)
        {
            using(this.KinectDevice = Device.Open(deviceId))
            {
                this.sysState.DeviceConnected = true;
                this.KinectDevice.StartCameras(new DeviceConfiguration
                {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R720p,
                    DepthMode = DepthMode.NFOV_Unbinned,
                    SynchronizedImagesOnly = true,
                    WiredSyncMode = WiredSyncMode.Standalone,
                    CameraFPS = FPS.FPS30,
                });

                using (Transformation transform = this.KinectDevice.GetCalibration().CreateTransformation())
                {
                    using(Tracker tracker = Tracker.Create(this.KinectDevice.GetCalibration(), new TrackerConfiguration(){ ProcessingMode = TrackerProcessingMode.Gpu,SensorOrientation = SensorOrientation.Default}))
                    {
                        Stopwatch sw = new Stopwatch();
                        int frameCount = 0;
                        sw.Start();
                        while (this.sysState.DeviceConnected)
                        {
                            if (!Environment.Is64BitProcess)
                            {
                                // In 32-bit the BitmapSource memory runs out quickly and we can hit OutOfMemoryException.
                                // Force garbage collection in each loop iteration to keep memory in check.
                                GC.Collect();
                            }

                            // Wait for a capture on a thread pool thread
                            using (Capture capture = await Task.Run(() => { return this.KinectDevice.GetCapture(); }).ConfigureAwait(true))
                            {
                                //Call show initial image
                                BitmapSource initImageBitmap = await ShowInitImage(capture, transform);
                                this.initImageViewPane.Source = initImageBitmap;
                                //Call show skeleton data image
                                BitmapSource skeletonImageBitmap = await ShowSkeletonImage(capture, tracker);
                                this.skeletonImageViewPane.AddBitmapImage(skeletonImageBitmap);
                                //Call show frames diff image
                                BitmapSource framesDiffImageBitmap = await ShowFramesDiffImage();
                                this.framesDiffViewPane.AddBitmapImage(framesDiffImageBitmap);

                                ++frameCount;

                                if (sw.Elapsed > TimeSpan.FromSeconds(2))
                                {
                                    double framesPerSecond = (double)frameCount / sw.Elapsed.TotalSeconds;
                                    this.labFpstxt.Content = $"FPS: {framesPerSecond:F2}";

                                    frameCount = 0;
                                    sw.Restart();
                                }
                            }
                        }
                    }
                }

            }
        }

        //Show initial image
        private async Task<BitmapSource> ShowInitImage(Capture capture, Transformation transform)
        {
            BitmapSource initImageBitmap = null;
            // Create a BitmapSource for the unmodified color image.
            // Creating the BitmapSource is slow, so do it asynchronously on another thread
            Task<BitmapSource> createColorBitmapTask = new Task<BitmapSource>(() =>
            {
                BitmapSource source = capture.Color.CreateBitmapSource();

                // Allow the bitmap to move threads
                source.Freeze();
                return source;
            });

            // Compute the colorized output bitmap on a thread pool thread
            Task<BitmapSource> createDepthBitmapTask = new Task<BitmapSource>(() =>
            {
                int colorWidth = this.KinectDevice.GetCalibration().ColorCameraCalibration.ResolutionWidth;
                int colorHeight = this.KinectDevice.GetCalibration().ColorCameraCalibration.ResolutionHeight;
                // Allocate image buffers for us to manipulate
                var transformedDepth = new Microsoft.Azure.Kinect.Sensor.Image(ImageFormat.Depth16, colorWidth, colorHeight);
                var outputColorImage = new Microsoft.Azure.Kinect.Sensor.Image(ImageFormat.ColorBGRA32, colorWidth, colorHeight);
                // Transform the depth image to the perspective of the color camera
                transform.DepthImageToColorCamera(capture, transformedDepth);

                // Get Span<T> references to the pixel buffers for fast pixel access.
                Span<ushort> depthBuffer = transformedDepth.GetPixels<ushort>().Span;
                Span<BGRA> colorBuffer = capture.Color.GetPixels<BGRA>().Span;
                Span<BGRA> outputColorBuffer = outputColorImage.GetPixels<BGRA>().Span;

                // Create an output color image with data from the depth image
                for (int i = 0; i < colorBuffer.Length; i++)
                {
                    // The output image will be the same as the input color image,
                    // but colorized with Red where there is no depth data, and Green
                    // where there is depth data at more than 1.5 meters
                    outputColorBuffer[i] = colorBuffer[i];

                    if (depthBuffer[i] == 0)
                    {
                        outputColorBuffer[i].R = 255;
                    }
                    else if (depthBuffer[i] > 1500)
                    {
                        outputColorBuffer[i].G = 255;
                    }
                }

                BitmapSource source = outputColorImage.CreateBitmapSource();

                // Allow the bitmap to move threads
                source.Freeze();

                return source;
            });

            if (this.sysState.DeviceDepthMode)
            {
                createDepthBitmapTask.Start();
                initImageBitmap = await createDepthBitmapTask.ConfigureAwait(false);
            }
            else
            {
                createColorBitmapTask.Start();
                initImageBitmap = await createColorBitmapTask.ConfigureAwait(false);
            }

            return initImageBitmap;
        }

        //Show skeleton image
        private async Task<BitmapSource> ShowSkeletonImage(Capture capture, Tracker tracker)
        {
            tracker.EnqueueCapture(capture);
            // Try getting latest tracker frame.
            using (Microsoft.Azure.Kinect.BodyTracking.Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
            {
                if (frame != null)
                {
                    // Save this frame for visualization in Renderer.

                    // One can access frame data here and extract e.g. tracked bodies from it for the needed purpose.
                    // Instead, for simplicity, we transfer the frame object to the rendering background thread.
                    // This example shows that frame popped from tracker should be disposed. Since here it is used
                    // in a different thread, we use Reference method to prolong the lifetime of the frame object.
                    // For reference on how to read frame data, please take a look at Renderer.NativeWindow_Render().
                    this.visualFrameData.Frame = frame.Reference();
                }
                else
                {
                    return null;
                }
            }
            
            float dpi = MyCanvas.getDpi();
            double Width = skeletonImageViewPane.ActualWidth;
            double Height = skeletonImageViewPane.ActualHeight;

            Task<BitmapSource> DrawSkeletonDataTask = Task.Run(() =>
            {
                var lastFrame = this.visualFrameData.TakeFrameWithOwnership();
                if (lastFrame == null || lastFrame.NumberOfBodies == 0)
                {
                    return null;
                }

                //Get first skeleton
                var skeleton = lastFrame.GetBodySkeleton(0);

                //Update last post skeleton data
                UpdateLastPostSkeletonData(skeleton);

                //Draw Visual
                List<float> jointPosList = new List<float>();
                for (int jointId = 0;jointId < (int)JointId.Count; jointId++)
                {
                    var joint = skeleton.GetJoint(jointId);
                    jointPosList.Add(joint.Position.X);
                    jointPosList.Add(joint.Position.Y);
                }

                float MarginX = 200.0f; float MarginY = 100.0f;
                jointPosList = this.utils.SkeletonDataAbs2Rel(jointPosList, (int)Width, (int)Height, MarginX, MarginY);

                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                {
                    const float radius = 3.0f;

                    var boneColor = Color.FromRgb(255, 255, 255);
                    Brush bursh = new SolidColorBrush(boneColor);
                    Pen whitePen = new Pen(Brushes.White, radius);
                    Pen greenPen = new Pen(Brushes.LightGreen, radius);

                    for (int jointId = 0; jointId < jointPosList.Count / 2; jointId++)
                    {
                        // Render the joint as a ellipse.
                        var jointPoint = new Point(jointPosList[2 * jointId], jointPosList[2 * jointId + 1]);
                        context.DrawEllipse(bursh, whitePen, jointPoint, radius, radius);

                        // Render the bone as a line.
                        if (JointConnections.JointParent.TryGetValue((JointId)jointId, out JointId parentId))
                        {
                            var startPos = new Point(jointPosList[2 * jointId], jointPosList[2 * jointId + 1]);
                            int parId = Convert.ToInt32(parentId);
                            var endPos = new Point(jointPosList[2 * parId], jointPosList[2 * parId + 1]);

                            if(SLRPostJoint.PostJointsList.Contains((JointId)jointId) && SLRPostJoint.PostJointsList.Contains(parentId))
                            {
                                context.DrawLine(greenPen, startPos, endPos);
                            }
                            else
                            {
                                context.DrawLine(whitePen, startPos, endPos);
                            }
                        }
                    }

                }

                //this.utils.UpdataMyCanvasView(skeletonImageViewPane, visual, dpi);
                BitmapSource skeletonBitmapSource = utils.ConvertVisual2BitmapSource(visual, Width, Height, dpi);

                skeletonBitmapSource.Freeze();
                
                return skeletonBitmapSource;
            });

            return await DrawSkeletonDataTask.ConfigureAwait(false);
        }

        //Show frames diff image
        private async Task<BitmapSource> ShowFramesDiffImage()
        {
            float dpi = MyCanvas.getDpi();
            double Width = skeletonImageViewPane.ActualWidth;
            double Height = skeletonImageViewPane.ActualHeight;
            Task<BitmapSource> DrawFramesDiffImageTask = Task.Run(() => { 
                DrawingVisual visual = new DrawingVisual();

                int intWidth = Convert.ToInt32(Width);
                int intHeight = Convert.ToInt32(Height);

                List<KeyValuePair<float, bool>> framesDiffList = this.postSkeletonData.getFrameDiffList();
                float MaxFrameDiff = this.postSkeletonData.MaxFrameDiff;
                //List<KeyValuePair<float, bool>> framesDiffList = utils.TestFramesDiffList;

                using (DrawingContext context = visual.RenderOpen())
                {
                    float LineWidth = (float)Width / this.sysParameter.frame_diff_queue_len;
                    Pen whitePen = new Pen(Brushes.White, LineWidth);
                    Pen greenPen = new Pen(Brushes.LightGreen, LineWidth);
                    for (int i = 0; i < framesDiffList.Count; i++)
                    {
                        var frameDiffPair = framesDiffList[i];
                        float lineHeight = frameDiffPair.Key / MaxFrameDiff * intHeight;
                        var startPoint = new Point(LineWidth * i, intHeight);
                        var endPoint = new Point(LineWidth * i, intHeight - lineHeight);
                        if (frameDiffPair.Value)
                        {
                            context.DrawLine(greenPen, startPoint, endPoint);
                        }
                        else
                        {
                            context.DrawLine(whitePen, startPoint, endPoint);
                        }
                    }
                    FormattedText formattedText = new FormattedText(
                        $"Frames Diff Max: {MaxFrameDiff:F2}",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        12,
                        Brushes.White,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                    context.DrawText(formattedText, new Point(0, 0));
                }

                BitmapSource framesDiffBitmap = this.utils.ConvertVisual2BitmapSource(visual, Width, Height, dpi);
                framesDiffBitmap.Freeze();

                return framesDiffBitmap;
            });

            return await DrawFramesDiffImageTask.ConfigureAwait(false);
        }

        //Update last post skeleton data
        private void UpdateLastPostSkeletonData(Skeleton skeleton)
        {
            List<float> jointPosList = new List<float>();
            foreach (JointId postId in SLRPostJoint.PostJointsList)
            {
                var joint = skeleton.GetJoint(postId);
                jointPosList.Add(joint.Position.X);
                jointPosList.Add(joint.Position.Y);
            }
            //Abs to Rel
            jointPosList = utils.SkeletonDataAbs2Rel(jointPosList, this.sysParameter.crop_size, this.sysParameter.crop_size);
            if (this.postSkeletonData != null)
            {
                this.postSkeletonData.setLastFrameSkeletonData(jointPosList);
                //if (this.postSkeletonData.setLastFrameSkeletonData(jointPosList) == false)
                //{
                //    this.utils.AddLogInfo(rtbLoggerInfo, utils.GenLogString("Data Ready or Skeleton one frame data length error"));
                //}
            }
        }

        //prediction
        private async void Prediction()
        {
            if (this.sysState.RecognitionRunning)
            {
                utils.ShowErrorMsg("Prediction is running, Please stop the last one first!");
                return;
            }
            if(this.postSkeletonData is null)
            {
                utils.ShowErrorMsg("PostSkeletonData is null, please connect server first!");
                return;
            }
            this.sysState.RecognitionRunning = true;

            Task RunningPredictionTask = Task.Run(async () =>
            {
                while (this.sysState.RecognitionRunning)
                {
                    List<float> KeyFramesSkeletonData = this.postSkeletonData.getKeyFramesSkeletonData();
                    //List<float> KeyFramesSkeletonData = utils.TestKeyFrameSkeletonData;
                    if (KeyFramesSkeletonData == null)
                    {
                        //this.utils.AddLogInfo(this.rtbLoggerInfo, utils.GenLogString("KeyFramesSkeletonData is Null"));
                        Thread.Sleep(1000);
                        continue;
                    }
                    PostData postdata = new PostData(this.sysParameter, KeyFramesSkeletonData);
                    try
                    {
                        this.recResult = await this.client.getRecognitionResult(postdata);
                        if (recResult.sucess)
                        {
                            //change display, async binding has some error
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.labResult.Content = recResult.prediction;
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                            this.utils.AddLogInfo(rtbLoggerInfo, utils.GenLogString(this.recResult.ToString()));
                        }
                        else
                        {
                            //this.utils.UpdataResultLabel(labResult, "Unkonw");
                            this.utils.AddLogInfo(rtbLoggerInfo, utils.GenLogString("Prediction Unsucess, Check Server API"));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.utils.AddLogInfo(rtbLoggerInfo, utils.GenLogString("Server Connect Error, Prediction Stop"));
                        this.sysState.RecognitionRunning = false;
                        utils.ShowErrorMsg(ex.Message);
                    }
                    Thread.Sleep(1000);
                }
            });
            await RunningPredictionTask.ConfigureAwait(true);
        }

        //Start Prediction
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            Prediction();
        }

        //Stop Prediction
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.sysState.RecognitionRunning = false;
        }

        //Change InitImage View Mode
        private void btnDepthMode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.sysState.DeviceDepthMode = !this.sysState.DeviceDepthMode;
        }

        private void sldKeyFrameThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.sysParameter.keyFrameThreshold = (float)sldKeyFrameThreshold.Value;
        }
    }
}
