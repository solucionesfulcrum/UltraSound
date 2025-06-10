using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DemoUltrasound
{

    /// <summary>
    /// 即插即用设备信息结构
    /// </summary>
    public struct PnPEntityInfo
    {
        public String PNPDeviceID;      // 设备ID
        public String Name;             // 设备名称
        public String Description;      // 设备描述
        public String Service;          // 服务
        public String Status;           // 设备状态
        public UInt16 VendorID;         // 供应商标识
        public UInt16 ProductID;        // 产品编号 
        public Guid ClassGuid;          // 设备安装类GUID
    }

    public class CyUSBMonitor
    {
        /// <summary>
        /// 根据VID和PID及设备安装类GUID定位即插即用设备实体
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// HID：{745a17a0-74d3-11d0-b6fe-00a0c90f57da}
        /// Imaging Device：{6bdd1fc6-810f-11d0-bec7-08002be2092f}
        /// Keyboard：{4d36e96b-e325-11ce-bfc1-08002be10318} 
        /// Mouse：{4d36e96f-e325-11ce-bfc1-08002be10318}
        /// Network Adapter：{4d36e972-e325-11ce-bfc1-08002be10318}
        /// USB：{36fc9e60-c465-11cf-8056-444553540000}
        /// </remarks>
        public PnPEntityInfo[] WhoPnPEntity(UInt16 VendorID, UInt16 ProductID, Guid ClassGuid)
        {
            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String VIDPID;
            if (VendorID == UInt16.MinValue)
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID_%&PID_%'";
                else
                    VIDPID = "'%VID_%&PID_" + ProductID.ToString("X4") + "%'";
            }
            else
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID_" + VendorID.ToString("X4") + "&PID_%'";
                else
                    VIDPID = "'%VID_" + VendorID.ToString("X4") + "&PID_" + ProductID.ToString("X4") + "%'";
            }

            String QueryString;
            if (ClassGuid == Guid.Empty)
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID;
            else
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID + " AND ClassGuid='" + ClassGuid.ToString("B") + "'";

            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String PNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(PNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        PnPEntityInfo Element;

                        Element.PNPDeviceID = PNPDeviceID;                      // 设备ID
                        Element.Name = Entity["Name"] as String;                // 设备名称
                        Element.Description = Entity["Description"] as String;  // 设备描述
                        Element.Service = Entity["Service"] as String;          // 服务
                        Element.Status = Entity["Status"] as String;            // 设备状态
                        Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                        PnPEntities.Add(Element);
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }



        //监视系统队列

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        public const int WM_DEVICECHANGE = 0x219;       //USB设备插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;        //一个设备或媒体已被插入一块，现在可用。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;  // device interface class



        public Guid CYUSBDRV_GUID = new Guid(0xae18aa60, 0x7f6a, 0x11d4, 0x97, 0xdd, 0x0, 0x1, 0x2, 0x29, 0xb9, 0x59);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, ref DEV_BROADCAST_DEVICEINTERFACE NotificationFilter, UInt32 Flags);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        };

        public void RegisterDeviceToWin(IntPtr hwnd)
        {
            DEV_BROADCAST_DEVICEINTERFACE NotificationFilter = new DEV_BROADCAST_DEVICEINTERFACE();

            NotificationFilter.dbcc_size = Marshal.SizeOf(NotificationFilter);
            NotificationFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            NotificationFilter.dbcc_classguid = CYUSBDRV_GUID;	//判断条件

            RegisterDeviceNotification(hwnd, ref NotificationFilter, DEVICE_NOTIFY_WINDOW_HANDLE);

        }

        private int[] GetDeviceID(string dbcc_name)
        {
            int[] deviceID = new int[2];
            string[] Parts = dbcc_name.Split('#');
            if (Parts.Length >= 3)
            {
                string DevType = Parts[0].Substring(Parts[0].IndexOf(@"?\") + 2);

                string DeviceInstanceId = Parts[1];//"VID_04B4&PID_00F3"
                string[] idStr = DeviceInstanceId.Split('&');

                deviceID[0] = Convert.ToInt32(idStr[0].Substring(4), 16);
                deviceID[1] = Convert.ToInt32(idStr[1].Substring(4), 16);


                string DeviceUniqueID = Parts[2];
                Guid device_Guid = Guid.Parse(Parts[3]);

            }
            return deviceID;
        }

        public virtual IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            DEV_BROADCAST_HDR stHDR;
            DEV_BROADCAST_DEVICEINTERFACE deviceInfo;

            if (msg == WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case DBT_DEVICEARRIVAL:

                        stHDR = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        switch (stHDR.dbch_devicetype)
                        {
                            case DBT_DEVTYP_DEVICEINTERFACE:
                                deviceInfo = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                                if (deviceInfo.dbcc_classguid.Equals(CYUSBDRV_GUID)) //通过获取系统信息，判断是否和设备的GUID相等，来确定接入的USB设备是否是自己的设备
                                {

                                    Console.WriteLine("Usb接入 " + deviceInfo.dbcc_name);
                                    if (USBConnectStateChangeEvent != null)
                                    {
                                        int[] idArray = GetDeviceID(deviceInfo.dbcc_name);

                                        if (idArray[0] == 0x04B4 && (idArray[1] == 0x00F0 || idArray[1] == 0x4611))
                                        {
                                            USBConnectStateChangeEvent(true, idArray[0], idArray[1]);
                                        }
                                    }

                                }
                                break;
                            default:
                                break;
                        }

                        break;
                    case DBT_DEVICEREMOVECOMPLETE:

                        stHDR = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        switch (stHDR.dbch_devicetype)
                        {
                            case DBT_DEVTYP_DEVICEINTERFACE:
                                deviceInfo = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                                if (deviceInfo.dbcc_classguid.Equals(CYUSBDRV_GUID)) //通过获取系统信息，判断是否和设备的GUID相等，来确定接入的USB设备是否是自己的设备
                                {
                                    int[] idArray = GetDeviceID(deviceInfo.dbcc_name);
                                    Console.WriteLine("Usb断开" + deviceInfo.dbcc_name);
                                    if (USBConnectStateChangeEvent != null)
                                    {
                                        USBConnectStateChangeEvent(false, idArray[0], idArray[1]);
                                    }

                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            return IntPtr.Zero;
        }


        //扩大和缩小
        public delegate void USBConnectStateChangeHandler(bool isConnect, int VID, int PID);
        public event USBConnectStateChangeHandler USBConnectStateChangeEvent;
    }
}
