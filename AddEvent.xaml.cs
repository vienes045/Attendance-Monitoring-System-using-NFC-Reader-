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

namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for AddEvent.xaml
    /// </summary>
    public partial class AddEvent : Window
    {
        public AddEvent()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtEvent.Text != "")
            {
                OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb");

                OleDbCommand cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = "Insert into tbl_Events(Events)Values('" + txtEvent.Text + "')";
                cmd.Connection = con;
                cmd.ExecuteNonQuery();


                MessageBox.Show("An event has been added.");

                con.Close();
                txtEvent.Text = "";

                Display attendance = new Display();
                attendance.Show();
                this.Close();

            }
            else
            {
                MessageBox.Show("Enter an Event..");
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home aizen6 = new Home();
            aizen6.Show();
            this.Close();
        }
    }
}
