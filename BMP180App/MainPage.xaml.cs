using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BMP180App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Bmp180Sensor _bmp180;
        private Timer _periodicTimer;

        public MainPage()
        {
            this.InitializeComponent();
            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;

            // Initialize the Sensors
            InitializeSensors();
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            /* Cleanup */
            _bmp180.Dispose();
        }

        private async void InitializeSensors()
        {
            // Initialize the BMP180 Sensor
            _bmp180 = new Bmp180Sensor();
            await _bmp180.InitializeAsync();
            var calibrationData = _bmp180.CalibrationData.ToString();

            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                calibrationDataTextBlock.Text = calibrationData;
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (_bmp180 == null)
                return;

            button.IsEnabled = false;

            /* Now that everything is initialized, create a timer so we read data every 1S */
            _periodicTimer = new Timer(this.TimerCallback, null, 0, 1000);
        }

        private async void TimerCallback(object state)
        {
            string temperatureText, pressureText;

            /* Read and format Sensor data */
            try
            {
                var sensorData = await _bmp180.GetSensorDataAsync(Bmp180AccuracyMode.UltraHighResolution);
                temperatureText = sensorData.Temperature.ToString("F1");
                pressureText = sensorData.Pressure.ToString("F2");
                temperatureText += "C - hex:" + BitConverter.ToString(sensorData.UncompestatedTemperature);
                pressureText += "hPa - hex:" + BitConverter.ToString(sensorData.UncompestatedPressure);
            }
            catch (Exception ex)
            {
                temperatureText = "Sensor Error: " + ex.Message;
                pressureText = "Sensor Error!" + ex.Message;
            }

            /* UI updates must be invoked on the UI thread */
            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                temperatureTextBlock.Text = temperatureText;
                pressureTextBlock.Text = pressureText;
            });
        }
    }
}
