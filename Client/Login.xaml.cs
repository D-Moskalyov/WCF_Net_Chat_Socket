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
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        Socket log_socket;

        public Login()
        {
            InitializeComponent();

            Console.WriteLine("Log Client Start");

            
        }

        private void Label_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Registration regW = new Registration();
            regW.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (name.Text != "" && pass.Password != "")
                {
                    log_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    //Console.WriteLine("Enter server IP:");
                    string ip_str = "localhost";

                    int port = 4000;

                    IPHostEntry ipList = Dns.Resolve(ip_str);
                    IPAddress ip = ipList.AddressList[0];
                    IPEndPoint endPoint = new IPEndPoint(ip, port);

                    log_socket.Connect(endPoint);



                    LoginInfo logInfo = new LoginInfo(name.Text, pass.Password);
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new MemoryStream();

                    formatter.Serialize(stream, logInfo);

                    byte[] buffer2 = new byte[1024];
                    stream.Position = 0;
                    while (stream.Position < stream.Length)
                    {
                        int readCount = stream.Read(buffer2, 0, 1024);
                        log_socket.Send(buffer2, readCount, 0);
                    }

                    byte[] answerFromServer = new byte[1024];
                    int resCount = log_socket.Receive(answerFromServer);
                    string messFromServer = Encoding.UTF8.GetString(answerFromServer, 0, resCount);

                    if (messFromServer == "Добро пожаловать!")
                    {
                        MainWindow mainW = new MainWindow(logInfo.Name);
                        mainW.Show();
                        //mainW.name = logInfo.Name;

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(messFromServer);
                    }
                    log_socket.Close();
                }
            }
            catch (SocketException exp)
            {
                MessageBox.Show(exp.Message);
            }
        }
    }
}
