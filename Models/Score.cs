using System;

namespace AttendanceSystem.Models
{
    public class Score
    {
        public DateTime Date { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string ScoreValue { get; set; }
        public string Remark { get; set; }

        public Score() { }

        public Score(DateTime date, string studentId, string name, string scoreValue, string remark)
        {
            Date = date;
            StudentId = studentId;
            Name = name;
            ScoreValue = scoreValue;
            Remark = remark;
        }
    }
} 