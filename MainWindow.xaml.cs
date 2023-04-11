using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Data.OleDb;





namespace ReadWriteRFID
{
    /// <summary>
    //
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
       
        string strName, imageName;
      

        int retCode;
        int hCard;
        int hContext;
        int Protocol;
        public bool connActive = false;
        string readername = "ACS ACR122 0";      // change depending on reader
        public byte[] SendBuff = new byte[263];
        public byte[] RecvBuff = new byte[263];
        public int SendLen, RecvLen, nBytesRet, reqType, Aprotocol, dwProtocol, cbPciLength;
        public Card.SCARD_READERSTATE RdrState;
        public Card.SCARD_IO_REQUEST pioSendRequest;
        private BackgroundWorker _worker;
        private Card.SCARD_READERSTATE[] states;

        public MainWindow()
        {
            InitializeComponent();
            SelectDevice();
            establishContext();
            ReadData();
           
        }

   
        


        internal enum SmartcardState
        {
            None = 0,
            Inserted = 1,
            Ejected = 2
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
               
            insertImageData();  
             

             
        }

        private void insertImageData()
        {
            try
            {
                if (txtCard.Text != "")
            {
                
                    //Initialize a file stream to read the image file
                    System.IO.FileStream fs = new System.IO.FileStream(imageName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    //Initialize a byte array with size of stream
                    byte[] imgByteArr = new byte[fs.Length];

                    //Read data from the file stream and put into the byte array
                    fs.Read(imgByteArr, 0, Convert.ToInt32(fs.Length));

                    //Close a file stream
                    fs.Close();

                     OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
                     OleDbCommand cmd = con.CreateCommand();
                     con.Open();
                     cmd.CommandText = "insert into tbl_Details(UID, StudentName, Course, Fines, StudentNum, ImageCover) values('" + txtCard.Text + "','" + txtStudent.Text + "','" + txtCourse.Text + "','" + tbAll.Text + "','" + txtStudentNum.Text + "', @img )";

                            //Pass byte array into database
                            cmd.Parameters.Add(new OleDbParameter("img", imgByteArr));
                            int result = cmd.ExecuteNonQuery();
                            if (result == 1)
                            {
                                MessageBox.Show("Record Added Successful.");
                                ClearAll();
                            }
                            
                        }
                        
                
                  }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
       
        private void ClearAll()
        {
            txtCard.Text = "";
            txtStudent.Text = "";
            txtCourse.Text = "";
            txtStudentNum.Text = "";
            ImageViewer.Source = null;
            
    
        }

        public void SelectDevice()
        {
            List<string> availableReaders = this.ListReaders();
            if (availableReaders.Count == 0) { return; }

            this.RdrState = new Card.SCARD_READERSTATE();
            readername = availableReaders[0].ToString();//selecting first device
            this.RdrState.RdrName = readername;

            ///
            states = new Card.SCARD_READERSTATE[1];
            states[0] = new Card.SCARD_READERSTATE();
            states[0].RdrName = readername;
            //  states[0].UserData = IntPtr
            states[0].RdrCurrState = Card.SCARD_STATE_EMPTY;
            states[0].RdrEventState = 0;
            states[0].ATRLength = 0;
            states[0].ATRValue = null;


            if (availableReaders.Count > 0)
            {
                this._worker = new BackgroundWorker();
                this._worker.WorkerSupportsCancellation = true;
                this._worker.DoWork += WaitChangeStatus1;
                this._worker.RunWorkerAsync();
            }
        }
        private void WaitChangeStatus1(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                int nErrCode = Card.SCardGetStatusChange(hContext, 1000, ref states[0], 1);

                //Check if the state changed from the last time.
                if ((this.states[0].RdrEventState & 2) == 2)
                {
                    //Check what changed.
                    SmartcardState state = SmartcardState.None;
                    if ((this.states[0].RdrEventState & 32) == 32 && (this.states[0].RdrCurrState & 32) != 32)
                    {
                        //The card was inserted. 
                        state = SmartcardState.Inserted;
                    }
                    else if ((this.states[0].RdrEventState & 16) == 16 && (this.states[0].RdrCurrState & 16) != 16)
                    {
                        //The card was ejected.
                        state = SmartcardState.Ejected;
                    }
                    if (state != SmartcardState.None && this.states[0].RdrCurrState != 0)
                    {
                        switch (state)
                        {
                            case SmartcardState.Inserted:
                                {

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        txtCard.Text = "";
                                        ReadData();
                                    });
                                    break;
                                }
                            case SmartcardState.Ejected:
                                {



                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    //Update the current state for the next time they are checked.
                    this.states[0].RdrCurrState = this.states[0].RdrEventState;
                }

            }

        }
        public void ReadData()
        {
            if (connectCard())
            {
                string cardUID = getcardUID();
                txtCard.Text = cardUID; //displaying on text block
            }
            
        }

        public string verifyCard(String Block)
        {

            string value = "";

            if (connectCard())
            {
                value = readBlock(Block);
            }

            value = value.Split(new char[] { '\0' }, 2, StringSplitOptions.None)[0].ToString();
            return value;

        }
        public string readBlock(String Block)

        {

            string tmpStr = "";
            int indx;

            if (authenticateBlock1(Block))
            {

                ClearBuffers1();
                SendBuff[0] = 0xFF; // CLA 
                SendBuff[1] = 0xB0;// INS
                SendBuff[2] = 0x00;// P1
                SendBuff[3] = (byte)int.Parse(Block);// P2 : Block No.
                SendBuff[4] = (byte)int.Parse("16");// Le

                SendLen = 5;

                RecvLen = SendBuff[4] + 2;

                retCode = SendAPDUandDisplay(2);


                  if (retCode == -200)
                {

                    return "outofrangeexception";

                }

                else if (retCode == -202)
                {

                    return "BytesNotAcceptable";
                }

                else if (retCode != Card.SCARD_S_SUCCESS)
                {

                    return "FailRead";
                }


                  // Display data in text format
                  for (indx = 0; indx <= RecvLen - 1; indx++)
                  {
                      tmpStr = tmpStr + Convert.ToInt32(RecvBuff[indx]);

                  }


                  return (tmpStr);
              

            }



            else
            {
                // Display data in text format
                for (indx = 0; indx <= RecvLen - 1; indx++)
                {
                    tmpStr = tmpStr + Convert.ToInt32(RecvBuff[indx]);

                }


                return (tmpStr);
               
            }



        }
        public List<string> ListReaders()
        {
            int ReaderCount = 0;
            List<string> AvailableReaderList = new List<string>();

            //Make sure a context has been established before 
            //retrieving the list of smartcard readers.
            retCode = Card.SCardListReaders(hContext, null, null, ref ReaderCount);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode));
                //connActive = false;
            }

