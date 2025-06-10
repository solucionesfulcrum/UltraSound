//File：DataSource.h
//Purpose：定义数据源模块接口的实现。
#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif


#include "DataSourceInterface.h"

namespace CIES {

	class Cypress_Windows_USB;

	class SimuImage;

	class COLORIMAGEENGINESERVER_API DataSource : public DataSourceInterface
	{
	public:
		DataSource();

		~DataSource();

		int OpenDevice(char *DevicePath); 
		int CloseDevice();
		bool IsDeviceOpen();

		int WriteCmdMsgSync(unsigned char *pCmdMsg, int nCmdMsgLen);
		int ReadDataMsgAsync(unsigned char *pDataMsg, int &nDataMsgLen, int &index); 
		int ReadCmdMsgSync(unsigned char *pCmdMsg, int &nCmdMsgLen);
		int ReadInterruptMsgSync(unsigned char *pInterruptMsg, int &nMsgLen);
		int WriteFeedigDogSync();

		int SetReadDataBufferSize(int nSize);

		int GetUSDeviceInfo(void *usHareWareInfoArray, int &nArrayLen);

		//0： 设置成功
		//-1：没有该设备
		//-2： 打开usb设备识别
		int PowerOn(char *DevicePath);

		//0： 设置成功
		//-1：没有该设备
		//-2： 打开usb设备识别
		int PowerOff(char *DevicePath);

		int GetPID();

		int GetUSDeviceInfoByDevicePath(void *usHareWareInfo, char *DevicePath);
	};

}
