using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DID
{
    /// <summary>
    /// UserControl2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogoSub : UserControl
    {
        public LogoSub()
        {
            InitializeComponent();
            DispatcherTimer clockUpdater = new DispatcherTimer();
            clockUpdater.Tick += updateTime;
            clockUpdater.Interval = new TimeSpan(0, 0, 5);
            clockUpdater.Start();
        }

        private void updateTime(object sender, EventArgs e)
        {
            clock.Text = DateTime.Now.ToString("yyyy년 MM월 dd일 HH시 mm분");
        }
    }
}
