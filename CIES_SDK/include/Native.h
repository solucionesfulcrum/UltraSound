#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif

#include "FrameDataListener.h"

namespace CIES {
	typedef void(__stdcall* EventCallback_HW)(int, int);
	typedef void(__stdcall* EventCallback_HWS)(int msgType, int* nValues, int nValueLen);
	class COLORIMAGEENGINESERVER_API Native : public FrameDataListener
	{
	public:

		Native();

		/**
		* 注册回调函数
		*/
		void RegisterCallback(EventCallback_HW callback);

		/**
		* 调用注册的回调函数  
		*/
		void Invoke_HW(int type, int value);
		/**
		* 硬件消息上传
		* @param msgType 消息类型 ： 2：探头ID消息 
		* @param nValue 消息值 ：探头ID、 0x5555（探头脱落）、0xAAAA(一线器件损坏或读取一线器件失败)
		*/
		void HardWareMsgUpdated(int msgType, int nValue);

		/// 注册回调函数  
		void RegisterCallback(EventCallback_HWS callback);

		/// 调用注册的回调函数  
		void Invoke_HWS(int type, int* value, int valueLen);

		void HardWareMsgSUpdated(int msgType, int* nValue, int nValueLen);
	private:
		EventCallback_HW ms_callback_HW;
		EventCallback_HWS ms_callback_HWS;
	};


}