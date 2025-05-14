using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AttendanceSystem.Helpers;
using AttendanceSystem.Models;

namespace AttendanceSystem.Forms
{
    public partial class AttendanceForm : Form
    {
        private readonly ExcelHelper _excelHelper;
        private DateTime _currentDate;
        private List<Student> _allStudents;
        private CameraHelper _cameraHelper;
        private bool _isCameraActive = false;

        public AttendanceForm(ExcelHelper excelHelper)
        {
            InitializeComponent();
            _excelHelper = excelHelper;
            _currentDate = DateTime.Today;
            
            // 初始化学生列表
            _allStudents = new List<Student>();
        }

        private void AttendanceForm_Load(object sender, EventArgs e)
        {
            // 初始化学生列表
            RefreshAllStudents();
            
            // 设置日期选择器的值（这会触发ValueChanged事件）
            dateTimePicker1.Value = _currentDate;
            
            // 更新签到状态（初次加载时）
            UpdateAttendanceStatus();

            // 初始化摄像头
            InitializeCamera();

            // 加载摄像头设备
            LoadCameraDevices();
        }

        private void AttendanceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 确保在窗体关闭时停止摄像头
            if (_cameraHelper != null)
            {
                _cameraHelper.StopCamera();
            }
        }

        private void InitializeCamera()
        {
            _cameraHelper = new CameraHelper(pictureBoxCamera, OnQRCodeDetected);
        }

        private void LoadCameraDevices()
        {
            string[] devices = _cameraHelper.GetCameraDevices();
            if (devices != null && devices.Length > 0)
            {
                comboBoxCameras.Items.Clear();
                comboBoxCameras.Items.AddRange(devices);
                comboBoxCameras.SelectedIndex = 0;
                btnStartCamera.Enabled = true;
            }
            else
            {
                comboBoxCameras.Items.Add("没有找到摄像头设备");
                comboBoxCameras.SelectedIndex = 0;
                btnStartCamera.Enabled = false;
            }
        }

        private void OnQRCodeDetected(string qrCode)
        {
            // 在UI线程中处理
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnQRCodeDetected), qrCode);
                return;
            }

            // 防止重复扫描同一个码
            txtStudentId.Text = qrCode;
            
            // 自动触发签到
            btnAttendance_Click(this, EventArgs.Empty);
        }

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            if (!_isCameraActive)
            {
                int selectedIndex = comboBoxCameras.SelectedIndex;
                if (_cameraHelper.StartCamera(selectedIndex))
                {
                    _isCameraActive = true;
                    btnStartCamera.Text = "停止摄像头";
                    lblCameraStatus.Text = "摄像头状态: 正在扫描";
                }
                else
                {
                    MessageBox.Show("无法启动摄像头，请检查设备是否连接正常。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _cameraHelper.StopCamera();
                _isCameraActive = false;
                btnStartCamera.Text = "启动摄像头";
                lblCameraStatus.Text = "摄像头状态: 已停止";
                
                // 清空预览框
                if (pictureBoxCamera.Image != null)
                {
                    pictureBoxCamera.Image.Dispose();
                    pictureBoxCamera.Image = null;
                }
            }
        }

        private void btnExportQRCodes_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择导出二维码的文件夹";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    QRCodeHelper.ExportAllStudentQRCodes(_allStudents, folderPath);
                }
            }
        }

        private void RefreshAllStudents()
        {
            _allStudents = _excelHelper.GetAllStudents() ?? new List<Student>();
        }

        private void UpdateAttendanceStatus()
        {
            // 确保学生列表已初始化
            if (_allStudents == null)
            {
                RefreshAllStudents();
            }

            // 获取当天已签到学生列表
            var attendances = _excelHelper.GetAttendanceByDate(_currentDate);
            
            // 更新已签到学生表
            dataGridViewAttended.DataSource = attendances;

            // 更新未签到学生表
            var attendedIds = attendances?.Select(a => a.StudentId)?.ToList() ?? new List<string>();
            var notAttendedStudents = _allStudents.Where(s => !attendedIds.Contains(s.StudentId)).ToList();
            dataGridViewNotAttended.DataSource = notAttendedStudents;
            
            // 显示签到统计信息
            labelAttendanceInfo.Text = $"共 {_allStudents.Count} 人，已签到 {attendances?.Count ?? 0} 人，未签到 {notAttendedStudents.Count} 人";
        }

        private void btnAttendance_Click(object sender, EventArgs e)
        {
            string studentId = txtStudentId.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("请输入学号或扫描二维码！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查学生是否存在
            var student = _excelHelper.GetStudentById(studentId);
            if (student == null)
            {
                MessageBox.Show("该学生不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 检查是否已经签到
            if (_excelHelper.IsStudentAttended(studentId, _currentDate))
            {
                MessageBox.Show("该学生今天已经签到了！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 添加签到记录
            try
            {
                var attendance = new Attendance
                {
                    Date = _currentDate,
                    StudentId = student.StudentId,
                    Name = student.Name,
                    AttendanceTime = DateTime.Now
                };

                _excelHelper.AddAttendance(attendance);
                MessageBox.Show("签到成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 清空输入框
                txtStudentId.Clear();
                
                // 刷新列表
                UpdateAttendanceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("签到失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            _currentDate = dateTimePicker1.Value.Date;
            UpdateAttendanceStatus();
        }
    }
} 