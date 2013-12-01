using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;


namespace LiftClass
{
    // ************* LiftMoveArgs class **************
    // Extension of EventArgs class to contain additional
    // arguments applicable for a Lift movement
    public class LiftMoveArgs : EventArgs
    {
        public int LiftHeight { get; set; }
        public int LiftFloor { get; set; }
        public string LiftDirection { get; set; }

        // Constructor
        public LiftMoveArgs(int Height, int Floor, string Direction)
        {
            LiftHeight = Height;
            LiftFloor = Floor;
            LiftDirection = Direction;
        }
    }

    // ************* LiftMoveDel Delegate ***************
    // Create delegate for sending Floor request to local
    // lists when added to central list
    public delegate void LiftMoveDel<LiftHeight>(object sender, LiftMoveArgs e);

    // ************* RequestAddedDel Delegate ***************
    // Create delegate for sending Floor request to local
    // lists when added to central list
    public delegate void RequestAddedDel(object sender, EventArgs e);

    // ***************** FloorRequestList class *******************
    // Public list class made up of Tuples to handle floor requests
    public class FloorRequestList<FloorNumReq, DirecReq> : List<Tuple<int, string>>
    {
        // Constructor
        public FloorRequestList()
        {

        }

        // ************ FloorReqEvent Event ***********
        // New event for when a floor request is made
        public event RequestAddedDel FloorReqEvent;

        protected virtual void OnFloorReqEvent(EventArgs e)
        {
            if (FloorReqEvent != null)
            {
                FloorReqEvent(this, e);
            }
        }

        // ************** AddRequest Method ****************
        // Method created to add lift requests to list.
        // Upon pressing the button the floor request
        // will be added to this list in order they can be addressed
        // by the lift itself.
        public void AddRequest(int FloorNumber, string Direction)
        {
            // The list type stores Tuples so both FloorNumber direction
            // are both held in the same record.
            this.Add(new Tuple<int, string>(FloorNumber, Direction));
            // When a floor request is added, the GetFloorRequests method
            // from the lifts shall be called by invoking an event saying
            // an event has been added
            OnFloorReqEvent(new EventArgs());

        }

        // ************** RemRequest Method ****************
        // Method created to remove lift requests from list.
        // Upon answering the floor request the lift may remove
        // the request from the central list
        public void RemRequest(int FloorNumber, string Direction)
        {
            var ReqToRemove = new Tuple<int, String>(FloorNumber, Direction);
            this.Remove(ReqToRemove);
        }

        // Create object in order to be able to lock the return request
        // method. This is so that two lifts do not both get a request
        // and both try to answer it at the same time.
        private Object ReturnRequestLock = new Object();

