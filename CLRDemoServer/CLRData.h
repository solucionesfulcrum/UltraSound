#pragma once
#include "Native.h"
namespace CLRDemoServer {

#define IMAGE_DATA_NUM 2048*2048

	public ref class ImageDataInfo_CLR
	{
	public:
		ImageDataInfo_CLR(void);
		~ImageDataInfo_CLR(void);
		float m_fFrameRate;			//帧率
		int m_nFrameWidthPixels;	//帧横向分辨率
		int m_nFrameHeightPixels;	//帧纵向分辨率
		int m_nImageWidthPixels;	//图像横向分辨率
		int m_nImageHeightPixels;	//图像纵向分辨率
		float m_fResolution;		//分辨率 单位 像素/毫米。
		int m_nHour; //时
		int m_nMinute; //分
		int m_nSecond; //秒
		int m_nMillisecond; //毫秒
		int m_nDepth;//当前深度
		int m_nFrameNumber;//帧编号
	};
	public ref class EnvelopeInfo_CLR
	{
	public:
		EnvelopeInfo_CLR();
		bool    m_bHadData;//是否有数据
		short	m_nEnvelopeMax;//最大包络
		short	m_nEnvelopeMean;//平均包络
	};

	public ref class D_PWDataInfo_CLR
	{
	public:
		D_PWDataInfo_CLR(void);
		~D_PWDataInfo_CLR(void);

		EnvelopeInfo_CLR^ envelopeInfo;

		int m_nDateNum;						//数据编号
		float m_AutoMeasureAngle;//默认值是-999不生效的值 自动测量角度 -90 ~ 90 
		float m_BloodRadius_T;//默认值是-999不生效的值 像素
		float m_BloodRadius_B;//默认值是-999不生效的值 像素
		int m_nD_PWDataLen;	//pw 一线长
	};

	public ref class MDataInfo_CLR
	{
	public:
		MDataInfo_CLR(void);
		~MDataInfo_CLR(void);

		int m_nDateNum;						//数据编号
		int m_nMDataLen;	//一线长

		int m_nFrameNumber;			//帧号
		int m_nImageHeightPixels;	//图像纵向分辨率
		int m_nMLineNum;			//M谱线数量
		bool m_bClearScreen;		//清屏
	};
	public ref class ImageData
	{
	public:
		ImageData(void);
		~ImageData(void);
		array<int>^	m_B_Imagedata;
		array<int>^	m_C_Imagedata;
		array<int>^ m_D_PW_Imagedata;
		array<int>^ m_M_Imagedata;

		ImageDataInfo_CLR^ m_B_ImageInfo;
		ImageDataInfo_CLR^ m_C_ImageInfo;
		array<D_PWDataInfo_CLR^>^ m_D_PW_ImageInfos;
		MDataInfo_CLR^ m_M_ImageInfo;

		int m_nBImageDataLen;
		int m_nCImageDataLen;
		int m_nD_PWImageDataLen;
		int m_nMImageDataLen;

		bool m_bBHadData;
		bool m_bCHadData;
		bool m_bD_PWHadData;
		bool m_bMHadData;

		int m_nD_PWDataNum; // pw数据线数
		const int m_nD_PWImageDataMaxLen;//PW最大缓存

		int m_nMDataNum; // pw数据线数
		const int m_nMImageDataMaxLen;//PW最大缓存
	};

	public ref class ImageData_D_PW
	{
	public:
		ImageData_D_PW(void);
		~ImageData_D_PW(void);
		array<int>^ m_D_PW_Imagedata;
		array<D_PWDataInfo_CLR^>^ m_D_PW_ImageInfos;
		int m_nD_PWImageDataLen;
		bool m_bD_PWHadData;

		int m_nD_PWDataNum; // pw数据线数
	};

