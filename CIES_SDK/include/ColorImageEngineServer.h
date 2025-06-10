//File：ImageEngineServer.h
//Purpose：定义数据引擎接口。
#pragma once


#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif

#include "CIESParamter.h"
#include "DataSourceInterface.h"
#include "FrameDataListener.h"
#include "ShowFrame.h"

namespace CIES {


	class COLORIMAGEENGINESERVER_API ColorImageEngineServer
	{
	public:
		/*
		*需要添加数据源接口对象
		*此处使用子类DataSource即可
		*
		*/
		ColorImageEngineServer(DataSourceInterface &dsInterface);

		~ColorImageEngineServer();

		//////////////////////////////////////////////////////////////////////////
		//Version
		//////////////////////////////////////////////////////////////////////////
		/*
		*获取当前版本
		*
		*/

		Version GetVersion();
		//////////////////////////////////////////////////////////////////////////
		//服务器控制
		//////////////////////////////////////////////////////////////////////////
		/*
		*获取当前已经连接超声设备的列表
		*当出现设备插入和断开时，请重新获取设备列表
		*Param
		*USDeviceInfo* usDeviceInfoArray	连接超声设备的列表，如果实际连接设备数量大于列表长度，只返回前nArrayLen个设备
		*int& nArrayLen							传入 连接设备的列表长度，传出 获取到的连接设备列表长度
		*return
		*>=0：实际识别的设备数，可能获取>nArrayLen 长度
		*-1：传入参数错误
		*-2：USB设备开启失败
		*/
		int GetUSDeviceInfos(USDeviceInfo* usDeviceInfoArray, int& nArrayLen);

		/*
		*设置当前超声设备的索引
		*
		*Param
		*char* DevicePath	超声设备系统地址
		*DevicePath 从GetUSDeviceInfos 方法获取的USDeviceInfo对象中获取
		*此方法可以在StartImageEngine调用
		*
		*如果在StartImageEngine之后调
		*1.需要先冻结
		*2.再设置DevicePath
		*3.最后解冻才会生效
		*
		*如果不调用此接口，默认连接第一个识别的设备
		*/
		void SetUSDevicePath(char* DevicePath);


		/*
		 *启动图像引擎
		 *说明：
		 *
		 *return
		 *0：成功
		 *-1：USB设备开启失败
		 *-2：处理流程启动失败
		 *-3: 没有设置正确的CheckModeParamPath路径
		 *-4: 没有设置正确的B_ImageParam参数
		 *-5: 没有设置正确的C_ImageParam参数
		 */

		int StartImageEngine();

		/*
		 *停止图像引擎
		 *
		 *return
		 *0：成功
		 */
		int StopImageEngine();


		/*
		*图像引擎是否运行
		*
		*return
		*true：运行
		*false：停止
		*/
		bool IsRuning();

		/*
		*USB设备是否打开
		*说明：
		*1.IsUSBOpen方法当USB读写错误后，返回false。不能通过此方法来判断USB物理断开。
		*
		*2.使用系统对于USB设备的检测方法，可以准确判断USB是否是物理断开，需要用户自己实现
		*
		*
		*return
		*true：USB设备读写正常
		*false：USB设备读写不正常
		*/
		bool IsUSBOpen();

		/*
		*冻结
		*
		*/
		void Freeze();

		/*
		*解冻
		*说明：
		*当硬件断开并重连后，此方法会重新开启USB设备。无需重新调用StartImageEngine方法
		*/
		void UnFreeze();


		/*
	   * 设置是否使用图像引擎自带的图像历史缓存
	   *
	   * Param
	   *bool isUse
	   * true：通过调用“GetImageDisplayCacheDataCount”方法获取缓存图像的数量
	   * 通过调用“GetImageDisplayCacheData（ImageDisplayData＆pImageDisplayData，int index）”获取任何一帧图像缓存
	   * false：不使用图像缓存，“GetImageDisplayCacheDataCount”方法返回0，
	   *“int GetImageDisplayData(UImagingData& pUImagingData)”方法返回0
	   *
	   * int cacheLen
	   * 如果isUse设置为true，则cacheLen是历史缓存数据长度。 cacheLen设置范围是100-1000。
	   */
		void SetUseImageHistoryCache(bool isUse, int cacheLen);

