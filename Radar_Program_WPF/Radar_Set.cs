using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//for radar
using Peak.Can.Basic;
using TPCANHandle = System.UInt16;
using TPCANBitrateFD = System.String;
using TPCANTimestampFD = System.UInt64;
//for DB
//using MySql.Data.MySqlClient;

//for Obj info
using MsgFormat;
using MySql.Data.MySqlClient;


namespace Radar_Program_WPF
{
    class Radar_Set
    {
        #region member
        #region radar
        private TPCANHandle PcanHandle;
        private TPCANBaudrate Baudrate;
        private TPCANType HwType;
        private uint IOPort;
        private ushort Interrupt;
        private int Radar_ID;
        public Msg_Format Msg_format = new Msg_Format();
        #endregion
        #region DB
        #region Local DB
        private MySqlConnection local_DB;
        private string local_IP = "127.0.0.1";
        private string local_PORT = "3306";
        private string local_ID = "root";
        private string local_PW = "0000";
        public string local_DBNAME = "RADAR";
        public string local_TABLENAME = "TEST_DATA";
        #endregion
        #region Server DB
        private MySqlConnection server_DB;
        private string server_IP = "183.99.41.239";
        private string server_PORT = "23306";
        private string server_ID = "root";
        private string server_PW = "hbrain0372!";
        public string server_DBNAME = "RADAR";
        public string server_TABLENAME = "TEST_DATA";
        #endregion
        #endregion
        #region obj info
        public LinkedList<Msg_Format.Object_inf>[] Obj_inf = new LinkedList<Msg_Format.Object_inf>[100];
        #endregion
        #region Clst info
        //public Msg_Format.Cluster_inf[,] Clst_inf = new Msg_Format.Cluster_inf[100, 100];
        #endregion
        #endregion

        #region Radar method
        public void Initialize_Radar_Value(int id = 0,
            TPCANHandle h = 0x51,
            TPCANBaudrate b = TPCANBaudrate.PCAN_BAUD_500K,
            TPCANType t = TPCANType.PCAN_TYPE_ISA,
            uint p = 0100,
            ushort i = 3)
        {
            Set_Radar_ID(id);
            Set_PCAN_Handle(h);
            Set_PCAN_Baudrate(b);
            Set_PCAN_HWType(t);
            Set_PCAN_Port(p);
            Set_PCAN_Interrupt(i);

            for(int NODE = 0; NODE < 100; NODE++)
                Obj_inf[NODE] = new LinkedList<Msg_Format.Object_inf>();
        }
        public bool Radar_Connect()
        {
            TPCANStatus stsResult;
            stsResult = PCANBasic.Initialize(
                PcanHandle,
                Baudrate,
                HwType,
                IOPort,
                Interrupt);

            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                return false;

            return true;
        }
        public bool Radar_Disconnect()
        {
            TPCANStatus stsResult;

            stsResult = PCANBasic.Uninitialize(PcanHandle);

            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                return false;

            return true;
        }
        #endregion

