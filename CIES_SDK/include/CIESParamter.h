//File：IESParamter.h
//Purpose：定义数据引擎参数接口。
#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif

namespace CIES {


	class Version {
	public:
		int Major;		//主版本
		int Minor;		//次版本
		int Revision;	//修订号
	};


	class COLORIMAGEENGINESERVER_API B_ImageParam
	{
	public:
		B_ImageParam();

		~B_ImageParam();

		B_ImageParam &operator=(const B_ImageParam &s);


		enum B_ImageParamType {
			All_Param = 0x7FFF,						//所有参数
			Width_Param = 0x0001,					//DSC宽（像素） 输出图像的宽
			Height_Param = 0x0002,					//DSC高（像素）	输出图像的高
			Depth_level_Param = 0x0004,				//深度档位
			Gain_Param = 0x0008,					//增益
			DR_Param = 0x0010,						//动态范围
			SRI_level_Param = 0x0020,				//图像增强档位
			Correlation_level_Param = 0x0040,		//帧相关档位
			TGC_Param = 0x0080,						//六段TGC
			GrayColorMap_level_Param = 0x0100,		//灰度映射Map档位
			PseudoColorMap_level_Param = 0x0200,	//伪彩映射Map档位
			Harmonic_level_Param = 0x0400,			//谐波
			Frequency_level_Param = 0x0800,			//频率
			FocusArea_level_Param = 0x1000,			//焦区
			NE_Param = 0x2000,                      //针增强
			NE_Theta_Param = 0x4000,                //针增强角度
		};

		int m_B_ImageParamType;			//参数类型 B_ImageParamType 变量的集合

		int m_nWidth;					//DSC宽（像素）输出图像的宽 （0~2048）
		int m_nHeight;					//DSC高（像素）输出图像的高  （0~2048）
		int m_nDepth_level;				//深度档位 （0~n，n一般为6档）
		float m_fGain;					//增益范围 （1~100）
		float m_fDR;					//动态范围	1~100）
		int m_SRI_Level;				//图像增强档位 （1~8）
		int m_nCorrelation_Level;		//帧相关系数 （1~4）
		float m_fTGC[6];				//六段TGC （-15~15）
		int m_nGrayColorMap_level;		//灰度映射Map档位 （1~9，9为自定义档位）
		int m_nPseudoColorMap_level;	//伪彩映射Map档位 （0~8，0为关）
		int m_nHarmonic;				//谐波	（0:基波；1：谐波）
		int m_nFrequency;				//频率	（0：低频;1：中频；2：高频；）
		int m_nFocusArea;            	//0：焦点1;1：焦点2；2：焦点3；3：焦点4；4：焦点5；5：焦点6；6：全域；
		int m_nNE;                    	//针增强 0:关;1:开;
		int m_nNE_Theta;           		//针增强角度: -30°- 30°;（角度值从ProbeInfo.m_pB_NE_Theta_angle参数中获取）
	};


	class COLORIMAGEENGINESERVER_API C_ImageParam
	{
	public:
		C_ImageParam();

		~C_ImageParam();

		C_ImageParam &operator=(const C_ImageParam &s);

		enum C_ImageParamType {
			All_Param = 0x07FF,						//所有参数
			Gain_Param = 0x0001,					//增益
			WallFilter_level_Param = 0x0002,		//壁滤波档位
			ColorPriority_level_Param = 0x0004,		//彩色优先度档位
			FrameCorrelation_level_Param = 0x0008,  //彩色帧相关档位
			Color_Mode_Param = 0x0010,				//模式
			ColorMap_level_Param = 0x0020,			//映射Map档位
			ColorMap_Inversion_Param = 0x0040,		//图像翻转
			PRF_Level_Param = 0x0080,				//PRF档位
			Theta_Param = 0x0100,					//发射偏转角度
			B_BC_Mode_Param = 0x0200,				//使用 B & BC模式
			Speed_Param = 0x0400,					//速度
		};

		int m_C_ImageParamType;			//参数类型 C_ImageParamType 变量的集合

