// CLRDemoServer.h

#pragma once

#include "ColorImageEngineServer.h"
#include "DataSource.h"
#include "IESParamter.h"
#include "Native.h"
namespace CLRDemoServer {

	public ref class Probe_Info
	{
	public:
		Probe_Info();
		~Probe_Info();

		float m_fPith;				//探头Pith
		float m_fRadiusCurvature;	//探头曲率半径
		int m_nFocusNum;			//探头震源数量
		int m_nType;				//探头1倍降采样，2倍降采样,3倍降采样
		int m_nPA;					//是否是相控阵 1:是；0：否
		int m_nShowDepthLevel;		//显示深度档位
		array<int>^	m_pDepthList;	//深度列表 单位mm 显示需要*10	
	};

	public ref class SignalParam
	{
	public:
		SignalParam();
		~SignalParam();

		enum class SignalParamType
		{
			All_Signal_Param = 0x00FF,
			Width_Param = 0x0001,
			High_Param = 0x0002,
			Depth_Param = 0x0004,
			Gain_Param = 0x0008,
			DR_Param = 0x0010,
			ImageEnhancementLevel_Param = 0x0020,
			Correlation_Param = 0x0040,
			TGC_Param = 0x0080
		};

		int m_SignalParamType;


		int m_nWidth;//显示像素宽度
		int m_nHigh;//显示像素高度
		int m_nDepth;//深度档位
		float m_fGain;//增益范围 0 - 100
		float m_fDR;//动态范围 0 - 100
		unsigned char m_ucImageEnhancementLevel;//图像增强档位 1 - 8
		unsigned char m_ucCorrelation;//帧相关系数 1 - 8
		array<float>^ m_fTGC;
	};

	public ref class ImageData
	{
	public:
		ImageData(void);
		~ImageData(void);
		array<unsigned char>^	m_data;

		float m_fFrameRate;			//帧率
		int m_nFrameWidthPixels;	//帧横向分辨率
		int m_nFrameHeightPixels;	//帧纵向分辨率
		int m_nImageWidthPixels;	//图像横向分辨率
		int m_nImageHeightPixels;	//图像纵向分辨率
		float m_fAspectRatio;//横纵比 有图像的区域，非整幅图
		int m_nHour; //时
		int m_nMinute; //分
		int m_nSecond; //秒
		int m_nMillisecond; //毫秒
		int m_nDepth;//当前深度
		int m_nFrameNumber;//帧编号
	};

	public ref class EnvelopeInfoCLR
	{
	public:
		EnvelopeInfoCLR();
		bool    bHadData;//是否有数据
		short	m_nEnvelope;
	};


	public ref class DopplerDataCLR
	{
	public:
		DopplerDataCLR();
		~DopplerDataCLR();

		array<unsigned char>^	m_uSpectralLine;	//谱线
		EnvelopeInfoCLR^ m_envelopeInfo;		//包络

		int nDateNum;						//数据编号

	};

	public ref class DopplerParamCLR
	{
	public:
		DopplerParamCLR();
		~DopplerParamCLR();

		enum class DopplerParamCLRType
		{
			All_Doppler_Param = 0x03FF,
			Width_Param = 0x0001,
			High_Param = 0x0002,
			PRF_Rate_Param = 0x0004,
			Gain_Param = 0x0008,
			DR_Param = 0x0010,
			Frequency_Param = 0x0020,
			Time_Param = 0x0040,
			BaseLine_Level_Param = 0x0080,
			Wall_Level_Param = 0x0100,
			Sampling_Volume_Param = 0x0200,
		};

		int m_dopplerParamType; //参数类型

		int m_nWidth;//显示像素宽度 
		int m_nHeight;//显示像素高度
		int m_prf_rate;//PRF rate
		float m_fGain;//增益范围 -20- 20 默认0
		float m_fDR;//动态范围	20 - 60 默认40
		float m_fFrequency;//频率  PW_Info 中获取
		float m_fTime;//显示时间 4 6 8
		int m_nBaseLineLevel;//基线level -3 - 3
		int m_nWall_level;//壁滤波Level 0 - 2
		int m_nSamplingVolume;//取样门宽度 32 64 128 256
	};

