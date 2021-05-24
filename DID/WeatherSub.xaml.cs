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

namespace DID
{
    /// <summary>
    /// UserControl2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WeatherSub : UserControl
    {
        public WeatherSub()
        {
            InitializeComponent();
        }

        internal void setWeather(String sTemp, String sClimate)
        {
            temp.Text = sTemp;
            climate.Text = sClimate;
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            switch (sClimate)
            {
                case "맑음":
                    img.UriSource = new Uri("img\\sun.png", UriKind.Relative);
                    break;
                case "구름 많음":
                    img.UriSource = new Uri("img\\cloud.png", UriKind.Relative);
                    break;
                case "흐림":
                    img.UriSource = new Uri("img\\cloud.png", UriKind.Relative);
                    break;
                case "비":
                    img.UriSource = new Uri("img\\rain.png", UriKind.Relative);
                    break;
                case "비/눈":
                    img.UriSource = new Uri("img\\rain_snow.png", UriKind.Relative);
                    break;
                case "눈":
                    img.UriSource = new Uri("img\\snow.png", UriKind.Relative);
                    break;
                case "소나기":
                    img.UriSource = new Uri("img\\rain.png", UriKind.Relative);
                    break;
            }
            img.EndInit();
            climateIMG.Stretch = Stretch.Fill;
            climateIMG.Source = img;
        }
    }
}
