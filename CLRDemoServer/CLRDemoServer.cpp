// 这是主 DLL 文件。

#include "stdafx.h"

#include "CLRDemoServer.h"

using namespace System;
using namespace System::Runtime::InteropServices;	// needed for Marshal

namespace CLRDemoServer {

#define IMAGE_DATA_NUM 2048*2048
	Probe_Info::Probe_Info()
	{
		m_fPith = 0;
		m_fRadiusCurvature = 0;
		m_nFocusNum = 0;
		m_nType = 0;
		m_nPA = 0;
		m_nShowDepthLevel = 0;
		m_pDepthList = nullptr;
	}
	Probe_Info::~Probe_Info()
	{
		if (m_pDepthList != nullptr)
		{
			delete m_pDepthList;
			m_pDepthList = nullptr;
		}
	}

	SignalParam::SignalParam()
	{
		m_nDepth = -999;
		m_nWidth = -999;
		m_nHigh = -999;
		m_fGain = -999;
		m_fDR = -999;
		m_ucImageEnhancementLevel = (unsigned char)128;
		m_ucCorrelation = (unsigned char)128;
		m_fTGC = gcnew array<float>(6);
	}
	SignalParam::~SignalParam()
	{

	}

	ImageData::ImageData(void)
	{
		m_data = gcnew array<unsigned char>(IMAGE_DATA_NUM);
		m_fFrameRate = 0;
		m_nFrameWidthPixels = 0;
		m_nFrameHeightPixels = 0;
		m_nImageWidthPixels = 0;
		m_nImageHeightPixels = 0;
		m_fAspectRatio = 0;
		m_nHour = 0;
		m_nMinute = 0;
		m_nSecond = 0;
		m_nMillisecond = 0;
		m_nDepth = 0;
		m_nFrameNumber = 0;
	}

	ImageData::~ImageData(void)
	{
	}

	EnvelopeInfoCLR::EnvelopeInfoCLR()
	{
		bHadData = false;
		m_nEnvelope = 0;

	}


	DopplerDataCLR::DopplerDataCLR()
	{
		m_uSpectralLine = gcnew array<unsigned char>(1024);
		m_envelopeInfo = gcnew EnvelopeInfoCLR();
		nDateNum = 0;


	}
	DopplerDataCLR::~DopplerDataCLR()
	{

	}

	DopplerParamCLR::DopplerParamCLR()
	{
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
	}
	DopplerParamCLR::~DopplerParamCLR()
	{

	}

	PW_Info::PW_Info()
	{
		m_nFrequency = 0;
		m_fPRF = nullptr;
	}

	PW_Info::~PW_Info()
	{
		if (m_fPRF != nullptr)
		{
			delete m_fPRF;
			m_fPRF = nullptr;
		}
	}

	typedef void(__stdcall* EventCallback)(int);
	typedef void(__stdcall* EventCallback_HW)(int, int);
	NativeInterface::NativeInterface()
	{
		nativeCallback = nullptr;
		nativeCallback_HWMH = nullptr;
		frameDataUpdated = nullptr;
		hardWareMsgHandler = nullptr;
	}

	NativeInterface::~NativeInterface()
	{
		if (nativeCallback != nullptr)
		{
			// 释放委托句柄  
			if (this->delegateHandle.IsAllocated)
				this->delegateHandle.Free();
		}
		if (nativeCallback_HWMH != nullptr)
		{
			// 释放委托句柄  
			if (this->delegateHandle_HWMH.IsAllocated)
				this->delegateHandle_HWMH.Free();
		}
	}

	void NativeInterface::SetNative(IES::Native* native)
	{
		// 从成员函数创建一个委托  
		this->nativeCallback = gcnew EventDelegate(this, &NativeInterface::Callback);

		// 保证委托不会被内存移动和垃圾回收掉  
		this->delegateHandle = System::Runtime::InteropServices::GCHandle::Alloc(this->nativeCallback);

		// 转换为函数指针注册  
		System::IntPtr ptr = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(this->nativeCallback);
		native->RegisterCallback(static_cast<EventCallback>(ptr.ToPointer()));

		// 从成员函数创建一个委托  
		this->nativeCallback_HWMH = gcnew EventDelegate_HWMH(this, &NativeInterface::Callback_HWMH);

		// 保证委托不会被内存移动和垃圾回收掉  
		this->delegateHandle_HWMH = System::Runtime::InteropServices::GCHandle::Alloc(this->nativeCallback_HWMH);

		// 转换为函数指针注册  
		System::IntPtr ptr1 = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(this->nativeCallback_HWMH);
		native->RegisterCallback(static_cast<EventCallback_HW>(ptr1.ToPointer()));
	}


