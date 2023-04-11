﻿using System;
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
using System.Windows.Shapes;
using System.Data.OleDb;
using System.IO;

namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    ///  
        

    public partial class Update : Window
    {
        

        int retCode1;
        int hCard1;
        int hContext1;
        int Protocol1;
        public bool connActive1 = false;
        string readername1 = "ACS ACR122 0";      // change depending on reader
        public byte[] SendBuff1 = new byte[263];
        public byte[] RecvBuff1 = new byte[263];
        public int SendLen1, RecvLen1, nBytesRet1, reqType1, Aprotocol1, dwProtocol1, cbPciLength1;
        public Card.SCARD_READERSTATE RdrState1;
        public Card.SCARD_IO_REQUEST pioSendRequest1;
        private System.ComponentModel.BackgroundWorker _worker1;
        private Card.SCARD_READERSTATE[] states1;
        public Update()
        {
            InitializeComponent();
            SelectDevice1();
            establishContext1();
            ReadData1();
        }

        internal enum SmartcardState1
        {
            None = 0,
            Inserted = 1,
            Ejected = 2
        }
       
        
        public void SelectDevice1()
        {
            List<string> availableReaders = this.ListReaders();
            if (availableReaders.Count == 0) { return; }

            this.RdrState1 = new Card.SCARD_READERSTATE();
            readername1 = availableReaders[0].ToString();//selecting first device
            this.RdrState1.RdrName = readername1;

            ///
            states1 = new Card.SCARD_READERSTATE[1];
            states1[0] = new Card.SCARD_READERSTATE();
            states1[0].RdrName = readername1;
            //  states[0].UserData = IntPtr
            states1[0].RdrCurrState = Card.SCARD_STATE_EMPTY;
            states1[0].RdrEventState = 0;
            states1[0].ATRLength = 0;
            states1[0].ATRValue = null;


            if (availableReaders.Count > 0)
            {
                this._worker1 = new System.ComponentModel.BackgroundWorker();
                this._worker1.WorkerSupportsCancellation = true;
                this._worker1.DoWork += WaitChangeStatus1;
                this._worker1.RunWorkerAsync();
            }
        }
        private void WaitChangeStatus1(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                int nErrCode = Card.SCardGetStatusChange(hContext1, 1000, ref states1[0], 1);

                //Check if the state changed from the last time.
                if ((this.states1[0].RdrEventState & 2) == 2)
                {
                    //Check what changed.
                    SmartcardState1 state = SmartcardState1.None;
                    if ((this.states1[0].RdrEventState & 32) == 32 && (this.states1[0].RdrCurrState & 32) != 32)
                    {
                        //The card was inserted. 
                        state = SmartcardState1.Inserted;
                    }
                    else if ((this.states1[0].RdrEventState & 16) == 16 && (this.states1[0].RdrCurrState & 16) != 16)
                    {
                        //The card was ejected.
                        state = SmartcardState1.Ejected;
                    }
                    if (state != SmartcardState1.None && this.states1[0].RdrCurrState != 0)
                    {
                        switch (state)
                        {
                            case SmartcardState1.Inserted:
                                {

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        txtCardUID.Text = "";
                                        ReadData1();
                                    });
                                    break;
                                }
                            case SmartcardState1.Ejected:
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
                    this.states1[0].RdrCurrState = this.states1[0].RdrEventState;
                }

            }

        }
        public void ReadData1()
        {
            if (connectCard())
            {
                string cardUID = getcardUID();
                txtCardUID.Text = cardUID; //displaying on text block
            }

            if (txtCardUID.Text != "")
            {
                
                OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
                OleDbCommand cmd = con.CreateCommand();
                con.Open();

               
                cmd = new OleDbCommand("select * from tbl_Details where UID ='" + txtCardUID.Text + "'", con);
                 OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {


                    txtNumber.Text = (reader[4].ToString());
                    txtStudentName.Text = (reader[2].ToString());
                    txtCourse.Text = (reader[3].ToString());

                    if (reader.IsDBNull(5))
                          imageViewer.Source = null;   
                    else
                        imageViewer.Source = BytesToImage((byte[])reader.GetValue(5));

                   
                }
                reader.Close();
                con.Close();
            }
            
        }

        // Convert a byte array into a BitmapImage.
        private static BitmapImage BytesToImage(byte[] bytes)
        {
            var bm = new BitmapImage();
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes))
            {
                stream.Position = 0;
                bm.BeginInit();
                bm.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bm.CacheOption = BitmapCacheOption.OnLoad;
                bm.UriSource = null;
                bm.StreamSource = stream;
                bm.EndInit();
            }
            return bm;
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
                SendBuff1[0] = 0xFF; // CLA 
                SendBuff1[1] = 0xB0;// INS
                SendBuff1[2] = 0x00;// P1
                SendBuff1[3] = (byte)int.Parse(Block);// P2 : Block No.
                SendBuff1[4] = (byte)int.Parse("16");// Le

                SendLen1 = 5;

                RecvLen1 = SendBuff1[4] + 2;

                retCode1 = SendAPDUandDisplay(2);


                if (retCode1 == -200)
                {

                    return "outofrangeexception";

                }

                else if (retCode1 == -202)
                {

                    return "BytesNotAcceptable";
                }

                else if (retCode1 != Card.SCARD_S_SUCCESS)
                {

                    return "FailRead";
                }


                // Display data in text format
                for (indx = 0; indx <= RecvLen1 - 1; indx++)
                {
                    tmpStr = tmpStr + Convert.ToInt32(RecvBuff1[indx]);

                }


                return (tmpStr);


            }



            else
            {
                // Display data in text format
                for (indx = 0; indx <= RecvLen1 - 1; indx++)
                {
                    tmpStr = tmpStr + Convert.ToInt32(RecvBuff1[indx]);

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
            retCode1 = Card.SCardListReaders(hContext1, null, null, ref ReaderCount);
            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode1));
                //connActive = false;
            }

            byte[] ReadersList = new byte[ReaderCount];

            //Get the list of reader present again but this time add sReaderGroup, retData as 2rd & 3rd parameter respectively.
            retCode1 = Card.SCardListReaders(hContext1, null, ReadersList, ref ReaderCount);
            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode1));
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
        internal void establishContext1()
        {
            retCode1 = Card.SCardEstablishContext(Card.SCARD_SCOPE_SYSTEM, 0, 0, ref hContext1);
            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show("Check your device and please restart again", "Reader not connected");
                connActive1 = false;
                return;
            }
        }
        public bool connectCard()
        {
            connActive1 = true;

            retCode1 = Card.SCardConnect(hContext1, readername1, Card.SCARD_SHARE_SHARED,
                      Card.SCARD_PROTOCOL_T0 | Card.SCARD_PROTOCOL_T1, ref hCard1, ref Protocol1);

            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
              
                connActive1 = false;
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
                SendBuff1[0] = 0xFF;                             // CLA
                SendBuff1[1] = 0xD6;                             // INS
                SendBuff1[2] = 0x00;                             // P1
                SendBuff1[3] = (byte)int.Parse(Block);           // P2 : Starting Block No.
                SendBuff1[4] = (byte)int.Parse("16");            // P3 : Data length

                for (indx = 0; indx <= (tmpStr).Length - 1; indx++)
                {
                    SendBuff1[indx + 5] = (byte)tmpStr[indx];
                }
                SendLen1 = SendBuff1[4] + 5;
                RecvLen1 = 0x02;

                retCode1 = SendAPDUandDisplay(2);

                if (retCode1 != Card.SCARD_S_SUCCESS)
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

        private void btnRegister_Click_1(object sender, RoutedEventArgs e)
        {

        }

        public bool authenticateBlock1(String block)
        {
            ClearBuffers1();
            SendBuff1[0] = 0xFF;                         // CLA
            SendBuff1[2] = 0x00;                         // P1: same for all source types 
            SendBuff1[1] = 0x86;                         // INS: for stored key input
            SendBuff1[3] = 0x00;                         // P2 : Memory location;  P2: for stored key input
            SendBuff1[4] = 0x05;                         // P3: for stored key input
            SendBuff1[5] = 0x01;                         // Byte 1: version number
            SendBuff1[6] = 0x00;                         // Byte 2
            SendBuff1[7] = (byte)int.Parse(block);       // Byte 3: sectore no. for stored key input
            SendBuff1[8] = 0x60;                         // Byte 4 : Key A for stored key input
            SendBuff1[9] = (byte)int.Parse("1");         // Byte 5 : Session key for non-volatile memory

            SendLen1 = 0x0A;
            RecvLen1 = 0x02;

            retCode1 = SendAPDUandDisplay(0);

            if (retCode1 != Card.SCARD_S_SUCCESS)
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
                RecvBuff1[indx] = 0;
                SendBuff1[indx] = 0;
            }
        }
        public int SendAPDUandDisplay(int reqType)
        {
            int indx;
            string tmpStr = "";

            pioSendRequest1.dwProtocol = Aprotocol1;
            pioSendRequest1.cbPciLength = 8;

            //Display Apdu In
            for (indx = 0; indx <= SendLen1 - 1; indx++)
            {
                tmpStr = tmpStr + " " + string.Format("{0:X2}", SendBuff1[indx]);
            }
            
            retCode1 = Card.SCardTransmit(hCard1, ref pioSendRequest1, ref SendBuff1[0],
                                 SendLen1, ref pioSendRequest1, ref RecvBuff1[0], ref RecvLen1);

            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
                return retCode1;
            }

            else
            {
                try
                {
                    tmpStr = "";
                    switch (reqType)
                    {
                        case 0:
                            for (indx = (RecvLen1 - 2); indx <= (RecvLen1 - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff1[indx]);
                            }

                            if ((tmpStr).Trim() != "90 00")
                            {
                                //MessageBox.Show("Return bytes are not acceptable.");
                                return -202;
                            }

                            break;

                        case 1:

                            for (indx = (RecvLen1 - 2); indx <= (RecvLen1 - 1); indx++)
                            {
                                tmpStr = tmpStr + string.Format("{0:X2}", RecvBuff1[indx]);
                            }

                            if (tmpStr.Trim() != "90 00")
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff1[indx]);
                            }

                            else
                            {
                                tmpStr = "ATR : ";
                                for (indx = 0; indx <= (RecvLen1 - 3); indx++)
                                {
                                    tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff1[indx]);
                                }
                            }

                            break;

                        case 2:

                            for (indx = 0; indx <= (RecvLen1 - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff1[indx]);
                            }

                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    return -200;
                }
            }
            return retCode1;
        }
        public void CardClose1()
        {
            if (connActive1)
            {
                retCode1 = Card.SCardDisconnect(hCard1, Card.SCARD_UNPOWER_CARD);
            }
            retCode1 = Card.SCardReleaseContext(hCard1);
        }
        private void WriteCard1()
        {
            if (connectCard())// establish connection to the card: you've declared this from previous post
            {
                submitText1(txtCardUID.Text, "5"); // 5 - is the block we are writing data on the card
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
            connActive1 = true;

            retCode1 = Card.SCardConnect(hContext1, readername1, Card.SCARD_SHARE_SHARED,
                      Card.SCARD_PROTOCOL_T0 | Card.SCARD_PROTOCOL_T1, ref hCard1, ref Protocol1);

            if (retCode1 != Card.SCARD_S_SUCCESS)
            {
                MessageBox.Show(Card.GetScardErrMsg(retCode1), "Card not available", MessageBoxButton.OK, MessageBoxImage.Error);
                connActive1 = false;
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
            int status = Card.SCardTransmit(hCard1, ref request, ref sendBytes[0], sendBytes.Length, ref request, ref receivedUID[0], ref outBytes);

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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtCardUID.Text != "")
            {
               

                OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");

                OleDbCommand cmd = con.CreateCommand();
                con.Open();
                if (txtStudentName.Text != "")
                {
                    cmd = new OleDbCommand("Update tbl_Details SET StudentName='" + this.txtStudentName.Text + "' where UID='" + this.txtCardUID.Text + "'", con);
                    cmd.ExecuteNonQuery();
                }

                if (txtCourse.Text != "")
                {
                    cmd = new OleDbCommand("Update tbl_Details SET Course='" + this.txtCourse.Text + "' where UID='" + this.txtCardUID.Text + "'", con);
                    cmd.ExecuteNonQuery();
                }

                if (txtNumber.Text != "")
                {
                    cmd = new OleDbCommand("Update tbl_Details SET StudentNum='" + this.txtNumber.Text + "' where UID='" + this.txtCardUID.Text + "'", con);
                    cmd.ExecuteNonQuery();
                }
               
                int result = cmd.ExecuteNonQuery();
                if (result == 1)
                {
                    MessageBox.Show("Record Updated Successful.");

                }

                con.Close();
                txtCardUID.Text = "";
                txtStudentName.Text = "";
                txtCourse.Text = "";
                txtNumber.Text = "";
                imageViewer.Source = null;



            }
            else
            {
                MessageBox.Show("Data was not saved...");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");

            OleDbCommand cmd = con.CreateCommand();
            con.Open();

            if (txtCardUID.Text != "")
            {
                cmd = new OleDbCommand("DELETE FROM tbl_details WHERE UID ='" + this.txtCardUID.Text + "'", con);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Data Deleted", "Deleted", MessageBoxButton.OKCancel);
                con.Close();

                txtCardUID.Text = "";
                txtStudentName.Text = "";
                txtCourse.Text = "";
                txtNumber.Text = "";
                imageViewer.Source = null;

            }

        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home aizen2 = new Home();
            aizen2.Show();
            this.Close();
        }

        
        

    }
}
