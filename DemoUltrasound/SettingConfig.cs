using CLRDemoServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DemoUltrasound.Setting
{
    

    public class DepthMarksShowConfig
    {
        //开始深度（含）
        public float MinDepth { get; set; }
        //结束深度（含）
        public float MaxDepth { get; set; }
        //主要深度步街 画长线
        public float PrimaryDepthStep { get; set; }
        //次要深度步街 画长短线
        public float SecondaryDepthStep { get; set; }
    }

    public class DepthTextShowConfig
    {
        //开始深度（不含）
        public float MinDepth { get; set; }
        //结束深度（含）
        public float MaxDepth { get; set; }
        //显示深度数值的步节
        public float ShowDepthStep { get; set; }

    }

    public class SettingConfig
    {
        #region 成员变量

        private const string PathDefault = "../conf/SettingConfig.xml";
        private static string _version = "1.0";
        private static SettingConfig _settingConfig;

        #endregion

        #region 公有属性

        /// <summary>
        ///     版本号
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }




        //深度刻度线设置配置
        private ObservableCollection<DepthMarksShowConfig> _depthMarksShowConfigArray;
        public ObservableCollection<DepthMarksShowConfig> DepthMarksShowConfigArray
        {
            get { return _depthMarksShowConfigArray; }
            set
            {
                _depthMarksShowConfigArray = value;
            }
        }

        //深度刻度线设置配置
        private ObservableCollection<DepthTextShowConfig> _depthTextShowConfigArray;
        public ObservableCollection<DepthTextShowConfig> DepthTextShowConfigArray
        {
            get { return _depthTextShowConfigArray; }
            set
            {
                _depthTextShowConfigArray = value;
            }
        }
        #endregion

        #region 私有方法

        private static string GetPath(string pPath = null)
        {
            string path = string.IsNullOrEmpty(pPath) ? PathDefault : pPath;

            return path;
        }

        #endregion

        #region 公有方法

        private void Save<T>(string pPath = null)
        {
            string path = GetPath(pPath);

            FileStream fs = null;

            try
            {
                var xs = new XmlSerializer(typeof(SettingConfig));

                fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                xs.Serialize(fs, this);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(" 序列化异常 " + ex.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            if (!object.ReferenceEquals(this, _settingConfig))
            {
                _settingConfig = this;
            }

        }
        //在只有本模块使用时使用SendSaveEvent == false；当其他模块也要重新设定的信息时，SendSaveEvent == true，需要重新获取。
        public void Save(bool SendSaveEvent = true, string pPath = null)
        {
            Save<SettingConfig>();

            if (SaveValueEvent != null && SendSaveEvent)
            {
                SaveValueEvent(_settingConfig);
            }

        }


        public bool GetDepthMarksDepthStep(float currentDepth, out float PrimaryDepthStep, out float SecondaryDepthStep)
        {
            bool isGetDepthStep = true;
            float PrimaryDepthStepTemp = 1.0f;
            float SecondaryDepthStepTemp = 0.5f;
            if (DepthMarksShowConfigArray.Count == 0)
            {
                PrimaryDepthStep = PrimaryDepthStepTemp;
                SecondaryDepthStep = SecondaryDepthStepTemp;
                return isGetDepthStep;
            }


            foreach (var depthMarksShowConfig in DepthMarksShowConfigArray)
            {
                if (depthMarksShowConfig.MinDepth <= currentDepth && currentDepth <= depthMarksShowConfig.MaxDepth)
                {
                    PrimaryDepthStepTemp = depthMarksShowConfig.PrimaryDepthStep;
                    SecondaryDepthStepTemp = depthMarksShowConfig.SecondaryDepthStep;
                    break;
                }
            }
            PrimaryDepthStep = PrimaryDepthStepTemp;
            SecondaryDepthStep = SecondaryDepthStepTemp;

            return isGetDepthStep;
        }
        public bool GetDepthTextShowDepthStep(float currentDepth, out float ShowTextDepthStep)
        {
            bool isGetDepthStep = true;
            float ShowTextDepthStepTemp = 0.5f;

            if (DepthTextShowConfigArray.Count == 0)
            {
                ShowTextDepthStep = ShowTextDepthStepTemp;
                return isGetDepthStep;
            }


            foreach (var depthTextShowConfig in DepthTextShowConfigArray)
            {
                if (depthTextShowConfig.MinDepth < currentDepth && currentDepth <= depthTextShowConfig.MaxDepth)
                {
                    ShowTextDepthStepTemp = depthTextShowConfig.ShowDepthStep;
                    break;
                }
            }
            ShowTextDepthStep = ShowTextDepthStepTemp;

            return isGetDepthStep;
        }
        #endregion

        #region 静态方法

        /// <summary>
        ///     加载设置 当p_path 为空是 加载默认路径
        /// </summary>
        /// <param name="pPath">路径</param>
        /// <returns></returns>
        public void Load(string pPath = null)
        {
            string path = GetPath(pPath);

            SettingConfig config;

            FileStream fs = null;

            string version = _version;

            try
            {
                if (!File.Exists(path))
                {
                    config = CreateNew(path);
                    _settingConfig = config;
                    return ;
                }

                fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                var xs = new XmlSerializer(typeof(SettingConfig));
                config = (SettingConfig)xs.Deserialize(fs);

                fs.Seek(0, SeekOrigin.Begin);
                XElement root = XElement.Load(fs);

                XAttribute verAttr = root.Attribute("Version");

                if (verAttr == null || verAttr.Value != version)
                {
                    config.Version = version;
                    config.Save();
                }
                //如果之前有事件，则将事件链接到新的对象上。
                if (_settingConfig != null && _settingConfig.SaveValueEvent != null)
                {
                    config.SaveValueEvent = _settingConfig.SaveValueEvent;
                }
                _settingConfig = config;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(" 反序列化异常 " + ex.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        ///     创建一个新的配置
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        private SettingConfig CreateNew(string pPath = null)
        {
            string path = GetPath(pPath);

            var config = new SettingConfig();
            config.CreateDefault();
            config.Save(true, path);
            return config;           
        }

        private void CreateDefault()
        {
            DepthMarksShowConfigArray = new ObservableCollection<DepthMarksShowConfig>();
            DepthMarksShowConfigArray.Add(new DepthMarksShowConfig() { PrimaryDepthStep = 1.0f, SecondaryDepthStep = 0.5f, MinDepth = 0, MaxDepth = 14 });

            DepthTextShowConfigArray = new ObservableCollection<DepthTextShowConfig>();
            DepthTextShowConfigArray.Add(new DepthTextShowConfig() { ShowDepthStep = 0.5f, MinDepth = 0, MaxDepth = 2 });
        }

        #endregion

        #region 静态属性

        public static SettingConfig SettingConfigSetting
        {
            get
            {
                if (null == _settingConfig)
                {
                    _settingConfig = new SettingConfig();
                    _settingConfig.Load();
                }
                return _settingConfig;
            }
        }

        #endregion

        #region 事件
        public delegate void SaveValueHandler(object sender);
        public event SaveValueHandler SaveValueEvent;

        #endregion
    }

    //探头信息
    public class ProbeInfo
    {
        private string _proberName;
        public string ProberName
        {
            get { return _proberName; }
            set { _proberName = value; }
        }
        private string _proberFrequency;
        public string ProberFrequency
        {
            get { return _proberFrequency; }
            set { _proberFrequency = value; }
        }
        private string _proberShowName;
        public string ProberShowName
        {
            get { return _proberShowName; }
            set { _proberShowName = value; }
        }

        private bool _proberCanShow;
        public bool ProberCanShow
        {
            get { return _proberCanShow; }
            set { _proberCanShow = value; }
        }

        private bool _isSupportPW;
        public bool IsSupportPW
        {
            get { return _isSupportPW; }
            set { _isSupportPW = value; }
        }

        
    }



    public class ProbeConfig
    {
        #region 成员变量

        private static string _version = "1.0";

        #endregion

        #region 公有属性
        /// <summary>
        ///     版本号
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        //导管列表
        private ObservableCollection<ProbeInfo> _probeInfoList;
        public ObservableCollection<ProbeInfo> ProbeInfoList
        {
            get { return _probeInfoList; }
            set
            {
                _probeInfoList = value;
            }
        }


        #endregion


        #region 共有方法

        /// <summary>
        ///     加载设置 当p_path 为空是 加载默认路径
        /// </summary>
        /// <param name="pPath">路径</param>
        /// <returns></returns>
        public int Load(string pPath)
        {
            if (string.IsNullOrEmpty(pPath))
            {
                return -1;
            }
            if (!File.Exists(pPath))
            {
                return -2;
            }
            ProbeConfig config;

            FileStream fs = null;

            string version = _version;

            try
            {
                fs = new FileStream(pPath, FileMode.Open, FileAccess.Read);

                var xs = new XmlSerializer(typeof(ProbeConfig));
                config = (ProbeConfig)xs.Deserialize(fs);

                this._probeInfoList = config._probeInfoList;
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialization Anomalies");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return 0;
        }

        public int Save(string pPath = null)
        {
            FileStream fs = null;
            int nRet = 0;
            try
            {
                var xs = new XmlSerializer(typeof(ProbeConfig));

                fs = new FileStream(pPath, FileMode.Create, FileAccess.Write);
                xs.Serialize(fs, this);
            }
            catch (Exception ex)
            {
                nRet = -1;
                throw new Exception(string.Format("Serialization Fail " + ex.Message));
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return 0;
        }
        #endregion


    }

    //检查信息
    public class ExamInfo
    {
        private string _examName_ZH;
        public string ExamName_ZH
        {
            get { return _examName_ZH; }
            set { _examName_ZH = value; }
        }

        private string _examName_EN;
        public string ExamName_EN
        {
            get { return _examName_EN; }
            set { _examName_EN = value; }
        }

        private bool _examNameCanShow;
        public bool ExamNameCanShow
        {
            get { return _examNameCanShow; }
            set { _examNameCanShow = value; }
        }
    }

    public class ExamConfig
    {
        #region 成员变量

        private static string _version = "1.0";

        #endregion

        #region 公有属性
        /// <summary>
        ///     版本号
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        //检查列表
        private ObservableCollection<ExamInfo> _examInfoList;
        public ObservableCollection<ExamInfo> ExamInfoList
        {
            get { return _examInfoList; }
            set
            {
                _examInfoList = value;
            }
        }
        #endregion



        #region 共有方法

        /// <summary>
        ///     加载设置 当p_path 为空是 加载默认路径
        /// </summary>
        /// <param name="pPath">路径</param>
        /// <returns></returns>
        public int Load(string pPath)
        {
            if (string.IsNullOrEmpty(pPath))
            {
                return -1;
            }
            if (!File.Exists(pPath))
            {
                return -2;
            }
            ExamConfig config;

            FileStream fs = null;

            string version = _version;

            int canshowCount = 0;

            try
            {
                fs = new FileStream(pPath, FileMode.Open, FileAccess.Read);

                var xs = new XmlSerializer(typeof(ExamConfig));
                config = (ExamConfig)xs.Deserialize(fs);

                ExamInfoList = config.ExamInfoList;


                foreach (var ei in ExamInfoList)
                {
                    if (ei.ExamNameCanShow)
                    {
                        canshowCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialization Anomalies");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            if (canshowCount == 0)
            {
                foreach (var ei in ExamInfoList)
                {
                    ei.ExamNameCanShow = true;
                }

                Save(pPath);
            }

            return 0;
        }

        public int Save(string pPath = null)
        {
            FileStream fs = null;
            int nRet = 0;
            try
            {
                var xs = new XmlSerializer(typeof(ExamConfig));

                fs = new FileStream(pPath, FileMode.Create, FileAccess.Write);
                xs.Serialize(fs, this);
            }
            catch (Exception ex)
            {
                nRet = -1;
                throw new Exception(string.Format("Serialization Fail " + ex.Message));
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return 0;
        }
        #endregion


    }


    //图像参数
    public class ImageParamInfo
    {
        //增益
        private int _gainLevel;
        public int GainLevel
        {
            get { return _gainLevel; }
            set
            {
                _gainLevel = value;
            }
        }

        //动态范围
        private double _dynamicRange;
        public double DynamicRange
        {
            get { return _dynamicRange; }
            set
            {
                _dynamicRange = value;
            }
        }

        //是否开启图像增强
        private bool _isImageEnhancement;
        public bool IsImageEnhancement
        {
            get { return _isImageEnhancement; }
            set
            {
                _isImageEnhancement = value;
            }
        }

        //图像增强
        private int _imageEnhancementLevel;
        public int ImageEnhancementLevel
        {
            get { return _imageEnhancementLevel; }
            set
            {
                _imageEnhancementLevel = value;
            }
        }

        //帧相关
        private int _correlationLevel;
        public int CorrelationLevel
        {
            get { return _correlationLevel; }
            set
            {
                _correlationLevel = value;
            }
        }

        //灰阶图
        private int _grayLevel;
        public int GrayLevel
        {
            get { return _grayLevel; }
            set
            {
                _grayLevel = value;
            }
        }

        //伪彩图
        private int _pseudocolorLevel;
        public int PseudocolorLevel
        {
            get { return _pseudocolorLevel; }
            set
            {
                _pseudocolorLevel = value;
            }
        }
    }



    public class ExamModeConfig
    {
        #region 成员变量
        private static string exePath = "";
        private const string PathDefault = "../Param";
        private const string ProbeConfigName = "ProbeConfig.xml";
        private const string ExamConfigName = "ExamConfig.xml";
        private static string _version = "1.0";
        private static ExamModeConfig _examModeConfig;

        #endregion

        private ExamModeConfig()
        {
            exePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        #region 公有属性
        /// <summary>
        ///     版本号
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        //探头信息列表
        private ObservableCollection<ProbeInfo> _probeInfoList;
        public ObservableCollection<ProbeInfo> ProbeInfoList
        {
            get { return _probeInfoList; }
            set
            {
                _probeInfoList = value;
            }
        }


        //探头及对应检查信息列表
        private Dictionary<string, ObservableCollection<ExamInfo>> _probeAExamInfoList = new Dictionary<string, ObservableCollection<ExamInfo>>();
        public Dictionary<string, ObservableCollection<ExamInfo>> ProbeAExamInfoList
        {
            get { return _probeAExamInfoList; }
            set
            {
                _probeAExamInfoList = value;
            }
        }

        //探头及深度列表
        private Dictionary<string, Probe_Info> _probeAHWProbeInfo = new Dictionary<string, Probe_Info>();
        public Dictionary<string, Probe_Info> ProbeAHWProbeInfo
        {
            get { return _probeAHWProbeInfo; }
            set
            {
                _probeAHWProbeInfo = value;
            }
        }

        //探头_模式及深度列表
        private Dictionary<string, Probe_Info> _probeExamAHWProbeInfo = new Dictionary<string, Probe_Info>();
        public Dictionary<string, Probe_Info> ProbeExamAHWProbeInfo
        {
            get { return _probeExamAHWProbeInfo; }
            set
            {
                _probeExamAHWProbeInfo = value;
            }
        }

        #endregion


        public string GetProbeAExamName(string probeName, string examName_EN)
        {
            string probe_exam_name = probeName + "_" + examName_EN;

            return probe_exam_name;
        }

        public ProbeInfo GetProbeInfoByProbeName(string probeName)
        {
            ProbeInfo _probeInfo = null;
            foreach (var proberInfo in ProbeInfoList)
            {
                if (proberInfo.ProberName == probeName)
                {
                    _probeInfo = new ProbeInfo();
                    _probeInfo.ProberName = proberInfo.ProberName;
                    _probeInfo.ProberFrequency = proberInfo.ProberFrequency;
                    _probeInfo.ProberShowName = proberInfo.ProberShowName;
                }
            }
            return _probeInfo;
        }

        public void Load(string paramPath)
        {
            string path = exePath + paramPath + "/" + ProbeConfigName; ;

            if (!File.Exists(path))
            {
                return;
            }

            ProbeConfig probeConfig = new ProbeConfig();
            int nRet = probeConfig.Load(path);
            if (nRet != 0)
            {
                return;
            }

            ProbeInfoList = probeConfig.ProbeInfoList;

            int Count = 0;

            foreach (var proberInfo in ProbeInfoList)
            {
                ExamConfig examConfig = new ExamConfig();

                string examPath = exePath + paramPath + "/" + proberInfo.ProberName + "/" + ExamConfigName;
                string probeInfoRootPath = exePath + paramPath + "/" + proberInfo.ProberName;

                if (!File.Exists(examPath))
                {
                    continue; ;
                }

                nRet = examConfig.Load(examPath);
                if (nRet != 0)
                {
                    continue;
                }

                ProbeAExamInfoList.Add(proberInfo.ProberName, examConfig.ExamInfoList);

                if (!Directory.Exists(probeInfoRootPath))
                {
                    continue;
                }

                foreach (var ei in examConfig.ExamInfoList)
                {

                    string probeInfoPath = probeInfoRootPath + "/" + ei.ExamName_EN;
                    if (!Directory.Exists(probeInfoPath))
                    {
                        continue;
                    }

                    Probe_Info _probe_Info = new Probe_Info();
                    nRet = CLR_DemoServer.GetInstance().GetProberInfo(_probe_Info, probeInfoPath);
                    if (nRet == 0)
                    {
                        string probe_exam_name = proberInfo.ProberName + "_" + ei.ExamName_EN;
                      
                        _probe_Info.Probe_type += 1; //sdk中0是线阵，前台1是线阵，需要加1

                        ProbeExamAHWProbeInfo.Add(probe_exam_name, _probe_Info);
                    }

                    Count++;
                }


            }

            return;
        }


        public ProbeConfig GetProbeConfig(ref bool loadPC)
        {
            string path = exePath + PathDefault + "/" + ProbeConfigName;
            ProbeConfig probeConfig = new ProbeConfig();
            if (!File.Exists(path))
            {
                loadPC = false;
                return probeConfig;
            }


            int nRet = probeConfig.Load(path);
            if (nRet != 0)
            {
                loadPC = false;
                return probeConfig;

            }
            loadPC = true;
            return probeConfig;

        }

        public int SaveProbeConfig(ProbeConfig probconfig)
        {
            string path = exePath + PathDefault + "/" + ProbeConfigName;

            if (!File.Exists(path))
            {
                return -1;
            }


            int nRet = probconfig.Save(path);
            if (nRet != 0)
            {
                return nRet;
            }
            return 0;
        }


        public int SaveExamConfig(string probeName, ExamConfig examConfig)
        {
            string path = exePath + PathDefault + "/" + probeName + "/" + ExamConfigName;

            if (!File.Exists(path))
            {
                return -1;
            }


            int nRet = examConfig.Save(path);
            if (nRet != 0)
            {
                return nRet;
            }
            return 0;
        }
        #region 静态属性

        public static ExamModeConfig ExamModeConfigSetting
        {
            get
            {
                if (null == _examModeConfig)
                {
                    _examModeConfig = new ExamModeConfig();
                }
                return _examModeConfig;
            }
        }

        #endregion
    }
}
