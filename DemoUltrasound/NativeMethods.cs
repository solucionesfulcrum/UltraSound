using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DemoUltrasound
{

    public static class NativeMethods
    {
        
        [DllImport("ColorImageEngineServer.dll", EntryPoint = "?SetCheckModeParamPath_And_ScanMode@ColorImageEngineServer@CIES@@QEAAHPEADW4ScanModeEnum@ScanMode@2@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetCheckModeParamPath_And_ScanMode(string paramPath, int scanMode);
        
        [DllImport("ColorImageEngineServer.dll", EntryPoint = "?StartImageEngine@ColorImageEngineServer@CIES@@QEAAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern int StartImageEngine();
        // Importar SetFrameDataListener desde la DLL
        //[DllImport("ColorImageEngineServer.dll", EntryPoint = "?SetFrameDataListener@ColorImageEngineServer@CIES@@QAEXPAVFrameDataListener@2@@Z", CallingConvention = CallingConvention.StdCall)]
        [DllImport("ColorImageEngineServer.dll", EntryPoint = "?SetFrameDataListener@ColorImageEngineServer@CIES@@QEAAXPEAVFrameDataListener@2@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFrameDataListener(IntPtr listener);
    }
}