            byte[] ReadersList = new byte[ReaderCount];

            //Get the list of reader present again but this time add sReaderGroup, retData as 2rd & 3rd parameter respectively.
            retCode = Card.SCardListReaders(hContext, null, ReadersList, ref ReaderCount);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode));
            }

            string rName = "";
            int indx = 0;
            if (ReaderCount > 0)
            {
                // Convert reader buffer to string
                while (ReadersList[indx] != 0)
                {

                    while (ReadersList[indx] != 0)
                    {
                        rName = rName + (char)ReadersList[indx];
                        indx = indx + 1;
                    }

                    //Add reader name to list
                    AvailableReaderList.Add(rName);
                    rName = "";
                    indx = indx + 1;

                }
            }
            return AvailableReaderList;

        }
        internal void establishContext()
        {
            retCode = Card.SCardEstablishContext(Card.SCARD_SCOPE_SYSTEM, 0, 0, ref hContext);
            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show("Check your device and please restart again", "Reader not connected");
                connActive = false;
                return;
            }
        }
        public bool connectCard()
        {
            connActive = true;

            retCode = Card.SCardConnect(hContext, readername, Card.SCARD_SHARE_SHARED,
                      Card.SCARD_PROTOCOL_T0 | Card.SCARD_PROTOCOL_T1, ref hCard, ref Protocol);

            if (retCode != Card.SCARD_S_SUCCESS)
                
            {
                /*
                MessageBox.Show(Card.GetScardErrMsg(retCode), "Card not available");
                */
                connActive = false;
                return false;
            }
            
            return true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void submitText1(String Text, String Block)
        {

            String tmpStr = Text;
            int indx;
            if (authenticateBlock1(Block))
            {
                ClearBuffers1();
                SendBuff[0] = 0xFF;                             // CLA
                SendBuff[1] = 0xD6;                             // INS
                SendBuff[2] = 0x00;                             // P1
                SendBuff[3] = (byte)int.Parse(Block);           // P2 : Starting Block No.
                SendBuff[4] = (byte)int.Parse("16");            // P3 : Data length

                for (indx = 0; indx <= (tmpStr).Length - 1; indx++)
                {
                    SendBuff[indx + 5] = (byte)tmpStr[indx];
                }
                SendLen = SendBuff[4] + 5;
                RecvLen = 0x02;

                retCode = SendAPDUandDisplay(2);

                if (retCode != Card.SCARD_S_SUCCESS)
                {
                    MessageBox.Show("fail write");

                }
                else
                {
                    MessageBox.Show("write success");
                }
            }
            else
            {
                MessageBox.Show("FailAuthentication");
            }
        }

       

        public bool authenticateBlock1(String block)
        {
            ClearBuffers1();
            SendBuff[0] = 0xFF;                         // CLA
            SendBuff[2] = 0x00;                         // P1: same for all source types 
            SendBuff[1] = 0x86;                         // INS: for stored key input
            SendBuff[3] = 0x00;                         // P2 : Memory location;  P2: for stored key input
            SendBuff[4] = 0x05;                         // P3: for stored key input
            SendBuff[5] = 0x01;                         // Byte 1: version number
            SendBuff[6] = 0x00;                         // Byte 2
            SendBuff[7] = (byte)int.Parse(block);       // Byte 3: sectore no. for stored key input
            SendBuff[8] = 0x60;                         // Byte 4 : Key A for stored key input
            SendBuff[9] = (byte)int.Parse("1");         // Byte 5 : Session key for non-volatile memory

            SendLen = 0x0A;
            RecvLen = 0x02;

            retCode = SendAPDUandDisplay(0);

            if (retCode != Card.SCARD_S_SUCCESS)
            {
                //MessageBox.Show("FAIL Authentication!");
                return false;
            }

            return true;
        }
        public void ClearBuffers1()
        {
            long indx;

            for (indx = 0; indx <= 262; indx++)
            {
                RecvBuff[indx] = 0;
                SendBuff[indx] = 0;
            }
        }
        public int SendAPDUandDisplay(int reqType)
        {
            int indx;
            string tmpStr = "";

            pioSendRequest.dwProtocol = Aprotocol;
            pioSendRequest.cbPciLength = 8;

            //Display Apdu In
            for (indx = 0; indx <= SendLen - 1; indx++)
            {
                tmpStr = tmpStr + " " + string.Format("{0:X2}", SendBuff[indx]);
            }

            retCode = Card.SCardTransmit(hCard, ref pioSendRequest, ref SendBuff[0],
                                 SendLen, ref pioSendRequest, ref RecvBuff[0], ref RecvLen);

            if (retCode != Card.SCARD_S_SUCCESS)
            {
                return retCode;
            }

            else
            {
                try
                {
                    tmpStr = "";
                    switch (reqType)
                    {
                        case 0:
                            for (indx = (RecvLen - 2); indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            if ((tmpStr).Trim() != "90 00")
                            {
                                //MessageBox.Show("Return bytes are not acceptable.");
                                return -202;
                            }

                            break;

                        case 1:

                            for (indx = (RecvLen - 2); indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            if (tmpStr.Trim() != "90 00")
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            else
                            {
                                tmpStr = "ATR : ";
                                for (indx = 0; indx <= (RecvLen - 3); indx++)
                                {
                                    tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                                }
                            }

                            break;

                        case 2:

                            for (indx = 0; indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    return -200;
                }
            }
            return retCode;
        }
        public void CardClose1()
        {
            if (connActive)
            {
                retCode = Card.SCardDisconnect(hCard, Card.SCARD_UNPOWER_CARD);
            }
            retCode = Card.SCardReleaseContext(hCard);
        }
        private void WriteCard1()
        {
            if (connectCard())// establish connection to the card: you've declared this from previous post
            {
                submitText1(txtCard.Text, "5"); // 5 - is the block we are writing data on the card
                CardClose1();

            }
        }
     

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Enter)
            {
                WriteCard1();
            }


        }

       

        public bool connectCard(String Text)
        {
            connActive = true;

            retCode = Card.SCardConnect(hContext, readername, Card.SCARD_SHARE_SHARED,
                      Card.SCARD_PROTOCOL_T0 | Card.SCARD_PROTOCOL_T1, ref hCard, ref Protocol);

            if (retCode != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode), "Card not available", MessageBoxButton.OK, MessageBoxImage.Error);
                connActive = false;
                return false;
            }
            return true;
        }

        private string getcardUID()//only for mifare 1k cards
        {
            string cardUID = "";
            byte[] receivedUID = new byte[256];
            Card.SCARD_IO_REQUEST request = new Card.SCARD_IO_REQUEST();
            request.dwProtocol = Card.SCARD_PROTOCOL_T1;
            request.cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Card.SCARD_IO_REQUEST));
            byte[] sendBytes = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; //get UID command      for Mifare cards
            int outBytes = receivedUID.Length;
            int status = Card.SCardTransmit(hCard, ref request, ref sendBytes[0], sendBytes.Length, ref request, ref receivedUID[0], ref outBytes);

            if (status != Card.SCARD_S_SUCCESS)
            {
                cardUID = "Error";
            }
            else
            {
                cardUID = BitConverter.ToString(receivedUID.Take(4).ToArray()).Replace("-", string.Empty).ToLower();
            }

            return cardUID;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileDialog fldlg = new OpenFileDialog();
                fldlg.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
                fldlg.Filter = "Image File (*.jpg;*.bmp;*.gif)|*.jpg;*.bmp;*.gif";
                fldlg.ShowDialog();
                {
                    strName = fldlg.SafeFileName;
                    imageName = fldlg.FileName;
                    ImageSourceConverter isc = new ImageSourceConverter();
                    ImageViewer.SetValue(Image.SourceProperty, isc.ConvertFromString(imageName));
                }
                fldlg = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
       
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home back = new Home();
            back.Show();
            this.Close();

        }

        private void txtStudentNum_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
            OleDbCommand cmd = con.CreateCommand();
            con.Open();

            cmd = new OleDbCommand("select * from tbl_Students", con);
            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (txtStudentNum.Text == reader[3].ToString())
                {
                    txtStudent.Text = (reader[1].ToString());
                    txtCourse.Text = (reader[2].ToString());
                }
               
            }


            reader.Close();
            con.Close();
        }

        
        

    }
}
