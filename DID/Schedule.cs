using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Threading;
using NReco.VideoInfo;
using System.IO;

namespace DID
{
    public partial class Schedule : UserControl
    {
        Thread _ScheduleThread;                                                                                                                 //컨텐츠 표출하는 Thread
        Thread _BasicScheduleThread;                                                                                                          //컨텐츠 표출하는 Thread
        JArray _jBasicArr = new JArray();
        JArray _jArr = new JArray();                                                                                                              //스케쥴 정보 저장(메인에서 넘어오는 데이터)
        bool _bScheduleType = false;                                                                                                           //true = 우선 스케쥴, false = 기본 스케쥴
        string _sScheduleTempOrder = "1";                                                                                                   //스케쥴의 템플릿 수, 위치
        string _sStartTime = "";
        string _sEndTime = "";

        ConnectWeb _ConnectWeb = new ConnectWeb();                                                                                //웹 서버에 상태 업데이트를 위해 설정

        public Schedule()
        {
            InitializeComponent();
        }

        private void Schedule_Load(object sender, EventArgs e)
        {
            picBox.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + "img\\layout\\basic.png";
            mediaPlayer.uiMode = "none";
            mediaPlayer.stretchToFit = true;
        }

        public void ChangeContentsImage(string sContentFullPath)
        {
            picBox.ImageLocation = sContentFullPath;
        }

        //jArray 우선 스케쥴 
        //jArraryBasic 기본 스케쥴 
        //iTempOrder 화면에 컨텐츠 띄우는 위치
        public void ChangeSchedule(JArray jArray, JArray jArraryBasic, int iTempOrder)
        {
            if(_ScheduleThread != null && _ScheduleThread.IsAlive)
            {
                _ScheduleThread.Abort();
            }
            if(_BasicScheduleThread != null && _BasicScheduleThread.IsAlive)
            {
                _BasicScheduleThread.Abort();
            }

            _jArr = jArray;                             //우선 스케쥴 리스트
            _jBasicArr = jArraryBasic;               //기본 스케쥴 리스트
            //_bScheduleType = bScheduleType; //기본, 우선 스케쥴 
            _sScheduleTempOrder = iTempOrder.ToString();    //스케쥴 템플릿 번호

            foreach (JObject itemObj in _jArr)
            {
                _bScheduleType = TimeCheck(itemObj["SCHEDULE_STARTTIME"].ToString(), itemObj["SCHEDULE_ENDTIME"].ToString());
            }

            if (_bScheduleType)
            {
                _ScheduleThread = new Thread(new ThreadStart(LoadSchedule));
                _ScheduleThread.IsBackground = true;
                _ScheduleThread.Start();
            }
            else
            {
                _BasicScheduleThread = new Thread(new ThreadStart(LoadBasicSchedule));
                _BasicScheduleThread.IsBackground = true;
                _BasicScheduleThread.Start();
            }
        }

        private void LoadSchedule()
        {
            bool bScheduleCheck = false;
            for (int i = 0; i < _jArr.Count; i++)
            {
                if (_jArr[i]["SCHEDULE_TEMPLATEVIEW"].ToString().Equals(_sScheduleTempOrder))
                {
                    string sContentsData = DID_Form._dicContentsList[int.Parse(_jArr[i]["CONTENTS_SEQ"].ToString())];
                    string sContentTempPath = DID_Form._sContentLocalPath + sContentsData;
                    string[] sViewTimeSplit = _jArr[i]["SCHEDULE_VIEWTIME"].ToString().Split(':');
                    int iViewTime = (int.Parse(sViewTimeSplit[0]) * 60 * 60) + (int.Parse(sViewTimeSplit[1]) * 60) + int.Parse(sViewTimeSplit[2]);
                    if (iViewTime <= 0)
                        iViewTime = 10;
                    string[] sExt = sContentsData.Split('.');

                    if (_sScheduleTempOrder.Equals("1"))
                    {
                        //1번은 무조건 돌기 때문에 1번에서 상태 업데이트를 하고
                        //다른 번호들은 업데이트 하지 않는다.
                        StatusUpdate(_jArr[i]["CONTENTS_SEQ"].ToString());
                    }

                    bool bDayCheck = DayCheck(_jArr[i]["MONDAY"].ToString(), _jArr[i]["TUESDAY"].ToString(), _jArr[i]["WEDNESDAY"].ToString(),
                        _jArr[i]["THURSDAY"].ToString(), _jArr[i]["FRIDAY"].ToString(), _jArr[i]["SATURDAY"].ToString(), _jArr[i]["SUNDAY"].ToString());

                    bool bTimeCheck = TimeCheck(_jArr[i]["SCHEDULE_STARTTIME"].ToString(), _jArr[i]["SCHEDULE_ENDTIME"].ToString());

                    if(bDayCheck && bTimeCheck)
                    {
                        bScheduleCheck = true;
                        ChangeContents(sExt, sContentTempPath, iViewTime);
                    }
                    else
                    {
                        if (_BasicScheduleThread != null && _BasicScheduleThread.IsAlive)
                        {
                            _BasicScheduleThread.Abort();
                        }
                        _BasicScheduleThread = new Thread(new ThreadStart(LoadBasicSchedule));
                        _BasicScheduleThread.IsBackground = true;
                        _BasicScheduleThread.Start();

                        break;
                    }
                }
                if (i == _jArr.Count - 1)
                {
                    i = -1;
                    if (!bScheduleCheck)
                    {
                        //우선 스케쥴로 들어왔는데 요일, 시간 정보가 조건에 충족하지 못해서 스케쥴 전체 중 한개도 돌지 못한다면 
                        //우선 스케쥴을 종료하고 기본 스케쥴로 돈다.
                        if (_BasicScheduleThread != null && _BasicScheduleThread.IsAlive)
                        {
                            _BasicScheduleThread.Abort();
                        }
                        _BasicScheduleThread = new Thread(new ThreadStart(LoadBasicSchedule));
                        _BasicScheduleThread.IsBackground = true;
                        _BasicScheduleThread.Start();

                        break;
                    }
                }
            }
        }

