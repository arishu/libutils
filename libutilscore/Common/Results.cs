
#region Using directives
using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#endregion

namespace libutilscore.Common
{
    internal static class Results
    {
        #region Private Properties

        /// <summary>
        /// Lock Object
        /// </summary>
        private static Object LockObject = new object();

        /// <summary>
        /// Hashtable holding execution result
        /// </summary>
        private static Hashtable execResults = new Hashtable();

        #endregion

        internal enum RTYPE
        {
            [Description("")]
            SUCCESS = 0x6000,   // execute successfully

            [Description("known exception")]
            E_UNKNOWN,          // 未知异常

            /****************************************************************************************
             * FTP Exceptions
             ****************************************************************************************/
            [Description("failed to create ftp request")]
            E_CREATE_REQUEST,

            [Description("not a valid uri")] // requestUriString 中指定的 URI 不是有效的 URI
            E_URI_FORMAT,

            [Description("invalid operation")]
            E_INVLID_OPERATION, // 对于一个已在进行的请求为此属性指定了一个新值

            [Description("not enough memory")]
            E_OUTOF_MEMORY,

            [Description("argument is empty ")]
            E_ARG,         // argument is ""

            [Description("argument is nil")]
            E_ARG_NULL,             // argument is null

            [Description("file not found")]
            E_FILENOTFOUND,         // file not found

            [Description("directory not found")]
            E_DIRECTORYNOTFOUND,    // 指定的路径无效，比如在未映射的驱动器上

            [Description("io exception")]
            E_IOEXCEPTION           // 包括不正确或无效的文件名、目录名或卷标的语法
        }


        #region Public Methods

        public static Tuple<bool, string> GetExecResult(object operationId)
        {
            Tuple<bool, string> result = Tuple.Create(false, "No Result");
            lock (LockObject)
            {
                if (execResults.Count > 0)
                {
                    foreach (DictionaryEntry entry in execResults)
                    {
                        if (entry.Key == operationId)
                        {
                            Tuple<bool, string> ret = (Tuple<bool, string>)entry.Value;
                            result = Tuple.Create(ret.Item1, ret.Item2);
                        }
                    }
                }
            }
            return result;
        }

        public static void AddExecResult(object operationId, Tuple<bool, string> result)
        {
            lock (LockObject)
            {
                execResults.Add(operationId, result);
            }
        }

        /* get results */
        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static Tuple<bool, string> GetResultsTuple(bool issuccess) 
        {
            return GetResultsTuple(true, RTYPE.SUCCESS);
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static Tuple<bool, string> GetResultsTuple(bool issuccess, Exception e)
        {
            //StackTrace trace = new StackTrace(e, true);
            //StackFrame frame = trace.GetFrame(trace.FrameCount - 1);
            //StringBuilder where = new StringBuilder();
            //where.Append("  FileName:" + frame.GetFileName());
            //where.Append(", MethodName:" + frame.GetMethod().Name);
            //where.Append(", LineNo:" + frame.GetFileLineNumber().ToString());
            //where.Append(", ColumnNo:" + frame.GetFileColumnNumber().ToString());
            return Tuple.Create(false, e.ToString());
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static Tuple<bool, string> GetResultsTuple(bool issuccess, RTYPE rtype) 
        {
            //string fileName = new StackFrame(1, true).GetMethod().Name;
            string msg = EnumHelper.GetEnumDescription(rtype);
            return Tuple.Create(issuccess, msg);
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetResultString(bool issuccess)
        {
            return GetResultString(true, RTYPE.SUCCESS);
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetResultString(bool issuccess, RTYPE rtype)
        {
            string fileName = new StackFrame(1, true).GetMethod().Name;
            string msg = EnumHelper.GetEnumDescription(rtype);
            if (issuccess)
                return "true";
            else
                return "failed: " + msg + ", in file: " + fileName;
        }

        #endregion
    }
}
