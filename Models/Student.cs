using System;

namespace AttendanceSystem.Models
{
    public class Student
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }

        public Student() { }

        public Student(string studentId, string name, string className)
        {
            StudentId = studentId;
            Name = name;
            ClassName = className;
        }

        public override string ToString()
        {
            return $"{StudentId} - {Name} ({ClassName})";
        }
    }
} 