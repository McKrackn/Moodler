using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight;

namespace Moodler
{
    public class Lecture : ViewModelBase
    {
        private string _backgroundColor = "transparent";
        private string _lectureGrade;
        private DateTime _updatedAt;
        public string LectureName { get; set; }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; RaisePropertyChanged();}
        }

        public string LectureGrade
        {
            get => _lectureGrade;
            set { _lectureGrade = value; RaisePropertyChanged();}
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => Set(ref _updatedAt, value);
        }

        public Lecture(string lecture, string lectureGrade)
        {
            LectureName = lecture;
            LectureGrade = lectureGrade;
            UpdatedAt = DateTime.Now;
        }
    }
}
