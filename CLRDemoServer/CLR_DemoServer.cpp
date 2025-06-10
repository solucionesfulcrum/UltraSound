// 这是主 DLL 文件。

#include "stdafx.h"

#include "CLR_DemoServer.h"
#include "Native.h"
#include <iostream>

using namespace System;
using namespace System::Runtime::InteropServices;	// needed for Marshal

namespace CLRDemoServer {
	CLR_DemoServer::CLR_DemoServer(void)
		:m_pImageEngineServer(NULL)
		, m_bStartSaveVideo(false)
	{
		LoadKRProcessClass();
	}

	CLR_DemoServer::~CLR_DemoServer(void)
	{
		ReleaseResource();
	}

	void CLR_DemoServer::ReleaseResource()
	{
		if (m_pImageEngineServer == NULL)
		{
			return;
		}

		m_pImageEngineServer->StopImageEngine();
		delete m_pImageEngineServer;
		m_pImageEngineServer = NULL;

	}

	void CLR_DemoServer::LoadKRProcessClass()
	{
		m_pDataSource = new CIES::DataSource();
		m_pImageEngineServer = new CIES::ColorImageEngineServer(*m_pDataSource);
		m_pNative = new CIES::Native();
		_nativeInterface = gcnew NativeInterface();
		_nativeInterface->SetNative(m_pNative);
		m_pImageEngineServer->SetFrameDataListener((CIES::FrameDataListener*)m_pNative);
		pD_PWArray = nullptr;
		m_nD_PWArrayLen = 0;
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

	int CLR_DemoServer::GetSDKVersion(array<int >^	SDKversion)
	{
		if (SDKversion == nullptr && SDKversion->Length < 3)
		{
			return -10;
		}

		CIES::Version ver = m_pImageEngineServer->GetVersion();
		SDKversion[0] = ver.Major;
		SDKversion[1] = ver.Minor;
		SDKversion[2] = ver.Revision;
		return 0;

	}

	int CLR_DemoServer::GetUSDeviceInfos(array<CLRUSDeviceInfo^>^ usHardWareInfoArray, int% getDataNum)
	{
		if (usHardWareInfoArray == nullptr)
		{
			return -3;
		}
		int nRet = 0;

		nRet = _getUSDeviceInfos(usHardWareInfoArray, getDataNum);

		return nRet;
	}

	void CLR_DemoServer::SetUSDevicePath(System::String^ DevicePath)
	{

		System::IntPtr ip = Marshal::StringToHGlobalAnsi(DevicePath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pDevicePath = static_cast<char*>(ip.ToPointer());
		m_pImageEngineServer->SetUSDevicePath(pDevicePath);

	}

	int CLR_DemoServer::SetParamPath(System::String^ paramPath, CLScanMode::CLScanModeEnum scanMode)
	{
		int nRet = 0;

		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pFilePath = static_cast<char*>(ip.ToPointer());
		CIES::ScanMode::ScanModeEnum smn = (CIES::ScanMode::ScanModeEnum)scanMode;
		nRet = m_pImageEngineServer->SetCheckModeParamPath_And_ScanMode(pFilePath, smn);

		return nRet;
	}

	int CLR_DemoServer::StartImageEngine()
	{
		return m_pImageEngineServer->StartImageEngine();
	}

	int CLR_DemoServer::StopImageEngine()
	{
		return m_pImageEngineServer->StopImageEngine();
	}

	void CLR_DemoServer::Freeze()
	{
		return m_pImageEngineServer->Freeze();
	}

	void CLR_DemoServer::UnFreeze()
	{
		return m_pImageEngineServer->UnFreeze();
	}


	bool CLR_DemoServer::IsRuning()
	{
		return m_pImageEngineServer->IsRuning();
	}

	int CLR_DemoServer::GetHardWareInfo(CLRHardWareInfo^ hardWareInfo)
	{
		if (hardWareInfo == nullptr)
		{
			return -1;
		}
		int nRet = 0;

		nRet = _getHardWareInfo(hardWareInfo);


		return nRet;
	}

	int CLR_DemoServer::GetProberInfo(Probe_Info^ probeInfo, System::String^ paramPath)
	{
		return _getProbeInfo(probeInfo, paramPath);
	}
	int CLR_DemoServer::GetD_PWInfo(D_PWInfo^ pwInfo, System::String^ paramPath)
	{
		return _getD_PWInfo(pwInfo, paramPath);
	}
	int CLR_DemoServer::SetImageParam_B(B_ImageParam_CLR ^ b_ImageParam_CLR)
	{
		return _setImageParam_B(b_ImageParam_CLR);
	}

	int CLR_DemoServer::SetImageParam_C(C_ImageParam_CLR^ c_ImageParam_CLR)
	{
		return _setImageParam_C(c_ImageParam_CLR);
	}
	int CLR_DemoServer::SetImageParam_D_PW(D_PW_ImageParam_CLR^ d_PW_ImageParam_CLR)
	{
		return _setImageParam_D_PW(d_PW_ImageParam_CLR);
	}
	int CLR_DemoServer::SetImageParam_M(M_ImageParam_CLR^ m_ImageParam_CLR)
	{
		return _setImageParam_M(m_ImageParam_CLR);
	}

	int CLR_DemoServer::SetFocusArea(float fFocusArea)
	{
		return m_pImageEngineServer->SetFocusArea(fFocusArea);
	}

	int CLR_DemoServer::GetCurrentFocusArea(float% startDepth, float% endDepth)
	{
		float depth0 = 0;
		float depth1 = 0;
		int nRet = m_pImageEngineServer->GetCurrentFocusArea(depth0, depth1);
		startDepth = depth0;
		endDepth = depth1;

		return nRet;
	}

	int CLR_DemoServer::GetCurrentFocusArray(array<float>^ B_FocusArray)
	{
		if (B_FocusArray == nullptr || B_FocusArray->Length < 8)
			return -2;
		float FocusArray[8];
		int FocusArrayLen = 8;
		int nRet = 0;

		nRet = m_pImageEngineServer->GetCurrentFocusArray(FocusArray, FocusArrayLen);

		Marshal::Copy((IntPtr)FocusArray, B_FocusArray, 0, FocusArrayLen);


		return nRet;
	}

	int CLR_DemoServer::GetCurrentFocusArrayByParamPath(System::String^ paramPath, int m_nDepth_level, int m_nHarmonic, array<float>^ B_FocusArray)
	{
		return _getCurrentFocusArrayByParamPath(paramPath, m_nDepth_level, m_nHarmonic, B_FocusArray);
	}
	int CLR_DemoServer::GetColorMap(array<int>^ B_MapArray, array<int>^ C_MapArray)
	{
		if (B_MapArray == nullptr || B_MapArray->Length < 256 || C_MapArray == nullptr || C_MapArray->Length < 256 * 16)
		{
			return -2;
		}
		int nRet = 0;
		int B_Map[256] = { 0 };
		int C_Map[256 * 16] = { 0 };
		int BMapLen = 256;
		int CMapLen = 256 * 16;
		nRet = m_pImageEngineServer->GetColorMap((unsigned int*)B_Map, BMapLen, (unsigned int*)C_Map, CMapLen);

		Marshal::Copy((IntPtr)B_Map, B_MapArray, 0, BMapLen);
		Marshal::Copy((IntPtr)C_Map, C_MapArray, 0, CMapLen);


		return nRet;
	}

	int CLR_DemoServer::GetColorMap_B(array<int>^ B_MapArray)
	{
		if (B_MapArray == nullptr || B_MapArray->Length < 256)
		{
			return -2;
		}
		int nRet = 0;

		int B_Map[256] = { 0 };
		int BMapLen = 256;
		nRet = m_pImageEngineServer->GetColorMap_B((unsigned int*)B_Map, BMapLen);

		Marshal::Copy((IntPtr)B_Map, B_MapArray, 0, BMapLen);

		return nRet;
	}

	int CLR_DemoServer::GetColorMap_C(array<int>^ C_MapArray)
	{
		if (C_MapArray == nullptr || C_MapArray->Length < 256 * 16)
		{
			return -2;
		}
		int nRet = 0;
		int C_Map[256 * 16] = { 0 };
		int CMapLen = 256 * 16;
		nRet = m_pImageEngineServer->GetColorMap_C((unsigned int*)C_Map, CMapLen);
		Marshal::Copy((IntPtr)C_Map, C_MapArray, 0, CMapLen);

		return nRet;
	}
	int CLR_DemoServer::GetColorMap_D(array<int>^ D_MapArray)
	{
		if (D_MapArray == nullptr || D_MapArray->Length < 256)
		{
			return -2;
		}
		int nRet = 0;

		int D_Map[256] = { 0 };
		int DMapLen = 256;
		nRet = m_pImageEngineServer->GetColorMap_D((unsigned int*)D_Map, DMapLen);
		Marshal::Copy((IntPtr)D_Map, D_MapArray, 0, DMapLen);


		return nRet;
	}
	int CLR_DemoServer::GetColorMap_M(array<int>^ M_MapArray)
	{
		if (M_MapArray == nullptr || M_MapArray->Length < 256)
		{
			return -2;
		}
		int nRet = 0;

		int M_Map[256] = { 0 };
		int MMapLen = 256;
		nRet = m_pImageEngineServer->GetColorMap_M((unsigned int*)M_Map, MMapLen);
		Marshal::Copy((IntPtr)M_Map, M_MapArray, 0, MMapLen);
		return nRet;
	}
	int CLR_DemoServer::GetImageDisplayData(ImageData^ imageData)
	{
		return _getImageDisplayData(imageData);
	}

	static int count = 0;

	int CLR_DemoServer::GetHistoryImageCount()
	{
		return m_pImageEngineServer->GetImageDisplayCacheDataCount();
	}

	int CLR_DemoServer::GetHistoryImageData(ImageData^ imageData, int nIndex)
	{
		return _getHistoryImageData(imageData, nIndex);
	}
	int CLR_DemoServer::GetImageDisplayData_D_PW(ImageData^ imageData, int dataNumber, bool% resetDisplayData)
	{
		return _getImageDisplayData_D_PW(imageData, dataNumber, resetDisplayData);

	}
	int CLR_DemoServer::GetHistoryImageData_D_PW(ImageData_D_PW^ imageData, int startIndex, int dataNumber)
	{
		return _getHistoryImageData_D_PW(imageData, startIndex, dataNumber);
	}

	int CLR_DemoServer::GetHistoryImageData_BM(int OneScreenWidth, int index, ImageData^ imageData, int% StartPixel, int% MIndexPixel, double MIndexAppend, bool getOneScreen, double MPosOnScreen)
	{
		return _getHistoryImageData_BM(OneScreenWidth, index, imageData, StartPixel, MIndexPixel, MIndexAppend, getOneScreen, MPosOnScreen);
	}
	float CLR_DemoServer::GetCurrentFrameRate()
	{
		return m_pImageEngineServer->GetCurrentFrameRate();
	}

	void CLR_DemoServer::SetUseImageHistoryCache(bool isUse, int cacheLen)
	{
		m_pImageEngineServer->SetUseImageHistoryCache(isUse, cacheLen);

	}

	void CLR_DemoServer::SetDelegateMthod(HardWareMsgHandler^ d)
	{
		_nativeInterface->SetDelegateMthod(d);
	}
	void CLR_DemoServer::SetDelegateMthod(HardWareMsgSHandler^ d)
	{
		_nativeInterface->SetDelegateMthod(d);
	}
	void CLR_DemoServer::ClearDelegateMthod()
	{
		_nativeInterface->ClearDelegateMthod();
	}

	bool CLR_DemoServer::IsUSBOpen()
	{
		return m_pImageEngineServer->IsUSBOpen();
	}

	int CLR_DemoServer::GetImageWidthPixels()
	{
		return m_pImageEngineServer->GetImageWidthPixels();
	}

	int CLR_DemoServer::GetImageHeightPixels()
	{
		return m_pImageEngineServer->GetImageHeightPixels();
	}

	int CLR_DemoServer::PowerOn(System::String^ DevicePath)
	{
		int nRet = 0;

		System::IntPtr ip = Marshal::StringToHGlobalAnsi(DevicePath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pDevicePath = static_cast<char*>(ip.ToPointer());
		nRet = m_pDataSource->PowerOn(pDevicePath);

		return nRet;
	}

	int CLR_DemoServer::PowerOff(System::String^ DevicePath)
	{
		int nRet = 0;

		System::IntPtr ip = Marshal::StringToHGlobalAnsi(DevicePath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char *pDevicePath = static_cast<char*>(ip.ToPointer());
		nRet = m_pDataSource->PowerOff(pDevicePath);

		return nRet;
	}

	int CLR_DemoServer::SetCSamplingFrameParam_Quadrangle(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SX, double SY, double SWidthLen, double SHeightLen, int LDAngle)
	{
		return m_pImageEngineServer->SetCSamplingFrameParam_Quadrangle(Depth, nFWidthP, nFHeightP, nIWidthP, nIHeightP, SX, SY, SWidthLen, SHeightLen, LDAngle);
		
	}

	int CLR_DemoServer::Get_C_PRFList_Quadrangle(int Depth, double nIHeightP, double SY, double SHeightLen, int LDAngle, array<double>^ fPrfList, double% fTxFrequency)
	{
		if (fPrfList->Length < 8)
		{
			return -1;
		}
		int nRet = 0;
		double fPrfListA[8];
		int fPrfListALen = 8;
		double fTxFrequencyT = 0;

		nRet = m_pImageEngineServer->Get_C_PRFList_Quadrangle(Depth, nIHeightP, SY, SHeightLen, LDAngle, fPrfListA, fPrfListALen, fTxFrequencyT);

		if (nRet == 0)
		{
			Marshal::Copy((IntPtr)fPrfListA, fPrfList, 0, fPrfListALen);
		}
		fTxFrequency = fTxFrequencyT;


		return nRet;
	}

	int CLR_DemoServer::SetCSamplingFrameParam_Sector(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SCToPCCP, double SHeight, double SectorAngle, double SectorCenterAngle)
	{
		return m_pImageEngineServer->SetCSamplingFrameParam_Sector(Depth, nFWidthP, nFHeightP, nIWidthP, nIHeightP, SCToPCCP, SHeight, SectorAngle, SectorCenterAngle);
	}

	int CLR_DemoServer::Get_C_PRFList_Sector(int Depth, double nIHeightP, double SCToPCCP, double SHeight, array<double>^ fPrfList, double% fTxFrequency)
	{
		if (fPrfList->Length < 8)
		{
			return -1;
		}
		int nRet = 0;
		double fPrfListA[8];
		int fPrfListALen = 8;
		double fTxFrequencyT = 0;

		nRet = m_pImageEngineServer->Get_C_PRFList_Sector(Depth, nIHeightP, SCToPCCP, SHeight, fPrfListA, fPrfListALen, fTxFrequencyT);

		if (nRet == 0)
		{
			Marshal::Copy((IntPtr)fPrfListA, fPrfList, 0, fPrfListALen);
		}
		fTxFrequency = fTxFrequencyT;


		return nRet;
	}
	int CLR_DemoServer::SetD_PWSamplingGateParam_Line(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, int launchDeflectionAngle, int samplingVolume)
	{
		return m_pImageEngineServer->SetD_PWSamplingGateParam_Line(SX, SY, Depth, fIWidthP, fIHeightP, launchDeflectionAngle, samplingVolume);
	}

	int CLR_DemoServer::Get_D_PRFList_Line(float SY, float Depth, float fIHeightP, int launchDeflectionAngle, array<double>^ fPrfList, double% fTxFrequency)
	{
		if (fPrfList->Length < 8)
		{
			return -1;
		}
		int nRet = 0;
		double fPrfListA[8];
		int fPrfListALen = 8;
		double fTxFrequencyT = 0;

			nRet = m_pImageEngineServer->Get_D_PRFList_Line(SY, Depth, fIHeightP, launchDeflectionAngle, fPrfListA, fPrfListALen, fTxFrequencyT);

			if (nRet == 0)
			{
				Marshal::Copy((IntPtr)fPrfListA, fPrfList, 0, fPrfListALen);
				fTxFrequency = fTxFrequencyT;
			}


		return nRet;
	}

	int CLR_DemoServer::SetD_PWSamplingGateParam_Convex(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume)
	{
		return m_pImageEngineServer->SetD_PWSamplingGateParam_Convex(SX, SY, Depth, fIWidthP, fIHeightP, SectorCenterAngle, samplingVolume);
	}

	int CLR_DemoServer::Get_D_PRFList_Convex(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, array<double>^ fPrfList, double% fTxFrequency)
	{
		if (fPrfList->Length < 8)
		{
			return -1;
		}
		int nRet = 0;
		double fPrfListA[8];
		int fPrfListALen = 8;
		double fTxFrequencyT = 0;
			nRet = m_pImageEngineServer->Get_D_PRFList_Convex(SX, SY, Depth, fIWidthP, fIHeightP, fPrfListA, fPrfListALen, fTxFrequencyT);

			if (nRet == 0)
			{
				Marshal::Copy((IntPtr)fPrfListA, fPrfList, 0, fPrfListALen);
				fTxFrequency = fTxFrequencyT;
			}
		return nRet;
	}

	int CLR_DemoServer::SetD_PWSamplingGateParam_PA(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume)
	{
		return m_pImageEngineServer->SetD_PWSamplingGateParam_PA(SX, SY, Depth, fIWidthP, fIHeightP, SectorCenterAngle, samplingVolume);
	}

	int CLR_DemoServer::Get_D_PRFList_PA(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, array<double>^ fPrfList, double% fTxFrequency)
	{
		if (fPrfList->Length < 8)
		{
			return -1;
		}
		int nRet = 0;
		double fPrfListA[8];
		int fPrfListALen = 8;
		double fTxFrequencyT = 0;

			nRet = m_pImageEngineServer->Get_D_PRFList_PA(SX, SY, Depth, fIWidthP, fIHeightP, fPrfListA, fPrfListALen, fTxFrequencyT);

			if (nRet == 0)
			{
				Marshal::Copy((IntPtr)fPrfListA, fPrfList, 0, fPrfListALen);
				fTxFrequency = fTxFrequencyT;
			}


		return nRet;
	}

	int CLR_DemoServer::SetSamplingLineParam_Line(float SLX, float depth, float imageWidth, float imageHight)
	{
			return m_pImageEngineServer->SetSamplingLineParam_Line(SLX, depth, imageWidth, imageHight);
	}

	int CLR_DemoServer::SetSamplingLineParam_Convex(float SectorCenterAngle, float depth, float imageWidth, float imageHight)
	{
			return m_pImageEngineServer->SetSamplingLineParam_Convex(SectorCenterAngle, depth, imageWidth, imageHight);
	}

	int CLR_DemoServer::SetSamplingLineParam_Phased(float SectorCenterAngle, float depth, float imageWidth, float imageHight)
	{
			return m_pImageEngineServer->SetSamplingLineParam_Phased(SectorCenterAngle, depth, imageWidth, imageHight);
	}

	int CLR_DemoServer::AutomaticDiameterMeasurement(int index, float PointOnBImageX, float PointOnBImageY, float% CenterOfVascularDiameterX, float% CenterOfVascularDiameterY, float% VascularRadius)
	{
		int nRet = 0;
		float CenterOfVascularDiameterXT = -1;
		float CenterOfVascularDiameterYT = -1;
		float VascularRadiusT = -1;


		nRet = m_pImageEngineServer->AutomaticDiameterMeasurement(index, PointOnBImageX, PointOnBImageY, CenterOfVascularDiameterXT, CenterOfVascularDiameterYT, VascularRadiusT);

		CenterOfVascularDiameterX = CenterOfVascularDiameterXT;
		CenterOfVascularDiameterY = CenterOfVascularDiameterYT;
		VascularRadius = VascularRadiusT;

		return nRet;
	}

	
	int CLR_DemoServer::AutomaticTGC(array<float>^ fTGCList)
	{
		if (fTGCList->Length < 6)
		{
			return -2;
		}
		float tgc[6];
		memset(tgc, 0, 6 * sizeof(float));
		int tgcLen = 6;
		int nRet = 0;

		nRet = m_pImageEngineServer->AutomaticTGC(tgc, tgcLen);

		if (nRet == 0)
		{
			Marshal::Copy((IntPtr)tgc, fTGCList, 0, tgcLen);
		}

		return nRet;
	}

	int CLR_DemoServer::AutomaticSV(float% svx, float% svy, float% svH, int% svAngleLevel)
	{
		float svxT = 0;
		float svyT = 0;
		float svHT = 0;
		int svAngleLevelT = 0;
		int nRet = 0;

		nRet = m_pImageEngineServer->AutomaticSV(svxT, svyT, svHT, svAngleLevelT);

		if (nRet == 0)
		{
			svx = svxT;
			svy = svyT;
			svH = svHT;
			svAngleLevel = svAngleLevelT;
		}

		return nRet;
	}

	int CLR_DemoServer::AutoPRF(int% PRF_level, int% Baseline_level)
	{

		int PRF_levelT = 0;
		int Baseline_levelT = 0;
		int nRet = 0;

		nRet = m_pImageEngineServer->AutoPRF(PRF_levelT, Baseline_levelT);

		if (nRet == 0)
		{
			PRF_level = PRF_levelT;
			Baseline_level = Baseline_levelT;
		}

		return nRet;

	}


	int CLR_DemoServer::_setImageParam_B(B_ImageParam_CLR^ b_ImageParam_CLR)
	{
		CIES::B_ImageParam b_ImageParam;

		b_ImageParam.m_B_ImageParamType = b_ImageParam_CLR->m_B_ImageParamType;

		b_ImageParam.m_nWidth = b_ImageParam_CLR->m_nWidth;
		b_ImageParam.m_nHeight = b_ImageParam_CLR->m_nHeight;
		b_ImageParam.m_nDepth_level = b_ImageParam_CLR->m_nDepth_level;
		b_ImageParam.m_fGain = b_ImageParam_CLR->m_fGain;
		b_ImageParam.m_fDR = b_ImageParam_CLR->m_fDR;
		b_ImageParam.m_SRI_Level = b_ImageParam_CLR->m_SRI_Level;
		b_ImageParam.m_nCorrelation_Level = b_ImageParam_CLR->m_nCorrelation_Level;
		b_ImageParam.m_nGrayColorMap_level = b_ImageParam_CLR->m_nGrayColorMap_level;
		b_ImageParam.m_nPseudoColorMap_level = b_ImageParam_CLR->m_nPseudoColorMap_level;
		b_ImageParam.m_nHarmonic = b_ImageParam_CLR->m_nHarmonic;
		b_ImageParam.m_nFrequency = b_ImageParam_CLR->m_nFrequency;
		b_ImageParam.m_nFocusArea = b_ImageParam_CLR->m_nFocusArea;
		b_ImageParam.m_nNE = b_ImageParam_CLR->m_nNE;
		b_ImageParam.m_nNE_Theta = b_ImageParam_CLR->m_nNE_Theta;
		for (int i = 0; i < 6; i++)
		{
			b_ImageParam.m_fTGC[i] = b_ImageParam_CLR->m_fTGC[i];
		}


		return m_pImageEngineServer->SetImageParam_B(b_ImageParam);
	}

	int CLR_DemoServer::_setImageParam_C(C_ImageParam_CLR^ c_ImageParam_CLR)
	{
		CIES::C_ImageParam c_ImageParam;

		c_ImageParam.m_C_ImageParamType = c_ImageParam_CLR->m_C_ImageParamType;

		c_ImageParam.m_fGain = c_ImageParam_CLR->m_fGain;
		c_ImageParam.m_nWallFilter_level = c_ImageParam_CLR->m_nWallFilter_level;
		c_ImageParam.m_nColor_mode = c_ImageParam_CLR->m_nColorMap_mode;
		c_ImageParam.m_nColorMap_level = c_ImageParam_CLR->m_nColorMap_level;
		c_ImageParam.m_nColorMap_inversion = c_ImageParam_CLR->m_nColorMap_inversion;
		c_ImageParam.m_nPRF_Level = c_ImageParam_CLR->m_nPRF_Level;
		c_ImageParam.m_fTheta = c_ImageParam_CLR->m_fTheta;
		c_ImageParam.m_nFrameCorrelation_level = c_ImageParam_CLR->m_nFrameCorrelation_level;
		c_ImageParam.m_nColorPriority_level = c_ImageParam_CLR->m_nColorPriority_level;
		c_ImageParam.m_bUse_B_BC_Mode = c_ImageParam_CLR->m_bUse_B_BC_Mode;
		c_ImageParam.m_nSpeed = c_ImageParam_CLR->m_nSpeed;
		return m_pImageEngineServer->SetImageParam_C(c_ImageParam);
	}
	int CLR_DemoServer::_setImageParam_D_PW(D_PW_ImageParam_CLR^ d_PW_ImageParam_CLR)
	{
		CIES::D_PW_ImageParam d_PW_ImageParam;

		d_PW_ImageParam.m_D_PW_ImageParamType = d_PW_ImageParam_CLR->m_D_PW_ImageParamType;
		d_PW_ImageParam.m_nWidth = d_PW_ImageParam_CLR->m_nWidth;
		d_PW_ImageParam.m_nHeight = d_PW_ImageParam_CLR->m_nHeight;
		d_PW_ImageParam.m_prf_rate = d_PW_ImageParam_CLR->m_prf_rate;
		d_PW_ImageParam.m_fGain = d_PW_ImageParam_CLR->m_fGain;
		d_PW_ImageParam.m_fDR = d_PW_ImageParam_CLR->m_fDR;
		d_PW_ImageParam.m_fFrequency = d_PW_ImageParam_CLR->m_fFrequency;
		d_PW_ImageParam.m_fTime = d_PW_ImageParam_CLR->m_fTime;
		d_PW_ImageParam.m_nBaseLineLevel = d_PW_ImageParam_CLR->m_nBaseLineLevel;
		d_PW_ImageParam.m_nWall_level = d_PW_ImageParam_CLR->m_nWall_level;
		d_PW_ImageParam.m_nSamplingVolume = d_PW_ImageParam_CLR->m_nSamplingVolume;
		d_PW_ImageParam.m_nInversion = d_PW_ImageParam_CLR->m_nInversion;
		d_PW_ImageParam.m_nSpeed = d_PW_ImageParam_CLR->m_nSpeed;
		d_PW_ImageParam.m_nGrayColorMap_level = d_PW_ImageParam_CLR->m_nGrayColorMap_level;
		d_PW_ImageParam.m_nPseudoColorMap_level = d_PW_ImageParam_CLR->m_nPseudoColorMap_level;
		return  m_pImageEngineServer->SetImageParam_D_PW(d_PW_ImageParam);
	}

	int CLR_DemoServer::_setImageParam_M(M_ImageParam_CLR^ m_ImageParam_CLR)
	{
		CIES::M_ImageParam m_ImageParam;

		m_ImageParam.m_M_ImageParamType = m_ImageParam_CLR->m_M_ImageParamType;
		m_ImageParam.m_fGain = m_ImageParam_CLR->m_fGain;
		m_ImageParam.m_fTime = m_ImageParam_CLR->m_fTime;
		m_ImageParam.m_nWidth = m_ImageParam_CLR->m_nWidth;
		m_ImageParam.m_nHeight = m_ImageParam_CLR->m_nHeight;
		m_ImageParam.m_nGrayColorMap_level = m_ImageParam_CLR->m_nGrayColorMap_level;
		m_ImageParam.m_nPseudoColorMap_level = m_ImageParam_CLR->m_nPseudoColorMap_level;
		return  m_pImageEngineServer->SetImageParam_M(m_ImageParam);
	}

	int CLR_DemoServer::_getImageDisplayData(ImageData ^ imageData)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}
		CIES::UImagingData uimagingData;
		int nDataCount = m_pImageEngineServer->GetImageDisplayData(uimagingData);

		if (nDataCount == 1)
		{
			imageData->m_bBHadData = false;
			imageData->m_bCHadData = false;
			if (uimagingData.m_bBHadData)
			{
				imageData->m_bBHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_B_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{

					int bImageData = datalen / 4;
					imageData->m_nBImageDataLen = bImageData;
					if (imageData->m_B_Imagedata == nullptr)
						imageData->m_B_Imagedata = gcnew array<int>(2048 * 2048);

					Marshal::Copy((IntPtr)pD, imageData->m_B_Imagedata, 0, bImageData);

					CIES::ImageDataInfo* idi = uimagingData.m_B_Data.GetImageDataInfo();

					if (idi != nullptr)
					{
						imageData->m_B_ImageInfo->m_fFrameRate = idi->m_fFrameRate;
						imageData->m_B_ImageInfo->m_nFrameWidthPixels = idi->m_nFrameWidthPixels;
						imageData->m_B_ImageInfo->m_nFrameHeightPixels = idi->m_nFrameHeightPixels;
						imageData->m_B_ImageInfo->m_nImageWidthPixels = idi->m_nImageWidthPixels;
						imageData->m_B_ImageInfo->m_nImageHeightPixels = idi->m_nImageHeightPixels;
						imageData->m_B_ImageInfo->m_fResolution = idi->m_fResolution;
						imageData->m_B_ImageInfo->m_nHour = idi->m_nHour;
						imageData->m_B_ImageInfo->m_nMinute = idi->m_nMinute;
						imageData->m_B_ImageInfo->m_nSecond = idi->m_nSecond;
						imageData->m_B_ImageInfo->m_nMillisecond = idi->m_nMillisecond;
						imageData->m_B_ImageInfo->m_nDepth = idi->m_nDepth;
						imageData->m_B_ImageInfo->m_nFrameNumber = idi->m_nFrameNumber;
					}
				}

			}

			if (uimagingData.m_bCHadData)
			{
				imageData->m_bCHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_C_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{
					int cImageData = datalen / 4;
					imageData->m_nCImageDataLen = cImageData;
					if (imageData->m_C_Imagedata == nullptr)
						imageData->m_C_Imagedata = gcnew array<int>(2048 * 2048);

					Marshal::Copy((IntPtr)pD, imageData->m_C_Imagedata, 0, cImageData);

					CIES::ImageDataInfo* idi = uimagingData.m_C_Data.GetImageDataInfo();

					if (idi != nullptr)
					{
						imageData->m_C_ImageInfo->m_fFrameRate = idi->m_fFrameRate;
						imageData->m_C_ImageInfo->m_nFrameWidthPixels = idi->m_nFrameWidthPixels;
						imageData->m_C_ImageInfo->m_nFrameHeightPixels = idi->m_nFrameHeightPixels;
						imageData->m_C_ImageInfo->m_nImageWidthPixels = idi->m_nImageWidthPixels;
						imageData->m_C_ImageInfo->m_nImageHeightPixels = idi->m_nImageHeightPixels;
						imageData->m_C_ImageInfo->m_fResolution = idi->m_fResolution;
						imageData->m_C_ImageInfo->m_nHour = idi->m_nHour;
						imageData->m_C_ImageInfo->m_nMinute = idi->m_nMinute;
						imageData->m_C_ImageInfo->m_nSecond = idi->m_nSecond;
						imageData->m_C_ImageInfo->m_nMillisecond = idi->m_nMillisecond;
						imageData->m_C_ImageInfo->m_nDepth = idi->m_nDepth;
						imageData->m_C_ImageInfo->m_nFrameNumber = idi->m_nFrameNumber;
					}
				}

			}

			if (uimagingData.m_bMHadData)
			{
				imageData->m_bMHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_M_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{

					int mImageData = datalen / 4;
					imageData->m_nMImageDataLen = mImageData;
					if (imageData->m_M_Imagedata == nullptr)
						imageData->m_M_Imagedata = gcnew array<int>(2048 * 40);

					Marshal::Copy((IntPtr)pD, imageData->m_M_Imagedata, 0, mImageData);

					CIES::MDataInfo* di = uimagingData.m_M_Data.GetMDataInfo();

					if (di != nullptr)
					{
						imageData->m_M_ImageInfo->m_nFrameNumber = di->m_nFrameNumber;
						imageData->m_M_ImageInfo->m_nImageHeightPixels = di->m_nImageHeightPixels;
						imageData->m_M_ImageInfo->m_nMLineNum = di->m_nMLineNum;
						imageData->m_M_ImageInfo->m_bClearScreen = di->m_bClearScreen;
					}
				}

			}

		}
		return nDataCount;
	}


	int CLR_DemoServer::_getHistoryImageData(ImageData^ imageData, int nIndex)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}
		CIES::UImagingData uimagingData;
		int nDataCount = m_pImageEngineServer->GetImageDisplayCacheData(uimagingData, nIndex);

		if (nDataCount == 1)
		{
			imageData->m_bBHadData = false;
			imageData->m_bCHadData = false;
			
			if (uimagingData.m_bBHadData)
			{
				imageData->m_bBHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_B_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{

					int bImageData = datalen / 4;
					imageData->m_nBImageDataLen = bImageData;
					if (imageData->m_B_Imagedata == nullptr)
						imageData->m_B_Imagedata = gcnew array<int>(2048 * 2048);
					Marshal::Copy((IntPtr)pD, imageData->m_B_Imagedata, 0, bImageData);

					CIES::ImageDataInfo* idi = uimagingData.m_B_Data.GetImageDataInfo();

					if (idi != nullptr)
					{
						imageData->m_B_ImageInfo->m_fFrameRate = idi->m_fFrameRate;
						imageData->m_B_ImageInfo->m_nFrameWidthPixels = idi->m_nFrameWidthPixels;
						imageData->m_B_ImageInfo->m_nFrameHeightPixels = idi->m_nFrameHeightPixels;
						imageData->m_B_ImageInfo->m_nImageWidthPixels = idi->m_nImageWidthPixels;
						imageData->m_B_ImageInfo->m_nImageHeightPixels = idi->m_nImageHeightPixels;
						imageData->m_B_ImageInfo->m_fResolution = idi->m_fResolution;
						imageData->m_B_ImageInfo->m_nHour = idi->m_nHour;
						imageData->m_B_ImageInfo->m_nMinute = idi->m_nMinute;
						imageData->m_B_ImageInfo->m_nSecond = idi->m_nSecond;
						imageData->m_B_ImageInfo->m_nMillisecond = idi->m_nMillisecond;
						imageData->m_B_ImageInfo->m_nDepth = idi->m_nDepth;
						imageData->m_B_ImageInfo->m_nFrameNumber = idi->m_nFrameNumber;
					}
				}

			}

			if (uimagingData.m_bCHadData)
			{
				imageData->m_bCHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_C_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{
					int cImageData = datalen / 4;
					imageData->m_nCImageDataLen = cImageData;
					if (imageData->m_C_Imagedata == nullptr)
						imageData->m_C_Imagedata = gcnew array<int>(2048 * 2048);
					Marshal::Copy((IntPtr)pD, imageData->m_C_Imagedata, 0, cImageData);

					CIES::ImageDataInfo* idi = uimagingData.m_C_Data.GetImageDataInfo();

					if (idi != nullptr)
					{
						imageData->m_C_ImageInfo->m_fFrameRate = idi->m_fFrameRate;
						imageData->m_C_ImageInfo->m_nFrameWidthPixels = idi->m_nFrameWidthPixels;
						imageData->m_C_ImageInfo->m_nFrameHeightPixels = idi->m_nFrameHeightPixels;
						imageData->m_C_ImageInfo->m_nImageWidthPixels = idi->m_nImageWidthPixels;
						imageData->m_C_ImageInfo->m_nImageHeightPixels = idi->m_nImageHeightPixels;
						imageData->m_C_ImageInfo->m_fResolution = idi->m_fResolution;
						imageData->m_C_ImageInfo->m_nHour = idi->m_nHour;
						imageData->m_C_ImageInfo->m_nMinute = idi->m_nMinute;
						imageData->m_C_ImageInfo->m_nSecond = idi->m_nSecond;
						imageData->m_C_ImageInfo->m_nMillisecond = idi->m_nMillisecond;
						imageData->m_C_ImageInfo->m_nDepth = idi->m_nDepth;
						imageData->m_C_ImageInfo->m_nFrameNumber = idi->m_nFrameNumber;
					}
				}

			}

		}
		return nDataCount;
	}
	int CLR_DemoServer::_getImageDisplayData_D_PW(ImageData^ imageData, int dataNumber, bool% resetDisplayData)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		resetDisplayData = false;
		int nDataCount = 0;
		int maxReadNumber = dataNumber;
		if (dataNumber > imageData->m_D_PW_ImageInfos->Length)
		{
			maxReadNumber = imageData->m_D_PW_ImageInfos->Length;
		}
		imageData->m_nD_PWImageDataLen = 0;
		for (int i = 0; i < maxReadNumber; i++)
		{
			CIES::UImagingData uimagingData;
			imageData->m_bBHadData = false;
			imageData->m_bCHadData = false;
			imageData->m_bD_PWHadData = false;

			int nDC = m_pImageEngineServer->GetImageDisplayData(uimagingData);

			if (nDC == 1)
			{
				if (uimagingData.m_bDPWHadData)
				{
					int datalen = 0;
					int* pD = (int*)uimagingData.m_D_PW_Data.GetFrameData(datalen);
					if (pD != nullptr)
					{
						int d_PWImageData = datalen / 4;
						imageData->m_nD_PWImageDataLen += d_PWImageData;
						if (imageData->m_D_PW_Imagedata == nullptr)
							imageData->m_D_PW_Imagedata = gcnew array<int>(1024 * 4 * 10);

						Marshal::Copy((IntPtr)pD, imageData->m_D_PW_Imagedata, nDataCount * d_PWImageData, d_PWImageData);

						CIES::D_PWDataInfo* dpwi = uimagingData.m_D_PW_Data.GetD_PWDataInfo();

						if (dpwi != nullptr)
						{
							imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_bHadData = dpwi->envelopeInfo.bHadData;
							imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_nEnvelopeMax = dpwi->envelopeInfo.nEnvelopeMax;
							imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_nEnvelopeMean = dpwi->envelopeInfo.nEnvelopeMean;

							imageData->m_D_PW_ImageInfos[nDataCount]->m_nDateNum = dpwi->m_nDateNum;
							imageData->m_D_PW_ImageInfos[nDataCount]->m_AutoMeasureAngle = dpwi->m_AutoMeasureAngle;
							imageData->m_D_PW_ImageInfos[nDataCount]->m_BloodRadius_B = dpwi->m_BloodRadius_B;
							imageData->m_D_PW_ImageInfos[nDataCount]->m_BloodRadius_T = dpwi->m_BloodRadius_T;
							imageData->m_D_PW_ImageInfos[nDataCount]->m_nD_PWDataLen = d_PWImageData;
							if (dpwi->m_nDateNum == 0)
							{
								resetDisplayData = true;
							}
						}
					}
					nDataCount++;
				}
			}
		}
		imageData->m_nD_PWDataNum = nDataCount;
		imageData->m_bD_PWHadData = (nDataCount > 0 ? true : false);
		return nDataCount;
	}

	int CLR_DemoServer::_getHistoryImageData_D_PW(ImageData_D_PW^ imageData, int startIndex, int dataNumber)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		if (pD_PWArray != nullptr)
		{
			delete[]pD_PWArray;
			pD_PWArray = nullptr;
		}
		m_nD_PWArrayLen = dataNumber;
		pD_PWArray = new CIES::UImagingData[m_nD_PWArrayLen];

		int nGetCount = m_pImageEngineServer->GetImageDisplayCacheData_D_PW(pD_PWArray, startIndex, dataNumber);

		if (nGetCount <= 0)
		{
			return 0;
		}
		imageData->m_bD_PWHadData = true;
		imageData->m_D_PW_Imagedata = gcnew array<int>(nGetCount * 1024);//按照最大一线1024点计算
		imageData->m_D_PW_ImageInfos = gcnew array<D_PWDataInfo_CLR^>(nGetCount);
		for (int i = 0; i < nGetCount; i++)
		{
			imageData->m_D_PW_ImageInfos[i] = nullptr;
		}

		imageData->m_nD_PWDataNum = nGetCount;
		imageData->m_nD_PWImageDataLen = 0;
		int nDataCount = 0;
		for (int i = 0; i < nGetCount; i++)
		{
			if (pD_PWArray[i].m_bDPWHadData)
			{
				int datalen = 0;
				int* pD = (int*)pD_PWArray[i].m_D_PW_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{
					int d_PWImageData = datalen / 4;
					imageData->m_nD_PWImageDataLen += d_PWImageData;

					Marshal::Copy((IntPtr)pD, imageData->m_D_PW_Imagedata, nDataCount * d_PWImageData, d_PWImageData);

					CIES::D_PWDataInfo* dpwi = pD_PWArray[i].m_D_PW_Data.GetD_PWDataInfo();

					if (dpwi != nullptr)
					{
						imageData->m_D_PW_ImageInfos[nDataCount] = gcnew D_PWDataInfo_CLR();
						imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo = gcnew EnvelopeInfo_CLR();
						imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_bHadData = dpwi->envelopeInfo.bHadData;
						imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_nEnvelopeMax = dpwi->envelopeInfo.nEnvelopeMax;
						imageData->m_D_PW_ImageInfos[nDataCount]->envelopeInfo->m_nEnvelopeMean = dpwi->envelopeInfo.nEnvelopeMean;

						imageData->m_D_PW_ImageInfos[nDataCount]->m_nDateNum = dpwi->m_nDateNum;
						imageData->m_D_PW_ImageInfos[nDataCount]->m_nD_PWDataLen = d_PWImageData;
					}
				}
				nDataCount++;
			}

		}
		imageData->m_nD_PWDataNum = nDataCount;
		imageData->m_bD_PWHadData = (nDataCount > 0 ? true : false);
		return nDataCount;
	}

	int CLR_DemoServer::_getHistoryImageData_BM(int OneScreenWidth, int index, ImageData^ imageData, int% StartPixel, int% MIndexPixel, double MIndexAppend, bool getOneScreen, double MPosOnScreen)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		CIES::UImagingData uimagingData;
		int startPixelTemp = 0;
		int IndexPixelTemp = 0;
		int nDataCount = m_pImageEngineServer->GetImageDisplayCacheData_BM(OneScreenWidth, index, &uimagingData, startPixelTemp, IndexPixelTemp, MIndexAppend, getOneScreen, MPosOnScreen);
		if (nDataCount == 1)
		{
			imageData->m_bBHadData = false;
			imageData->m_bMHadData = false;
			if (uimagingData.m_bBHadData)
			{
				imageData->m_bBHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_B_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{

					int bImageData = datalen / 4;
					imageData->m_nBImageDataLen = bImageData;
					if (imageData->m_B_Imagedata == nullptr)
						imageData->m_B_Imagedata = gcnew array<int>(2048 * 2048);
					Marshal::Copy((IntPtr)pD, imageData->m_B_Imagedata, 0, bImageData);

					CIES::ImageDataInfo* idi = uimagingData.m_B_Data.GetImageDataInfo();

					if (idi != nullptr)
					{
						imageData->m_B_ImageInfo->m_fFrameRate = idi->m_fFrameRate;
						imageData->m_B_ImageInfo->m_nFrameWidthPixels = idi->m_nFrameWidthPixels;
						imageData->m_B_ImageInfo->m_nFrameHeightPixels = idi->m_nFrameHeightPixels;
						imageData->m_B_ImageInfo->m_nImageWidthPixels = idi->m_nImageWidthPixels;
						imageData->m_B_ImageInfo->m_nImageHeightPixels = idi->m_nImageHeightPixels;
						imageData->m_B_ImageInfo->m_fResolution = idi->m_fResolution;
						imageData->m_B_ImageInfo->m_nHour = idi->m_nHour;
						imageData->m_B_ImageInfo->m_nMinute = idi->m_nMinute;
						imageData->m_B_ImageInfo->m_nSecond = idi->m_nSecond;
						imageData->m_B_ImageInfo->m_nMillisecond = idi->m_nMillisecond;
						imageData->m_B_ImageInfo->m_nDepth = idi->m_nDepth;
						imageData->m_B_ImageInfo->m_nFrameNumber = idi->m_nFrameNumber;
					}
				}

			}

			if (uimagingData.m_bMHadData)
			{
				imageData->m_bMHadData = true;
				int datalen = 0;
				int* pD = (int*)uimagingData.m_M_Data.GetFrameData(datalen);
				if (pD != nullptr)
				{

					int mImageData = datalen / 4;
					imageData->m_nMImageDataLen = mImageData;
					if (imageData->m_M_Imagedata == nullptr)
						imageData->m_M_Imagedata = gcnew array<int>(2048 * 2048);

					Marshal::Copy((IntPtr)pD, imageData->m_M_Imagedata, 0, mImageData);

					CIES::MDataInfo* di = uimagingData.m_M_Data.GetMDataInfo();

					if (di != nullptr)
					{
						imageData->m_M_ImageInfo->m_nFrameNumber = di->m_nFrameNumber;
						imageData->m_M_ImageInfo->m_nImageHeightPixels = di->m_nImageHeightPixels;
						imageData->m_M_ImageInfo->m_nMLineNum = di->m_nMLineNum;
						imageData->m_M_ImageInfo->m_bClearScreen = di->m_bClearScreen;
					}
				}

			}

			StartPixel = startPixelTemp;
			MIndexPixel = IndexPixelTemp;
		}

		return nDataCount;
	}
	int CLR_DemoServer::_getUSDeviceInfos(array<CLRUSDeviceInfo^>^ usDeviceInfoArray, int% getDataNum)
	{
		CIES::USDeviceInfo usDeviceIArray[5];
		int nArrayLen = getDataNum;

		int nRet = m_pImageEngineServer->GetUSDeviceInfos(usDeviceIArray, nArrayLen);
		getDataNum = nArrayLen;
		for (int i = 0; i < nArrayLen; i++)
		{
			usDeviceInfoArray[i] = gcnew CLRUSDeviceInfo();

			usDeviceInfoArray[i]->isPowerOff = usDeviceIArray[i].isPowerOff;
			usDeviceInfoArray[i]->VID = usDeviceIArray[i].VID;
			usDeviceInfoArray[i]->PID = usDeviceIArray[i].PID;
			usDeviceInfoArray[i]->DevicePath = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)usDeviceIArray[i].DevicPath);
			usDeviceInfoArray[i]->fwVersion_major = usDeviceIArray[i].fwVersion_major;
			usDeviceInfoArray[i]->fwVersion_minor = usDeviceIArray[i].fwVersion_minor;
			usDeviceInfoArray[i]->nLogicVersion_Major = usDeviceIArray[i].hwInfo.nLogicVersion[0];
			usDeviceInfoArray[i]->nLogicVersion_Minor = usDeviceIArray[i].hwInfo.nLogicVersion[1];
			usDeviceInfoArray[i]->nHWVersion_Major = usDeviceIArray[i].hwInfo.nHWVersion[0];
			usDeviceInfoArray[i]->nHWVersion_Minor = usDeviceIArray[i].hwInfo.nHWVersion[1];
			usDeviceInfoArray[i]->unLogicCompileVersion = usDeviceIArray[i].hwInfo.unLogicCompileVersion;
			usDeviceInfoArray[i]->DNA1 = usDeviceIArray[i].hwInfo.DNA1;
			usDeviceInfoArray[i]->DNA2 = usDeviceIArray[i].hwInfo.DNA2;
			usDeviceInfoArray[i]->DNA3 = usDeviceIArray[i].hwInfo.DNA3;
			usDeviceInfoArray[i]->DNA4 = usDeviceIArray[i].hwInfo.DNA4;
			usDeviceInfoArray[i]->fUSBSupplyVoltage = usDeviceIArray[i].hwInfo.fUSBSupplyVoltage;
			usDeviceInfoArray[i]->fTotalCurrent = usDeviceIArray[i].hwInfo.fTotalCurrent;
			usDeviceInfoArray[i]->fTemperature = usDeviceIArray[i].hwInfo.fTemperature;
			usDeviceInfoArray[i]->fEmissionVoltage = usDeviceIArray[i].hwInfo.fEmissionVoltage;
			if (usDeviceIArray[i].PID == 0x4611)
			{
				usDeviceInfoArray[i]->probeID = usDeviceIArray[i].productInfo.probeID;
				usDeviceInfoArray[i]->probeConnect = true;
			}
			else
			{
				usDeviceInfoArray[i]->ProbeCanreplaced = usDeviceIArray[i].hwInfo.ProbeCanreplaced;
				usDeviceInfoArray[i]->IsMultiProbe = usDeviceIArray[i].hwInfo.IsMultiProbe;
				usDeviceInfoArray[i]->probeID = usDeviceIArray[i].hwInfo.unProbeID & 0x0000FFFF;
				usDeviceInfoArray[i]->probeAppendInfo = usDeviceIArray[i].hwInfo.unProbeAppendInfo & 0x0000FFFF;
				usDeviceInfoArray[i]->probeConnect = usDeviceIArray[i].hwInfo.unProbeConnect;
				usDeviceInfoArray[i]->probeID_B = usDeviceIArray[i].hwInfo.unProbeID_B & 0x0000FFFF;
				usDeviceInfoArray[i]->probeAppendInfo_B = usDeviceIArray[i].hwInfo.unProbeAppendInfo_B & 0x0000FFFF;
				usDeviceInfoArray[i]->probeConnect_B = usDeviceIArray[i].hwInfo.unProbeConnect_B;
			}

			usDeviceInfoArray[i]->productInfo = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)usDeviceIArray[i].productInfo.productCategory) + usDeviceIArray[i].productInfo.productVersion;
			usDeviceInfoArray[i]->serialNumber = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((IntPtr)usDeviceIArray[i].snInfo.serialNumber);
			usDeviceInfoArray[i]->isExportFlag = usDeviceIArray[i].otherInfo.isExportFlag;
			usDeviceInfoArray[i]->isOEMFlag = usDeviceIArray[i].otherInfo.isOEMFlag;
			usDeviceInfoArray[i]->usHardWareIndex = usDeviceIArray[i].usHardWareIndex;

			usDeviceInfoArray[i]->PCB_Major = usDeviceIArray[i].pcb_PCBA_Info.PCB_Major;
			usDeviceInfoArray[i]->PCB_Minor = usDeviceIArray[i].pcb_PCBA_Info.PCB_Minor;
			usDeviceInfoArray[i]->PCBA_Major = usDeviceIArray[i].pcb_PCBA_Info.PCBA_Major;
			usDeviceInfoArray[i]->PCBA_Minor = usDeviceIArray[i].pcb_PCBA_Info.PCBA_Minor;
		}

		return nRet;
	}

	int CLR_DemoServer::_getHardWareInfo(CLRHardWareInfo^ hardwareInfo)
	{
		if (hardwareInfo == nullptr)
		{
			return -1;
		}
		CIES::HWInfo hwIArray;

		int nRet = m_pImageEngineServer->GetHardWareInfo(hwIArray);
		if (nRet == 0)
		{
			hardwareInfo->nLogicVersion_Major = hwIArray.nLogicVersion[0];
			hardwareInfo->nLogicVersion_Minor = hwIArray.nLogicVersion[1];
			hardwareInfo->nHWVersion_Major = hwIArray.nHWVersion[0];
			hardwareInfo->nHWVersion_Minor = hwIArray.nHWVersion[1];
			hardwareInfo->unLogicCompileVersion = hwIArray.unLogicCompileVersion;
			hardwareInfo->DNA1 = hwIArray.DNA1;
			hardwareInfo->DNA2 = hwIArray.DNA2;
			hardwareInfo->DNA3 = hwIArray.DNA3;
			hardwareInfo->DNA4 = hwIArray.DNA4;
			hardwareInfo->fUSBSupplyVoltage = hwIArray.fUSBSupplyVoltage;
			hardwareInfo->fTotalCurrent = hwIArray.fTotalCurrent;
			hardwareInfo->fTemperature = hwIArray.fTemperature;
			hardwareInfo->fEmissionVoltage = hwIArray.fEmissionVoltage;
			hardwareInfo->OtherMsg1 = hwIArray.OtherMsg1;
			hardwareInfo->ProbeCanreplaced = hwIArray.ProbeCanreplaced;
			
			hardwareInfo->unProbeID = hwIArray.unProbeID;

			hardwareInfo->IsMultiProbe = hwIArray.IsMultiProbe;
			hardwareInfo->unProbeID = hwIArray.unProbeID;
			hardwareInfo->unProbeAppendInfo = hwIArray.unProbeAppendInfo;
			hardwareInfo->unProbeConnect = hwIArray.unProbeConnect;
			hardwareInfo->unProbeID_B = hwIArray.unProbeID_B;
			hardwareInfo->unProbeAppendInfo_B = hwIArray.unProbeAppendInfo_B;
			hardwareInfo->unProbeConnect_B = hwIArray.unProbeConnect_B;
		}

		return nRet;
	}

	int CLR_DemoServer::_getProbeInfo(Probe_Info^ probeInfo, System::String^ paramPath)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		CIES::ProbeInfo probeinfo;
		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char* pFilePath = static_cast<char*>(ip.ToPointer());

		int nRet = m_pImageEngineServer->GetProberInfo(probeinfo, pFilePath);
		Marshal::FreeHGlobal(ip);
		if (nRet == 0)
		{
			probeInfo->m_fPith = probeinfo.m_fProbe_pitch;
			probeInfo->m_fRadiusCurvature = probeinfo.m_fProbe_r;
			probeInfo->m_nFocusNum = probeinfo.m_nProbe_element;
			probeInfo->Probe_type = probeinfo.m_nProbe_type;
			probeInfo->m_fImageAngle = probeinfo.m_fImageAngle;
			probeInfo->m_nShowDepthLevel = probeinfo.m_nShowDepthLevel;
			probeInfo->m_pDepthList = gcnew array<int>(probeInfo->m_nShowDepthLevel);

			Marshal::Copy((IntPtr)probeinfo.m_pDepthList, probeInfo->m_pDepthList, 0, probeInfo->m_nShowDepthLevel);

		
			probeInfo->m_bC_Tx_deflectionflag = probeinfo.m_bC_Tx_deflectionflag;
			Marshal::Copy((IntPtr)probeinfo.m_pC_Tx_angle, probeInfo->m_pC_Tx_angle, 0, 5);

			probeInfo->m_bD_Tx_deflectionflag = probeinfo.m_bD_Tx_deflectionflag;
			Marshal::Copy((IntPtr)probeinfo.m_pD_Tx_angle, probeInfo->m_pD_Tx_angle, 0, 5);

			Marshal::Copy((IntPtr)probeinfo.m_fB_fund_Tx_freq, probeInfo->m_fB_fund_Tx_freq, 0, 3);
			Marshal::Copy((IntPtr)probeinfo.m_fB_harm_Tx_freq, probeInfo->m_fB_harm_Tx_freq, 0, 3);
			Marshal::Copy((IntPtr)probeinfo.m_fC_Tx_freq, probeInfo->m_fC_Tx_freq, 0, 2);
			Marshal::Copy((IntPtr)probeinfo.m_fD_Tx_freq, probeInfo->m_fD_Tx_freq, 0, 2);
		}
		return nRet;
	}

	int CLR_DemoServer::_getD_PWInfo(D_PWInfo^ pwInfo, System::String^ paramPath)
	{
		if (m_Instance == nullptr)
		{
			return -1;
		}

		CIES::D_PWInfo d_PwInfo;
		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char* pFilePath = static_cast<char*>(ip.ToPointer());

		int nRet = m_pImageEngineServer->GetD_PWInfo(d_PwInfo, pFilePath);
		Marshal::FreeHGlobal(ip);
		if (nRet == 0)
		{
			pwInfo->m_nFrequency = d_PwInfo.m_nFrequency;
			pwInfo->m_bUseLaunchDeflection = d_PwInfo.m_bUseLaunchDeflection;
			pwInfo->m_nLaunchDeflectionAngle = d_PwInfo.m_nLaunchDeflectionAngle;

			pwInfo->m_fPRF = gcnew array<float>(8);

			Marshal::Copy((IntPtr)d_PwInfo.m_fPRF, pwInfo->m_fPRF, 0, 8);
		}
		return nRet;
	}
	int CLR_DemoServer::_getCurrentFocusArrayByParamPath(System::String^ paramPath, int m_nDepth_level, int m_nHarmonic, array<float>^ B_FocusArray)
	{
		if (m_pImageEngineServer == nullptr)
			return -5;
		if (B_FocusArray == nullptr || B_FocusArray->Length < 8)
			return -2;
		float FocusArray[8];
		int FocusArrayLen = 8;
		CIES::D_PWInfo d_PwInfo;
		System::IntPtr ip = Marshal::StringToHGlobalAnsi(paramPath); //string参数转化成char* 其他数值型参数，直接赋值，不需要转换。
		char* pFilePath = static_cast<char*>(ip.ToPointer());

		int nRet = m_pImageEngineServer->GetCurrentFocusArrayByParamPath(pFilePath, m_nDepth_level, m_nHarmonic, FocusArray, FocusArrayLen);

		Marshal::Copy((IntPtr)FocusArray, B_FocusArray, 0, FocusArrayLen);


		return nRet;
	}

}