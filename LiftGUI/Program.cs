using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace LiftGUI
{
    // *********** ActualLiftMoveDel Delegate *************
    // Create delegate to carry event for when lift actually moves.
    // This will be use so the floor sensers can see when the lift
    // has reached their floor. When this occurs they will send a signal
    // that can tell the lift which floor it is at.
    public delegate void ActualLiftMoveDel<FloorEventArgs>(object sender, FloorEventArgs e);

    // *********** AddRequestDel Delegate **************
    // Create delegate for sending Floor request to central list
    public delegate void AddRequestDel<FloorEventArgs>(object sender, FloorEventArgs e);

    // *********** AddFloorDel Delegate **************
    // Create delegate for sending Floor request to central list
    public delegate void AddFloorDel<FloorEventArgs>(object sender, FloorEventArgs e);

    // ************* FloorEventArgs class **************
    // Extension of EventArgs class to contain additional
    // arguments applicable for a floor request
    public class FloorEventArgs : EventArgs
    {
        // Variables to hold values that make up a floor request
        public int ReqFloorNum { get; set; }
        public string ReqDirection { get; set; }

        // Constructor for FloorEventArgs extension
        public FloorEventArgs(int FloorNum, string Direction)
        {
            ReqFloorNum = FloorNum;
            ReqDirection = Direction;
        }
    }

    // ************* AddLiftButton Class ***************
    // Class to OverRide the standard Button class 
    // and add the properties necessary to create
    // the floor buttons required for lift requests
    public class AddLiftButton : Button
    {
        protected Form RefForm;
        protected LiftClass.FloorRequestList<int, string> Req1;

        public AddLiftButton(Form Frm, LiftClass.FloorRequestList<int, string> CentList)
        {
            Req1 = CentList;
            RefForm = Frm;
            this.Location = new System.Drawing.Point(350, 35);
            this.Name = "AddLiftBtn";
            this.Size = new System.Drawing.Size(75, 23);
            this.TabIndex = 0;
            this.Text = "Add Lift";
            this.UseVisualStyleBackColor = true;
            this.Click += (sender, e) => new LiftSim(RefForm, Req1);
            RefForm.Controls.Add(this);
        }
    }

    // ************* AddFloorButton Class ***************
    // Class to OverRide the standard Button class 
    // and add the properties necessary to create
    // the floor buttons required for lift requests
    public class AddFloorButton : Button
    {
        protected Form RefForm;
        protected LiftClass.FloorRequestList<int, string> Req1;

        public AddFloorButton(Form Frm, LiftClass.FloorRequestList<int, string> CentList)
        {
            Req1 = CentList;
            RefForm = Frm;
            this.Location = new System.Drawing.Point(350, 10);
            this.Name = "AddFlrBtn";
            this.Size = new System.Drawing.Size(75, 23);
            this.TabIndex = 0;
            this.Text = "Add Floor";
            this.UseVisualStyleBackColor = true;
            this.Click += (sender, e) => new Floors(RefForm, Req1);
            RefForm.Controls.Add(this);
        }
    }

    // ************* SimToggleButton Class ***************
    // Class to OverRide the standard Button class 
    // and add the properties necessary to toggle
    // the lift simulator required for automatically generating
    // lift requests without user interaction.
    public class SimToggleButton : Button
    {
        protected Form RefForm;
        protected bool TestSim;
        protected EventGenerator.TestEventGenerator EventGenRef;

        // Constructor to insitalise button and all its properties
        public SimToggleButton(Form Frm, EventGenerator.TestEventGenerator EventGen)
        {
            TestSim = false;
            RefForm = Frm;
            EventGenRef = EventGen;

            this.Location = new System.Drawing.Point(13, 13);
            this.Name = "SimToggle";
            this.Size = new System.Drawing.Size(75, 23);
            this.TabIndex = 0;
            this.Text = "Toggle Sim";
            this.UseVisualStyleBackColor = true;
            this.Click += (sender, e) => EventGen.ToggleSimBool();
            RefForm.Controls.Add(this);
        }
    }

    // ************** FloorButton Class *****************
    // Class to OverRide the standard Button class 
    // and add the properties necessary to create
    // the floor buttons required for lift requests
    public class FloorButton : Button
    {
        protected int ReqFloorNum;
        protected string ReqDirection;
        protected Form RefForm;
        protected LiftClass.FloorRequestList<int, string> Req1;

        // Constructor for a floor button for resuesting a lift
        public FloorButton(string Name, string Text, int Floor, Form Frm, LiftClass.FloorRequestList<int, string> CentList)
        {
            RefForm = Frm;
            Req1 = CentList;

            ReqFloorNum = Floor;
            ReqDirection = Text;

            var FormHeight = RefForm.Height;
            var Height = 0;
            if (Text == "Down")
                Height = (FormHeight - 80) - (Floor * 100);
            else
                Height = (FormHeight - 100) - (Floor * 100);

            // Create the button to call the lift
            this.Location = new System.Drawing.Point(50, Height);
            this.Name = Name;
            this.Size = new System.Drawing.Size(50, 25);
            // Assign the clicking of this button to a floor reqeuest event
            this.Click += (sender, e) => AddCentralRequest(sender, e);
            this.TabIndex = 0;
            this.Text = Text;
            this.Visible = true;
            this.UseVisualStyleBackColor = true;
            RefForm.Controls.Add(this);
        }

        // ************ AddReqEvent Event ***********
        // New event for when a floor request is made
        public new event AddRequestDel<FloorEventArgs> Click;

        // Override the click handler for floor request buttons in order
        // they directly trigger a floor request event
        protected override void OnClick(EventArgs e)
        {
            FloorEventArgs myArgs = new FloorEventArgs(ReqFloorNum, ReqDirection);
            AddRequestDel<FloorEventArgs> AddReqEvent = Click;
            if (AddReqEvent != null)
                AddReqEvent(this, myArgs);
            base.OnClick(e);
        }

        // Method to Add reqeusts generated to the central list in order they can be dealt
        // with by the lift class.
        public void AddCentralRequest(object sender, FloorEventArgs e)
        {
            Req1.AddRequest(e.ReqFloorNum, e.ReqDirection);
        }
    }

    // ************** LiftButton Class *****************
    // Class to OverRide the standard Button class 
    // and add the properties necessary to create
    // the Lift buttons required for floor requests
    public class LiftButton : Button
    {
        protected int ReqFloorNum;
        protected string ReqDirection;
        protected Panel RefPanel;
        protected int LiftBtnPosX;
        protected int LiftBtnPosY;
        protected LiftClass.Lift LiftRef;

        public LiftButton(LiftClass.Lift Lft, int BtnFloor, string Text, Panel Pan, int PosX, int PosY)
        {
            LiftRef = Lft;
            LiftBtnPosX = PosX;
            LiftBtnPosY = PosY;

            ReqFloorNum = BtnFloor;

            var BtnName = "Floor" + BtnFloor;

            RefPanel = Pan;
            ReqDirection = Text;

            RefPanel.Controls.Add(this);
            // Create the button to call the lift
            this.Location = new System.Drawing.Point(LiftBtnPosX, LiftBtnPosY);
            this.Name = Name;
            this.Size = new System.Drawing.Size(15, 20);
            // Assign the clicking of this button to a floor reqeuest event
            this.Click += (sender, e) => AddCentralRequest(sender, e);
            this.TabIndex = 0;
            this.Text = Text;
            this.Visible = true;
            this.UseVisualStyleBackColor = true;

        }

        // ************ AddReqEvent Event ***********
        // New event for when a floor request is made
        public event AddRequestDel<FloorEventArgs> AddReqEvent;
        public new event AddRequestDel<FloorEventArgs> Click;

        // Invoke the FloorReqEvent
        protected virtual void OnAddReqEvent(FloorEventArgs e)
        {
            if (AddReqEvent != null)
            {
                AddReqEvent(this, e);
            }
        }

        // Override the click handler for floor request buttons in order
        // they directly trigger a floor request event
        protected override void OnClick(EventArgs e)
        {
            FloorEventArgs myArgs = new FloorEventArgs(ReqFloorNum, ReqDirection);
            AddRequestDel<FloorEventArgs> AddReqEvent = Click;
            if (AddReqEvent != null)
                AddReqEvent(this, myArgs);
            base.OnClick(e);
        }

        // Method to Add reqeusts generated to the central list in order they can be dealt
        // with by the lift class.
        public void AddCentralRequest(object sender, FloorEventArgs e)
        {
            var testing = e.ReqDirection;
            LiftRef.AddLiftRequest(e.ReqFloorNum);
        }
    }

    // ************** LiftDisplay Class *****************
    // Label to display the direction the lift is moving in
    // and what floor it is currently at.
    public class LiftDisplay : Label
    {
        protected static int LastLiftPos;
        protected Form RefForm;
        protected string Direction;
        protected int Floor;

        public LiftDisplay(Form Frm, int LiftPost)
        {
            RefForm = Frm;
            LastLiftPos = LiftPost;

            this.AutoSize = true;
            this.Location = new System.Drawing.Point(LastLiftPos, 13);
            this.Name = "label1";
            this.Size = new System.Drawing.Size(35, 13);
            this.TabIndex = 0;
            this.Text = "---";
            RefForm.Controls.Add(this);
        }

        public void UpdateDisplay(object sender)
        {

            var LiftRef = ((LiftClass.Lift)sender);
            Direction = LiftRef.GetLiftData().Item2;
            Floor = LiftRef.GetLiftData().Item1;

            this.Text = String.Format("{0}:{1}", Direction, Floor);
        }
    }

    // ************ LiftSimulator Class **************
    // Class to create lift and its simlator panel.
    // This lift itself will be contained in teh MainControllerProgram
    // and will simply create events every time it moves that will update 
    // teh panel's location
    public class LiftSimulator : Panel
    {
        // Static for tracking position of last lift
        // in order to be able to position the next one further accross
        protected static int LastLiftPos;
        // Pass form Reference in order controls can be placed
        protected Form RefForm;
        protected LiftClass.Lift Lift;
        protected int ActualHeight;
        protected int ActualFloor;
        protected LiftDisplay LiftDisp;

        static LiftSimulator()
        {
            LastLiftPos = 150;
        }

        public LiftSimulator(Form Frm, LiftClass.Lift Lft, int RefHeight)
        {
            ActualHeight = RefHeight;
            RefForm = Frm;
            Lift = Lft;
            // Create panel to simulate lift movement
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Location = new System.Drawing.Point(LastLiftPos, 539);
            this.Name = "LiftPanel";
            this.Size = new System.Drawing.Size(35, 60);
            this.TabIndex = 0;
            RefForm.Controls.Add(this);

            LiftDisp = new LiftDisplay(RefForm, LastLiftPos);

            LastLiftPos += 50;

            // Assign instance of the LiftMoveEvent to the update panel metod in order that when the
            // lift moves that the panel position is being updated accordingly.
            Lift.LiftMoveEvent += new LiftClass.LiftMoveDel<LiftClass.LiftMoveArgs>(this.UpdatePanel);
        }

        // Method to update the panel location every time the lift moves also
        // update current floor should the lift pass by another floor.
        public void UpdatePanel(object sender, LiftClass.LiftMoveArgs e)
        {
            this.Top = (RefForm.Height - 100) - e.LiftHeight;
            ActualHeight = e.LiftHeight;
            ActualFloor = e.LiftFloor;

            this.LiftDisp.UpdateDisplay(sender);

            var NewFloor = Floors.FindFloorByHeight(ActualHeight, ActualFloor);

            if (NewFloor != ActualFloor)
            {
                Lift.UpdateFloorNumber(NewFloor);
            }
        }
    }

    // *************** Floors Class ****************
    // Class defnied to handle addition of floors
    // and their properties
    public class Floors
    {
        // Static to track the number of floors addressable by all lifts
        protected static List<Tuple<FloorButton, FloorButton>> BtnReqTracker;
        protected static List<Floors> FloorTracker;
        protected static int TotalNumberOfFloors = 0;
        protected FloorButton DownReq;
        protected FloorButton UpReq;
        protected Form RefForm;
        protected int Floor;
        protected int Height;
        protected LiftClass.FloorRequestList<int, string> Req1;

        // *************** Static Constructor ****************
        // Static constructor to initialise the static lists
        // being used in this class.
        static Floors()
        {
            BtnReqTracker = new List<Tuple<FloorButton, FloorButton>>();
            FloorTracker = new List<Floors>();
        }
        // ******************* Constructor ******************
        // Constructor to add additional floor object and asscociated properties
        public Floors(Form Frm, LiftClass.FloorRequestList<int, string> CentList)
        {
            Req1 = CentList;
            RefForm = Frm;

            // If a new floor request is added after this one is created an event
            // shall be triggered in order that the Up button for this floor can
            // be made visible
            MoreFloorEvent += (Sender, e) => RevealUp();

            // Set variables for 
            Floor = TotalNumberOfFloors;
            Height = Floor * 100;

            // If more then one floor has been added then start adding the down button
            // as there is no point putting a down button on the lowest floor.
            if (TotalNumberOfFloors > 0)
            {
                // Variables to name the floor button being added by
                // concactanating the direction and floor number
                var Name = "Down" + TotalNumberOfFloors;
                FloorButton DownReq = new FloorButton(Name, "Down", Floor, RefForm, Req1);
                // Track the buttons added in order they can be used in the event generator
                // to simulate the lift functionality. At this stage only the down button is 
                // added to the tracker list. 
                BtnReqTracker.Add(new Tuple<FloorButton, FloorButton>(DownReq, null));
            }

            // If not the first floor then reveal the Up button for the previous floor
            if (Floors.TotalNumberOfFloors > 0)
                OnMoreFloorEvent(new EventArgs());

            // Add floor to FloorTracker list
            FloorTracker.Add(this);
            TotalNumberOfFloors++;
        }

        // ************** FindFloorByHeight Method ****************
        // Method for finding floor by searcing through all floors stored
        // in the floor tracker away and seeing if one matches the height being
        // searched with. This will be used as the lift is moving to determine
        // which floor the lift is currently at.
        public static int FindFloorByHeight(int HeightToFind, int CurrFloor)
        {
            var FloorReturn = CurrFloor;

            for (int Index = 0; Index < FloorTracker.Count; Index++)
            {
                var FloorObj = FloorTracker[Index];
                if (FloorObj.Height == HeightToFind)
                    FloorReturn = FloorObj.Floor; ;
            }

            return FloorReturn;
        }

        // *********** GetNumberOfFloors Method ****************
        // Method to return the total number of floors that have been created.
        // This is used when the program is generating the lift buttons to request a flor
        // to esnure that 1 is created for every floor that exists.
        public static int GetNumberOfFloors()
        {
            return FloorTracker.Count;
        }

        // ************ MoreFloorEvent Event ***********
        // New event for when the more floors are added
        public event EventHandler MoreFloorEvent;
        // Invoke the FloorReqEvent
        protected virtual void OnMoreFloorEvent(EventArgs e)
        {
            if (MoreFloorEvent != null)
                MoreFloorEvent(this, e);
        }

        // ************** RevealUp Method ****************
        // Method to add up button to previous floor so the floor being
        // added is accessible to flower floors
        public void RevealUp()
        {
            // Temporary variables to create the name and floor number of the
            // button being created
            var Floor = TotalNumberOfFloors - 1;
            var Name = "Up" + Floor;
            FloorButton UpReq = new FloorButton(Name, "Up", Floor, RefForm, Req1);

            // When the button has been created add to the BtnReqTracker list
            // in order the event generator can create lift requests.
            // If this is an additonal floor then it will need to remove the old
            // entry with just the down request button and add a new one with 
            // both request buttons.
            if (TotalNumberOfFloors > 0)
            {
                var BtnReqIndex = BtnReqTracker.Count - 1;
                var LstDownReq = BtnReqTracker[BtnReqIndex].Item1;
                BtnReqTracker.RemoveAt(BtnReqIndex);
                BtnReqTracker.Add(new Tuple<FloorButton, FloorButton>(LstDownReq, UpReq));
            }
            // Else if this is the first floor to be added then only the up button is required
            // and the item for the down button can be set to null. 
            else
                BtnReqTracker.Add(new Tuple<FloorButton, FloorButton>(null, UpReq));

            // Once the Up button for the previous floro has been create this 
            // object unsuscribes from the event as it has no need to know if
            // any further floors are added.
            this.MoreFloorEvent -= (Sender, e) => RevealUp();
        }

        // ************* GetFloorNumber method ***************
        // Simple class to be able to read the number of floors
        // that have been created.
        public static int GetFloorNumber()
        {
            // Return number of total floors
            return TotalNumberOfFloors;
        }

        // ************** ReturnBtnReqList *****************
        // This method return the BtnReqTracker list that is storing
        // all of the floor request buttons that have been created. 
        public static List<Tuple<FloorButton, FloorButton>> ReturnBtnReqList()
        {
            return BtnReqTracker;
        }
    }

    // ************** LiftSim class *************
    // Class to create lift instances and the simulator panels
    // to represent their movement.
    public class LiftSim
    {
        // Create static list of all the lifts that have been created
        protected static List<LiftSim> LiftTracker;
        // Create list of all the lifts request buttons that have been created
        // in each lift.
        protected List<LiftButton> LftBtnReqTracker;
        // Reference to the form in order the controls can be added
        protected Form RefForm;
        protected LiftClass.FloorRequestList<int, string> Req1;
        public LiftClass.Lift Lift;
        protected LiftSimulator LiftSimPanel;
        protected int ActualHeight;
        // Variables to be used when creating the lift buttons
        protected LiftButton LftBtn;
        protected int LiftBtnPosX;
        protected int LiftBtnPosY;

        // ************* Static Constructor ************
        // Constructor to initiliase static properties
        static LiftSim()
        {
            LiftTracker = new List<LiftSim>();
        }

        // **************** Constructor *****************
        public LiftSim(Form Frm, LiftClass.FloorRequestList<int, string> CentList)
        {
            RefForm = Frm;
            Req1 = CentList;

            LiftBtnPosX = 0;
            LiftBtnPosY = 0;

            // Instantiate lift to track all buttons to be added
            LftBtnReqTracker = new List<LiftButton>();
            // Create instance of lift class to service requests
            Lift = new LiftClass.Lift(Req1);
            // Create new panel to simulate lift
            LiftSimulator LiftSimPanel = new LiftSimulator(RefForm, Lift, ActualHeight);
            // Variable to get the total number of floors in order they
            // can all be looped through
            var NumberOfFloors = Floors.GetNumberOfFloors();

            // Loop through all floors that have been created in order
            // a button can be created for every floor within the lift
            // that is being created.
            for (int Index = 0; Index < NumberOfFloors; Index++)
            {
                // Create names for each button from the floor it represents
                var BtnName = Index;
                var BtnText = "" + Index;
                // Create lift button control to be added to lift panel
                LftBtn = new LiftButton(Lift, BtnName, BtnText, LiftSimPanel, LiftBtnPosX, LiftBtnPosY);
                // Check to see if floor is odd or even numbered. This will 
                // identify where button is positioned in grid within panel. 
                if ((Index % 2) == 0)
                {
                    // If button just placed is even numbered then next button will be placed
                    // at the same height but shifted to the right 20 pixels
                    LiftBtnPosX = 20;
                }
                else
                {
                    // If button just placed is odd numbered then next button will be placed
                    // at the same position on the x Axis (0) and down 20 pixels on the y axis
                    LiftBtnPosY += 20;
                    LiftBtnPosX = 0;
                }
                // Once button has been added then add the button the list 
                // in order it can be tracked and used for the event generator.
                this.LftBtnReqTracker.Add(LftBtn);
            }

            LiftTracker.Add(this);
        }

        // ************** ReturnLftBtnReqList Method *****************
        // This method return the LftBtnReqTracker list that is storing
        // all of the lift request buttons that have been created. 
        public List<LiftButton> ReturnLftBtnReqList()
        {
            return LftBtnReqTracker;
        }

        // ************** ReturnLiftTracker Static Method *****************
        // This method return the LiftTracker list that is storing
        // all of the lifts that have been created. By using this list
        // to reference each lift instance it can access each Lifts, Lift
        // button request to be able to use them for the event generator
        public static List<LiftSim> ReturnLiftTracker()
        {
            return LiftTracker;
        }
    }

    // ************ CreateSimulation Class *************
    // Class to create simulation, i.e. add buttons to form
    // in order that the user can add floros and lifts. Also
    // add the toggle Event SimToggleButton which will turn 
    // the event generator on or off when pressed.
    public class CreateSimulation
    {
        protected Form RefForm;
        protected SimToggleButton SimulatorButton;

        public CreateSimulation(Form Frm)
        {
            RefForm = Frm;

            // Create instance of floor request class to handle all central requests
            LiftClass.FloorRequestList<int, string> Req1 = new LiftClass.FloorRequestList<int, string>();

            AddFloorButton AddFlrBtn = new AddFloorButton(RefForm, Req1);
            AddLiftButton AddLftBtn = new AddLiftButton(RefForm, Req1);
            EventGenerator.TestEventGenerator EventGen1 = new EventGenerator.TestEventGenerator();
            SimToggleButton SimulatorButton = new SimToggleButton(RefForm, EventGen1);
        }
    }

    // Static class containing entry point into program
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            // Create and enable form and all components
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Assign form creation to variable in order it can be passed 
            // for reference
            Form1 RefForm = new Form1();
            // Create simulator to create lifts and floor creation buttons
            CreateSimulation Sim1 = new CreateSimulation(RefForm);
            // Initialise the form in the current application
            Application.Run(RefForm);
        }
    }
}