		float m_fGain;					//增益 (0~100)
		int m_nWallFilter_level;		//壁滤波档位 (0~4)
		int m_nColorPriority_level;		//彩色优先度档位（0~4）
		int m_nFrameCorrelation_level;	//彩色帧相关档位（0~4)
		int m_nColor_mode;				//模式（0：速度模式；1：能量模式；）
		int m_nColorMap_level;			//映射Map档位 （0~11)
		int m_nColorMap_inversion;		//图像翻转 （0：正常；1：翻转）
		int m_nPRF_Level;				//PRF档位  （0~7)
		float m_fTheta;					//发射偏转角度 （角度值从ProbeInfo.m_pC_Tx_angle参数中获取）
		bool m_bUse_B_BC_Mode;			//使用 B & BC模式
		int m_nSpeed;					//速度 （0：低速，1：高速）
	};


	class COLORIMAGEENGINESERVER_API D_PW_ImageParam
	{
	public:
		D_PW_ImageParam();

		~D_PW_ImageParam();

		D_PW_ImageParam &operator=(const D_PW_ImageParam &s);

		enum D_PW_ImageParamType {
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

		int m_D_PW_ImageParamType;    //参数类型

		int m_nWidth;				//显示像素宽度  最大值2048 需要是32的整数倍
		int m_nHeight;				//显示像素高度 最大值2048 需要是32的整数倍
		int m_prf_rate;				//PRF rate
		float m_fGain;				//增益范围 -20- 20 默认0
		float m_fDR;				//动态范围	20 - 60 默认40
		float m_fFrequency;			//频率
		float m_fTime;				//显示时间
		int m_nBaseLineLevel;		//基线level -3 - 3
		int m_nWall_level;			//壁滤波Level 0 - 2
		int m_nSamplingVolume;		//取样门宽度 1- 10mm
		int m_nInversion;    		//图像翻转 正常：0；	翻转：1
		int m_nSpeed;               //速度 0：低速，1：高速
		int m_nGrayColorMap_level;  //灰度映射Map档位 1 - 8
		int m_nPseudoColorMap_level;//伪彩映射Map档位，0为默认 0 - 8
	};


	class COLORIMAGEENGINESERVER_API M_ImageParam
	{
	public:
		M_ImageParam();

		~M_ImageParam();

		M_ImageParam &operator=(const M_ImageParam &s);

		enum M_ImageParamType {
			All_Param = 0x003F,
			Gain_Param = 0x0001,
			Time_Param = 0x0002,
			Width_Param = 0x0004,
			Height_Param = 0x0008,
			GrayColorMap_level_Param = 0x0010,
			PseudoColorMap_level_Param = 0x0020
		};

		int m_M_ImageParamType;    //参数类型

		float m_fGain;//增益范围 0-100
		float m_fTime;//显示时间
		int m_nWidth;                //M宽（像素） Max 2048
		int m_nHeight;                //M高（像素）  Max 2048
		int m_nGrayColorMap_level;    //灰度映射Map档位 1 - 8
		int m_nPseudoColorMap_level;  //伪彩映射Map档位，0为默认 0 - 8
	};


	class COLORIMAGEENGINESERVER_API ProbeInfo
	{
	public:
		ProbeInfo();

		~ProbeInfo();

		ProbeInfo &operator=(const ProbeInfo &s);

		float m_fProbe_pitch;			//振元间距 mm
		float m_fProbe_r;				//曲率半径 mm
		float m_fProbe_lens;			//声透镜厚度 mm
		int m_nProbe_element;			//振元数量
		int m_nProbe_type;				//探头类型 （0：线阵；1：凸阵；2：相控阵；）
		int m_nAD_downsample;			//AD降采样率 （1：1倍降采样（约40MHz），2: 2倍降采样（约20MHz））
		float m_fClock_frequency;		//时钟频率 （默认156.25MHz）
		int m_nCH_physical_RX;			//接收通道数 ：32，16，8
		int m_nADC_type;				//ADC芯片型号
		int m_nRate_control;			//控制事件rate
		float m_fImageAngle;			//相控阵张角
		float m_fSound_velocity[4];		//声速 单位m/s，依次为常规、液体、脂肪、肌肉中的超声传播速度
		int m_nHV_SW;					//高压开关控制。
		int m_nCH_physical_TX;          //发射通道数 ：64，32，16
		bool m_bNE_On_Off;            	//针增强开关 0：关，1：开
		
