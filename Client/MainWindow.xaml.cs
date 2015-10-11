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
using System.ServiceModel;
using ChatInfoDAL;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using EventLib;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    //class ClientCallback : ICustomMessageCallback
    //{
    //    public void OnCallback(string message)
    //    {
    //        Console.WriteLine("yeah");
    //        MessageBox.Show(string.Format("Message from server {0}", message));
    //    }
    //}

    public partial class MainWindow : Window
    {
        public string name = string.Empty;
        static int myId;
        Socket client_socket;
        //Thread mainThread;

        //delegate void dasd(Message mess);

        List<Message> messages = new List<Message>();
        List<Group> groups = new List<Group>();
        List<User> users = new List<User>();
        //List<IndividualMessage> indMes = new List<IndividualMessage>();
        List<BanPair> banPairs = new List<BanPair>();
        List<MemberOfGroup> memeberOgGroups = new List<MemberOfGroup>();
        Dictionary<Message, StackPanel> dictMesStack = new Dictionary<Message, StackPanel>();
        //static StackPanel sP;

        public MainWindow(string n)
        {
            InitializeComponent();
            //mainThread = Thread.CurrentThread;
            //Console.WriteLine(UserManager.Users[0].NickName);
            name = n;
            Console.WriteLine("Start Client");
            this.Title = "User - " + name;
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Console.WriteLine("Enter server IP:");
            string ip_str = "localhost";

            int port = 2000;

            IPHostEntry ipList = Dns.Resolve(ip_str);
            IPAddress ip = ipList.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ip, port);

            client_socket.Connect(endPoint);

            byte[] introduce = Encoding.UTF8.GetBytes(name);
            client_socket.Send(introduce);

            byte[] myID = new byte[1024];
            int res = client_socket.Receive(myID);
            string ID = Encoding.UTF8.GetString(myID, 0, res);
            myId = int.Parse(ID);

            //PrimaryReciveByte();

            Thread ear = new Thread(EarMethod);
            //ear.SetApartmentState(ApartmentState.STA);
            ear.Start(client_socket);

        }

        void EarMethod(object obj)
        {
            Socket ear_socket = obj as Socket;

            try
            {
                string defTypeOper = "00";
                while (true)
                {
                    string typeOper;
                    if (defTypeOper == "00")
                    {
                        byte[] typeOperation = new byte[2];
                        //ear_socket.Blocking = false;
                        int byteCount = ear_socket.Receive(typeOperation);
                        //ear_socket.Blocking = true;
                        typeOper = Encoding.UTF8.GetString(typeOperation, 0, byteCount);
                        Console.WriteLine(typeOper);
                    }
                    else
                    {
                        typeOper = defTypeOper;
                        defTypeOper = "00";
                    }

                    byte[] info = new byte[1024];
                    Stream stream = new MemoryStream();
                    IFormatter formatter = new BinaryFormatter();

                    int resCount = 1024;
                    //client_socket.ReceiveTimeout = 500;
                    while (resCount == 1024)
                    {
                        //ear_socket.Blocking = false;
                        //Console.WriteLine(ear_socket.Available.ToString());
                        resCount = ear_socket.Receive(info);
                        if (resCount == 2 && (Encoding.UTF8.GetString(info, 0, resCount) == "01" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "02" || Encoding.UTF8.GetString(info, 0, resCount) == "03" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "04" || Encoding.UTF8.GetString(info, 0, resCount) == "05" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "06" || Encoding.UTF8.GetString(info, 0, resCount) == "07"))
                        {
                            defTypeOper = Encoding.UTF8.GetString(info, 0, resCount); break;
                        }
                        else { stream.Write(info, 0, resCount); }
                        //stream.Write(info, 0, resCount);
                    }
                    //ear_socket.Blocking = true;
                    switch (typeOper)
                    {
                        //case "00": { Method0(stream, formatter); break; }
                        case "01": { Method1(stream, formatter); break; }
                        case "02": { Method2(stream, formatter); break; }
                        case "03": { Method3(stream, formatter); break; }
                        case "04": { Method4(stream, formatter); break; }
                        case "05": { Method5(stream, formatter); break; }
                        case "06": { Method6(stream, formatter); break; }
                        case "07": { Method7(stream, formatter); break; }
                        case "08": { Method8(stream, formatter); break; }
                        case "09": { Method9(stream, formatter); break; }
                        case "10": { Method10(stream, formatter); break; }
                        case "11": { Method11(stream, formatter); break; }
                        default: { break; }
                    }
                }
            }
            catch (SocketException exp)
            {
                Console.WriteLine("Error. " + exp.Message);

                Action<string> action = MessageToInfoStackAdd;
                string messa = "Сервер отключился. Выход через 3 сек.";
                infoStack.Dispatcher.Invoke(action, messa);

                Thread.Sleep(3000);
                this.Dispatcher.Invoke(new Action(() => { this.Close(); }));
            }
        }

        void MessageToInfoStackAdd(string message)
        {
            TextBlock tBlock = new TextBlock();
            tBlock.Text = message;
            tBlock.TextWrapping = TextWrapping.Wrap;

            infoStack.Children.Add(tBlock);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //выйти из группы
            if (group1.SelectedValue.ToString() == "GENERAL")
            {
                MessageBox.Show("Вы не можете выйти из основной группы");
            }
            else
            {
                byte[] code = Encoding.UTF8.GetBytes("05");
                client_socket.Send(code);

                Group group = null;
                foreach (Group gr in groups)
                {
                    if (gr == group1.SelectedValue) { group = gr; break; }
                }
                User my = null;
                foreach (User us in users)
                {
                    if (us.UserID == myId) { my = us; break; }
                }
                ClaimToGroup claimToGroup = new ClaimToGroup(my, group);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                formatter.Serialize(stream, claimToGroup);

                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    client_socket.Send(buffer2, readCount, 0);
                }

                foreach (MemberOfGroup mem in memeberOgGroups)
                {
                    if (mem.GroupID == group.Id) { memeberOgGroups.Remove(mem); break; }
                }

                Action<Group> groupMixered = GroupMixer2;
                group1.Dispatcher.Invoke(groupMixered, group);

                stacMess.Dispatcher.Invoke(new Action(() => { stacMess.Children.Clear(); }));

                foreach (Message message in messages)
                {
                    PrintMessage(message);
                }

                messScroll.Dispatcher.Invoke(new Action(() =>
                {
                    messScroll.ScrollToVerticalOffset(messScroll.ActualHeight);
                }));

                for (int i = 0; i < messages.Count; i++)
                {
                    if (messages[i].GroupID == claimToGroup.Group.Id)
                    {
                        messages.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (usersCombo.SelectedValue != null)
            {
                User usToBan = null;
                int id = 0;
                foreach (User us in users)
                {
                    if (us == usersCombo.SelectedValue) { id = us.UserID; usToBan = us; break; }
                }

                byte[] code = Encoding.UTF8.GetBytes("02");
                client_socket.Send(code);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                BanPair newBP = new BanPair();
                newBP.Id1 = myId;
                newBP.Id2 = id;
                
                formatter.Serialize(stream, newBP);

                //byte[] buffer = ((MemoryStream)stream).ToArray();

                //byte[] b4 = ((MemoryStream)stream).ToArray();
                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    client_socket.Send(buffer2, readCount, 0);
                }
                stream.Close();

                banPairs.Add(newBP);

                usersCombo.SelectedIndex = -1;
                usersCombo.Items.Remove(usToBan);
                usersInBan.Items.Add(usToBan);

                //for (int i = 0; i < dictMesStack.Count; i++)
                //{
                //    if(dictMesStack.ElementAt(i).Key.UserID == id) 
                //    {
                //        dictMesStack.ElementAt(i).Value.Visibility = System.Windows.Visibility.Collapsed;
                //    }
                //}

                stacMess.Dispatcher.Invoke(new Action(() => { stacMess.Children.Clear(); }));
                //dictMesStack.Clear();

                foreach (Message message in messages)
                {
                    PrintMessage(message);
                }

                messScroll.Dispatcher.Invoke(new Action(() =>
                {
                    messScroll.ScrollToVerticalOffset(messScroll.ActualHeight);
                }));
            }
        }

        private void TextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && textMess.Text != "")
            {
                Message mes = new Message();
                mes.UserID = myId;
                mes.MessageID = 0;
                mes.Text = textMess.Text;
                mes.DT = DateTime.Now;
                if (groups.Count > 0)
                {
                    foreach (Group gr in groups)
                    {
                        if (gr.Title == group1.Text && group1.Text != "") { mes.GroupID = gr.Id; break; }
                    }
                }

                byte[] code = Encoding.UTF8.GetBytes("01");
                client_socket.Send(code);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                formatter.Serialize(stream, mes);

                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    client_socket.Send(buffer2, readCount, 0);
                }
                stream.Close();

                textMess.Text = "";
            }
        }

        private static string GetGroupValue(ComboBox cB)
        {
            //return (Group)(cB.SelectedValue);
            return cB.SelectedValue.ToString();
        }

        private void Creator(Message message)
        {
            StackPanel stackP = new StackPanel();

            if (message.UserID == myId)
            {
                stackP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right; stackP.MaxWidth = 300;
            }
            else
            {
                stackP.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; stackP.MaxWidth = 800;
            }
            stackP.Margin = new Thickness(5, 5, 10, 5);
            stackP.Background = Brushes.LightBlue;
            stackP.Orientation = Orientation.Vertical;

            TextBlock textB1 = new TextBlock();
            textB1.FontWeight = FontWeights.Bold;
            textB1.TextWrapping = TextWrapping.Wrap;
            string nameSender = string.Empty;
            foreach (User us in users)
            {
                if (us.UserID == message.UserID) { nameSender = us.NickName; break; }
            }
            textB1.Text = nameSender + " :";

            TextBlock textB2 = new TextBlock();
            textB2.TextWrapping = TextWrapping.Wrap;
            textB2.Text = message.Text;

            TextBlock textB3 = new TextBlock();
            textB3.TextWrapping = TextWrapping.Wrap;
            textB3.Foreground = Brushes.Gray;
            textB3.Text = message.DT.ToLongDateString() + " " + message.DT.ToLongTimeString();

            stackP.Children.Add(textB1);
            stackP.Children.Add(textB2);
            stackP.Children.Add(textB3);

            //dictMesStack.Add(message, stackP);//!!!!!!!!!!!!!!!!!!!!!!!!!!

            bool ban = false;
            foreach (BanPair bP in banPairs)
            {
                if (bP.Id2 == message.UserID) { ban = true; break; }
            }

            if (ban) { stackP.Visibility = System.Windows.Visibility.Collapsed; }

            stacMess.Children.Add(stackP);
            //stacMess.Dispatcher.Invoke(new Action(() =>
            //        {
            //            stacMess.Children.Add(stackP);
            //        }));
        }

        private void InfoMessAdd(string text)
        {
            TextBlock tBlock = new TextBlock();
            tBlock.Text = text;
            tBlock.TextWrapping = TextWrapping.Wrap;
            if(text.Contains("разбанил"))
                tBlock.Foreground = Brushes.Green;
            if (text.Contains("забанил"))
                tBlock.Foreground = Brushes.Red;

            infoStack.Children.Add(tBlock);
        }

        private void GroupMixer(Group group)
        {
            group2.Items.Remove(group);
            group1.Items.Add(group);
        }

        private void GroupMixer2(Group group)
        {
            Group def = null;
            foreach (Group defGr in groups)
            {
                if (defGr.Title == "GENERAL") { def = defGr; break; }
            }
            group1.SelectedValue = def;

            group1.Items.Remove(group);
            group2.Items.Add(group);
        }

        private void SetDefGroup(Group group)
        {
            group1.SelectedValue = group;
        }

        void PrintMessage(Message message)
        {
            string nameGroup = string.Empty;
            foreach (Group gr in groups)
            {
                if (message.GroupID == gr.Id) { nameGroup = gr.Title; break; }
            }
            Func<ComboBox, string> getGroupValue = GetGroupValue;
            if ((string)(group1.Dispatcher.Invoke(getGroupValue, group1)) == nameGroup)
            {

                Action<Message> createStacPs = Creator;
                stacMess.Dispatcher.Invoke(createStacPs, message);

            }
            //Console.WriteLine(users[0].NickName);
        }

        //void Method0(Stream stream, IFormatter formatter)
        //{
        //    Console.WriteLine("Method0 Start : !!!Экземпляр класса MainEvent!!!");

        //    //stream.Flush();
        //    stream.Position = 0;
        //    if (stream.Length > 0)
        //    {
        //        MainEvent mainEvent = (MainEvent)(formatter.Deserialize(stream));
        //        mainEvent.SomeEnent += (sender, myEventAgrs) => Console.WriteLine(myEventAgrs.t.ToString());
        //    }
        //}

        void Method1(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method1 Start : Печать сообщения");

            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                Message message = (Message)(formatter.Deserialize(stream));
                
                messages.Add(message);
                PrintMessage(message);
            }
        }

        void Method2(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method2 Start : Приём пользователей");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                users = (List<ChatInfoDAL.User>)(formatter.Deserialize(stream));
                //Console.WriteLine(users[0].NickName.ToString());
                //MessageBox.Show(bans[0].Id2.ToString());
            }

        }

        void Method3(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method3 Start");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                groups = (List<Group>)(formatter.Deserialize(stream));
            }
        }

        void Method4(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method4 Start");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                messages = (List<Message>)(formatter.Deserialize(stream));
            }
        }

        void Method5(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method5 Start");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                banPairs = (List<BanPair>)(formatter.Deserialize(stream));
            }
        }

        void Method6(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method6 Start");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                memeberOgGroups = (List<MemberOfGroup>)(formatter.Deserialize(stream));
            }

            //заполнение ComboBoxoв
            foreach (Group gr in groups)
            {
                bool member = false;
                foreach (MemberOfGroup mG in memeberOgGroups)
                {
                    if (mG.GroupID == gr.Id) { member = true;  break; }
                }
                if (member)
                {
                    group1.Dispatcher.Invoke(new Action(() =>
                        {
                            group1.Items.Add(gr);
                        }));
                }
                else
                {
                    group2.Dispatcher.Invoke(new Action(() =>
                    {
                        group2.Items.Add(gr);
                    }));
                }
                
            }

            group1.Dispatcher.Invoke(new Action(() =>
            {
                group1.SelectedIndex = 0;
            }));
            group2.Dispatcher.Invoke(new Action(() =>
            {
                group2.SelectedIndex = 0;
            }));

            foreach (User us in users)
            {
                if (us.UserID != myId)
                {
                    bool banned = false;
                    foreach (BanPair bP in banPairs)
                    {
                        if (bP.Id2 == us.UserID) { banned = true; break; }
                    }
                    if (banned)
                    {
                        usersInBan.Dispatcher.Invoke
                        (new Action(() =>
                        {
                            usersInBan.Items.Add(us);

                        }));
                    }
                    else
                    {
                        usersCombo.Dispatcher.Invoke
                            (new Action(() =>
                            {
                                usersCombo.Items.Add(us);
                            }));
                    }
                }
            }

            Group group = null;
            foreach(Group gr in groups)
            {
                if(gr.Title == "GENERAL") {group = gr; break;}
            }
            Action<Group> setterDefGroup = SetDefGroup;
            group1.Dispatcher.Invoke(setterDefGroup, group); 

            foreach (Message mes in messages)
            {
                PrintMessage(mes);
            }
            messScroll.Dispatcher.Invoke(new Action(() =>
            {
                messScroll.ScrollToVerticalOffset(messScroll.ActualHeight);
            }));
        }

        void Method7(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method7 : Просто сообщение");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                string info = (string)(formatter.Deserialize(stream));
                Action<string> infoMesAdder = InfoMessAdd;
                infoStack.Dispatcher.Invoke(infoMesAdder, info);
            }
        }

        void Method8(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method8 : Ban/Unban от админа");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                User user = (User)(formatter.Deserialize(stream));
                User chUser = null;

                foreach (User us in users)
                {
                    if (us.UserID == user.UserID) { chUser = us; us.Ban = user.Ban; break; }
                }

                string info = string.Empty;
                if (chUser.Ban < DateTime.Now)
                    info = String.Format("АДИНИСТРАТОР РАЗБАНИЛ {0}", chUser.NickName);
                else
                    info = String.Format("АДИНИСТРАТОР ЗАБАНИЛ {0}", chUser.NickName);
                
                Action<string> infoMesAdder = InfoMessAdd;
                infoStack.Dispatcher.Invoke(infoMesAdder, info);
            }
        }

        void Method9(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method9 : Новый юзер");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                User user = (User)(formatter.Deserialize(stream));
                users.Add(user);

                usersCombo.Dispatcher.Invoke(new Action(() => { usersCombo.Items.Add(user); }));

                Action<string> infoMesAdder = InfoMessAdd;
                infoStack.Dispatcher.Invoke(infoMesAdder, String.Format("Пользователь {0} зарегистрировался", user.NickName));
            }
        }

        void Method10(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method10 : Принята заявка о вступлении в группу");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                MemberOfGroup mem = (MemberOfGroup)(formatter.Deserialize(stream));
                memeberOgGroups.Add(mem);

                Group group = null;
                foreach (Group gr in groups)
                {
                    if (gr.Id == mem.GroupID) { group = gr; break; }
                }

                Action<Group> groupMixered = GroupMixer;
                group2.Dispatcher.Invoke(groupMixered, group);
                //usersCombo.Dispatcher.Invoke(new Action(() => { usersCombo.Items.Add(user); }));

                //Action<string> infoMesAdder = InfoMessAdd;
                //infoStack.Dispatcher.Invoke(infoMesAdder, String.Format("Пользователь {0} зарегистрировался", user.NickName));
            }
        }

        void Method11(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method10 : Принятие сообщений из новой группы");
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                List<Message> messes = (List<Message>)(formatter.Deserialize(stream));
                messages.AddRange(messes);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (group2.SelectedValue != null)
            {
                byte[] code = Encoding.UTF8.GetBytes("04");
                client_socket.Send(code);

                Group group = null;
                foreach (Group gr in groups)
                {
                    if (gr == group2.SelectedValue) { group = gr; break; }
                }
                User my = null;
                foreach (User us in users)
                {
                    if (us.UserID == myId) { my = us; break; }
                }
                ClaimToGroup claimToGroup = new ClaimToGroup(my, group);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                formatter.Serialize(stream, claimToGroup);

                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    client_socket.Send(buffer2, readCount, 0);
                }
                
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (usersInBan.SelectedValue != null)
            {
                User usToUnBan = null;
                int id = 0;
                foreach (User us in users)
                {
                    if (us == usersInBan.SelectedValue) { id = us.UserID; usToUnBan = us; break; }
                }

                byte[] code = Encoding.UTF8.GetBytes("03");
                client_socket.Send(code);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                BanPair oldBP = new BanPair();
                oldBP.Id1 = myId;
                oldBP.Id2 = id;

                formatter.Serialize(stream, oldBP);

                //byte[] b4 = ((MemoryStream)stream).ToArray();
                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    client_socket.Send(buffer2, readCount, 0);
                }
                stream.Close();

                foreach (BanPair bP in banPairs)
                {
                    if (bP.Id1 == myId && bP.Id2 == id) { banPairs.Remove(bP); break; }
                }

                usersInBan.SelectedIndex = -1;
                usersInBan.Items.Remove(usToUnBan);
                usersCombo.Items.Add(usToUnBan);

                //for (int i = 0; i < dictMesStack.Count; i++)
                //{
                //    if (dictMesStack.ElementAt(i).Key.UserID == id)
                //    {
                //        dictMesStack.ElementAt(i).Value.Visibility = System.Windows.Visibility.Visible;
                //    }
                //}

                stacMess.Dispatcher.Invoke(new Action(() => { stacMess.Children.Clear(); }));
                //dictMesStack.Clear();

                foreach (Message message in messages)
                {
                    PrintMessage(message);
                }

                messScroll.Dispatcher.Invoke(new Action(() =>
                {
                    messScroll.ScrollToVerticalOffset(messScroll.ActualHeight);
                }));
            }
        }

        private void messScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            bool autoScroll = false;
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange != 0)
            {   // Content unchanged : user scroll event
                if (messScroll.VerticalOffset == messScroll.ScrollableHeight - e.ExtentHeightChange)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    autoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    autoScroll = false;
                }
            }

            //Content scroll event : autoscroll eventually
            if (autoScroll)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                messScroll.ScrollToVerticalOffset(messScroll.ExtentHeight);

            }

        }

        private void infoScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool autoScroll = false;
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange != 0)
            {   // Content unchanged : user scroll event
                if (infoScroll.VerticalOffset == infoScroll.ScrollableHeight - e.ExtentHeightChange)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    autoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    autoScroll = false;
                }
            }

            //Content scroll event : autoscroll eventually
            if (autoScroll)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                infoScroll.ScrollToVerticalOffset(infoScroll.ExtentHeight);

            }
        }

        private void group1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stacMess.Dispatcher.Invoke(new Action(() => { stacMess.Children.Clear(); }));
            //dictMesStack.Clear();

            foreach (Message message in messages)
            {
                PrintMessage(message);
            }

            messScroll.Dispatcher.Invoke(new Action(() =>
            {
                messScroll.ScrollToVerticalOffset(messScroll.ActualHeight);
            }));
        }
    }
}