        // ************** ReturnRequest Method ****************
        // Method created to allow lift to see if request is valid
        // for it to handle, i.e. if it is after the current floor 
        // the lift is at and in the direction it is traveling in.
        public Tuple<int, string> ReturnRequest(int CurrFloorNum, string CurrDirection)
        {
            lock (ReturnRequestLock)
            {
                // Assign a variable to record floor requests recovered
                var RequestRet = new Tuple<int, string>(0, null);

                // Loop through all requests stored in central list and 
                // see which ones can be answered by lift
                for (int index = 0; index < this.Count; index++)
                {
                    // Assign the current FloorRequest Tuple to a variable
                    // to be used throughout loop
                    // Assign first item (Floor number requested) to the
                    // variable CurrentFloorReq
                    var CurrentFloorReq = this[index].Item1;
                    // Assign the second item (Direction requested) to the
                    // variabel CurrentDirReq
                    var CurrentDirReq = this[index].Item2;

                    // If CurrDirection == still then the lift is not answering 
                    // any requests currently and so can pick up the first
                    // request in the list and all other matching requests
                    if (CurrDirection == "Still")
                    {
                        if (CurrentDirReq == this[0].Item2)
                        {
                            // Return floor number and direction for request
                            // to be answered
                            var Floor = this[index].Item1;
                            var Direction = this[index].Item2;
                            RequestRet = new Tuple<int, string>(Floor, Direction);
                            break;
                        }
                    }

                    // If lift is answering a request already, then check to see
                    // current request in list is for the same direction as is 
                    // being answered already
                    else if (CurrDirection == CurrentDirReq)
                    {
                        // If direction does match that of the lift being travelled
                        // then check to see if that floor has been passed already
                        // This also requires determining which direction the lift

                        // Check to see when lift is traveling down whether the
                        // floorNumber of the request being examined is less than
                        // that which the lift is currently at
                        if (CurrentFloorReq < CurrFloorNum & CurrDirection == "Down")
                        {
                            // Return floor number and direction for request
                            // to be answered
                            var Floor = this[index].Item1;
                            var Direction = this[index].Item2;
                            RequestRet = new Tuple<int, string>(Floor, Direction);
                            break;
                        }
                        // Check to see when lift is traveling up whether the
                        // floorNumber of the request being examined is greater than
                        // that which the lift is currently at
                        else if (CurrentFloorReq > CurrFloorNum & CurrDirection == "Up")
                        {
                            // Return floor number and direction for request
                            // to be answered
                            var Floor = this[index].Item1;
                            var Direction = this[index].Item2;
                            RequestRet = new Tuple<int, string>(Floor, Direction);
                            break;
                        }
                        // Else floor has been passed already and cannot be answered on
                        // this lift run

                    }
                    // Else request is for the opposite direction to that which the lift
                    // is already traveling.

                }
                // If entire list is gone through and no request can be picked up
                // then zeros are returned in order and the request ignored
                return RequestRet;
            }
        }
    }

    // Class that will represent actual lift being operated. The lift will
    // fetch requests from the FloorRequests class and store floor requests 
    // in it's own private list.
    public class Lift
    {
        // Variables used to deterine where lift is and which direction
        // it is moving in
        protected int CurrFloorNum;
        protected int CurrHeight;
        protected string CurrDirection;
        protected string CurrStatus;
        // Variable to determine which floor lift will stop at next
        protected int IntendedFloor;
        // Create tuple list for reference to instance of central list
        protected FloorRequestList<int, string> Req1;
        // Create tuple list for storing of local requests
        protected FloorRequestList<int, string> LocalFlr;

        private BackgroundWorker MovementThread = new BackgroundWorker();

        // ************ InitReqEvent Event ***********
        // New event for when the first floor request is made
        public event DoWorkEventHandler InitReqEvent;

        public virtual void OnInitReqEvent(DoWorkEventArgs e)
        {
            if (InitReqEvent != null)
            {
                InitReqEvent(this, e);
            }
        }

        // ************ LiftMoveEvent Event ***********
        // New event for when the first floor request is made
        public event LiftMoveDel<LiftMoveArgs> LiftMoveEvent;

        protected virtual void OnLiftMoveEvent(LiftMoveArgs e)
        {
            if (LiftMoveEvent != null)
                LiftMoveEvent(this, e);
        }
            
        // Constructor class intialises all variables to zero/null/still
        // this set the lift to a starting position of the ground floor and
        // not moving
        public Lift(FloorRequestList<int, string> CentralList)
        {
            // Asissign reference for Central request list to local instance
            Req1 = CentralList;

            CurrFloorNum = 0;
            CurrHeight = 0;
            CurrDirection = "Still";
            CurrStatus = "NotMoving";
            IntendedFloor = 0;
            // Local list for storing Floor requests being addressed
            // by this lift and request Tuple for handling request created
            LocalFlr = new FloorRequestList<int, string>();

            // When either list is modified an event will be generated and method called
            // When an event is added to the central list then all lifts need to check
            // the list to see if the new request is relevant to them
            Req1.FloorReqEvent += (Sender, e) => GetFloorRequests();

            // If a local request is add then the GetIntended height method
            // needs to be run as if the new requested floor is before the previously
            // next request floor then the intended height will need to be changed
            LocalFlr.FloorReqEvent += (Sender, e) => GetIntendedFloor();

            // When the first request is added to the local list, then the 
            // move lift method shall be initiated with an InitReqEvent Event
            // This method will continue to loop untill all requests have been answered
            MovementThread.WorkerSupportsCancellation = true;
            MovementThread.WorkerReportsProgress = true;

            MovementThread.DoWork += new DoWorkEventHandler(LiftMove);
            MovementThread.ProgressChanged += new ProgressChangedEventHandler(MovementThread_ProgressChanged);

            InitReqEvent += (Sender, e) => this.MovementThread.RunWorkerAsync();
        }