namespace EventGenerator
{
    public class TestEventGenerator
    {
        protected bool TestSim;
        protected List<LiftGUI.LiftSim> LiftList;
        protected List<Tuple<LiftGUI.FloorButton, LiftGUI.FloorButton>> FloorButtonList;
        protected List<LiftGUI.LiftButton> LftBtnList;
        protected int NumberOfLifts;
        protected int NumberOfFloors;
        protected LiftGUI.LiftSim LiftRef;
        protected Random RandGen;
        protected bool AtLeastOneMove;
        protected int NoMoveCounter;
        protected int FloorToReq;
        private BackgroundWorker EventGenThread = new BackgroundWorker();

        // ************ EventGenReqEvent Event ***********
        public event DoWorkEventHandler EventGenReqEvent;

        public virtual void OnEventGenReqEvent(DoWorkEventArgs e)
        {
            if (EventGenReqEvent != null)
            {
                EventGenReqEvent(this, e);
            }
        }
        
        // ************ TestEventGenerator Constructor ***********
        // Event to be generated when the boolean value ToggleSimBool
        // is set True.
        public TestEventGenerator()
        {
            // backgroun worker thred created to handle the work of the Event
            // generator
            EventGenThread.WorkerSupportsCancellation = true;
            EventGenThread.DoWork += new DoWorkEventHandler(RunTestMethod);
            EventGenReqEvent += (Sender, e) => this.EventGenThread.RunWorkerAsync();
        }