	public ref class ImageData_M
	{
	public:
		ImageData_M(void);
		~ImageData_M(void);
		array<int>^ m_M_ImageData;
		array<MDataInfo_CLR^>^ m_M_ImageInfos;
		int m_nMImageDataLen;
		bool m_bMHadData;

		int m_nMDataNum; //M数据线数
	};


	public ref class B_ImageParam_CLR
	{
	public:
		B_ImageParam_CLR();
		~B_ImageParam_CLR();

		enum class B_ImageParamType
		{
			All_Param = 0x1FFF,
			Width_Param = 0x0001,				//DSC宽（像素）
			Height_Param = 0x0002,				//DSC高（像素）
			Depth_level_Param = 0x0004,			//深度档位
			Gain_Param = 0x0008,				//增益
			DR_Param = 0x0010,					//动态范围
			SRI_level_Param = 0x0020,			//图像增强档位
			Correlation_level_Param = 0x0040,	//帧相关档位
			TGC_Param = 0x0080,					//六段TGC
			GrayColorMap_level_Param = 0x0100,	//灰度映射Map档位
			PseudoColorMap_level_Param = 0x0200,//伪彩映射Map档位
			Harmonic_level_Param = 0x0400,		//0:基波；1：谐波
			Frequency_level_Param = 0x0800,		//0：低频;1：中评；2：高频；
			FocusArea_level_Param = 0x1000,		//0：焦点1;1：焦点2；2：焦点3；3：焦点4；4：焦点5；5：焦点6；6：全域；
			NE_Param = 0x2000,					//针增强 0:关;1:开;
			NE_Theta_Param = 0x4000,			//针增强角度: -30°- 30°;
		};

		int m_B_ImageParamType;

		
		int m_nWidth;				//DSC宽（像素） Max 2048
		int m_nHeight;				//DSC高（像素）  Max 2048
		int m_nDepth_level;			//深度档位
		float m_fGain;				//增益范围 1- 100
		float m_fDR;				//动态范围	0 - 100
		int m_SRI_Level;			//图像增强档位 1 - 8
		int m_nCorrelation_Level;	//帧相关系数 1 - 8
		int m_nGrayColorMap_level;	//灰度映射Map档位 1 - 8
		int m_nPseudoColorMap_level;//伪彩映射Map档位，0为默认 0 - 8
		array<float>^ m_fTGC;
		int m_nHarmonic;			//0:基波；1：谐波
		int m_nFrequency;			//0：低频;1：中评；2：高频；
		int m_nFocusArea;			//0：焦点1;1：焦点2；2：焦点3；3：焦点4；4：焦点5；5：焦点6；6：全域；
		int m_nNE;					//针增强 0:关;1:开;
		int m_nNE_Theta;			//针增强角度: -30°- 30°;
	};

	public ref class C_ImageParam_CLR
	{
	public:
		C_ImageParam_CLR();
		~C_ImageParam_CLR();

		enum class C_ImageParamType
		{
			All_Param = 0x07FF,
			Gain_Param = 0x0001,				//增益
			WallFilter_level_Param = 0x0002,	//壁滤波档位
			ColorPriority_level_Param = 0x0004,	//彩色优先度档位
			FrameCorrelation_level_Param = 0x0008,	//彩色帧相关档位
			Color_Mode_Param = 0x0010,		//速度：0；	能量：1；	弹性：2；
			ColorMap_level_Param = 0x0020,		//映射Map档位
			ColorMap_Inversion_Param = 0x0040,	//映射Map档位 0 - 11
			PRF_Level_Param = 0x0080,			//PRF档位
			Theta_Param = 0x0100,				//发射偏转角度°
			B_BC_Mode_Param = 0x0200,			//使用 B & BC模式
			Speed_Param = 0x0400,				//速度 0：低速，1：高速
		};

		int m_C_ImageParamType;

		float m_fGain;				//增益
		int m_nWallFilter_level;	//壁滤波档位
		int m_nColorPriority_level;	//彩色优先度档位（0~4）
		int m_nFrameCorrelation_level;//彩色帧相关档位（0~4)
		int m_nColorMap_mode;		//速度：0；	能量：1；	弹性：2；
		int m_nColorMap_level;		//映射Map档位 0-11档
		int m_nColorMap_inversion;	//图像翻转 正常：0；	翻转：1	
		int m_nPRF_Level;			//PRF档位  0-7 
		float m_fTheta;				//发射偏转角度/°
		bool m_bUse_B_BC_Mode;		//使用 B & BC模式
		int m_nSpeed;				//速度 0：低速，1：高速
	};

