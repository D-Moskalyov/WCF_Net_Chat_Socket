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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ChatInfoDAL;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        Socket reg_socket;

        public Registration()
        {
            InitializeComponent();

            Console.WriteLine("Reg Client Start");

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (name.Text != "" && pass1.Password != "" && pass2.Password != "")
                {
                    reg_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    //Console.WriteLine("Enter server IP:");
                    string ip_str = "localhost";

                    int port = 3000;

                    IPHostEntry ipList = Dns.Resolve(ip_str);
                    IPAddress ip = ipList.AddressList[0];
                    IPEndPoint endPoint = new IPEndPoint(ip, port);

                    reg_socket.Connect(endPoint);



                    RegInfo regInfo = new RegInfo(name.Text, pass1.Password, pass2.Password);
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new MemoryStream();

                    formatter.Serialize(stream, regInfo);

                    byte[] buffer2 = new byte[1024];
                    stream.Position = 0;
                    while (stream.Position < stream.Length)
                    {
                        int readCount = stream.Read(buffer2, 0, 1024);
                        reg_socket.Send(buffer2, readCount, 0);
                    }

                    byte[] answerFromServer = new byte[1024];
                    int resCount = reg_socket.Receive(answerFromServer);
                    string messFromServer = Encoding.UTF8.GetString(answerFromServer, 0, resCount);
                    if (messFromServer == "Добро пожаловать!")
                    {
                        MainWindow mainW = new MainWindow(regInfo.Name);
                        mainW.Show();
                        //mainW.name = regInfo.Name;

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(messFromServer);
                    }
                    reg_socket.Close();
                }
            }
            catch (SocketException exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void Label_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Login logW = new Login();
            logW.Show();
            this.Close();
        }
        
    }
}