        // Boolean value that determines whether or not the automatic event
        // generator is being executed.
        public void ToggleSimBool()
        {
            TestSim = !TestSim;
            if (TestSim == true)
                if(EventGenThread.IsBusy != true)
                    EventGenThread.RunWorkerAsync();
            if (TestSim == false)
                EventGenThread.CancelAsync();
        }

        public void RunTestMethod(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            // While TestSim is true continue to perform randomly 
            // generated lift requests
            while (TestSim == true)
            {
                LiftRef = null;
                HitFloorButton();
                Thread.Sleep(500);
                // optional method to be used in conjunction with self
                // test functionality.
                //FindLift();
                HitLiftButton();
            }
        }

        // Event generator to randomly hit floor buttons to create requests
        protected void HitFloorButton()
        {
            // Instantiate List objects
            FloorButtonList = new List<Tuple<LiftGUI.FloorButton, LiftGUI.FloorButton>>();
            FloorButtonList = LiftGUI.Floors.ReturnBtnReqList();
            LftBtnList = new List<LiftGUI.LiftButton>();

            // Set initial values
            NumberOfFloors = FloorButtonList.Count();

            // Generate a random number based on the total number of
            // floors currently active
            Random RandGen = new Random();
            FloorToReq = RandGen.Next(0, NumberOfFloors);
            // Generate another random number between 1 and 2
            int UpOrDown = RandGen.Next(1, 3);
            // If the UpOrDown number generated is 1
            // then try to seelct item 1 from the FloorButtons object
            if (UpOrDown == 1)
            {
                // Check to make sure the button in the FloorButtonList
                // is not null in order that it can be activated
                if (FloorButtonList[FloorToReq].Item1 != null)
                {
                    FloorButtonList[FloorToReq].Item1.Invoke(new Action(() => 
                    { FloorButtonList[FloorToReq].Item1.PerformClick(); }));
                }
                // If item 1 is null then it is a floor that does not 
                // have a down button and therefore cannot be pressed.
                else
                {
                    FloorButtonList[FloorToReq].Item2.Invoke(new Action(() => 
                    { FloorButtonList[FloorToReq].Item2.PerformClick(); }));
                }
            }
            // Else the button to be pressed is item 2. 
            else
            {
                // Check to make sure the button in the FloorButtonList
                // is not null in order that it can be activated
                if (FloorButtonList[FloorToReq].Item2 != null)
                {
                    FloorButtonList[FloorToReq].Item2.Invoke(new Action(() => 
                    { FloorButtonList[FloorToReq].Item2.PerformClick(); }));
                }
                // If item 2 is null then it is a floor that does not 
                // have an up button and therefore cannot be pressed.
                else
                {
                    FloorButtonList[FloorToReq].Item1.Invoke(new Action(() => 
                    { FloorButtonList[FloorToReq].Item1.PerformClick(); }));
                }
            }
        }