	void NativeInterface::SetDelegateMthod(FrameDataUpdated^ d)
	{
		frameDataUpdated = d;
	}
	void NativeInterface::SetDelegateMthod(HardWareMsgHandler^ d)
	{
		hardWareMsgHandler = d;
	}
	void NativeInterface::Callback(int i)
	{
		if (frameDataUpdated != nullptr)
		{
			frameDataUpdated(i);
		}

	}
	void NativeInterface::Callback_HWMH(int msgType, int value)
	{
		if (hardWareMsgHandler != nullptr)
		{
			hardWareMsgHandler(msgType, value);
		}
	}

	CLR_DemoServer::CLR_DemoServer(void)
		:m_pDemoServer(nullptr)
	{
			LoadKRProcessClass();
	}

	CLR_DemoServer::~CLR_DemoServer(void)
	{
		
			ReleaseResource();
	}

	void CLR_DemoServer::ReleaseResource()
	{
		if (m_pDemoServer == nullptr)
		{
			return;
		}

		m_pDemoServer->StopImageEngine();
		delete m_pDemoServer;
		m_pDemoServer = nullptr;

		
	}

	void CLR_DemoServer::LoadKRProcessClass()
	{
		m_pDataSource = new IES::DataSource();
		m_pDemoServer = new IES::ImageEngineServer(*m_pDataSource);

		m_pNative = new IES::Native();
		_nativeInterface = gcnew NativeInterface();
		_nativeInterface->SetNative(m_pNative);
		m_pDemoServer->SetFrameDataListener((IES::FrameDataListener*)m_pNative);

		pDopplerData = new IES::DopplerData();
		pDopplerDataArray = nullptr;
		m_nDopplerDataArrayLen = 0;
	}

	CLR_DemoServer ^ CLR_DemoServer::GetInstance()
	{
		if (m_Instance == nullptr)
		{
			m_Instance = gcnew CLR_DemoServer();
		}
		return m_Instance;
	}

	void CLR_DemoServer::ReleaseInstance()
	{
		if (m_Instance != nullptr)
		{
			delete m_Instance;
			m_Instance = nullptr;
		}
		return;
	}

	int CLR_DemoServer::SetCheckModeParamPath(System::String^ paramPath)
	{
		int nRet = 0;
		
			System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
			char *pFilePath = static_cast<char*>(ip.ToPointer());
			nRet = m_pDemoServer->SetCheckModeParamPath(pFilePath);

		return nRet;
	}

	int CLR_DemoServer::ResetFW()
	{
		int nRet = 0;

		nRet = m_pDataSource->ResetFW();

		return nRet;
	}

