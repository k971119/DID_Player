using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using System.Xml;


namespace DID
{
    public partial class DID_Form : Form
    {
        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow);

        double _dWidth;                                                             //가로 크기
        double _dHeight;                                                            //세로 크기
        
        WebClient _webClient = new WebClient();                             //웹서버에서 파일 다운로드를 위해 생성
        ConnectWeb _ConnectWeb = new ConnectWeb();                  //웹서버 접속
        Schedule _ContentsModule1 = new Schedule();                     //컨텐츠 판넬1
        Schedule _ContentsModule2 = new Schedule();                     //컨텐츠 판넬2
        Schedule _ContentsModule3 = new Schedule();                     //컨텐츠 판넬3
        //MoveText testText = new MoveText();

        //하단 뉴스 WPF
        ElementHost _NewsHost = new ElementHost();

        //사이드 WPF
        ElementHost _LogoHost = new ElementHost();
        ElementHost _StockHost = new ElementHost();
        ElementHost _WeatherHost = new ElementHost();
        ElementHost etc1 = new ElementHost();

        LogoSub _LogoSub = new LogoSub();
        StockSub _StockSub = new StockSub();
        WeatherSub _WeatherSub = new WeatherSub();
        EtcSub _EtcSub = new EtcSub();
        NoticeText _NoticeText = new NoticeText();

        PlayerInfo _PlayerInfo = new PlayerInfo();                              //단말기 정보 저장
        struct PlayerInfo                            //단말기 정보 저장
        {
            public int iBasicScheduleSeq;           //기본 스케쥴 번호
            public int iNowScheduleSeq;           //우선 스케쥴 번호
            public string sPowerOnTime;           //전원 On 시간
            public string sPowerOffTime;           //전원 Off 시간
            public int iPowerControl;                //전원 ON, OFF 여부
            public int iPowerControlCheck;        //전원 제어 여부
            public string sPlayerType;               //단말기 타입
        }
        public static string _sWebServerDownloadPath = "http://192.168.0.116:8080/Building_Information/img/upload/";                //웹서버 이미지 경로
        public static string _sWebServerPath = "http://192.168.0.116:8080/DID_Supercom/contents/client/";                                //웹서버 페이지 경로
        public static string _sContentLocalPath = AppDomain.CurrentDomain.BaseDirectory + "img\\";                                    //이미지 저장 로컬 경로
        string _sXmlPath = AppDomain.CurrentDomain.BaseDirectory + "xml\\";

        string _sStartTime = string.Empty;                                                                      //우선 스케쥴 시작 시간
        string _sEndTime = string.Empty;                                                                        //우선 스케쥴 종료 시간

        public static Dictionary<int, string> _dicContentsList = new Dictionary<int, string>();                       //컨텐츠 리스트(seq, 파일명.png)
        public static string _sFileList = string.Empty;                                                                            //파일 리스트 저장
        public static ILog _Log;                                                                                                      //로그 셋팅
        string _sLogPath = AppDomain.CurrentDomain.BaseDirectory + "log\\";                                     //로그 경로

        DispatcherTimer ContentsLoadTimer1 = new DispatcherTimer();                                                 //컨텐츠 로드 타이머
        DispatcherTimer ContentsFileCheckTimer = new DispatcherTimer();                                             //컨텐츠 파일 다운로드 타이머
        Thread _FileDownThread;                                                                                                   //파일 다운로드를 위한 Thread
        
        JArray _jArrBasic = new JArray();                                                                           //기본 스케쥴 
        JArray _jArrNow = new JArray();                                                                           //우선 스케쥴
        bool _bNowSchedule = false;                                                                              //우선 스케쥴 여부
        
        string _sBasicScheduleList = string.Empty;                                                                                 //기본 스케쥴 비교 저장
        string _sNowScheduleList = string.Empty;                                                                                  //우선 스케쥴 비교 저장
        string _NoticeData = "카카오뱅크 1분기 순익 467억원…이용자 1615만명 은행권 1위...그리고 그다음 텍스트 어디까지 가나 보자";
        int _iCaptionTime = 3;
        int _iTextCount = 1000;


