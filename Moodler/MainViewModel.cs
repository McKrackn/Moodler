using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Moodler
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Lecture> _filteredlectures = new ObservableCollection<Lecture>();
        private ObservableCollection<Lecture> _lectures = new ObservableCollection<Lecture>();
        public ObservableCollection<Lecture> Lectures
        {
            get
            {
                if (ShowEmpty) return _lectures;
                else return _filteredlectures;
            }
            set
            {
                if (ShowEmpty) _lectures=value;
                else _filteredlectures = value;
            }
        }
        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value, nameof(Status));
        }
        private string _nameVisibility = "True";
        public string NameVisibility
        {
            get => _nameVisibility;
            set { _nameVisibility = value; RaisePropertyChanged(); }
        }
        private bool _silentmode;
        public bool SilentMode
        {
            get => _silentmode;
            set => this.Set(ref _silentmode, value, nameof(SilentMode));
        }
        private bool _showEmpty;
        public bool ShowEmpty
        {
            get => _showEmpty;
            set => this.Set(ref _showEmpty, value, nameof(ShowEmpty));
        }

        public string Pass { get; set; }
        public string User { get; set; }

        string pageSource;
        string getUrl = "https://moodle.technikum-wien.at/grade/report/overview/index.php";

        public RelayCommand MoodleConnectCommand { get; }

        public int RefreshRate { get; set; } = 30;

        public MainViewModel()
        {
            MoodleConnectCommand = new RelayCommand(()=>
                {
                    NameVisibility = "False"; 
                    Task.Factory.StartNew(ConnectToMoodle);
                    MoodleConnectCommand.RaiseCanExecuteChanged();
                },
            () => NameVisibility=="True");
        }
        
        private void ConnectToMoodle()
        {
            string formUrl = "https://moodle.technikum-wien.at/";
            string formParams = string.Format("username={0}&password={1}", User, Pass);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(formUrl + "login/index.php?" + formParams);
            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.AllowAutoRedirect = true;
            req.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }
            WebResponse resp = (HttpWebResponse)req.GetResponse();
            while (NameVisibility=="False")
            {
                GetMoodleGrades(req);
                Thread.Sleep(RefreshRate * 1000);
            }
        }

        public void Moo()
        {
            while (!SilentMode)
            {
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri($"media\\moo.wav", UriKind.Relative));
                mediaPlayer.Play();
                Thread.Sleep(4000);
            }
        }

        public void GetMoodleGrades(HttpWebRequest req)
        {
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
            getRequest.CookieContainer = req.CookieContainer;
            getRequest.AllowAutoRedirect = false;
            try
            {
                WebResponse getResponse = getRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
                        {
                            pageSource = sr.ReadToEnd();
                        }
                        var regexFindLectures = new Regex("grade-report-overview-.*?a\\ href.*?>(.*?)<\\/a.*?c1\">(.*?)<", RegexOptions.IgnoreCase);
                        var matches = regexFindLectures.Matches(pageSource);
                        foreach (Match match in matches)
                        {
                            Lecture tmpLecture = new Lecture(match.Groups[1].ToString().Trim(), match.Groups[2].ToString().Trim());
                            App.Current.Dispatcher.Invoke(
                                () =>
                                {
                                    bool found = false;
                                    foreach (Lecture actLecture in Lectures)
                                        if (actLecture.LectureName == tmpLecture.LectureName)
                                        {
                                            found = true;
                                            if (actLecture.LectureGrade != tmpLecture.LectureGrade)
                                            {
                                                actLecture.BackgroundColor = "OrangeRed";
                                                actLecture.LectureGrade = tmpLecture.LectureGrade;
                                                Task.Factory.StartNew(Moo);
                                            }
                                            break;
                                        }
                                    if (found == false)
                                    {
                                        if (ShowEmpty)
                                            Lectures.Add(tmpLecture);
                                        else if (!ShowEmpty && !tmpLecture.LectureGrade.Contains('-') && !tmpLecture.LectureGrade.Contains("0,00"))
                                            Lectures.Add(tmpLecture); 
                                    }
                                    RaisePropertyChanged("Lectures");
                                });
                        }
                Status = "Moodle Grades refreshed at " + DateTime.Now.ToLongTimeString();
            }
            catch (WebException)
            {
                Status = "Connection failed (check user/password) at " + DateTime.Now.ToLongTimeString();

                App.Current.Dispatcher.Invoke(
                    () =>
                    {
                        NameVisibility = "True";
                        MoodleConnectCommand.RaiseCanExecuteChanged(); });
            }
            catch (NullReferenceException)
            {
                Status = "Moodle returned NULL at " + DateTime.Now.ToLongTimeString() + "\nThat really should not have happened.";
            }
        }
    }
}