	public ref class D_PW_ImageParam_CLR
	{
	public:
		D_PW_ImageParam_CLR();
		~D_PW_ImageParam_CLR();

		enum class D_PW_ImageParamType
		{
			All_Param = 0x3FFF,
			Width_Param = 0x0001,
			Height_Param = 0x0002,
			PRF_Rate_Param = 0x0004,
			Gain_Param = 0x0008,
			DR_Param = 0x0010,
			Frequency_Param = 0x0020,
			Time_Param = 0x0040,
			BaseLine_Level_Param = 0x0080,
			Wall_Level_Param = 0x0100,
			Sampling_Volume_Param = 0x0200,
			Inversion_Param = 0x0400,
			Speed_Param = 0x0800,
			GrayColorMap_level_Param = 0x1000,
			PseudoColorMap_level_Param = 0x2000
		};

		int m_D_PW_ImageParamType; //参数类型

		int m_nWidth;//显示像素宽度 
		int m_nHeight;//显示像素高度
		int m_prf_rate;//PRF rate
		float m_fGain;//增益范围 -20- 20 默认0
		float m_fDR;//动态范围	20 - 60 默认40
		float m_fFrequency;//频率
		float m_fTime;//显示时间
		int m_nBaseLineLevel;//基线level -3 - 3
		int m_nWall_level;//壁滤波Level 0 - 2
		int m_nSamplingVolume;//取样门宽度
		int m_nInversion;	//图像翻转 正常：0；	翻转：1
		int m_nSpeed;				//速度 0：低速，1：高速
		int m_nGrayColorMap_level;   //灰度映射Map档位 1 - 8
		int m_nPseudoColorMap_level; //伪彩映射Map档位，0为默认 0 - 8
	};

	public ref class M_ImageParam_CLR
	{
	public:
		M_ImageParam_CLR();
		~M_ImageParam_CLR();

		enum class M_ImageParamType
		{
			All_Param = 0x003F,
			Gain_Param = 0x0001,
			Time_Param = 0x0002,
			Width_Param = 0x0004,
			Height_Param = 0x0008,
			GrayColorMap_level_Param = 0x0010,
			PseudoColorMap_level_Param = 0x0020
		};

		int m_M_ImageParamType;	//参数类型

		float m_fGain;//增益范围 0-100
		float m_fTime;//显示时间
		int m_nWidth;                //M宽（像素） Max 2048
		int m_nHeight;                //M高（像素）  Max 2048
		int m_nGrayColorMap_level;    //灰度映射Map档位 1 - 8
		int m_nPseudoColorMap_level;  //伪彩映射Map档位，0为默认 0 - 8
	};

	public ref class Probe_Info
	{
	public:
		Probe_Info();
		~Probe_Info();

		float m_fPith;				//探头Pith
		float m_fRadiusCurvature;	//探头曲率半径
		int m_nFocusNum;			//探头震源数量
		int Probe_type;				//探头类型  探头类型 1：线阵 2：凸阵 3：相控阵
		float m_fImageAngle;		//相控阵张角
		int m_nShowDepthLevel;		//显示深度档位
		array<int>^	m_pDepthList;	//深度列表 单位mm 显示需要*10	
		
	
		bool m_bC_Tx_deflectionflag;	//C是否有发射偏转
		array<int>^ m_pC_Tx_angle;			//C发射偏转角度

		bool m_bD_Tx_deflectionflag;	//D是否有发射偏转
		array<int>^ m_pD_Tx_angle;			//D发射偏转角度

		bool m_bB_NE_deflectionflag;	//B是否有针增强
		array<int>^ m_pB_NE_Theta_angle;//B针增强角度

		array<float>^ m_fB_fund_Tx_freq;//频率：基波发射频率依次为：高，中，低，单位MHz
		array<float>^ m_fB_harm_Tx_freq;//频率：谐波发射频率依次为：高，中，低，单位MHz
		array<float>^ m_fC_Tx_freq;//发射显示频率 依次为高频（低速），低频（高速）
		array<float>^ m_fD_Tx_freq;//发射显示频率 依次为高频（低速），低频（高速）
	};