        #region DB method
        public void Initialize_DB_Value(
            string localIP = "127.0.0.1", string localPORT = "3306", string localID = "root", string localPW = "0000",
            string serverIP = "183.99.41.239", string serverPORT = "23306", string serverID = "root", string serverPW = "hbrain0372!")
        {
            Set_localDB_IP(localIP);
            Set_localDB_PORT(localPORT);
            Set_localDB_ID(localID);
            Set_localDB_PW(localPW);
            Set_serverDB_IP(serverIP);
            Set_serverDB_PORT(serverPORT);
            Set_serverDB_ID(serverID);
            Set_serverDB_PW(serverPW);
        }
        #region local DB
        public void localDB_Connect()
        {
            make_localDB();

            string ConStr = "Server = " + Get_localDB_IP() +
                    ";port = " + Get_localDB_PORT() +
                ";database = " + local_DBNAME +
                    ";uid = " + Get_localDB_ID() +
                    ";pwd = " + Get_localDB_PW() + ";";

            local_DB = new MySqlConnection(ConStr);
            make_localTable();
        }
        private void make_localDB()
        {
            string ConStr = "Server = " + Get_localDB_IP() +
                    ";port = " + Get_localDB_PORT() +
                    ";uid = " + Get_localDB_ID() +
                    ";pwd = " + Get_localDB_PW() + ";";

            MySqlConnection create_localDB = new MySqlConnection(ConStr);
            create_localDB.Open();
            MySqlCommand cmd = create_localDB.CreateCommand();
            cmd.CommandText = "CREATE DATABASE IF NOT EXISTS " + local_DBNAME + ";";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("make local DB ERROR");
            }
            create_localDB.Close();
        }
        private void make_localTable()
        {
            local_DB.Open();
            MySqlCommand cmd = local_DB.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + local_TABLENAME + "(" +
                "TIME DATETIME(3) NOT NULL," +
                "ID INT NOT NULL," +
                "DISTLONG DOUBLE DEFAULT NULL," +
                "DISTLAT DOUBLE DEFAULT NULL," +
                "VRELLONG DOUBLE DEFAULT NULL," +
                "VRELLAT DOUBLE DEFAULT NULL," +
                "DYNPROP INT DEFAULT NULL," +
                "RCS DOUBLE DEFAULT NULL," +
                "DISTLAT_RMS INT DEFAULT NULL," +
                "DISTLONG_RMS INT DEFAULT NULL," +
                "VRELLAT_RMS INT DEFAULT NULL," +
                "VRELLONG_RMS INT DEFAULT NULL," +
                "ARELLAT_RMS INT DEFAULT NULL," +
                "ARELLONG_RMS INT DEFAULT NULL," +
                "ORIENTATION_RMS INT DEFAULT NULL," +
                "MIRRORPROB INT DEFAULT NULL," +
                "PROBOFEXIST INT DEFAULT NULL," +
                "MEASSTATE INT DEFAULT NULL," +
                "ARELLONG DOUBLE DEFAULT NULL," +
                "ARELLAT DOUBLE DEFAULT NULL," +
                "CLASS INT DEFAULT NULL," +
                "ORIEMTATIONANGLE DOUBLE DEFAULT NULL," +
                "LENGTH DOUBLE DEFAULT NULL," +
                "WIDTH DOUBLE DEFAULT NULL," +
                "PRIMARY KEY(TIME, ID));";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("make local table ERROR");
            }
            local_DB.Close();
        }
        public void save_localDB(string data)
        {
            local_DB.Open();
            MySqlCommand cmd = local_DB.CreateCommand();

            cmd.CommandText = data + ";";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("save local DB ERROR");
            }
            local_DB.Close();
        }
        public bool localDB_Disconnect()
        {
            /*
            try
            {
                Database.Close();
            }
            catch
            {
                return false;
            }
            return true;
            */
            return true;
        }
        #endregion
        #region server DB
        public void serverDB_Connect()
        {
            make_serverDB();

            string ConStr = "Server = " + Get_serverDB_IP() +
                ";port = " + Get_serverDB_PORT() +
                ";database = " + server_DBNAME +
                ";uid = " + Get_serverDB_ID() +
                ";pwd = " + Get_serverDB_PW() + ";";
            server_DB = new MySqlConnection(ConStr);
            make_serverTable();
        }
        private void make_serverDB()
        {
            string ConStr = "Server = " + Get_serverDB_IP() +
                ";port = " + Get_serverDB_PORT() +
                ";uid = " + Get_serverDB_ID() +
                ";pwd = " + Get_serverDB_PW() + ";";

            MySqlConnection create_serverDB = new MySqlConnection(ConStr);
            create_serverDB.Open();
            MySqlCommand cmd = create_serverDB.CreateCommand();
            cmd.CommandText = "CREATE DATABASE IF NOT EXISTS " + server_DBNAME + ";";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("make local DB ERROR");
            }
            create_serverDB.Close();
        }
        private void make_serverTable()
        {
            server_DB.Open();
            MySqlCommand cmd = server_DB.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + server_TABLENAME + "(" +
                "TIME DATETIME(3) NOT NULL," +
                "ID INT NOT NULL," +
                "DISTLONG DOUBLE DEFAULT NULL," +
                "DISTLAT DOUBLE DEFAULT NULL," +
                "VRELLONG DOUBLE DEFAULT NULL," +
                "VRELLAT DOUBLE DEFAULT NULL," +
                "DYNPROP INT DEFAULT NULL," +
                "RCS DOUBLE DEFAULT NULL," +
                "DISTLAT_RMS INT DEFAULT NULL," +
                "DISTLONG_RMS INT DEFAULT NULL," +
                "VRELLAT_RMS INT DEFAULT NULL," +
                "VRELLONG_RMS INT DEFAULT NULL," +
                "ARELLAT_RMS INT DEFAULT NULL," +
                "ARELLONG_RMS INT DEFAULT NULL," +
                "ORIENTATION_RMS INT DEFAULT NULL," +
                "MIRRORPROB INT DEFAULT NULL," +
                "PROBOFEXIST INT DEFAULT NULL," +
                "MEASSTATE INT DEFAULT NULL," +
                "ARELLONG DOUBLE DEFAULT NULL," +
                "ARELLAT DOUBLE DEFAULT NULL," +
                "CLASS INT DEFAULT NULL," +
                "ORIEMTATIONANGLE DOUBLE DEFAULT NULL," +
                "LENGTH DOUBLE DEFAULT NULL," +
                "WIDTH DOUBLE DEFAULT NULL," +
                "PRIMARY KEY(TIME, ID));";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("make server table ERROR");
            }
            server_DB.Close();
        }
        public void update_Data_to_ServerDB()
        {
            try
            {
                local_DB.Open();
                string query = "SELECT * FROM " + local_TABLENAME + ";";

                using (MySqlCommand cmd = new MySqlCommand(query, local_DB))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        string sql = "INSERT INTO " + server_TABLENAME + " VALUES";
                        int count = 0;
                        while (reader.Read())
                        {

                            sql += "('" + ((DateTime)reader["TIME"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "', " +
                                                reader["ID"] + ", " + reader["DISTLONG"] + ", " +
                                                reader["DISTLAT"] + ", " + reader["VRELLONG"] + ", " +
                                                reader["VRELLAT"] + ", " + reader["DYNPROP"] + ", " +
                                                reader["RCS"] + ", " + reader["DISTLAT_RMS"] + ", " +
                                                reader["DISTLONG_RMS"] + ", " + reader["VRELLAT_RMS"] + ", " +
                                                reader["VRELLONG_RMS"] + ", " + reader["ARELLAT_RMS"] + ", " +
                                                reader["ARELLONG_RMS"] + ", " + reader["ORIENTATION_RMS"] + ", " +
                                                reader["MIRRORPROB"] + ", " + reader["PROBOFEXIST"] + ", " +
                                                reader["MEASSTATE"] + ", " + reader["ARELLONG"] + ", " +
                                                reader["ARELLAT"] + ", " + reader["CLASS"] + ", " +
                                                reader["ORIEMTATIONANGLE"] + ", " + reader["LENGTH"] + ", " +
                                                reader["WIDTH"] +
                                                "),";

                            count++;
                            if(count >= 1000)
                            {
                                sql = sql.TrimEnd(',');
                                save_serverDB(sql);
                                
                                sql = "INSERT INTO " + server_TABLENAME + " VALUES";
                                count = 0;
                            }
                        }
                        sql = sql.TrimEnd(',');
                        save_serverDB(sql);
                    }
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("update Data to Server DB ERROR");
            }
        }
        public void save_serverDB(string data)
        {
            server_DB.Open();
            MySqlCommand cmd = server_DB.CreateCommand();

            cmd.CommandText = data + ";";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                System.Console.WriteLine("save local DB ERROR");
            }
            server_DB.Close();
        }
        public bool serverDB_Disconnect()
        {
            /*
            try
            {
                Database.Close();
            }
            catch
            {
                return false;
            }
            return true;
            */
            return true;
        }
        #endregion
        #endregion

        #region control member
        #region radar
        public TPCANHandle Get_PCAN_Handle()
        {
            return PcanHandle;
        }
        public void Set_PCAN_Handle(TPCANHandle input)
        {
            PcanHandle = input;
        }
        public TPCANBaudrate Get_PCAN_Baudrate()
        {
            return Baudrate;
        }
        public void Set_PCAN_Baudrate(TPCANBaudrate input)
        {
            Baudrate = input;
        }
        public TPCANType Get_PCAN_HWType()
        {
            return HwType;
        }
        public void Set_PCAN_HWType(TPCANType input)
        {
            HwType = input;
        }
        public uint Get_PCAN_Port()
        {
            return IOPort;
        }
        public void Set_PCAN_Port(uint input)
        {
            IOPort = input;
        }
        public ushort Get_PCAN_Interrupt()
        {
            return Interrupt;
        }
        public void Set_PCAN_Interrupt(ushort input)
        {
            Interrupt = input;
        }
        public int Get_Radar_ID()
        {
            return Radar_ID;
        }
        public void Set_Radar_ID(int input)
        {
            if ((input >= 0) && (input <= 7))
                Radar_ID = input;
        }
        #endregion
        #region DB
        #region local DB
        public string Get_localDB_IP()
        {
            return local_IP;
        }
        public void Set_localDB_IP(string input)
        {
            local_IP = input;
        }
        public string Get_localDB_PORT()
        {
            return local_PORT;
        }
        public void Set_localDB_PORT(string input)
        {
            local_PORT = input;
        }
        public string Get_localDB_ID()
        {
            return local_ID;
        }
        public void Set_localDB_ID(string input)
        {
            local_ID = input;
        }
        public string Get_localDB_PW()
        {
            return local_PW;
        }
        public void Set_localDB_PW(string input)
        {
            local_PW = input;
        }
        #endregion
        #region server DB
        public string Get_serverDB_IP()
        {
            return server_IP;
        }
        public void Set_serverDB_IP(string input)
        {
            server_IP = input;
        }
        public string Get_serverDB_PORT()
        {
            return server_PORT;
        }
        public void Set_serverDB_PORT(string input)
        {
            server_PORT = input;
        }
        public string Get_serverDB_ID()
        {
            return server_ID;
        }
        public void Set_serverDB_ID(string input)
        {
            server_ID = input;
        }
        public string Get_serverDB_PW()
        {
            return server_PW;
        }
        public void Set_serverDB_PW(string input)
        {
            server_PW = input;
        }
        #endregion
        #endregion
        #endregion
    }
}