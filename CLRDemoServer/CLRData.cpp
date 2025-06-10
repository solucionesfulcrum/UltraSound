#include "stdafx.h"
#include "CLRData.h"

using namespace System;
using namespace System::Runtime::InteropServices;	// needed for Marshal

namespace CLRDemoServer {

	ImageDataInfo_CLR::ImageDataInfo_CLR(void)
	{
		m_fFrameRate = 0;
		m_nFrameWidthPixels = 0;
		m_nFrameHeightPixels = 0;
		m_nImageWidthPixels = 0;
		m_nImageHeightPixels = 0;
		m_fResolution = 0;
		m_nHour = 0;
		m_nMinute = 0;
		m_nSecond = 0;
		m_nMillisecond = 0;
		m_nDepth = 0;
		m_nFrameNumber = 0;
	}

	ImageDataInfo_CLR::~ImageDataInfo_CLR(void)
	{

	}

	
	ImageData::ImageData(void)
		:m_nD_PWImageDataMaxLen(1024 * 4 * 10)
		, m_nMImageDataMaxLen(1024 * 4 * 10)
	{

		m_B_Imagedata = nullptr;// gcnew array<int>(2048 * 2048);
		m_C_Imagedata = nullptr;// gcnew array<int>(2048 * 2048);
		m_D_PW_Imagedata = nullptr;// gcnew array<int>(1024 * 4 * 10);
		m_M_Imagedata = nullptr;//gcnew array<int>(2048 * 2048);

		m_B_ImageInfo = gcnew ImageDataInfo_CLR();
		m_C_ImageInfo = gcnew ImageDataInfo_CLR();
		m_D_PW_ImageInfos = gcnew array<D_PWDataInfo_CLR^>(10);
		for (int i = 0; i < 10; i++)
		{
			m_D_PW_ImageInfos[i] = gcnew D_PWDataInfo_CLR();
			m_D_PW_ImageInfos[i]->envelopeInfo = gcnew EnvelopeInfo_CLR();
		}

		m_M_ImageInfo = gcnew MDataInfo_CLR();

		m_nBImageDataLen = 0;
		m_nCImageDataLen = 0;
		m_nD_PWImageDataLen = 0;
		m_nMImageDataLen = 0;

		m_bBHadData = false;
		m_bCHadData = false;
		m_bD_PWHadData = false;
		m_bMHadData = false;

		m_nD_PWDataNum = 0;
		m_nMDataNum = 0;
	}

	ImageData::~ImageData(void)
	{
		if (m_B_Imagedata != nullptr)
		{
			delete m_B_Imagedata;
			m_B_Imagedata = nullptr;
		}
		if (m_C_Imagedata != nullptr)
		{
			delete m_C_Imagedata;
			m_C_Imagedata = nullptr;
		}
		if (m_D_PW_Imagedata != nullptr)
		{
			delete m_D_PW_Imagedata;
			m_D_PW_Imagedata = nullptr;
		}
		if (m_M_Imagedata != nullptr)
		{
			delete m_M_Imagedata;
			m_M_Imagedata = nullptr;
		}
		if (m_B_ImageInfo != nullptr)
		{
			delete m_B_ImageInfo;
			m_B_ImageInfo = nullptr;
		}

		if (m_C_ImageInfo != nullptr)
		{
			delete m_C_ImageInfo;
			m_C_ImageInfo = nullptr;
		}
		if (m_D_PW_ImageInfos != nullptr)
		{
			for (int i = 0; i < 10; i++)
			{
				if (m_D_PW_ImageInfos[i] != nullptr)
				{
					delete m_D_PW_ImageInfos[i];
				}
			}
			delete m_D_PW_ImageInfos;
			m_D_PW_ImageInfos = nullptr;
		}

		if (m_M_ImageInfo != nullptr)
		{
			delete m_M_ImageInfo;
			m_M_ImageInfo = nullptr;
		}		
	}
	ImageData_D_PW::ImageData_D_PW(void)
	{
		m_nD_PWImageDataLen = 0;
		m_D_PW_Imagedata = nullptr;
		m_D_PW_ImageInfos = nullptr;
		m_bD_PWHadData = false;
		m_nD_PWDataNum = 0;
	}

