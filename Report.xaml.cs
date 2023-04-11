using System;
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
using System.Data;
using System.IO;



using Excel = Microsoft.Office.Interop.Excel;





namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        OleDbConnection con;
        DataTable dt;
        public Report()
        {
            InitializeComponent();

            con = new OleDbConnection();
            con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb";

           
            
        }

        private void BindGrid()
        {
            OleDbCommand cmd = new OleDbCommand();
            if (con.State != ConnectionState.Open)
                con.Open();
            cmd.Connection = con;
            cmd.CommandText = "select UID, StudentName, StudentNum, Course, Events, Login, Logout, Fines from tbl_Details";
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            gvData.ItemsSource = dt.AsDataView();

            if (dt.Rows.Count > 0)
            {
             
                gvData.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
               
                gvData.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void BindGrid1()
        {
            OleDbCommand cmd = new OleDbCommand();
            if (con.State != ConnectionState.Open)
                con.Open();
            cmd.Connection = con;
            cmd.CommandText = "select UID, StudentName, StudentNum, Course, AttendanceDate, Login, Logout  from tbl_Regular";
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            dt = new DataTable();
            da.Fill(dt);
            gvData.ItemsSource = dt.AsDataView();

            if (dt.Rows.Count > 0)
            {

                gvData.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {

                gvData.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home aizen1 = new Home();
            aizen1.Show();
            this.Close();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
           
            ExportToExcel();

            OleDbConnection con1 = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
            OleDbCommand cmd1 = con1.CreateCommand();
            con1.Open();
            cmd1.CommandText = "select * from tbl_Details";
            OleDbDataReader reader2 = cmd1.ExecuteReader();
            while (reader2.Read())
            {


                cmd1 = new OleDbCommand("Update tbl_Details SET Events='" + this.tbEvent.Text + "' where UID='" + reader2[1] + "'", con1);
                cmd1.ExecuteNonQuery();
                cmd1 = new OleDbCommand("Update tbl_Details SET Login='" + this.tbInOut.Text + "' where UID='" + reader2[1] + "'", con1);
                cmd1.ExecuteNonQuery();
                cmd1 = new OleDbCommand("Update tbl_Details SET Logout='" + this.tbInOut.Text + "' where UID='" + reader2[1] + "'", con1);
                cmd1.ExecuteNonQuery();
                cmd1 = new OleDbCommand("Update tbl_Details SET Fines='" + this.tbFines.Text + "' where UID='" + reader2[1] + "'", con1);
                cmd1.ExecuteNonQuery();


            }
            reader2.Close();
            con1.Close();
        }

       
     
        private void ExportToExcel()
        {
            
             gvData.SelectAllCells();
             gvData.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
             ApplicationCommands.Copy.Execute(null, gvData);
             String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
             String result = (string)Clipboard.GetData(DataFormats.Text);
             gvData.UnselectAllCells();
            
         System.IO.StreamWriter file1 = new System.IO.StreamWriter(@"");
          
         file1.WriteLine(result.Replace(',', ' '));

             file1.Close();


             

            

             MessageBox.Show("File Export Successful.");
        }

       
          
       private void Window_Activated(object sender, EventArgs e)
       {
          
       }

       private void btnDaily_Click(object sender, RoutedEventArgs e)
       {
           con = new OleDbConnection();
           con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb";
           BindGrid1();
           BindComboBox(cbCourse);

           if (btnDaily.IsEnabled == true)
           {
               
              lblCourse.Visibility = System.Windows.Visibility.Visible;
               lblFilter.Visibility = System.Windows.Visibility.Visible;
               lblStud.Visibility = System.Windows.Visibility.Visible;
               tbStudentNumber.Visibility = System.Windows.Visibility.Visible;
               cbCourse.Visibility = System.Windows.Visibility.Visible;
               btnSearch.Visibility = System.Windows.Visibility.Visible;
               imgSearch.Visibility = System.Windows.Visibility.Visible;
           }

       }

       private void btnEvent_Click(object sender, RoutedEventArgs e)
       {
           con = new OleDbConnection();
           con.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb";
           BindGrid();
           if(btnSearch.IsEnabled == true)
           {
               lblCourse.Visibility = System.Windows.Visibility.Hidden;
               lblFilter.Visibility = System.Windows.Visibility.Hidden;
               lblStud.Visibility = System.Windows.Visibility.Hidden;
                tbStudentNumber.Visibility = System.Windows.Visibility.Hidden;
                 cbCourse.Visibility = System.Windows.Visibility.Hidden;
                btnSearch.Visibility = System.Windows.Visibility.Hidden;
                imgSearch.Visibility = System.Windows.Visibility.Hidden;
            }
           


       }
        public void BindComboBox(ComboBox cbCourse)
       {
             OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
                OleDbCommand cmd = con.CreateCommand();
                con.Open();
                     cmd = new OleDbCommand("select Course from tbl_Course ", con);
                     OleDbDataAdapter da1 = new OleDbDataAdapter(cmd);
                    DataSet dt1 = new DataSet();
                     da1.Fill(dt1, "tbl_Course");
                     cbCourse.ItemsSource = dt1.Tables[0].DefaultView;
                     cbCourse.DisplayMemberPath = dt1.Tables[0].Columns["Course"].ToString();
       }
        private void BtnDaily()
        {
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
            OleDbCommand cmd = con.CreateCommand();
            con.Open();

            cmd = new OleDbCommand("select * from tbl_Regular ", con);
            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {

                if (tbStudentNumber.Text == reader[3].ToString())
                {
                    OleDbCommand cmd1 = new OleDbCommand();
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    cmd1.Connection = con;
                    cmd1 = new OleDbCommand("select * from tbl_Regular where StudentNum ='" + tbStudentNumber.Text + "'", con);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd1);
                    dt = new DataTable();
                    da.Fill(dt);
                    gvData.ItemsSource = dt.AsDataView();

                    if (dt.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }
                    tbStudentNumber.Text = "";
                }
                if (cbCourse.Text == reader[4].ToString())
                {
                    OleDbCommand cmd2 = new OleDbCommand();
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    cmd2.Connection = con;
                    cmd2 = new OleDbCommand("select * from tbl_Regular where Course ='" + cbCourse.Text + "'", con);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd2);
                    dt = new DataTable();
                    da.Fill(dt);
                    gvData.ItemsSource = dt.AsDataView();

                    if (dt.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }

                }

                if (tbStudentNumber.Text == reader[3].ToString() && cbCourse.Text == reader[4].ToString())
                {
                    OleDbCommand cmd1 = new OleDbCommand();
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    cmd1.Connection = con;
                    cmd1 = new OleDbCommand("select * from tbl_Regular where StudentNum ='" + tbStudentNumber.Text + "'", con);
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd1);
                    dt = new DataTable();
                    da.Fill(dt);
                    gvData.ItemsSource = dt.AsDataView();

                    if (dt.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }
                    tbStudentNumber.Text = "";
                }


            }
            reader.Close();
            con.Close();
        }
        private void BtnEvent()
        {
            OleDbConnection con4 = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");
            OleDbCommand cmd4 = con4.CreateCommand();
            con4.Open();

            cmd4 = new OleDbCommand("select * from tbl_Details ", con4);
            OleDbDataReader reader4 = cmd4.ExecuteReader();
            while (reader4.Read())
            {

                if (tbStudentNumber.Text == reader4[4].ToString())
                {
                    OleDbCommand cmd5 = new OleDbCommand();
                    if (con4.State != ConnectionState.Open)
                        con4.Open();
                    cmd5.Connection = con4;
                    cmd5 = new OleDbCommand("select * from tbl_Details where StudentNum ='" + tbStudentNumber.Text + "'", con4);
                    OleDbDataAdapter da1 = new OleDbDataAdapter(cmd5);
                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    gvData.ItemsSource = dt1.AsDataView();

                    if (dt1.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }
                    tbStudentNumber.Text = "";
                }
                if (cbCourse.Text == reader4[3].ToString())
                {
                    OleDbCommand cmd5 = new OleDbCommand();
                    if (con4.State != ConnectionState.Open)
                        con4.Open();
                    cmd5.Connection = con4;
                    cmd5 = new OleDbCommand("select * from tbl_Details where Course ='" + cbCourse.Text + "'", con4);
                    OleDbDataAdapter da2 = new OleDbDataAdapter(cmd5);
                    DataTable dt2 = new DataTable();
                    da2.Fill(dt2);
                    gvData.ItemsSource = dt2.AsDataView();

                    if (dt2.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }

                }

                if (tbStudentNumber.Text == reader4[4].ToString() && cbCourse.Text == reader4[3].ToString())
                {
                    OleDbCommand cmd6 = new OleDbCommand();
                    if (con4.State != ConnectionState.Open)
                        con4.Open();
                    cmd6.Connection = con;
                    cmd6 = new OleDbCommand("select * from tbl_Details where StudentNum ='" + tbStudentNumber.Text + "'", con4);
                    OleDbDataAdapter da3 = new OleDbDataAdapter(cmd6);
                    DataTable dt3 = new DataTable();
                    da3.Fill(dt3);
                    gvData.ItemsSource = dt3.AsDataView();

                    if (dt3.Rows.Count > 0)
                    {

                        gvData.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {

                        gvData.Visibility = System.Windows.Visibility.Hidden;
                    }
                    tbStudentNumber.Text = "";
                }


            }
            reader4.Close();
            con.Close();
        }
       private void btnSearch_Click(object sender, RoutedEventArgs e)
       {
           if (btnDaily.IsEnabled == true)
           {
               BtnDaily();
           }
           if (btnDaily.IsEnabled == false)
           {
               BtnEvent();
           }
           
         
       }
    }
}