		/*
		* 设置监听器
		* 当有硬件消息时，会通过FrameDataListener.HardWareMsgUpdated方法返回硬件消息。
		* 帧监听器需要自定义实现
		* Native继承FrameDataListener实现的监听器
		*/
		void SetFrameDataListener(FrameDataListener *listener);


		//////////////////////////////////////////////////////////////////////////
		//数据接口
		//////////////////////////////////////////////////////////////////////////
		/*
		*获取实时图像数据
		*
		*Param
		*ImageDisplayData& pImageDisplayData	显示数据存储对象
		*
		*return
		*0：没有显示数据
		*1：获取到一个显示数据
		*/
		int GetImageDisplayData(UImagingData &pUImagingData);

		/*
		*获取缓存图像数据
		*
		*Param
		*ImageDisplayData& pImageDisplayData	显示数据存储对象
		*int index								获取显示数据的索引，先调用GetImageDisplayCacheDataCount方法获取缓存的数据量Num，索引的范围是0 - (Num-1)。
		*
		*return
		*0：没有显示数据，当index值超出范围时返回值也是 0
		*1：获取到一个显示数据
		*/
		int GetImageDisplayCacheData(UImagingData &pUImagingData, int index);


		/*
		*获取缓存的多普勒数据
		*
		*Param
		*UImagingData* pUImagingData		多普勒数据存储对象 长度== dataLen
		*int dataStartNumber				多普勒数据起始编码
		*int dataLen						要获取的多普勒数据长度
		*
		*return
		*0：没有数据返回
		*1：获取多普勒数据
		*/
		int GetImageDisplayCacheData_D_PW(UImagingData *pUImagingData, int dataStartNumber,
			int dataLen);


		/*
		*获取缓存的M数据
		*
		*Param
		*int OneScreenWidth					一屏的宽度
		*int index							数据缓存索引，从0开始
		*UImagingData& pUImagingData		B & M存储对象
		*int& StartPixel					获取的数据，在宽度为OneScreenWidth的屏幕上，开始绘制的位置。绘制坐标索引从0开始，例如：宽480，绘制索引0-479。
		*int& MIndexPixel					获取的M数据，在宽度为OneScreenWidth的屏幕上，Index数据绘制的结束位置。绘制坐标索引从0开始，例如：宽480，绘制索引0-479。
		*double MIndexAppend				M附加索引，取值范围-1.0-1.0。MIndexAppend 标识在Index位置上 获取前一屏到后一屏位置上的数据。默认==0。
		*bool getOneScreen					是否获取一屏数据 ，获取M图像会比实际的大一些。 当前 getOneScreen 生效时，MIndexAppend失效。
		*double MPosOnScreen				在 getOneScreen==true 时，MPosOnScreen是标识当前MIndex在一屏数据的位置。默认==1。 取值范围0.0-1.0
		*return
		*0：没有数据返回
		*1：获取多普勒数据
		*/
		int GetImageDisplayCacheData_BM(int OneScreenWidth, int index, UImagingData *pUImagingData,
			int &StartPixel, int &MIndexPixel, double MIndexAppend,
			bool getOneScreen, double MPosOnScreen = 1.0);


		/*
		*获取缓存图像数据数量
		*
		*return
		*返回缓存的显示数据量，数值范围：0 - 100
		*
		*/
		int GetImageDisplayCacheDataCount();



		//////////////////////////////////////////////////////////////////////////
		//参数控制
		//////////////////////////////////////////////////////////////////////////



		/*
		*设置检查模式参数路径和扫描模式
		*在StartImageEngine前，需要先调用SetCheckModeParamPath_And_ScanMode方法，SetImageParam_B方法/SetImageParam_C/SetImageParam_C/SetImageParam_D_PW方法对图像引擎初始化。
		*
		*
		*Param
		*char* checkModeParamPath	检查模式参数路径，例：C:/CIESDKUseDemo/IES_SDK/Param/L5-10/Vascular
		*ScanMode::ScanModeEnum scanmode扫描模式
		*return
		*0：成功
		*-1：目录不存在
		*-2：目录结构不正确
		*-999:不支持当前设备
		*/
		int SetCheckModeParamPath_And_ScanMode(char *checkModeParamPath,
			ScanMode::ScanModeEnum scanmode);