	ImageData_D_PW::~ImageData_D_PW(void)
	{
		if (m_nD_PWDataNum > 0)
		{
			if (m_D_PW_Imagedata != nullptr)
			{
				delete m_D_PW_Imagedata;
				m_D_PW_Imagedata = nullptr;
			}

			for (int i = 0; i < m_nD_PWDataNum; i++)
			{
				if (m_D_PW_ImageInfos[i] != nullptr)
				{
					delete m_D_PW_ImageInfos[i];
				}
			}

		}

	}

	ImageData_M::ImageData_M(void)
	{
		m_nMImageDataLen = 0;
		m_M_ImageData = nullptr;
		m_M_ImageInfos = nullptr;
		m_bMHadData = false;
		m_nMDataNum = 0;
	}

	ImageData_M::~ImageData_M(void)
	{
		if (m_nMDataNum > 0)
		{
			if (m_M_ImageData != nullptr)
			{
				delete m_M_ImageData;
				m_M_ImageData = nullptr;
			}

			for (int i = 0; i < m_nMDataNum; i++)
			{
				if (m_M_ImageInfos[i] != nullptr)
				{
					delete m_M_ImageInfos[i];
				}
			}

		}
	}

	EnvelopeInfo_CLR::EnvelopeInfo_CLR()
	{
		m_bHadData = false;
		m_nEnvelopeMax = 0;
		m_nEnvelopeMean = 0;
	}
	D_PWDataInfo_CLR::D_PWDataInfo_CLR()
	{
		envelopeInfo = gcnew EnvelopeInfo_CLR();
		m_nDateNum = 0;
		m_AutoMeasureAngle = -999;
		m_BloodRadius_T = -999;
		m_BloodRadius_B = -999;
		m_nD_PWDataLen = 0;
	}

	D_PWDataInfo_CLR::~D_PWDataInfo_CLR()
	{
		if (envelopeInfo != nullptr)
		{
			delete envelopeInfo;
			envelopeInfo = nullptr;
		}
	}
	MDataInfo_CLR::MDataInfo_CLR()
	{
		m_nDateNum = 0;
		m_nMDataLen = 0;

		m_nFrameNumber = 0;
		m_nImageHeightPixels = 0;
		m_nMLineNum = 0;
		m_bClearScreen = false;
	}

	MDataInfo_CLR::~MDataInfo_CLR()
	{

	}
	B_ImageParam_CLR::B_ImageParam_CLR()
	{
	
		m_B_ImageParamType = (int)B_ImageParamType::All_Param;
		m_nWidth = -999;
		m_nHeight = -999;
		m_nDepth_level = -999;
		m_fGain = -999;
		m_fDR = -999;
		m_SRI_Level = -999;
		m_nCorrelation_Level = -999;
		m_nGrayColorMap_level = -999;
		m_nPseudoColorMap_level = -999;
		m_fTGC = gcnew array<float>(6);
		m_nHarmonic = -999;
		m_nFrequency = -999;
		m_nFocusArea = -999;
		m_nNE = -999;
		m_nNE_Theta = -999;
	}
	B_ImageParam_CLR::~B_ImageParam_CLR()
	{
		if (m_fTGC != nullptr)
		{
			delete m_fTGC;
			m_fTGC = nullptr;
		}
	}

	C_ImageParam_CLR::C_ImageParam_CLR()
	{
		m_C_ImageParamType = (int)C_ImageParamType::All_Param;

		m_fGain = -999;
		m_nWallFilter_level = -999;
		m_nColorPriority_level = -999;
		m_nFrameCorrelation_level = -999;
		m_nColorMap_mode = -999;
		m_nColorMap_level = -999;
		m_nColorMap_inversion = -999;
		m_nPRF_Level = -999;
		m_fTheta = -999;
		m_bUse_B_BC_Mode = false;
		m_nSpeed = -999;
	}

	C_ImageParam_CLR::~C_ImageParam_CLR()
	{

	}
	D_PW_ImageParam_CLR::D_PW_ImageParam_CLR()
	{
		m_D_PW_ImageParamType = (int)D_PW_ImageParamType::All_Param;

		m_nWidth = -999;
		m_nHeight = -999;
		m_prf_rate = -999;
		m_fGain = -999;
		m_fDR = -999;
		m_fFrequency = -999;
		m_fTime = -999;
		m_nBaseLineLevel = -999;
		m_nWall_level = -999;
		m_nSamplingVolume = -999;
		m_nInversion = -999;
		m_nSpeed = -999;
		m_nGrayColorMap_level = -999;
		m_nPseudoColorMap_level = -999;
	}

