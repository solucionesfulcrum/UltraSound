//File：ShowFrame.h
//Purpose：定义数据存储接口。
#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif


namespace CIES {

	/**
	* B/C 图像信息
	*/
	struct COLORIMAGEENGINESERVER_API ImageDataInfo
	{
		ImageDataInfo() {
			m_nFrameNumber = 0;
			m_nDepth = 0;


			m_nFrameWidthPixels = 0;
			m_nFrameHeightPixels = 0;
			m_nImageWidthPixels = 0;
			m_nImageHeightPixels = 0;

			m_nHour = 0;
			m_nMinute = 0;
			m_nSecond = 0;
			m_nMillisecond = 0;

			m_fFrameRate = 0;
			m_fResolution = 0;
			C_EmitStartLineNo = 0;
			C_EmitStopLineNo = 0;
		}

		int m_nFrameNumber;				//帧号
		int m_nDepth;					//当前深度

		int m_nFrameWidthPixels;		//帧横向分辨率
		int m_nFrameHeightPixels;		//帧纵向分辨率
		int m_nImageWidthPixels;		//图像横向分辨率
		int m_nImageHeightPixels;		//图像纵向分辨率

		int m_nHour;					//时
		int m_nMinute;					//分
		int m_nSecond;					//秒
		int m_nMillisecond;             //毫秒

		float m_fFrameRate;				//帧率
		float m_fResolution;			//分辨率 单位 像素/毫米。

		int C_EmitStartLineNo;			//C模式 发射起始线 
		int C_EmitStopLineNo;			//C模式 发射结束线

	};


	struct COLORIMAGEENGINESERVER_API EnvelopeInfo
	{
		EnvelopeInfo() {
			bHadData = false;
			nEnvelopeMax = 0;
			nEnvelopeMean = 0;

		}

		bool bHadData;//是否有数据
		short nEnvelopeMax; //最大包络
		short nEnvelopeMean;//平均包络
	};

	struct COLORIMAGEENGINESERVER_API D_PWDataInfo

	{
		D_PWDataInfo() {
			m_nDateNum = 0;
			m_AutoMeasureAngle = -999;
			m_BloodRadius_T = -999;
			m_BloodRadius_B = -999;

		}

		EnvelopeInfo envelopeInfo;

		int m_nDateNum;                        //数据编号

		float m_AutoMeasureAngle;				//默认值是-999不生效的值 自动测量角度 -90 ~ 90
		float m_BloodRadius_T;					//默认值是-999不生效的值 像素
		float m_BloodRadius_B;					//默认值是-999不生效的值 像素
	};

	struct COLORIMAGEENGINESERVER_API MDataInfo
	{
		MDataInfo() {
			m_nFrameNumber = 0;
			m_nImageHeightPixels = 0;
			m_nMLineNum = 0;
			m_bClearScreen = false;
		}

		int m_nFrameNumber;            //帧号
		int m_nImageHeightPixels;    //图像纵向分辨率
		int m_nMLineNum;            //M谱线数量
		bool m_bClearScreen;        //清屏

	};


	class Cache_Ptr;

	/*
	* 显示帧
	*/
	class COLORIMAGEENGINESERVER_API ShowFrame
	{
	public:
		ShowFrame();

		ShowFrame(const ShowFrame& s);
		~ShowFrame();

		ShowFrame& operator=(const ShowFrame& s);

		void SetFrameData(unsigned char* frameData, int frameDataLen);

		const unsigned char* GetFrameData(int& frameDataLen);

		int SetImageDataInfo(ImageDataInfo* imageDataInfo);

		ImageDataInfo* GetImageDataInfo();

		int SetD_PWDataInfo(D_PWDataInfo* d_PWDataInfo);

		D_PWDataInfo* GetD_PWDataInfo();

		int SetMDataInfo(MDataInfo* mDataInfo);

		MDataInfo* GetMDataInfo();
	private:
		Cache_Ptr* rp;

	};

	/*
	* 超声图像数据
	*/
	class COLORIMAGEENGINESERVER_API UImagingData
	{
	public:
		UImagingData();

		~UImagingData();

		UImagingData& operator=(const UImagingData& s);

		ShowFrame m_B_Data; //B 数据
		ShowFrame m_C_Data; //C 数据 包含B和C的显示数据

		ShowFrame m_D_PW_Data; //D PW 数据 包含D_pw数据
		ShowFrame m_M_Data;        //M 数据

		bool m_bBHadData; //包含B数据
		bool m_bCHadData; //包含C数据
		bool m_bDPWHadData; //包含DPW数据
		bool m_bMHadData; //包含M数据
	};
}