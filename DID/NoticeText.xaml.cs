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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DID
{
    /// <summary>
    /// test.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NoticeText : UserControl
    {

        public static string _sText = "";
        public delegate void OnChildTextInputHandler(string Parameters);
        public event OnChildTextInputHandler onChildTextInputEvent;
        public NoticeText()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void setText(string sText, double dWidth, int iTime)
        {
            
            TxtInterfaceBottom.Text = sText;
            double dTextLength = TxtInterfaceBottom.Text.Length;
            double dTextTime = dTextLength / iTime;
            Size TextBlockSize = ShapeMeasure(TxtInterfaceBottom);

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -TextBlockSize.Width;   
            doubleAnimation.To = dWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //doubleAnimation.Duration = new Duration(Timeli);//TimeSpan.FromSeconds(dTextTime));
            TxtInterfaceBottom.BeginAnimation(Canvas.RightProperty, doubleAnimation);
        }

        public void moveText(string txt, double width)
        {
            TxtInterfaceBottom.Text = txt;

            DoubleAnimation anime = new DoubleAnimation();

            anime.To = width;
            anime.From = -(width*3);
            anime.RepeatBehavior = RepeatBehavior.Forever;
            anime.Duration = TimeSpan.FromSeconds(txt.Replace(" ","").Length *0.3 < 10? 15: txt.Replace(" ", "").Length * 0.3);
            TxtInterfaceBottom.BeginAnimation(Canvas.RightProperty, anime);
        }

        /*
         *  TextBlock에 새로운 Text를 할당하고 적용된 TextBlock의 Size값을 리턴
         *   - 애니메이션을 적용하기 위해 TextBlock의 최대 Width값이 필요
         */
        public static Size ShapeMeasure(TextBlock tb)
        {
            // Measured Size is bounded to be less than maxSize
            Size maxSize = new Size(
                 double.PositiveInfinity,
                 double.PositiveInfinity);
            tb.Measure(maxSize);
            return tb.DesiredSize;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
       
        }
    }
}
