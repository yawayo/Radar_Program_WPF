﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MsgFormat;
using Peak.Can.Basic;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Threading.Tasks;

namespace Radar_Program_WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
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
        private System.Windows.Point shift_pos;
        private double rect_size = 10;
        Setting setting_form;


        private DateTime Aframe_timestamp = DateTime.Now;
        private Msg_Format.Object_inf[] this_frame_data = new Msg_Format.Object_inf[100];
        private bool[] exist = new bool[100];




        #region 창 크기에 따른 컨트롤 크기 조절
        double orginalWidth, originalHeight;
        ScaleTransform scale = new ScaleTransform();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
           // change_btn_state(false);
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

        void MainWindow_Closing(object sender, EventArgs e)
        {
            Radar_status = false;

            if (Device_set.Radar_Disconnect())
            {
                if (ReadThread != null)
                {
                    ReadThread.Abort();
                    ReadThread.Join();
                    ReadThread = null;
                }
                if (DrawThread != null)
                {
                    DrawThread.Abort();
                    DrawThread.Join();
                    DrawThread = null;
                }
                /*if (Device_set.DB_Disconnect())
                {
                    BitmapImage theImage = new BitmapImage(new Uri("image/radar_off.png", UriKind.Relative));
                    ImageBrush imageBrush = new ImageBrush(theImage);
                    imageBrush.Stretch = Stretch.None;
                    Radar_btn.Background = imageBrush;
                    Radar_btn_label.Foreground = Brushes.White;
                }*/
            }
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

        #region btn
        void change_btn_state(bool b)
        {
            Radar_Connect_btn.IsEnabled = !b;
            Radar_Disconnect_btn.IsEnabled = b;
            Radar_Setting_btn.IsEnabled = b;
            Data_Save_Server_btn.IsEnabled = !b;
        }
        private void Radar_Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!Radar_status)
            {
                Radar_Connect();
            }
        }
        private void Radar_Disconnect_btn_Click(object sender, RoutedEventArgs e)
        {
            if (Radar_status)
            {
                Radar_Disconnect();
            }
        }

        private void Data_View_btn_Click(object sender, RoutedEventArgs e)
        {
            main.Cursor = Cursors.Wait;
            Data_View_btn.Cursor = Cursors.Wait;
            if (!Text_status)
                Text_on();
            else
                Text_off();
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

        private void Data_Save_Server_btn_Click(object sender, RoutedEventArgs e)
        {
            Device_set.Initialize_DB_Value(
                "127.0.0.1", "3306", "root", "0000",
                "183.99.41.239", "23306", "root", "hbrain0372!");
            Device_set.localDB_Connect();
            Device_set.serverDB_Connect();

            Device_set.update_Data_to_ServerDB();
        }
        #endregion

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
                /*Device_set.Initialize_DB_Value(
                    "127.0.0.1", "3306", "root", "0000",
                    "183.99.41.239", "23306", "root", "hbrain0372!");
                Device_set.localDB_Connect();*/

                Radar_status = true;
                read_Thread_Func();
                //draw_Thread_Func();

                change_btn_state(true);
            }
        }

        private void Radar_Disconnect()
        {
            if (Device_set.Radar_Disconnect())
            {
                Radar_status = false;

                if (ReadThread != null)
                {
                    ReadThread.Abort();
                    ReadThread.Join();
                    ReadThread = null;
                }
                if (DrawThread != null)
                {
                    DrawThread.Abort();
                    DrawThread.Join();
                    DrawThread = null;
                }
                change_btn_state(false);
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
                if(Device_set.Obj_inf[i].Count != 0)
                {
                    try
                    {
                        Msg_Format.Object_inf last_data = this_frame_data[i];

                        double X = ((-1 * last_data.DistLat) * (Data_Draw.ActualWidth / max_lat)) + (Data_Draw.ActualWidth / 2);
                        double Y = last_data.DistLong * (Data_Draw.ActualHeight / max_long);

                        Rectangle rect = new Rectangle
                        {
                            Stroke = new SolidColorBrush(Color.FromRgb(244, 143, 61)),
                            StrokeThickness = rect_size
                        };

                        Canvas.SetLeft(rect, X);
                        Canvas.SetTop(rect, Data_Draw.ActualHeight - Y);
                        this.Data_Draw.Children.Add(rect);


                        TextBox textBox = new TextBox();
                        textBox.Text = "";
                        textBox.VerticalAlignment = VerticalAlignment.Center;
                        textBox.Margin = new Thickness(10, 0, 0, 0);

                        textBox.Text = last_data.DistLong.ToString("0.0") + "\n";
                        textBox.Text += last_data.RCS.ToString("0.0");


                        Canvas.SetLeft(textBox, X + 10);
                        Canvas.SetTop(textBox, Data_Draw.ActualHeight - Y);
                        Data_Draw.Children.Add(textBox);
                    }
                    catch { }
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

                #region setting form set Radar state
                try
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                    {
                        setting_form.NVMReadStatus_value.Content = rs.NVMReadStatus.ToString();
                        setting_form.NVMWriteStatus_value.Content = rs.NVMwriteStatus.ToString();
                        setting_form.MaxDistance_value.Content = rs.MaxDistanceCfg.ToString();
                        setting_form.PersistentError_value.Content = rs.Persistent_Error.ToString();
                        setting_form.Interference_value.Content = rs.Interference.ToString();
                        setting_form.TemperatureError_value.Content = rs.Temperature_Error.ToString();
                        setting_form.TemporaryError_value.Content = rs.Temporary_Error.ToString();
                        setting_form.VoltageError_value.Content = rs.Voltage_Error.ToString();
                        setting_form.SensorID_value.Content = rs.SensorID.ToString();
                        setting_form.SortIndex_value.Content = rs.SortIndex.ToString();
                        setting_form.RadarPower_value.Content = rs.RadarPowerCfg.ToString();
                        setting_form.CtrlRelay_value.Content = rs.CtrlRelayCfg.ToString();
                        setting_form.OutputType_value.Content = rs.OutputTypeCfg.ToString();
                        setting_form.SendQuality_value.Content = rs.SendQualityCfg.ToString();
                        setting_form.SendExtInfo_value.Content = rs.SendExtInfoCfg.ToString();
                        setting_form.MotionRxState_value.Content = rs.MotionRxState.ToString();
                        setting_form.RCS_Threshold_value.Content = rs.RCS_Threshold.ToString();
                        setting_form.InvalidClusters_value.Content = rs.InvalidClusters.ToString();

                    }));
                }
                catch { }
                #endregion
            }
        }
        #endregion
        #region 204
        private void Process_FilterCfg(TPCANMsg Msg, DateTime Timestamp)
        {
            if (Setting_status)
            {
                Msg_Format.FilterState_Config fsc = Device_set.Msg_format.msg2FilterStateCfg(Msg);
                Brush brushcolor;
                try
                {
                    switch (fsc.Index)
                    {
                        case 0:

                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.NofObj.Foreground = brushcolor;
                                setting_form.NofObj_Min_value.Foreground = brushcolor;
                                setting_form.NofObj_Max_value.Foreground = brushcolor;
                                setting_form.NofObj_Min_value.Content = fsc.Min.ToString();
                                setting_form.NofObj_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 1:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.Distance.Foreground = brushcolor;
                                setting_form.Distance_Min_value.Foreground = brushcolor;
                                setting_form.Distance_Max_value.Foreground = brushcolor;
                                setting_form.Distance_Min_value.Content = fsc.Min.ToString();
                                setting_form.Distance_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 2:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.Azimuth.Foreground = brushcolor;
                                setting_form.Azimuth_Min_value.Foreground = brushcolor;
                                setting_form.Azimuth_Max_value.Foreground = brushcolor;
                                setting_form.Azimuth_Min_value.Content = fsc.Min.ToString();
                                setting_form.Azimuth_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 3:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VrelOncome.Foreground = brushcolor;
                                setting_form.VrelOncome_Min_value.Foreground = brushcolor;
                                setting_form.VrelOncome_Max_value.Foreground = brushcolor;
                                setting_form.VrelOncome_Min_value.Content = fsc.Min.ToString();
                                setting_form.VrelOncome_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 4:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VrelDepart.Foreground = brushcolor;
                                setting_form.VrelDepart_Min_value.Foreground = brushcolor;
                                setting_form.VrelDepart_Max_value.Foreground = brushcolor;
                                setting_form.VrelDepart_Min_value.Content = fsc.Min.ToString();
                                setting_form.VrelDepart_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 5:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.RCS.Foreground = brushcolor;
                                setting_form.RCS_Min_value.Foreground = brushcolor;
                                setting_form.RCS_Max_value.Foreground = brushcolor;
                                setting_form.RCS_Min_value.Content = fsc.Min.ToString();
                                setting_form.RCS_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 6:
                            //setting_form.Filter_Lifetime_MIN_value.Text = string.Format("{0:N1}", (float)min * 0.1);
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.Lifetime.Foreground = brushcolor;
                                setting_form.Lifetime_Min_value.Foreground = brushcolor;
                                setting_form.Lifetime_Max_value.Foreground = brushcolor;
                                setting_form.Lifetime_Min_value.Content = fsc.Min.ToString();
                                setting_form.Lifetime_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 7:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.Size.Foreground = brushcolor;
                                setting_form.Size_Min_value.Foreground = brushcolor;
                                setting_form.Size_Max_value.Foreground = brushcolor;
                                setting_form.Size_Min_value.Content = fsc.Min.ToString();
                                setting_form.Size_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 8:
                            string min_value = "", max_value = "";
                            switch (fsc.Min)
                            {
                                case 0: min_value = "0%"; break;
                                case 1: min_value = "25%"; break;
                                case 2: min_value = "50%"; break;
                                case 3: min_value = "75%"; break;
                                case 4: min_value = "90%"; break;
                                case 5: min_value = "99%"; break;
                                case 6: min_value = "99.9%"; break;
                                case 7: min_value = "100%"; break;
                                default: break;
                            }
                            switch (fsc.Max)
                            {
                                case 0: max_value = "0%"; break;
                                case 1: max_value = "25%"; break;
                                case 2: max_value = "50%"; break;
                                case 3: max_value = "75%"; break;
                                case 4: max_value = "90%"; break;
                                case 5: max_value = "99%"; break;
                                case 6: max_value = "99.9%"; break;
                                case 7: max_value = "100%"; break;
                                default: break;
                            }
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.ProbExists.Foreground = brushcolor;
                                setting_form.ProbExists_Min_value.Foreground = brushcolor;
                                setting_form.ProbExists_Max_value.Foreground = brushcolor;
                                setting_form.ProbExists_Min_value.Content = min_value;
                                setting_form.ProbExists_Max_value.Content = max_value;
                            }));
                            break;

                        case 9:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.Y.Foreground = brushcolor;
                                setting_form.Y_Min_value.Foreground = brushcolor;
                                setting_form.Y_Max_value.Foreground = brushcolor;
                                setting_form.Y_Min_value.Content = fsc.Min.ToString();
                                setting_form.Y_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 10:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.X.Foreground = brushcolor;
                                setting_form.X_Min_value.Foreground = brushcolor;
                                setting_form.X_Max_value.Foreground = brushcolor;
                                setting_form.X_Min_value.Content = fsc.Min.ToString();
                                setting_form.X_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 11:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VYRightLeft.Foreground = brushcolor;
                                setting_form.VYRightLeft_Min_value.Foreground = brushcolor;
                                setting_form.VYRightLeft_Max_value.Foreground = brushcolor;
                                setting_form.VYRightLeft_Min_value.Content = fsc.Min.ToString();
                                setting_form.VYRightLeft_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 12:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VXOncome.Foreground = brushcolor;
                                setting_form.VXOncome_Min_value.Foreground = brushcolor;
                                setting_form.VXOncome_Max_value.Foreground = brushcolor;
                                setting_form.VXOncome_Min_value.Content = fsc.Min.ToString();
                                setting_form.VXOncome_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 13:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VYLeftRight.Foreground = brushcolor;
                                setting_form.VYLeftRight_Min_value.Foreground = brushcolor;
                                setting_form.VYLeftRight_Max_value.Foreground = brushcolor;
                                setting_form.VYLeftRight_Min_value.Content = fsc.Min.ToString();
                                setting_form.VYLeftRight_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        case 14:
                            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                            {
                                if (fsc.Active == 1) brushcolor = new SolidColorBrush(Color.FromRgb(244, 143, 61));
                                else brushcolor = new SolidColorBrush(Colors.Black);
                                setting_form.VXDepart.Foreground = brushcolor;
                                setting_form.VXDepart_Min_value.Foreground = brushcolor;
                                setting_form.VXDepart_Max_value.Foreground = brushcolor;
                                setting_form.VXDepart_Min_value.Content = fsc.Min.ToString();
                                setting_form.VXDepart_Max_value.Content = fsc.Max.ToString();
                            }));
                            break;

                        default:
                            break;
                    }
                }
                catch { }
            }
        }
        #endregion
        #region 60A
        private void Process_Obj_Status(TPCANMsg Msg, DateTime Timestamp)
        {
            Aframe_timestamp = DateTime.Now;
            if (first_A)
            {
                save_this_frame_obj_data();
                if (Text_status)
                {
                    text_this_frame_obj_data();
                }
                draw_this_frame_obj_data();

                Clear_thisframe_data();
            }

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

                if ((og.ID >= 0) && (og.ID < 100))
                {
                    exist[og.ID] = true;

                    this_frame_data[og.ID].Timestamp = Aframe_timestamp;

                    this_frame_data[og.ID].ID = og.ID;
                    this_frame_data[og.ID].DistLong = og.DistLong;
                    this_frame_data[og.ID].DistLat = og.DistLat;
                    this_frame_data[og.ID].VrelLong = og.VrelLong;
                    this_frame_data[og.ID].VrelLat = og.VrelLat;
                    this_frame_data[og.ID].DynProp = og.DynProp;
                    this_frame_data[og.ID].RCS = og.RCS;
                    this_frame_data[og.ID].Distance = Math.Sqrt((this_frame_data[og.ID].DistLat * this_frame_data[og.ID].DistLat) + (this_frame_data[og.ID].DistLong * this_frame_data[og.ID].DistLong));
                    this_frame_data[og.ID].Speed = 3.6 * Math.Sqrt((this_frame_data[og.ID].VrelLat * this_frame_data[og.ID].VrelLat) + (this_frame_data[og.ID].VrelLong * this_frame_data[og.ID].VrelLong));
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

                if ((oq.ID >= 0) && (oq.ID < 100))
                {
                    if (exist[oq.ID])
                    {
                        this_frame_data[oq.ID].DistLat_rms = oq.DistLat_rms;
                        this_frame_data[oq.ID].DistLong_rms = oq.DistLong_rms;
                        this_frame_data[oq.ID].VrelLat_rms = oq.VrelLat_rms;
                        this_frame_data[oq.ID].VrelLong_rms = oq.VrelLong_rms;
                        this_frame_data[oq.ID].ArelLat_rms = oq.ArelLat_rms;
                        this_frame_data[oq.ID].ArelLong_rms = oq.ArelLong_rms;
                        this_frame_data[oq.ID].Orientation_rms = oq.Orientation_rms;
                        this_frame_data[oq.ID].MirrorProb = oq.MirrorProb;
                        this_frame_data[oq.ID].MeasState = oq.MeasState;
                        this_frame_data[oq.ID].ProbOfExist = oq.ProbOfExist;
                    }
                }
            }
        }
        #endregion
        #region 60D
        private void Process_Obj_extended(TPCANMsg Msg, DateTime Timestamp)
        {
            if (first_A)
            {
                Msg_Format.Object_Extended oe = Device_set.Msg_format.msg2ObjectExtended(Msg);

                if ((oe.ID >= 0) && (oe.ID < 100))
                {
                    if (exist[oe.ID])
                    {
                        this_frame_data[oe.ID].ArelLat = oe.ArelLat;
                        this_frame_data[oe.ID].ArelLong = oe.ArelLong;
                        this_frame_data[oe.ID].Class = oe.Class;
                        this_frame_data[oe.ID].OrientationAngle = oe.OrientationAngle;
                        this_frame_data[oe.ID].Length = oe.Length;
                        this_frame_data[oe.ID].Width = oe.Width;
                    }
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

        #region save obj info
        public void Clear_thisframe_data()
        {
            for (int i = 0; i < 100; i++)
                this_frame_data[i] = default(Msg_Format.Object_inf);
            System.Array.Clear(exist, 0, sizeof(bool) * 100);
        }
        private void draw_this_frame_obj_data()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ClearCanvas();
                DrawCube();
            }));
        }
        private void save_this_frame_obj_data()
        {
            bool exist_DB = false;
            string sql = "INSERT INTO " + Device_set.local_TABLENAME + " VALUES";
            
            for (int i = 0; i < 100; i++)
            {
                if (exist[i])
                {
                    Device_set.Obj_inf[i].AddLast(this_frame_data[i]);
                    if (Device_set.Obj_inf[i].Count >= 100)
                        Device_set.Obj_inf[i].RemoveFirst();

                    sql += "('" + this_frame_data[i].Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', " +
                                      this_frame_data[i].ID + ", " + this_frame_data[i].DistLong.ToString("F2") + ", " +
                                      this_frame_data[i].DistLat.ToString("F2") + ", " + this_frame_data[i].VrelLong + ", " +
                                      this_frame_data[i].VrelLat + ", " + this_frame_data[i].DynProp + ", " +
                                      this_frame_data[i].RCS + ", " + this_frame_data[i].DistLat_rms + ", " +
                                      this_frame_data[i].DistLong_rms + ", " + this_frame_data[i].VrelLat_rms + ", " +
                                      this_frame_data[i].VrelLong_rms + ", " + this_frame_data[i].ArelLat_rms + ", " +
                                      this_frame_data[i].ArelLong_rms + ", " + this_frame_data[i].Orientation_rms + ", " +
                                      this_frame_data[i].MirrorProb + ", " + this_frame_data[i].ProbOfExist + ", " +
                                      this_frame_data[i].MeasState + ", " + this_frame_data[i].ArelLong + ", " +
                                      this_frame_data[i].ArelLat + ", " + this_frame_data[i].Class + ", " +
                                      this_frame_data[i].OrientationAngle + ", " + this_frame_data[i].Length + ", " +
                                      this_frame_data[i].Width +
                                      "),";
                    exist_DB = true;
                }
                else
                {
                    if (Device_set.Obj_inf[i].Count != 0)
                    {
                        Msg_Format.Object_inf last_data = Device_set.Obj_inf[i].Last.Value;
                        TimeSpan difTime = DateTime.Now - last_data.Timestamp;
                        if ((difTime.Seconds > 0) || (difTime.Milliseconds > 300))
                            Device_set.Obj_inf[i].Clear();
                    }
                }
            }
            if(exist_DB)
            {
                sql = sql.TrimEnd(',');
                //Device_set.save_localDB(sql);
            }
        }
        #endregion

        #region update textbox
        private void Data_View_ckb_Click(object sender, RoutedEventArgs e)
        {
            main.Cursor = Cursors.Wait;
            Data_View_btn.Cursor = Cursors.Wait;
            if (!Text_status)
                Text_on();
            else
                Text_off();
            main.Cursor = Cursors.Arrow;
            Data_View_btn.Cursor = Cursors.Arrow;
        }
        public void text_this_frame_obj_data()
        {
            string str = "";
            for (int i = 0; i < 100; i++)
            {
                if (exist[i])
                {
                    str += "Object ID: " + this_frame_data[i].ID +
                            "  Speed: " + this_frame_data[i].Speed.ToString("F2") +
                            "  Distance: " + this_frame_data[i].Distance.ToString("F2") + "\n";
                }
            }
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
            /*check_201 = false; check_204 = false; check_700 = false;
            check_600 = false; check_701 = false; check_702 = false;
            check_60A = false; check_60B = false; check_60C = false; check_60D = false; check_60E = false;*/
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
                // dummy filter
                if (Radar_status)
                    write_FilterCfg(true);
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

        #region 카메라 연결 및 녹화
        private VideoCapture capture;
        private VideoWriter writer;
        private List<Mat> frameBuffer = new List<Mat>();
        private bool cameraStatus;

        private async void Camera_Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            cameraStatus = true;
            capture = new VideoCapture(0);
            capture.Set(VideoCaptureProperties.Fps, 15);
            writer = new VideoWriter("C:\\Users\\윤금채\\Desktop\\파일\\" + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".avi", FourCC.MJPG, 15, new OpenCvSharp.Size(640, 480));
            await PlayVideoAsync();
        }

        private async Task PlayVideoAsync()
        {
            await Task.Run(() =>
            {
                while (cameraStatus)
                {
                    Mat frame = capture.RetrieveMat();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            BitmapSource bitmapSource = BitmapSourceConverter.ToBitmapSource(frame);
                            Cam_1.Source = bitmapSource;
                            writer.Write(frame);
                        }
                        catch (Exception err)
                        {
                            frameBuffer.Add(frame.Clone());
                            Console.WriteLine(err.Message);
                        }
                        frameBuffer_listup();
                    });
                }
                frameBuffer_listup();
                capture.Release();
                writer.Release();
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    Cam_1.Source = null;
                }));
            });
        }

        private void frameBuffer_listup()
        {
            foreach (Mat frame in frameBuffer)
            {
                writer.Write(frame);
            }
            frameBuffer.Clear();
        }

        private void Camera_Disconnect_btn_Click(object sender, RoutedEventArgs e)
        {
            cameraStatus = false;
        }
        #endregion
    }
}
