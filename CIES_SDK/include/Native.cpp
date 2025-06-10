#include "Native.h"
#include <assert.h>
namespace CIES {

	Native::Native()
	{
		ms_callback_HW = nullptr;
		ms_callback_HWS = nullptr;
	}

	void Native::RegisterCallback(EventCallback_HW callback)
	{
		ms_callback_HW = callback;
	}

	void Native::HardWareMsgUpdated(int msgType, int nValue)
	{
		Invoke_HW(msgType, nValue);
	}

	void Native::Invoke_HW(int type, int value)
	{
		if (ms_callback_HW != nullptr)
			ms_callback_HW(type, value);
	}
	void Native::RegisterCallback(EventCallback_HWS callback)
	{
		ms_callback_HWS = callback;
	}

	void Native::HardWareMsgSUpdated(int msgType, int* nValue, int nValueLen)
	{
		Invoke_HWS(msgType, nValue, nValueLen);
	}

	void Native::Invoke_HWS(int type, int* value, int valueLen)
	{
		if (ms_callback_HWS != nullptr)
			ms_callback_HWS(type, value, valueLen);
	}
}