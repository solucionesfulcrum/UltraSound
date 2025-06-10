using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DemoUltrasound
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //启动项重写
        protected override void OnStartup(StartupEventArgs e)
        {
          
            DispatcherUnhandledException += OnDispatcherUnhandledException; //关联异常信息
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                base.OnStartup(e);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {

            string info = e.Exception.Source + "->" + e.Exception.Message + "\n" + e.Exception.StackTrace;
        
            MessageBox.Show(info, "Oh my God!---DebugMode"); //使用系统自带消息窗体
      
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var ex = e.ExceptionObject as Exception;

            string info = ex.Source + "->" + ex.Message + "\n" + ex.StackTrace;
            // #if DEBUG
            MessageBox.Show(info, "---iTrason Exception Msg--- SWVer:" + ver.ToString()); //使用系统自带消息窗体
            //#endif
        }
    }
}