		bool m_bB_Tx_deflectionflag;    //B是否有发射偏转   未使用
		int m_pB_Tx_angle[5];           //B发射偏转角度   未使用

		bool m_bC_Tx_deflectionflag;    //C是否有发射偏转
		int m_pC_Tx_angle[5];           //C发射偏转角度

		bool m_bD_Tx_deflectionflag;    //D是否有发射偏转
		int m_pD_Tx_angle[5];           //D发射偏转角度

		bool m_bB_NE_deflectionflag;    //B是否有针增强
		int m_pB_NE_Theta_angle[4];     //B针增强角度

		float m_fB_fund_Tx_freq[3];		//频率：基波发射频率依次为：高，中，低，单位MHz
		float m_fB_harm_Tx_freq[3];		//频率：谐波发射频率依次为：高，中，低，单位MHz
		float m_fC_Tx_freq[2];			//发射显示频率 依次为高频（低速），低频（高速）
		float m_fD_Tx_freq[2];			//发射显示频率 依次为高频（低速），低频（高速）
		int m_nShowDepthLevel;			//显示深度档位
		int *m_pDepthList;				//深度列表 单位mm 显示需要*10
		void Clear();
	};


	class COLORIMAGEENGINESERVER_API D_PWInfo
	{
	public:
		D_PWInfo();

		~D_PWInfo();

		D_PWInfo &operator=(const D_PWInfo &s);

		float m_nFrequency;        //频率
		float m_fPRF[8];            // 对应深度档位
		bool m_bUseLaunchDeflection;//探头是否支持发射偏转 凸阵不支持发射偏转
		float m_nLaunchDeflectionAngle;//发射偏转角度（°） 单位度
	};


	class COLORIMAGEENGINESERVER_API ScanMode
	{
	public:
		ScanMode::ScanMode()
		{
		}

		enum ScanModeEnum        //扫描模式
		{
			B = 0x01,            //B 模式
			C = 0x02,            //C 模式

			D_PW = 0x04,        //D_PW 模式
			D = 0x10,            //D 模式

			BC = B | C,            //B&C 模式

			M = 0x20,            //M 模式
			BM = B | M            //B&M 模式

		};

		static bool ContainBScanMode(ScanModeEnum scan_mode)    //包含B扫描模式
		{
			return ((scan_mode & B) == B);
		}

		static bool ContainCScanMode(ScanModeEnum scan_mode)    //包含C扫描模式
		{
			return ((scan_mode & C) == C);
		}

		static bool ContainBCScanMode(ScanModeEnum scan_mode)    //包含B&C扫描模式
		{
			return ((scan_mode & (BC)) == BC);
		}


		static bool ContainDPWScanMode(ScanModeEnum scan_mode)    //包含D_PW扫描模式
		{
			return ((scan_mode & (D_PW)) == D_PW);
		}

		static bool ContainDScanMode(ScanModeEnum scan_mode)    //包含D扫描模式
		{
			return ((scan_mode & (D)) == D);
		}

		static bool ContainMScanMode(ScanModeEnum scan_mode)    //包含M扫描模式
		{
			return ((scan_mode & (M)) == M);
		}

		static bool ContainBMScanMode(ScanModeEnum scan_mode)    //包含B&M扫描模式
		{
			return ((scan_mode & (BM)) == BM);
		}

	};


	class COLORIMAGEENGINESERVER_API HWInfo
	{
	public:
		HWInfo();

		~HWInfo();

		int ParsingMSG(unsigned char msg[512]); //解析序列码
		void Clear();

		HWInfo &operator=(const HWInfo &s);

