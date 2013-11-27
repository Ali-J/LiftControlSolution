using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiftGUI
{
    // *********** AddRequestDel Delegate **************
    // Create delegate for sending Floor request to central list
    public delegate void AddRequestDel<FloorEventArgs>(object sender, FloorEventArgs e);

    // ************* FloorEventArgs class **************
    // Extension of EventArgs class to contain additional
    // arguments applicable for a floor request
    public class FloorEventArgs : EventArgs
    {

        public int ReqFloorNum { get; set; }
        public string ReqDirection { get; set; }

        // Constructor
        public FloorEventArgs(int FloorNum, string Direction)
        {
            ReqFloorNum = FloorNum;
            ReqDirection = Direction;
        }
    }

    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 RefForm = new Form1();
            Building Build1 = new Building(RefForm);

            Application.Run(RefForm);

            Thread BuildThread = new Thread(Build1.InstantiateLift);
            BuildThread.IsBackground = true;
            BuildThread.Start();
        }
    }

    public class Building
    {
        public LiftClass.FloorRequestList<int, string> Req1;
        public LiftClass.Lift Lift1;
        public Form1 RefForm;
        
        // ************ AddReqEvent Event ***********
        // New event for when a floor request is made
        public event AddRequestDel<FloorEventArgs> AddReqEvent;
        // Invoke the FloorReqEvent
        protected virtual void OnAddReqEvent(FloorEventArgs e)
        {
            if (AddReqEvent != null)
            {
                AddReqEvent(this, e);
            }
        }

        public Building(Form1 TargetForm)
        {
            RefForm = TargetForm;
        }
        public void InstantiateLift()
        {
            LiftClass.Floors Floor1 = new LiftClass.Floors();
            LiftClass.Floors Floor2 = new LiftClass.Floors();
            LiftClass.Floors Floor3 = new LiftClass.Floors();

            // Create instance of floor request class
            LiftClass.FloorRequestList<int, string> Req1 = new LiftClass.FloorRequestList<int, string>();
            LiftClass.Lift Lift1 = new LiftClass.Lift(Req1);

            this.AddReqEvent += new AddRequestDel<FloorEventArgs>(AddCentralRequest);
            Lift1.LiftMoveEvent += new LiftClass.LiftMoveDel<LiftClass.LiftMoveArgs>(UpdatePanel);
        }

        public void AddCentralRequest(object sender, FloorEventArgs e)
        {
            Req1.AddRequest(e.ReqFloorNum, e.ReqDirection);
        }

        public void UpdatePanel(object sender, LiftClass.LiftMoveArgs e)
        {
            RefForm.LiftPanel1.Top = e.LiftHeight;
        }

    }
}