	D_PW_ImageParam_CLR::~D_PW_ImageParam_CLR()
	{

	}

	M_ImageParam_CLR::M_ImageParam_CLR()
	{
		m_M_ImageParamType = (int)M_ImageParamType::All_Param;
		m_fGain = -999;
		m_fTime = -999;
		m_nWidth = -999;
		m_nHeight = -999;
		m_nGrayColorMap_level = -999;
		m_nPseudoColorMap_level = -999;
	}

	M_ImageParam_CLR::~M_ImageParam_CLR()
	{

	}

	Probe_Info::Probe_Info()
	{
		m_fPith = 0;
		m_fRadiusCurvature = 0;
		m_nFocusNum = 0;
		Probe_type = 0;
		m_fImageAngle = 0.0f;
		m_nShowDepthLevel = 0;
		m_pDepthList = nullptr;

		m_bC_Tx_deflectionflag = false;
		m_pC_Tx_angle = gcnew array<int>(5);
		m_bD_Tx_deflectionflag = false;
		m_pD_Tx_angle = gcnew array<int>(5);
		m_bB_NE_deflectionflag = false;
		m_pB_NE_Theta_angle = gcnew array<int>(4);

		m_fB_fund_Tx_freq = gcnew array<float>(3);
		m_fB_harm_Tx_freq = gcnew array<float>(3);
		m_fC_Tx_freq = gcnew array<float>(2);
		m_fD_Tx_freq = gcnew array<float>(2);
	}
	Probe_Info::~Probe_Info()
	{
		if (m_pDepthList != nullptr)
		{
			delete m_pDepthList;
			m_pDepthList = nullptr;
		}

		delete m_pC_Tx_angle;
		m_pC_Tx_angle = nullptr;
		delete m_pD_Tx_angle;
		m_pD_Tx_angle = nullptr;

		delete m_pB_NE_Theta_angle;
		m_pB_NE_Theta_angle = nullptr;

		delete m_fB_fund_Tx_freq;
		m_fB_fund_Tx_freq = nullptr;
		delete m_fB_harm_Tx_freq;
		m_fB_harm_Tx_freq = nullptr;
		delete m_fC_Tx_freq;
		m_fC_Tx_freq = nullptr;
		delete m_fD_Tx_freq;
		m_fD_Tx_freq = nullptr;

	}

	D_PWInfo::D_PWInfo()
	{
		m_nFrequency = 0;
		m_fPRF = nullptr;
		m_bUseLaunchDeflection = false;
		m_nLaunchDeflectionAngle = 0;
	}

	D_PWInfo::~D_PWInfo()
	{
		if (m_fPRF != nullptr)
		{
			delete m_fPRF;
			m_fPRF = nullptr;
		}
	}


	typedef void(__stdcall* EventCallback_HW)(int, int);
	typedef void(__stdcall* EventCallback_HWS)(int, int*, int);
	NativeInterface::NativeInterface()
	{
		nativeCallback_HWMH = nullptr;
		hardWareMsgHandler = nullptr;

		nativeCallback_HWMHS = nullptr;
		hardWareMsgSHandler = nullptr;

	}

	NativeInterface::~NativeInterface()
	{
		if (nativeCallback_HWMH != nullptr)
		{
			// 释放委托句柄  
			if (this->delegateHandle_HWMH.IsAllocated)
				this->delegateHandle_HWMH.Free();
		}

		if (nativeCallback_HWMHS != nullptr)
		{
			// 释放委托句柄  
			if (this->delegateHandle_HWMHS.IsAllocated)
				this->delegateHandle_HWMHS.Free();
		}
	}