        public DID_Form()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void DID_Form_Load(object sender, EventArgs e)
        {
            ShowCursor(true);

            _dWidth = Screen.PrimaryScreen.Bounds.Width;
            _dHeight = Screen.PrimaryScreen.Bounds.Height;
            //_dHeight = 800;
            this.KeyPreview = true;
            //this.Controls.Add(_ContentsModule1);

            LoadXml();

            SetLog();
            
            ///////////////////////////////////////////////
            //처음 로드했을때 타이머가 늦게 도니 타이머 돌기 전에 한번 실행시킨다.
            FileDownLoad();

            PlayerInfoData();

            PlayerTypeSetLayout();

            CheckScheduleData();

            if (_PlayerInfo.sPlayerType == "ElevDid")
            {
                NoticeDataLoad();
            }
            ///////////////////////////////////////////////

            ContentsFileCheckTimer.Tick += ContentsFileCheckTimer_Tick;
            ContentsFileCheckTimer.Interval = new TimeSpan(0, 0, 30);
            ContentsFileCheckTimer.Start();

            ContentsLoadTimer1.Tick += ContentsLoadTimer1_Tick;
            ContentsLoadTimer1.Interval = new TimeSpan(0, 0, 30);
            ContentsLoadTimer1.Start();
            


        }

        private void LoadXml()
        {
            XmlDocument xml = new XmlDocument();

            xml.Load(_sXmlPath + "Config.xml");

            XmlNodeList xmlList = xml.SelectNodes("/Info");

            foreach (XmlNode xmldata in xmlList)
            {
                string sServer = xmldata["ServerIP"].InnerText;
                _sWebServerDownloadPath = string.Format("http://{0}/img/upload/", sServer);
                _sWebServerPath = string.Format("http://{0}/contents/client/", sServer);
                _iCaptionTime = int.Parse(xmldata["CaptionTime"].InnerText);
                _iTextCount = int.Parse(xmldata["CaptionTextCount"].InnerText);
            }

            XMLReader reader = new XMLReader("C:\\Surable\\DataServer\\XML\\News.xml");
            //_NoticeData = reader.Read();
        }


        private void ContentsFileCheckTimer_Tick(object sender, EventArgs e)
        {
            if (_FileDownThread == null)
            {
                _FileDownThread = new Thread(new ThreadStart(FileDownLoad));
                _FileDownThread.Start();
            }
        }

        //데이터베이스 참조해서 _dicContentsList에 seq, 파일명을 등록하고
        //신규 파일들을 단말기에 다운로드 한다.
        private void FileDownLoad()
        {
            try
            {
                JArray jArr = _ConnectWeb.ConnectWebData(_sWebServerPath + "Contents.jsp");
                string sFileList = jArr.ToString();
                if (!sFileList.Equals(_sFileList))
                {
                    _sFileList = sFileList;
                    DirectoryInfo diFolder = new DirectoryInfo(_sContentLocalPath);
                    if (!diFolder.Exists)
                    {
                        diFolder.Create();
                    }

                    foreach (JObject itemObj in jArr)
                    {
                        int iContentsSeq = int.Parse(itemObj["CONTENTS_SEQ"].ToString());
                        string sContents = itemObj["CONTENTS"].ToString();
                        if (!_dicContentsList.ContainsKey(iContentsSeq))
                        {
                            _dicContentsList.Add(iContentsSeq, sContents);

                            Uri downUrl = new Uri(_sWebServerDownloadPath + sContents);
                            _webClient.DownloadFile(downUrl, _sContentLocalPath + sContents);
                        }
                    }
                }
                _FileDownThread = null;
            }
            catch (Exception ex)
            {
                _FileDownThread = null;
                _Log.Error("ContentsFileCheckTimer_Tick Error \r\n", ex);
            }
        }

        //타이머 핵심
        private void ContentsLoadTimer1_Tick(object sender, EventArgs e)
        {
            PlayerInfoData();               //단말기 정보 조회

            CheckScheduleData();        //스케쥴 조회

            if (_PlayerInfo.sPlayerType == "ElevDid")
            {
                NoticeDataLoad();
            }


        }

