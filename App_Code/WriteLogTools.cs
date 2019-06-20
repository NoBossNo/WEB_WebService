using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;


namespace FSMesService
{
    public class WriteLogTools { 

        private static string m_LogDirPath =  @"D:\Logs";
        private static string m_LogFile = string.Empty;
        private static Worker m_Worker = new Worker();
        private static object m_Lock = new object();

        static WriteLogTools() {
            if (Directory.Exists(m_LogDirPath) == false) {
                Directory.CreateDirectory(m_LogDirPath);
            }
        }

        #region 写出错信息
        public static void WriteMessage(string msg, Exception ex) {
            lock (m_Lock) {
                m_Worker.WriteLogo(msg, ex);
            }
        }

        public static void WriteMessage(Exception ex) {
            lock (m_Lock) {
                string functionName = GetFunctionName();
                m_Worker.WriteLogo(functionName + "出错。", ex);
            }
        }

        public static void WriteMessage(string msg) {
            lock (m_Lock) {
                m_Worker.WriteMessage(msg);
            }
        }
        #endregion

        #region WriteDebug
        [Conditional("DEBUG")]
        public static void WriteDebug(Exception ex) {
            lock (m_Lock) {
                string functionName = GetFunctionName();
                m_Worker.WriteLogo(functionName + "出错。", ex);
            }
        }

        [Conditional("DEBUG")]
        public static void WriteDebug(string msg, Exception ex) {
            lock (m_Lock) {
                m_Worker.WriteLogo(msg, ex);
            }
        }

        [Conditional("DEBUG")]
        public static void WriteDebug(string msg) {
            lock (m_Lock) {
                m_Worker.WriteMessage(msg);
            }
        }

        public static string GetFunctionName() {
            try {
                StackTrace trace = new StackTrace(2);
                if (trace.FrameCount <= 1) {
                    return string.Empty;
                }
                MethodBase method = trace.GetFrame(0).GetMethod();
                return string.Format("{0}.{1}", method.ReflectedType.FullName, method.Name);
            } catch {
            }
            return string.Empty;
        }
        #endregion

        
        private class Worker {
            int fileIndex = 0;
            DateTime logTime = DateTime.Now;
            string exFileName = ".log";
            string baseFileName = m_LogDirPath + @"\" + "Log." + DateTime.Now.ToString("yyyyMMdd");

            public void WriteLogo(string msg, Exception ex) {
                FileIndexControl();
                using (StreamWriter sw = File.AppendText(m_LogFile)) {
                    sw.Write(DateTime.Now);
                    sw.Write("     ");
                    sw.Write(msg);
                    sw.Write("     ");
                    sw.Write(ex.ToString());
                    sw.WriteLine();
                }
            }

            public void WriteMessage(string msg) {
                FileIndexControl();
                using (StreamWriter sw = File.AppendText(m_LogFile)) {
                    sw.Write(DateTime.Now);
                    sw.Write("     ");
                    sw.WriteLine(msg);
                }

            }

            private void FileIndexControl() {
                if (logTime.Date != DateTime.Now.Date) {//另外一天
                    logTime = DateTime.Now;
                    fileIndex = 0;
                    baseFileName = m_LogDirPath + @"\" + Process.GetCurrentProcess().ProcessName + "." + DateTime.Now.ToString("yyyyMMdd");
                    m_LogFile = baseFileName + "_0" + exFileName;
                } else if (m_LogFile != string.Empty && File.Exists(m_LogFile))//
                {
                    FileInfo fi = new FileInfo(m_LogFile);
                    if (fi.Length > 1024000) {
                        fileIndex++;
                        m_LogFile = baseFileName + "_" + fileIndex.ToString() + exFileName;
                    }
                } else if (m_LogFile == string.Empty || m_LogFile.Length == 0)//logfile为空
                {
                    m_LogFile = baseFileName + "_" + fileIndex.ToString() + exFileName;
                }
            }
        }//worker
    }

}