		/*
		*设置B/C/D/M图像参数
		*
		*@return
		*0: 成功
		*-1: 有参数未初始化
		*-2: 参数在接口中会进行保护，当参数设置超出范围时，将返回-2。
		*-3: SetCheckModeParamPath_And_ScanMode 方法调用失败
		*/
		int SetImageParam_B(B_ImageParam b_ImageParam);
		int SetImageParam_C(C_ImageParam c_ImageParam);
		int SetImageParam_D_PW(D_PW_ImageParam d_PW_ImageParam);
		int SetImageParam_M(M_ImageParam m_ImageParam);


		/// <summary>
		/// 设置焦点，此焦点设置范围限定在获取的全域焦点范围内。 mm
		/// </summary>
		int SetFocusArea(float fFocusArea);

		//获取当前焦区范围  当B的基波/谐波切换， 深度切换，焦区修改时获取
		int GetCurrentFocusArea(float& startDepth, float& endDepth);

		//获取当前焦点组  当B的基波/谐波切换， 深度切换时获取焦点数值数组
		//数组长度8，1~6代表焦点0~6，7~8是全域的起始焦点
		int GetCurrentFocusArray(float* FocusArray, int& FocusArrayLen);

		//获取当前焦点组  当B的基波/谐波切换， 深度切换时获取焦点数值数组
		//数组长度8，1~6代表焦点0~6，7~8是全域的起始焦点
		int GetCurrentFocusArrayByParamPath(char* checkModeParamPath,int m_nDepth_level, int m_nHarmonic,float* FocusArray, int& FocusArrayLen);
		
		/*
		*获取map
		*
		*Param
		*unsigned int* B_MapArray  B Map                  B map绘制方向由上至下
		*int &B_MapArrayLen        B_MapArrayLen = 256
		*unsigned int* C_MapArray  C Map                  C map绘制方向由上至下，由左至右
		*int &C_MapArraylen        C_MapArrayLen = 256*16
		*@return
		*0: 成功
		*-1: 参数不正确
		*/
		int GetColorMap(unsigned int *B_MapArray, int &B_MapArrayLen, unsigned int *C_MapArray,
			int &C_MapArraylen);

		int GetColorMap_B(unsigned int *B_MapArray, int &B_MapArrayLen);

		int GetColorMap_C(unsigned int *C_MapArray, int &C_MapArraylen);

		int GetColorMap_D(unsigned int *D_MapArray, int &D_MapArrayLen);

		int GetColorMap_M(unsigned int *M_MapArray, int &M_MapArraylen);


		/// <summary>
		/// 获取新的B模式的map
		/// </summary>
		/// <param name="ctrlPoint_X">X轴坐标 取值范围0-255</param>
		/// <param name="ctrlPoint_Y">Y轴 坐标 取值范围0-255</param>
		/// <param name="useCtrlPointLen">有效长度</param>
		/// <param name="B_MapArray"></param>
		/// <param name="B_MapArrayLen"></param>
		/// <returns></returns>
		int GetNewColorMap_B(int ctrlPoint_X[256], int ctrlPoint_Y[256], int useCtrlPointLen, unsigned int* B_MapArray, int& B_MapArrayLen);

		/// <summary>
		/// 设置B的第9档灰阶图
		/// </summary>
		/// <param name="B_MapArray"></param>
		/// <param name="B_MapArrayLen"></param>
		/// <returns></returns>
		int SetColorMap_B(unsigned int* B_MapArray, int B_MapArrayLen);

		/*
		*获取当前探头下，当前深度的帧率,参数预制值，可能比实际值高。
		*/
		float GetCurrentFrameRate();

		/*
		*获取探头信息
		*
		*Param
		*ProbeInfo& probeInfo	探头参数信息
		*char *probeParamPath	检查模式参数路径 例：C:/CIESDKUseDemo/IES_SDK/Param/L5-10/Vascular
		*
		*return
		*0：成功
		*-1：目录错误
		*/
		int GetProberInfo(ProbeInfo &probeInfo, char *checkModeParamPath);

