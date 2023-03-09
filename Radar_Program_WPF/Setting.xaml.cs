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

namespace Radar_Program_WPF
{
    /// <summary>
    /// Setting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Setting : Window
    {
        #region 창 크기에 따른 컨트롤 크기 조절
        double orginalWidth, originalHeight;
        ScaleTransform scale = new ScaleTransform();
        public Setting()
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

        #region bool Radar Valid 
        public bool MaxDistance_Valid = false;
        public bool SensorID_Valid = false;
        public bool OutputType_Valid = false;
        public bool RadarPower_Valid = false;
        public bool CtrlRelay_Valid = false;
        public bool SendQuality_Valid = false;
        public bool SendExtInfo_Valid = false;
        public bool SortIndex_Valid = false;
        public bool StoreInNVM_Valid = false;
        public bool RCSThreshold_Valid = false;
        public bool InvalidClusters_Valid = false;
        #endregion
        #region bool Filter Valid
        public bool NofObj_Valid = false;
        public bool Distance_Valid = false;
        public bool Azimuth_Valid = false;
        public bool VrelOncome_Valid = false;
        public bool VrelDepart_Valid = false;
        public bool RCS_Valid = false;
        public bool Lifetime_Valid = false;
        public bool ProbExists_Valid = false;
        public bool Y_Valid = false;
        public bool X_Valid = false;
        public bool Size_Valid = false;
        public bool VYRightLeft_Valid = false;
        public bool VXOncome_Valid = false;
        public bool VYLeftRight_Valid = false;
        public bool VXDepart_Valid = false;
        #endregion
        #region bool Filter Active
        public bool NofObj_Active = false;
        public bool Distance_Active = false;
        public bool Azimuth_Active = false;
        public bool VrelOncome_Active = false;
        public bool VrelDepart_Active = false;
        public bool RCS_Active = false;
        public bool Lifetime_Active = false;
        public bool ProbExists_Active = false;
        public bool Y_Active = false;
        public bool X_Active = false;
        public bool Size_Active = false;
        public bool VYRightLeft_Active = false;
        public bool VXOncome_Active = false;
        public bool VYLeftRight_Active = false;
        public bool VXDepart_Active = false;
        #endregion


        public delegate void Write_Radar_Handler();
        public delegate void Write_Filter_Hendler(Object dummy);
        public event Write_Radar_Handler Write_Radar;
        public event Write_Filter_Hendler Write_Filter;

        private void Filter_Write_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Write_Filter(false);
        }
        private void Radar_Write_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Write_Radar();
        }
        #region radar checkbox
        private void MaxDistance_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            MaxDistance_Valid = !MaxDistance_Valid;
        }
        private void SensorID_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            SensorID_Valid = !SensorID_Valid;
        }

        private void OutputType_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            OutputType_Valid = !OutputType_Valid;
        }

        private void RadarPower_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            RadarPower_Valid = !RadarPower_Valid;
        }

        private void CtrlRelay_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            CtrlRelay_Valid = !CtrlRelay_Valid;
        }

        private void SendQuality_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            SendQuality_Valid = !SendQuality_Valid;
        }

        private void SendExtInfo_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            SendExtInfo_Valid = !SendExtInfo_Valid;
        }

        private void SortIndex_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            SortIndex_Valid = !SortIndex_Valid;
        }

        private void StoreInNVM_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            StoreInNVM_Valid = !StoreInNVM_Valid;
        }

        private void RCSThreshold_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            RCSThreshold_Valid = !RCSThreshold_Valid;
        }

        private void InvalidClusters_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            InvalidClusters_Valid = !InvalidClusters_Valid;
        }
        #endregion
        #region fillter checkbox
        private void NofObj_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            NofObj_Valid = !NofObj_Valid;
        }
        private void NofObj_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            NofObj_Active = !NofObj_Active;
        }
        private void Distance_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Distance_Valid = !Distance_Valid;
        }
        private void Distance_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Distance_Active = !Distance_Active;
        }
        private void Azimuth_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Azimuth_Valid = !Azimuth_Valid;
        }
        private void Azimuth_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Azimuth_Active = !Azimuth_Active;
        }

        private void VrelOncome_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VrelOncome_Valid = !VrelOncome_Valid;
        }
        private void VrelOncome_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VrelOncome_Active = !VrelOncome_Active;
        }
        private void VrelDepart_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VrelDepart_Valid = !VrelDepart_Valid;
        }
        private void VrelDepart_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VrelDepart_Active = !VrelDepart_Active;

        }
        private void RCS_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            RCS_Valid = !RCS_Valid;
        }
        private void RCS_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            RCS_Active = !RCS_Active;
        }
        private void Lifetime_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Lifetime_Valid = !Lifetime_Valid;
        }
        private void Lifetime_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Lifetime_Active = !Lifetime_Active;
        }
        private void Size_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Size_Valid = !Size_Valid;
        }
        private void Size_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Size_Active = !Size_Active;
        }
        private void ProbExists_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            ProbExists_Valid = !ProbExists_Valid;
        }
        private void ProbExists_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            ProbExists_Active = !ProbExists_Active;
        }
        private void Y_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Y_Valid = !Y_Valid;
        }
        private void Y_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            Y_Active = !Y_Active;
        }
        private void X_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            X_Valid = !X_Valid;
        }
        private void X_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            X_Active = !X_Active;
        }

        private void VYRightLeft_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VYRightLeft_Valid = !VYRightLeft_Valid;
        }
        private void VYRightLeft_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VYLeftRight_Active = !VYLeftRight_Active;
        }
        private void VXOncome_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        { 
            VXOncome_Valid = !VXOncome_Valid;
        }
        private void VXOncome_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VXOncome_Active = !VXOncome_Active;
        }
        private void VYLeftRight_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VYLeftRight_Valid = !VYLeftRight_Valid;
        }
        private void VYLeftRight_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VYLeftRight_Active = !VYLeftRight_Active;
        }
        private void VXDepart_Valid_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VXDepart_Valid = !VXDepart_Valid;
        }
        private void VXDepart_Active_ckb_Checked(object sender, RoutedEventArgs e)
        {
            VXDepart_Active = !VXDepart_Active;
        }
        #endregion


    }
}
