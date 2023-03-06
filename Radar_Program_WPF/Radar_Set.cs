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
        private string DB_Server;
        private string DB_Port;
        private string DB_ID;
        private string DB_PW;

        private MySqlConnection Database;
        private string table;

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

            return true; ;
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
        public void Initialize_DB_Value(string s = "127.0.0.1",
            string p = "9591",
            string ID = "root",
            string PW = "cody0901")
        {
            Set_DB_Server(s);
            Set_DB_Port(p);
            Set_DB_ID(ID);
            Set_DB_PW(PW);
        }
        public bool DB_Connect()
        {
            string ConStr = "Server = " + Get_DB_Server() +
                   ";port = " + Get_DB_Port() +
                   ";uid = " + Get_DB_ID() +
                   ";pwd = " + Get_DB_PW() + ";";

            Database = new MySqlConnection(ConStr);
            if (make_DB())
                if (make_Table())
                    return true;

            return false;
        }
        private bool make_DB()
        {
            Database.Open();
            MySqlCommand cmd = Database.CreateCommand();
            cmd.CommandText = "CREATE DATABASE IF NOT EXISTS RADAR;" +
                "USE RADAR;";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Database.Close();
                return false;
            }
            Database.Close();
            return true;
        }
        private bool make_Table()
        {
            Database.Open();
            table = "OBJ_INFO";
            MySqlCommand cmd = Database.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + table + "(" +
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
                Database.Close();
                return false;
            }
            Database.Close();
            return true;
        }
        public bool save_DB(string data)
        {
            Database.Open();
            MySqlCommand cmd = Database.CreateCommand();

            cmd.CommandText = data + ";";

            System.Console.WriteLine("time: {0}", cmd.CommandText);

            try
            {
                cmd.ExecuteNonQuery();

            }
            catch
            {
                Database.Close();
                return false;
            }
            Database.Close();
            return true;
        }
        public bool DB_Disconnect()
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
        public string Get_DB_ID()
        {
            return DB_ID;
        }
        public void Set_DB_ID(string input)
        {
            DB_ID = input;
        }
        public string Get_DB_PW()
        {
            return DB_PW;
        }
        public void Set_DB_PW(string input)
        {
            DB_PW = input;
        }
        public string Get_DB_Server()
        {
            return DB_Server;
        }
        public void Set_DB_Server(string input)
        {
            DB_Server = input;
        }
        public string Get_DB_Port()
        {
            return DB_Port;
        }
        public void Set_DB_Port(string input)
        {
            DB_Port = input;
        }
        #endregion
        #endregion
    }
}