        #region 로그 셋팅
        private void SetLog()
        {
            // 로그 매니져 세팅
            var repository = LogManager.GetRepository();
            repository.Configured = true;
            // 콘솔 로그 패턴 설정
            //var consoleAppender = new ConsoleAppender();
            //consoleAppender.Name = "Console";
            // 로그 패턴
            //consoleAppender.Layout = new PatternLayout("%d [%t] %-5p %c - %m%n");
            // 파일 로그 패턴 설정
            var rollingAppender = new RollingFileAppender();
            rollingAppender.Name = "RollingFile";
            // 시스템이 기동되면 파일을 추가해서 할 것인가? 새로 작성할 것인가?
            rollingAppender.AppendToFile = true;
            rollingAppender.DatePattern = "-yyyy-MM-dd";
            // 로그 파일 설정
            rollingAppender.File = _sLogPath + "Log(" + DateTime.Today.ToShortDateString() + ").log";
            // 파일 단위는 날짜 단위인 것인가, 파일 사이즈인가?
            rollingAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            // 로그 패턴
            rollingAppender.Layout = new PatternLayout("%d [%t] %-5p %c - %m%n");
            var hierarchy = (Hierarchy)repository;
            //hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.AddAppender(rollingAppender);
            rollingAppender.ActivateOptions();
            // 로그 출력 설정 All 이면 모든 설정이 되고 Info 이면 최하 레벨 Info 위가 설정됩니다.
            hierarchy.Root.Level = log4net.Core.Level.All;
            _Log = LogManager.GetLogger(this.GetType());
        }
        #endregion

        private void NoticeDataLoad()
        {
            try
            {
                JArray jArr = _ConnectWeb.ConnectWebData(_sWebServerPath + "CaptionText.jsp");

                foreach (JObject itemObj in jArr)
                {
                    _NoticeData = itemObj["CAPTION_CONTENTS"].ToString();
                }
            }
            catch (Exception ex)
            {
                _Log.Error("NoticeDataLoad Error \r\n", ex);
            }
        }

        //단말기 정보를 로드해서 저장한다.
        //기본, 우선 스케쥴, 전원 관리의 정보를 저장
        private void PlayerInfoData()
        {
            try
            {
                JArray jArr = _ConnectWeb.ConnectWebData(_sWebServerPath + "PlayerInfo.jsp");

                foreach (JObject itemObj in jArr)
                {
                    _PlayerInfo.iBasicScheduleSeq = int.Parse(itemObj["BASIC_SCHEDULE_SEQ"].ToString());
                    _PlayerInfo.iNowScheduleSeq = int.Parse(itemObj["TEMPLATE_SEQ"].ToString());
                    _PlayerInfo.sPowerOnTime = itemObj["POWER_ON_TIME"].ToString();
                    _PlayerInfo.sPowerOffTime = itemObj["POWER_OFF_TIME"].ToString();
                    _PlayerInfo.iPowerControl = int.Parse(itemObj["POWER_CONTROL"].ToString());
                    _PlayerInfo.iPowerControlCheck = int.Parse(itemObj["POWER_CONTROL_CHECK"].ToString());
                    _PlayerInfo.sPlayerType = itemObj["PLAYER_TYPE"].ToString();
                }

                if (_PlayerInfo.iPowerControlCheck == 0)
                {
                    PowerControl(_PlayerInfo.iPowerControl);
                }
            }
            catch(Exception ex)
            {
                _Log.Error("PlayerInfoData Error \r\n", ex);
            }
        }

