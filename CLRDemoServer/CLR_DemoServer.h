#pragma once
#include "ColorImageEngineServer.h"
#include "DataSource.h"
#include "CIESParamter.h"
#include "CLRData.h"

//CLR层，将C++代码通过CLR包装，C#层方便引用CLR层的数据。
namespace CLRDemoServer {

	public ref class CLR_DemoServer
	{
	private:
		CLR_DemoServer(void);
		~CLR_DemoServer(void);

		void ReleaseResource();
		void LoadKRProcessClass();

		static CLR_DemoServer^ m_Instance = nullptr;
		NativeInterface^ _nativeInterface;
		CIES::Native* m_pNative;
		CIES::ColorImageEngineServer* m_pImageEngineServer;
		CIES::DataSource* m_pDataSource;
		bool m_bStartSaveVideo;
		CIES::UImagingData* pD_PWArray;
		int m_nD_PWArrayLen;
	public:

		static CLR_DemoServer^ GetInstance();	//获取单例

		static void ReleaseInstance();		//注释

		int GetSDKVersion(array<int >^ SDKversion); //获取SDK版本

		int GetUSDeviceInfos(array<CLRUSDeviceInfo^>^ usDeviceInfoArray, int% getDataNum);
		void SetUSDevicePath(System::String^ DevicePath);

		int SetParamPath(System::String^ paramPath, CLScanMode::CLScanModeEnum scanMode);	//设置参数目录

		int StartImageEngine();				//启动图像引擎
		int StopImageEngine();				//停止图像引擎

		void Freeze();
		void UnFreeze();

		bool IsRuning();

		int GetHardWareInfo(CLRHardWareInfo^ hardWareInfo);

		int GetProberInfo(Probe_Info^ probeInfo, System::String^ paramPath); //获取探头信息
		int GetD_PWInfo(D_PWInfo^ pwInfo, System::String^ paramPath);			 //获取PW显示信息
		int SetImageParam_B(B_ImageParam_CLR^ b_ImageParam_CLR); //设置算法参数
		int SetImageParam_C(C_ImageParam_CLR^ c_ImageParam_CLR); //设置算法参数
		int SetImageParam_D_PW(D_PW_ImageParam_CLR^ d_PW_ImageParam_CLR); //设置算法参数
		int SetImageParam_M(M_ImageParam_CLR^ m_ImageParam_CLR); //设置算法参数

		/// 设置焦点，此焦点设置范围限定在获取的全域焦点范围内。 mm
		/// </summary>
		int SetFocusArea(float fFocusArea);

		int GetCurrentFocusArea(float% startDepth, float% endDepth);//获取当前焦区范围  当B的基波/谐波切换， 深度切换，焦区修改时获取
		//获取当前焦点组  当B的基波/谐波切换， 深度切换时获取焦点数值数组
		//数组长度8，1~6代表焦点0~6，7~8是全域的起始焦点
		int GetCurrentFocusArray(array<float>^ B_FocusArray);
		int GetCurrentFocusArrayByParamPath(System::String^ paramPath, int m_nDepth_level, int m_nHarmonic, array<float>^ B_FocusArray);


		int GetColorMap(array<int>^ B_MapArray, array<int>^ C_MapArray); // 获取map
		int GetColorMap_B(array<int>^ B_MapArray);
		int GetColorMap_C(array<int>^ C_MapArray);
		int GetColorMap_D(array<int>^ D_MapArray);
		int GetColorMap_M(array<int>^ M_MapArray);

		int GetImageDisplayData(ImageData^ imageData);	//获取显示数据

		int GetHistoryImageCount();						//获取历史图像数据量(录像数据量)
		int GetHistoryImageData(ImageData^ imageData, int nIndex); //获取历史数据
		int GetImageDisplayData_D_PW(ImageData^ imageData, int dataNumber, bool% resetDisplayData);	//获取显示数据 D_PW模式下使用
		int GetHistoryImageData_D_PW(ImageData_D_PW^ imageData, int startIndex, int dataNumber);
		int GetHistoryImageData_BM(int OneScreenWidth, int index, ImageData^ imageData, int% StartPixel, int% MIndexPixel, double MIndexAppend, bool getOneScreen, double MPosOnScreen);

		float GetCurrentFrameRate();					//获取当前帧率

		void SetUseImageHistoryCache(bool isUse, int cacheLen);

		void SetDelegateMthod(HardWareMsgHandler^ d);
		void SetDelegateMthod(HardWareMsgSHandler^ d);
		void ClearDelegateMthod();


		bool IsUSBOpen();	//USB设备是否开启


		int GetImageWidthPixels();		//获取图像宽

		int GetImageHeightPixels();		//获取图像高

		int PowerOn(System::String^ DevicePath);
		int PowerOff(System::String^ DevicePath);

	public: //C
		/*
		*设置C取样门参数
		*
		*帧图像左上角为原点 0,0
		*
		*Param
		*int Depth		//当前深度 mm
		*double nFWidthP	//帧图像宽度像素，SDK中实际上传的值
		*double nFHeightP	//帧图像高度像素，SDK中实际上传的值
		*double nIWidthP	//实际图像区域宽度像素 SDK中实际上传的值
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值
		*double SX			//取样框左上点坐标X，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SY			//取样框左上点坐标Y，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SWidhtLenP	//取样框宽像素（四边形的宽），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeightLenP//取样框高像素（四边形的高），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*int LDAngle	//发射偏转角度
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetCSamplingFrameParam_Quadrangle(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SX, double SY, double SWidthLen, double SHeightLen, int LDAngle);


		/*
		*获取PRF
		*Param
		*int Depth			//当前深度 mm
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值。
		*double SY			//取样框左上点坐标Y，相对于帧图像的位置，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeightLenP //取样框高像素（四边形的高），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*int LDAngle		//发射偏转角度
		*array<float>^ fPrfList	//PRF列表== 8
		*double% fTxFrequency //发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		*/
		int Get_C_PRFList_Quadrangle(int Depth, double nIHeightP, double SY, double SHeightLen, int LDAngle, array<double>^ fPrfList, double% fTxFrequency);

		/*
		*设置C取样门参数
		*
		*设置完取样们参数后，调用Get_C_PRFList_Sector方法获取新得PRF列表，然后设置C 得PRFLevel参数
		*
		*帧图像左上角为原点 0,0
		*
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nFWidthP	//帧图像宽度像素，SDK中实际上传的值
		*double nFHeightP	//帧图像高度像素，SDK中实际上传的值
		*double nIWidthP	//实际图像区域宽度像素 SDK中实际上传的值
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值
		*double SCToPCCP	//扇形取样框的中心点到探头圆心的长度，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeight		//扇形取样框的高度，上下弧的差，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SectorAngle	//扇形取样框的夹角 单位度
		*double SectorCenterAngle//扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度

		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetCSamplingFrameParam_Sector(int Depth, double nFWidthP, double nFHeightP, double nIWidthP, double nIHeightP, double SCToPCCP, double SHeight, double SectorAngle, double SectorCenterAngle);

		/*
		*获取PRF
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值。
		*double SCToPCCP	//扇形取样框的中心点到探头圆心的长度，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeight		//扇形取样框的高度，上下弧的差，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double* fPrfList	//PRF列表
		*int &fPrfListLen   // == 8
		*double% fTxFrequency //发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		*/
		int Get_C_PRFList_Sector(int Depth, double nIHeightP, double SCToPCCP, double SHeight, array<double>^ fPrfList, double% fTxFrequency);
	public://D

		/*
		*设置D_PW 取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*int launchDeflectionAngle 发射偏转角度
		*int samplingVolume 取样门宽度
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_Line(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, int launchDeflectionAngle, int samplingVolume); //设置取样门参数

		/*
		*获取PRF
		*Param
		*float SY					取样门中心点Y值
		*float depth				当前深度,mm
		*float fIHeightP			当前B显示像素
		*int launchDeflectionAngle	发射偏转角度
		*double* fPrfList			//PRF列表
		*double% fTxFrequency //发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		*/
		int Get_D_PRFList_Line(float SY, float Depth, float fIHeightP, int launchDeflectionAngle, array<double>^ fPrfList, double% fTxFrequency);

		/*
		*设置D_PW 取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*float SectorCenterAngle 扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度
		*int samplingVolume 取样门宽度
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_Convex(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume);

		/*
		*获取PRF
		*Param
		*float SX					取样门中心点Y值
		*float SY					取样门中心点Y值
		*float depth				当前深度cm
		*float fIWidthP				当前B显示宽度像素
		*float fIHeightP			当前B显示像素
		*double* fPrfList			//PRF列表
		*double% fTxFrequency //发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		*/
		int Get_D_PRFList_Convex(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, array<double>^ fPrfList, double% fTxFrequency);

		/*
		*设置D_PW 取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*float SectorCenterAngle 扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度
		*int samplingVolume 取样门宽度
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_PA(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, float SectorCenterAngle, int samplingVolume);

		/*
		*获取PRF
		*Param
		*float SX					取样门中心点Y值
		*float SY					取样门中心点Y值
		*float depth				当前深度cm
		*float fIWidthP				当前B显示宽度像素
		*float fIHeightP			当前B显示像素
		*double* fPrfList			//PRF列表
		*double% fTxFrequency //发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		*/
		int Get_D_PRFList_PA(float SX, float SY, float Depth, float fIWidthP, float fIHeightP, array<double>^ fPrfList, double% fTxFrequency);



	public://M

		int SetSamplingLineParam_Line(float SLX, float depth, float imageWidth, float imageHight);
		int SetSamplingLineParam_Convex(float SectorCenterAngle, float depth, float imageWidth, float imageHight);
		int SetSamplingLineParam_Phased(float SectorCenterAngle, float depth, float imageWidth, float imageHight);

		public:
			/*
			*自动管径测量
			*冻结后调用
			*int index				获取显示数据的索引，先调用GetImageDisplayCacheDataCount方法获取缓存的数据量Num，索引的范围是0 - (Num-1)。
			*float PointOnBImageX	B图像上选取的一点的X轴坐标
			*float PointOnBImageY	B图像上选取的一点的Y轴坐标
			*float &CenterOfVascularDiameterX	返回血管圆心的X轴坐标
			*float &CenterOfVascularDiameterY	返回血管圆心的Y轴坐标
			*float &VascularRadius				返回血管半径
			*
			*返回值：-1表示自动管径测量失败
			*
			*/
			int AutomaticDiameterMeasurement(int index, float PointOnBImageX, float PointOnBImageY, float% CenterOfVascularDiameterX, float% CenterOfVascularDiameterY, float% VascularRadius); //自动管径测量


			/*
			*自动TGC调节
			*
			*array<float>^ fTGCList		返回TGC 控制数组数组长度固定是6
			*
			*返回值：-1表示自动TGC失败
			*
			*/
			int AutomaticTGC(array<float>^ fTGCList);


			/**
			* 根据C图像，自动计算D模式取样门位置
			*/
			int AutomaticSV(float% svx, float% svy, float% svH, int% svAngleLevel);

			//D模式自动PRF 自动Baseline
			int AutoPRF(int% PRF_level, int% Baseline_level);


	private:

		int _setImageParam_B(B_ImageParam_CLR^ b_ImageParam_CLR);
		int _setImageParam_C(C_ImageParam_CLR^ c_ImageParam_CLR);
		int _setImageParam_D_PW(D_PW_ImageParam_CLR^ d_PW_ImageParam_CLR);
		int _setImageParam_M(M_ImageParam_CLR^ m_ImageParam_CLR);

		int _getImageDisplayData(ImageData^ imageData);
		int _getHistoryImageData(ImageData^ imageData, int nIndex);
		int _getImageDisplayData_D_PW(ImageData^ imageData, int dataNumber, bool% resetDisplayData);
		int _getHistoryImageData_D_PW(ImageData_D_PW^ imageData, int startIndex, int dataNumber);
		int _getHistoryImageData_BM(int OneScreenWidth, int index, ImageData^ imageData, int% StartPixel, int% MIndexPixel, double MIndexAppend, bool getOneScreen, double MPosOnScreen);

		int _getUSDeviceInfos(array<CLRUSDeviceInfo^>^ usDeviceInfoArray, int% getDataNum);
		int _getHardWareInfo(CLRHardWareInfo^ hardwareInfo);
		int _getProbeInfo(Probe_Info^ probeInfo, System::String^ paramPath);
		int _getD_PWInfo(D_PWInfo^ pwInfo, System::String^ paramPath);

		int _getCurrentFocusArrayByParamPath(System::String^ paramPath, int m_nDepth_level, int m_nHarmonic, array<float>^ B_FocusArray);

	};
}