	public ref class PW_Info
	{
	public:
		PW_Info();
		~PW_Info();

		float m_nFrequency;		//频率发
		array<float>^ m_fPRF;		// 对应深度档位
	};

	public delegate void EventDelegate(int i);
	public delegate void FrameDataUpdated(int nFrameDataNumber);
	public delegate void EventDelegate_HWMH(int msgType, int value);
	public delegate void HardWareMsgHandler(int msgType, int value);

	public ref class NativeInterface
	{
	public:
		NativeInterface();

		~NativeInterface();

		void SetNative(IES::Native* native);

		void SetDelegateMthod(FrameDataUpdated^ d);
		void SetDelegateMthod(HardWareMsgHandler^ d);

		void Callback(int i);
		void Callback_HWMH(int msgType, int value);
	private:
		System::Runtime::InteropServices::GCHandle delegateHandle;
		EventDelegate^ nativeCallback;
		FrameDataUpdated^ frameDataUpdated;
		System::Runtime::InteropServices::GCHandle delegateHandle_HWMH;
		EventDelegate_HWMH^ nativeCallback_HWMH;
		HardWareMsgHandler^ hardWareMsgHandler;
	};

	public ref class CLR_DemoServer
	{
	private:
		CLR_DemoServer(void);
		~CLR_DemoServer(void);
		void ReleaseResource();
		void LoadKRProcessClass();

		static CLR_DemoServer^ m_Instance = nullptr;

		IES::ImageEngineServer* m_pDemoServer;
		IES::DataSource* m_pDataSource;

		IES::DopplerData* pDopplerData;
		IES::DopplerData* pDopplerDataArray;
		int m_nDopplerDataArrayLen;

		NativeInterface^ _nativeInterface;
		IES::Native* m_pNative;
	public:

		static CLR_DemoServer^ GetInstance();	//获取单例

		static void ReleaseInstance();		//注释

		int SetCheckModeParamPath(System::String^ paramPath);	//设置参数目录

		int ResetFW();	//重置硬件固件
		int DownLoadFW(System::String^ fwFilePath);	//下载USB固件

		int StartImageEngine();				//启动图像引擎
		int StopImageEngine();				//停止图像引擎

		void Freeze();
		void UnFreeze();

		bool IsRuning();

		int GetProberInfo(Probe_Info^ probeInfo, System::String^ paramPath); //获取探头信息
		int SetSignalParam(SignalParam^ signalParam); //设置算法参数

		int GetImageWidthPixels();
		int GetImageHeightPixels();

		int GetImageDisplayData(ImageData^ imageData);	//获取显示数据
		int GetHistoryImageCount();						//获取历史图像数据量(录像数据量)
		int GetHistoryImageData(ImageData^ imageData, int nIndex); //获取历史数据
		float GetCurrentFrameRate();					//获取当前帧率

		bool IsUSBOpen();	//USB设备是否开启

		int GetHardWareVersion(array<int >^	HWversion); //获取硬件版本

		int GetSDKVersion(array<int >^	SDKversion); //获取SDK版本

		void SetDelegateMthod(FrameDataUpdated^ d);
		void SetDelegateMthod(HardWareMsgHandler^ d);

	public:
		int GetPWInfo(PW_Info^ pwInfo, System::String^ paramPath);			 //获取PW显示信息
		
		int SetDopplerParam(DopplerParamCLR^ dopplerParamClr);	//设置多普勒参数

		int StartDoppler();	//启动多普勒
		int StopDoppler();	//关闭多普勒

		int SetD_PWSamplingGateParam_Line(float SX, float SY, float depth, float fIWidthP, float fIHeightP, int launchDeflectionAngle, int samplingVolume);
		int SetD_PWSamplingGateParam_Convex(float SX, float SY, float depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume);

		int GetDopplerDisplayData(array<DopplerDataCLR^>^ dopplerDataClrArray, int dataNumber); // 获取多普勒数据

		int GetHistoryDopplerData(array<DopplerDataCLR^>^ dopplerDataClrArray, int startIndex, int dataNumber);//获取缓存的多普勒数据
		int GetHistoryDopplerDataCount();//获取缓存多普勒数据数量

	


	};
}
