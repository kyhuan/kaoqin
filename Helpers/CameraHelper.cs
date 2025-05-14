using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AttendanceSystem.Helpers
{
    public class CameraHelper
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private PictureBox _displayBox;
        private Action<string> _onQRCodeDetected;
        private Timer _scanTimer;
        
        public CameraHelper(PictureBox displayBox, Action<string> onQRCodeDetected)
        {
            _displayBox = displayBox;
            _onQRCodeDetected = onQRCodeDetected;
            
            // 初始化扫描计时器
            _scanTimer = new Timer();
            _scanTimer.Interval = 500; // 每500毫秒扫描一次
            _scanTimer.Tick += ScanTimer_Tick;
        }
        
        // 获取可用摄像头设备
        public string[] GetCameraDevices()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
                return null;
                
            string[] devices = new string[_videoDevices.Count];
            for (int i = 0; i < _videoDevices.Count; i++)
            {
                devices[i] = _videoDevices[i].Name;
            }
            
            return devices;
        }
        
        // 开始摄像头
        public bool StartCamera(int deviceIndex = 0)
        {
            try
            {
                if (_videoDevices == null || _videoDevices.Count == 0)
                {
                    GetCameraDevices();
                }
                
                if (_videoDevices.Count <= deviceIndex)
                    return false;
                    
                if (_videoSource != null && _videoSource.IsRunning)
                {
                    StopCamera();
                }
                
                _videoSource = new VideoCaptureDevice(_videoDevices[deviceIndex].MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();
                
                // 启动扫描计时器
                _scanTimer.Start();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // 停止摄像头
        public void StopCamera()
        {
            _scanTimer.Stop();
            
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
            }
        }
        
        // 处理新帧
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (_displayBox.InvokeRequired)
            {
                _displayBox.Invoke(new Action<object, NewFrameEventArgs>(VideoSource_NewFrame), sender, eventArgs);
                return;
            }
            
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            
            if (_displayBox.Image != null)
            {
                _displayBox.Image.Dispose();
            }
            
            _displayBox.Image = bitmap;
        }
        
        // 扫描计时器事件
        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (_displayBox.Image == null)
                return;
                
            try
            {
                // 复制当前图像进行扫描
                Bitmap bitmap = (Bitmap)_displayBox.Image.Clone();
                string qrResult = QRCodeHelper.DecodeQRCode(bitmap);
                bitmap.Dispose();
                
                if (!string.IsNullOrEmpty(qrResult))
                {
                    _onQRCodeDetected(qrResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("扫描错误: " + ex.Message);
            }
        }
    }
} 