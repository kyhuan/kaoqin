using System;

namespace AttendanceSystem.Models
{
    public class Attendance
    {
        public DateTime Date { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public DateTime AttendanceTime { get; set; }

        public Attendance() { }

        public Attendance(DateTime date, string studentId, string name, DateTime attendanceTime)
        {
            Date = date;
            StudentId = studentId;
            Name = name;
            AttendanceTime = attendanceTime;
        }
    }
} 