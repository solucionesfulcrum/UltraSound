using CLRDemoServer;
using DemoUltrasound.Setting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Accord.Video.FFMPEG;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using DemoUltrasound;
using DemoUltrasound.SignalProcessing;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace DemoUltrasound
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    
       

        private CLR_DemoServer _demoServer = null;
        private ImageData imageData = new ImageData(); //刷新数据缓存
        private ExamModeConfig _examModeConfigSetting = ExamModeConfig.ExamModeConfigSetting; //检查模式参数配置类
        
        private string currentCheckModeParamPath = "";


        private BitmapImage[] _bitmapImageSource = new BitmapImage[21];

        private Thread _ReadDataThread = null;
        private bool _readDataThreadFlag = false;

        private Thread _ReadHistoryDataThread = null;
        private bool _readHistoryDataThreadFlag = false;

        private HwndSource hwndSource;
        private HwndSourceHook hook;
        private CyUSBMonitor cyUSBMonitor = new CyUSBMonitor();

        private CLScanMode.CLScanModeEnum mScanMode = CLScanMode.CLScanModeEnum.B;
        private CLScanMode.CLScanModeEnum mLastScanMode = CLScanMode.CLScanModeEnum.B;
        public event HardWareMsgHandler HardWareMsgEvent;
        private event HardWareMsgSHandler HardWareMsgSEvent;
        private System.Threading.Timer _getHWVerTimer;

        private double currentDepth = 0;

        private System.Threading.Timer _setTextTimer;

        public static System.Collections.Hashtable ProbeIDAndName = new System.Collections.Hashtable();
        private static int[] D_PrfRateArray = new int[8];
        private int currentVID = 0;

        private VideoFileWriter _videoWriter;
        private bool _isRecording;
        private string _outputFile;
        private Thread _recordingThread;
        private System.Drawing.Rectangle _captureArea;

        private bool isDragging = false;
        private System.Windows.Point clickPosition;
        
        public MainWindow()
        {
            _demoServer = CLR_DemoServer.GetInstance(); //后台demoServer获取demo单例

            InitializeComponent();
            //TODO-CWT
            overlayImage.Visibility = Visibility.Collapsed;
            // configuracion 
            //ConfigurarListener();
            SelectionRectangle.MouseDown += SelectionRectangle_MouseDown;
            SelectionRectangle.MouseMove += SelectionRectangle_MouseMove;
            SelectionRectangle.MouseUp += SelectionRectangle_MouseUp;
            PositionThumbs();
            _captureArea = new System.Drawing.Rectangle(100, 100, 800, 600);
            this.DataContext = this;
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.ContentRendered += MainWindow_ContentRendered;
            //设置参数路经
            _examModeConfigSetting.Load("../CIES_SDK/Param");

            _bitmapImageSource = new BitmapImage[2];
            _bitmapImageSource[0] = new BitmapImage(new Uri("pack://application:,,,/Images/usb_connected.png"));
            _bitmapImageSource[1] = new BitmapImage(new Uri("pack://application:,,,/Images/usb_disconnected.png"));

            UsbConn = false;


            //彩超USB 8通道
            ProbeIDAndName.Add(1035, "UC5-2S_8");
            ProbeIDAndName.Add(1007, "UL10-5E_8");

            //彩超USB 8兽用通道
            ProbeIDAndName.Add(1110, "UC5-2ST_8");
            ProbeIDAndName.Add(1100, "UL8-4T_8");

            //彩超USB 16通道
            ProbeIDAndName.Add(1406, "UC5-2E_16");
            ProbeIDAndName.Add(1407, "UL10-5E_16");
            ProbeIDAndName.Add(1410, "UC7-3E_16");
            ProbeIDAndName.Add(1413, "UEV9-4_16");
            ProbeIDAndName.Add(1434, "UL10-5ES_16");

            //彩超USB 16兽用通道
            ProbeIDAndName.Add(1127, "UL8-4E_16");
            ProbeIDAndName.Add(1128, "UC5-2ET_16");
            ProbeIDAndName.Add(1129, "UL8-4TE_16");

            //彩超USB 32通道
            ProbeIDAndName.Add(1514, "UC5-2A_32");
            ProbeIDAndName.Add(1502, "UL10-5_32");
            ProbeIDAndName.Add(1507, "UL10-5E_32");
            ProbeIDAndName.Add(1510, "UC7-3E_32");

            //彩超大板卡ID
           ProbeIDAndName.Add(1702, "L10-5");
           ProbeIDAndName.Add(1704, "C5-2");
           ProbeIDAndName.Add(1705, "C6-2");
           ProbeIDAndName.Add(1708, "P4-2");
           ProbeIDAndName.Add(1712, "EV9-4");
           ProbeIDAndName.Add(1720, "L12-5");
           ProbeIDAndName.Add(1723, "C7-3");
           ProbeIDAndName.Add(1724, "C5-2A");
           ProbeIDAndName.Add(1725, "C5-2SC");
           ProbeIDAndName.Add(1726, "L10-5SC");
           ProbeIDAndName.Add(1727, "L12-5SC");
           ProbeIDAndName.Add(1728, "L8-4");
           ProbeIDAndName.Add(1729, "P4-2JR");
           ProbeIDAndName.Add(1730, "L12-5A");
           ProbeIDAndName.Add(1731, "L14-6");

            D_PrfRateArray[0] = 1;
            D_PrfRateArray[1] = 2;
            D_PrfRateArray[2] = 3;
            D_PrfRateArray[3] = 4;
            D_PrfRateArray[4] = 6;
            D_PrfRateArray[5] = 8;
            D_PrfRateArray[6] = 12;
            D_PrfRateArray[7] = 16;

            HardWareMsgEvent += MainWindow_HardWareMsgEvent;
            HardWareMsgSEvent += MainWindow_HardWareMsgSEvent;
            _demoServer.SetDelegateMthod(HardWareMsgEvent);
            _demoServer.SetDelegateMthod(HardWareMsgSEvent);

            _getHWVerTimer = new Timer(GetHWVersion, this, Timeout.Infinite, Timeout.Infinite);
            _setTextTimer = new Timer(SetTextTimer, this, Timeout.Infinite, Timeout.Infinite);
        }

        //Simular data en cruda generada
        /*private void GenerarDatosSimulados()
        {
           /* // Dimensiones de la imagen simulada (128x128 píxeles)
            int width = 128;
            int height = 128;
    
            // Generar datos en crudo simulados
            byte[,] datosEnCrudo = new byte[height, width];
            Random random = new Random();

            // Llenar la matriz con valores aleatorios (0-255)
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    datosEnCrudo[i, j] = (byte)random.Next(0, 256);
                }
            }

            // Guardar los datos en un archivo binario
            string filePath = "datosEnCrudoSimulados.bin"; // Puedes ajustar la ruta según sea necesario
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            writer.Write(datosEnCrudo[i, j]);
                        }
                    }
                }
            }

            MessageBox.Show(string.Format("Datos en crudo simulados guardados en",filePath));*/
           // MessageBox.Show(string.Format("Datos en crudo simulados guardados en"));
        //}

        // -----------------------------------------------------------------------
        // MÉTODO AUXILIAR #1: convierte imageData.m_B_Imagedata (float[]) a float[,]
        // -----------------------------------------------------------------------
        
        
        //TODO-CWT SIMULACION DE DATA PHANTOM
        
        /// <summary>
        /// Genera un phantom simple en escala de grises (float[,] entre 0 y 255).
        /// width  = número de columnas (eje X)
        /// height = número de filas   (eje Y)
        /// </summary>
        
        private float[,] GenerarPhantom(int width, int height)
        {
        //TODO-PHATOM HOMOGENEO B MODE ESCALA DE GRISES
            /*var rnd = new Random();
            float[,] phantom = new float[height, width];

            // 1) Llenamos de “speckle” (ruido uniforme entre 30 y 80)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Ruido base
                    phantom[y, x] = 30f + (float)(rnd.NextDouble() * 50.0);
                }
            }

            // 2) Dibujamos un círculo de intensidad mayor en el centro
            int centroX = width / 2;
            int centroY = height / 2;
            int radio = Math.Min(width, height) / 4;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = x - centroX;
                    int dy = y - centroY;
                    if (dx * dx + dy * dy <= radio * radio)
                    {
                        // Intensidad “eco fuerte” dentro del círculo
                        phantom[y, x] = 200f + (float)(rnd.NextDouble() * 50.0);
                    }
                }
            }

            return phantom;*/
            
            var rnd = new Random();
            float[,] powerMap = new float[height, width];

            // Parámetros del disco central
            int centroX = width  / 2;
            int centroY = height / 2;
            int radio   = Math.Min(width, height) / 4;

            // 1) Fondo: speckle de baja amplitud → baja potencia
            //    I,Q ∈ [0.1 .. 0.2]
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float i = 0.1f + (float)rnd.NextDouble() * 0.1f;
                    float q = 0.1f + (float)rnd.NextDouble() * 0.1f;
                    powerMap[y, x] = i * i + q * q;
                }
            }

            // 2) Disco central: speckle de alta amplitud → mayor potencia
            //    I,Q ∈ [0.5 .. 1.0]
            for (int y = 0; y < height; y++)
            {
                int dy = y - centroY;
                for (int x = 0; x < width; x++)
                {
                    int dx = x - centroX;
                    if (dx * dx + dy * dy <= radio * radio)
                    {
                        float i = 0.5f + (float)rnd.NextDouble() * 0.5f;
                        float q = 0.5f + (float)rnd.NextDouble() * 0.5f;
                        powerMap[y, x] = i * i + q * q;
                    }
                }
            }

            // 3) (Opcional) Normalizar a [0..1]
            float minP = float.MaxValue, maxP = float.MinValue;
            foreach (var p in powerMap)
            {
                if (p < minP) minP = p;
                if (p > maxP) maxP = p;
            }
            float range = maxP - minP;
            if (range < 1e-6f) range = 1f;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                powerMap[y, x] = (powerMap[y, x] - minP) / range;

            return powerMap;
        }
        
        //TODO-CWT
        
        private float[,] ConvertBModeToFloat2D()
        {
            // 1) Obtener dimensiones actuales de la imagen desde el SDK
            int width  = _demoServer.GetImageWidthPixels();
            int height = _demoServer.GetImageHeightPixels();

            // 2) imageData.m_B_Imagedata ahora es un int[]
            int[] rawInts = imageData.m_B_Imagedata;
            if (rawInts == null || rawInts.Length < width * height)
                return null; // no hay imagen válida

            // 3) Copiar valores a un arreglo 2D [height, width], convirtiendo a float
            float[,] result = new float[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = (float) rawInts[i * width + j];
                }
            }
            return result;
        }
        
        // -----------------------------------------------------------------------
        // MÉTODO AUXILIAR #2: convierte double[,] (SWS) a WriteableBitmap Gray8
        // -----------------------------------------------------------------------
        private WriteableBitmap CreateGrayscaleBitmapFromDouble2D(double[,] data)
        {
            MessageBox.Show("[CreateGrayscale] Paso 1: Método iniciado.", "Debug CWT");
            int height = data.GetLength(0);
            int width  = data.GetLength(1);
            MessageBox.Show($"[CreateGrayscale] Paso 2: Dimensiones - alto={height}, ancho={width}.", "Debug CWT");
            // 1) Encontrar valores mínimo y máximo para normalizar
            double minVal = double.MaxValue, maxVal = double.MinValue;
            foreach (var val in data)
            {
                if (val < minVal) minVal = val;
                if (val > maxVal) maxVal = val;
            }
            MessageBox.Show($"[CreateGrayscale] Paso 3: minVal={minVal:F4}, maxVal={maxVal:F4}.", "Debug CWT");

            double range = maxVal - minVal;
            if (range <= 0) range = 1.0; // evitar división por cero
            MessageBox.Show($"[CreateGrayscale] Paso 4: range ajustado = {range:F4}.", "Debug CWT");

            // 2) Crear arreglo de bytes (Gray8), normalizando a [0..255]
            byte[] pixels = new byte[width * height];
            MessageBox.Show($"[CreateGrayscale] Paso 5: Array 'pixels' creado con longitud {pixels.Length}.", "Debug CWT");
            
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double norm = (data[i, j] - minVal) / range;
                    if (norm < 0) norm = 0;
                    if (norm > 1) norm = 1;
                    pixels[i * width + j] = (byte)(norm * 255.0);
                }
                if ((i + 1) % 150 == 0)
                {
                    MessageBox.Show($"[CreateGrayscale] Paso 5.x: Se normalizaron {i + 1} de {height} filas.", "Debug CWT");
                }
            }
            MessageBox.Show("[CreateGrayscale] Paso 6: Normalización completada y 'pixels' rellenado.", "Debug CWT");


            // 3) Construir el WriteableBitmap en formato Gray8
            var wb = new WriteableBitmap(
                width, height,
                96, 96,                  // DPI
                PixelFormats.Gray8,      // un byte por píxel
                null);

            MessageBox.Show("[CreateGrayscale] Paso 7: WriteableBitmap creado (Gray8).", "Debug CWT");
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = width; // en Gray8, stride = ancho en bytes
            wb.WritePixels(rect, pixels, stride, 0);
            MessageBox.Show("[CreateGrayscale] Paso 8: WritePixels completado.", "Debug CWT");
            MessageBox.Show("[CreateGrayscale] Paso 9: Devolviendo WriteableBitmap final.", "Debug CWT");

            return wb;
        }
        
        
        
        public enum ScanModeEnum
        {
            B = 0x01,			//B Ä£Ê½
			C = 0x02,			//C Ä£Ê½
			D_PW = 0x04,		//D_PW Ä£Ê½
			D = 0x10,			//D Ä£Ê½
			BC = B | C,			//B&C Ä£Ê½
			M = 0x20,			//M Ä£Ê½
			BM = B | M			//B&M Ä£Ê½
        }
        private Thread _dopplerDataThread;
        private bool _isCapturingDopplerData = false;

        private void StartEngineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(currentCheckModeParamPath))
                {
                    MessageBox.Show("El parámetro de ruta no está configurado.");
                    return;
                } 

                //MessageBox.Show("scanMode: " + scanMode);

                int configResult = _demoServer.SetParamPath(currentCheckModeParamPath, mScanMode);
                if (configResult != 0)
                {
                    MessageBox.Show("No se pudo configurar los parámetros del motor. Código de error: " + configResult);
                    return;
                }

                if (!_demoServer.IsRuning())
                {
                    int startResult = _demoServer.StartImageEngine();
                    if (startResult != 0)
                    {
                        MessageBox.Show("No se pudo iniciar el motor de imagen. Código de error: " + startResult);
                        return;
                    }
                }

                MessageBox.Show("Motor de imagen iniciado correctamente.");
                _isCapturingDopplerData = true;
                _dopplerDataThread = new Thread(new ParameterizedThreadStart(CaptureDopplerData));
                _dopplerDataThread.Start(this);

                MessageBox.Show("Motor de imagen y captura de datos Doppler iniciados correctamente.");
    

                // Crear una instancia del listener
                /*int dataSize = 830542940; // Asegúrate de que este tamaño sea correcto y esté definido en la DLL
                UltrasoundListener listener = new UltrasoundListener(dataSize);


                if (listener == null)
                {
                    MessageBox.Show("No se pudo crear el listener.");
                    return;
                }

                //MessageBox.Show(string.Format("Listener...:  {0}", listener));

                // Definir el delegado para el método OnFrameDataUpdated
                FrameDataUpdatedCallback callback = new FrameDataUpdatedCallback(listener.OnFrameDataUpdated);

                //MessageBox.Show(string.Format("Callback...:  {0}", callback));

                // Obtener un puntero a la función
                IntPtr listenerPtr = Marshal.GetFunctionPointerForDelegate(callback);

                // Configurar el listener utilizando el método de la DLL
                NativeMethods.SetFrameDataListener(listenerPtr);

                MessageBox.Show(string.Format("Listener configurado. Esperando datos...:  {0}", listenerPtr));*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al configurar el listener: {0}", ex.Message));
            }
        }

        // Método para capturar datos Doppler
        private void CaptureDopplerData(object state)
        {

            /*MainWindow control = state as MainWindow;
            MessageBox.Show("PROBAMOS SI INGRESA PARTE INICIAL");
            if (control == null)
            {
                MessageBox.Show("Error: El parámetro 'state' no es una instancia válida de MainWindow.");
                return;
            }

            if (control.imageData == null)
            {
                MessageBox.Show("Error: 'imageData' no está inicializado.");
                return;
            }

            if (_demoServer == null)
            {
                MessageBox.Show("Error: '_demoServer' no está inicializado.");
                return;
            }

            try
            {
                MessageBox.Show("PROBAMOS SI INGRESA PARTE MEDIA");

                /*int dopplerReadNum = 10;
                bool resetDisplayData = false;
                int nDataCount = control._demoServer.GetImageDisplayData_D_PW(control.imageData, dopplerReadNum, ref resetDisplayData);

                if (nDataCount > 0)
                {
                    if (!control._readDataThreadFlag)
                    {
                        return;
                    }
                    //System.Console.WriteLine("D PW Number " + imageData_GetDataTread.m_D_PW_ImageInfos[0].m_nDateNum.ToString());
                    //数据都拷贝到一个大数组里面
                    control.Dispatcher.Invoke(new Action(() =>
                    {
                        if (resetDisplayData)
                            control.DopplerCtrl.ClearScreen();
                        control.DopplerCtrl.AddData_D(control.imageData.m_D_PW_Imagedata, nDataCount, control.imageData.m_D_PW_ImageInfos[nDataCount - 1].m_nDateNum, control.pwState);

                    }));
                }*/


                /*int dopplerReadNum = 10;
                bool resetDisplayData = false;

                while (_isCapturingDopplerData)
                {
                    int nDataCount = control._demoServer.GetImageDisplayData_D_PW(control.imageData, dopplerReadNum, ref resetDisplayData);
                    MessageBox.Show(string.Format("nDataCount: {0}", nDataCount));
                    if (nDataCount > 0 && control.imageData.m_bD_PWHadData)
                    {
                        int[] dopplerValues = control.imageData.m_C_Imagedata;

                        // Verificar si hay datos en dopplerValues
                        if (dopplerValues != null && dopplerValues.Length > 0)
                        {
                            MessageBox.Show("Datos Doppler capturados exitosamente.");
                            foreach (var value in dopplerValues)
                            {
                                MessageBox.Show(string.Format("Datos Doppler capturados exitosamente: {0}", value));
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se recibieron datos Doppler.");
                        }
                    }
                    MessageBox.Show("PROBAMOS SI INGRESA PARTE FINAL");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Error en la captura de datos Doppler: " + ex.Message));
            }*/

            MainWindow control = state as MainWindow;
            int count = 0;
            while (control._readDataThreadFlag)
            {
                MessageBox.Show("INICIO");
                count++;
                control.TestUSBState();
                if (control.mScanMode == CLScanMode.CLScanModeEnum.D_PW && control.pwState == D_PW_StateE.PW_D)
                {
                    int dopplerReadNum = 10;
                    bool resetDisplayData = false;
                    int nDataCount = control._demoServer.GetImageDisplayData_D_PW(control.imageData, dopplerReadNum, ref resetDisplayData);
                    MessageBox.Show("nDataCount CLScanMode.CLScanModeEnum.D_PW && control.pwState == D_PW_StateE.PW_D");
                    if (nDataCount > 0)
                    {
                        if (!control._readDataThreadFlag)
                        {
                            return;
                        }
                        //System.Console.WriteLine("D PW Number " + imageData_GetDataTread.m_D_PW_ImageInfos[0].m_nDateNum.ToString());
                        //数据都拷贝到一个大数组里面
                        control.Dispatcher.Invoke(new Action(() =>
                        {
                            if (resetDisplayData)
                                control.DopplerCtrl.ClearScreen();
                            control.DopplerCtrl.AddData_D(control.imageData.m_D_PW_Imagedata, nDataCount, control.imageData.m_D_PW_ImageInfos[nDataCount - 1].m_nDateNum, control.pwState);

                        }));
                    }

                    //if (control.OpenAutoVesselAngle)
                    {
                        for (int i = 0; i < nDataCount; i++)
                        {

                            if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle != -999)
                            {

                                double currentLaunchDeflectionAngle_PW = control.BAWBUtrs.DoppleSGControl.GetLaunchDeflection();

                                double angle = control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW;
                                if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW < -90)
                                {
                                    angle = 180 + angle;
                                }
                                else if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW > 90)
                                {
                                    angle = angle - 180;
                                }

                                if (angle > 80)
                                {
                                    angle = 80;
                                }
                                else if (angle < -80)
                                {
                                    angle = -80;
                                }

                                control.Dispatcher.Invoke(new Action(() =>
                                {
                                    control.D_Angle.Value = (int)angle;
                                }));

                            }


                            if (control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_T != -999 && control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_B != -999)
                            {
                                //以取样门中心点为中心，垂直于角度方向上的血管半径。
                                //control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_T, control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_B
                            }

                        }
                    }

                }
                else
                {
                    int nDataCount = control._demoServer.GetImageDisplayData(control.imageData);
                    MessageBox.Show("nDataCount control._demoServer.GetImageDisplayData(control.imageData)");
                    if (nDataCount == 1)
                    {

                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (control.mScanMode == CLScanMode.CLScanModeEnum.B)
                            {
                                if (control.imageData.m_bBHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                                    using (FileStream fileStream = new FileStream("m_B_Imagedata.bin" + count, FileMode.Append, FileAccess.Write))
                                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                                    {
                                        // Escribir los datos de m_B_Imagedata en el archivo binario
                                        foreach (var value in control.imageData.m_B_Imagedata)
                                        {
                                            writer.Write(value);
                                        }
                                    }

                                    MessageBox.Show("Datos m_B_Imagedata guardados en el archivo .bin.");
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.BC)
                            {
                                MessageBox.Show("MODO C");
                                if (control.imageData.m_bCHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_C_Imagedata, control.imageData.m_nCImageDataLen);
                                    using (FileStream fileStream = new FileStream("m_C_Imagedata.bin" + count, FileMode.Append, FileAccess.Write))
                                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                                    {
                                        // Escribir los datos de m_C_Imagedata en el archivo binario
                                        foreach (var value in control.imageData.m_C_Imagedata)
                                        {
                                            writer.Write(value);
                                        }
                                    }

                                    MessageBox.Show("Datos m_C_Imagedata guardados en el archivo .bin.");
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.D_PW)
                            {
                                if (control.pwState == D_PW_StateE.PW_B)
                                {
                                    if (control.imageData.m_bBHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                                }
                                else if (control.pwState == D_PW_StateE.PW_BC)
                                {
                                    if (control.imageData.m_bCHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_C_Imagedata, control.imageData.m_nCImageDataLen);
                                    else if (control.imageData.m_bBHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                                }
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.BM)
                            {
                                if (control.imageData.m_bBHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);

                                if (control.imageData.m_bMHadData)
                                {
                                    if (control.imageData.m_M_ImageInfo.m_bClearScreen)
                                    {
                                        control.DopplerCtrl.ClearScreen();
                                    }
                                    //数据都拷贝到一个大数组里面
                                    control.DopplerCtrl.AddData_M(control.imageData.m_M_Imagedata,
                                    control.imageData.m_nMImageDataLen,
                                    control.imageData.m_M_ImageInfo.m_nImageHeightPixels,
                                    control.imageData.m_M_ImageInfo.m_nMLineNum);
                                }

                            }

                        }));

                        control._showTimeCount++;
                    }

                    if (control._showTimeCount >= 15)
                    {
                        control._TS_E = new TimeSpan(DateTime.Now.Ticks);

                        TimeSpan _TS_Mid = control._TS_E.Subtract(control._TS_S).Duration();

                        double fps = control._showTimeCount * 1000 / _TS_Mid.TotalMilliseconds;


                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            control.BAWBUtrs.FPS.Text = fps.ToString("F02") + "fps";
                        }));


                        control._TS_S = new TimeSpan(DateTime.Now.Ticks);
                        control._showTimeCount = 0;
                    }
                }

            }

        }

        // Método opcional para detener la captura de datos cuando se detiene el motor
        private void StopDopplerDataCapture()
        {
            _isCapturingDopplerData = false;
            _dopplerDataThread.Join();
        }

        /*private void StartEngineButton_Click(object sender, RoutedEventArgs e)
        {
            // Dimensiones de la imagen simulada (128x128 píxeles)
            int width = 128;
            int height = 128;

            // Generar datos en crudo simulados
            byte[,] datosEnCrudo = new byte[height, width];
            Random random = new Random();

            // Llenar la matriz con valores aleatorios (0-255)
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    datosEnCrudo[i, j] = (byte)random.Next(0, 256);
                }
            }

            // Guardar los datos en un archivo binario
            string filePath = "datosEnCrudoSimulados.bin"; // Puedes ajustar la ruta según sea necesario
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            writer.Write(datosEnCrudo[i, j]);
                        }
                    }
                }
            }

            MessageBox.Show(string.Format("Datos en crudo simulados guardados en", filePath));
        }*/

        private void GenerarDatosSimulados_Click(object sender, RoutedEventArgs e)
        {

            int width = 128;
            int height = 128;

            // Generar datos en crudo simulados para I y Q
            float[,] datosI = new float[height, width];
            float[,] datosQ = new float[height, width];
            Random random = new Random();

            // Parámetros para simular una señal sinusoidal
            double frecuencia = 5.0;
            double amplitud = 1.0;
            double ruido = 0.1;

            // Llenar las matrices con valores simulados de señales I/Q
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Simular una señal sinusoidal modulada con ruido
                    datosI[i, j] = (float)(amplitud * Math.Sin(2 * Math.PI * frecuencia * j / width) + ruido * (random.NextDouble() - 0.5));
                    datosQ[i, j] = (float)(amplitud * Math.Cos(2 * Math.PI * frecuencia * j / width) + ruido * (random.NextDouble() - 0.5));
                }
            }

            // Guardar los datos en un archivo binario
            string filePath = "datosDopplerSimulados.bin";
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            writer.Write(datosI[i, j]);
                            writer.Write(datosQ[i, j]);
                        }
                    }
                }
            }

            MessageBox.Show(string.Format("Datos Doppler simulados guardados en {0}", filePath));

        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Mueve el rectángulo
            double newLeft = Canvas.GetLeft(SelectionRectangle) + e.HorizontalChange;
            double newTop = Canvas.GetTop(SelectionRectangle) + e.VerticalChange;

            // Actualiza la posición del rectángulo
            Canvas.SetLeft(SelectionRectangle, newLeft);
            Canvas.SetTop(SelectionRectangle, newTop);

            // Actualiza también la posición de los Thumbs
            PositionThumbs();
        }
        // Método para redimensionar el rectángulo
        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // Calcular el nuevo tamaño
            double newWidth = SelectionRectangle.Width + e.HorizontalChange;
            double newHeight = SelectionRectangle.Height + e.VerticalChange;

            // Limitar el tamaño para que no sea mayor que la pantalla
            if (Canvas.GetLeft(SelectionRectangle) + newWidth > screenWidth)
            {
                newWidth = screenWidth - Canvas.GetLeft(SelectionRectangle);
            }

            if (Canvas.GetTop(SelectionRectangle) + newHeight > screenHeight)
            {
                newHeight = screenHeight - Canvas.GetTop(SelectionRectangle);
            }

            // Asegurarse de que el rectángulo no tenga un tamaño negativo o demasiado pequeño
            if (newWidth > 20 && newHeight > 20)
            {
                SelectionRectangle.Width = newWidth;
                SelectionRectangle.Height = newHeight;

                // Reposiciona los Thumbs
                PositionThumbs();
            }
        }

        // Método para posicionar los Thumbs en las esquinas correctas del rectángulo
        private void PositionThumbs()
        {
            // Posiciona el Thumb para mover
            Canvas.SetLeft(MoveThumb, Canvas.GetLeft(SelectionRectangle) + SelectionRectangle.Width - MoveThumb.Width / 2);
            Canvas.SetTop(MoveThumb, Canvas.GetTop(SelectionRectangle) + SelectionRectangle.Height - MoveThumb.Height / 2);

            // Posiciona el Thumb para redimensionar
            Canvas.SetLeft(ResizeThumb, Canvas.GetLeft(SelectionRectangle) + SelectionRectangle.Width - ResizeThumb.Width / 2);
            Canvas.SetTop(ResizeThumb, Canvas.GetTop(SelectionRectangle) + SelectionRectangle.Height - ResizeThumb.Height / 2);
        }

        private void SelectionRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Marcar que el rectángulo está siendo arrastrado
            isDragging = true;

            // Obtener la posición inicial del ratón dentro del rectángulo
            clickPosition = e.GetPosition(SelectionRectangle);

            // Capturar el ratón para seguir recibiendo eventos MouseMove incluso fuera del rectángulo
            SelectionRectangle.CaptureMouse();
        }

        private void SelectionRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Obtener la posición del ratón dentro del canvas
                System.Windows.Point mousePosition = e.GetPosition(SelectionCanvas); // Asegúrate de reemplazar 'MyCanvas' por el nombre de tu canvas

                // Calcular la nueva posición del rectángulo
                double newLeft = mousePosition.X - clickPosition.X;
                double newTop = mousePosition.Y - clickPosition.Y;

                // Mover el rectángulo a la nueva posición
                Canvas.SetLeft(SelectionRectangle, newLeft);
                Canvas.SetTop(SelectionRectangle, newTop);
            }
        }

        private void SelectionRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Finalizar el arrastre
            isDragging = false;

            // Liberar el ratón
            SelectionRectangle.ReleaseMouseCapture();
        }

        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {

            // Captura las coordenadas del rectángulo de selección dentro del Canvas
            System.Windows.Point topLeft = SelectionRectangle.PointToScreen(new System.Windows.Point(0, 0));

            double x = topLeft.X;
            double y = topLeft.Y;
            double width = SelectionRectangle.Width;
            double height = SelectionRectangle.Height;

            // Asegúrate de que el área sea válida
            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("Selecciona un área válida para la grabación.");
                return;
            }

            // Ajustar el ancho y el alto para que sean múltiplos de dos
            int adjustedWidth = (int)width;
            int adjustedHeight = (int)height;

            adjustedWidth = adjustedWidth % 2 == 0 ? adjustedWidth : adjustedWidth + 1;
            adjustedHeight = adjustedHeight % 2 == 0 ? adjustedHeight : adjustedHeight + 1;

            // Convertir el área seleccionada en un rectángulo para usar en la captura de pantalla
            _captureArea = new System.Drawing.Rectangle((int)x, (int)y, adjustedWidth, adjustedHeight);

            // Especificar el archivo de salida
            _outputFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ScreenRecording.avi");

            // Iniciar el escritor de video
            _videoWriter = new VideoFileWriter();
            _videoWriter.Open(_outputFile, _captureArea.Width, _captureArea.Height, 30, VideoCodec.MPEG4, 1000000);

            _isRecording = true;
            _recordingThread = new Thread(RecordScreen);
            _recordingThread.Start();

            // Cambiar visibilidad de los botones
            StartRecording.Visibility = Visibility.Collapsed;
            StopRecording.Visibility = Visibility.Visible;
        }



        private void RecordScreen()
        {
            try
            {
                while (_isRecording)
                {
                    // Captura el área definida por el rectángulo y conviértela en un bitmap
                    Bitmap screenshot = CaptureScreen(_captureArea);

                    // Escribe cada frame en el video
                    _videoWriter.WriteVideoFrame(screenshot);

                    // Libera el bitmap de la memoria
                    screenshot.Dispose();

                    // Espera para capturar el siguiente frame (~30fps)
                    Thread.Sleep(33); // ~30 fps
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Selecciona un área válida para la grabación."));
            }
        }

        private Bitmap CaptureScreen(System.Drawing.Rectangle area)
        {
            // Crear un bitmap del tamaño del área seleccionada
            Bitmap screenshot = new Bitmap(area.Width, area.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Capturar el área específica de la pantalla
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                // Copiar desde la pantalla la porción que ha sido seleccionada
                g.CopyFromScreen(area.X, area.Y, 0, 0, new System.Drawing.Size(area.Width, area.Height));
            }

            return screenshot;
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            // Detener la grabación
            _isRecording = false;

            // Esperar a que termine el hilo de grabación
            _recordingThread.Join();

            // Cerrar el archivo de video
            _videoWriter.Close();

            MessageBox.Show(string.Format("Grabación guardada en {0}", _outputFile));

            // Cambiar visibilidad de los botones
            StopRecording.Visibility = Visibility.Collapsed;
            StartRecording.Visibility = Visibility.Visible;
        }

        private void SetShowText(string msg, int dueTime, int periodTime)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.ErrorMsg.Text = msg;
                this.ErrorMsg.Visibility = Visibility.Visible;
            }));

            _setTextTimer.Change(dueTime, periodTime);
        }
        private void SetTextTimer(object state)
        {
            MainWindow control = state as MainWindow;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ErrorMsg.Text = "";
                this.ErrorMsg.Visibility = Visibility.Collapsed;
                _setTextTimer.Change(Timeout.Infinite, Timeout.Infinite);

            }));
        }

        private void MainWindow_HardWareMsgSEvent(int msgType, int[] value)
        {
            if (msgType == 2)
            {
                if (value != null && value.Length == 3)
                {
                    if (value[2] == 1)
                    {
                        if (value[0] == 0xaaaa)
                        {
                            //一线器件异常
                            SetShowText("一线器件异常", 10000, 100);
                        }
                        else
                        {

                            string probeName = "";
                            if (ProbeIDAndName.ContainsKey(value[0]))
                            {
                                probeName = "探头ID: " + value[0].ToString() + " 探头名称：" + (string)ProbeIDAndName[value[0]];
                                SelecteProbeNameByProbeID(value[0]);

                            }
                            else
                            {
                                probeName = "未识别探头id";
                            }
                            SetShowText(probeName, 50000, 100);

                        }
                    }
                    else if (value[2] == 0)
                    {
                        if (value[0] == 0x5555)
                        {
                            SetShowText("探头脱落", 10000, 100);

                        }
                    }

                }

            }
        }

        private void MainWindow_HardWareMsgEvent(int msgType, int value)
        {
            if (msgType == 2)
            {
                if (value == 0x5555) //探头脱落
                {
                    SetShowText("探头脱落", 10000, 100);
                }
                else if (value == 0xaaaa)//一线器件异常
                {
                    SetShowText("一线器件异常", 10000, 100);

                }
                else //探头连接
                {
                    string probeName = "";
                    if (ProbeIDAndName.ContainsKey(value))
                    {
                        probeName = "探头ID: " + value.ToString() + " 探头名称：" + (string)ProbeIDAndName[value];
                        SelecteProbeNameByProbeID(value);

                    }
                    else
                    {
                        probeName = "未识别探头id";
                    }
                    SetShowText(probeName, 50000, 100);
                }

            }
        }

        private void StartGetHWVersion()
        {
            _getHWVerTimer.Change(500, Timeout.Infinite);
        }

        private string GetLogicCompileVersion(uint LogicCompileVersion)
        {
            if (currentVID == 0x4611)
            {

                uint _LogicType = (LogicCompileVersion & 0x0000C000) >> 14;

                uint _ProductSerialNumber = (LogicCompileVersion & 0x00003E00) >> 9;

                uint _LogicCompileVersion = LogicCompileVersion & 0x000001FF;

                return _LogicType.ToString() + "_" + _ProductSerialNumber.ToString() + "_" + _LogicCompileVersion.ToString();
            }

            return LogicCompileVersion.ToString();
        }
        private void GetHWVersion(object state)
        {
            MainWindow control = state as MainWindow;
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                CLRHardWareInfo hwInfo = new CLRHardWareInfo();
                int nRet = _demoServer.GetHardWareInfo(hwInfo);
                string ver = "--";
                if (nRet == 0)
                {
                    //ver = "硬件版本：" + hwInfo.nHWVersion_Major.ToString() + "." + hwInfo.nLogicVersion_Minor.ToString();
                    ver = " 固件版本：" + hwInfo.nLogicVersion_Major.ToString() + "." + hwInfo.nLogicVersion_Minor.ToString() + "-" + GetLogicCompileVersion(hwInfo.unLogicCompileVersion);
                }
                //获取HW Ver
                control.HardWareVersion.Text = ver;

                int[] sdkversion = new int[3];
                _demoServer.GetSDKVersion(sdkversion);

                control.SDKVersion.Text = sdkversion[0].ToString() + "." + sdkversion[1].ToString() + "." + sdkversion[2].ToString();

                control._getHWVerTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            ));
        }


        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            //监视USB硬件的插入和拔出事件
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hook = new HwndSourceHook(cyUSBMonitor.WindowProc);
                hwndSource.AddHook(hook);
                cyUSBMonitor.RegisterDeviceToWin(hwndSource.Handle);
            }
            cyUSBMonitor.USBConnectStateChangeEvent += cyUSBMonitor_USBConnectStateChangeEvent;

           
            this.BAWBUtrs.ColorFrameControl.QuadrangleFrameChangedEvent += ColorFrameControl_QuadrangleFrameChangedEvent;
            this.BAWBUtrs.ColorFrameControl.SectorFrameChangedEvent += ColorFrameControl_SectorFrameChangedEvent;

            this.BAWBUtrs.DoppleSGControl.SamplingGateLineChangedEvent += DoppleSGControl_SamplingGateLineChangedEvent;
            this.BAWBUtrs.DoppleSGControl.SamplingGateConvexChangedEvent += DoppleSGControl_SamplingGateConvexChangedEvent;
            this.BAWBUtrs.DoppleSGControl.SamplingGatePAChangedEvent += DoppleSGControl_SamplingGatePAChangedEvent;

            this.BAWBUtrs.MModeSLControl.SamplingLineChangedEvent += MModeSLControl_SamplingLineChangedEvent;
            this.BAWBUtrs.MModeSLControl.SamplingLineConvexChangedEvent += MModeSLControl_SamplingLineConvexChangedEvent;
            this.BAWBUtrs.MModeSLControl.SamplingLinePhasedChangedEvent += MModeSLControl_SamplingLinePhasedChangedEvent;

            this.BAWBUtrs.DoppleSGControl.SetVascularAngle(0);
            this.DopplerCtrl.SetShowTime(4);
            this.DopplerCtrl.SetBaseLine(0);
            this.DopplerCtrl.SetVascularAngle(0);
            this.DopplerCtrl.SetLaunchDeflection(0);
            this.DopplerCtrl.ShowGridLine(false);
            this.DopplerCtrl.GetPwDataEvent += DopplerCtrl_GetPwDataEvent;
            this.DopplerCtrl.SetBaseLineChangeEvent += DopplerCtrl_SetBaseLineChangeEvent;


            //设置图像显示范围
            ImageWidth = 1280;
            ImageHeigh = 960;
            RefreshProbeNamesValue();

        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            //主动查找USB硬件的当前状态
            PnPEntityInfo[] infolist = cyUSBMonitor.WhoPnPEntity(0x04B4, UInt16.MinValue, Guid.Empty);

            if (infolist != null && infolist.Length >= 1)
            {
                if (infolist[0].VendorID == 0x04B4)
                {
                    currentVID = infolist[0].ProductID;
                    CLRUSDeviceInfo[] usDeviceInfoArray = new CLRUSDeviceInfo[5];
                    int getDataNum = 5;
                    int count = _demoServer.GetUSDeviceInfos(usDeviceInfoArray, ref getDataNum);
                    if (count > 0)
                    {

                        if (getDataNum > 0)
                        {
                            string devicePath = usDeviceInfoArray[0].DevicePath;

                            //探头可插拔
                            //usDeviceInfoArray[0].ProbeCanreplaced
                            string probeName = "";

                            if (usDeviceInfoArray[0].IsMultiProbe == 0)
                            {
                                if (usDeviceInfoArray[0].probeConnect)
                                {
                                    if (usDeviceInfoArray[0].probeID == 0xaaaa)//一线器件异常
                                    {
                                        SetShowText("一线器件异常", 10000, 100);
                                        UsbConn = true;
                                    }

                                    if (ProbeIDAndName.ContainsKey(usDeviceInfoArray[0].probeID))
                                    {
                                        probeName = "探头ID: " + usDeviceInfoArray[0].probeID.ToString() + " 探头名称：" + (string)ProbeIDAndName[usDeviceInfoArray[0].probeID];

                                        SelecteProbeNameByProbeID(usDeviceInfoArray[0].probeID);
                                        UsbConn = true;

                                    }
                                    else
                                    {
                                        probeName = "未识别探头id";
                                        UsbConn = true;

                                    }
                                    SetShowText(probeName, 50000, 100);
                                }
                                else
                                {
                                    SetShowText("探头脱落", 10000, 100);
                                    UsbConn = false;
                                }

                            }
                            //获取硬件版本想信息会影响数据传输。引起pw图像出现错误。
                            StartGetHWVersion();
                        }


                    }
                    else
                    {
                        SetShowText("获取超声设备信息失败", 10000, 100);
                        UsbConn = false;
                    }
                }

            }
        }
      
        void cyUSBMonitor_USBConnectStateChangeEvent(bool isConnect,int VID,int PID)
        {

            if (isConnect && VID == 0x04B4)
            {
                currentVID = PID;
                CLRUSDeviceInfo[] usDeviceInfoArray = new CLRUSDeviceInfo[5];
                int getDataNum = 5;
                int count = _demoServer.GetUSDeviceInfos(usDeviceInfoArray, ref getDataNum);
                if (count > 0)
                {

                    if (getDataNum > 0)
                    {
                        string devicePath = usDeviceInfoArray[0].DevicePath;

                        if(PID == 0x00F0)
                        {
                            //探头可插拔
                            //usDeviceInfoArray[0].ProbeCanreplaced
                            string probeName = "";

                            if (usDeviceInfoArray[0].IsMultiProbe == 0)
                            {
                                if (usDeviceInfoArray[0].probeConnect)
                                {
                                    if (usDeviceInfoArray[0].probeID == 0xaaaa)//一线器件异常
                                    {
                                        SetShowText("一线器件异常", 10000, 100);
                                        UsbConn = true;
                                    }

                                    if (ProbeIDAndName.ContainsKey(usDeviceInfoArray[0].probeID))
                                    {
                                        probeName = "探头ID: " + usDeviceInfoArray[0].probeID.ToString() + " 探头名称：" + (string)ProbeIDAndName[usDeviceInfoArray[0].probeID];

                                        SelecteProbeNameByProbeID(usDeviceInfoArray[0].probeID);
                                        UsbConn = true;

                                    }
                                    else
                                    {
                                        probeName = "未识别探头id";
                                        UsbConn = true;

                                    }
                                    SetShowText(probeName, 50000, 100);
                                }
                                else
                                {
                                    SetShowText("探头脱落", 10000, 100);
                                    UsbConn = false;
                                }

                            }
                        }
                        else if (PID == 0x4611)
                        {
                            SelecteProbeNameByProbeID(usDeviceInfoArray[0].probeID);
                            UsbConn = true;
                        }
                    }
                }
                else
                {
                    SetShowText("获取超声设备信息失败", 10000, 100);
                    UsbConn = false;
                }

                //获取硬件版本想信息会影响数据传输。引起pw图像出现错误。
                StartGetHWVersion();
            }
            else if (!isConnect && VID == 0x04B4)
            {
                DoFreezeClick();

                UsbConn = false;
            }

       
        }


        private Dictionary<int, double> _depthDictionary = new Dictionary<int, double>();


        private void RefreshProbeNamesValue()
        {
            //清空显示
            this.UseProbeName.Items.Clear();

            var probeInfoList = _examModeConfigSetting.ProbeInfoList;
            //将探头填入
            foreach (var probeInfo in probeInfoList)
            {
                this.UseProbeName.Items.Add(new ComboBoxItem() { Content = probeInfo.ProberName, Tag = probeInfo });
            }

            this.UseProbeName.SelectedIndex = 0;

        }


        #region 属性

        private double _defaultFontSize = 22;

        private double _currentFontSize = 22;
        public double CurrentFontSize
        {
            get { return _currentFontSize; }
            set
            {
                _currentFontSize = value;
                OnPropertyChanged("CurrentFontSize");
            }
        }

        private int _imageHeigh;
        public int ImageHeigh
        {
            get { return _imageHeigh; }
            set
            {
                _imageHeigh = value;
                OnPropertyChanged("ImageHeigh");
            }
        }
        private int _imageWidth;
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                OnPropertyChanged("ImageWidth");
            }
        }

        private Visibility _neVisibility = Visibility.Collapsed;
        public Visibility NEVisibility
        {
            get { return _neVisibility; }
            set
            {
                _neVisibility = value;
                OnPropertyChanged("NEVisibility");
            }
        }

        #endregion



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _readDataThreadFlag = false;
            _demoServer.StopImageEngine();
        }


        private void SelecteProbeNameByProbeID(int probeID)
        {
            string probeName = "";
            if (ProbeIDAndName.ContainsKey(probeID))
                probeName = (string)ProbeIDAndName[probeID];
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var probeItem in this.UseProbeName.Items)
                {
                    ComboBoxItem item = probeItem as ComboBoxItem;
                    if (item != null)
                    {
                        if (item.Content.ToString().Equals(probeName))
                        {
                            this.UseProbeName.SelectedItem = item;
                            break;
                        }
                    }
                }
            }));


        }


        ObservableCollection<ExamInfo> _examNameList_byUseExamName = new ObservableCollection<ExamInfo>();
        private void UseProbeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedUseProbeName = ((ComboBoxItem)this.UseProbeName.SelectedValue).Content.ToString();

            //根据选中的探头，重新填入检查列表
            this.UseExameName.Items.Clear();

            if (_examModeConfigSetting.ProbeAExamInfoList.TryGetValue(selectedUseProbeName, out _examNameList_byUseExamName))
            {
                foreach (var examInfo in _examNameList_byUseExamName)
                {

                    this.UseExameName.Items.Add(new ComboBoxItem() { Content = examInfo.ExamName_ZH, Tag = examInfo.ExamName_EN });

                }
            }

            this.UseExameName.SelectedIndex = 0;
            ProbeInfo currentProbeInfo = (ProbeInfo)((ComboBoxItem)this.UseProbeName.SelectedValue).Tag;
            if (currentProbeInfo.IsSupportPW)
            {
                this.PWCtrl.Visibility = Visibility.Visible;
            }
            else
            {
                this.PWCtrl.Visibility = Visibility.Collapsed;
            }

        }
        #region  获取数据
        private TimeSpan _TS_S;
        private TimeSpan _TS_E;
        private int _showTimeCount;
        private int currentDataIndex = 0;
        private static void GetDataTread(object state)
        {
            MainWindow control = state as MainWindow;

            while (control._readDataThreadFlag)
            {
                control.TestUSBState();
                if (control.mScanMode == CLScanMode.CLScanModeEnum.D_PW && control.pwState == D_PW_StateE.PW_D)
                {
                    int dopplerReadNum = 10;
                    bool resetDisplayData = false;
                    int nDataCount = control._demoServer.GetImageDisplayData_D_PW(control.imageData, dopplerReadNum, ref resetDisplayData);
                    if (nDataCount > 0)
                    {
                        if (!control._readDataThreadFlag)
                        {
                            return;
                        }
                        //System.Console.WriteLine("D PW Number " + imageData_GetDataTread.m_D_PW_ImageInfos[0].m_nDateNum.ToString());
                        //数据都拷贝到一个大数组里面
                        control.Dispatcher.Invoke(new Action(() =>
                        {
                            if (resetDisplayData)
                                control.DopplerCtrl.ClearScreen();
                            control.DopplerCtrl.AddData_D(control.imageData.m_D_PW_Imagedata, nDataCount, control.imageData.m_D_PW_ImageInfos[nDataCount - 1].m_nDateNum, control.pwState);

                        }));
                    }

                    //if (control.OpenAutoVesselAngle)
                    {
                        for (int i = 0; i < nDataCount; i++)
                        {

                            if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle != -999)
                            {
                               
                                double currentLaunchDeflectionAngle_PW = control.BAWBUtrs.DoppleSGControl.GetLaunchDeflection();

                                double angle = control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW;
                                if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW < -90)
                                {
                                    angle = 180 + angle;
                                }
                                else if (control.imageData.m_D_PW_ImageInfos[i].m_AutoMeasureAngle + currentLaunchDeflectionAngle_PW > 90)
                                {
                                    angle = angle - 180;
                                }

                                if (angle > 80)
                                {
                                    angle = 80;
                                }
                                else if (angle < -80)
                                {
                                    angle = -80;
                                }

                                control.Dispatcher.Invoke(new Action(() =>
                                {
                                    control.D_Angle.Value = (int)angle;
                                }));

                            }


                            if (control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_T != -999 && control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_B != -999)
                            {
                                //以取样门中心点为中心，垂直于角度方向上的血管半径。
                                //control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_T, control.imageData.m_D_PW_ImageInfos[i].m_BloodRadius_B
                            }

                        }
                    }

                }
                else
                {
                    int nDataCount = control._demoServer.GetImageDisplayData(control.imageData);

                    if (nDataCount == 1)
                    {

                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (control.mScanMode == CLScanMode.CLScanModeEnum.B)
                            {
                                if (control.imageData.m_bBHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.BC)
                            {
                                if (control.imageData.m_bCHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_C_Imagedata, control.imageData.m_nCImageDataLen);
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.D_PW)
                            {
                                if (control.pwState == D_PW_StateE.PW_B)
                                {
                                    if (control.imageData.m_bBHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                                }
                                else if (control.pwState == D_PW_StateE.PW_BC)
                                {
                                    if (control.imageData.m_bCHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_C_Imagedata, control.imageData.m_nCImageDataLen);
                                    else if (control.imageData.m_bBHadData)
                                        control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);
                                }
                            }
                            else if (control.mScanMode == CLScanMode.CLScanModeEnum.BM)
                            {
                                if (control.imageData.m_bBHadData)
                                    control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);

                                if (control.imageData.m_bMHadData)
                                {
                                    if (control.imageData.m_M_ImageInfo.m_bClearScreen)
                                    {
                                        control.DopplerCtrl.ClearScreen();
                                    }
                                    //数据都拷贝到一个大数组里面
                                    control.DopplerCtrl.AddData_M(control.imageData.m_M_Imagedata,
                                    control.imageData.m_nMImageDataLen,
                                    control.imageData.m_M_ImageInfo.m_nImageHeightPixels,
                                    control.imageData.m_M_ImageInfo.m_nMLineNum);
                                }

                            }

                        }));

                        control._showTimeCount++;
                    }

                    if (control._showTimeCount >= 15)
                    {
                        control._TS_E = new TimeSpan(DateTime.Now.Ticks);

                        TimeSpan _TS_Mid = control._TS_E.Subtract(control._TS_S).Duration();

                        double fps = control._showTimeCount * 1000 / _TS_Mid.TotalMilliseconds;


                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            control.BAWBUtrs.FPS.Text = fps.ToString("F02") + "fps";
                        }));


                        control._TS_S = new TimeSpan(DateTime.Now.Ticks);
                        control._showTimeCount = 0;
                    }
                }

            }
        }
      

        //获取录像数据
        private int _historyDataIndex = 1;
        private static void GetHistoryDataTread(object state)
        {
            MainWindow control = state as MainWindow;
            //M在屏幕的位置
            double MPosOnScreen = 1;
            //M增加index
            double MIndexAppend = 0;
            int nFrameRate = (int)control._demoServer.GetCurrentFrameRate();
            int sleepTime = 1000 / nFrameRate;
            int historyDataCount = control._demoServer.GetHistoryImageCount();
            while (control._readHistoryDataThreadFlag)
            {

                if (control.mScanMode == CLScanMode.CLScanModeEnum.BM)
                {

                    int startPixel = 0;
                    int mIndexPixel = 0;
                    int nDataCount = control._demoServer.GetHistoryImageData_BM(control.DopplerCtrl.ImageShowWidth, control.currentDataIndex, control.imageData, ref startPixel, ref mIndexPixel, MIndexAppend, false, MPosOnScreen);

                    if (nDataCount == 1)
                    {
                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (control.imageData.m_bBHadData)
                                control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata, control.imageData.m_nBImageDataLen);

                            if (control.imageData.m_bMHadData)
                            {
                                if (control.imageData.m_M_ImageInfo.m_bClearScreen || control.currentDataIndex == 0)
                                {
                                    control.DopplerCtrl.ClearScreen();
                                }
                                //数据都拷贝到一个大数组里面
                                control.DopplerCtrl.AddData_M(control.imageData.m_M_Imagedata,
                                control.imageData.m_nMImageDataLen,
                               control.imageData.m_M_ImageInfo.m_nImageHeightPixels,
                                control.imageData.m_M_ImageInfo.m_nMLineNum,
                                mIndexPixel,
                                (MPosOnScreen != 1) ? true : false);
                            }
                            else
                            if (mIndexPixel != -1)
                            {
                                control.DopplerCtrl.Refresh_M_Index(mIndexPixel, (MPosOnScreen != 1) ? true : false);
                            }

                        }));
    
                    }
                }
                else
                {

                    int nDataCount = control._demoServer.GetHistoryImageData(control.imageData, control.currentDataIndex);

                    if (nDataCount == 1)
                    {

                        control.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (control.imageData.m_bBHadData)
                                control.BAWBUtrs.AddData(control.imageData.m_B_Imagedata);
                            if (control.imageData.m_bCHadData)
                                control.BAWBUtrs.AddData(control.imageData.m_C_Imagedata);

                        }));

                    }
                }

                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    control.BAWBUtrs.FPS.Text = control.currentDataIndex.ToString() + " / " + historyDataCount.ToString();
                }));

                control.currentDataIndex++;
                if (control.currentDataIndex >= historyDataCount)
                {
                    control.currentDataIndex = 0;
                }
                Thread.Sleep(sleepTime);
            }
        }

        public class replayPW
        {
            public replayPW()
            {
                _regetData = true;
            }
            public int _lastLineNum;
            public int _lineNumbers;
            public bool _regetData;
        }

        private int _freezeDataBaseLine = 0;
        private Queue<replayPW> replayPWQ = new Queue<replayPW>();
        private object locker_PW = new object();
        private Thread thread_replayPW = null;

        void DopplerCtrl_GetPwDataEvent(int lastLineNum, int lineNumbers, bool regetData)
        {
            if (!DopplerCtrl.IsVisible)
            {
                return;
            }
            //非冻结状态不能重新绘制
            if (!this.BAWBUtrs.FreezeState)
                return;

            lock (locker_PW)
            {
                replayPWQ.Enqueue(new replayPW() { _lastLineNum = lastLineNum, _lineNumbers = lineNumbers, _regetData = regetData });
            }
            if (thread_replayPW == null || !thread_replayPW.IsAlive)
            {
                thread_replayPW = new Thread(new ParameterizedThreadStart(GetHistoryD_PWDataThread));
                thread_replayPW.Start(this);
            }
        }

        private int lastHistoryPWDataCount = 0;
        private void GetHistoryD_PWDataThread(object state)
        {

            int lastLineNum = 0;
            int lineNumbers = 0;
            bool regetData = false;
            int nDataCount = 0;
            MainWindow control = state as MainWindow;
            ImageData_D_PW imageData = new ImageData_D_PW();
            ImageData_M imageData_M = new ImageData_M();
            while (control.replayPWQ.Count > 0)
            {
               

                replayPW _replayPW = null;
                lock (control.locker_PW)
                {
                    while (control.replayPWQ.Count > 0)
                    {
                        _replayPW = control.replayPWQ.Dequeue();
                    }
                }
                if (_replayPW == null)
                {
                    break;
                }

                lastLineNum = _replayPW._lastLineNum;
                lineNumbers = _replayPW._lineNumbers;
                regetData = _replayPW._regetData;
                System.Console.WriteLine("His PW Get lastLineNum " + lastLineNum.ToString() + " lineNumbers " + lineNumbers.ToString());
                int curentBaseLine = DopplerCtrl.FlowScale.GetBaseLine();
                int pwImageWidth = DopplerCtrl.ImageRealShowWidth;
                int pwImageHeight = DopplerCtrl.ImageRealShowHeight;
                if (DopplerImage_SourceData == null || DopplerImage_SourceData.Length != pwImageHeight * pwImageWidth * dopplerImageNum)
                {
                    DopplerImage_SourceData = new int[pwImageHeight * pwImageWidth * dopplerImageNum];
                }

                int _currentDataNum = lastLineNum - lineNumbers + 1 >= 0 ? lastLineNum - lineNumbers + 1 : 0;
                int dopplerReadNum = lineNumbers;

                //D
                if (regetData)
                {
                    nDataCount = _demoServer.GetHistoryImageData_D_PW(imageData, _currentDataNum, dopplerReadNum);
                    lastHistoryPWDataCount = nDataCount;
                }
                else
                {
                    nDataCount = lastHistoryPWDataCount;
                }

           
                int[] envelopeInfoMax = null;
                int[] envelopeInfoMean = null;

                if (nDataCount > 0)
                {
                    envelopeInfoMax = new int[nDataCount];
                    envelopeInfoMean = new int[nDataCount];
                    Array.Clear(DopplerImage_SourceData, 0, DopplerImage_SourceData.Length);

                    //计算基线位移
                    int dataMove = 0;

                    int bl = (int)((-_freezeDataBaseLine + 128) * 1.0 / 256 * pwImageHeight);
                    int currentBL = (int)((-curentBaseLine + 128) * 1.0 / 256 * pwImageHeight);

                    if (currentBL == bl)
                    {
                        dataMove = 0;
                    }
                    else
                    {
                        dataMove = currentBL - bl;
                    }

                    int[] line = new int[1024];
                    //将数据填成一副图
                    for (int i = 0; i < nDataCount; i++)
                    {
                        int SpectralLineLen = imageData.m_D_PW_ImageInfos[i].m_nD_PWDataLen;
                        //当前缓存中的数据编号如果大于显示编号，不再显示
                        if (imageData.m_D_PW_ImageInfos[i].m_nDateNum > lastLineNum)
                        {
                            nDataCount = i;
                            break;
                        }
                        Array.Copy(imageData.m_D_PW_Imagedata, i * SpectralLineLen, line, 0, SpectralLineLen);
                        for (int j = 0; j < pwImageHeight; j++)
                        {
                            DopplerImage_SourceData[j * pwImageWidth * dopplerImageNum + i] = line[(j - dataMove + pwImageHeight) % pwImageHeight];
                        }
                        //根据基线修改包络
                        // envelopeInfo[i] = dopplerData[i].m_envelopeInfo.bHadData ? (dopplerData[i].m_envelopeInfo.m_nEnvelope + dataMove + pwImageHeight) % pwImageHeight : -1;
                        //获取包络
                        envelopeInfoMax[i] = imageData.m_D_PW_ImageInfos[i].envelopeInfo.m_bHadData ? imageData.m_D_PW_ImageInfos[i].envelopeInfo.m_nEnvelopeMax : -1;
                        envelopeInfoMean[i] = imageData.m_D_PW_ImageInfos[i].envelopeInfo.m_bHadData ? imageData.m_D_PW_ImageInfos[i].envelopeInfo.m_nEnvelopeMean : -1;
                    }

                    if (nDataCount == 0)
                    {
                        continue;
                    }
                    int firstDataNum = imageData.m_D_PW_ImageInfos[0].m_nDateNum;
                    int lastDataNum = imageData.m_D_PW_ImageInfos[nDataCount - 1].m_nDateNum;
                    System.Console.WriteLine("His PW Get firstDataNum " + firstDataNum.ToString() + " lastDataNum " + lastDataNum.ToString());
                    int[] envelopeInfoRealMax = new int[nDataCount];
                    int[] envelopeInfoRealMean = new int[nDataCount];
                    Array.Copy(envelopeInfoMax, envelopeInfoRealMax, nDataCount);
                    Array.Copy(envelopeInfoMean, envelopeInfoRealMean, nDataCount);
                    //包络测试使用
                    //DopplerImage = SuperimposedEnvelopeDopplerImage(DopplerImage, envelopeInfo, pwImageHeight, pwImageWidth, dopplerImageNum);

                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DopplerCtrl.AddDataDopplerImage(DopplerImage_SourceData, envelopeInfoRealMax, envelopeInfoRealMean, firstDataNum, lastDataNum);

                        }));
                    }

                }

            }


            thread_replayPW.Abort();
        }

        #endregion

        private bool canSetAlgorithParam = false;   //在设置算法目录前，不能设置参数。防止控件在初始化时，调用设置参数方法。
        private Probe_Info _current_probe_Info = new Probe_Info();
        private void ResetParam()
        {
            //设置默认值
            //B
            this.Gain.Value = 50;
            this.DynamicRange.Value = 60;
            this.SRI.Value = 5;
            this.Correlation.Value = 3;
            this.GrayLevel.Value = 4;
            this.PseudocolorLevel.Value = 0;
            this.Harmonic.SelectedIndex = 0;
            this.FocusArea.SelectedIndex = 3;
            this.Frequency.SelectedIndex = 1;
            this.NEONOFF.SelectedIndex = 0;
            this.NeAngle.Value = 30;


            this.TGC1.Value = 0;
            this.TGC2.Value = 0;
            this.TGC3.Value = 0;
            this.TGC4.Value = 0;
            this.TGC5.Value = 0;
            this.TGC6.Value = 0;

            //C
            this.C_Gain.Value = 50;
            this.C_WF.Value = 1;
            this.C_Correlation.Value = 2;
            this.C_Scale.Value = 3;
            this.C_Steer.Value = 0;
            this.C_Priority.Value = 2;
            this.C_Mode.SelectedIndex = 0;
            this.C_Map.Value = 1;
            this.C_Speed.SelectedIndex = 0;
            this.C_Invert.SelectedIndex = 0;

            //D
            this.D_Gain.Value = 50;
            this.D_DynamicRange.Value = 40;
            this.D_Scale.Value = 3;
            this.D_BaseLine.Value = 0;
            this.D_WF.Value = 1;
            this.D_Angle.Value = 0;
            this.D_TimeScale.SelectedIndex = 0;
            this.D_Speed.SelectedIndex = 0;
            this.D_Steer.Value = 0;
            this.D_SamplingVolume.Value = 4;
            this.D_Invert.SelectedIndex = 0;
            this.D_Map.Value = 1;
            this.D_PseudocolorLevel.Value = 0;

            //M
            this.M_Gain.Value = 50;
            this.M_TimeScale.SelectedIndex = 0;
            this.M_Map.Value = 1;
            this.M_PseudocolorLevel.Value = 0;


            string probeName = ((ComboBoxItem)this.UseProbeName.SelectedValue).Content.ToString();
            string exameNameEN = "";
            foreach (var examInfo in _examNameList_byUseExamName)
            {
                if (((ComboBoxItem)this.UseExameName.SelectedValue).Content.ToString() == examInfo.ExamName_ZH)
                {
                    exameNameEN = examInfo.ExamName_EN;
                    break;
                }
            }

            string ParamRootPath = "../CIES_SDK/Param";

            currentCheckModeParamPath = ParamRootPath + "/" + probeName + "/" + exameNameEN;


            //设置当前参数
            int nRet = _demoServer.SetParamPath(currentCheckModeParamPath, mScanMode); //设置检查模式参数目录和当前模式
            canSetAlgorithParam = true;

            //B参数
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.All_Param;
            b_ImageParam_CLR.m_fGain = (float)this.Gain.Value;
            b_ImageParam_CLR.m_nDepth_level = (int)this.Depth.Value;
            b_ImageParam_CLR.m_fDR = (float)this.DynamicRange.Value;
            b_ImageParam_CLR.m_SRI_Level = (int)this.SRI.Value;
            b_ImageParam_CLR.m_nCorrelation_Level = (int)this.Correlation.Value;
            b_ImageParam_CLR.m_nHeight = ImageHeigh;
            b_ImageParam_CLR.m_nWidth = ImageWidth;
            b_ImageParam_CLR.m_fTGC[0] = (float)TGC1.Value;
            b_ImageParam_CLR.m_fTGC[1] = (float)TGC2.Value;
            b_ImageParam_CLR.m_fTGC[2] = (float)TGC3.Value;
            b_ImageParam_CLR.m_fTGC[3] = (float)TGC4.Value;
            b_ImageParam_CLR.m_fTGC[4] = (float)TGC5.Value;
            b_ImageParam_CLR.m_fTGC[5] = (float)TGC6.Value;

            b_ImageParam_CLR.m_nFocusArea = this.FocusArea.SelectedIndex;
            b_ImageParam_CLR.m_nFrequency = this.Frequency.SelectedIndex;
            b_ImageParam_CLR.m_nHarmonic = this.Harmonic.SelectedIndex;
            b_ImageParam_CLR.m_nGrayColorMap_level = (int)this.GrayLevel.Value;
            b_ImageParam_CLR.m_nPseudoColorMap_level = (int)this.PseudocolorLevel.Value;
            b_ImageParam_CLR.m_nNE = (int)this.NEONOFF.SelectedIndex;
            b_ImageParam_CLR.m_nNE_Theta = (int)this.NeAngle.Value;
            nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            //B超控件初始化

            if (_examModeConfigSetting.ProbeExamAHWProbeInfo.TryGetValue(ExamModeConfig.ExamModeConfigSetting.GetProbeAExamName(probeName, exameNameEN), out _current_probe_Info))
            {
                this.BAWBUtrs.Probe_type = _current_probe_Info.Probe_type;
                this.BAWBUtrs.Probe_RadiusCurvature = _current_probe_Info.m_fRadiusCurvature;
                this.BAWBUtrs.Probe_pith = _current_probe_Info.m_fPith;
                this.BAWBUtrs.Probe_element = _current_probe_Info.m_nFocusNum;
            }

            this.BAWBUtrs.ColorFrameControl.SetProbeInfo(_current_probe_Info);
            this.BAWBUtrs.DoppleSGControl.SetProbeInfo(_current_probe_Info);
            this.BAWBUtrs.MModeSLControl.SetProbeInfo(_current_probe_Info);

            this.BAWBUtrs.OpenNEFunction(false);
            this.BAWBUtrs.SetNEAngle(30);

            List<double> depthArray = new List<double>();
            foreach (var depth in _depthDictionary)
            {
                depthArray.Add(depth.Value);
            }
            this.BAWBUtrs.CalculateImageRealWidth();
            this.BAWBUtrs.ScaleMarksDepthList = depthArray;
            this.BAWBUtrs.ScaleMarksDepthLevel = 0;
            this.BAWBUtrs.ImageShowHeigth = ImageHeigh;
            this.BAWBUtrs.ImageShowWidth = ImageWidth;
            
            this.BAWBUtrs.ColorFrameControl.Visibility = Visibility.Collapsed;
            this.BAWBUtrs.ScaleMarksDepthLevel = (int)this.Depth.Value;
            this.BAWBUtrs.ColorFrameControl.SetDefaultDepthInfo(this.BAWBUtrs.ScaleMarksDepthList[this.BAWBUtrs.ScaleMarksDepthLevel] * 10 / 2);

            this.BAWBUtrs.ScaleControl.FocusDepthChangedEvent += ScaleControl_FocusDepthChangedEvent;

            RefreshB_FrequencyShowValue(this.Harmonic.SelectedIndex);
            RefreshFocusAreaShowValue();
            RefreshFocusArea();
            //B
            if (this.BAWBUtrs.Probe_type == 1)
            {
                this.NEONOFF.Visibility = Visibility.Visible;
                this.NeAngle.Visibility = Visibility.Visible;
            }
            else
            {
                this.NEONOFF.Visibility = Visibility.Collapsed;
                this.NeAngle.Visibility = Visibility.Collapsed;

                this.NeAngle.Minimum = _current_probe_Info.m_pB_NE_Theta_angle[0];
                this.NeAngle.Maximum = _current_probe_Info.m_pB_NE_Theta_angle[_current_probe_Info.m_pB_NE_Theta_angle.Length -1];
                this.NeAngle.TickFrequency = _current_probe_Info.m_pB_NE_Theta_angle[1] - _current_probe_Info.m_pB_NE_Theta_angle[0];
            }

            //C
            if (this.BAWBUtrs.Probe_type == 1)
            {
                this.BAWBUtrs.ColorFrameControl.SetQuadrangleAngle(0);
                this.C_Steer_StackPanel.Visibility = Visibility.Visible;
                this.C_Steer.Minimum = _current_probe_Info.m_pC_Tx_angle[_current_probe_Info.m_pC_Tx_angle.Length - 1];
                this.C_Steer.Maximum = _current_probe_Info.m_pC_Tx_angle[0];
                this.C_Steer.TickFrequency = _current_probe_Info.m_pC_Tx_angle[0] - _current_probe_Info.m_pC_Tx_angle[1];
            }
            else
            {
                this.C_Steer_StackPanel.Visibility = Visibility.Collapsed;
            }

            RefreshC_FrequencyShowValue();

            //D
            if (this.BAWBUtrs.Probe_type == 1)
            {
                //this.BAWBUtrs.ColorFrameControl.SetQuadrangleAngle(0);
                this.D_Steer_StackPanel.Visibility = Visibility.Visible;
                this.D_Steer.Minimum = _current_probe_Info.m_pD_Tx_angle[_current_probe_Info.m_pD_Tx_angle.Length - 1];
                this.D_Steer.Maximum = _current_probe_Info.m_pD_Tx_angle[0];
                this.D_Steer.TickFrequency = _current_probe_Info.m_pD_Tx_angle[0] - _current_probe_Info.m_pD_Tx_angle[1];
            }
            else
            {
                this.D_Steer_StackPanel.Visibility = Visibility.Collapsed;
            }
            this.DopplerCtrl.SetDepthList(depthArray);
            RefreshD_FrequencyShowValue();

            //M

            Click_B_Mode();
        }

        void ScaleControl_FocusDepthChangedEvent(double focusDepth)
        {
            this.FocusAreaNew.Text = focusDepth.ToString("F01") + "cm";
            //下发参数 转成 mm
            _demoServer.SetFocusArea((float)(focusDepth * 10));
            this.BAWBUtrs.ScaleControl.ResetFocusArea(focusDepth);
        }


        private void UseExameName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.UseExameName.SelectedValue == null)
            {
                return;
            }
            DoFreezeClick();

            canSetAlgorithParam = false;

            string probeName = ((ComboBoxItem)this.UseProbeName.SelectedValue).Content.ToString();
            string examname_EN = ((ComboBoxItem)this.UseExameName.SelectedValue).Tag.ToString();
            Probe_Info _probe_Info = new Probe_Info();
            if (_examModeConfigSetting.ProbeExamAHWProbeInfo.TryGetValue(ExamModeConfig.ExamModeConfigSetting.GetProbeAExamName(probeName, examname_EN), out _probe_Info))
            {
                _depthDictionary.Clear();
                for (int i = 0; i < _probe_Info.m_nShowDepthLevel; i++)
                {
                    _depthDictionary.Add(i, _probe_Info.m_pDepthList[i] / 10.0);
                }
                this.Depth.Minimum = 0;
                this.Depth.Maximum = _probe_Info.m_nShowDepthLevel - 1;
                this.Depth.Value = 2;
                //手动更新显示深度值
                RefreshDepthShowValue();

                NEVisibility = _probe_Info.m_bB_NE_deflectionflag ? Visibility.Visible : Visibility.Collapsed;
                    
            }

            ResetParam();

            DoUnFreezeClick();
        }


        public void DoFreezeClick() //手动触发冻结按钮的Click事件
        {
            if (this.FreezeOrUnFreeze.IsChecked != true)
            {
                this.FreezeOrUnFreeze.IsChecked = true;
                RoutedEventArgs ee = new RoutedEventArgs(Button.ClickEvent, this);
                this.FreezeOrUnFreeze.RaiseEvent(ee);
            }
        }

        public void DoUnFreezeClick()
        {
            if (this.FreezeOrUnFreeze.IsChecked != false)
            {
                this.FreezeOrUnFreeze.IsChecked = false;
                RoutedEventArgs ee = new RoutedEventArgs(Button.ClickEvent, this);
                this.FreezeOrUnFreeze.RaiseEvent(ee);
            }
        }
        //获取USB状态，设置USB断开和连接图标，USB断开后，会一直显示断开图标。USB连接后，会显示USB连接图片，在1s后消失。
        private TimeSpan _testUsbStart = TimeSpan.Zero;
        private bool _usbConn = false;
        public bool UsbConn
        {
            get { return _usbConn; }
            set
            {
                _usbConn = value;
                if (_usbConn)
                {
                    UsbConnState.Source = _bitmapImageSource[0];

                    if (this.FreezeOrUnFreeze.IsChecked != false)
                    {
                        this.FreezeOrUnFreeze.IsChecked = false;
                        RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                        this.FreezeOrUnFreeze.RaiseEvent(ee);
                    }
                }
                else
                {
                    UsbConnState.Source = _bitmapImageSource[1];
                }
            }
        }
        private bool _firstStartTestUsb = false;
        private int _testUsbStateFirstTime = 1000;
        private int _testUsbStateTime = 100;
        private bool _calTimeDiff = false;

        private void RefreshTestUsbStart()
        {
            _testUsbStart = TimeSpan.Zero;
        }


        private void TestUSBState()
        {
            if (_demoServer != null)
            {
                if (this.BAWBUtrs.FreezeState)
                {
                    return;
                }

                if (_testUsbStart == TimeSpan.Zero)
                {
                    _testUsbStart = new TimeSpan(DateTime.Now.Ticks);
                    _calTimeDiff = true;
                    return;
                }

                if (_calTimeDiff)
                {
                    TimeSpan diff = new TimeSpan(DateTime.Now.Ticks - _testUsbStart.Ticks);


                    if (_firstStartTestUsb)
                    {
                        if (diff.TotalMilliseconds < _testUsbStateFirstTime)
                        {
                            _calTimeDiff = false;
                            _firstStartTestUsb = false;
                            return;
                        }

                    }
                    else
                    {
                        if (diff.TotalMilliseconds < _testUsbStateTime)
                        {
                            _calTimeDiff = false;
                            return;
                        }
                    }
                }

                if (!_demoServer.IsUSBOpen())
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DoFreezeClick();
                        UsbConn = false;
                    }));

                }
            }
        }


        private void FreezeOrUnFreeze_Click(object sender, RoutedEventArgs e)
        {
            if (this.FreezeOrUnFreeze.IsChecked == true)
            {

                //连续冻结，只记录第一次冻结时的基线位置。
                if (!this.BAWBUtrs.FreezeState)
                {
                    _freezeDataBaseLine = DopplerCtrl.FlowScale.GetBaseLine();
                }

             
                this.BAWBUtrs.FreezeState = true;
                if (this.BAWBUtrs.FreezeState)
                {
                    if (pwState != D_PW_StateE.PW_D)
                    {
                        this.VideoPlay.Visibility = Visibility.Visible;
                        this.VideoPlay.IsChecked = false;
                        this.VideoPlay.Content = "播放";
                    }
                }
                else
                {
                    this.VideoPlay.Visibility = Visibility.Collapsed;
                }

                _demoServer.Freeze();
                this.FreezeOrUnFreeze.Content = "解冻";

                _readDataThreadFlag = false;
                DopplerCtrl.SetFreeze(true);
                if (pwState == D_PW_StateE.PW_D)
                {
                    DopplerCtrl.SetVideoState(true);
                }
                else
                {
                    DopplerCtrl.SetVideoState(false);
                }

                currentDataIndex = 0;
            }
            else
            {

                if (pwState == D_PW_StateE.PW_D)
                {
                    this.DopplerCtrl.ClearScreen();
                }

                if (this.VideoPlay.Visibility == Visibility.Visible)
                {
                    this.VideoPlay.Visibility = Visibility.Collapsed;
                    _readHistoryDataThreadFlag = false;
                }

                int nRet = 0;
                if (!_demoServer.IsRuning())
                {
                    nRet =_demoServer.StartImageEngine();
                }

                RefreshTestUsbStart();
                _demoServer.UnFreeze();

              

                this.BAWBUtrs.FreezeState = false;
                if (!_readDataThreadFlag)
                {
                    _readDataThreadFlag = true;
                    _ReadDataThread = new Thread(new ParameterizedThreadStart(GetDataTread));
                    _ReadDataThread.Start(this);
                }
                this.FreezeOrUnFreeze.Content = "冻结";

                DopplerCtrl.SetFreeze(false);
                DopplerCtrl.SetVideoState(false);

                if (!_usbConn)
                {
                    TestUSBState();
                }
            }

        }

        private void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (this.VideoPlay.IsChecked == true)
            {

                _readHistoryDataThreadFlag = true;
                _ReadHistoryDataThread = new Thread(new ParameterizedThreadStart(GetHistoryDataTread));
                _ReadHistoryDataThread.Start(this);

                this.VideoPlay.Content = "暂停";
            }
            else
            {
                _readHistoryDataThreadFlag = false;
                this.VideoPlay.Content = "播放";
            }
        }

        private void CloseSoftWare_Click(object sender, RoutedEventArgs e)
        {
            _readDataThreadFlag = false;
            _readHistoryDataThreadFlag = false;
            _demoServer.StopImageEngine();
            this.Close();
        }

        private void Click_B_Mode()
        {
            if (this.B.IsChecked.Value == false)
            {
                this.B.IsChecked = true;
                RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                this.B.RaiseEvent(ee);
            }
        }


        #region B Ctrl
        private void B_Click(object sender, RoutedEventArgs e)
        {
            if (this.B.IsChecked.HasValue && this.B.IsChecked.Value)
            {
                if (this.C.IsChecked.Value == true)
                {
                    this.C.IsChecked = false;
                }
                if (this.D.IsChecked.Value == true)
                {
                    this.D.IsChecked = false;
                }
                if (this.M.IsChecked.Value == true)
                {
                    this.M.IsChecked = false;
                }
                ExitPW();
                ExitMMode();
                mLastScanMode = CLScanMode.CLScanModeEnum.B;
                mScanMode = CLScanMode.CLScanModeEnum.B;
                //设置当前参数
                int nRet = _demoServer.SetParamPath(currentCheckModeParamPath, mScanMode); //设置检查模式参数目录和当前模式
                this.BAWBUtrs.ColorFrameControl.Visibility = Visibility.Collapsed;

            }
            else
            {
                this.B.IsChecked = true;
            }
            RefreshMapVisibility();
            RefreshMap();

        }


        private void Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.Gain_Param;
            b_ImageParam_CLR.m_fGain = (float)this.Gain.Value;

            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
            int imageWidth = _demoServer.GetImageWidthPixels();

        }

        private void RefreshDepthShowValue()
        {
            int key = (int)this.Depth.Value;
            double value = 0;
            bool bRet = _depthDictionary.TryGetValue(key, out value);
            if (bRet)
            {
                this.DepthValue.Text = value.ToString() + "cm";
                currentDepth = value;
            }
        }

        private void Depth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            RefreshDepthShowValue();
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.Depth_level_Param;
            b_ImageParam_CLR.m_nDepth_level = (int)this.Depth.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            this.BAWBUtrs.ScaleMarksDepthLevel = (int)this.Depth.Value;
            RefreshFocusAreaShowValue();
            RefreshFocusArea();
            if (mScanMode == CLScanMode.CLScanModeEnum.D_PW)
                this.BAWBUtrs.DoppleSGControl.SetImageWidthAHeightPixels(_demoServer.GetImageWidthPixels(), _demoServer.GetImageHeightPixels());
            if (mScanMode == CLScanMode.CLScanModeEnum.BM)
            {
                DopplerCtrl.SetDepthLevel(this.BAWBUtrs.ScaleMarksDepthLevel);
                this.BAWBUtrs.MModeSLControl.SetImageWidthAHeightPixels(_demoServer.GetImageWidthPixels(), _demoServer.GetImageHeightPixels());
            }
        }

        private void DynamicRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.DR_Param;
            b_ImageParam_CLR.m_fDR = (float)this.DynamicRange.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
        }

        private void SRI_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.SRI_level_Param;
            b_ImageParam_CLR.m_SRI_Level = (int)this.SRI.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

        }


        private void Correlation_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.Correlation_level_Param;
            b_ImageParam_CLR.m_nCorrelation_Level = (int)this.Correlation.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

        }

        private void GrayLevel_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.GrayColorMap_level_Param;
            b_ImageParam_CLR.m_nGrayColorMap_level = (int)this.GrayLevel.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
            RefreshMap();
        }

        private void PseudocolorLevel_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.PseudoColorMap_level_Param;
            b_ImageParam_CLR.m_nPseudoColorMap_level = (int)this.PseudocolorLevel.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
            RefreshMap();
        }

        private void Harmonic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.Harmonic_level_Param;
            b_ImageParam_CLR.m_nHarmonic = this.Harmonic.SelectedIndex;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            RefreshB_FrequencyShowValue(this.Harmonic.SelectedIndex);
            RefreshFocusAreaShowValue();
            RefreshFocusArea();
        }
        float[] focusArray = new float[8];//焦点1-6  全域的开始和结束焦点
        private void RefreshFocusAreaShowValue()
        {
            if (this.FocusArea == null)
                return;

            //获取新的焦点范围

            _demoServer.GetCurrentFocusArray(focusArray);
            for (int i = 0; i < 8; i++)
            {
                focusArray[i] = focusArray[i] / 10; //mm转换成cm
            }

            int j = 0;
            foreach (ComboBoxItem item in this.FocusArea.Items)
            {
                item.Content = focusArray[j++].ToString("F01") + "cm";
                if (j >= 6)
                    break;
            }
            if (this.BAWBUtrs != null)
                this.BAWBUtrs.ScaleControl.MaxFocusAreaStartDepth = focusArray[0];

        }

        private void RefreshFocusArea()
        {
            if (this.FocusArea == null || this.BAWBUtrs == null)
                return;
            //深度标尺焦点设置
            bool UseFocusAreaFlag = false;
            bool UseFocusPointFlag = false;
            int FocusAreaIndex = this.FocusArea.SelectedIndex;
            if (FocusAreaIndex == 6)
            {
                UseFocusAreaFlag = true;
                UseFocusPointFlag = false;
            }
            else
            {
                UseFocusAreaFlag = false;
                UseFocusPointFlag = true;
            }

            float startDepth = (float)focusArray[FocusAreaIndex];
            float endDepth = (float)focusArray[FocusAreaIndex];

            if (UseFocusAreaFlag)
            {
                startDepth = (float)focusArray[6];
                endDepth = (float)focusArray[7];
            }


            this.BAWBUtrs.FocusAreaStartDepth = startDepth;
            this.BAWBUtrs.FocusAreaEndDepth = endDepth;
            this.BAWBUtrs.UseFocusAreaFlag = UseFocusAreaFlag;
            this.BAWBUtrs.UseFocusPointFlag = UseFocusPointFlag;
        }


        private void FocusArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.FocusArea_level_Param;
            b_ImageParam_CLR.m_nFocusArea = this.FocusArea.SelectedIndex;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
            RefreshFocusArea();
            if (this.FocusAreaNew != null)
                this.FocusAreaNew.Text = "";
            if (this.BAWBUtrs != null)
                this.BAWBUtrs.ScaleControl.ResetFocusArea(0);
        }

        private void RefreshB_FrequencyShowValue(int Harmonic)
        {
            if (this.Frequency == null)
                return;
            double[] B_Frequency = new double[3];
            for (int i = 0; i < 3; i++)
            {
                if (Harmonic == 0)
                    B_Frequency[i] = _current_probe_Info.m_fB_fund_Tx_freq[i];
                else
                    B_Frequency[i] = _current_probe_Info.m_fB_harm_Tx_freq[i];
            }

            int j = 0;
            foreach (ComboBoxItem item in this.Frequency.Items)
            {
                item.Content = B_Frequency[j++].ToString("F01") + "M";
            }
        }

        private void Frequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.Frequency_level_Param;
            b_ImageParam_CLR.m_nFrequency = this.Frequency.SelectedIndex;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
        }

        private void TGC_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.TGC_Param;
            b_ImageParam_CLR.m_fTGC[0] = (float)TGC1.Value;

            b_ImageParam_CLR.m_fTGC[1] = (float)TGC2.Value;

            b_ImageParam_CLR.m_fTGC[2] = (float)TGC3.Value;

            b_ImageParam_CLR.m_fTGC[3] = (float)TGC4.Value;

            b_ImageParam_CLR.m_fTGC[4] = (float)TGC5.Value;

            b_ImageParam_CLR.m_fTGC[5] = (float)TGC6.Value;

            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

        }

        private void AutoTGC_Click(object sender, RoutedEventArgs e)
        {
            float[] TGCAuto = new float[6];
            _demoServer.AutomaticTGC(TGCAuto);

            this.TGC1.Value = TGCAuto[0];
            this.TGC2.Value = TGCAuto[1];
            this.TGC3.Value = TGCAuto[2];
            this.TGC4.Value = TGCAuto[3];
            this.TGC5.Value = TGCAuto[4];
            this.TGC6.Value = TGCAuto[5];

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.TGC_Param;
            b_ImageParam_CLR.m_fTGC[0] = (float)TGC1.Value;

            b_ImageParam_CLR.m_fTGC[1] = (float)TGC2.Value;

            b_ImageParam_CLR.m_fTGC[2] = (float)TGC3.Value;

            b_ImageParam_CLR.m_fTGC[3] = (float)TGC4.Value;

            b_ImageParam_CLR.m_fTGC[4] = (float)TGC5.Value;

            b_ImageParam_CLR.m_fTGC[5] = (float)TGC6.Value;

            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
        }

        private void NEONOFF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.NE_Param;
            b_ImageParam_CLR.m_nNE = this.NEONOFF.SelectedIndex;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            if(BAWBUtrs != null)
                BAWBUtrs.OpenNEFunction(b_ImageParam_CLR.m_nNE == 0?false : true);

        }

        private void NeAngle_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)B_ImageParam_CLR.B_ImageParamType.NE_Theta_Param;
            b_ImageParam_CLR.m_nNE_Theta = (int)this.NeAngle.Value;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
            if (BAWBUtrs != null)
                BAWBUtrs.SetNEAngle(b_ImageParam_CLR.m_nNE_Theta);

        }

        #endregion

        #region C Ctrl

        private Dictionary<int, double> _c_Scale_Dictionary = new Dictionary<int, double>();

        private void C_Click(object sender, RoutedEventArgs e)
        {
            CWT.Visibility = Visibility.Visible;
            if (C.IsChecked == true)
            {
                MainTabControl.SelectedItem = TabItemC;
            }
            
            if (this.C.IsChecked.HasValue && this.C.IsChecked.Value)
            {
                if (this.B.IsChecked.HasValue && this.B.IsChecked.Value)
                {
                    this.B.IsChecked = false;
                }
                if (this.D.IsChecked.Value == true)
                {
                    this.D.IsChecked = false;
                }
                if (this.M.IsChecked.Value == true)
                {
                    this.M.IsChecked = false;
                }
                ExitPW();
                ExitMMode();

                mLastScanMode = mScanMode;
                mScanMode = CLScanMode.CLScanModeEnum.BC;

                //设置当前参数
                int nRet = _demoServer.SetParamPath(currentCheckModeParamPath, mScanMode); //设置检查模式参数目录和当前模式
                                                                                           //C参数
                C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
                c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.All_Param;
                c_ImageParam_CLR.m_fGain = (float)this.C_Gain.Value;
                c_ImageParam_CLR.m_nWallFilter_level = (int)this.C_WF.Value;
                c_ImageParam_CLR.m_nColorPriority_level = (int)this.C_Priority.Value;
                c_ImageParam_CLR.m_nFrameCorrelation_level = (int)this.C_Correlation.Value;
                c_ImageParam_CLR.m_nColorMap_mode = (byte)this.C_Mode.SelectedIndex;
                c_ImageParam_CLR.m_nColorMap_level = (byte)this.C_Map.Value;
                c_ImageParam_CLR.m_nColorMap_inversion = (byte)this.C_Invert.SelectedIndex;
                c_ImageParam_CLR.m_nPRF_Level = (int)this.C_Scale.Value;
                c_ImageParam_CLR.m_fTheta = (int)this.C_Steer.Value;
                c_ImageParam_CLR.m_bUse_B_BC_Mode = false;
                c_ImageParam_CLR.m_nSpeed = (byte)this.C_Speed.SelectedIndex;

                nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);

                RefreshMapVisibility();
                RefreshCScaleShowValue();
                RefreshScale();
                RefreshMap();

                this.BAWBUtrs.ColorFrameControl.Visibility = Visibility.Visible;

            }
            else
            {
                this.B.IsChecked = true;
                RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                this.B.RaiseEvent(ee);

            }

        }

        private bool DoubleValueEquals(double[] doubleV1, double[] doubleV2)
        {
            if (doubleV1.Length != doubleV2.Length)
            {
                return false;
            }
            int len = doubleV2.Length;

            for (int i = 0; i < len; i++)
            {
                if (doubleV1[i] != doubleV2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void DoubleValueCopy(double[] doubleV1, double[] doubleV2)
        {
            int len1 = doubleV1.Length;
            int len2 = doubleV2.Length;
            int len = 0;
            if (len1 >= len2)
                len = len2;
            else
                len = len1;

            for (int i = 0; i < len; i++)
            {
                doubleV1[i] = doubleV2[i];
            }
        }

        private double _soundVelocity = 1540 * 100;
        private double _angle = 0;
        private double _emissionFrequency = 5 * 1000000;
        private double GetFlow(double prf, double _frequency)
        {
            return (prf * _soundVelocity) / (2 * _frequency * Math.Cos(_angle * Math.PI / 180));

        }

        private void GetFlowList(double[] C_FlowList, double[] C_PRFList)
        {
            for (int i = 0; i < C_PRFList.Length; i++)
            {
                C_FlowList[i] = GetFlow(C_PRFList[i] * 1000, _emissionFrequency) / 2;
            }

        }


        /// <summary>
        /// 重置C PRF数值
        /// </summary>
        /// <param name="SelectNearestValue"></param>
        private void ReInit_C_RPFValue(bool SelectNearestValue)
        {
            _c_Scale_Dictionary.Clear();
            double[] C_FlowListT = new double[8];
            GetFlowList(C_FlowListT, C_PRFList);

            for (int i = 0; i < 8; i++)
            {
                _c_Scale_Dictionary.Add(i, C_FlowListT[i]);
            }

            if (this.C_ScaleValue.Text == "")
                RefreshCScaleShowValue();

        }

        private double[] C_PRFList = new double[8];
        private bool Get_C_PRFList = false;
        private double _C_fTxFrequency = 0;
        void ColorFrameControl_QuadrangleFrameChangedEvent(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SX, double SY, double SWidthLen, double SHeightLen, int LDAngle)
        {
            nIWidthP = _demoServer.GetImageWidthPixels();
            nIHeightP = _demoServer.GetImageHeightPixels();
            _demoServer.SetCSamplingFrameParam_Quadrangle(Depth, this.BAWBUtrs.ImageShowWidth, BAWBUtrs.ImageShowHeigth, nIWidthP, nIHeightP, SX, SY, SWidthLen, SHeightLen, LDAngle);
            System.Console.WriteLine("ColorFrameControl_QuadrangleFrameChangedEvent");

            RefresC_Quadrangle_PRFList(Depth, nIHeightP, SY, SHeightLen, LDAngle);

            RefreshMap();

            if (mScanMode == CLScanMode.CLScanModeEnum.D_PW && (this.BAWBUtrs.Probe_type == 1))
            {
                var thread = new Thread((ThreadStart)delegate
                {

                    Thread.Sleep(1000);
                    float svx = 0;
                    float svy = 0;
                    float svH = 0;
                    int svAngleLevel = 0;
                    int nRet = _demoServer.AutomaticSV(ref svx, ref svy, ref svH, ref svAngleLevel);
                    if (nRet == 0)
                    {
                        float svf = svH * Depth / (float)BAWBUtrs.ImageShowHeigth;
                        int sv = (int)(svf + 0.5);
                        if (sv < 1)
                            sv = 1;
                        if (sv > 10)
                            sv = 10;

                        if (svx <= 0 || svx >= nIWidthP)
                            return;
                        if (svy <= 0 || svy >= BAWBUtrs.ImageShowHeigth)
                            return;

                        //转换到屏幕坐标
                        svx += (float)(BAWBUtrs.ImageShowWidth - nIWidthP) / 2;

                        if (Application.Current != null)
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.BAWBUtrs.DoppleSGControl.ResetDopplerSamplingGateCenter(new System.Windows.Point() { X = svx, Y = svy });
                                this.BAWBUtrs.DoppleSGControl.SetSamplingVolume(sv);
                            //重置取样容积
                            {
                                    this.D_SamplingVolume.Value = sv - 1;
                                }
                            //重置偏转角度
                            if (svAngleLevel >= 0)
                                {

                                    this.D_Steer.Value = _current_probe_Info.m_pD_Tx_angle[svAngleLevel];

                                }

                            }));

                    }

                });
                thread.Start();

            }

        }

        private int Depth_Quadrangle = 0;
        private double nIHeightP_Quadrangle = 0;
        private double SY_Quadrangle = 0;
        private double SHeightLen_Quadrangle = 0;
        private int LDAngle_Quadrangle = 0;

        private void RefresC_Quadrangle_PRFList(int Depth, double nIHeightP, double SY, double SHeightLen, int LDAngle)
        {
            Depth_Quadrangle = Depth;
            nIHeightP_Quadrangle = nIHeightP;
            SY_Quadrangle = SY;
            SHeightLen_Quadrangle = SHeightLen;
            LDAngle_Quadrangle = LDAngle;

            double[] C_PRFListT = new double[8];
            double fTxFrequency = 0;
            int nRet = _demoServer.Get_C_PRFList_Quadrangle(Depth, nIHeightP, SY, SHeightLen, LDAngle, C_PRFListT, ref fTxFrequency);
            if (nRet == 0)
            {
                if (!DoubleValueEquals(C_PRFList, C_PRFListT) || _C_fTxFrequency != fTxFrequency)
                {
                    DoubleValueCopy(C_PRFList, C_PRFListT);
                    _C_fTxFrequency = fTxFrequency;
                    _emissionFrequency = fTxFrequency * 1000000;
                    Get_C_PRFList = true;
                    ReInit_C_RPFValue(true);
                }
            }
            else
            {
                Get_C_PRFList = false;
            }
        }

        void ColorFrameControl_SectorFrameChangedEvent(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SCToPCCP, double SHeight, double SectorAngle, double SectorCenterAngle)
        {
            nIWidthP = _demoServer.GetImageWidthPixels();
            nIHeightP = _demoServer.GetImageHeightPixels();
            _demoServer.SetCSamplingFrameParam_Sector(Depth, this.BAWBUtrs.ImageShowWidth, BAWBUtrs.ImageShowHeigth, nIWidthP, nIHeightP, SCToPCCP, SHeight, SectorAngle, SectorCenterAngle);
            System.Console.WriteLine("ColorFrameControl_SectorFrameChangedEvent");
            RefresC_Sector_PRFList(Depth, nIHeightP, SCToPCCP, SHeight);

            RefreshMap();
        }

        private int Depth_Sector = 0;
        private double nIHeightP_Sector = 0;
        private double SCToPCCP_Sector = 0;
        private double SHeight_Sector = 0;

        private void RefresC_Sector_PRFList(int Depth, double nIHeightP, double SCToPCCP, double SHeight)
        {
            Depth_Sector = Depth;
            nIHeightP_Sector = nIHeightP;
            SCToPCCP_Sector = SCToPCCP;
            SHeight_Sector = SHeight;

            double[] C_PRFListT = new double[8];
            double fTxFrequency = 0;
            int nRet = _demoServer.Get_C_PRFList_Sector(Depth, nIHeightP, SCToPCCP, SHeight, C_PRFListT, ref fTxFrequency);
            if (nRet == 0)
            {
                if (!DoubleValueEquals(C_PRFList, C_PRFListT) || _C_fTxFrequency != fTxFrequency)
                {
                    DoubleValueCopy(C_PRFList, C_PRFListT);
                    _C_fTxFrequency = fTxFrequency;
                    _emissionFrequency = fTxFrequency * 1000000;
                    Get_C_PRFList = true;
                    ReInit_C_RPFValue(true);
                }
                else
                {

                }
            }
            else
            {
                Get_C_PRFList = false;
            }
        }


        private void C_Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.Gain_Param;
            c_ImageParam_CLR.m_fGain = (float)this.C_Gain.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }

        private void C_WF_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.WallFilter_level_Param;
            c_ImageParam_CLR.m_nWallFilter_level = (int)this.C_WF.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }

        private void C_Correlation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.FrameCorrelation_level_Param;
            c_ImageParam_CLR.m_nColorPriority_level = (int)this.C_Correlation.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }
        private void RefreshCScaleShowValue()
        {
            int key = (int)this.C_Scale.Value;
            double value = 0;
            bool bRet = _c_Scale_Dictionary.TryGetValue(key, out value);
            if (bRet)
            {
                this.C_ScaleValue.Text = value.ToString("F00") + "cm/s";
            }
        }
        private void C_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.PRF_Level_Param;
            c_ImageParam_CLR.m_nPRF_Level = (int)this.C_Scale.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
            RefreshCScaleShowValue();

            RefreshScale();
        }

        private void C_Steer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.Theta_Param;
            c_ImageParam_CLR.m_fTheta = (float)this.C_Steer.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);

            this.BAWBUtrs.ColorFrameControl.SetQuadrangleAngle(this.C_Steer.Value);
        }

        private void C_Priority_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.ColorPriority_level_Param;
            c_ImageParam_CLR.m_nColorPriority_level = (int)this.C_Priority.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }

        private void C_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.Color_Mode_Param;
            c_ImageParam_CLR.m_nColorMap_mode = (int)this.C_Mode.SelectedIndex;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }

        private void C_Map_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.ColorMap_level_Param;
            c_ImageParam_CLR.m_nColorMap_level = (int)this.C_Map.Value;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
            RefreshMap();
        }

        private void RefreshC_FrequencyShowValue()
        {
            if (this.C_Speed == null)
                return;
            double[] C_Frequency = new double[2];
            for (int i = 0; i < 2; i++)
            {
                C_Frequency[i] = _current_probe_Info.m_fC_Tx_freq[i];
            }

            int j = 0;
            foreach (ComboBoxItem item in this.C_Speed.Items)
            {
                item.Content = C_Frequency[j++].ToString("F01") + "M";
            }
        }


        private void C_Speed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.Speed_Param;
            c_ImageParam_CLR.m_nSpeed = this.C_Speed.SelectedIndex;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);

        }

        private void C_Invert_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!canSetAlgorithParam)
                return;
            C_ImageParam_CLR c_ImageParam_CLR = new C_ImageParam_CLR();
            c_ImageParam_CLR.m_C_ImageParamType = (int)C_ImageParam_CLR.C_ImageParamType.ColorMap_Inversion_Param;
            c_ImageParam_CLR.m_nColorMap_inversion = this.C_Invert.SelectedIndex;

            int nRet = _demoServer.SetImageParam_C(c_ImageParam_CLR);
        }

        #endregion

        #region D Ctrl

        private int[] DopplerImage_SourceData = null;
        private int dopplerImageNum = 2;
        private int dopplerDataNum = 0;


        #region D PW 模式切换


        private D_PW_StateE pwState = D_PW_StateE.End_PW;
        private D_PW_StateE pwStateLast = D_PW_StateE.End_PW;
        private D_PW_ModeE pwMode_Current = D_PW_ModeE.PW_BD;
        private void SetServerParamAScanMode_By_D_PW_State()
        {
            if (pwState == D_PW_StateE.PW_B)
            {
                _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.B);
            }
            else if (pwState == D_PW_StateE.PW_BC)
            {
                _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.BC);
            }
            else if (pwState == D_PW_StateE.End_PW)
            {
                if (pwMode_Current == D_PW_ModeE.PW_BCD)
                {
                    _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.BC);
                }
                else if (pwMode_Current == D_PW_ModeE.PW_BD)
                {
                    _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.B);
                }
            }
            else if (pwState == D_PW_StateE.PW_D)
            {
                _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.D_PW);
            }

        }

        private void EnterPW(CLScanMode.CLScanModeEnum mLastScanMode)
        {
            pwStateLast = pwState;
            if (mLastScanMode == CLScanMode.CLScanModeEnum.BC)
            {
                pwState = D_PW_StateE.PW_BC;
                pwMode_Current = D_PW_ModeE.PW_BCD;
                this.BAWBUtrs.SetBCPWMode(true);
                this.BAWBUtrs.ActivateColorSamplingFrame(true);
            }
            else
            {
                pwState = D_PW_StateE.PW_B;
                pwMode_Current = D_PW_ModeE.PW_BD;
            }


            this.DopplerCtrl.Visibility = Visibility.Visible;

            this.BAWBUtrs.DoppleSGCanUse = true;
            this.BAWBUtrs.DoppleSGVisibility = Visibility.Visible;

            this.BAWBUtrs.MModeSLCanUse = false;
            this.BAWBUtrs.MModeSLVisibility = Visibility.Collapsed;

            this.BAWBUtrs.SetUsePWState(pwState);
            this.DopplerCtrl.SetUsePWState(pwState);

            int h1 = this.BAWBUtrs.ImageShowHeigth;
            int w1 = this.BAWBUtrs.ImageShowWidth;

            //设置新的宽高
            this.BAWBUtrs.ImageShowHeigth = h1 / 2;
            this.BAWBUtrs.ImageShowWidth = ImageWidth;

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)(B_ImageParam_CLR.B_ImageParamType.Height_Param | B_ImageParam_CLR.B_ImageParamType.Width_Param);

            b_ImageParam_CLR.m_nHeight = this.BAWBUtrs.ImageShowHeigth;
            b_ImageParam_CLR.m_nWidth = this.BAWBUtrs.ImageShowWidth;

            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            //设置D PW控件大小
            DopplerCtrl.ImageShowHeigth = this.BAWBUtrs.ImageShowHeigth;
            DopplerCtrl.ImageShowWidth = this.BAWBUtrs.ImageShowWidth;

            if (dopplerDataNum != ImageWidth * dopplerImageNum)
            {
                dopplerDataNum = ImageWidth * dopplerImageNum;
            }

            this.BAWBUtrs.DoppleSGControl.SetImageWidthAHeightPixels(_demoServer.GetImageWidthPixels(), _demoServer.GetImageHeightPixels());

            //设置D PW 其他参数
            D_PW_ImageParam_CLR d_PW_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_PW_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.All_Param;
            d_PW_ImageParam_CLR.m_nHeight = DopplerCtrl.ImageShowHeigth;
            d_PW_ImageParam_CLR.m_nWidth = DopplerCtrl.ImageShowWidth;
            d_PW_ImageParam_CLR.m_nPseudoColorMap_level = (int)this.D_PseudocolorLevel.Value;
            d_PW_ImageParam_CLR.m_nGrayColorMap_level = (int)this.D_Map.Value;
            d_PW_ImageParam_CLR.m_nSpeed = this.D_Speed.SelectedIndex;
            d_PW_ImageParam_CLR.m_nInversion = this.D_Invert.SelectedIndex;
            d_PW_ImageParam_CLR.m_nSamplingVolume = (int)this.D_SamplingVolume.Value;
            d_PW_ImageParam_CLR.m_nBaseLineLevel = (int)this.D_BaseLine.Value;
            d_PW_ImageParam_CLR.m_nWall_level = (int)this.D_WF.Value;
            d_PW_ImageParam_CLR.m_fFrequency = m_fFrequency;
            d_PW_ImageParam_CLR.m_fDR = (int)this.D_DynamicRange.Value;
            d_PW_ImageParam_CLR.m_fGain = (int)this.D_Gain.Value;
            d_PW_ImageParam_CLR.m_prf_rate = (int)this.D_Scale.Value;
            d_PW_ImageParam_CLR.m_nHeight = DopplerCtrl.ImageShowHeigth;
            d_PW_ImageParam_CLR.m_nWidth = DopplerCtrl.ImageShowWidth;
            int time = 0;
            switch (D_TimeScale.SelectedIndex)
            {
                case 0:
                    time = 4;
                    break;
                case 1:
                    time = 6;
                    break;
                case 2:
                    time = 8;
                    break;
            }
            d_PW_ImageParam_CLR.m_fTime = time;
            _demoServer.SetImageParam_D_PW(d_PW_ImageParam_CLR);

            this.DopplerCtrl.SetBaseLine((int)this.D_BaseLine.Value);
            this.DopplerCtrl.SetShowTime(time);
            this.DopplerCtrl.SetVascularAngle((int)this.D_Angle.Value);
            this.DopplerCtrl.SetLaunchDeflection((int)this.D_Steer.Value);

            this.BAWBUtrs.DoppleSGControl.SetVascularAngle((int)this.D_Angle.Value);
            this.BAWBUtrs.DoppleSGControl.SetDefaultShowDepth(2);
            this.BAWBUtrs.DoppleSGControl.SetMaxDepth((float)this.BAWBUtrs.ScaleMarksDepth);
            this.BAWBUtrs.DoppleSGControl.SetSamplingVolume((int)this.D_SamplingVolume.Value);
            this.BAWBUtrs.DoppleSGControl.SetLaunchDeflection((int)this.D_Steer.Value);
            
            SetServerParamAScanMode_By_D_PW_State();
            this.DopplerCtrl.SetShowTime(time);
            this.DopplerCtrl.ClearScreen();

            _demoServer.GetColorMap_D(_colorIntBar_DM);
            SetDMGrapMap(_colorIntBar_DM);

            this.Update.Visibility = Visibility.Visible;
        }



        private void ExitPW()
        {
            if (pwState != D_PW_StateE.End_PW)
            {
                pwState = D_PW_StateE.End_PW;


                this.DopplerCtrl.Visibility = System.Windows.Visibility.Collapsed;
                this.BAWBUtrs.DoppleSGCanUse = false;
                this.BAWBUtrs.DoppleSGVisibility = Visibility.Collapsed;
                this.BAWBUtrs.SetBCPWMode(false);
                this.BAWBUtrs.SetUsePWState(pwState);
                this.DopplerCtrl.SetUsePWState(pwState);

                this.BAWBUtrs.ClearImageShow();

                //设置新的宽高
                this.BAWBUtrs.ImageShowHeigth = ImageHeigh;
                this.BAWBUtrs.ImageShowWidth = ImageWidth;

                B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
                b_ImageParam_CLR.m_B_ImageParamType = (int)(B_ImageParam_CLR.B_ImageParamType.Height_Param | B_ImageParam_CLR.B_ImageParamType.Width_Param);
                b_ImageParam_CLR.m_nHeight = this.BAWBUtrs.ImageShowHeigth;
                b_ImageParam_CLR.m_nWidth = this.BAWBUtrs.ImageShowWidth;
                int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

                SetServerParamAScanMode_By_D_PW_State();
            }
            this.Update.Visibility = Visibility.Collapsed;
        }

        private void Enter_D_PW_BC()
        {
            pwStateLast = pwState;
            pwState = D_PW_StateE.PW_BC;

            this.BAWBUtrs.ColorFrameControl.ReCalculateFrameChanged();

            this.BAWBUtrs.SetUsePWState(pwState);
            this.DopplerCtrl.SetUsePWState(pwState);

            DopplerCtrl.SetVideoState(false);

            SetServerParamAScanMode_By_D_PW_State();

        }

        private void Enter_D_PW_B()
        {
            pwStateLast = pwState;
            pwState = D_PW_StateE.PW_B;

            this.BAWBUtrs.SetUsePWState(pwState);
            this.DopplerCtrl.SetUsePWState(pwState);

            DopplerCtrl.SetVideoState(false);

            SetServerParamAScanMode_By_D_PW_State();

        }

        private void Enter_D_PW_D()
        {

            if (!this.BAWBUtrs.FreezeState)
            {
                this.DopplerCtrl.ClearScreen();
            }


            pwStateLast = pwState;
            pwState = D_PW_StateE.PW_D;

            //ClearASetUnChecked_MeasureAndNote();

            SetServerParamAScanMode_By_D_PW_State();
            this.BAWBUtrs.DoppleSGControl.ReCalculateSamplingGateChanged();
            this.BAWBUtrs.SetUsePWState(pwState);
            this.DopplerCtrl.SetUsePWState(pwState);

            var thread = new Thread((ThreadStart)delegate
            {
                int count = 0;
                Thread.Sleep(1500);
                while (true)
                {
                    int PRF_level = 0;
                    int Baseline_level = 0;
                    int nRet = _demoServer.AutoPRF(ref PRF_level, ref Baseline_level);
                    if (nRet == 0)
                    {

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //重置取样PRF
                            {
                               
                                int prfL = PRF_level;
                                switch (PRF_level)
                                {
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        prfL = PRF_level;
                                        break;
                                    case 6:
                                        prfL = 5;
                                        break;
                                    case 8:
                                        prfL = 6;
                                        break;
                                    case 12:
                                        prfL = 7;
                                        break;
                                    case 16:
                                        prfL = 8;
                                        break;

                                }

                                this.D_Scale.Value = prfL;

                            }


                            //重置基线
                            {
                               

                                if (Baseline_level < -3)
                                    Baseline_level = -3;
                                if (Baseline_level > 3)
                                    Baseline_level = 3;

                                this.D_BaseLine.Value = Baseline_level;

                            }


                        }));
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        count++;
                    }
                    if (count > 20)
                    {
                        break;
                    }
                }


            });
            thread.Start();
        }


        void BAWBUtrs_ActivateBEvent()
        {
            if (mScanMode == CLScanMode.CLScanModeEnum.D_PW)
            {
                if (pwMode_Current == D_PW_ModeE.PW_BCD)
                    Enter_D_PW_BC();
                else if (pwMode_Current == D_PW_ModeE.PW_BD)
                    Enter_D_PW_B();
            }

        }

        void DopplerCtrl_ActivatePWDORMEvent()
        {
            if (mScanMode == CLScanMode.CLScanModeEnum.D_PW)
            {
                Enter_D_PW_D();
            }
        }

        #endregion




        private Dictionary<int, double> _d_Scale_Dictionary = new Dictionary<int, double>();

        private void D_Click(object sender, RoutedEventArgs e)
        {
            if (this.D.IsChecked.HasValue && this.D.IsChecked.Value)
            {
                if (this.B.IsChecked.HasValue && this.B.IsChecked.Value)
                {
                    this.B.IsChecked = false;
                }
                if (this.C.IsChecked.Value == true)
                {
                    this.C.IsChecked = false;
                }
                if (this.M.IsChecked.Value == true)
                {
                    this.M.IsChecked = false;
                }
                ExitMMode();
                mLastScanMode = mScanMode;
                mScanMode = CLScanMode.CLScanModeEnum.D_PW;
                EnterPW(mLastScanMode);
                RefreshUpdateText(mLastScanMode);
                RefreshUpdateTextState(0);
                RefreshMapVisibility();
                RefreshMap();
            }
            else
            {
                if(mLastScanMode == CLScanMode.CLScanModeEnum.BC)
                {
                    this.C.IsChecked = true;
                    RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                    this.C.RaiseEvent(ee);
                }
                else if (mLastScanMode == CLScanMode.CLScanModeEnum.B)
                {
                    this.B.IsChecked = true;
                    RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                    this.B.RaiseEvent(ee);
                }
            }
        }

        private void Set_D_ImageParam_Frequency(float fFrequency)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Frequency_Param;
            d_ImageParam_CLR.m_fFrequency = fFrequency;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void ReInit_D_RPFValue(bool SelectNearestValue)
        {
            _d_Scale_Dictionary.Clear();
            for (int i = 0; i < 8; i++)
            {
                _d_Scale_Dictionary.Add(i, D_PRFList[i]);
            }

            if (this.D_ScaleValue.Text == "")
            {
                double D_ScaleValue = RefreshDScaleShowValue();
                this.DopplerCtrl.SetFlowParam((float)D_ScaleValue * 1000, 1540 * 100, m_fFrequency * 1000000);
            }
        }

        private double[] D_PRFList = new double[8];
        private bool Get_D_PRFList = false;
        private float m_fFrequency = 5;
        void DoppleSGControl_SamplingGateLineChangedEvent(float SGX, float SGY, float depth, float imageWidth, float imageHight, int launchDeflectionAngle, int samplingVolume)
        {
            _demoServer.SetD_PWSamplingGateParam_Line(SGX, SGY, depth, imageWidth, imageHight, launchDeflectionAngle, samplingVolume);

            RefresD_SamplingGateLine_PRFList(SGY, depth, imageHight, launchDeflectionAngle);

        }
        float SGY_SGLine = 0;
        float depth_SGLine = 0;
        float imageHight_SGLine = 0;
        int launchDeflectionAngle_SGLine = 0;
        private void RefresD_SamplingGateLine_PRFList(float SGY, float depth, float imageHight, int launchDeflectionAngle)
        {
            SGY_SGLine = SGY;
            depth_SGLine = depth;
            imageHight_SGLine = imageHight;
            launchDeflectionAngle_SGLine = launchDeflectionAngle;

            double[] D_PRFListT = new double[8];
            double fFrequencyT = 0;
            int nRet = _demoServer.Get_D_PRFList_Line(SGY, depth, imageHight, launchDeflectionAngle, D_PRFListT, ref fFrequencyT);
            if (nRet == 0)
            {
                bool dFrequencyDiff = false;
                if (fFrequencyT != m_fFrequency)
                {
                    dFrequencyDiff = true;
                    m_fFrequency = (float)fFrequencyT;
                    Set_D_ImageParam_Frequency(m_fFrequency);
                    _emissionFrequency = m_fFrequency * 1000000;
                    this.DopplerCtrl.SetEmissionFrequency((float)_emissionFrequency);

                }
                if (!DoubleValueEquals(D_PRFList, D_PRFListT) || dFrequencyDiff)
                {
                    DoubleValueCopy(D_PRFList, D_PRFListT);

                    Get_D_PRFList = true;
                    ReInit_D_RPFValue(true);
                }
            }
            else
            {
                Get_D_PRFList = false;
            }
        }

        void DoppleSGControl_SamplingGateConvexChangedEvent(float SGX, float SGY, float depth, float imageWidth, float imageHight, float SectorCenterAngle, int samplingVolume)
        {
            _demoServer.SetD_PWSamplingGateParam_Convex(SGX, SGY, depth, imageWidth, imageHight, SectorCenterAngle, samplingVolume);

            RefresD_SamplingGateConvex_PRFList(SGX, SGY, depth, imageWidth, imageHight);

        }
        float SGX_SGConvex = 0;
        float SGY_SGConvex = 0;
        float depth_SGConvex = 0;
        float imageWidth_SGConvex = 0;
        float imageHight_SGConvex = 0;
        private void RefresD_SamplingGateConvex_PRFList(float SGX, float SGY, float depth, float imageWidth, float imageHight)
        {
            SGX_SGConvex = SGX;
            SGY_SGConvex = SGY;
            depth_SGConvex = depth;
            imageWidth_SGConvex = imageWidth;
            imageHight_SGConvex = imageHight;
            double[] D_PRFListT = new double[8];
            double fFrequencyT = 0;
            int nRet = _demoServer.Get_D_PRFList_Convex(SGX, SGY, depth, imageWidth, imageHight, D_PRFListT, ref fFrequencyT);
            if (nRet == 0)
            {
                bool dFrequencyDiff = false;
                if (fFrequencyT != m_fFrequency)
                {
                    dFrequencyDiff = true;
                    m_fFrequency = (float)fFrequencyT;
                    Set_D_ImageParam_Frequency(m_fFrequency);
                    _emissionFrequency = m_fFrequency * 1000000;
                    this.DopplerCtrl.SetEmissionFrequency((float)_emissionFrequency);
                }
                if (!DoubleValueEquals(D_PRFList, D_PRFListT) || dFrequencyDiff)
                {
                    DoubleValueCopy(D_PRFList, D_PRFListT);

                    Get_D_PRFList = true;
                    ReInit_D_RPFValue(true);
                }
            }
            else
            {
                Get_D_PRFList = false;
            }
        }

        void DoppleSGControl_SamplingGatePAChangedEvent(float SGX, float SGY, float depth, float imageWidth, float imageHight, float SectorCenterAngle, int samplingVolume)
        {
            _demoServer.SetD_PWSamplingGateParam_PA(SGX, SGY, depth, imageWidth, imageHight, SectorCenterAngle, samplingVolume);

            RefresD_SamplingGatePA_PRFList(SGX, SGY, depth, imageWidth, imageHight);

        }

        private void RefresD_SamplingGatePA_PRFList(float SGX, float SGY, float depth, float imageWidth, float imageHight)
        {
            SGX_SGConvex = SGX;
            SGY_SGConvex = SGY;
            depth_SGConvex = depth;
            imageWidth_SGConvex = imageWidth;
            imageHight_SGConvex = imageHight;
            double[] D_PRFListT = new double[8];
            double fFrequencyT = 0;
            int nRet = _demoServer.Get_D_PRFList_PA(SGX, SGY, depth, imageWidth, imageHight, D_PRFListT, ref fFrequencyT);
            if (nRet == 0)
            {
                bool dFrequencyDiff = false;
                if (fFrequencyT != m_fFrequency)
                {
                    dFrequencyDiff = true;
                    m_fFrequency = (float)fFrequencyT;
                    Set_D_ImageParam_Frequency(m_fFrequency);
                    _emissionFrequency = m_fFrequency * 1000000;
                    this.DopplerCtrl.SetEmissionFrequency((float)_emissionFrequency);
                }
                if (!DoubleValueEquals(D_PRFList, D_PRFListT) || dFrequencyDiff)
                {
                    DoubleValueCopy(D_PRFList, D_PRFListT);

                    Get_D_PRFList = true;
                    ReInit_D_RPFValue(true);
                }
            }
            else
            {
                Get_D_PRFList = false;
            }
        }

        void DopplerCtrl_SetBaseLineChangeEvent(int AddLevel)
        {
            double value =this.D_BaseLine.Value;
            this.D_BaseLine.Value = value + AddLevel;
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.BaseLine_Level_Param;
            d_ImageParam_CLR.m_nBaseLineLevel = (int)this.D_BaseLine.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }
        private void D_Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Gain_Param;
            d_ImageParam_CLR.m_fGain = (int)this.D_Gain.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void D_DynamicRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.DR_Param;
            d_ImageParam_CLR.m_fDR = (int)this.D_DynamicRange.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }
        private double RefreshDScaleShowValue()
        {
            int key = (int)this.D_Scale.Value;
            double value = 0;
            bool bRet = _d_Scale_Dictionary.TryGetValue(key, out value);
            if (bRet)
            {
                this.D_ScaleValue.Text = value.ToString("F01") + "KHz";
            }

            return value;
        }
       

        private void D_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.PRF_Rate_Param;
            d_ImageParam_CLR.m_prf_rate = D_PrfRateArray[(int)this.D_Scale.Value];
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
            double D_ScaleValue = RefreshDScaleShowValue();
            this.DopplerCtrl.SetFlowParam((float)D_ScaleValue * 1000, 1540 * 100, m_fFrequency * 1000000);
        }

        private void D_BaseLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.BaseLine_Level_Param;
            d_ImageParam_CLR.m_nBaseLineLevel = (int)this.D_BaseLine.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);

            if (this.DopplerCtrl != null)
            {
                this.DopplerCtrl.SetBaseLine((int)this.D_BaseLine.Value);
                this.DopplerCtrl.RefreshDopplerImage(true);
            }
        
        }

        private void D_WF_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Wall_Level_Param;
            d_ImageParam_CLR.m_nWall_level = (int)this.D_WF.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void D_Angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.DopplerCtrl != null)
                this.DopplerCtrl.SetVascularAngle((int)this.D_Angle.Value);
            if (this.BAWBUtrs != null)
                this.BAWBUtrs.DoppleSGControl.SetVascularAngle((int)this.D_Angle.Value);
        }

        private void D_TimeScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int time = 0;
            switch (D_TimeScale.SelectedIndex)
            {
                case 0:
                    time = 4;
                    break;
                case 1:
                    time = 6;
                    break;
                case 2:
                    time = 8;
                    break;
            }
            if(this.DopplerCtrl != null)
            this.DopplerCtrl.SetShowTime(time);
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Time_Param;
            d_ImageParam_CLR.m_fTime = time;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void RefreshD_FrequencyShowValue()
        {
            if (this.D_Speed == null)
                return;
            double[] D_Frequency = new double[2];
            for (int i = 0; i < 2; i++)
            {
                D_Frequency[i] = _current_probe_Info.m_fD_Tx_freq[i];
            }

            int j = 0;
            foreach (ComboBoxItem item in this.D_Speed.Items)
            {
                item.Content = D_Frequency[j++].ToString("F01") + "M";
            }
        }
        private void D_Speed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Speed_Param;
            d_ImageParam_CLR.m_nSpeed = (int)this.D_Speed.SelectedIndex;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void D_Steer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.DopplerCtrl != null)
                this.DopplerCtrl.SetLaunchDeflection((float)this.D_Steer.Value);
            if (this.BAWBUtrs != null)
                this.BAWBUtrs.DoppleSGControl.SetLaunchDeflection((float)this.D_Steer.Value);
        }

        private void D_SamplingVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Sampling_Volume_Param;
            d_ImageParam_CLR.m_nSamplingVolume = (int)this.D_SamplingVolume.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
            if (this.BAWBUtrs != null)
                this.BAWBUtrs.DoppleSGControl.SetSamplingVolume((int)this.D_SamplingVolume.Value);
        }

        private void D_Invert_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.Inversion_Param;
            d_ImageParam_CLR.m_nInversion = (int)this.D_Invert.SelectedIndex;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
        }

        private void D_Map_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.GrayColorMap_level_Param;
            d_ImageParam_CLR.m_nGrayColorMap_level = (int)this.D_Map.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
            RefreshMap();
        }

        private void D_PseudocolorLevel_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            D_PW_ImageParam_CLR d_ImageParam_CLR = new D_PW_ImageParam_CLR();
            d_ImageParam_CLR.m_D_PW_ImageParamType = (int)D_PW_ImageParam_CLR.D_PW_ImageParamType.PseudoColorMap_level_Param;
            d_ImageParam_CLR.m_nPseudoColorMap_level = (int)this.D_PseudocolorLevel.Value;
            int nRet = _demoServer.SetImageParam_D_PW(d_ImageParam_CLR);
            RefreshMap();
        }
        #endregion

        #region M Ctrl


        void MModeSLControl_SamplingLineChangedEvent(float SLX, float depth, float imageWidth, float imageHight)
        {
            _demoServer.SetSamplingLineParam_Line(SLX, depth, imageWidth, imageHight);
        }

        void MModeSLControl_SamplingLineConvexChangedEvent(float SectorCenterAngle, float depth, float imageWidth, float imageHight)
        {
            _demoServer.SetSamplingLineParam_Convex(SectorCenterAngle, depth, imageWidth, imageHight);
        }

        void MModeSLControl_SamplingLinePhasedChangedEvent(float SectorCenterAngle, float depth, float imageWidth, float imageHight)
        {
            _demoServer.SetSamplingLineParam_Phased(SectorCenterAngle, depth, imageWidth, imageHight);
        }


        #region M 模式

        private bool enterMMode = false;
        private void EnterMMode()
        {
            enterMMode = true;

            this.DopplerCtrl.Visibility = System.Windows.Visibility.Visible;

            this.BAWBUtrs.ColorFrameCanUse = false;
            this.BAWBUtrs.ColorFrameVisibility = Visibility.Collapsed;

            this.BAWBUtrs.DoppleSGCanUse = false;
            this.BAWBUtrs.DoppleSGVisibility = Visibility.Collapsed;

            this.BAWBUtrs.MModeSLCanUse = true;
            this.BAWBUtrs.MModeSLVisibility = Visibility.Visible;


            this.BAWBUtrs.SetUseMMode(true);
            this.DopplerCtrl.SetUseMMode(true);

            int time = 0;
            switch (M_TimeScale.SelectedIndex)
            {
                case 0:
                    time = 4;
                    break;
                case 1:
                    time = 6;
                    break;
                case 2:
                    time = 8;
                    break;
            }
            this.DopplerCtrl.SetShowTime(time);

            this.DopplerCtrl.SetDepthLevel(this.BAWBUtrs.ScaleMarksDepthLevel);
            this.DopplerCtrl.SetVideoState(false);
            this.DopplerCtrl.ClearScreen();

            int h1 = this.BAWBUtrs.ImageShowHeigth;
            int w1 = this.BAWBUtrs.ImageShowWidth;

            //设置新的宽高
            this.BAWBUtrs.ImageShowHeigth = h1 / 2;
            this.BAWBUtrs.ImageShowWidth = ImageWidth;

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)(B_ImageParam_CLR.B_ImageParamType.Height_Param | B_ImageParam_CLR.B_ImageParamType.Width_Param);

            b_ImageParam_CLR.m_nHeight = this.BAWBUtrs.ImageShowHeigth;
            b_ImageParam_CLR.m_nWidth = this.BAWBUtrs.ImageShowWidth;

            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);

            //设置D M控件大小
            DopplerCtrl.ImageShowHeigth = this.BAWBUtrs.ImageShowHeigth;
            DopplerCtrl.ImageShowWidth = this.BAWBUtrs.ImageShowWidth;

            //设置图像显示宽高
            this.BAWBUtrs.MModeSLControl.SetImageWidthAHeightPixels(_demoServer.GetImageWidthPixels(), _demoServer.GetImageHeightPixels());

            //下发M参数

            //设置算法计算宽高和实际相同，通过算法获取放大图像。
            M_ImageParam_CLR m_ImageParam = new M_ImageParam_CLR();
            m_ImageParam.m_M_ImageParamType = (int)(M_ImageParam_CLR.M_ImageParamType.All_Param);
            m_ImageParam.m_fGain = (int)this.M_Gain.Value;
            m_ImageParam.m_fTime = time;
            m_ImageParam.m_nGrayColorMap_level = (int)this.M_Map.Value;
            m_ImageParam.m_nPseudoColorMap_level = (int)this.M_PseudocolorLevel.Value;
            m_ImageParam.m_nHeight = this.DopplerCtrl.ImageShowHeigth;
            m_ImageParam.m_nWidth = this.DopplerCtrl.ImageShowWidth;
            _demoServer.SetImageParam_M(m_ImageParam);

            _demoServer.SetParamPath(currentCheckModeParamPath, CLScanMode.CLScanModeEnum.BM);

            this.BAWBUtrs.MModeSLControl.ReCalculateSamplingLineChanged();
            _demoServer.GetColorMap_M(_colorIntBar_DM);
            SetDMGrapMap(_colorIntBar_DM);
        }

        private void ExitMMode()
        {
            enterMMode = false;
            this.DopplerCtrl.Visibility = System.Windows.Visibility.Collapsed;

            this.BAWBUtrs.MModeSLCanUse = false;
            this.BAWBUtrs.MModeSLVisibility = Visibility.Collapsed;


            this.BAWBUtrs.SetUseMMode(false);
            this.DopplerCtrl.SetUseMMode(false);

            this.BAWBUtrs.ClearImageShow();

            //设置新的宽高
            this.BAWBUtrs.ImageShowHeigth = ImageHeigh;
            this.BAWBUtrs.ImageShowWidth = ImageWidth;

            B_ImageParam_CLR b_ImageParam_CLR = new B_ImageParam_CLR();
            b_ImageParam_CLR.m_B_ImageParamType = (int)(B_ImageParam_CLR.B_ImageParamType.Height_Param | B_ImageParam_CLR.B_ImageParamType.Width_Param);
            b_ImageParam_CLR.m_nHeight = this.BAWBUtrs.ImageShowHeigth;
            b_ImageParam_CLR.m_nWidth = this.BAWBUtrs.ImageShowWidth;
            int nRet = _demoServer.SetImageParam_B(b_ImageParam_CLR);
        }

        #endregion

        private void Z_Click(object sender, RoutedEventArgs e)
        {
            this.M.Visibility = Visibility.Collapsed;
        }

        private void M_Click(object sender, RoutedEventArgs e)
        {
            if (this.M.IsChecked.HasValue && this.M.IsChecked.Value)
            {
                if (this.B.IsChecked.HasValue && this.B.IsChecked.Value)
                {
                    this.B.IsChecked = false;
                }
                if (this.D.IsChecked.Value == true)
                {
                    this.D.IsChecked = false;
                }
                if (this.C.IsChecked.Value == true)
                {
                    this.C.IsChecked = false;
                }

                ExitPW();
                this.BAWBUtrs.ColorFrameControl.Visibility = Visibility.Collapsed;
                mLastScanMode = mScanMode;
                mScanMode = CLScanMode.CLScanModeEnum.BM;
                EnterMMode();

                RefreshMapVisibility();
                RefreshMap();
            }
            else
            {
                this.B.IsChecked = true;
                RoutedEventArgs ee = new RoutedEventArgs(ToggleButton.ClickEvent, this);
                this.B.RaiseEvent(ee);
            }
        }
        
        //TODO-CWT
        private void SaveWriteableBitmapAsPng(WriteableBitmap bmp, string filePath)
        {
            // 1) Obtener el directorio padre usando System.IO.Path
            string dir = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // 2) Abrir el FileStream y guardar el PNG
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }
        }
        
        private WriteableBitmap CreateGrayscaleBitmapFromFloat2D(float[,] data)
        {
            int height = data.GetLength(0);
            int width  = data.GetLength(1);

            // 1) Encontrar valores mínimo y máximo para normalizar
            float minVal = float.MaxValue, maxVal = float.MinValue;
            foreach (var val in data)
            {
                if (val < minVal) minVal = val;
                if (val > maxVal) maxVal = val;
            }
            float range = maxVal - minVal;
            if (range <= 0) range = 1f; // evitar división por cero

            // 2) Crear arreglo de bytes (Gray8), normalizando a [0..255]
            byte[] pixels = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float norm = (data[y, x] - minVal) / range;
                    if (norm < 0f) norm = 0f;
                    if (norm > 1f) norm = 1f;
                    pixels[y * width + x] = (byte)(norm * 255f);
                }
            }

            // 3) Construir el WriteableBitmap en formato Gray8
            var wb = new WriteableBitmap(
                width, height,
                96, 96,                    // DPI
                PixelFormats.Gray8,        // un byte por píxel
                null);

            Int32Rect rect = new Int32Rect(0, 0, width, height);
            int stride = width; // en Gray8, stride = ancho en bytes
            wb.WritePixels(rect, pixels, stride, 0);
            return wb;
        }

        private async void CWT_Click(object sender, RoutedEventArgs e)
        {
            var tg = sender as ToggleButton;
            if (tg == null) return;

            if (tg.IsChecked == true)
            {
                
                MessageBox.Show("Paso 1: Entramos en CWT_Click y el Toggle está checked.", "Debug CWT");

                // 1) Obtén dimensiones
                int width  = _demoServer.GetImageWidthPixels();
                int height = _demoServer.GetImageHeightPixels();
                MessageBox.Show($"Paso 2: Dimensiones obtenidas → width={width}, height={height}", "Debug CWT");
                // 2) Genera el phantom
                MessageBox.Show("Paso 3: Generando el phantom en memoria...", "Debug CWT");
                float[,] sonoImage = GenerarPhantom(width, height);
                MessageBox.Show("Paso 4: Phantom generado correctamente.", "Debug CWT");
                // 2.a) Convertir y guardar el phantom en disco
                WriteableBitmap phantomBmp = CreateGrayscaleBitmapFromFloat2D(sonoImage);
                string phantomPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "phantom_CWT.png"
                );
                SaveWriteableBitmapAsPng(phantomBmp, phantomPath);
                MessageBox.Show($"[Debug] Phantom guardado en:\n{phantomPath}", "Debug CWT");
                // 3) Lee parámetros
                double sigma  = CWT_Depth.Value;
                double omega0 = CWT_Gain.Value;
                MessageBox.Show($"Paso 5: Parámetros leídos → σ={sigma}, ω₀={omega0}", "Debug CWT");

                double vibFreq = 200.0;
                double dx      = 0.0005;
                double swsMin  = 0.5, swsMax = 5.0;
                double[] swsRange      = new double[] { swsMin, swsMax };
                double[] waveletParams = new double[] { omega0, sigma };
                MessageBox.Show($"Paso 6: VibFreq={vibFreq}, dx={dx}, rangoSWS=[{swsMin},{swsMax}]", "Debug CWT");

                // 4) Ejecutar ProcessImage en background y medir tiempo
                Stopwatch sw = Stopwatch.StartNew();
                MessageBox.Show("Paso 7: Iniciando procesamiento en Task.Run...", "Debug CWT");

                (double[,] SWSImage, double[,] wtMaxImage) resultado =
                    await Task.Run(() =>
                    {
                        // Ya no usamos MessageBox aquí, porque no se muestra en hilos de background.
                        // Simplemente devolvemos el resultado cuando termine.

                        var processor = new CWTSWSProcessor(vibFreq, dx, swsRange, waveletParams);
                        return processor.ProcessImage(sonoImage, "Morlet");
                    });

                sw.Stop();
                MessageBox.Show($"Paso 8: ProcessImage terminó en {sw.ElapsedMilliseconds} ms.", "Debug CWT");
                
                // 5) Mostrar la imagen sobre la UI
                double[,] sws2D = resultado.SWSImage;
                MessageBox.Show("Paso 9: Convirtiendo SWSImage a WriteableBitmap...", "Debug CWT");
                WriteableBitmap wb = CreateGrayscaleBitmapFromDouble2D(sws2D);
                string overlayPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "overlay_CWT.png"
                );
                SaveWriteableBitmapAsPng(wb, overlayPath);

                MessageBox.Show($"[Debug] Paso 6: Overlay CWT guardado en:\n{overlayPath}", "Debug CWT");
                MessageBox.Show("Paso 10: Asignando overlayImage.Source y haciéndolo visible.", "Debug CWT");
                overlayImage.Source     = wb;
                overlayImage.Visibility = Visibility.Visible;
                /*// — 1) Convertir la imagen B-mode actual a float[,]
                float[,] sonoImage = ConvertBModeToFloat2D();
                if (sonoImage == null)
                {
                    MessageBox.Show(
                        "No hay imagen B-mode disponible para procesar.\n" +
                        "Asegúrate de haber hecho Freeze antes de activar CWT.",
                        "CWT",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    tg.IsChecked = false;
                    return;
                }

                // — 2) Leer parámetros CWT de los sliders en el XAML
                //    (CWT_Depth y CWT_Gain están definidos en tu XAML dentro del GroupBox "Parámetros CWT")
                double sigma  = CWT_Depth.Value;   // slider “Parametro 1”
                double omega0 = CWT_Gain.Value;    // slider “Parametro 2”

                //    Si quieres mapearlos a rangos específicos (por ejemplo, omega0 ∈ [1..10]),
                //    ajusta Minimum/Maximum en XAML o aquí:
                //    omega0 = MapRange(omega0, 0,100, 1,10); // ejemplo de remapeo

                double[] waveletParams = new double[] { omega0, sigma };

                // — 3) Otros parámetros fijos (o puedes exponerlos con más controles)
                //    Frecuencia de vibración (Hz) y pitch (dx en m)
                //TODO-CWT-AJUSTE PARAMETROS
                double vibFreq = 200.0;    // ejemplo: 200 Hz (ajusta al valor real de tu hardware)
                double dx      = 0.0005;   // ejemplo: 0.5 mm → 0.0005 m (ajusta al pitch real de la sonda)

                //    Rango de velocidades SWS (en m/s). Podrías exponer dos sliders (min/max),
                //    pero aquí lo mantengo constante:
                double swsMin = 0.5;
                double swsMax = 5.0;
                //TODO-CWT-AJUSTE PARAMETROS
                double[] swsRange = new double[] { swsMin, swsMax };

                // — 4) Instanciar el procesador CWT → SWS con Morlet
                var processor = new CWTSWSProcessor(vibFreq, dx, swsRange, waveletParams);

                // — 5) Ejecutar ProcessImage (devuelve SWSImage y wtMaxImage, aunque aquí mostramos solo SWSImage)
                (double[,] SWSImage, double[,] wtMaxImage) = processor.ProcessImage(sonoImage, "Morlet");

                // — 6) Convertir SWSImage a WriteableBitmap (grises) y asignar a overlayImage
                WriteableBitmap wb = CreateGrayscaleBitmapFromDouble2D(SWSImage);
                overlayImage.Source = wb;
                overlayImage.Visibility = Visibility.Visible;*/
            }
            else
            {
                // Desmarcado → ocultar overlayImage
                overlayImage.Visibility = Visibility.Collapsed;
            }
        }
        
        private void CWT_Depth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Opcional: aquí puedes poner lógica para reaccionar
            // cuando cambie el valor de CWT_Depth.
            // Por ejemplo, si quisieras mostrar el valor en algún TextBlock:
            //    this.CWTDepthLabel.Text = e.NewValue.ToString("F0");
        }
        
        private void CWT_Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Opcional: lógica cuando cambie el valor de CWT_Gain.
        }
        
        private void M_Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            M_ImageParam_CLR m_ImageParam_CLR = new M_ImageParam_CLR();
            m_ImageParam_CLR.m_M_ImageParamType = (int)M_ImageParam_CLR.M_ImageParamType.Gain_Param;
            m_ImageParam_CLR.m_fGain = (int)this.M_Gain.Value;
            int nRet = _demoServer.SetImageParam_M(m_ImageParam_CLR);
        }
        private void M_TimeScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            M_ImageParam_CLR m_ImageParam_CLR = new M_ImageParam_CLR();
            m_ImageParam_CLR.m_M_ImageParamType = (int)M_ImageParam_CLR.M_ImageParamType.Time_Param;
            switch (this.M_TimeScale.SelectedIndex)
            {
                case 0:
                    m_ImageParam_CLR.m_fTime = 4;
                    break;
                case 1:
                    m_ImageParam_CLR.m_fTime = 6;
                    break;
                case 2:
                    m_ImageParam_CLR.m_fTime = 8;
                    break;
            }
            int nRet = _demoServer.SetImageParam_M(m_ImageParam_CLR);

            if(this.DopplerCtrl != null)
                this.DopplerCtrl.SetShowTime((int)m_ImageParam_CLR.m_fTime);
        }
        private void M_Map_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            M_ImageParam_CLR m_ImageParam_CLR = new M_ImageParam_CLR();
            m_ImageParam_CLR.m_M_ImageParamType = (int)M_ImageParam_CLR.M_ImageParamType.GrayColorMap_level_Param;
            m_ImageParam_CLR.m_nGrayColorMap_level = (int)this.M_Map.Value;
            int nRet = _demoServer.SetImageParam_M(m_ImageParam_CLR);
            RefreshMap();
        }

        private void M_PseudocolorLevel_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            M_ImageParam_CLR m_ImageParam_CLR = new M_ImageParam_CLR();
            m_ImageParam_CLR.m_M_ImageParamType = (int)M_ImageParam_CLR.M_ImageParamType.PseudoColorMap_level_Param;
            m_ImageParam_CLR.m_nPseudoColorMap_level = (int)this.M_PseudocolorLevel.Value;
            int nRet = _demoServer.SetImageParam_M(m_ImageParam_CLR);
            RefreshMap();
        }

        #endregion

        #region Update Ctrl 

        private void RefreshUpdateText( CLScanMode.CLScanModeEnum mLastScanMode)
        {
            if (mLastScanMode ==  CLScanMode.CLScanModeEnum.B)
            {
                this.Update_B_BC.Text = "B";
              
            }
            else if (mLastScanMode == CLScanMode.CLScanModeEnum.BC)
            {
                this.Update_B_BC.Text = "C";
            }
        }

        private void RefreshUpdateTextState(D_PW_StateE pwState)
        {
            if(pwState ==  D_PW_StateE.PW_B || pwState == D_PW_StateE.PW_BC)
            {
                this.Update_B_BC.Foreground = (SolidColorBrush)this.TryFindResource("UpdateForeground");
                this.Update_D.Foreground = (SolidColorBrush)this.TryFindResource("TitleAHelpForeground");
            }
            else if (pwState == D_PW_StateE.PW_D)
            {
                this.Update_B_BC.Foreground = (SolidColorBrush)this.TryFindResource("TitleAHelpForeground");
                this.Update_D.Foreground = (SolidColorBrush)this.TryFindResource("UpdateForeground"); 
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (mScanMode == CLScanMode.CLScanModeEnum.D_PW)
            {
                if (!this.BAWBUtrs.FreezeState)
                {
                    if (pwState == D_PW_StateE.PW_D)
                    {
                        BAWBUtrs_ActivateBEvent();
                    }
                    else if (pwState == D_PW_StateE.PW_B || pwState == D_PW_StateE.PW_BC)
                    {
                        DopplerCtrl_ActivatePWDORMEvent();
                    }
                    RefreshUpdateTextState(pwState);
                }
            }

        }
        #endregion

        #region map Flag
        //根据模式选择是否要显示灰阶图和彩色图
        public void RefreshMapVisibility()
        {
            switch (mScanMode)
            {
                case CLScanMode.CLScanModeEnum.B:
                    this.GrayMap.Visibility = Visibility.Visible;
                    this.ColorMap.Visibility = Visibility.Collapsed;
                    this.MaxScale.Visibility = Visibility.Collapsed;
                    this.MinScale.Visibility = Visibility.Collapsed;
                    this.DM_GrayMap.Visibility = Visibility.Collapsed;
                    break;
                case CLScanMode.CLScanModeEnum.BC:
                    this.GrayMap.Visibility = Visibility.Visible;
                    this.ColorMap.Visibility = Visibility.Visible;
                    this.MaxScale.Visibility = Visibility.Visible;
                    this.MinScale.Visibility = Visibility.Visible;
                    this.DM_GrayMap.Visibility = Visibility.Collapsed;
                    break;
                case CLScanMode.CLScanModeEnum.D_PW:
                    this.GrayMap.Visibility = Visibility.Visible;
                    if (mLastScanMode == CLScanMode.CLScanModeEnum.BC)
                    {
                        this.ColorMap.Visibility = Visibility.Visible;
                        this.MaxScale.Visibility = Visibility.Visible;
                        this.MinScale.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.ColorMap.Visibility = Visibility.Collapsed;
                        this.MaxScale.Visibility = Visibility.Collapsed;
                        this.MinScale.Visibility = Visibility.Collapsed;
                    }
                        
                    this.DM_GrayMap.Visibility = Visibility.Visible;
                    break;
                case CLScanMode.CLScanModeEnum.BM:
                    this.GrayMap.Visibility = Visibility.Visible;
                    this.ColorMap.Visibility = Visibility.Collapsed;
                    this.MaxScale.Visibility = Visibility.Collapsed;
                    this.MinScale.Visibility = Visibility.Collapsed;
                    this.DM_GrayMap.Visibility = Visibility.Visible;
                    break;
            }

        }

        private int[] _colorIntBar_B = new int[256];
        private int[] _colorIntBar_C = new int[256 * 16];
        private int[] _colorIntBar_DM = new int[256];
        public void RefreshMap()
        {
            if (this.GrayMap != null && this.GrayMap.Visibility == Visibility.Visible)
            {
                _demoServer.GetColorMap_B(_colorIntBar_B);
                SetGrapMap(_colorIntBar_B);
            }
            if (this.ColorMap != null && this.ColorMap.Visibility == Visibility.Visible)
            {
                _demoServer.GetColorMap_C(_colorIntBar_C);
                SetColorMap(_colorIntBar_C);
            }
            if (this.DM_GrayMap != null && this.DM_GrayMap.Visibility == Visibility.Visible)
            {
                if(mScanMode == CLScanMode.CLScanModeEnum.D_PW)
                {
                    _demoServer.GetColorMap_D(_colorIntBar_DM);
                    SetDMGrapMap(_colorIntBar_DM);
                }
                else
                {
                    _demoServer.GetColorMap_M(_colorIntBar_DM);
                    SetDMGrapMap(_colorIntBar_DM);
                }
            }
        }

        public void SetGrapMap(int[] mapData)
        {
            if (this.GrayMap == null || mapData == null)
            {
                return;
            }
            this.GrayMap.AddGrayMapData(mapData);
        }

        public void SetColorMap(int[] mapData)
        {
            if (this.ColorMap == null || mapData == null)
            {
                return;
            }
            int[] colorMap = new int[256 * 16];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    colorMap[(255 - i) * 16 + j] = mapData[j * 256 + i];
                }
            }

            this.ColorMap.AddColorMapData(colorMap);

        }
        public void SetDMGrapMap(int[] mapData)
        {
            if (mapData == null)
            {
                return;
            }
            this.DM_GrayMap.AddGrayMapData(mapData);
        }
        public void RefreshScale()
        {
            if (this.MaxScale == null || this.MinScale == null || this.C_Mode == null)
            {
                return;
            }
            //彩色模式等级 0:Color; 1:Power; 能量模式不显示流速
            if (this.C_Mode.SelectedIndex == 1)
            {
                this.MaxScale.Visibility = System.Windows.Visibility.Hidden;
                this.MinScale.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (this.C_Mode.SelectedIndex == 0)
            {
                this.MaxScale.Visibility = System.Windows.Visibility.Visible;
                this.MinScale.Visibility = System.Windows.Visibility.Visible;

            }
            this.MaxScale.Text = "+" + this.C_ScaleValue.Text;
            this.MinScale.Text = "-" + this.C_ScaleValue.Text;
        }




        #endregion
    }
}
