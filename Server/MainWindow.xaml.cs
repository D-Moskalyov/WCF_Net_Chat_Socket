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
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using ChatInfoDAL;
using System.ServiceModel;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using EventLib;

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket reg_socket;
        Socket login_socket;
        Socket server_socket;
        Socket connection_socket;

        //MainEvent mainEvent = new MainEvent();

        //List<Server.User> users = new List<Server.User>();
        static Dictionary<Socket, ChatInfoDAL.User> dictSocketUser = new Dictionary<Socket, ChatInfoDAL.User>();
        static List<MessageQueue> messInQueue = new List<MessageQueue>();
        public DateTime tempBan;
        //ServiceHost serviceHost = null;

        public MainWindow()
        {

            if (DataBase.GetSqlConnection().State == System.Data.ConnectionState.Open)
            {
                InitializeComponent();
                Console.WriteLine("Start Server");

                FirstInit();

                foreach(Group gr in GroupManager.Groups)
                {
                    groupCombo.Items.Add(gr);
                    if (gr.Title == "GENERAL")
                        groupCombo.SelectedValue = gr;
                }

                BanCombo.Items.Add("Все");
                BanCombo.Items.Add("В бане");
                BanCombo.Items.Add("Не в бане");
                BanCombo.SelectedValue = "Все";

                groupCombo.SelectionChanged += groupCombo_SelectionChanged;
                BanCombo.SelectionChanged += BanCombo_SelectionChanged;

                FillListUser(groupCombo.SelectedValue.ToString(), BanCombo.SelectedValue.ToString());

                reg_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPointReg = new System.Net.IPEndPoint(IPAddress.Any, 3000);
                reg_socket.Bind(ipEndPointReg);
                Thread listenReg = new Thread(StartListenReg);
                listenReg.Start(reg_socket);


                login_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPointLog = new System.Net.IPEndPoint(IPAddress.Any, 4000);
                login_socket.Bind(ipEndPointLog);
                Thread listenLogin = new Thread(StartListenLogin);
                listenLogin.Start(login_socket);


                server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new System.Net.IPEndPoint(IPAddress.Any, 2000);
                server_socket.Bind(ipEndPoint);
                Thread listenUser = new Thread(StartListenUser);
                listenUser.Start(server_socket);

            }
        }

        void FirstInit()
        {
            string query = "select * from Users";
            SqlCommand command = new SqlCommand(query, DataBase.Sql);
            SqlDataReader reader = command.ExecuteReader();
            UserManager.LoadUsers(reader);
            reader.Close();

            query = "select * from Messagese";
            command = new SqlCommand(query, DataBase.Sql);
            reader = command.ExecuteReader();
            MessageManager.LoadMessages(reader);
            reader.Close();

            query = "select * from Groups";
            command = new SqlCommand(query, DataBase.Sql);
            reader = command.ExecuteReader();
            GroupManager.LoadGroups(reader);
            reader.Close();

            query = "select * from MemeberOfGroups";
            command = new SqlCommand(query, DataBase.Sql);
            reader = command.ExecuteReader();
            MemberOfGroupManager.LoadMembersOfGroup(reader);
            reader.Close();

            query = "select * from BanList";
            command = new SqlCommand(query, DataBase.Sql);
            reader = command.ExecuteReader();
            BanPairManager.LoadBanPairs(reader);
            reader.Close();
        }

        void FillListUser(string group, string banOrNot)
        {
            listBox.Dispatcher.Invoke(new Action(() => { listBox.Items.Clear(); }));

            int idGroup = 0;
            foreach(Group gr in GroupManager.Groups)
            {
                if(gr.Title == group) {idGroup = gr.Id; break;}
            }

            foreach (ChatInfoDAL.User user in UserManager.Users)
            {
                bool member = false;
                foreach (MemberOfGroup mem in MemberOfGroupManager.MembersOfGroup)
                {
                    if (mem.GroupID == idGroup && mem.UserID == user.UserID) { member = true; break; }
                }
                if (member)
                {
                    if (banOrNot == "Все")
                    {
                        listBox.Items.Add(user);
                    }
                    else
                    {
                        bool banned = false;
                        if (user.Ban > DateTime.Now) banned = true;
                        switch (banOrNot)
                        {
                            case "В бане":
                                {
                                    if (banned) listBox.Items.Add(user);
                                    break;
                                }
                            case "Не в бане":
                                {
                                    if (!banned) listBox.Items.Add(user);
                                    break;
                                }
                            default: break;
                        }
                    }
                }
            }
        }

        void StartListenUser(object sct)
        {
            Socket socket = sct as Socket;
            socket.Listen(5);

            while (true)
            {
                //Console.WriteLine("попытка установить подключение");
                connection_socket = socket.Accept();

                if (connection_socket == null)
                {
                    continue;
                }

                byte[] introduce = new byte[1024];
                int res = connection_socket.Receive(introduce);
                string name = Encoding.UTF8.GetString(introduce, 0, res);
                int idUser = 0;
                ChatInfoDAL.User newUser = null;

                foreach (Socket sock in dictSocketUser.Keys)
                {
                    byte[] code = Encoding.UTF8.GetBytes("07");
                    sock.Send(code);

                    IFormatter formatter2 = new BinaryFormatter();
                    Stream stream2 = new MemoryStream();

                    formatter2.Serialize(stream2, String.Format("Пользователь {0} присоеденился", name));

                    byte[] buffer2 = new byte[1024];
                    stream2.Position = 0;
                    while (stream2.Position < stream2.Length)
                    {
                        int readCount = stream2.Read(buffer2, 0, 1024);
                        sock.Send(buffer2, readCount, 0);
                    }
                }

                foreach(ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.NickName == name) { dictSocketUser.Add(connection_socket, us); idUser = us.UserID; newUser = us; break; }
                }
                
                byte[] userID = Encoding.UTF8.GetBytes(idUser.ToString());
                connection_socket.Send(userID);
                //Console.WriteLine("Соединение произошло успешно");

                PrimarySend(newUser, connection_socket);

                Action<ListBox, ChatInfoDAL.User> act = SetNewUserToList;

                if (!listBox.Items.Contains(newUser))
                {
                    listBox.Dispatcher.Invoke(act, listBox, newUser);
                }

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} подключился", newUser.NickName);
                infoStack.Dispatcher.Invoke(action, messa);

                Thread client_thread = new Thread(Listen_Client);
                //Console.WriteLine("Начинаем Прослушивание");

                client_thread.Start(connection_socket);

            }
        }

        void StartListenReg(object sct)
        {
            Socket socket = sct as Socket;
            socket.Listen(5);

            while (true)
            {
                //Console.WriteLine("попытка установить подключение");
                Socket reg_socket = socket.Accept();

                if (reg_socket == null)
                {
                    continue;
                }

                //Console.WriteLine("Соединение произошло успешно");

                Thread client_thread = new Thread(Listen_Reg);
                //Console.WriteLine("Начинаем Прослушивание");

                client_thread.Start(reg_socket);

            }
        }

        void StartListenLogin(object sct)
        {
            Socket socket = sct as Socket;
            socket.Listen(5);

            while (true)
            {
                //Console.WriteLine("попытка установить подключение");
                Socket log_socket = socket.Accept();
                
                if (log_socket == null)
                {
                    continue;
                }

                //Console.WriteLine("Соединение произошло успешно");

                Thread client_thread = new Thread(Listen_Log);
                //Console.WriteLine("Начинаем Прослушивание");

                client_thread.Start(log_socket);

            }
        }

        void Listen_Client(object user_socket)
        {
            Socket connection_socket = user_socket as Socket;
            if (connection_socket == null)
                return;

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
                        int byteCount = connection_socket.Receive(typeOperation);
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
                        resCount = connection_socket.Receive(info);
                        if (resCount == 2 && (Encoding.UTF8.GetString(info, 0, resCount) == "01" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "02" || Encoding.UTF8.GetString(info, 0, resCount) == "03" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "04" || Encoding.UTF8.GetString(info, 0, resCount) == "05" ||
                            Encoding.UTF8.GetString(info, 0, resCount) == "06" || Encoding.UTF8.GetString(info, 0, resCount) == "07"))
                        {
                            defTypeOper = Encoding.UTF8.GetString(info, 0, resCount); break;
                        }
                        else { stream.Write(info, 0, resCount); }
                        stream.Write(info, 0, resCount);
                    }

                    switch (typeOper)
                    {
                        case "01": { Method1(stream, formatter); break; }
                        case "02": { Method2(stream, formatter); break; }
                        case "03": { Method3(stream, formatter); break; }
                        case "04": { Method4(stream, formatter); break; }
                        case "05": { Method5(stream, formatter); break; }
                        default: { break; }

                    }
                }
            }
            catch (SocketException exp)
            {
                
                ChatInfoDAL.User user = dictSocketUser[connection_socket];

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} отсоединился", user);
                infoStack.Dispatcher.Invoke(action, messa);

                dictSocketUser.Remove(connection_socket);
                
                connection_socket.Shutdown(SocketShutdown.Both);
                connection_socket.Close();

                foreach (Socket sct in dictSocketUser.Keys)
                {
                    byte[] code = Encoding.UTF8.GetBytes("07");
                    sct.Send(code);

                    IFormatter formatter2 = new BinaryFormatter();
                    Stream stream2 = new MemoryStream();

                    formatter2.Serialize(stream2, String.Format("Пользователь {0} отсоединился", user.NickName));

                    byte[] buffer2 = new byte[1024];
                    stream2.Position = 0;
                    while (stream2.Position < stream2.Length)
                    {
                        int readCount = stream2.Read(buffer2, 0, 1024);
                        sct.Send(buffer2, readCount, 0);
                    }
                }
            }
        }

        void Listen_Reg(object reg_socket)
        {
            Console.WriteLine("Reg Connect");
            Socket connection_socket = reg_socket as Socket;
            if (connection_socket == null)
                return;

            byte[] regInfo = new byte[1024];
            ////int byteCount = client_socket.Receive(firstAnswer);
            ////Stream stream = new MemoryStream(firstAnswer);
            Stream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();

            int resCount = -1;
            connection_socket.ReceiveTimeout = 500;
            while (resCount != 0)
            {
                try
                {
                    resCount = connection_socket.Receive(regInfo);
                    stream.Write(regInfo, 0, resCount);
                }
                catch (SocketException exp) { resCount = 0; }
            }
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                RegInfo reg = (RegInfo)(formatter.Deserialize(stream));
                Console.WriteLine(reg.Name);
                if (reg.Pass1 != reg.Pass2)
                {
                    byte[] answer = Encoding.UTF8.GetBytes("Пароли не совпадают");
                    connection_socket.Send(answer);
                }
                else
                {
                    string query = string.Format("select * from Users where NickName='{0}'", reg.Name);
                    SqlCommand command = new SqlCommand(query, DataBase.Sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        byte[] answer = Encoding.UTF8.GetBytes("Пользователь уже существует");
                        connection_socket.Send(answer);
                        reader.Close();
                    }
                    else
                    {
                        reader.Close();
                        string queryIns = string.Format("INSERT INTO Users (NickName, Pass, Ban) VALUES ('{0}', {1}, '1900-01-01')", reg.Name, reg.Pass1);
                        SqlCommand commandIns = new SqlCommand(queryIns, DataBase.Sql);
                        commandIns.ExecuteNonQuery();

                        string queryNewID = string.Format("select IDENT_CURRENT('Users') as ID_cur");
                        SqlCommand commandNewID = new SqlCommand(queryNewID, DataBase.Sql);
                        SqlDataReader readerNewID  = commandNewID.ExecuteReader();
                        readerNewID.Read();
                        //Console.WriteLine(readerNewID["ID_cur"]);
                        int id = int.Parse(readerNewID["ID_cur"].ToString());
                        readerNewID.Close();

                        ChatInfoDAL.User newUser = new ChatInfoDAL.User(id, reg.Name, reg.Pass1, new DateTime(1900, 1, 1));
                        ChatInfoDAL.UserManager.Users.Add(newUser);

                        string queryInsGroup = string.Format("INSERT INTO MemeberOfGroups (GroupID, UserID) VALUES (6, {0})", id);
                        SqlCommand commandInsGroup = new SqlCommand(queryInsGroup, DataBase.Sql);
                        commandInsGroup.ExecuteNonQuery();

                        //Func<ComboBox, string> getGroupValue = GetStringvalueDispatch;
                        //string first = (string)groupCombo.Dispatcher.Invoke(getGroupValue, groupCombo);
                        //string second = (string)BanCombo.Dispatcher.Invoke(getGroupValue, BanCombo);
                        //FillListUser(first, second);

                        ChatInfoDAL.MemberOfGroupManager.MembersOfGroup.Add(new ChatInfoDAL.MemberOfGroup(id, 6));

                        byte[] answer = Encoding.UTF8.GetBytes("Добро пожаловать!");
                        connection_socket.Send(answer);

                        Action<string> action = MessageToInfoStackAdd;
                        string messa = String.Format("{0} зарегестрирован", newUser.NickName);
                        infoStack.Dispatcher.Invoke(action, messa);

                        foreach (Socket sct in dictSocketUser.Keys)
                        {
                            byte[] code = Encoding.UTF8.GetBytes("09");
                            sct.Send(code);

                            IFormatter formatter2 = new BinaryFormatter();
                            Stream stream2 = new MemoryStream();

                            formatter2.Serialize(stream2, newUser);

                            byte[] buffer2 = new byte[1024];
                            stream2.Position = 0;
                            while (stream2.Position < stream2.Length)
                            {
                                int readCount = stream2.Read(buffer2, 0, 1024);
                                sct.Send(buffer2, readCount, 0);
                            }
                        }
                    }
                    
                }
                connection_socket.Close();
            }
            else { connection_socket.Close(); }
        }

        void MessageToInfoStackAdd(string message)
        {
            TextBlock tBlock = new TextBlock();
            tBlock.Text = message;
            tBlock.TextWrapping = TextWrapping.Wrap;

            infoStack.Children.Add(tBlock);
        }

        void SetNewUserToList(ListBox lB, ChatInfoDAL.User user)
        {
            lB.Items.Add(user);
        }

        void SetNewClaimToList(ClaimToGroup claim)
        {
            bool exist = false;
            foreach (ClaimToGroup cl in listBox2.Items)
            {
                if (cl.User.UserID == claim.User.UserID && cl.Group.Id == claim.Group.Id) { exist = true; break; }
            }
            if(!exist)
                listBox2.Items.Add(claim);
        }

        void Listen_Log(object log_socket)
        {
            Console.WriteLine("Log Connect");
            Socket connection_socket = log_socket as Socket;
            if (connection_socket == null)
                return;

            byte[] logInfo = new byte[1024];
            ////int byteCount = client_socket.Receive(firstAnswer);
            ////Stream stream = new MemoryStream(firstAnswer);
            Stream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();

            int resCount = -1;
            connection_socket.ReceiveTimeout = 500;
            while (resCount != 0)
            {
                try
                {
                    resCount = connection_socket.Receive(logInfo);
                    stream.Write(logInfo, 0, resCount);
                }
                catch (SocketException exp) { resCount = 0; }
            }
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                LoginInfo log = (LoginInfo)(formatter.Deserialize(stream));
                Console.WriteLine(log.Name);
                string query = string.Format("select * from Users where NickName='{0}' and Pass = '{1}'", log.Name, log.Password);
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                SqlDataReader reader = command.ExecuteReader();
                
                if (reader.HasRows)
                {
                    reader.Close();
                    bool loged = false;
                    foreach (ChatInfoDAL.User us in dictSocketUser.Values)
                    {
                        if (us.NickName == log.Name)
                        {
                            loged = true;
                            break;
                        }
                    }
                    if (!loged)
                    {
                        byte[] answer = Encoding.UTF8.GetBytes("Добро пожаловать!");
                        connection_socket.Send(answer);
                    }
                    else
                    {
                        byte[] answer = Encoding.UTF8.GetBytes("Вы уже авторизировались!");
                        connection_socket.Send(answer);
                    }
                }
                else
                {
                    byte[] answer = Encoding.UTF8.GetBytes("Не верное имя пользователя или пароль");
                    connection_socket.Send(answer);
                    reader.Close();
                }

                connection_socket.Close();
            }
        }

        void PrimarySend(ChatInfoDAL.User user, Socket socket)
        {
            //byte []code0 = Encoding.UTF8.GetBytes("00");
            //socket.Send(code0);

            //IFormatter formatter0 = new BinaryFormatter();
            //Stream stream0 = new MemoryStream();

            //formatter0.Serialize(stream0, mainEvent);

            //byte[] buffer0 = new byte[1024];
            //stream0.Position = 0;
            //while (stream0.Position < stream0.Length)
            //{
            //    int readCount = stream0.Read(buffer0, 0, 1024);
            //    socket.Send(buffer0, readCount, 0);
            //}
            //stream0.Close();
            //Thread.Sleep(50);
            
            byte[] code = Encoding.UTF8.GetBytes("02");
            socket.Send(code);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();

            formatter.Serialize(stream, UserManager.Users);

            byte[] buffer = new byte[1024];
            stream.Position = 0;
            while (stream.Position < stream.Length)
            {
                int readCount = stream.Read(buffer, 0, 1024);
                socket.Send(buffer, readCount, 0);
            }
            stream.Close();

            Thread.Sleep(50);

            code = Encoding.UTF8.GetBytes("03");
            socket.Send(code);

            IFormatter formatter2 = new BinaryFormatter();
            Stream stream2 = new MemoryStream();

            formatter2.Serialize(stream2, GroupManager.Groups);

            byte[] buffer2 = new byte[1024];
            stream2.Position = 0;
            while (stream2.Position < stream2.Length)
            {
                int readCount = stream2.Read(buffer2, 0, 1024);
                socket.Send(buffer2, readCount, 0);
            }
            stream2.Close();

            Thread.Sleep(50);

            List<Message> messGen = new List<Message>();
            List<int> numOfGroup = new List<int>();
            foreach (MemberOfGroup mem in MemberOfGroupManager.MembersOfGroup)
            {
                if (user.UserID == mem.UserID) numOfGroup.Add(mem.GroupID);
            }
            foreach (Message mes in MessageManager.Messages)
            {
                if (numOfGroup.Contains(mes.GroupID)) messGen.Add(mes);
            }
            code = Encoding.UTF8.GetBytes("04");
            socket.Send(code);

            IFormatter formatter3 = new BinaryFormatter();
            Stream stream3 = new MemoryStream();

            formatter3.Serialize(stream3, messGen);

            byte[] buffer3 = new byte[1024];
            stream3.Position = 0;
            while (stream3.Position < stream3.Length)
            {
                int readCount = stream3.Read(buffer3, 0, 1024);
                socket.Send(buffer3, readCount, 0);
            }
            stream3.Close();

            Thread.Sleep(50);

            code = Encoding.UTF8.GetBytes("05");
            socket.Send(code);

            List<BanPair> bPair = new List<BanPair>();
            foreach (BanPair bP in BanPairManager.BanPairs)
            {
                if (bP.Id1 == user.UserID) { bPair.Add(bP); }
            }
            IFormatter formatter4 = new BinaryFormatter();
            Stream stream4 = new MemoryStream();

            formatter4.Serialize(stream4, bPair);

            byte[] buffer4 = new byte[1024];
            stream4.Position = 0;
            while (stream4.Position < stream4.Length)
            {
                int readCount = stream4.Read(buffer4, 0, 1024);
                socket.Send(buffer4, readCount, 0);
            }
            stream4.Close();

            Thread.Sleep(50);

            code = Encoding.UTF8.GetBytes("06");
            socket.Send(code);

            List<MemberOfGroup> memberOfGroup = new List<MemberOfGroup>();
            foreach (MemberOfGroup mG in MemberOfGroupManager.MembersOfGroup)
            {
                if (mG.UserID == user.UserID) { memberOfGroup.Add(mG); }
            }
            IFormatter formatter5 = new BinaryFormatter();
            Stream stream5 = new MemoryStream();

            formatter5.Serialize(stream5, memberOfGroup);

            byte[] buffer5 = new byte[1024];
            stream5.Position = 0;
            while (stream5.Position < stream5.Length)
            {
                int readCount = stream5.Read(buffer5, 0, 1024);
                socket.Send(buffer5, readCount, 0);
            }
            stream5.Close();

            for (int i = 0; i < messInQueue.Count; i++ )
            {
                if (messInQueue[i].User.UserID == user.UserID)
                {
                    Thread.Sleep(50);

                    code = Encoding.UTF8.GetBytes("07");
                    socket.Send(code);

                    IFormatter formatter6 = new BinaryFormatter();
                    Stream stream6 = new MemoryStream();

                    formatter6.Serialize(stream6, messInQueue[i].Message);

                    byte[] buffer6 = new byte[1024];
                    stream6.Position = 0;
                    while (stream6.Position < stream6.Length)
                    {
                        int readCount = stream6.Read(buffer6, 0, 1024);
                        socket.Send(buffer6, readCount, 0);
                    }
                    stream6.Close();
                    messInQueue.RemoveAt(i);
                    i--;
                }
            }
        }

        void Method1(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method1 Start : Сообщение от клиента");
            
            //stream.Flush();
            stream.Position = 0;
            if (stream.Length > 0)
            {
                Message message = (Message)(formatter.Deserialize(stream));

                ChatInfoDAL.User user = null;
                foreach (ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.UserID == message.UserID) { user = us; break; }
                }
                if (user.Ban > DateTime.Now)
                {
                    Action<string> action = MessageToInfoStackAdd;
                    string messa = String.Format("{0} забанен и не может отправлять сообщения", user.NickName);
                    infoStack.Dispatcher.Invoke(action, messa);
                }
                else
                {
                    string query = string.Format("insert into Messagese (GroupID, Texts, UserID, TimeMes) values ({0}, '{1}', {2}, '{3}')",
                        message.GroupID, message.Text, message.UserID, message.DT);
                    SqlCommand command = new SqlCommand(query, DataBase.Sql);
                    command.ExecuteNonQuery();

                    string queryNewID = string.Format("select IDENT_CURRENT('Messagese') as ID_cur");
                    SqlCommand commandNewID = new SqlCommand(queryNewID, DataBase.Sql);
                    SqlDataReader readerNewID = commandNewID.ExecuteReader();
                    readerNewID.Read();
                    int id = int.Parse(readerNewID["ID_cur"].ToString());
                    readerNewID.Close();

                    message.MessageID = id;
                    MessageManager.Messages.Add(message);

                    Action<string> action = MessageToInfoStackAdd;
                    string messa = String.Format("{0} отправил сообщение : {1}", user.NickName, message.Text);
                    infoStack.Dispatcher.Invoke(action, messa);

                    SendingMessage(message);
                    
                }
            }
            stream.Close();
        }

        void Method2(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method2 Start : Ban User To User");
            stream.Position = 0;
            if (stream.Length > 0)
            {
                BanPair bP = (BanPair)(formatter.Deserialize(stream));
                BanPairManager.BanPairs.Add(bP);

                string query = string.Format("INSERT INTO BanList (UserID1, UserID2) VALUES ({0}, {1})", bP.Id1, bP.Id2);
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Dictionary<Socket, ChatInfoDAL.User>.ValueCollection valUser = dictSocketUser.Values;
                int numBanned;
                bool online = false;
                
                for (numBanned = 0; numBanned < valUser.Count; numBanned++)
                {
                    if (valUser.ElementAt(numBanned).UserID == bP.Id2) { online = true; break; }
                }
                string nameBan = string.Empty;

                foreach (ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.UserID == bP.Id1) { nameBan = us.NickName; break; }
                }

                string nameBan2 = string.Empty;

                foreach (ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.UserID == bP.Id2) { nameBan2 = us.NickName; break; }
                }

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} забанил {1}", nameBan, nameBan2);
                infoStack.Dispatcher.Invoke(action, messa);

                if (online)
                {
                    Socket sctBanned = dictSocketUser.ElementAt(numBanned).Key;

                    byte[] code = Encoding.UTF8.GetBytes("07");
                    sctBanned.Send(code);

                    IFormatter formatter2 = new BinaryFormatter();
                    Stream stream2 = new MemoryStream();

                    string messageBan = String.Format("Пользователь {0} вас забанил", nameBan);

                    formatter2.Serialize(stream2, messageBan);

                    byte[] buffer2 = new byte[1024];
                    stream2.Position = 0;
                    while (stream2.Position < stream2.Length)
                    {
                        int readCount = stream2.Read(buffer2, 0, 1024);
                        sctBanned.Send(buffer2, readCount, 0);
                    }
                }
                else
                { 
                    ChatInfoDAL.User userToSend = null;
                    foreach (ChatInfoDAL.User us in UserManager.Users)
                    {
                        if (us.UserID == bP.Id2) { userToSend = us; break; }
                    }
                    messInQueue.Add(new MessageQueue(userToSend, String.Format("Пользователь {0} вас забанил", nameBan)));
                }
            }
        }//Ban User To User

        void Method3(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method3 Start : UnBan User To User");
            stream.Position = 0;
            if (stream.Length > 0)
            {
                BanPair bP = (BanPair)(formatter.Deserialize(stream));
                foreach (BanPair bPair in BanPairManager.BanPairs)
                {
                    if (bPair.Id1 == bP.Id1 && bPair.Id2 == bP.Id2) { BanPairManager.BanPairs.Remove(bPair); break; }
                }

                string query = string.Format("delete from BanList where UserID1={0} and UserID2={1}", bP.Id1, bP.Id2);
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Dictionary<Socket, ChatInfoDAL.User>.ValueCollection valUser = dictSocketUser.Values;
                int numBanned;
                bool online = false;
                for (numBanned = 0; numBanned < valUser.Count; numBanned++)
                {
                    if (valUser.ElementAt(numBanned).UserID == bP.Id2) { online = true; break; }
                }

                string nameBan = string.Empty;
                foreach (ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.UserID == bP.Id1) { nameBan = us.NickName; break; }
                }
                string nameBan2 = string.Empty;
                foreach (ChatInfoDAL.User us in UserManager.Users)
                {
                    if (us.UserID == bP.Id2) { nameBan2 = us.NickName; break; }
                }

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} разбанил {1}", nameBan, nameBan2);
                infoStack.Dispatcher.Invoke(action, messa);

                if (online)
                {
                    Socket sctBanned = dictSocketUser.ElementAt(numBanned).Key;

                    byte[] code = Encoding.UTF8.GetBytes("07");
                    sctBanned.Send(code);

                    IFormatter formatter2 = new BinaryFormatter();
                    Stream stream2 = new MemoryStream();

                    string messageBan = String.Format("Пользователь {0} вас разбанил", nameBan);

                    formatter2.Serialize(stream2, messageBan);

                    byte[] buffer2 = new byte[1024];
                    stream2.Position = 0;
                    while (stream2.Position < stream2.Length)
                    {
                        int readCount = stream2.Read(buffer2, 0, 1024);
                        sctBanned.Send(buffer2, readCount, 0);
                    }
                }
                else
                {
                    ChatInfoDAL.User userToSend = null;
                    foreach (ChatInfoDAL.User us in UserManager.Users)
                    {
                        if (us.UserID == bP.Id2) { userToSend = us; break; }
                    }
                    messInQueue.Add(new MessageQueue(userToSend, String.Format("Пользователь {0} вас разбанил", nameBan)));
                }
            }
        }

        void Method4(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method4 Start : Заявка на вступление в группу");

            stream.Position = 0;
            if (stream.Length > 0)
            {
                ClaimToGroup claimToGroup = (ClaimToGroup)(formatter.Deserialize(stream));

                Action<ClaimToGroup> claimer = SetNewClaimToList;
                listBox2.Dispatcher.Invoke(claimer, claimToGroup);

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} прислал заявку на вступление в группу {1}", 
                    claimToGroup.User, claimToGroup.Group);
                infoStack.Dispatcher.Invoke(action, messa);
            }
        }

        void Method5(Stream stream, IFormatter formatter)
        {
            Console.WriteLine("Method5 Start : Заявка на выход из группы");

            stream.Position = 0;
            if (stream.Length > 0)
            {
                ClaimToGroup claimToGroup = (ClaimToGroup)(formatter.Deserialize(stream));

                foreach (MemberOfGroup mem in MemberOfGroupManager.MembersOfGroup)
                {
                    if (mem.GroupID == claimToGroup.Group.Id && mem.UserID == claimToGroup.User.UserID)
                    {
                        MemberOfGroupManager.MembersOfGroup.Remove(mem);
                        break;
                    }
                }

                string query = string.Format("delete from MemeberOfGroups where UserID={0} and GroupID={1}",
                    claimToGroup.User.UserID, claimToGroup.Group.Id);
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{1} вышел из группы {0}",
                    claimToGroup.Group, claimToGroup.User);
                infoStack.Dispatcher.Invoke(action, messa);

                foreach (Socket socket in dictSocketUser.Keys)
                {
                    bool member = false;
                    foreach (MemberOfGroup mem in MemberOfGroupManager.MembersOfGroup)
                    {
                        if (dictSocketUser[socket].UserID == mem.UserID && mem.GroupID == claimToGroup.Group.Id) { member = true; break; }
                    }

                    if (member)
                    {
                        byte[] code2 = Encoding.UTF8.GetBytes("07");
                        socket.Send(code2);

                        IFormatter formatter2 = new BinaryFormatter();
                        Stream stream2 = new MemoryStream();

                        formatter2.Serialize(stream2, String.Format("{0} вышел из группы {1}",
                            claimToGroup.User, claimToGroup.Group));

                        byte[] buffer = new byte[1024];
                        stream2.Position = 0;
                        while (stream2.Position < stream2.Length)
                        {
                            int readCount = stream2.Read(buffer, 0, 1024);
                            socket.Send(buffer, readCount, 0);
                        }
                    }
                }
            }
        }

        void SendingMessage(Message mess)
        {
            foreach (Socket sock in dictSocketUser.Keys)
            {

                byte[] code = Encoding.UTF8.GetBytes("01");
                sock.Send(code);

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();

                formatter.Serialize(stream, mess);

                byte[] buffer2 = new byte[1024];
                stream.Position = 0;
                while (stream.Position < stream.Length)
                {
                    int readCount = stream.Read(buffer2, 0, 1024);
                    sock.Send(buffer2, readCount, 0);
                }
                stream.Close();

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedValue != null)
            {
                //бан
                BanWindow banW = new BanWindow();
                banW.Owner = this;
                //banW.Closed += banW_Closed;
                banW.ShowDialog();
            }
        }//окно с баном

        public void banW_Closed(object sender, EventArgs e)
        {
            if (tempBan != null)
            {
                ((ChatInfoDAL.User)(listBox.SelectedValue)).Ban = tempBan;

                string query = string.Format("update Users set Ban='{1}' where UserID={0}",
                    ((ChatInfoDAL.User)(listBox.SelectedValue)).UserID, tempBan.ToShortDateString());
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("{0} забанен администратором", listBox.SelectedValue);
                infoStack.Dispatcher.Invoke(action, messa);

                foreach (ChatInfoDAL.User user in listBox.Items)
                {
                    Dictionary<Socket, ChatInfoDAL.User>.ValueCollection userCol = dictSocketUser.Values;
                    int numSocket = -1;
                    bool online = false;
                    for (numSocket = 0; numSocket < userCol.Count; numSocket++)
                    {
                        if (userCol.ElementAt(numSocket) == user) { online = true; break; }
                    }
                    if (online)
                    {
                        Socket sctToSend = dictSocketUser.ElementAt(numSocket).Key;
                        byte[] code = Encoding.UTF8.GetBytes("08");
                        sctToSend.Send(code);

                        IFormatter formatter2 = new BinaryFormatter();
                        Stream stream2 = new MemoryStream();

                        //string message = String.Format("АДМИНИСТРАИТОР ЗАБАНИЛ {0}", ((ChatInfoDAL.User)(listBox.SelectedValue)).NickName);

                        formatter2.Serialize(stream2, (ChatInfoDAL.User)listBox.SelectedValue);

                        byte[] buffer2 = new byte[1024];
                        stream2.Position = 0;
                        while (stream2.Position < stream2.Length)
                        {
                            int readCount = stream2.Read(buffer2, 0, 1024);
                            sctToSend.Send(buffer2, readCount, 0);
                        }

                    }
                    else
                    {
                        if(((ChatInfoDAL.User)(listBox.SelectedValue)).Ban < DateTime.Now)
                            messInQueue.Add(new MessageQueue(user, String.Format("АДМИНИСТРАИТОР РАЗБАНИЛ {0}", (ChatInfoDAL.User)(listBox.SelectedValue))));
                        else
                            messInQueue.Add(new MessageQueue(user, String.Format("АДМИНИСТРАИТОР ЗАБАНИЛ {0}", (ChatInfoDAL.User)(listBox.SelectedValue))));
                    }
                }
                FillListUser(groupCombo.SelectedValue.ToString(), BanCombo.SelectedValue.ToString());
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedValue != null && messageBox.Text != "")
            {
                Dictionary<Socket, ChatInfoDAL.User>.ValueCollection userCol = dictSocketUser.Values;
                int numSocket = 0;
                bool online = false;
                for (numSocket = 0; numSocket < userCol.Count; numSocket++)
                {
                    if (userCol.ElementAt(numSocket) == listBox.SelectedValue) { online = true;  break; }
                }
                if (online)
                {
                    Action<string> action = MessageToInfoStackAdd;
                    string messa = String.Format("Сообщение отправлено {0}", listBox.SelectedValue);
                    infoStack.Dispatcher.Invoke(action, messa);

                    Socket sctToSend = dictSocketUser.ElementAt(numSocket).Key;

                    byte[] code = Encoding.UTF8.GetBytes("07");
                    sctToSend.Send(code);

                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new MemoryStream();

                    formatter.Serialize(stream, messageBox.Text);

                    byte[] buffer2 = new byte[1024];
                    stream.Position = 0;
                    while (stream.Position < stream.Length)
                    {
                        int readCount = stream.Read(buffer2, 0, 1024);
                        sctToSend.Send(buffer2, readCount, 0);
                    }
                    stream.Close();
                }
                else
                {
                    Action<string> action = MessageToInfoStackAdd;
                    string messa = String.Format("Сообщение для {0} поставлено в очередь", listBox.SelectedValue);
                    infoStack.Dispatcher.Invoke(action, messa);

                    messInQueue.Add(new MessageQueue((ChatInfoDAL.User)(listBox.SelectedValue), messageBox.Text));
                }
                messageBox.Text = "";
            }
        }//индивидуальное сообщение

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (messageBox.Text != "")
            {
                foreach (ChatInfoDAL.User user in listBox.Items)
                {
                    Dictionary<Socket, ChatInfoDAL.User>.ValueCollection userCol = dictSocketUser.Values;
                    int numSocket = -1;
                    bool online = false;
                    for (numSocket = 0; numSocket < userCol.Count; numSocket++)
                    {
                        if (userCol.ElementAt(numSocket) == user) { online = true; break; }
                    }
                    if (online)
                    {
                        Socket sctToSend = dictSocketUser.ElementAt(numSocket).Key;

                        byte[] code = Encoding.UTF8.GetBytes("07");
                        sctToSend.Send(code);

                        IFormatter formatter = new BinaryFormatter();
                        Stream stream = new MemoryStream();

                        formatter.Serialize(stream, messageBox.Text);

                        byte[] buffer2 = new byte[1024];
                        stream.Position = 0;
                        while (stream.Position < stream.Length)
                        {
                            int readCount = stream.Read(buffer2, 0, 1024);
                            sctToSend.Send(buffer2, readCount, 0);
                        }

                        Action<string> action = MessageToInfoStackAdd;
                        string messa = String.Format("Групповое сообщение отправлено для {0}", user.ToString());
                        infoStack.Dispatcher.Invoke(action, messa);

                        stream.Close();
                    }
                    else
                    {
                        Action<string> action = MessageToInfoStackAdd;
                        string messa = String.Format("Групповое сообщение поставлено в очередь для {0}", user.ToString());
                        infoStack.Dispatcher.Invoke(action, messa);

                        messInQueue.Add(new MessageQueue(user, messageBox.Text));
                    }
                }
                messageBox.Text = "";
            }
        }//массовое сообщение

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedValue != null && ((ChatInfoDAL.User)(listBox.SelectedValue)).Ban >= DateTime.Now)
            {
                ((ChatInfoDAL.User)(listBox.SelectedValue)).Ban = new DateTime(1900, 1, 1);

                string query = string.Format("update Users set Ban='{1}' where UserID={0}",
                    ((ChatInfoDAL.User)(listBox.SelectedValue)).UserID, "1900-01-01");
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("Пользователь {0} разбанен", listBox.SelectedValue);
                infoStack.Dispatcher.Invoke(action, messa);

                Dictionary<Socket, ChatInfoDAL.User>.KeyCollection socketCol = dictSocketUser.Keys;
                foreach (Socket sct in socketCol)
                {
                    byte[] code = Encoding.UTF8.GetBytes("08");
                    sct.Send(code);

                    IFormatter formatter2 = new BinaryFormatter();
                    Stream stream2 = new MemoryStream();

                    ChatInfoDAL.User user = (ChatInfoDAL.User)(listBox.SelectedValue);
                    formatter2.Serialize(stream2, user);

                    byte[] buffer2 = new byte[1024];
                    stream2.Position = 0;
                    while (stream2.Position < stream2.Length)
                    {
                        int readCount = stream2.Read(buffer2, 0, 1024);
                        sct.Send(buffer2, readCount, 0);
                    }
                }

                FillListUser(groupCombo.SelectedValue.ToString(), BanCombo.SelectedValue.ToString());
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (listBox2.SelectedValue != null)
            {
                ClaimToGroup currentClaim = (ClaimToGroup)listBox2.SelectedValue;
                MemberOfGroup memOfGroup = new MemberOfGroup(currentClaim.User.UserID, currentClaim.Group.Id);
                MemberOfGroupManager.MembersOfGroup.Add(memOfGroup);

                string query = string.Format("insert into MemeberOfGroups (GroupID, UserID) values ({0}, {1})",
                    currentClaim.Group.Id, currentClaim.User.UserID);
                SqlCommand command = new SqlCommand(query, DataBase.Sql);
                command.ExecuteNonQuery();

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("Заявка на вступление в группу {0} юзера {1} принята",
                    currentClaim.Group, currentClaim.User);
                infoStack.Dispatcher.Invoke(action, messa);

                int numOfSocket = -1;
                Dictionary<Socket, User>.ValueCollection usCol = dictSocketUser.Values;
                for (numOfSocket = 0; numOfSocket < usCol.Count; numOfSocket++)
                {
                    if (usCol.ElementAt(numOfSocket).UserID == currentClaim.User.UserID) { break; }
                }
                Socket socketToSend = dictSocketUser.ElementAt(numOfSocket).Key;

                byte[] code = Encoding.UTF8.GetBytes("10");
                socketToSend.Send(code);

                IFormatter formatter2 = new BinaryFormatter();
                Stream stream2 = new MemoryStream();

                formatter2.Serialize(stream2, memOfGroup);

                byte[] buffer2 = new byte[1024];
                stream2.Position = 0;
                while (stream2.Position < stream2.Length)
                {
                    int readCount = stream2.Read(buffer2, 0, 1024);
                    socketToSend.Send(buffer2, readCount, 0);
                }

                Thread.Sleep(50);
                List<Message> messToSend = new List<Message>();
                foreach (Message mess in MessageManager.Messages)
                {
                    if (mess.GroupID == memOfGroup.GroupID) messToSend.Add(mess);
                }

                byte[] code3 = Encoding.UTF8.GetBytes("11");
                socketToSend.Send(code3);

                IFormatter formatter3 = new BinaryFormatter();
                Stream stream3 = new MemoryStream();

                formatter3.Serialize(stream3, messToSend);

                byte[] buffer3 = new byte[1024];
                stream3.Position = 0;
                while (stream3.Position < stream3.Length)
                {
                    int readCount = stream3.Read(buffer3, 0, 1024);
                    socketToSend.Send(buffer3, readCount, 0);
                }

                Thread.Sleep(50);
                foreach (Socket socket in dictSocketUser.Keys)
                {
                    bool member = false;
                    foreach (MemberOfGroup mem in MemberOfGroupManager.MembersOfGroup)
                    {
                        if (dictSocketUser[socket].UserID == mem.UserID && mem.GroupID == memOfGroup.GroupID) { member = true; break; }
                    }
                    if (member)
                    {

                        byte[] code2 = Encoding.UTF8.GetBytes("07");
                        socket.Send(code2);

                        IFormatter formatter = new BinaryFormatter();
                        Stream stream = new MemoryStream();

                        formatter.Serialize(stream, String.Format("{0} принят в группу {1}",
                            currentClaim.User.ToString(), currentClaim.Group.ToString()));

                        byte[] buffer = new byte[1024];
                        stream.Position = 0;
                        while (stream.Position < stream.Length)
                        {
                            int readCount = stream.Read(buffer, 0, 1024);
                            socket.Send(buffer, readCount, 0);
                        }
                    }
                }

                listBox2.Items.Remove(listBox2.SelectedValue);
            }

            //else
            //{
            //    mainEvent.OnSomeEvent(123456);
            //}
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (listBox2.SelectedValue != null)
            {
                ClaimToGroup currentClaim = (ClaimToGroup)listBox2.SelectedValue;

                int numOfSocket = -1;
                Dictionary<Socket, User>.ValueCollection usCol = dictSocketUser.Values;
                for (numOfSocket = 0; numOfSocket < usCol.Count; numOfSocket++)
                {
                    if (usCol.ElementAt(numOfSocket).UserID == currentClaim.User.UserID) { break; }
                }
                Socket socketToSend = dictSocketUser.ElementAt(numOfSocket).Key;

                byte[] code = Encoding.UTF8.GetBytes("07");
                socketToSend.Send(code);

                IFormatter formatter2 = new BinaryFormatter();
                Stream stream2 = new MemoryStream();

                formatter2.Serialize(stream2, String.Format("Ваша заявка на вступление в группу {0} отвергнута",
                    currentClaim.Group));

                byte[] buffer2 = new byte[1024];
                stream2.Position = 0;
                while (stream2.Position < stream2.Length)
                {
                    int readCount = stream2.Read(buffer2, 0, 1024);
                    socketToSend.Send(buffer2, readCount, 0);
                }

                listBox2.Items.Remove(listBox2.SelectedValue);

                Action<string> action = MessageToInfoStackAdd;
                string messa = String.Format("Заявка на вступление в группу {0} юзера {1} отклонена",
                    currentClaim.Group, currentClaim.User);
                infoStack.Dispatcher.Invoke(action, messa);
            }
        }

        private void groupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListUser(groupCombo.SelectedValue.ToString(), BanCombo.SelectedValue.ToString());
        }

        private void BanCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListUser(groupCombo.SelectedValue.ToString(), BanCombo.SelectedValue.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
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

    }
}