        // Use the process changed method from the backgroud worker thread
        // to keep the lift panel updated with the lifts location.
        private void MovementThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnLiftMoveEvent(new LiftMoveArgs(e.ProgressPercentage, CurrFloorNum, CurrDirection));
        }

        // ************** GetFloorRequests Method ****************
        // Method to get floor requests from main list and to save them
        // into the local list for the lift to answer
        protected void GetFloorRequests()
        {
            // Create temporary variable for saving request data
            var FloorNumReq = 0;
            var DirecReq = "";

            // If Direction is null (i.e. not a valid request) then do not loop
            while (Req1.ReturnRequest(CurrFloorNum, CurrDirection).Item2 != null)
            {
                // Return values for request that matches direction currently traveling in
                FloorNumReq = Req1.ReturnRequest(CurrFloorNum, CurrDirection).Item1;
                DirecReq = Req1.ReturnRequest(CurrFloorNum, CurrDirection).Item2;
                // Add any request relevant to local list in order they can be
                // answerd by the lift created
                LocalFlr.AddRequest(FloorNumReq, DirecReq);
                if (LocalFlr.Count == 1) ////& CurrDirection == "Still")
                {
                    if (MovementThread.IsBusy != true)
                    {
                        MovementThread.RunWorkerAsync();
                    }
                }
                // Then remove request from main list in order that it doesn't
                // get picked up mutliple times.
                Req1.RemRequest(FloorNumReq, DirecReq);
            }
        }

        // ************** GetLiftData Method ****************
        // Return Floor and direction of lifts current status
        public Tuple<int, string, string> GetLiftData()
        {
            return new Tuple<int, string, string>(this.CurrFloorNum, this.CurrDirection, this.CurrStatus);
        }

        // ************** GetIntendedHeight Method ****************
        // Determine which is the next floor to stop at accodring to height
        // i.e. the value to be stored to measure against the height of the lift
        protected void GetIntendedFloor()
        {
            // Method to set intended height in relation to the floor 
            // being requested
            IntendedFloor = LocalFlr[0].Item1;
        }

        // ************** AddLiftRequest Method ****************
        // Add Requests that are made from within the lift itself
        public void AddLiftRequest(int FloorNumReq)
        {
            var DirectionReq = "";

            if (CurrFloorNum > FloorNumReq)
                DirectionReq = "Down";
            else if (CurrFloorNum < FloorNumReq)
                DirectionReq = "Up";
            else
                DirectionReq = null;

            // Check to see if request made once inside
            // the lift matches that of the direction of the lift already
            // first checking if there are requests already in lical request
            // list
            if (LocalFlr.Count > 0)
            {
                // Check to see if direction request inside the lift
                // matches that of the direction the lift was already traveling in
                if (DirectionReq != LocalFlr[0].Item2)
                {
                    // If not then simply add to the back of the requests
                    LocalFlr.AddRequest(FloorNumReq, DirectionReq);
                }
                // Else request is going in same direction as lift is already
                // traveling and therefore needs to be added in order
                // in order with the rest of the list
                else
                {
                    // Then cycle through current list
                    // to find the appropriate place to insert the request
                    for (int index = 0; index < LocalFlr.Count; index++)
                    {
                        // If request is going up search for first instance of
                        // a request being greater than the one being added.
                        if (DirectionReq == "Up")
                        {
                            // At the first isntance of a request being greater
                            // then the one being added, insert the new request
                            if (LocalFlr[index].Item1 > FloorNumReq)
                            {
                                // This will then be in order with all other requests in the list
                                // Duplicates will not matter as when a request is answered every instance 
                                // the same as that one is also deleted
                                LocalFlr.Insert(index, new Tuple<int, string>(FloorNumReq, DirectionReq));
                            }
                        }
                        // If request is going down search for first instance of
                        // a request being lower than the one being added.
                        else if (DirectionReq == "Down")
                        {
                            // At the first isntance of a request being lower
                            // then the one being added, insert the new request
                            if (LocalFlr[index].Item1 < FloorNumReq)
                            {
                                // This will then be in order with all other requests in the list
                                // Duplicates will not matter as when a request is answered every instance 
                                // the same as that one is also deleted
                                LocalFlr.Insert(index, new Tuple<int, string>(FloorNumReq, DirectionReq));
                            }
                        }
                    }
                }
            }
            // if there are no request already in list then simply add to start
            // of local requet list.
            else 
            {
                // Then proceed as if it were a new request being added
                // from the central list.
                LocalFlr.AddRequest(FloorNumReq, DirectionReq);
                if (MovementThread.IsBusy != true)
                {
                    MovementThread.RunWorkerAsync();
                }
                // Then remove request from main list in order that it doesn't
                // get picked up mutliple times.
                Req1.RemRequest(FloorNumReq, DirectionReq);
            }
        }