	int CLR_DemoServer::DownLoadFW(System::String^ fwFilePath)
	{
		int nRet = 0;
		
			System::IntPtr ip = Marshal::StringToHGlobalAnsi(fwFilePath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
			char *pFilePath = static_cast<char*>(ip.ToPointer());
			nRet = m_pDemoServer->DownLoadFW(pFilePath);

		return nRet;
	}

	int CLR_DemoServer::StartImageEngine()
	{
		int nRet = 0;
		
			nRet = m_pDemoServer->StartImageEngine();

		return nRet;
	}

	int CLR_DemoServer::StopImageEngine()
	{
		int nRet = 0;
		
			nRet = m_pDemoServer->StopImageEngine();

		return nRet;
	}

	void CLR_DemoServer::Freeze()
	{
		
			m_pDemoServer->Freeze();

		return;
	}

	void CLR_DemoServer::UnFreeze()
	{
		
			m_pDemoServer->UnFreeze();

		return;
	}

	bool CLR_DemoServer::IsRuning()
	{
		bool bRet = 0;
		
			bRet = m_pDemoServer->IsRuning();

		return bRet;
	}

	int CLR_DemoServer::GetProberInfo(Probe_Info^ probeInfo, System::String^ paramPath)
	{
		int nRet = 0;
		
		if (m_Instance == nullptr)
		{
			return -1;
		}

		IES::ProbeInfo probeinfo;
		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pFilePath = static_cast<char*>(ip.ToPointer());

		nRet = m_pDemoServer->GetProberInfo(probeinfo, pFilePath);
		if (nRet == 0)
		{
			probeInfo->m_fPith = probeinfo.m_fPith;
			probeInfo->m_fRadiusCurvature = probeinfo.m_fRadiusCurvature;
			probeInfo->m_nFocusNum = probeinfo.m_nVibrationNum;
			probeInfo->m_nType = probeinfo.m_nType;
			probeInfo->m_nPA = probeinfo.m_nPA;
			probeInfo->m_nShowDepthLevel = probeinfo.m_nShowDepthLevelNum;
			probeInfo->m_pDepthList = gcnew array<int>(probeInfo->m_nShowDepthLevel);

			Marshal::Copy((IntPtr)probeinfo.m_pDepthList, probeInfo->m_pDepthList, 0, probeInfo->m_nShowDepthLevel);
		}
		return nRet;
	}

	int CLR_DemoServer::SetSignalParam(SignalParam ^ signalParam)
	{
		int nRet = 0;
		
		IES::SignalProcParam signalProcParam;

		signalProcParam.m_signalProcParamType = (IES::SignalProcParam::SignalProcParamType)signalParam->m_SignalParamType;
		signalProcParam.m_nDepth = signalParam->m_nDepth;
		signalProcParam.m_nWidth = signalParam->m_nWidth;
		signalProcParam.m_nHeight = signalParam->m_nHigh;
		signalProcParam.m_fGain = signalParam->m_fGain;
		signalProcParam.m_fDR = signalParam->m_fDR;
		signalProcParam.m_ucCorrelation = signalParam->m_ucCorrelation;
		signalProcParam.m_ucImageEnhancementLevel = signalParam->m_ucImageEnhancementLevel;

		for (int i = 0; i < 6; i++)
		{
			signalProcParam.m_fTGC[i] = signalParam->m_fTGC[i];
		}


		return m_pDemoServer->SetSignalProcParam(signalProcParam);

	}
	int CLR_DemoServer::GetImageWidthPixels()
	{
		return m_pDemoServer->GetImageWidthPixels();
	}

	int CLR_DemoServer::GetImageHeightPixels()
	{
		return m_pDemoServer->GetImageHeightPixels();
	}

	int CLR_DemoServer::GetImageDisplayData(ImageData^ imageData)
	{
		int nDataCount = 0;
		
		if (m_Instance == nullptr)
		{
			return -1;
		}
		IES::ImageDisplayData imageDisplayData;
		nDataCount = m_pDemoServer->GetImageDisplayData(imageDisplayData);

		if (nDataCount == 1)
		{
			Marshal::Copy((IntPtr)imageDisplayData.m_pImageDisplayData, imageData->m_data, 0, IMAGE_DATA_NUM);
			imageData->m_fFrameRate = imageDisplayData.m_fFrameRate;
			imageData->m_nFrameWidthPixels = imageDisplayData.m_nFrameWidthPixels;
			imageData->m_nFrameHeightPixels = imageDisplayData.m_nFrameHeightPixels;
			imageData->m_nImageWidthPixels = imageDisplayData.m_nImageWidthPixels;
			imageData->m_nImageHeightPixels = imageDisplayData.m_nImageHeightPixels;
			imageData->m_fAspectRatio = imageDisplayData.m_fAspectRatio;
			imageData->m_nHour = imageDisplayData.m_nHour;
			imageData->m_nMinute = imageDisplayData.m_nMinute;
			imageData->m_nSecond = imageDisplayData.m_nSecond;
			imageData->m_nMillisecond = imageDisplayData.m_nMillisecond;
			imageData->m_nDepth = imageDisplayData.m_nDepth;
			imageData->m_nFrameNumber = imageDisplayData.m_nFrameNumber;
		}
		return nDataCount;
	}
	static int count = 0;

	int CLR_DemoServer::GetHistoryImageCount()
	{
		int nHistoryImageCount = 0;
		
			nHistoryImageCount = m_pDemoServer->GetImageDisplayCacheDataCount();

		return nHistoryImageCount;
	}

	int CLR_DemoServer::GetHistoryImageData(ImageData^ imageData, int nIndex)
	{
		int nDataCount = 0;
		
		if (m_Instance == nullptr)
		{
			return -1;
		}
		IES::ImageDisplayData imageDisplayData;
		nDataCount = m_pDemoServer->GetImageDisplayCacheData(imageDisplayData, nIndex);

		if (nDataCount == 1)
		{
			Marshal::Copy((IntPtr)imageDisplayData.m_pImageDisplayData, imageData->m_data, 0, IMAGE_DATA_NUM);
		}
		return nDataCount;
	}

	float CLR_DemoServer::GetCurrentFrameRate()
	{
		float nRet = 0;
		
			nRet = m_pDemoServer->GetCurrentFrameRate();

		return nRet;
	}
	bool CLR_DemoServer::IsUSBOpen()
	{
		bool nRet = false;
		
			nRet = m_pDemoServer->IsUSBOpen();

		return nRet;
	}

	int CLR_DemoServer::GetHardWareVersion(array<int >^	HWversion)
	{
		if (HWversion == nullptr && HWversion->Length < 2)
		{
			return -10;
		}

		int nErrorCode = 0;
		int*version = new int[2];

		nErrorCode = m_pDemoServer->GetHardWareVersion(version, 2);
		if (nErrorCode < 0)
		{
			version[0] = 0xFFFF;
			version[1] = 0xFFFF;
		}
		else
		{

			HWversion[0] = version[0];
			HWversion[1] = version[1];
		}

		return nErrorCode;
	}

	int CLR_DemoServer::GetSDKVersion(array<int >^	SDKversion)
	{
		if (SDKversion == nullptr && SDKversion->Length < 3)
		{
			return -10;
		}

		IES::Version ver = m_pDemoServer->GetVersion();
		SDKversion[0] = ver.Major;
		SDKversion[1] = ver.Minor;
		SDKversion[2] = ver.Revision;
		return 0;
	}

	void CLR_DemoServer::SetDelegateMthod(FrameDataUpdated^ d)
	{
		_nativeInterface->SetDelegateMthod(d);
	}

	void CLR_DemoServer::SetDelegateMthod(HardWareMsgHandler^ d)
	{
		_nativeInterface->SetDelegateMthod(d);
	}
	
	int CLR_DemoServer::GetPWInfo(PW_Info^ pwInfo, System::String^ paramPath)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		IES::PWShowInfo pwShowInfo;
		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pFilePath = static_cast<char*>(ip.ToPointer());

		int nRet = m_pDemoServer->GetPWShowInfo(pwShowInfo, pFilePath);
		if (nRet == 0)
		{
			pwInfo->m_nFrequency = pwShowInfo.m_nFrequency;

			pwInfo->m_fPRF = gcnew array<float>(8);

			Marshal::Copy((IntPtr)pwShowInfo.m_fPRF, pwInfo->m_fPRF, 0, 8);
		}
		return nRet;
	}


