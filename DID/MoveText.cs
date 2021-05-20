using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace DID
{
    public partial class MoveText : UserControl
    {
        Thread _TextThread;
        int _iTextSize = 0;
        public MoveText()
        {
            InitializeComponent();
            lblTxt.Location = new System.Drawing.Point(this.Width, 0);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public void SetMoveText(string sTextData)
        {
            _iTextSize = sTextData.Length * 40;
            lblTxt.Size = new Size(_iTextSize, 100);
            lblTxt.Text = sTextData;
            if (_TextThread != null && _TextThread.IsAlive)
            {
                _TextThread.Abort();
            }
            _TextThread = new Thread(new ThreadStart(LoadCaptionText));
            _TextThread.IsBackground = true;
            _TextThread.Start();

        }
        public void LoadCaptionText()
        {
            while (true)
            {
                if (lblTxt.Text.Length > 40)
                {
                    lblTxt.Left -= 2;

                    if (lblTxt.Left < -lblTxt.Width)
                    {
                        lblTxt.Left = this.Width;
                    }
                }
                Thread.Sleep(10);
            }
        }

    }
}
