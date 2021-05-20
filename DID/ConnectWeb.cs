using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DID
{
    class ConnectWeb
    {
        public JArray ConnectWebData(string sUrl)
        {
            JArray jArray = new JArray();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Method = "POST";
                request.ContentType = "Application/json;charset=utf-8";
                string sendData = ""; //파라메터
                byte[] buffer;
                buffer = Encoding.Default.GetBytes(sendData);
                request.ContentLength = buffer.Length;
                Stream sendStream = request.GetRequestStream();
                sendStream.Write(buffer, 0, buffer.Length);
                sendStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream respPostStream = response.GetResponseStream();
                StreamReader readerPost = new StreamReader(respPostStream, Encoding.UTF8);
                string sResult = readerPost.ReadToEnd();
                request.Abort();
                //var obj = JArray.Parse(sResult).ToObject<List<object>>();
                jArray = JArray.Parse(sResult);
            }
            catch (Exception ex)
            {
                jArray = null;
                DID_Form._Log.Error("ConnectWeb - ConnectWebData Error", ex);
            }
            return jArray;
        }

        public void SendWebData(string sUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Method = "POST";
                request.ContentType = "Application/json;charset=utf-8";
                string sendData = ""; //파라메터
                byte[] buffer;
                buffer = Encoding.Default.GetBytes(sendData);
                request.ContentLength = buffer.Length;
                Stream sendStream = request.GetRequestStream();
                sendStream.Write(buffer, 0, buffer.Length);
                sendStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream respPostStream = response.GetResponseStream();
                StreamReader readerPost = new StreamReader(respPostStream, Encoding.UTF8);
                string sResult = readerPost.ReadToEnd();
                request.Abort();
            }
            catch (Exception ex)
            {
                DID_Form._Log.Error("ConnectWeb - SendWebData Error", ex);
            }
        }
    }
}