		int nLogicVersion[2];                   //逻辑版本号 ;m_unLogicVersion[0].m_unLogicVersion[1]
		int nHWVersion[2];                      //硬件版本号 ;m_unHWVersion[0].m_unHWVersion[1]
		unsigned int unLogicCompileVersion;		//逻辑编译号
		unsigned short DNA1;                    //FPGA_DNA 0-15位
		unsigned short DNA2;                    //FPGA_DNA 16-31位
		unsigned short DNA3;                    //FPGA_DNA 32-47位
		unsigned short DNA4;                    //FPGA_DNA 48-56位
		float fUSBSupplyVoltage;                //USB供电电压		单位V
		float fTotalCurrent;                    //总电流			单位A
		float fTemperature;                     //温度				单位 ℃
		float fEmissionVoltage;                 //发射电压			单位V
		bool getInfoFlag;                       //获取到信息标识
		int OtherMsg1;
		bool ProbeCanreplaced;                   //探头可更换
		int IsMultiProbe;						  //0 不是多探头版本 1 两个探头版本
		int unProbeID;                            //探头ID/探头插座 A ID  使用标识 0x5555探头断开
		int unProbeAppendInfo;					  //探头附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool unProbeConnect;					  //探头ID/探头插座 A 0 不在位 1 在位
		int unProbeID_B;						  //探头插座B ID 使用标识 0x5555探头断开
		int unProbeAppendInfo_B;				  //探头插座B 附加消息  低16位有效，0位：是否是可插拔探头；1位：是否支持按键
		bool unProbeConnect_B;					  //探头插座 B 0 不在位 1 在位
	};


	class COLORIMAGEENGINESERVER_API ProductInfo
	{
	public:
		ProductInfo();

		~ProductInfo();

		void ParsingMSG(unsigned char msg[32]); //解析序列码
		void Clear();

		ProductInfo &operator=(const ProductInfo &s);

		char productCategory[2];		//产品类别
		short productVersion;			//产品版本
		short probeID;					//探头ID //针对USB 探头有效
		char describe[25];				//描述信息
		bool getInfoFlag;				//获取到信息标识
	};

	class COLORIMAGEENGINESERVER_API SNInfo
	{
	public:
		SNInfo();

		~SNInfo();

		void ParsingMSG(unsigned char msg[32]); //解析SN
		void Clear();

		SNInfo &operator=(const SNInfo &s);

		char serialNumber[32];                    //序列号
		bool getInfoFlag;                        //获取到信息标识
	};

	class COLORIMAGEENGINESERVER_API OtherInfo
	{
	public:
		OtherInfo();

		~OtherInfo();

		void ParsingMSG(unsigned char msg[8]); //解析OtherInfo
		void Clear();

		OtherInfo &operator=(const OtherInfo &s);

		bool isExportFlag;					//出口标识
		bool isOEMFlag;						//OEM标识
		unsigned short oEMCustomerID;		//OEM customer ID
		char unProbePos;					//探头位置。未启用
		char unReserve[4];					//没有使用
		bool getInfoFlag;					//获取到信息标识
	};

	class COLORIMAGEENGINESERVER_API PCB_PCBA_Info
	{
	public:
		PCB_PCBA_Info();

		~PCB_PCBA_Info();

		void ParsingMSG(unsigned char msg[4]); //解析PCB_PCBA_Info
		void Clear();

		PCB_PCBA_Info &operator=(const PCB_PCBA_Info &s);

		char PCB_Major;                 //PCB Major 主版本
		char PCB_Minor;                 //PCB Minor 子版本 2位显示例如1显示成01
		char PCBA_Major;                //PCBA Major 主版本
		char PCBA_Minor;                //PCBA Minor 子版本 2位显示例如1显示成01

	};


	//超声硬件信息
	class COLORIMAGEENGINESERVER_API USDeviceInfo
	{
	public:
		USDeviceInfo();

		~USDeviceInfo();

		void Clear();

		USDeviceInfo &operator=(const USDeviceInfo &s);

		bool isPowerOff;			//是否处于低功耗
		int VID;					//USB VID
		int PID;					//USB PID
		char DevicPath[256];		//设备唯一识别路径
		int fwVersion_major;		//usb固件版主本
		int fwVersion_minor;		//usb固件版子本
		HWInfo hwInfo;				//硬件信息
		ProductInfo productInfo;    //产品信息
		SNInfo snInfo;              //SN信息
		OtherInfo otherInfo;        //其他信息
		PCB_PCBA_Info pcb_PCBA_Info;//PCB PCBA 信息
		int usHardWareIndex;    //超声设备的索引  如何出现设备拔出和插入，请重新获取。
	};
}