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

namespace ReadWriteRFID
{
    /// <summary>
    /// Interaction logic for Select.xaml
    /// </summary>
    public partial class Select : Window
    {
        public Select()
        {
            InitializeComponent();
        }

        private void btnDaily_Click(object sender, RoutedEventArgs e)
        {
            Regular reg = new Regular();
            reg.Show();
            this.Close();
        }

        private void btnEvent_Click(object sender, RoutedEventArgs e)
        {
            AddEvent add = new AddEvent();
            add.Show();
            this.Close();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Home back = new Home();
            back.Show();
            this.Close();
        }
    }
}
