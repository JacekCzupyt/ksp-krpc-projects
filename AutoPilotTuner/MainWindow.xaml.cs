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
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Schema.KRPC;

namespace AutoPilotTuner {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Conn = new Connection();
            Vessel = Conn.SpaceCenter().ActiveVessel;
            Vessel.AutoPilot.AutoTune = false;
            Vessel.AutoPilot.AttenuationAngle = new(0.1, 0.1, 0.1);
            Vessel.AutoPilot.Engage();

            TimeToPeakSlider.Value = Vessel.AutoPilot.TimeToPeak.Item1;
            DecelerationTimeSlider.Value = decelerationTimeMultiplier;
            StoppingTimeSlider.Value = stoppingTimeMultiplier;
            
            SetValues();
        }

        public Vessel Vessel { get; set; }

        public Connection Conn { get; set; }

        private double decelerationTimeMultiplier = 1, stoppingTimeMultiplier = 300;

        private double angle = 0;
        private void TimeToPeakSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Vessel.AutoPilot.TimeToPeak = new(e.NewValue, e.NewValue, e.NewValue);
        }
        private void DecelerationTimeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            decelerationTimeMultiplier = e.NewValue;
        }
        private void StoppingTimeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            stoppingTimeMultiplier = e.NewValue;
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            Vessel.AutoPilot.TargetDirection = new(0, Math.Sin(angle), Math.Cos(angle));
            angle += Math.PI / 2;
        }

        private async Task SetValues() {
            while (true) {
                var err = Vessel.AutoPilot.Error;
                var stoppingTime = err / stoppingTimeMultiplier;
                Vessel.AutoPilot.StoppingTime = new(stoppingTime, stoppingTime, stoppingTime);
                var decelerationTime = decelerationTimeMultiplier / err;
                Vessel.AutoPilot.DecelerationTime = new(decelerationTime, decelerationTime, decelerationTime);
                ErrorValue.Text = err.ToString();
                await Task.Delay(10);
            }
        }
    }
}
