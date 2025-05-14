using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using AttendanceSystem.Models;

namespace AttendanceSystem.Helpers
{
    public class ExcelHelper
    {
        private readonly string _filePath;
        private const string STUDENT_SHEET = "Students";
        private const string ATTENDANCE_SHEET = "Attendance";
        private const string SCORE_SHEET = "Scores";

        // 定义评分等级常量
        public static readonly string[] ScoreLevels = new string[] 
        { 
            "完美", "优秀", "中等", "合格", "不合格" 
        };

        public ExcelHelper(string filePath)
        {
            _filePath = filePath;

            // 如果文件不存在，创建默认结构
            if (!File.Exists(_filePath))
            {
                CreateDefaultExcelFile();
            }
        }

        private void CreateDefaultExcelFile()
        {
            using (var workbook = new XLWorkbook())
            {
                // 创建学生表
                var studentSheet = workbook.Worksheets.Add(STUDENT_SHEET);
                studentSheet.Cell(1, 1).Value = "学号";
                studentSheet.Cell(1, 2).Value = "姓名";
                studentSheet.Cell(1, 3).Value = "班级";

                // 创建签到表
                var attendanceSheet = workbook.Worksheets.Add(ATTENDANCE_SHEET);
                attendanceSheet.Cell(1, 1).Value = "日期";
                attendanceSheet.Cell(1, 2).Value = "学号";
                attendanceSheet.Cell(1, 3).Value = "姓名";
                attendanceSheet.Cell(1, 4).Value = "签到时间";

                // 创建评分表
                var scoreSheet = workbook.Worksheets.Add(SCORE_SHEET);
                scoreSheet.Cell(1, 1).Value = "日期";
                scoreSheet.Cell(1, 2).Value = "学号";
                scoreSheet.Cell(1, 3).Value = "姓名";
                scoreSheet.Cell(1, 4).Value = "评分等级";
                scoreSheet.Cell(1, 5).Value = "备注";

                // 保存工作簿
                workbook.SaveAs(_filePath);
            }
        }

        #region 学生管理
        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();

            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(STUDENT_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    students.Add(new Student
                    {
                        StudentId = row.Cell(1).GetString(),
                        Name = row.Cell(2).GetString(),
                        ClassName = row.Cell(3).GetString()
                    });
                }
            }

            return students;
        }

        public Student GetStudentById(string studentId)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(STUDENT_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    if (row.Cell(1).GetString() == studentId)
                    {
                        return new Student
                        {
                            StudentId = row.Cell(1).GetString(),
                            Name = row.Cell(2).GetString(),
                            ClassName = row.Cell(3).GetString()
                        };
                    }
                }
            }

            return null;
        }

        public void AddStudent(Student student)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(STUDENT_SHEET);
                var lastRow = worksheet.LastRowUsed();
                int newRow = lastRow == null ? 2 : lastRow.RowNumber() + 1;

                worksheet.Cell(newRow, 1).Value = student.StudentId;
                worksheet.Cell(newRow, 2).Value = student.Name;
                worksheet.Cell(newRow, 3).Value = student.ClassName;

                workbook.SaveAs(_filePath);
            }
        }

        public void UpdateStudent(Student student)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(STUDENT_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    if (row.Cell(1).GetString() == student.StudentId)
                    {
                        row.Cell(2).Value = student.Name;
                        row.Cell(3).Value = student.ClassName;
                        workbook.SaveAs(_filePath);
                        return;
                    }
                }
            }
        }

        public void DeleteStudent(string studentId)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(STUDENT_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    if (row.Cell(1).GetString() == studentId)
                    {
                        row.Delete();
                        workbook.SaveAs(_filePath);
                        return;
                    }
                }
            }
        }
        #endregion

        #region 考勤管理
        public void AddAttendance(Attendance attendance)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(ATTENDANCE_SHEET);
                var lastRow = worksheet.LastRowUsed();
                int newRow = lastRow == null ? 2 : lastRow.RowNumber() + 1;

                worksheet.Cell(newRow, 1).Value = attendance.Date;
                worksheet.Cell(newRow, 2).Value = attendance.StudentId;
                worksheet.Cell(newRow, 3).Value = attendance.Name;
                worksheet.Cell(newRow, 4).Value = attendance.AttendanceTime;

                workbook.SaveAs(_filePath);
            }
        }

        public List<Attendance> GetAttendanceByDate(DateTime date)
        {
            var attendances = new List<Attendance>();

            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(ATTENDANCE_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    var rowDate = row.Cell(1).GetDateTime();
                    if (rowDate.Date == date.Date)
                    {
                        attendances.Add(new Attendance
                        {
                            Date = rowDate,
                            StudentId = row.Cell(2).GetString(),
                            Name = row.Cell(3).GetString(),
                            AttendanceTime = row.Cell(4).GetDateTime()
                        });
                    }
                }
            }

            return attendances;
        }

        public bool IsStudentAttended(string studentId, DateTime date)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(ATTENDANCE_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    var rowDate = row.Cell(1).GetDateTime();
                    var rowStudentId = row.Cell(2).GetString();
                    if (rowDate.Date == date.Date && rowStudentId == studentId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region 评分管理
        public void AddScore(Score score)
        {
            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(SCORE_SHEET);
                var lastRow = worksheet.LastRowUsed();
                int newRow = lastRow == null ? 2 : lastRow.RowNumber() + 1;

                worksheet.Cell(newRow, 1).Value = score.Date;
                worksheet.Cell(newRow, 2).Value = score.StudentId;
                worksheet.Cell(newRow, 3).Value = score.Name;
                worksheet.Cell(newRow, 4).Value = score.ScoreValue; // 现在存储的是评分等级字符串
                worksheet.Cell(newRow, 5).Value = score.Remark;

                workbook.SaveAs(_filePath);
            }
        }

        public List<Score> GetScoresByDate(DateTime date)
        {
            var scores = new List<Score>();

            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(SCORE_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    var rowDate = row.Cell(1).GetDateTime();
                    if (rowDate.Date == date.Date)
                    {
                        scores.Add(new Score
                        {
                            Date = rowDate,
                            StudentId = row.Cell(2).GetString(),
                            Name = row.Cell(3).GetString(),
                            ScoreValue = row.Cell(4).GetString(), // 获取评分等级字符串
                            Remark = row.Cell(5).GetString()
                        });
                    }
                }
            }

            return scores;
        }

        public List<Score> GetScoresByStudent(string studentId)
        {
            var scores = new List<Score>();

            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(SCORE_SHEET);
                var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

                foreach (var row in rows)
                {
                    var rowStudentId = row.Cell(2).GetString();
                    if (rowStudentId == studentId)
                    {
                        scores.Add(new Score
                        {
                            Date = row.Cell(1).GetDateTime(),
                            StudentId = rowStudentId,
                            Name = row.Cell(3).GetString(),
                            ScoreValue = row.Cell(4).GetString(), // 获取评分等级字符串
                            Remark = row.Cell(5).GetString()
                        });
                    }
                }
            }

            return scores;
        }
        #endregion

        #region 备份导出
        public void BackupExcel(string backupPath)
        {
            File.Copy(_filePath, backupPath, true);
        }
        #endregion
    }
} 