		/*
		*获取PW信息
		*
		*Param
		*D_PWInfo& d_PWInfo			PW显示参数信息
		*char *probeParamPath			检查模式参数路径 例：../Param/L5-10/PICC
		*
		*return
		*0：成功
		*-1：目录错误
		*/


		int GetD_PWInfo(D_PWInfo &d_PWInfo, char *checkModeParamPath);


		/**
		 *获取图像数据的宽度。
		 *在获取宽度之前，需要调用SetCheckModeParamPath_And_ScanMode方法设置检查模式参数路径，
		 *然后调用SetImageParam_B方法设置帧的宽度，高度和当前深度。
		 *修改检查参数路径或修改图像的宽度，高度和当前深度时，可以调用GetImageWidthPixels方法以获取新的宽度。
		 *
		 *
		 * @return
		 * -1 : SetCheckModeParamPath_And_ScanMode 方法调用失败
		 * -2 : 图像的宽，高，深度未设置
		 * >0 : 图像的宽度（像素）
		 */
		int GetImageWidthPixels();

		/**
		 *获取图像数据的高度。
		 *在获取高度之前，需要调用SetCheckModeParamPath_And_ScanMode方法设置检查模式参数路径，
		 *然后调用SetImageParam_B方法设置帧的宽度，高度和当前深度。
		 *修改检查参数路径或修改图像的宽度，高度和当前深度时，可以调用GetImageHeightPixels方法以获取新的高度。
		 *
		 *
		 * @return
		 * -1 : SetCheckModeParamPath_And_ScanMode 方法调用失败
		 * -2 : 图像的宽，高，深度未设置
		 * >0 : 图像的高度（像素）
		 */
		int GetImageHeightPixels();

		//////////////////////////////////////////////////////////////////////////
		//获取硬件消息
		//////////////////////////////////////////////////////////////////////////
		/**
		* 获取当前连接设备的硬件信息
		*
		*
		* @return
		* 0 :成功
		* -2: 发送usb消息失败，请检查usb连接
		* -3: 没获取到硬件信息请重置
		*/
		int GetHardWareInfo(HWInfo &hardWareInfo);


		//B

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
		int AutomaticDiameterMeasurement(int index, float PointOnBImageX, float PointOnBImageY,
			float &CenterOfVascularDiameterX,
			float &CenterOfVascularDiameterY, float &VascularRadius);


		/*
		*自动TGC调节
		*
		*float *TGCArray		返回TGC 控制数组
		*int& TGCArrayLne		数组长度固定是6
		*
		*返回值：-1表示自动TGC失败
		*
		*/
		int AutomaticTGC(float *TGCArray, int &TGCArrayLne);


		/**
		* 根据C图像，自动计算D模式取样门位置 在线阵的血管模式中使用
		* float& svx	取样门X坐标 在图像区域
		* float& svy	取样门Y坐标  在图像区域
		* float& svH	取样容积高度像素
		* int& svAngleLevel取样门偏转角度
		* 
		*/
		int AutomaticSV(float& svx, float& svy, float& svH, int& svAngleLevel);
		
		/**
		* 根据D图像，自动计算D模式PRF和Baseline
		* int& PRF_level		PRF档位
		* int& Baseline_level	基线档位
		*
		*/
		int AutoPRF(int& PRF_level, int& Baseline_level);//D模式自动PRF 自动Baseline

		//////////////////////////////////////////////////////////////////////////
		//C 取样窗控制
		//////////////////////////////////////////////////////////////////////////

		/*
		*设置C 四边形取样框参数
		*
		*设置完取样们参数后，调用Get_C_PRFList_Quadrangle方法获取新得PRF列表，然后设置C 得PRFLevel参数
		*
		*帧图像左上角为原点 0,0
		*
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nFWidthP	//帧图像宽度像素，SDK中实际上传的值
		*double nFHeightP	//帧图像高度像素，SDK中实际上传的值
		*double nIWidthP	//实际图像区域宽度像素 SDK中实际上传的值 GetImageWidthPixels方法获取
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值 GetImageHeightPixels方法获取
		*double SX			//取样框左上点坐标X，相对于实际图像区域宽度像素，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SY			//取样框左上点坐标Y，相对于实际图像区域宽度像素，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SWidhtLenP	//取样框宽像素（四边形的宽），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeightLenP//取样框高像素（四边形的高），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*int LDAngle	//发射偏转角度 单位度
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		 *
		 * C 的取样门参数
		*/
		int SetCSamplingFrameParam_Quadrangle(int Depth, double nFWidthP, double nFHeightP,
			double nIWidthP, double nIHeightP, double SX,
			double SY, double SWidthLen, double SHeightLen,
			int LDAngle);