	int CLR_DemoServer::StartDoppler()
	{
		int nRet = m_pDemoServer->StartDoppler();
		
		return nRet;
	}

	int CLR_DemoServer::StopDoppler()
	{
		int nRet = m_pDemoServer->StopDoppler();
		
		return nRet;
	}

	int CLR_DemoServer::SetD_PWSamplingGateParam_Line(float SX, float SY, float depth, float fIWidthP, float fIHeightP, int launchDeflectionAngle, int samplingVolume)
	{
		int nRet = m_pDemoServer->SetD_PWSamplingGateParam_Line(SX, SY, depth, fIWidthP, fIHeightP, launchDeflectionAngle, samplingVolume);

		return nRet;
	}

	int CLR_DemoServer::SetD_PWSamplingGateParam_Convex(float SX, float SY, float depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume)
	{
		int nRet = m_pDemoServer->SetD_PWSamplingGateParam_Convex(SX, SY, depth, fIWidthP, fIHeightP, SectorCenterAngle, samplingVolume);

		return nRet;
	}

	int CLR_DemoServer::GetDopplerDisplayData(array<DopplerDataCLR^>^ dopplerDataClrArray, int dataNumber)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		if (dopplerDataClrArray == nullptr)
		{
			return -2;
		}
		int dataCounter = 0;
		for (int i = 0; i < dataNumber; i++)
		{

			int nDataCount = m_pDemoServer->GetDopplerDisplayData(*pDopplerData);

			if (nDataCount == 1)
			{
				dataCounter++;
				DopplerDataCLR^ dopplerDataClr = dopplerDataClrArray[i];

				Marshal::Copy((IntPtr)pDopplerData->uSpectralLine, dopplerDataClr->m_uSpectralLine, 0, 1024);

				dopplerDataClr->nDateNum = pDopplerData->nDateNum;

				dopplerDataClr->m_envelopeInfo->bHadData = pDopplerData->envelopeInfo.bHadData;
				if (pDopplerData->envelopeInfo.bHadData)
				{
					dopplerDataClr->m_envelopeInfo->m_nEnvelope = pDopplerData->envelopeInfo.nEnvelope;
				}

			}
			else
			{
				break;
			}
		}
		return dataCounter;
	}

	int CLR_DemoServer::GetHistoryDopplerData(array<DopplerDataCLR^>^ dopplerDataClrArray, int index, int dataNumber)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		if (dopplerDataClrArray == nullptr)
		{
			return -2;
		}

		if (m_nDopplerDataArrayLen != dataNumber)
		{
			if (pDopplerDataArray != nullptr)
			{
				delete[]pDopplerDataArray;
				pDopplerDataArray = nullptr;
			}

			pDopplerDataArray = new IES::DopplerData[dataNumber];
			m_nDopplerDataArrayLen = dataNumber;
		}

		int nDataCount = m_pDemoServer->GetDopplerDisplayCacheData(pDopplerDataArray, index, dataNumber);
		int showDLen = dopplerDataClrArray->Length;
		for (int i = 0; i < nDataCount &&i < showDLen; i++)
		{
			DopplerDataCLR^ dopplerDataClr = dopplerDataClrArray[i];

			Marshal::Copy((IntPtr)pDopplerDataArray[i].uSpectralLine, dopplerDataClr->m_uSpectralLine, 0, 1024);


			dopplerDataClr->nDateNum = pDopplerDataArray[i].nDateNum;

			dopplerDataClr->m_envelopeInfo->bHadData = pDopplerDataArray->envelopeInfo.bHadData;
			if (pDopplerDataArray[i].envelopeInfo.bHadData)
			{
				dopplerDataClr->m_envelopeInfo->m_nEnvelope = pDopplerDataArray[i].envelopeInfo.nEnvelope;
			}


		}
		return nDataCount;
	}

	int CLR_DemoServer::GetHistoryDopplerDataCount()
	{
		int nDataCount = m_pDemoServer->GetDopplerDisplayCacheDataCount();
		
		return nDataCount;
	}


	int CLR_DemoServer::SetDopplerParam(DopplerParamCLR^ dopplerParamClr)
	{
		IES::DopplerParam dopplerParam;

		dopplerParam.m_dopplerParamType = dopplerParamClr->m_dopplerParamType;
		dopplerParam.m_nWidth = dopplerParamClr->m_nWidth;
		dopplerParam.m_nHeight = dopplerParamClr->m_nHeight;
		dopplerParam.m_prf_rate = dopplerParamClr->m_prf_rate;
		dopplerParam.m_fGain = dopplerParamClr->m_fGain;
		dopplerParam.m_fDR = dopplerParamClr->m_fDR;
		dopplerParam.m_fFrequency = dopplerParamClr->m_fFrequency;
		dopplerParam.m_fTime = dopplerParamClr->m_fTime;
		dopplerParam.m_nBaseLineLevel = dopplerParamClr->m_nBaseLineLevel;
		dopplerParam.m_nWall_level = dopplerParamClr->m_nWall_level;
		dopplerParam.m_nSamplingVolume = dopplerParamClr->m_nSamplingVolume;

		return m_pDemoServer->SetDopplerParam(dopplerParam);


	}

}