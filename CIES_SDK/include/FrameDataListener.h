#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif

namespace CIES {

	class COLORIMAGEENGINESERVER_API FrameDataListener
	{
	public:
		/**
		* 硬件消息上传
		* @param msgType 消息类型 ： 2：探头ID消息
		* @param nValue 消息值 ：探头ID、 0x5555（探头脱落）、0xAAAA(一线器件损坏或读取一线器件失败)
		*/
		virtual void HardWareMsgUpdated(int msgType, int nValue) = 0;
		virtual void HardWareMsgSUpdated(int msgType, int* nValue, int nValueLen) = 0;


	};
}