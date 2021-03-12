using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight;

namespace Moodler
{
    public class Lecture : ViewModelBase
    {
        public string LectureName { get; set; }
        public string BackgroundColor { get; set; } = "transparent";
        public string LectureGrade { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Lecture(string lecture, string lectureGrade)
        {
            LectureName = lecture;
            LectureGrade = lectureGrade;
            UpdatedAt = DateTime.Now;
        }
    }
}