	void NativeInterface::SetNative(CIES::Native* native)
	{
		
		// 从成员函数创建一个委托  
		this->nativeCallback_HWMH = gcnew EventDelegate_HWMH(this, &NativeInterface::Callback_HWMH);

		// 保证委托不会被内存移动和垃圾回收掉  
		this->delegateHandle_HWMH = System::Runtime::InteropServices::GCHandle::Alloc(this->nativeCallback_HWMH);

		// 转换为函数指针注册  
		System::IntPtr ptr2 = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(this->nativeCallback_HWMH);
		native->RegisterCallback(static_cast<EventCallback_HW>(ptr2.ToPointer()));

		// 从成员函数创建一个委托  
		this->nativeCallback_HWMHS = gcnew EventDelegate_HWMHS(this, &NativeInterface::Callback_HWMHS);

		// 保证委托不会被内存移动和垃圾回收掉  
		this->delegateHandle_HWMHS = System::Runtime::InteropServices::GCHandle::Alloc(this->nativeCallback_HWMHS);

		// 转换为函数指针注册  
		System::IntPtr ptr4 = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(this->nativeCallback_HWMHS);
		native->RegisterCallback(static_cast<EventCallback_HWS>(ptr4.ToPointer()));

	
	}

	void NativeInterface::SetDelegateMthod(HardWareMsgHandler^ d)
	{
		hardWareMsgHandler = d;
	}
	void NativeInterface::SetDelegateMthod(HardWareMsgSHandler^ d)
	{
		hardWareMsgSHandler = d;
	}
	void NativeInterface::ClearDelegateMthod()
	{
		hardWareMsgHandler = nullptr;
		hardWareMsgSHandler = nullptr;
	}

	void NativeInterface::Callback_HWMH(int msgType, int value)
	{
		if (hardWareMsgHandler != nullptr)
		{
			hardWareMsgHandler(msgType, value);
		}
	}
	void NativeInterface::Callback_HWMHS(int msgType, int* value, int valueLen)
	{
		if (hardWareMsgSHandler != nullptr)
		{
			if (valueLen > 0)
			{
				array<int>^ nmsgArray = gcnew array<int>(valueLen);
				Marshal::Copy((IntPtr)value, nmsgArray, 0, valueLen);

				hardWareMsgSHandler(msgType, nmsgArray);
			}
			else
			{
				hardWareMsgSHandler(msgType, nullptr);
			}
		}
	}
	CLRUSDeviceInfo::CLRUSDeviceInfo()
	{
		isPowerOff = false;
		PID = 0;
		VID = 0;
		DevicePath = nullptr;
		fwVersion_major = 0;
		fwVersion_minor = 0;
		nLogicVersion_Major = 0;
		nLogicVersion_Minor = 0;
		nHWVersion_Major = 0;
		nHWVersion_Minor = 0;
		unLogicCompileVersion = 0;
		DNA1 = 0;
		DNA2 = 0;
		DNA3 = 0;
		DNA4 = 0;
		fUSBSupplyVoltage = 0;
		fTotalCurrent = 0;
		fTemperature = 0;
		fEmissionVoltage = 0;

		ProbeCanreplaced = false;
		IsMultiProbe = 0;
		probeID = 0;
		probeAppendInfo = 0;
		probeConnect = false;
		probeID_B = 0;
		probeAppendInfo_B = 0;
		probeConnect_B = false;

		productInfo = nullptr;
		serialNumber = nullptr;
		isExportFlag = false;
		isOEMFlag = false;
		usHardWareIndex = 0;

		PCB_Major = 0;
		PCB_Minor = 0;
		PCBA_Major = 0;
		PCBA_Minor = 0;
	}


	CLRHardWareInfo::CLRHardWareInfo()
	{
		nLogicVersion_Major = 0;
		nLogicVersion_Minor = 0;
		nHWVersion_Major = 0;
		nHWVersion_Minor = 0;
		unLogicCompileVersion = 0;
		DNA1 = 0;
		DNA2 = 0;
		DNA3 = 0;
		DNA4 = 0;
		fUSBSupplyVoltage = 0;
		fTotalCurrent = 0;
		fTemperature = 0;
		fEmissionVoltage = 0;
		OtherMsg1 = 0;
		ProbeCanreplaced = false;
		IsMultiProbe = 0;
		unProbeID = 0;
		unProbeAppendInfo = 0;
		unProbeConnect = 0;
		unProbeID_B = 0;
		unProbeAppendInfo_B = 0;
		unProbeConnect_B = 0;

	}
}