        #region PC 리부팅 Function
        //파라미터 iReboot이 1일 경우
        //PC를 재부팅 시킨다.
        private void PowerControl(int iReboot)
        {
            try
            {
                _ConnectWeb.SendWebData(_sWebServerPath + "PowerReboot.jsp");

                if (iReboot == 1)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-r");
                }
            }
            catch (Exception ex)
            {
                _Log.Error("PowerControl \r\n" + ex.ToString());
            }
        }
        #endregion

        //플레이어 타입에 따른 레이아웃 변경
        //기본은 풀화면이지만 엘리베이터의 경우 레이아웃 자체가 바뀔수 있음
        private void PlayerTypeSetLayout()
        {
            if (_PlayerInfo.sPlayerType == "BasicDid")
            {
                this.Width = (int)_dWidth;
                this.Height =(int)_dHeight;
            }
            else if (_PlayerInfo.sPlayerType == "ElevDid")
            {
                //레이아웃 정해지면 처리
            }
            else
            {
                MessageBox.Show("현재 지원되지 않는 타입입니다.");
            }
        }

        //우선 스케쥴 시간 적용 유무 확인
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

                    if (ts1.TotalMinutes <= 0 && ts2.TotalMinutes >= 0)
                        bUse = true;
                    else
                        bUse = false;

                    //우선 예약의 경우 해당 시간에만 컨텐츠 표출을 해야하기 때문에
                    //날짜 검사한후 시간도 검사한다.
                    TimeSpan tsStart = new TimeSpan(dtStart.Hour, dtStart.Minute, dtStart.Second);
                    TimeSpan tsEnd = new TimeSpan(dtEnd.Hour, dtEnd.Minute, dtEnd.Second);
                    TimeSpan tsNow = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);

                    if (tsNow >= tsStart && tsNow <= tsEnd && bUse)
                    {
                        bUse = true;
                    }
                    else
                    {
                        bUse = false;
                    }
                }
            }
            catch (Exception ex)
            {
                bUse = false;
                _Log.Error("TimeCheck Error\r\n", ex);
            }

            return bUse;
        }

        //기본, 우선 스케쥴을 조회한다.
        //우선 스케쥴과 기본 스케쥴의 적용 여부를 확인하고 스케쥴에 따라 레이아웃을 변경한다.
        private void CheckScheduleData()
        {
            try
            {
                string sBasicUrl = string.Format("{0}BasicSchedule.jsp?BasicSchedule={1}", _sWebServerPath, _PlayerInfo.iBasicScheduleSeq);
                string sNowUrl = string.Format("{0}NowSchedule.jsp?NowSchedule={1}", _sWebServerPath, _PlayerInfo.iNowScheduleSeq);
                _jArrBasic = _ConnectWeb.ConnectWebData(sBasicUrl);
                _jArrNow = _ConnectWeb.ConnectWebData(sNowUrl);
                string sBasicScheduleList = _jArrBasic.ToString();
                string sNowScheduleList = _jArrNow.ToString();

                bool bNowSchedule = TimeCheck(_sStartTime, _sEndTime);
                if (_bNowSchedule != bNowSchedule)
                {
                    //스케쥴을 도는중에 시간이 종료될경우 유저컨트롤에서 체크를 안하기 때문에 알수가 없다.
                    //그래서 여기에서 타이머 돌면서 기존에 저장했던 시작, 종료시간으로 우선스케쥴의 시간을 확인한다.
                    _sNowScheduleList = "";
                    _bNowSchedule = false;
                }

                if (!sBasicScheduleList.Equals(_sBasicScheduleList) || !sNowScheduleList.Equals(_sNowScheduleList))
                {
                    _sBasicScheduleList = sBasicScheduleList;
                    _sNowScheduleList = sNowScheduleList;
                    foreach (JObject itemObj in _jArrNow)
                    {
                        //스케쥴 시간은 동일하게 진행되기 때문에 수량에 상관없이 동일하다.
                        _bNowSchedule = TimeCheck(itemObj["SCHEDULE_STARTTIME"].ToString(), itemObj["SCHEDULE_ENDTIME"].ToString());
                        
                        _sStartTime = itemObj["SCHEDULE_STARTTIME"].ToString();
                        _sEndTime = itemObj["SCHEDULE_ENDTIME"].ToString();
                        if (_bNowSchedule)
                        {
                            SetScheduleLayout(itemObj["SCHEDULE_TYPE"].ToString(), int.Parse(itemObj["WIDTH1"].ToString()), int.Parse(itemObj["HEIGHT1"].ToString()),
                                                    int.Parse(itemObj["WIDTH2"].ToString()), int.Parse(itemObj["HEIGHT2"].ToString()), int.Parse(itemObj["WIDTH3"].ToString()), int.Parse(itemObj["HEIGHT3"].ToString()), true);
                        }
                        break;
                    }

                    if (!_bNowSchedule)
                    {
                        foreach (JObject itemObj in _jArrBasic)
                        {
                            //마찬가지로 스케쥴 타입도 수량에 상관없이 동일함.
                            SetScheduleLayout(itemObj["SCHEDULE_TYPE"].ToString(), int.Parse(itemObj["WIDTH1"].ToString()), int.Parse(itemObj["HEIGHT1"].ToString()),
                                                    int.Parse(itemObj["WIDTH2"].ToString()), int.Parse(itemObj["HEIGHT2"].ToString()), int.Parse(itemObj["WIDTH3"].ToString()), int.Parse(itemObj["HEIGHT3"].ToString()), false);
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _Log.Error("CheckScheduleData Error \r\n", ex);
            }
        }

        private void updateNews(object sender, EventArgs e)
        {
            XMLReader reader = new XMLReader(@"\\192.168.0.116\c$\Surable\DataServer\XML\News.xml");
            _NoticeData = reader.Read();
            _NoticeText.moveText(_NoticeData, _dWidth);
        }

        //sScheduleType ==> 스케쥴의 템플릿 레이아웃 수, 레이아웃 번호
        //bSchedule(true) 우선스케쥴
        //bSchedule(false) 기본스케쥴
        private void SetScheduleLayout(string sScheduleType, int iWidth1, int iHeight1, int iWidth2, int iHeight2, int iWidth3, int iHeight3, bool bSchedule)
        {
            try
            {
                /*
                JArray jArr = new JArray();

                if (bSchedule)
                    jArr = _jArrNow;
                else
                    jArr = _jArrBasic;
                */
                this.Controls.Clear();
                
                double dMaxWidth = _dWidth;
                double dMaxHeight = _dHeight;
                double dWidth1 = dMaxWidth / 100 * iWidth1;
                double dWidth2 = dMaxWidth / 100 * iWidth2;
                double dWidth3 = dMaxWidth / 100 * iWidth3;
                double dHeight1 = dMaxHeight / 100 * iHeight1;
                double dHeight2 = dMaxHeight / 100 * iHeight2;
                double dHeight3 = dMaxHeight / 100 * iHeight3;

                if (_PlayerInfo.sPlayerType == "ElevDid")
                {
                    //엘베 레이아웃 설정해야됨
                    this.Controls.Add(_ContentsModule1);
                    _ContentsModule1.Location = new System.Drawing.Point(0, 0);
                    _ContentsModule1.Width = (int)dMaxWidth-400;
                    _ContentsModule1.Height = (int)dMaxHeight-80;
                    _ContentsModule1.ChangeSchedule(_jArrNow, _jArrBasic, 1);

                    double svWidth = 400;
                    double svHeight = (_ContentsModule1.Height) / 4;

                    //로고
                    _LogoHost.Location = new Point((int)dMaxWidth - 400, 0);
                    _LogoHost.Width = (int)svWidth;
                    _LogoHost.Height = (int)svHeight;
                    _LogoHost.BackColor = Color.White;
                    this.Controls.Add(_LogoHost);
                    _LogoSub.InitializeComponent();
                    _LogoHost.Child = _LogoSub;


                    //주가정보
                    _StockHost.Location = new Point((int)dMaxWidth - 400, (int)svHeight * 1);
                    _StockHost.Width = (int)svWidth;
                    _StockHost.Height = (int)svHeight;
                    _StockHost.BackColor = Color.White;
                    this.Controls.Add(_StockHost);
                    _StockSub.InitializeComponent();
                    _StockHost.Child = _StockSub;

                    //날씨정보
                    _WeatherHost.Location = new Point((int)dMaxWidth - 400, (int)svHeight * 2);
                    _WeatherHost.Width = (int)svWidth;
                    _WeatherHost.Height = (int)svHeight;
                    _WeatherHost.BackColor = Color.White;
                    this.Controls.Add(_WeatherHost);
                    _WeatherSub.InitializeComponent();
                    _WeatherHost.Child = _WeatherSub;

                    etc1.Location = new Point((int)dMaxWidth - 400, (int)svHeight * 3);
                    etc1.Width = (int)svWidth;
                    etc1.Height = (int)svHeight;
                    etc1.BackColor = Color.Blue;
                    this.Controls.Add(etc1);

                    //뉴스 자막
                    _NewsHost.Location = new Point(0, (int)dMaxHeight - 100);
                    _NewsHost.Width = (int)dMaxWidth;
                    _NewsHost.Height = 100;
                    _NewsHost.BackColor = Color.DodgerBlue;
                    this.Controls.Add(_NewsHost);
                    _NoticeText.InitializeComponent();
                    _NewsHost.Child = _NoticeText;
                    //_NoticeText.setText("카카오 1분기 실적조사 및 계획 발표", dMaxWidth, 3);//_iCaptionTime);
                    XMLReader reader = new XMLReader(@"\\192.168.0.116\c$\Surable\DataServer\XML\News.xml");
                    _NoticeData = reader.Read();
                    DispatcherTimer newsUpdater = new DispatcherTimer();
                    newsUpdater.Tick += updateNews;
                    newsUpdater.Interval = new TimeSpan(0, 0, (int)(_NoticeData.Replace(" ", "").Length * 0.3 < 10 ? 15 : _NoticeData.Replace(" ", "").Length * 0.3)*2);
                    newsUpdater.Start();
                    _NoticeText.moveText(_NoticeData, dMaxWidth);

                }
                else if (_PlayerInfo.sPlayerType == "BasicDid")
                {
                    switch (sScheduleType)
                    {
                        case "1":
                        default:
                            this.Controls.Add(_ContentsModule1);
                            _ContentsModule1.Location = new System.Drawing.Point(0, 0);
                            _ContentsModule1.Width = (int)dMaxWidth;
                            _ContentsModule1.Height = (int)dMaxHeight;
                            _ContentsModule1.ChangeSchedule(_jArrNow, _jArrBasic, 1);
                            break;
                        case "2":
                            this.Controls.Add(_ContentsModule1);
                            this.Controls.Add(_ContentsModule2);

                            _ContentsModule1.Location = new System.Drawing.Point(0, 0);
                            _ContentsModule1.Width = (int)dMaxWidth;
                            _ContentsModule1.Height = (int)dHeight1;
                            _ContentsModule2.Location = new System.Drawing.Point(0, (int)dHeight1);
                            _ContentsModule2.Width = (int)dMaxWidth;
                            _ContentsModule2.Height = (int)dHeight2;
                            _ContentsModule1.ChangeSchedule(_jArrNow, _jArrBasic, 1);
                            _ContentsModule2.ChangeSchedule(_jArrNow, _jArrBasic, 2);
                            break;
                        case "3":
                            this.Controls.Add(_ContentsModule1);
                            this.Controls.Add(_ContentsModule2);

                            _ContentsModule1.Location = new System.Drawing.Point(0, 0);
                            _ContentsModule1.Width = (int)dWidth1;
                            _ContentsModule1.Height = (int)dMaxHeight;
                            _ContentsModule2.Location = new System.Drawing.Point((int)dWidth1, 0);
                            _ContentsModule2.Width = (int)dWidth2;
                            _ContentsModule2.Height = (int)dMaxHeight;
                            _ContentsModule1.ChangeSchedule(_jArrNow, _jArrBasic, 1);
                            _ContentsModule2.ChangeSchedule(_jArrNow, _jArrBasic, 2);
                            break;
                        case "4":
                            this.Controls.Add(_ContentsModule1);
                            this.Controls.Add(_ContentsModule2);
                            this.Controls.Add(_ContentsModule3);

                            _ContentsModule1.Location = new System.Drawing.Point(0, 0);
                            _ContentsModule1.Width = (int)dWidth1;
                            _ContentsModule1.Height = (int)dMaxHeight;
                            _ContentsModule2.Location = new System.Drawing.Point((int)dWidth1, 0);
                            _ContentsModule2.Width = (int)dWidth2;
                            _ContentsModule2.Height = (int)dHeight2;
                            _ContentsModule3.Location = new System.Drawing.Point((int)dWidth2, (int)dHeight2);
                            _ContentsModule3.Width = (int)dWidth3;
                            _ContentsModule3.Height = (int)dHeight3;
                            _ContentsModule1.ChangeSchedule(_jArrNow, _jArrBasic, 1);
                            _ContentsModule2.ChangeSchedule(_jArrNow, _jArrBasic, 2);
                            _ContentsModule3.ChangeSchedule(_jArrNow, _jArrBasic, 3);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Log.Error("ScheduleLayout Error \r\n", ex);
            }
        }



        #region 프로그램 종료
        private void DID_Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ContentsFileCheckTimer.Stop();

                if (_FileDownThread != null && _FileDownThread.IsAlive)
                {
                    _FileDownThread.Abort();
                }

                //this.Close();
                Application.ExitThread();
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
