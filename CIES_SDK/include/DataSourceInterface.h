//File：DataSourceInterface.h
//Purpose：定义数据源接口
#pragma once

#ifdef COLORIMAGEENGINESERVER_EXPORTS
#define COLORIMAGEENGINESERVER_API __declspec(dllexport)
#else
#define COLORIMAGEENGINESERVER_API __declspec(dllimport)
#endif

namespace CIES {

	class COLORIMAGEENGINESERVER_API DataSourceInterface
	{
	public:


		/**
		 * 打开硬件设备
		 * @param DevicePath  设置路径。 通过 USDeviceInfo.DevicPath 获取。
		 * @return
		 */
		virtual int OpenDevice(char* DevicePath) = 0;

		/**
		* 关闭硬件设备
		* @return  : ;
		*/
		virtual int CloseDevice() = 0;
		/**
		* 获取设备状态.
		* @return : 
		*/
		virtual bool IsDeviceOpen() = 0;

		/**
		* 向硬件写入命令消息.
		* @param pCmdMsg : 
		* @param nCmdMsgLen : 
		* @return :
		*/
		virtual int WriteCmdMsgSync(unsigned char* pCmdMsg, int nCmdMsgLen) = 0;

		/**
		* 从硬件读取消息
		* @param pDataMsg : 
		* @param nDataMsgLen : 
		* @return : 
		*/
		virtual int ReadDataMsgAsync(unsigned char* pDataMsg, int& nDataMsgLen, int& index) = 0;


		/**
		 * 读取硬件命令消息
		 * @param pCmdMsg :
		 * @param nCmdMsgLen : 
		 * @return  : 
		 */
		virtual int ReadCmdMsgSync(unsigned char* pCmdMsg, int& nCmdMsgLen) = 0;

		/**
		 * 读取硬件中断消息
		 * @param pInterruptMsg :
		 * @param nMsgLen : 
		 *
		 * @return :
		 */
		virtual int ReadInterruptMsgSync(unsigned char* pInterruptMsg, int& nMsgLen) = 0;

		/**
		 * 写喂狗消息
		 * @return : 
		 */
		virtual int WriteFeedigDogSync() = 0;


		/**
		* 设置读取缓存大小
		* @param nSize : 
		* @return : 
		*/
		virtual int SetReadDataBufferSize(int nSize) = 0;


		/**
		 * 获取超声设备信息 最多获取5个设备信息
		 * @param usHareWareInfoArray
		 * @param nArrayLen
		 * @return.
		 */
		virtual int GetUSDeviceInfo(void* usHareWareInfoArray, int& nArrayLen) = 0;
		
		/**
		 * 硬件退出低功耗，发送命令后延迟500ms后，需要重新调用后台接口设置参数。
		 * @param DevicePath 
		 * @return : 
		 */
		virtual int PowerOn(char* DevicePath) = 0;
		/**
		 * 硬件进入低功耗
		 * @param DevicePath 
		 * @return  : 
		 */
		virtual int PowerOff(char* DevicePath) = 0;
		/**
		 * 获取当前连接设备的PID
		 * @return
		 */
		virtual int GetPID() = 0;
		/**
		 * 获取超声设备信息，通过DevicePath。
		 * @param usHareWareInfo
		 * @param DevicePath
		 * @return
		 */
		virtual int GetUSDeviceInfoByDevicePath(void* usHareWareInfo, char* DevicePath) = 0;


	public:
		const unsigned short USB_VID = 0x04B4;					//USB VID 
		const unsigned short USB_PID1 = 0x00F0;					//USB PID  平板超声PID
		const unsigned short USB_PID2 = 0x4611;					//USB PID   USB探头超声PID
		const char* CYUSB_GUID = "AE18AA60-7F6A-11d4-97DD-00010229B959"; //USB GUID


	};
}