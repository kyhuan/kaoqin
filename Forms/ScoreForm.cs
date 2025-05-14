using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AttendanceSystem.Helpers;
using AttendanceSystem.Models;

namespace AttendanceSystem.Forms
{
    public partial class ScoreForm : Form
    {
        private readonly ExcelHelper _excelHelper;
        private readonly Random _random = new Random();
        private List<Student> _allStudents;
        private List<Student> _notCalledStudents;

        public ScoreForm(ExcelHelper excelHelper)
        {
            InitializeComponent();
            _excelHelper = excelHelper;
        }

        private void ScoreForm_Load(object sender, EventArgs e)
        {
            RefreshAllStudents();
            ClearSelection();
        }

        private void RefreshAllStudents()
        {
            _allStudents = _excelHelper.GetAllStudents();
            _notCalledStudents = new List<Student>(_allStudents);
            
            // 刷新学生列表到ListBox
            listBoxSearchResults.DataSource = null;
            listBoxSearchResults.DisplayMember = "ToString";
            listBoxSearchResults.ValueMember = "StudentId";
        }

        #region 随机点名
        private void btnRandomCall_Click(object sender, EventArgs e)
        {
            if (_notCalledStudents.Count == 0)
            {
                // 如果所有学生都已经被点过，则重置列表
                _notCalledStudents = new List<Student>(_allStudents);
                if (_notCalledStudents.Count == 0)
                {
                    MessageBox.Show("没有学生可以点名！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // 随机选一个学生
            int index = _random.Next(_notCalledStudents.Count);
            Student selectedStudent = _notCalledStudents[index];
            
            // 从未点名列表中移除
            _notCalledStudents.RemoveAt(index);
            
            // 显示结果
            lblRandomResult.Text = $"点到：{selectedStudent.Name} ({selectedStudent.StudentId})";
            
            // 自动填充评分区域
            txtScoreStudentId.Text = selectedStudent.StudentId;
            txtScoreName.Text = selectedStudent.Name;
        }
        #endregion

        #region 模糊搜索
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                listBoxSearchResults.DataSource = null;
                return;
            }

            var results = _allStudents.Where(s => 
                s.StudentId.ToLower().Contains(searchText) || 
                s.Name.ToLower().Contains(searchText) ||
                s.ClassName.ToLower().Contains(searchText)
            ).ToList();

            listBoxSearchResults.DataSource = results;
        }

        private void listBoxSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedItem is Student student)
            {
                txtScoreStudentId.Text = student.StudentId;
                txtScoreName.Text = student.Name;
                lblRandomResult.Text = $"当前选择：{student.Name} ({student.StudentId})";
            }
        }
        #endregion

        #region 评分记录
        private void btnSaveScore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtScoreStudentId.Text) ||
                string.IsNullOrWhiteSpace(txtScoreName.Text) ||
                string.IsNullOrWhiteSpace(txtScoreValue.Text) ||
                string.IsNullOrWhiteSpace(txtRemark.Text))
            {
                MessageBox.Show("请填写所有评分信息！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证分数是否为整数
            if (!int.TryParse(txtScoreValue.Text, out int scoreValue))
            {
                MessageBox.Show("分数必须是整数！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var score = new Score
                {
                    Date = DateTime.Today,
                    StudentId = txtScoreStudentId.Text.Trim(),
                    Name = txtScoreName.Text.Trim(),
                    ScoreValue = scoreValue,
                    Remark = txtRemark.Text.Trim()
                };

                _excelHelper.AddScore(score);
                MessageBox.Show("评分记录已保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 清空输入框
                ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearSelection()
        {
            txtScoreStudentId.Clear();
            txtScoreName.Clear();
            txtScoreValue.Clear();
            txtRemark.Clear();
            lblRandomResult.Text = "点名结果显示区";
            listBoxSearchResults.DataSource = null;
            txtSearch.Clear();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearSelection();
        }
        #endregion

        private void btnViewScores_Click(object sender, EventArgs e)
        {
            string studentId = txtScoreStudentId.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                var todayScores = _excelHelper.GetScoresByDate(DateTime.Today);
                if (todayScores.Count == 0)
                {
                    MessageBox.Show("今天还没有评分记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // 显示今天的所有评分
                dataGridViewScores.DataSource = todayScores;
            }
            else
            {
                // 显示该学生的所有评分
                var studentScores = _excelHelper.GetScoresByStudent(studentId);
                if (studentScores.Count == 0)
                {
                    MessageBox.Show("该学生还没有评分记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                dataGridViewScores.DataSource = studentScores;
            }
        }
    }
} 