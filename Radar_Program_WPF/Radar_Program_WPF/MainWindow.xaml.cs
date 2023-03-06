﻿using MsgFormat;
using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows.Shapes;


namespace Radar_Program_WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Radar_status = false;
        private readonly Radar_Set Device_set = new Radar_Set();
        private Thread ReadThread;
        private Thread DrawThread;
        private bool first_A = false;
        private bool Text_status = true;
        private bool Setting_status = false;
        private double max_lat = 20;
        private double max_long = 150;
        private Point shift_pos;
        private double rect_size = 10;
        Setting setting_form;

        #region 창 크기에 따른 컨트롤 크기 조절
        double orginalWidth, originalHeight;
        ScaleTransform scale = new ScaleTransform();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize(e.NewSize.Width, e.NewSize.Height);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            orginalWidth = this.Width;
            originalHeight = this.Height;

            if (this.WindowState == WindowState.Maximized)
            {
                ChangeSize(this.ActualWidth, this.ActualHeight);
            }

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
        }

        private void ChangeSize(double width, double height)
        {
            scale.ScaleX = width / orginalWidth;
            scale.ScaleY = height / originalHeight;

            FrameworkElement rootElement = this.Content as FrameworkElement;

            rootElement.LayoutTransform = scale;
        }
        #endregion

        #region check_id
        private bool check_201 = false;
        private bool check_204 = false;
        private bool check_700 = false;
        private bool check_600 = false;
        private bool check_701 = false;
        private bool check_702 = false;
        private bool check_60A = false;
        private bool check_60B = false;
        private bool check_60C = false;
        private bool check_60D = false;
        private bool check_60E = false;
        #endregion
        #region Msg_idf
        private string Msg_201;
        private string Msg_204;
        private string Msg_700;
        private string Msg_600;
        private string Msg_701;
        private string Msg_702;
        private string Msg_60A;
        private string Msg_60B;
        private string Msg_60C;
        private string Msg_60D;
        private string Msg_60E;
        #endregion

        private void Radar_Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!Radar_status)
            {
                Radar_Connect();
            }
        }
        private void Data_View_btn_Click(object sender, RoutedEventArgs e)
        {
            main.Cursor = Cursors.Wait;
            Data_View_btn.Cursor = Cursors.Wait;
            if (!Text_status)
            {
                Text_on();
            }
            else
            {
                Text_off();
            }
            main.Cursor = Cursors.Arrow;
            Data_View_btn.Cursor = Cursors.Arrow;
        }
        private void Radar_Setting_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!Setting_status)
            {
                setting_form_open();
            }
            else
            {
                setting_form_close();
            }
        }

        #region radar setting
        private void WriteMessage(TPCANMsg Msg)
        {
            TPCANStatus stsResult;
            stsResult = PCANBasic.Write(Device_set.Get_PCAN_Handle(), ref Msg);
            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                MessageBox.Show(stsResult.ToString());
        }
        private void write_RadarCfg()
        {
            Msg_Format.Radar_Config rc = new Msg_Format.Radar_Config();

            #region set data
            if (setting_form.MaxDistance_Valid)
            {
                rc.MaxDistance_valid = 1;
                int.TryParse(setting_form.MaxDistance_input.Text, out rc.MaxDistance);
                if (rc.MaxDistance > 2046) rc.MaxDistance = 2046;
                else if (rc.MaxDistance < 0) rc.MaxDistance = 0;
            }
            if (setting_form.SensorID_Valid)
            {
                rc.SensorID_valid = 1;
                rc.SensorID = setting_form.SensorID_input.SelectedIndex;
            }
            if (setting_form.OutputType_Valid)
            {
                rc.OutputType_valid = 1;
                rc.OutputType = setting_form.OutputType_input.SelectedIndex;
            }
            if (setting_form.SendQuality_Valid)
            {
                rc.SendQuality_valid = 1;
                rc.SendQuality = setting_form.SendQuality_input.SelectedIndex;
            }
            if (setting_form.SendExtInfo_Valid)
            {
                rc.SendExtInfo_valid = 1;
                rc.SendExtInfo = setting_form.SendExtInfo_input.SelectedIndex;
            }
            if (setting_form.SortIndex_Valid)
            {
                rc.SortIndex_valid = 1;
                rc.SortIndex = setting_form.SortIndex_input.SelectedIndex;
            }
            if (setting_form.StoreInNVM_Valid)
            {
                rc.StoreInNVM_valid = 1;
                rc.StoreInNVM = setting_form.StoreInNVM_input.SelectedIndex;
            }
            if (setting_form.CtrlRelay_Valid)
            {
                rc.CtrlRelay_valid = 1;
                rc.CtrlRelay = setting_form.CtrlRelay_input.SelectedIndex;
            }
            if (setting_form.RCSThreshold_Valid)
            {
                rc.RCS_Threshold_valid = 1;
                rc.RCS_Threshold = setting_form.RCS_Threshold_input.SelectedIndex;
            }
            if (setting_form.InvalidClusters_Valid)
            {
                rc.InvalidClusters_valid = 1;
                switch (setting_form.InvalidClusters_input.SelectedIndex)
                {
                    case 0: rc.InvalidClusters = 0x00; break;
                    case 1: rc.InvalidClusters = 0x01; break;
                    case 2: rc.InvalidClusters = 0x02; break;
                    case 3: rc.InvalidClusters = 0x04; break;
                    case 4: rc.InvalidClusters = 0x08; break;
                    case 5: rc.InvalidClusters = 0x10; break;
                    case 6: rc.InvalidClusters = 0x20; break;
                    case 7: rc.InvalidClusters = 0x40; break;
                }
            }
            #endregion

            TPCANMsg Msg = Device_set.Msg_format.RadarConfig2msg(rc, Device_set.Get_Radar_ID());
            WriteMessage(Msg);

        }
        private void write_FilterCfg(Object d)
        {
            bool dummy = Convert.ToBoolean(d);
            Msg_Format.Filter_Config fc = new Msg_Format.Filter_Config();

            for (int i = 0; i < 15; i++)
            {
                fc.Index = i;
                fc.Type = 1;

                #region set data
                if (!dummy)
                {
                    switch (i)
                    {
                        case 0:
                            fc.Valid = setting_form.NofObj_Valid ? 1 : 0;
                            fc.Active = setting_form.NofObj_Active ? 1 : 0;
                            double.TryParse(setting_form.NofObj_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.NofObj_MAX_input.Text, out fc.Max);
                            break;
                        case 1:
                            fc.Valid = setting_form.Distance_Valid ? 1 : 0;
                            fc.Active = setting_form.Distance_Active ? 1 : 0;
                            double.TryParse(setting_form.Distance_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.Distance_MAX_input.Text, out fc.Max);
                            break;
                        case 2:
                            fc.Valid = setting_form.Azimuth_Valid ? 1 : 0;
                            fc.Active = setting_form.Azimuth_Active ? 1 : 0;
                            double.TryParse(setting_form.Azimuth_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.Azimuth_MAX_input.Text, out fc.Max);
                            break;
                        case 3:
                            fc.Valid = setting_form.VrelOncome_Valid ? 1 : 0;
                            fc.Active = setting_form.VrelOncome_Active ? 1 : 0;
                            double.TryParse(setting_form.VrelOncome_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VrelOncome_MAX_input.Text, out fc.Max);
                            break;
                        case 4:
                            fc.Valid = setting_form.VrelDepart_Valid ? 1 : 0;
                            fc.Active = setting_form.VrelDepart_Active ? 1 : 0;
                            double.TryParse(setting_form.VrelDepart_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VrelDepart_MAX_input.Text, out fc.Max);
                            break;
                        case 5:
                            fc.Valid = setting_form.RCS_Valid ? 1 : 0;
                            fc.Active = setting_form.RCS_Active ? 1 : 0;
                            double.TryParse(setting_form.RCS_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.RCS_MAX_input.Text, out fc.Max);
                            break;
                        case 6:
                            fc.Valid = setting_form.Lifetime_Valid ? 1 : 0;
                            fc.Active = setting_form.Lifetime_Active ? 1 : 0;
                            double.TryParse(setting_form.Lifetime_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.Lifetime_MAX_input.Text, out fc.Max);
                            break;
                        case 7:
                            fc.Valid = setting_form.Size_Valid ? 1 : 0;
                            fc.Active = setting_form.Size_Active ? 1 : 0;
                            double.TryParse(setting_form.Size_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.Size_MAX_input.Text, out fc.Max);
                            break;
                        case 8:
                            fc.Valid = setting_form.ProbExists_Valid ? 1 : 0;
                            fc.Active = setting_form.ProbExists_Active ? 1 : 0;
                            fc.Min = setting_form.ProbExists_MIN_input.SelectedIndex;
                            fc.Max = setting_form.ProbExists_MAX_input.SelectedIndex;
                            break;
                        case 9:
                            fc.Valid = setting_form.Y_Valid ? 1 : 0;
                            fc.Active = setting_form.Y_Active ? 1 : 0;
                            double.TryParse(setting_form.Y_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.Y_MAX_input.Text, out fc.Max);
                            break;
                        case 10:
                            fc.Valid = setting_form.X_Valid ? 1 : 0;
                            fc.Active = setting_form.X_Active ? 1 : 0;
                            double.TryParse(setting_form.X_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.X_MAX_input.Text, out fc.Max);
                            break;
                        case 11:
                            fc.Valid = setting_form.VYRightLeft_Valid ? 1 : 0;
                            fc.Active = setting_form.VYRightLeft_Active ? 1 : 0;
                            double.TryParse(setting_form.VYRightLeft_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VYRightLeft_MAX_input.Text, out fc.Max);
                            break;
                        case 12:
                            fc.Valid = setting_form.VXOncome_Valid ? 1 : 0;
                            fc.Active = setting_form.VXOncome_Active ? 1 : 0;
                            double.TryParse(setting_form.VXOncome_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VXOncome_MAX_input.Text, out fc.Max);
                            break;
                        case 13:
                            fc.Valid = setting_form.VYLeftRight_Valid ? 1 : 0;
                            fc.Active = setting_form.VYLeftRight_Active ? 1 : 0;
                            double.TryParse(setting_form.VYLeftRight_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VYLeftRight_MAX_input.Text, out fc.Max);
                            break;
                        case 14:
                            fc.Valid = setting_form.VXDepart_Valid ? 1 : 0;
                            fc.Active = setting_form.VXDepart_Active ? 1 : 0;
                            double.TryParse(setting_form.VXDepart_MIN_input.Text, out fc.Min);
                            double.TryParse(setting_form.VXDepart_MAX_input.Text, out fc.Max);
                            break;
                        default:
                            break;
                    }
                }
                #endregion
                TPCANMsg Msg = Device_set.Msg_format.FilterConfig2msg(fc, Device_set.Get_Radar_ID());
                WriteMessage(Msg);
                //MessageBox.Show(Msg.DATA.ToString());
                Thread.Sleep(30);
            }
        }
        #endregion
        #region radar connect and TextBox update

        private void Radar_Connect()
        {
            Device_set.Initialize_Radar_Value(id: 0);

            if (Device_set.Radar_Connect())
            {
                Device_set.Initialize_DB_Value(); //20230211
                Device_set.DB_Connect(); //20230211

                Radar_status = true;
                read_Thread_Func();
                draw_Thread_Func();
            }
        }

        private void read_Thread_Func()
        {
            ReadThread = new Thread(new ThreadStart(ReadMessages));
            ReadThread.IsBackground = true;
            ReadThread.Start();
        }
        private void draw_Thread_Func()
        {
            DrawThread = new Thread(DrawLoop);
            DrawThread.IsBackground = true;
            DrawThread.Start();
        }
        private void DrawLoop()
        {
            double maxFPS = 60;
            double minFramePeriodMsec = 1000.0 / maxFPS;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ClearCanvas();
                    DrawCube();
                }));
                double msToWait = minFramePeriodMsec - stopwatch.ElapsedMilliseconds;
                if (msToWait > 0)
                    Thread.Sleep((int)msToWait);
                stopwatch.Restart();

            } while (Radar_status);
        }
        private void ClearCanvas()
        {
            Data_Draw.Children.Clear();
        }
        private void DrawCube()
        {
            for (int i = 0; i < 100; i++)
            {
                if (Device_set.Get_exist_num(i))
                {
                    if (Device_set.Obj_inf[i].Last.Value.Drawed)
                    {
                        Msg_Format.Object_inf last_data = Device_set.Obj_inf[i].Last.Value;
                        Device_set.Obj_inf[i].RemoveLast();
                        
                        double X = ((-1 * last_data.DistLat) * (Data_Draw.ActualWidth / max_lat)) + (Data_Draw.ActualWidth / 2);
                        double Y = last_data.DistLong * (Data_Draw.ActualHeight / max_long);
                        TimeSpan difTime = DateTime.Now - last_data.Timestamp;
                        if ((difTime.Seconds > 0) || (difTime.Milliseconds > 300))
                            last_data.Drawed = false;
                        Device_set.Obj_inf[i].AddLast(last_data);

                        Rectangle rect = new Rectangle
                        {
                            Stroke = new SolidColorBrush(Color.FromRgb(244, 143, 61)),
                            StrokeThickness = rect_size
                        };

                        Canvas.SetLeft(rect, X);
                        Canvas.SetTop(rect, Y);
                        this.Data_Draw.Children.Add(rect);
                    }
                }
            }
        }

        private void ReadMessages()
        {
            do
            {
                ReadMessage();
            }
            while (Radar_status);
        }

        private void ReadMessage()
        {
            TPCANMsg CANMsg;
            TPCANTimestamp CANTimeStamp;
            TPCANStatus stsResult;

            stsResult = PCANBasic.Read(Device_set.Get_PCAN_Handle(), out CANMsg, out CANTimeStamp);
            DateTime timestamp = DateTime.Now;
            if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                ProcessMessage(CANMsg, timestamp);

            return;
        }

        private void ProcessMessage(TPCANMsg Msg, DateTime Timestamp)
        {
            uint Msg_ID = Msg.ID & 0xF0F;

            //선배님 방법으로 현재 ID 값 초기화
            //int sensor_id = ((int)Msg.ID & 0xF0) >> 4;
            //Device_set.Set_Radar_ID(sensor_id);

            switch (Msg_ID)
            {
                case 0x201:
                    Process_RadarCfg(Msg, Timestamp);
                    break;
                case 0x204:
                    Process_FilterCfg(Msg, Timestamp);
                    break;
                case 0x600:
                    Process_Cluster_Status(Msg, Timestamp);
                    break;
                case 0x701:
                    Process_Cluster_General(Msg, Timestamp);
                    break;
                case 0x702:
                    Process_Cluster_Quality(Msg, Timestamp);
                    break;
                case 0x60A:
                    Process_Obj_Status(Msg, Timestamp);
                    break;
                case 0x60B:
                    Process_Obj_General(Msg, Timestamp);
                    break;
                case 0x60C:
                    Process_Obj_Quality(Msg, Timestamp);
                    break;
                case 0x60D:
                    Process_Obj_extended(Msg, Timestamp);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 201
        private void Process_RadarCfg(TPCANMsg Msg, DateTime Timestamp)
        {
            if (Setting_status)
            {
                Msg_Format.Radar_state rs = Device_set.Msg_format.msg2RadarState(Msg);
                Device_set.Set_Radar_ID(rs.SensorID);
            }
        }
        #endregion
        #region 204
        private void Process_FilterCfg(TPCANMsg Msg, DateTime Timestamp)
        {
            if (Setting_status)
            {
                Msg_Format.FilterState_Config fsc = Device_set.Msg_format.msg2FilterStateCfg(Msg);
            }
        }
        #endregion
        #region 60A
        private void Process_Obj_Status(TPCANMsg Msg, DateTime Timestamp)
        {
            Msg_Format.Object_Status os = Device_set.Msg_format.msg2ObjectStatus(Msg);
            first_A = true;
        }
        #endregion
        #region 60B
        private void Process_Obj_General(TPCANMsg Msg, DateTime Timestamp)
        {
            if (first_A)
            {
                Msg_Format.Object_General og = Device_set.Msg_format.msg2ObjectGeneral(Msg);
                Msg_Format.Object_inf now_data = new Msg_Format.Object_inf();

                now_data.ID = og.ID;
                now_data.DistLong = og.DistLong;
                now_data.DistLat = og.DistLat;
                now_data.VrelLong = og.VrelLong;
                now_data.VrelLat = og.VrelLat;
                now_data.DynProp = og.DynProp;
                now_data.RCS = og.RCS;
                now_data.Distance = Math.Sqrt((now_data.DistLat * now_data.DistLat) + (now_data.DistLong * now_data.DistLong));
                now_data.Speed = 3.6 * Math.Sqrt((now_data.VrelLat * now_data.VrelLat) + (now_data.VrelLong * now_data.VrelLong));
                now_data.Timestamp = Timestamp;
                now_data.Drawed = true;
                now_data.Writed = true;

                if (!Device_set.Get_exist_num(now_data.ID))
                {
                    Device_set.Obj_inf[now_data.ID] = new LinkedList<Msg_Format.Object_inf>();
                    Device_set.Obj_inf[now_data.ID].AddLast(now_data);
                    Device_set.Set_exist_num(now_data.ID, true);
                }
                else
                {
                    Device_set.Obj_inf[now_data.ID].AddLast(now_data);
                }
                if (Device_set.Obj_inf[now_data.ID].Count >= 100)
                {
                    Device_set.Obj_inf[now_data.ID].RemoveFirst();
                }
            }
        }
        #endregion
        #region 60C
        private void Process_Obj_Quality(TPCANMsg Msg, DateTime Timestamp)
        {
            if (first_A)
            {
                Msg_Format.Object_Quality oq = Device_set.Msg_format.msg2ObjectQuality(Msg);
                Msg_Format.Object_inf now_data = Device_set.Obj_inf[oq.ID].Last.Value;
                Device_set.Obj_inf[oq.ID].RemoveLast();

                // Quality
                now_data.DistLat_rms = oq.DistLat_rms;
                now_data.DistLong_rms = oq.DistLong_rms;
                now_data.VrelLat_rms = oq.VrelLat_rms;
                now_data.VrelLong_rms = oq.VrelLong_rms;
                now_data.ArelLat_rms = oq.ArelLat_rms;
                now_data.ArelLong_rms = oq.ArelLong_rms;
                now_data.Orientation_rms = oq.Orientation_rms;
                now_data.MirrorProb = oq.MirrorProb;
                now_data.MeasState = oq.MeasState;
                now_data.ProbOfExist = oq.ProbOfExist;

                Device_set.Obj_inf[oq.ID].AddLast(now_data);
            }
        }
        #endregion
        #region 60D
        private void Process_Obj_extended(TPCANMsg Msg, DateTime Timestamp)
        {
            if (first_A)
            {
                Msg_Format.Object_Extended oe = Device_set.Msg_format.msg2ObjectExtended(Msg);
                Msg_Format.Object_inf now_data = Device_set.Obj_inf[oe.ID].Last.Value;
                Device_set.Obj_inf[oe.ID].RemoveLast();

                now_data.ArelLat = oe.ArelLat;
                now_data.ArelLong = oe.ArelLong;
                now_data.Class = oe.Class;
                now_data.OrientationAngle = oe.OrientationAngle;
                now_data.Length = oe.Length;
                now_data.Width = oe.Width;

                Device_set.Obj_inf[oe.ID].AddLast(now_data);
                Device_set.Data_preprocess(Msg, Timestamp);
                if (Text_status)
                {
                    update_Textbox_msg(Msg, Timestamp);
                }
            }
        }
        #endregion
        #region Cluster Msg
        private void Process_Cluster_Status(TPCANMsg Msg, DateTime Timestamp)
        {

        }
        private void Process_Cluster_General(TPCANMsg Msg, DateTime Timestamp)
        {

        }
        private void Process_Cluster_Quality(TPCANMsg Msg, DateTime Timestamp)
        {

        }
        #endregion

        #region update textbox
        public void update_Textbox_msg(TPCANMsg Msg, DateTime Timestamp)
        {
            Msg_Format.Object_Extended oe = Device_set.Msg_format.msg2ObjectExtended(Msg);

            string str = "Object ID: " + Device_set.Obj_inf[oe.ID].First.Value.ID +
                         "  Speed: " + Device_set.Obj_inf[oe.ID].First.Value.Speed.ToString("F2") +
                         "  Distance: " + Device_set.Obj_inf[oe.ID].First.Value.Distance.ToString("F2");
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                TextBox_data.Text = str;
            }));
        }
        #endregion

        #region text on & off
        private void Text_on()
        {
            Text_status = true;
            check_201 = false; check_204 = false; check_700 = false;
            check_600 = false; check_701 = false; check_702 = false;
            check_60A = false; check_60B = false; check_60C = false; check_60D = false; check_60E = false;
            TextBox_data.Text = "";
        }
        private void Text_off()
        {
            Text_status = false;
            TextBox_data.Text = "";
        }
        #endregion

        #region Setting_form
        private void setting_form_open()
        {
            Setting_status = true;
            if (setting_form == null)
            {
                setting_form = new Setting();
                setting_form.Write_Radar += new Setting.Write_Radar_Handler(write_RadarCfg);
                setting_form.Write_Filter += new Setting.Write_Filter_Hendler(write_FilterCfg);
                setting_form.Closed += setting_form_closed;
                setting_form.Show();
            }
        }
        private void setting_form_closed(object sender, EventArgs e)
        {
            setting_form_close();
        }
        private void setting_form_close()
        {
            Setting_status = false;
            if (setting_form != null)
            {
                setting_form.Close();
                setting_form = null;
            }
        }
        #endregion
    }
}