        protected void HitLiftButton()
        {
            if (LiftRef != null)
            {
                // Generate a random number based on the total number of
                // floors currently active
                Random RandGen = new Random();
                FloorToReq = RandGen.Next(0, NumberOfFloors);
                LftBtnList = LiftRef.ReturnLftBtnReqList();
                // Invoke button click action
                LftBtnList[FloorToReq].Invoke(new Action(() =>
                { LftBtnList[FloorToReq].PerformClick(); }));
            }
        }

        // Method for fniding out which lift has answered the request that was
        // entered into the floor request buttton
        protected void FindLift()
        {
            AtLeastOneMove = true;
            LiftList = new List<LiftGUI.LiftSim>();
            LiftList = LiftGUI.LiftSim.ReturnLiftTracker();
            NumberOfLifts = LiftList.Count();
            
            // Keep Looping through looking for a lift that has answered the request
            // until one has been found or it is detected that all lifts aren't 
            // moving. If this is the case then for some reason the requests have been
            // registered with the lifts and they are not moving to respond. In this case
            // the loop is stopped.
            do
            {
                // Initilaise a counter that will be used to track how many lifts
                // are not moving. If all lifts are not moving then the AtLeasOneMove
                // boolean can be set to false to signify no lifts are responding the 
                // request.
                NoMoveCounter = 0;

                // Cycle through all lifts that have been created and see if their
                // current floor is that of the request made and if their status
                // is "NotMoving" I.e. they've answered the request.
                for (int Index = 0; Index < NumberOfLifts; Index++)
                {
                    // Fetch the status of the lift being checked this time
                    // in order it can be checked to see if it's answering the 
                    // request that has been made
                    var LiftData = LiftList[Index].Lift.GetLiftData();
                    var LiftFloor = LiftData.Item1;
                    var LiftDirection = LiftData.Item2;
                    var LiftStatus = LiftData.Item3;

                    // If the Floor of this lift match that request and the lifts
                    // status is not moving then it has answered the request made
                    if (LiftFloor == FloorToReq & LiftStatus == "NotMoving")
                    {
                        // If so set the Lift reference to the lift that has answered
                        // the request
                        LiftRef = LiftList[Index];
                    }

                    // ****** Fail safe to ensure loop does not continue forever *******
                    // Check to see if any lifts are actually answering request
                    // Check to see what the status of the lift is
                    if (LiftDirection == "Still")
                    {
                        // If the lift has no direction then it is not answering 
                        // any requests and so increment NoMoveCounter 
                        NoMoveCounter++;
                        // If the coutner is equal to the number of lifts then 
                        // there are no lifts responding to the request
                        if (NoMoveCounter == NumberOfLifts)
                            AtLeastOneMove = false;
                    }
                }
                // If a Lift has answered the request or none of the lifts are responding
                // to the request then stop the loop.
            } while (LiftRef == null | AtLeastOneMove == true);
        }
    }
}