	public ref class D_PWInfo
	{
	public:
		D_PWInfo();
		~D_PWInfo();

		float m_nFrequency;		//频率发
		array<float>^ m_fPRF;		// 对应深度档位
		bool m_bUseLaunchDeflection;//探头是否支持发射偏转 凸阵不支持发射偏转
		float m_nLaunchDeflectionAngle;//发射偏转角度（°） 单位度
	};

	public delegate void EventDelegate_HWMH(int msgType, int value);
	public delegate void HardWareMsgHandler(int msgType, int value);

	public delegate void EventDelegate_HWMHS(int msgType, int* value, int valueLen);
	public delegate void HardWareMsgSHandler(int msgType, array<int>^ value);

	public ref class NativeInterface
	{
	public:
		NativeInterface();

		~NativeInterface();

		void SetNative(CIES::Native* native);

		void SetDelegateMthod(HardWareMsgHandler^ d);
		void SetDelegateMthod(HardWareMsgSHandler^ d);

		void ClearDelegateMthod();
		void Callback_HWMH(int msgType, int value);
		void Callback_HWMHS(int msgType, int* value, int valueLen);
	private:
	
		System::Runtime::InteropServices::GCHandle delegateHandle_HWMH;
		EventDelegate_HWMH^ nativeCallback_HWMH;
		HardWareMsgHandler^ hardWareMsgHandler;

		System::Runtime::InteropServices::GCHandle delegateHandle_HWMHS;
		EventDelegate_HWMHS^ nativeCallback_HWMHS;
		HardWareMsgSHandler^ hardWareMsgSHandler;

	};