		/*
		*获取PRF
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值。 GetImageHeightPixels方法获取
		*double SY			//取样框左上点坐标Y，相对于帧图像的位置，如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeightLenP //取样框高像素（四边形的高），如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*int LDAngle		//发射偏转角度
		*double* fPrfList	//PRF列表
		*int &fPrfListLen   // == 8
		*double& fTxFrequency//发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		 *
		*/
		int Get_C_PRFList_Quadrangle(int Depth, double nIHeightP, double SY, double SHeightLen,
			int LDAngle, double *fPrfList, int &fPrfListLen,
			double &fTxFrequency);

		/*
		*设置C 扇形形取样框参数
		*
		*设置完取样们参数后，调用Get_C_PRFList_Sector方法获取新得PRF列表，然后设置C 得PRFLevel参数
		*
		*帧图像左上角为原点 0,0
		*
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nFWidthP	//帧图像宽度像素，SDK中实际上传的值
		*double nFHeightP	//帧图像高度像素，SDK中实际上传的值
		*double nIWidthP	//实际图像区域宽度像素 SDK中实际上传的值 GetImageWidthPixels方法获取
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值 GetImageHeightPixels方法获取
		*double SCToPCCP	//扇形取样框的中心点到探头圆心的长度，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeight		//扇形取样框的高度，上下弧的差，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SectorAngle	//扇形取样框的夹角 单位度
		*double SectorCenterAngle//扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度

		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int
			SetCSamplingFrameParam_Sector(int Depth, double nFWidthP, double nFHeightP, double nIWidthP,
				double nIHeightP, double SCToPCCP, double SHeight,
				double SectorAngle, double SectorCenterAngle);

		/*
		*获取PRF
		*Param
		*int Depth			//当前显示的最大深度 mm
		*double nIHeightP	//实际图像区域高度像素 SDK中实际上传的值。  GetImageHeightPixels方法获取
		*double SCToPCCP	//扇形取样框的中心点到探头圆心的长度，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double SHeight		//扇形取样框的高度，上下弧的差，单位像素 如果显示有进行放大缩小操作，请根据比例计算出实际的值。
		*double* fPrfList	//PRF列表
		*int &fPrfListLen   // == 8
		*double& fTxFrequency//发射频率，当前prf列表对应的频率
		*return
		* -1 : fPrfListLen长度小于8
		* -2 : 没有获取到当前模式下CPRF参数
		* -3 : 根据当前参数计算的取样框深度超出参数范围
		* -4 : 超出CPRF参数范围
		* 0  : 成功
		 *
		*/
		int Get_C_PRFList_Sector(int Depth, double nIHeightP, double SCToPCCP, double SHeight,
			double *fPrfList, int &fPrfListLen, double &fTxFrequency);

		/*
		*设置 线阵 D_PW取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*int launchDeflectionAngle 发射偏转角度
		*int samplingVolume 取样门宽度 1-10mm
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_Line(float SX, float SY, float depth, float fIWidthP,
			float fIHeightP, int launchDeflectionAngle,
			int samplingVolume);

		/*
		 *获取线阵 PRF
		 *Param
		 *float SY					取样门中心点Y值
		 *float depth				当前深度,mm
		 *float fIHeightP			当前B显示像素
		 *int launchDeflectionAngle	发射偏转角度
		 *double* fPrfList			//PRF列表
		 *int &fPrfListLen			// == 8
		 *double& fTxFrequency//发射频率，当前prf列表对应的频率
		 *return
		 * -1 : fPrfListLen长度小于8
		 * -2 : 没有获取到当前模式下CPRF参数
		 * -3 : 根据当前参数计算的取样框深度超出参数范围
		 * -4 : 超出CPRF参数范围
		 * 0  : 成功
		 */
		int Get_D_PRFList_Line(float SY, float depth, float fIHeightP, int launchDeflectionAngle,
			double *fPrfList, int &fPrfListLen, double &fTxFrequency);