        // ************* UpdateFloorNumber Method ***************
        // Method to update the floor number when lift passes past it
        public void UpdateFloorNumber(int NewFloorNumber)
        {
            CurrFloorNum = NewFloorNumber;
        }

        // ************** LiftMove method ***********************
        // When the first request is added to the local list, then the 
        // move lift method shall be initiated with an InitReqEvent Event
        // This method will continue to loop untill all requests have been answered
        private void LiftMove(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Make sure there is at least 1 item in the floor request list.
            while (LocalFlr.Count > 0)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Set current direction to that of the request being handled
                    CurrDirection = LocalFlr[0].Item2;

                    // Sort List into either ascending or descending
                    // order depending on which direciton the requests
                    // are in. First if for requests heading up
                    if (CurrDirection == "Up")
                    {
                        // Sort floor requests into ascending order so that they can be
                        // addressed appropriately.
                        LocalFlr.Sort((Sort, Asc) => Sort.Item1.CompareTo(Asc.Item1));
                    }
                    else if (CurrDirection == "Down")
                    {
                        // Sort floor requests into descending order so that they can be
                        // addressed appropriately.
                        LocalFlr.Sort((Sort, Desc) => Desc.Item1.CompareTo(Sort.Item1));
                    }

                    // Get the intended floor of the lift in order the lift
                    // will know when to stop moving
                    GetIntendedFloor();

                    // Change height until it's equal to the intended height
                    while (CurrFloorNum != IntendedFloor & LocalFlr[0].Item2 != null)
                    {
                        // Variable to indicate when the lift is moving
                        // and when it is still. Note this is different
                        // from the CurrDirection variable as the lift could
                        // still be intending to move upward for example, but 
                        // it is simply answering a request on the way.
                        CurrStatus = "Moving";

                        // If the floor request is to go up
                        if (IntendedFloor > CurrFloorNum)
                        {
                            CurrHeight++;
                            Thread.Sleep(10);
                        }
                        // If the floor request is to go down
                        else if (IntendedFloor < CurrFloorNum)
                        {
                            CurrHeight--;
                            Thread.Sleep(10);
                        }
                        // Leep updating lits location for panel simulation
                        worker.ReportProgress(CurrHeight);
                    }

                    // When the lift reaches the floor its request is responding
                    // then it's status is set to not moving but it's direction
                    // the same
                    CurrStatus = "NotMoving";
                    // Once the request has been answered then remove it
                    // from the floor request list
                    LocalFlr.RemRequest(CurrFloorNum, CurrDirection);
                    // Give final update upon reaching the floor the lift
                    // is responding to
                    worker.ReportProgress(CurrHeight);
                }
                // When LocalFlr.Count is not >0 then all requests have been answered
                // and the direction of the lift will then be set to "still"
                CurrDirection = "Still";
            }
        }
    }
}

namespace TestClass
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}

