using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DID
{

    class XMLReader
    {

        public static ILog _Log;

        XmlDocument xml = new XmlDocument();

        public XMLReader(String path)
        {
            try
            {
                xml.Load(path);
            }catch(Exception ex)
            {
                Console.WriteLine("XML파일을 불러올수 없습니다.");
            }
        }

        public String NewsRead()
        {
            XmlNodeList nodeList = xml.GetElementsByTagName("item");

            String[] News = new String[nodeList.Count];

            for(int i = 0; i<nodeList.Count; i++)
            {
                News[i] = nodeList[i].InnerText.Trim();
            }
            Random random = new Random();
            String result = News[0]+" ";
            for(int i = 0; i<5; i++)
            {
                string sub = News[random.Next(1, News.Length - 1)];
                if(sub.LastIndexOf("]") == sub.Length - 1)
                {
                    i--;
                    continue;
                }
                result += sub+"  ";
            }
            Console.Write(result);
            return result;
        }

        public String StockRead()
        {
            XmlNodeList nodeList = xml.GetElementsByTagName("data");
            return "";
        }

        public String[] WeatherRead()
        {
            String[] resultData = new string[2];

            XmlNodeList nodeList = xml.GetElementsByTagName("data");
            XmlNode node = xml.SelectSingleNode("data");
            //Console.WriteLine(node["temp"].InnerText);
            foreach(XmlNode data in nodeList)
            {
                resultData[0] = data["temp"].InnerText;
                Console.WriteLine(resultData[0]);
                resultData[1] = data["wfKor"].InnerText;
                Console.WriteLine(resultData[1]);
            }

            return resultData;
        }
    }
}