	public ref class CLScanMode
	{
	public:

		enum class CLScanModeEnum //扫描模式
		{
			B = 0x01,			//B 模式
			C = 0x02,			//C 模式
			D_PW = 0x04,		//D_PW 模式
			D = 0x10,			//D 模式
			BC = B | C,			//B&C 模式
			M = 0x20,			//M 模式
			BM = B | M			//B&M 模式
		};

		static bool ContainBScanMode(CLScanModeEnum scan_mode)	//包含B扫描模式
		{
			return ((scan_mode & CLScanModeEnum::B) == CLScanModeEnum::B);
		}

		static bool ContainCScanMode(CLScanModeEnum scan_mode)	//包含C扫描模式
		{
			return ((scan_mode & CLScanModeEnum::C) == CLScanModeEnum::C);
		}

		static bool ContainBCScanMode(CLScanModeEnum scan_mode)	//包含B&C扫描模式
		{
			return ((scan_mode & (CLScanModeEnum::BC)) == CLScanModeEnum::BC);
		}

		static bool ContainDPWScanMode(CLScanModeEnum scan_mode)	//包含D_PW扫描模式
		{
			return ((scan_mode & (CLScanModeEnum::D_PW)) == CLScanModeEnum::D_PW);
		}

		static bool ContainMScanMode(CLScanModeEnum scan_mode)	//包含M扫描模式
		{
			return ((scan_mode & (CLScanModeEnum::M)) == CLScanModeEnum::M);
		}

		static bool ContainBMScanMode(CLScanModeEnum scan_mode)	//包含B&M扫描模式
		{
			return ((scan_mode & (CLScanModeEnum::BM)) == CLScanModeEnum::BM);
		}

	};
	public ref class CLRUSDeviceInfo
	{
	public:
		CLRUSDeviceInfo();
		bool isPowerOff;						//是否处于低功耗
		int PID;								//USB VID
		int VID;								//USB PID
		System::String^ DevicePath;				//设备唯一识别路径
		int fwVersion_major;					//usb固件版主本
		int fwVersion_minor;					//usb固件版子本	
		int nLogicVersion_Major;				//逻辑版本号 主版本
		int nLogicVersion_Minor;				//逻辑版本号 次版本
		int nHWVersion_Major;					//硬件版本号 主版本
		int nHWVersion_Minor;					//硬件版本号 次版本
		unsigned int unLogicCompileVersion; 	//逻辑编译号
		unsigned short DNA1;					//FPGA_DNA 0-15位
		unsigned short DNA2;					//FPGA_DNA 16-31位
		unsigned short DNA3;					//FPGA_DNA 32-47位
		unsigned short DNA4;					//FPGA_DNA 48-56位
		float fUSBSupplyVoltage;				//USB供电电压	单位V
		float fTotalCurrent;					//总电流		单位A
		float fTemperature;						//温度			单位 ℃
		float fEmissionVoltage;					//发射电压		单位V
		bool ProbeCanreplaced;					//探头可更换
		int IsMultiProbe;						 //0 不是多探头版本 1 两个探头版本
		int probeID;							//探头ID /探头插座 A ID
		int probeAppendInfo;					//探头附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool probeConnect;						//探头ID/探头插座 A 0 不在位 1 在位
		int probeID_B;							//探头插座 B ID
		int probeAppendInfo_B;					//探头插座B 附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool probeConnect_B;					 //探头插座 B 0 不在位 1 在位
		System::String^ productInfo;			//产品信息
		System::String^ serialNumber;			//序列号
		bool isExportFlag;						//出口标识
		bool isOEMFlag;							//OEM标识
		int usHardWareIndex;					//超声设备的索引  如何出现设备拔出和插入，请重新获取。

		char PCB_Major;                 //PCB Major 主版本
		char PCB_Minor;                 //PCB Minor 子版本
		char PCBA_Major;                //PCBA Major 主版本
		char PCBA_Minor;                //PCBA Minor 子版本
	};

	public ref class CLRHardWareInfo
	{
	public:
		CLRHardWareInfo();

		int nLogicVersion_Major;				//逻辑版本号 主版本
		int nLogicVersion_Minor;				//逻辑版本号 次版本
		int nHWVersion_Major;					//硬件版本号 主版本
		int nHWVersion_Minor;					//硬件版本号 次版本
		unsigned int unLogicCompileVersion; 	//逻辑编译号
		unsigned short DNA1;					//FPGA_DNA 0-15位
		unsigned short DNA2;					//FPGA_DNA 16-31位
		unsigned short DNA3;					//FPGA_DNA 32-47位
		unsigned short DNA4;					//FPGA_DNA 48-56位
		float fUSBSupplyVoltage;				//USB供电电压	单位V
		float fTotalCurrent;					//总电流		单位A
		float fTemperature;						//温度			单位 ℃
		float fEmissionVoltage;					//发射电压		单位V
		int OtherMsg1;							//其他信息
		bool ProbeCanreplaced;					//探头可更换
		int IsMultiProbe;						  //0 不是多探头版本 1 两个探头版本
		int unProbeID;                            //探头ID/探头插座 A ID  使用标识 0x5555探头断开
		int unProbeAppendInfo;					  //探头附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool unProbeConnect;					  //探头ID/探头插座 A 0 不在位 1 在位
		int unProbeID_B;						  //探头插座B ID 使用标识 0x5555探头断开
		int unProbeAppendInfo_B;				  //探头插座B 附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool unProbeConnect_B;					  //探头插座 B 0 不在位 1 在位
	};
}