using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiftGUI
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void DownReqBtn2_Click(object sender, EventArgs e)
        {
            var FloorNum = 2;
            var FloorDirection = "Down";

            Build1.OnAddReqEvent(FloorEventArgs, e);
        }

        private void UpReqBtn1_Click(object sender, EventArgs e)
        {
            var FloorNum = 1;
            var FloorDirection = "Up";
            FloorEventArgs FlorReq = new FloorEventArgs(FloorNum, FloorDirection);
        }

        private void DownReqBtn1_Click(object sender, EventArgs e)
        {
            var FloorNum = 1;
            var FloorDirection = "Down";
            FloorEventArgs FlorReq = new FloorEventArgs(FloorNum, FloorDirection);
        }

        private void UpReqBtn0_Click(object sender, EventArgs e)
        {
            var FloorNum = 0;
            var FloorDirection = "Up";
            FloorEventArgs FlorReq = new FloorEventArgs(FloorNum, FloorDirection);
        }
    }
}
