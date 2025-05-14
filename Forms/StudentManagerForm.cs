using System;
using System.Windows.Forms;
using AttendanceSystem.Helpers;
using AttendanceSystem.Models;

namespace AttendanceSystem.Forms
{
    public partial class StudentManagerForm : Form
    {
        private readonly ExcelHelper _excelHelper;

        public StudentManagerForm(ExcelHelper excelHelper)
        {
            InitializeComponent();
            _excelHelper = excelHelper;
        }

        private void StudentManagerForm_Load(object sender, EventArgs e)
        {
            RefreshStudentList();
        }

        private void RefreshStudentList()
        {
            var students = _excelHelper.GetAllStudents();
            dataGridViewStudents.DataSource = null;
            dataGridViewStudents.DataSource = students;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentId.Text) || 
                string.IsNullOrWhiteSpace(txtName.Text) || 
                string.IsNullOrWhiteSpace(txtClassName.Text))
            {
                MessageBox.Show("请填写所有学生信息！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查学号是否已存在
            if (_excelHelper.GetStudentById(txtStudentId.Text) != null)
            {
                MessageBox.Show("该学号已存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var student = new Student
                {
                    StudentId = txtStudentId.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    ClassName = txtClassName.Text.Trim()
                };

                _excelHelper.AddStudent(student);
                MessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 清空输入框
                txtStudentId.Clear();
                txtName.Clear();
                txtClassName.Clear();
                
                // 刷新列表
                RefreshStudentList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一个学生！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtStudentId.Text) || 
                string.IsNullOrWhiteSpace(txtName.Text) || 
                string.IsNullOrWhiteSpace(txtClassName.Text))
            {
                MessageBox.Show("请填写所有学生信息！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var student = new Student
                {
                    StudentId = txtStudentId.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    ClassName = txtClassName.Text.Trim()
                };

                _excelHelper.UpdateStudent(student);
                MessageBox.Show("更新成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 刷新列表
                RefreshStudentList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一个学生！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string studentId = txtStudentId.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("请先选择一个学生！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除学号为 " + studentId + " 的学生吗？", "确认", 
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _excelHelper.DeleteStudent(studentId);
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 清空输入框
                    txtStudentId.Clear();
                    txtName.Clear();
                    txtClassName.Clear();
                    
                    // 刷新列表
                    RefreshStudentList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("删除失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewStudents.SelectedRows[0];
                txtStudentId.Text = selectedRow.Cells["StudentId"].Value.ToString();
                txtName.Text = selectedRow.Cells["Name"].Value.ToString();
                txtClassName.Text = selectedRow.Cells["ClassName"].Value.ToString();
                
                // 学号一旦选定就不能更改，只能更改其他信息
                txtStudentId.ReadOnly = true;
            }
            else
            {
                txtStudentId.ReadOnly = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtStudentId.Clear();
            txtName.Clear();
            txtClassName.Clear();
            txtStudentId.ReadOnly = false;
            dataGridViewStudents.ClearSelection();
        }
    }
} 