        private void LoadBasicSchedule()
        {
            for (int i = 0; i < _jBasicArr.Count; i++)
            {
                //스케쥴의 템플릿 위치 번호를 확인해서 해당 위치에만 컨텐츠를 뿌린다.
                if (_jBasicArr[i]["SCHEDULE_TEMPLATEVIEW"].ToString().Equals(_sScheduleTempOrder))
                {
                    string sContentsData = DID_Form._dicContentsList[int.Parse(_jBasicArr[i]["CONTENTS_SEQ"].ToString())];
                    string sContentTempPath = DID_Form._sContentLocalPath + sContentsData;
                    string[] sViewTimeSplit = _jBasicArr[i]["SCHEDULE_VIEWTIME"].ToString().Split(':');
                    int iViewTime = (int.Parse(sViewTimeSplit[0]) * 60 * 60) + (int.Parse(sViewTimeSplit[1]) * 60) + int.Parse(sViewTimeSplit[2]);
                    if (iViewTime <= 0)
                        iViewTime = 10;
                    string[] sExt = sContentsData.Split('.');

                    if (_sScheduleTempOrder.Equals("1"))
                    {
                        //1번은 무조건 돌기 때문에 1번에서 상태 업데이트를 하고
                        //다른 번호들은 업데이트 하지 않는다.
                        StatusUpdate(_jBasicArr[i]["CONTENTS_SEQ"].ToString());
                    }

                    //시간 확인, 요일 확인
                    bool bTimeCheck = TimeCheck(_sStartTime, _sEndTime);
                    bool bDayCheck = ContentsDayCheck();
                    

                    if (_bScheduleType && bDayCheck && bTimeCheck)
                    {
                        _bScheduleType = true;
                        //기본 스케쥴을 돌다가 우선스케쥴의 조건이 충족된다면 기본 스케쥴 종료하고 우선스케쥴로 돈다.
                        if (_ScheduleThread != null && _ScheduleThread.IsAlive)
                        {
                            _ScheduleThread.Abort();
                        }
                        _ScheduleThread = new Thread(new ThreadStart(LoadSchedule));
                        _ScheduleThread.IsBackground = true;
                        _ScheduleThread.Start();
                        break;
                    }
                    else    //기본 스케쥴
                    {
                        _bScheduleType = false;
                        ChangeContents(sExt, sContentTempPath, iViewTime);
                    }
                }
                if (i == _jBasicArr.Count - 1)
                {
                    //0부터 다시 시작
                    i = -1;
                }
            }
        }

