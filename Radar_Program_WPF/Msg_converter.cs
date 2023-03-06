/* 
 * using MsgConverter
 * 
 * Msg_Converter Msg_Format = new Msg_Converter();  
 * 
 * //IN MESSAGE
 * 
 * (ex.0x200)
 * Msg_Converter.Radar_Config RC;
 * RC.* = value;
 * 
 * TPCANMsg Msg = Msg_Format.RadarConfig2msg(RC, 'Your Sensor ID');
 * 
 * PCANBasic.Write(m_PcanHandle, ref Msg);
 * 
 * //OUT MESSAGE
 * 
 * (ex.0x201) 
 * Msg_Converter.Radar_state RS = Msg_Format.msg2RadarState(Msg);
 * 
 * int test = RS.*;
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Peak.Can.Basic;

namespace MsgFormat
{
    class Msg_Format
    {
        #region Object_Data
        public struct Object_inf
        {
            // General      
            public int ID;
            public double DistLong;
            public double DistLat;
            public double Distance;
            public double VrelLong;
            public int DynProp;
            public double VrelLat;
            public double Speed;
            public double RCS;

            // Quality
            public int DistLat_rms;
            public int DistLong_rms;
            public int VrelLat_rms;
            public int VrelLong_rms;
            public int ArelLat_rms;
            public int ArelLong_rms;
            public int Orientation_rms;
            public int MirrorProb;
            public int MeasState;
            public int ProbOfExist;

            // Extended
            public double ArelLat;
            public double ArelLong;
            public int Class;
            public double OrientationAngle;
            public double Length;
            public double Width;

            // Other
            public DateTime Timestamp;
            public bool isObjectWorthy;
            public int MeasCounter;
            public bool isGhost;
            public int zone_index;
            public bool Dangerous;
            public bool Read_C;
            public bool Read_D;
        }
        #endregion

        #region Cluster_Data
        //public struct Cluster_inf
        //{
        //    // Cluster status
        //    public int NofClustersNear;
        //    public int NofClusterFar;
        //    public int MeasCounter;
        //    public int InterfaceVersion;

        //    // Cluster General
        //    public int TargetID;
        //    public double DistLong;
        //    public double DistLat;
        //    public double VrelLong;
        //    public double VrelLat;
        //    public int DynProp;
        //    public double RCS;

        //    // Cluster Quality
        //    public double DistLong_rms;
        //    public double DistLat_rms;
        //    public double VrelLong_rms;
        //    public double VrelLat_rms;
        //    public int Pdh0;
        //    public int InvalidState;
        //    public int AmbifState;
        //}
        #endregion

        #region SW_version          0x700
        public struct SWversion
        {
            public int MajorRelease;
            public int MinorRelease;
            public int PatchLevel;
            public int ExtendedRange;
            public int CountryCode;
        }

        public SWversion msg2SWversion(TPCANMsg msg)
        {
            SWversion SW;

            SW.MajorRelease = msg.DATA[0];
            SW.MinorRelease = msg.DATA[1];
            SW.PatchLevel = msg.DATA[2];
            SW.CountryCode = msg.DATA[3] & 0x1;
            SW.ExtendedRange = msg.DATA[3] >> 1;

            return SW;
        }
        #endregion

        #region Radar_Cfg           0x200 IN
        public struct Radar_Config
        {
            public int MaxDistance_valid;
            public int SensorID_valid;
            public int RadarPower_valid;
            public int OutputType_valid;
            public int SendQuality_valid;
            public int SendExtInfo_valid;
            public int SortIndex_valid;
            public int StoreInNVM_valid;
            public int MaxDistance;
            public int SensorID;
            public int OutputType;
            public int RadarPower;
            public int CtrlRelay_valid;
            public int CtrlRelay;
            public int SendQuality;
            public int SendExtInfo;
            public int SortIndex;
            public int StoreInNVM;
            public int RCS_Threshold_valid;
            public int RCS_Threshold;
            public int InvalidClusters_valid;
            public int InvalidClusters;
        };

        public TPCANMsg RadarConfig2msg(Radar_Config RC, int radarSensorID)
        {
            TPCANMsg msg;
            msg.ID = 0x200 + (uint)(radarSensorID * 0x10);
            msg.DATA = new byte[8];
            msg.LEN = 5;
            msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

            msg.DATA[0] = (byte)((RC.StoreInNVM_valid << 7)
                + (RC.SortIndex_valid << 6)
                + (RC.SendExtInfo_valid << 5)
                + (RC.SendQuality << 4)
                + (RC.OutputType_valid << 3)
                + (RC.RadarPower_valid << 2)
                + (RC.SensorID_valid << 1)
                + (RC.MaxDistance_valid));
            msg.DATA[1] = (byte)(RC.MaxDistance/2 >> 2);
            msg.DATA[2] = (byte)((RC.MaxDistance/2 & 0x3) << 6);
            msg.DATA[3] = 0;
            msg.DATA[4] = (byte)((RC.RadarPower << 5)
                + (RC.OutputType << 3)
                + (RC.SensorID));
            msg.DATA[5] = (byte)((RC.StoreInNVM << 7)
                + (RC.SortIndex << 4)
                + (RC.SendExtInfo << 3)
                + (RC.SendQuality << 2)
                + (RC.CtrlRelay << 1)
                + RC.CtrlRelay_valid);
            msg.DATA[6] = (byte)((RC.InvalidClusters_valid << 4)
                + (RC.RCS_Threshold << 1)
                + RC.RCS_Threshold_valid);
            msg.DATA[7] = (byte)(RC.InvalidClusters);

            return msg;

        }
        #endregion        

        #region Filter_Config       0x202 IN
        public struct Filter_Config
        {
            public int Valid;
            public int Active;
            public int Index;
            public int Type;
            public double Min;
            public double Max;
        };

        public TPCANMsg FilterConfig2msg(Filter_Config FC, int radarSensorID)
        {

            TPCANMsg msg;
            msg.ID = 0x202 + (uint)(radarSensorID * 0x10);
            msg.DATA = new byte[8];
            msg.LEN = 5;
            msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

            switch (FC.Index)
            {
                case 0: //NofObj
                    break;
                case 1: //Distance
                    FC.Min = FC.Min / 0.1;
                    FC.Max = FC.Max / 0.1;
                    break;
                case 2: //Azimuth
                    FC.Min = (FC.Min + 50) / 0.025;
                    FC.Max = (FC.Max + 50) / 0.025;
                    break;
                case 3: //VrelOncome
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 4: //VrelDepart
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 5: //RCS
                    FC.Min = (FC.Min + 50) / 0.025;
                    FC.Max = (FC.Max + 50) / 0.025;
                    break;
                case 6: //Lifetime
                    FC.Min = FC.Min / 0.1;
                    FC.Max = FC.Max / 0.1;
                    break;
                case 7: //Size
                    FC.Min = FC.Min / 0.025;
                    FC.Max = FC.Max / 0.025;
                    break;
                case 8: //ProbExists
                    break;
                case 9: //Y
                    FC.Min = (FC.Min + 409.5) / 0.2;
                    FC.Max = (FC.Max + 409.5) / 0.2;
                    break;
                case 10: //X
                    FC.Min = (FC.Min + 500) / 0.2;
                    FC.Max = (FC.Max + 500) / 0.2;
                    break;
                case 11: //VYRightLeft
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 12: //VXOncome
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 13: //VYLeftRight
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 14: //VXDepart
                    FC.Min = FC.Min / 0.0315;
                    FC.Max = FC.Max / 0.0315;
                    break;
                case 15: //Class
                    break;
            }
            msg.DATA[0] = (byte)((FC.Type << 7) + (FC.Index << 3) + (FC.Active << 2) + (FC.Valid << 1));
            msg.DATA[1] = (byte)((int)FC.Min >> 8);
            msg.DATA[2] = (byte)((int)FC.Min & 0xFF);
            msg.DATA[3] = (byte)((int)FC.Max >> 8);
            msg.DATA[4] = (byte)((int)FC.Max & 0xFF);

            return msg;
        }
        #endregion

        #region CollDetCfg          0x400 IN

        public struct CollDetCfg
        {
            public int WarningReset;
            public int Activation;
            public int MinTime_Valid;
            public int clearReigons;
            public double MinTime;
        }

        public TPCANMsg CollDetCfg2msg(CollDetCfg CD, int radarSensorID)
        {
            TPCANMsg msg;
            msg.ID = (uint)(0x400 + (radarSensorID * 0x10));
            msg.LEN = 2;
            msg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            msg.DATA = new byte[8];

            msg.DATA[0] = (byte)((CD.clearReigons << 7)
                + (CD.MinTime_Valid << 3)
                + (CD.Activation << 1)
                + CD.WarningReset);
            msg.DATA[1] = (byte)CD.MinTime;

            return msg;
        }
        #endregion

        #region CollDetRegCfg       0x401 IN
        public struct CollDetRegionCfg
        {
            public int Activation;
            public int Coordinates_valid;
            public int RegionID;
            public double Point1X;
            public double Point1Y;
            public double Point2X;
            public double Point2Y;
        }

        #endregion

        #region Radar_state         0x201 OUT
        public struct Radar_state
        {
            public int NVMReadStatus;
            public int NVMwriteStatus;
            public int MaxDistanceCfg;
            public int Voltage_Error;
            public int Temporary_Error;
            public int Temperature_Error;
            public int Interference;
            public int Persistent_Error;
            public int RadarPowerCfg;
            public int SensorID;
            public int SortIndex;
            public int CtrlRelayCfg;
            public int OutputTypeCfg;
            public int SendQualityCfg;
            public int SendExtInfoCfg;
            public int MotionRxState;
            public int InvalidClusters;
            public int RCS_Threshold;

        }
        public Radar_state msg2RadarState(TPCANMsg msg)
        {
            Radar_state RS;

            RS.NVMReadStatus = msg.DATA[0] >> 6 & 0x01;
            RS.NVMwriteStatus = msg.DATA[0] >> 7;
            RS.MaxDistanceCfg = ((msg.DATA[1] << 2) + (msg.DATA[2] >> 6)) * 2;
            RS.Persistent_Error = (msg.DATA[2] >> 5) & 0x01;
            RS.Interference = (msg.DATA[2] >> 4) & 0x01;
            RS.Temperature_Error = (msg.DATA[2] >> 3) & 0x01;
            RS.Temporary_Error = (msg.DATA[2] >> 2) & 0x01;
            RS.Voltage_Error = (msg.DATA[2] >> 1) & 0x1;
            RS.SensorID = msg.DATA[4] & 0x7;
            RS.SortIndex = (msg.DATA[4] >> 4) & 0x7;
            RS.RadarPowerCfg = (msg.DATA[3] << 1) + (msg.DATA[4] >> 7);
            RS.CtrlRelayCfg = (msg.DATA[5] >> 1) & 0x1;
            RS.OutputTypeCfg = msg.DATA[5] >> 2 & 0x3;
            RS.SendQualityCfg = (msg.DATA[5] >> 4) & 0x01;
            RS.SendExtInfoCfg = (msg.DATA[5] >> 5) & 0x01;
            RS.MotionRxState = msg.DATA[5] >> 6;
            RS.RCS_Threshold = msg.DATA[6];
            RS.InvalidClusters = msg.DATA[7] >> 2;

            return RS;

        }

        #endregion

        #region FilterState_Config  0x204 OUT
        public struct FilterState_Config
        {
            public int Active;
            public int Index;
            public int Type;
            public double Min;
            public double Max;
        };
        public FilterState_Config msg2FilterStateCfg(TPCANMsg msg)
        {
            FilterState_Config FSC;

            FSC.Active = (msg.DATA[0] >> 2) & 0x01;
            FSC.Index = (msg.DATA[0] >> 3) & 0xF;
            FSC.Type = msg.DATA[0] >> 7;

            FSC.Min = (msg.DATA[1] << 8) + msg.DATA[2];
            FSC.Max = (msg.DATA[3] << 8) + msg.DATA[4];

            switch (FSC.Index)
            {
                case 0: //NofObj
                    FSC.Min = (msg.DATA[1] << 8) + msg.DATA[2];
                    FSC.Max = (msg.DATA[3] << 8) + msg.DATA[4];
                    break;
                case 1: //Distance
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.1;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.1;
                    break;
                case 2: //Azimuth
                    FSC.Min = (((msg.DATA[1] << 8) + msg.DATA[2]) * 0.025) - 50;
                    FSC.Max = (((msg.DATA[3] << 8) + msg.DATA[4]) * 0.025) - 50;
                    break;
                case 3: //VrelOncome
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 4: //VrelDepart
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 5: //RCS
                    FSC.Min = (((msg.DATA[1] << 8) + msg.DATA[2]) * 0.025) - 50;
                    FSC.Max = (((msg.DATA[3] << 8) + msg.DATA[4]) * 0.025) - 50;
                    break;
                case 6: //Lifetime
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.1;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.1;
                    break;
                case 7: //Size
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.025;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.025;
                    break;
                case 8: //ProbExists
                    FSC.Min = (msg.DATA[1] << 8) + msg.DATA[2];
                    FSC.Max = (msg.DATA[3] << 8) + msg.DATA[4];
                    break;
                case 9: //Y
                    FSC.Min = (((msg.DATA[1] << 8) + msg.DATA[2]) * 0.2) - 409.5;
                    FSC.Max = (((msg.DATA[3] << 8) + msg.DATA[4]) * 0.2) - 409.5;
                    break;
                case 10: //X
                    FSC.Min = (((msg.DATA[1] << 8) + msg.DATA[2]) * 0.2) - 500;
                    FSC.Max = (((msg.DATA[3] << 8) + msg.DATA[4]) * 0.2) - 500;
                    break;
                case 11: //VYRightLeft
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 12: //VXOncome
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 13: //VYLeftRight
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 14: //VXDepart
                    FSC.Min = ((msg.DATA[1] << 8) + msg.DATA[2]) * 0.0315;
                    FSC.Max = ((msg.DATA[3] << 8) + msg.DATA[4]) * 0.0315;
                    break;
                case 15: //Class
                    FSC.Min = (msg.DATA[1] << 8) + msg.DATA[2];
                    FSC.Max = (msg.DATA[3] << 8) + msg.DATA[4];
                    break;

            }

            return FSC;
        }
        #endregion

        #region Object_Status       0x60A OUT
        public struct Object_Status
        {
            public int NofObjects;
            public int MeasCounter;
            public int InterfaceVersion;
        }
        public Object_Status msg2ObjectStatus(TPCANMsg msg)
        {
            Object_Status OS;

            OS.NofObjects = msg.DATA[0];
            OS.MeasCounter = (msg.DATA[1] << 8) + msg.DATA[2];
            OS.InterfaceVersion = msg.DATA[3] >> 4;

            return OS;
        }
        #endregion

        #region Object_General      0x60B OUT
        public struct Object_General
        {
            public int ID;
            public double DistLong;
            public double DistLat;
            public double VrelLong;
            public double VrelLat;
            public int DynProp;
            public double RCS;
        };
        public Object_General msg2ObjectGeneral(TPCANMsg msg)
        {
            Object_General OG;

            OG.ID = msg.DATA[0];
            OG.DistLong = (((msg.DATA[1] << 5) + (msg.DATA[2] >> 3)) * 0.2) - 500;
            OG.DistLat = ((((msg.DATA[2] & 0x7) << 8) + msg.DATA[3]) * 0.2) - 204.6;
            OG.VrelLong = (((msg.DATA[4] << 2) + (msg.DATA[5] >> 6)) * 0.25) - 128.0;
            OG.VrelLat = ((((msg.DATA[5] & 0x3F) << 3) + (msg.DATA[6] >> 5)) * 0.25) - 64.0;
            OG.DynProp = msg.DATA[6] & 0x7;
            OG.RCS = (msg.DATA[7] * 0.5) - 64.0;

            return OG;
        }
        #endregion

        #region Object_Quality      0x60C OUT
        public struct Object_Quality
        {
            public int ID;
            public int DistLat_rms;
            public int DistLong_rms;
            public int VrelLat_rms;
            public int VrelLong_rms;
            public int ArelLong_rms;
            public int Orientation_rms;
            public int ArelLat_rms;
            public int MirrorProb;
            public int ProbOfExist;
            public int MeasState;
        };
        public Object_Quality msg2ObjectQuality(TPCANMsg msg)
        {
            Object_Quality OQ;

            OQ.ID = msg.DATA[0];
            OQ.DistLat_rms = ((msg.DATA[1] & 0x7) << 2) + (msg.DATA[2] >> 6);
            OQ.DistLong_rms = msg.DATA[1] >> 3;
            OQ.VrelLat_rms = ((msg.DATA[2] & 0x01) << 4) + (msg.DATA[3] >> 4);
            OQ.VrelLong_rms = (msg.DATA[2] >> 1) & 0x1F;
            OQ.ArelLat_rms = (msg.DATA[4] >> 2) & 0x1F;
            OQ.ArelLong_rms = ((msg.DATA[3] & 0xF) << 1) + (msg.DATA[4] >> 7);
            OQ.Orientation_rms = ((msg.DATA[4] & 0x3) << 3) + (msg.DATA[5] >> 5);
            OQ.MirrorProb = msg.DATA[5] & 0x7;
            OQ.ProbOfExist = msg.DATA[6] >> 5;
            OQ.MeasState = (msg.DATA[6] >> 2) & 0x7;

            return OQ;

        }

        #endregion

        #region Object_Extended     0x60D OUT
        public struct Object_Extended
        {
            public int ID;
            public double ArelLong;
            public double ArelLat;
            public int Class;
            public double OrientationAngle;
            public double Length;
            public double Width;
        };

        public Object_Extended msg2ObjectExtended(TPCANMsg msg)
        {
            Object_Extended OE;

            OE.ID = msg.DATA[0];
            OE.ArelLong = ((msg.DATA[1] << 3) + (msg.DATA[2] >> 5)) - 10.0;
            OE.ArelLat = (((msg.DATA[2] & 0x1F) << 4) + (msg.DATA[3] >> 4)) - 2.50;
            OE.Class = msg.DATA[3] & 0x7;
            OE.OrientationAngle = ((msg.DATA[4] << 2) + (msg.DATA[5] >> 6)) - 3.2;
            OE.Length = msg.DATA[6];
            OE.Width = msg.DATA[7];

            return OE;
        }
        #endregion

        #region Object_Collision    0x60E OUT
        public struct Object_Collision
        {
            public int ID;
            public int CollDetRegionBitfield;

            //Reserved
            //public int Reserved;    
        }
        public Object_Collision msg2ObjectCollision(TPCANMsg msg)
        {
            Object_Collision OC;

            OC.ID = msg.DATA[0];
            OC.CollDetRegionBitfield = msg.DATA[1];

            return OC;
        }
        #endregion

        #region Cluster_Status      0x600 OUT
        public struct Cluster_Status
        {
            public int NofClustersNear;
            public int NofClustersFar;
            public int MeasCounter;
            public int InterfaceVersion;
        };

        public Cluster_Status msg2ClusterStatus(TPCANMsg msg)
        {
            Cluster_Status CS;

            CS.NofClustersNear = msg.DATA[0];
            CS.NofClustersFar = msg.DATA[1];
            CS.MeasCounter = (msg.DATA[2] << 8) + msg.DATA[3];
            CS.InterfaceVersion = msg.DATA[4] >> 4;

            return CS;
        }
        #endregion

        #region Cluster_General     0x701 OUT
        public struct Cluster_General
        {
            public int TargetID;
            public double TargetDistLong;
            public double TargetDistLat;
            public double TargetVrelLong;
            public double TargetVrelLat;
            public int TargetDynProp;
            public double TargetRCS;
        };

        public Cluster_General msg2ClusterGeneral(TPCANMsg msg)
        {
            Cluster_General CG;
            CG.TargetID = msg.DATA[0];
            CG.TargetDistLong = (((msg.DATA[1] << 5) + (msg.DATA[2] >> 3)) * 0.2) - 500;
            CG.TargetDistLat = ((((msg.DATA[2] & 0x3) << 8) + msg.DATA[3]) * 0.2) - 102.3;
            CG.TargetVrelLong = (((msg.DATA[4] << 8) + (msg.DATA[5] >> 6)) * 0.25) - 128;
            CG.TargetVrelLat = ((((msg.DATA[5] & 0x3F) << 3) + (msg.DATA[6] >> 5)) * 0.25) - 64;
            CG.TargetDynProp = msg.DATA[6] & 0x7;
            CG.TargetRCS = (msg.DATA[7] * 0.5) - 64;

            return CG;
        }

        #endregion

        #region Cluster_Quality     0x702 OUT
        public struct Cluster_Quality
        {
            public int TargetID;
            public int TargetDistLat_rms;
            public int TargetDistLong_rms;
            public int TargetVrelLat_rms;
            public int TargetVrelLong_rms;
            public int TargetPdH0; //군집의 잘못된 경보 확률
            public int TargetAmbigState;
            public int TargetInvalidState;
        };

        public Cluster_Quality msg2ClusterQuality(TPCANMsg msg)
        {
            Cluster_Quality CQ;
            CQ.TargetID = msg.DATA[0];
            CQ.TargetDistLong_rms = msg.DATA[1] >> 3;
            CQ.TargetDistLat_rms = ((msg.DATA[1] & 0x7) << 2) + msg.DATA[2 >> 6];
            CQ.TargetVrelLong_rms = (msg.DATA[2] >> 1) & 0x1F;
            CQ.TargetVrelLat_rms = ((msg.DATA[2] & 0x1) << 4) + (msg.DATA[3] >> 4);
            CQ.TargetPdH0 = msg.DATA[3] & 0x7;
            CQ.TargetInvalidState = msg.DATA[4] >> 3;
            CQ.TargetAmbigState = msg.DATA[4] & 0x7;

            return CQ;
        }
        #endregion
    }
}

