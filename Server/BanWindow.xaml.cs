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

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для BanWindow.xaml
    /// </summary>
    public partial class BanWindow : Window
    {
        public BanWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (datePicker.SelectedDate != null)
            {
                this.Closed += ((MainWindow)(this.Owner)).banW_Closed;
                ((MainWindow)(this.Owner)).tempBan = (DateTime)(datePicker.SelectedDate);
            }
            this.Close();
        }
    }
}
