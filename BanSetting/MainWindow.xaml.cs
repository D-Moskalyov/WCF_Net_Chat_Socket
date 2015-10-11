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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BanSetting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Delegate d;

        public MainWindow(Delegate sender)
        {
            InitializeComponent();
            d = sender;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DateTime dT = (DateTime)(datePeacker.SelectedDate);
        }
    }
}
