using System;
using System.IO;
using System.Windows.Forms;
using AttendanceSystem.Helpers;

namespace AttendanceSystem.Forms
{
    public partial class MainForm : Form
    {
        private readonly string _dataPath;
        private ExcelHelper _excelHelper;

        public MainForm()
        {
            InitializeComponent();

            // 设置数据文件路径
            _dataPath = Path.Combine(Application.StartupPath, "data.xlsx");
            _excelHelper = new ExcelHelper(_dataPath);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 可以在这里初始化默认页面
        }

        private void studentManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm(new StudentManagerForm(_excelHelper));
        }

        private void attendanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm(new AttendanceForm(_excelHelper));
        }

        private void interactionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm(new ScoreForm(_excelHelper));
        }

        private void exportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel文件|*.xlsx";
                saveFileDialog.Title = "导出数据";
                saveFileDialog.FileName = "data_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _excelHelper.BackupExcel(saveFileDialog.FileName);
                        MessageBox.Show("数据导出成功！", "导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OpenForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
        }
    }
} 