        private void ChangeContents(string[] sExt, string sContentTempPath, int iViewTime)
        {
            if (sExt.Length >= 2)
            {
                if (sExt[sExt.Length - 1] == "avi" || sExt[sExt.Length - 1] == "mp4" || sExt[sExt.Length - 1] == "wmv")
                {
                    try
                    {
                        picBox.Visible = false;
                        mediaPlayer.Visible = true;
                        var ffProbe = new FFProbe();
                        mediaPlayer.URL = sContentTempPath;
                        var videoInfo = ffProbe.GetMediaInfo(sContentTempPath);
                        var duration = Math.Floor(videoInfo.Duration.TotalSeconds);     //영상 조회 시간
                        mediaPlayer.Ctlcontrols.play();

                        Thread.Sleep((int)duration * 1000);
                    }
                    catch (Exception ex)
                    {
                        //파일 다운로드하는데 제대로 다운이 안되면 이 오류가 남
                        //해당 파일을 삭제하고 메인에서 파일 다운로드할때 다시 받는다
                        if (ex.Message.Contains("moov atom"))
                        {
                            FileInfo file_info = new System.IO.FileInfo(sContentTempPath);
                            file_info.Delete();
                            DID_Form._sFileList = "";
                        }
                    }
                }
                else
                {
                    picBox.Visible = true;
                    mediaPlayer.Visible = false;
                    //mediaPlayer.Ctlcontrols.stop();
                    picBox.ImageLocation = sContentTempPath;
                    Thread.Sleep(iViewTime * 1000);
                }
            }
        }

        //웹서버에 컨트롤러 상태 체크를 위해 설정
        private void StatusUpdate(string sContentsNo)
        {
            _ConnectWeb.SendWebData(DID_Form._sWebServerPath + "ControllerStatus.jsp?ContentsInfo=" + sContentsNo);
        }

        //우선 스케쥴중에 하나라도 오늘 보여줘야 되는 컨텐츠가 있는지 확인
        private bool ContentsDayCheck()
        {
            bool bDayCheck = false;

            foreach (JObject itemObj in _jArr)
            {
                bDayCheck = DayCheck(itemObj["MONDAY"].ToString(), itemObj["TUESDAY"].ToString(), itemObj["WEDNESDAY"].ToString(),
                        itemObj["THURSDAY"].ToString(), itemObj["FRIDAY"].ToString(), itemObj["SATURDAY"].ToString(), itemObj["SUNDAY"].ToString());
            }
            return bDayCheck;
        }


        //우선 스케쥴의 요일 확인
        private bool DayCheck(string sMon, string sTue, string sWed, string sThu, string sFri, string sSat, string sSun)
        {
            bool bDayCheck = false;

            DateTime nowDt = DateTime.Now;

            if (nowDt.DayOfWeek == DayOfWeek.Monday)
            {
                if (sMon == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (sTue == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (sWed == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Thursday)
            {
                if (sThu == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Friday)
            {
                if (sFri == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Saturday)
            {
                if (sSat == "1")
                    bDayCheck = true;
            }
            else if (nowDt.DayOfWeek == DayOfWeek.Sunday)
            {
                if (sSun == "1")
                    bDayCheck = true;
            }
            return bDayCheck;
        }
        private bool TimeCheck(string sStartTime, string sEndTime)
        {
            bool bUse = false;
            try
            {
                if (sStartTime != "" && sEndTime != "")
                {
                    DateTime dtStart = DateTime.Parse(sStartTime);
                    DateTime dtEnd = DateTime.Parse(sEndTime);
                    DateTime dtNow = DateTime.Now;
                    TimeSpan ts1 = dtStart - dtNow;
                    TimeSpan ts2 = dtEnd - dtNow;
                    _sStartTime = sStartTime;
                    _sEndTime = sEndTime;

                    if (ts1.TotalMinutes <= 0 && ts2.TotalMinutes >= 0)
                        bUse = true;
                    else
                        bUse = false;
                    //우선 예약의 경우 해당 시간에만 컨텐츠 표출을 해야하기 때문에
                    //날짜 검사한후 시간도 검사한다.
                    TimeSpan tsStart = new TimeSpan(dtStart.Hour, dtStart.Minute, dtStart.Second);
                    TimeSpan tsEnd = new TimeSpan(dtEnd.Hour, dtEnd.Minute, dtEnd.Second);
                    TimeSpan tsNow = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);

                    if(tsNow >= tsStart && tsNow <= tsEnd && bUse)
                    {
                        bUse = true;
                    }
                    else
                    {
                        bUse = false;
                    }


                    /*
                    if (dtNow.Hour >= dtStart.Hour && dtNow.Hour <= dtEnd.Hour && bUse)
                    {
                        if (dtNow.Minute >= dtStart.Minute && dtNow.Minute <= dtEnd.Minute)
                        {
                            bUse = true;
                        }
                        else
                            bUse = false;
                    }
                    else
                        bUse = false;
                    */
                }
            }
            catch (Exception ex)
            {
                bUse = false;
                DID_Form._Log.Error("TimeCheck Error\r\n", ex);
            }

            return bUse;
        }
    }
}
