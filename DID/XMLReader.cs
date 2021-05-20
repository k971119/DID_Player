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

        public String Read()
        {
            XmlNodeList nodeList = xml.GetElementsByTagName("item");

            String[] News = new String[nodeList.Count];

            for(int i = 0; i<nodeList.Count; i++)
            {
                News[i] = nodeList[i].InnerText.Trim();
            }
            Random random = new Random();
            String result = News[0];
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
    }
}