		/*
		*设置 凸阵 D_PW 取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*float SectorCenterAngle 扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度
		*int samplingVolume 取样门宽度 1-10mm
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_Convex(float SX, float SY, float depth, float fIWidthP,
			float fIHeightP, float SectorCenterAngle,
			int samplingVolume);

		/*
		 *获取 凸阵 PRF
		 *Param
		 *float SX					取样门中心点Y值
		 *float SY					取样门中心点Y值
		 *float depth				当前深度mm
		 *float fIWidthP				当前B显示宽度像素
		 *float fIHeightP			当前B显示像素
		 *double* fPrfList			//PRF列表
		 *int &fPrfListLen			// == 8
		 *double& fTxFrequency//发射频率，当前prf列表对应的频率
		 *return
		 * -1 : fPrfListLen长度小于8
		 * -2 : 没有获取到当前模式下CPRF参数
		 * -3 : 根据当前参数计算的取样框深度超出参数范围
		 * -4 : 超出CPRF参数范围
		 * 0  : 成功
		 */
		int Get_D_PRFList_Convex(float SX, float SY, float depth, float fIWidthP, float fIHeightP,
			double *fPrfList, int &fPrfListLen, double &fTxFrequency);


		/*
		*设置 相控阵 D_PW 取样门参数
		*
		*Param
		*float SX 取样门中心点Y值
		*float SY 取样门中心点Y值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*float SectorCenterAngle 扇形取样框的中心点到探头圆心的连线与X轴正方向的夹角，顺时针方向，正直 单位度
		*int samplingVolume 取样门宽度 1-10mm
		*
		*return
		* -2 : 发送命令失败 请检查USB是否连接正常
		* -3 : 多普勒参数获取失败
		* 0  : 成功
		*/
		int SetD_PWSamplingGateParam_PA(float SX, float SY, float depth, float fIWidthP,
			float fIHeightP, float SectorCenterAngle,
			int samplingVolume);

		/*
		 *获取 相控阵 PRF
		 *Param
		 *float SX					取样门中心点Y值
		 *float SY					取样门中心点Y值
		 *float depth				当前深度mm
		 *float fIWidthP				当前B显示宽度像素
		 *float fIHeightP			当前B显示像素
		 *double* fPrfList			//PRF列表
		 *int &fPrfListLen			// == 8
		 *double& fTxFrequency//发射频率，当前prf列表对应的频率
		 *return
		 * -1 : fPrfListLen长度小于8
		 * -2 : 没有获取到当前模式下CPRF参数
		 * -3 : 根据当前参数计算的取样框深度超出参数范围
		 * -4 : 超出CPRF参数范围
		 * 0  : 成功
		 */
		int Get_D_PRFList_PA(float SX, float SY, float depth, float fIWidthP, float fIHeightP,
			double *fPrfList, int &fPrfListLen, double &fTxFrequency);


		//M
		/*
		*设置M 取样线参数 线阵
		*
		*Param
		*float SLX 取样线坐标X值
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*
		*return
		* 0  : 成功
		*/
		int SetSamplingLineParam_Line(float SLX, float depth, float fIWidthP, float fIHeightP);

		/*
		*设置M 取样线参数 凸阵
		*
		*Param
		*float SectorCenterAngle //取样线的与X轴正方向的夹角，顺时针方向，正直 单位度
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*
		*return
		* 0  : 成功
		*/
		int SetSamplingLineParam_Convex(float SectorCenterAngle, float depth, float fIWidthP,
			float fIHeightP);

		/*
		*设置M 取样线参数 相控阵
		*
		*Param
		*float SectorCenterAngle //取样线的与X轴正方向的夹角，顺时针方向，正直 单位度
		*float depth 当前深度mm
		*float fIWidthP 当前B显示宽度像素
		*float fIHeightP 当前B显示像素
		*
		*return
		* 0  : 成功
		*/
		int SetSamplingLineParam_Phased(float SectorCenterAngle, float depth, float fIWidthP,
			float fIHeightP);


	private:

		bool m_bIsEngineRuning;
		DataSourceInterface &m_dsInterface;
	};

}
