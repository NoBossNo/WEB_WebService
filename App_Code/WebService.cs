using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.OracleClient;
using FSMesService;
using System.Collections;
using MesService;
using System.IO;
using System.Net;
using System.Text;

/// <summary>
///WebService 的摘要说明
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
//若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。 
// [System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService {
    private DBOracleOperater db = DBOracleOperater.GetInstance();
    private MesService.Tools mes_tools = new MesService.Tools();
    //private Util util = new Util();
    string str = null;

    public WebService () {

        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    [WebMethod]
    //world测试用
    public String HelloWorld(int a, int b)
    {
        int c = a + b;
        String s = mes_tools.ShowMessage().ToString()+c;
        return s;
    }

    [WebMethod]
    //这个函数
    public bool CheckUserAndResourcePassed(string _iUserCode, string _iPassWord, out string oHandle, out string oErrMessage)
    //传入用户（_iUserCode）和密码（_iPassWord），返回特征码（oHandle）和错误信息（oErrMessage）
    {
        str = null;
        oHandle = "";//返回特征码
        oErrMessage = "";//返回错误信息
        try
        {
            if (string.IsNullOrEmpty(_iUserCode))//检查用户名不能为空值
            {
                oErrMessage = "用户名不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(_iPassWord))//检查密码不能为空值
            {
                oErrMessage = "输入的密码不能为空";
                return false;
            }
            Hashtable hs = new Hashtable();//创建一个哈希表向存储过程传入值
            hs.Add("T_User", _iUserCode.ToUpper());//添加哈希表用户一个键值
            hs.Add("T_PassWord", _iPassWord.ToUpper());//添加哈希表密码一个键值
            string[] rs = db.CallPrc("PRC_TXUSER_CheckUser", new string[] { "T_oHandle", "T_oErrMSG" }, hs, out str);
            //调用存储过程传入名称，传入数组获取存储过程的返回值，传入哈希表存储过程传入值

            if (rs[1].ToString().Substring(0, 1).Equals("0"))//判断存储过程中返回是否正确，0为错误信息
            {
                oErrMessage = rs[1].ToString();
                return false;
            }
            else
            {
                oHandle = rs[0].ToString();
                oErrMessage = rs[1].ToString();
            }
        }
        catch (Exception ex)
        {
            oErrMessage = oErrMessage + "CheckUserAndResourcePassed错误信息：" + str + ex.Message;
            return false;
        }

        return true;
    }

    [WebMethod]
    public bool GetBillList(string iHandle, string IUserCode, int iDataType, out List<string> getlist, out string oErrMessage)
    //输入特征码（iHandle）,用户（IUserCode）和查询标识（iDataType），返回出库单表数据（oTable），和错误信息（oErrMessage）
    {
        //oilist = null;
        oErrMessage = null;
        DataTable uTable;
        getlist = new List<string>();
        string sqlDataTable = "";
        try
        {
            if (string.IsNullOrEmpty(iHandle))
            {
                oErrMessage = "特征码不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(IUserCode))
            {
                oErrMessage = "用户不能为空";
                return false;
            }
            //if (iDataType.Equals(null))
            //{
            //    oErrMessage = "查询标识不能为空123456";
            //    return false;
            //}

            Hashtable hs = new Hashtable();
            hs.Add("T_user_agreement", iHandle.ToUpper());
            hs.Add("T_User", IUserCode.ToUpper());
            string[] rs = db.CallPrc("PRC_TXOUT_ITEM", new string[] { "T_oErrMSG" }, hs, out str);
            if (rs[0].ToString().Substring(0, 1).Equals("0"))
            {
                oErrMessage = "NG错误" + rs[0].ToString();
                return false;
            }
            else
            {
                oErrMessage = rs[0].ToString();
            }

            if (iDataType == 1 || iDataType == 0)
            {
                sqlDataTable = "select a.item_out_no as 出库单,b.dep004 as 机型,a.task_no as 工单,f_getoutqtybyid_sn(a.id) as 出库单数量,b.task_qty as 工单数量,a.create_by as 创单人,a.create_date as 创单日期,a.kunnr as 客户编码,a.kunnr_name as 客户名称,a.update_status as 上传标识 from MES_INV_ITEM_OUTS a " +
                                " left join mes_dep_task_info b" +
                                " on a.task_no=b.task_no" +
                                " where a.status=1 and a.bill_type_id=83 and b.isplan=0 and b.verify=1" +
                                " and b.customer_no=(select kunnr from mes_tx_user where sys_user=" + "'" + IUserCode + "')" + " and a.update_status=" + iDataType;
            }
            else
            {
                sqlDataTable = "select a.item_out_no as 出库单,b.dep004 as 机型,a.task_no as 工单,f_getoutqtybyid_sn(a.id) as 出库单数量,b.task_qty as 工单数量,a.create_by as 创单人,a.create_date as 创单日期,a.kunnr as 客户编码,a.kunnr_name as 客户名称,a.update_status as 上传标识 from MES_INV_ITEM_OUTS a " +
                                " left join mes_dep_task_info b" +
                                " on a.task_no=b.task_no" +
                                " where a.status=1 and a.bill_type_id=83 and b.isplan=0 and b.verify=1" +
                                " and b.customer_no=(select kunnr from mes_tx_user where sys_user=" + "'" + IUserCode + "')";
            }
            uTable = db.SelectDataBase(sqlDataTable);
            for (int i = 0; i < uTable.Rows.Count; i++)
            {
                for (int j = 0; j < uTable.Columns.Count; j++)
                {
                    getlist.Add(uTable.Rows[i][uTable.Columns[j].ColumnName].ToString());
                }
            }
            //getlist = db.ConvertTo<string>(uTable);
        }
        catch (Exception ex)
        {
            oErrMessage += "错误信息：GetBillList" + ex.Message;
            return false;
        }

        return true;
    }

    [WebMethod]
    public bool GetDataSet(string iHandle, string IUserCode, string iBillNo, out List<string> getlistdata, out string oErrMessage)
    //输入特征码（iHandle），用户（IUserCode），出库单（iBillNo），返回出货表数据（oTable），返回错误信息（oErrMessage）
    {
        DataTable oTable = null;
        getlistdata = new List<string>();
        oErrMessage = null;
        string T_out_sqlconn = null;
        string sqlDataTable = "";
        try
        {
            if (string.IsNullOrEmpty(iHandle))
            {
                oErrMessage = "特征码不允许为空";
                return false;
            }
            if (string.IsNullOrEmpty(IUserCode))
            {
                oErrMessage = "密码不允许为空";
                return false;
            }
            if (string.IsNullOrEmpty(iBillNo))
            {
                oErrMessage = "出库单不允许为空";
                return false;
            }
            Hashtable hs = new Hashtable();
            hs.Add("T_user_agreement", iHandle.ToUpper());
            hs.Add("T_User", IUserCode.ToUpper());
            hs.Add("T_out_item", iBillNo.ToUpper());
            string[] rs = db.CallPrc("PRC_TXOUT_SNDETA", new string[] { "T_oErrMSG", "T_out_sqlconn" }, hs, out str);
            if (rs[0].ToString().Substring(0, 1).Equals("0"))
            {
                oErrMessage = rs[0].ToString();
                return false;//注意
            }
            else
            {
                oErrMessage = rs[0].ToString();
                T_out_sqlconn = rs[1].ToString();
            }
            //T_out_sqlconn = "select * from MS_test_20190103 r where r.出库单=";
            //string testiBillNo = iBillNo;//注意
            sqlDataTable = T_out_sqlconn + "'" + iBillNo + "'";
            oTable = db.SelectDataBase(sqlDataTable);
            for (int i = 0; i < oTable.Rows.Count; i++)
            {
                for (int j = 0; j < oTable.Columns.Count; j++)
                {
                    getlistdata.Add(oTable.Rows[i][oTable.Columns[j].ColumnName].ToString());
                }
            }

        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息：GetDataSet" + ex.Message;
            return false;
        }
        return true;
    }

    [WebMethod]
    public bool SetBillStatus(string iHandle, string IUserCode, string iBillNo, out string oFlag, out string oErrMessage)
    //输入特征码（iHandle），用户（IUserCode），出库单（iBillNo），返回是否ok（oFlag），返回错误信息（oErrMessage）
    {
        oFlag = "OK";
        oErrMessage = "";
        try
        {
            if (string.IsNullOrEmpty(iHandle))
            {
                oErrMessage = "特征码不允许为空";
                oFlag = "NG";
                return false;
            }
            if (string.IsNullOrEmpty(IUserCode))
            {
                oErrMessage = "密码不允许为空";
                oFlag = "NG";
                return false;
            }
            if (string.IsNullOrEmpty(iBillNo))
            {
                oErrMessage = "出库单不允许为空";
                oFlag = "NG";
                return false;
            }
            Hashtable hs = new Hashtable();
            hs.Add("T_user_agreement", iHandle.ToUpper());
            hs.Add("T_User", IUserCode.ToUpper());
            hs.Add("T_out_item", iBillNo.ToUpper());
            string[] rs = db.CallPrc("PRC_UPDATE_OUTITEM", new string[] { "T_oErrMSG" }, hs, out str);
            if (rs[0].ToString().Substring(0, 1).Equals("0"))
            {
                oErrMessage = "NG错误信息" + rs[0].ToString();
                return false;
            }
            else
            {
                oErrMessage = rs[0].ToString();

            }
        }
        catch (Exception e)
        {
            oErrMessage = "错误信息SetBillStatus" + e.Message;
            oFlag = "NG";
            return false;
        }

        return true;
    }

    [WebMethod]
    public bool GetiSN_barif(string iSN, out List<string> getlistdata, out string oErrMessage)
    //输入SN，返回过站信息（oTable），返回错误信息（oErrMessage）
    {
        DataTable oTable = null;
        getlistdata = new List<string>();
        oErrMessage = null;
        string sqlDataTable = "";
        try
        {
            if (string.IsNullOrEmpty(iSN))
            {
                oErrMessage = "SN不允许为空";
                return false;
            }
            sqlDataTable ="select a.if001 排产单,b.workshop_center_name 工作中心, a.if003 产品编码,c.line_name 线体, d.proc_name 工序,a.if009 条码,a.if011 维修标识,a.if012 过站标识,a.create_by 创建人,a.create_date 创建时间" 
                         +"from mes_barif a"
                         +"left join MES_WORKSHOP_CENTERS b"
                         +" on a.if002=b.workshop_center_code"
                         +" left join MES_LINE c"
                         +" on a.if004=c.line_no"
                         +" left join MES_PROC_INFO d"
                         +" on a.if005=d.proc_no"
                         +" where a.if009='"+iSN+"'";

            oTable = db.SelectDataBase(sqlDataTable);
            for (int i = 0; i < oTable.Rows.Count; i++)
            {
                for (int j = 0; j < oTable.Columns.Count; j++)
                {
                    getlistdata.Add(oTable.Rows[i][oTable.Columns[j].ColumnName].ToString());
                }
            }

        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息：GetiSN_barif" + ex.Message + "SN" + iSN;
            return false;
        }
        return true;
    }


    /// <summary>
    /// 以下所有函数，都来自于MES server DLL，copy而来。
    /// </summary>
    /// <param name="_iUserCode"></param>
    /// <param name="iResCode"></param>
    /// <param name="_iPassWord"></param>
    /// <param name="oErrMessage"></param>
    /// <returns></returns>

    [WebMethod]
    //迁移DLL方法，用于检查用户密码以及岗位资源代码
    public bool CheckUserAndResourcePassed_Plus(string _iUserCode, string iResCode, string _iPassWord, out string oErrMessage)
    {
        oErrMessage = null;
        try 
        {
            if (mes_tools.CheckUserAndResourcePassed(_iUserCode, iResCode, _iPassWord, out oErrMessage))
            {
                
                return true;
            }
            else 
            {
                
                return false;
            }
        }
        catch (Exception ex)
        {

            oErrMessage = "NG:" + ex.Message;
            return false;
        }

        return true;
    }

    [WebMethod]
    //迁移DLL方法，检查SN当前能否进入当前站点（途程检查）
    public bool CheckRoutePassed(string iSN, string iResCode, out string oErrMessage)
    {
        oErrMessage = "";
        if (string.IsNullOrEmpty(iSN))
        {
            oErrMessage = "SN不能为空";
            return false;
        }
        if (string.IsNullOrEmpty(iResCode))
        {
            oErrMessage = "资源代码不能为空";
            return false;
        }
        try
        {
            if (mes_tools.CheckRoutePassed(iSN,iResCode,out oErrMessage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {

            oErrMessage = "错误信息：" + ex.Message;
            return false;
        }

        return true;
    }

    [WebMethod]
    //迁移DLL方法，上传附件信息
    public bool SetAttachment(string iResource, string iType, string iSN, string iAttSn, string iUser, out string oErrMessage)
    {
        try
        {
            if (mes_tools.SetAttachment(iResource, iType, iSN, iAttSn, iUser, out oErrMessage))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息：" + ex.Message;
            return false;
        }
        return true;
    }

    [WebMethod]
    //迁移DLL方法，获取附件信息
    public bool GetAttachment(string iResource, string iType, string iSN, out string oAttSn, out string oErrMessage)
    {
        try
        {
            if (mes_tools.GetAttachment(iResource, iType, iSN, out oAttSn, out oErrMessage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息：" + ex.Message;
            oAttSn = "错误信息：" + ex.Message;
            return false;
        }
        return true;
    }

    [WebMethod]
    //迁移DLL方法，条码过站函数
    public bool SetMobileData(string iSN, string iResCode, string iOperator, string iResult, string iErrCode, string iTSMemo, out string oErrMessage)
    {
        oErrMessage = string.Empty;
        try
        {
            if (mes_tools.SetMobileData(iSN,iResCode,iOperator,iResult,iErrCode,iTSMemo,out oErrMessage))
            {
                oErrMessage ="OK"+oErrMessage;
                return true;
            }
            else
            {
                oErrMessage = "NG" + oErrMessage;
                return false;
            }
        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息：" + ex.Message;
            return false;
        }
        return true;
    }

    [WebMethod]
    //迁移DLL方法，GetAllCode,根据号段表进行写号.
    public bool GetAllCode(string iResource, string iSn, string iBarCode, out string oAllCode, out string oErrMessage)
    {
        oAllCode = null;
        try 
        {
           if(mes_tools.GetAllCode(iResource,iSn,iBarCode,out oAllCode,out oErrMessage))
           {
               return true;
           }
            else
           {
               return false;
           }
        }
        catch(Exception ex)
        {
            oErrMessage = ex.Message;
            return false;
        }
    }

    [WebMethod]
    //迁移DLL方法，GetAllCode,根据号段确认写入的号
    public bool SetAllInfo(string iSN, string iFlag, out string oErrMessage)
    {
        try
        {
            if (mes_tools.SetAllInfo(iSN, iFlag, out oErrMessage))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        catch (Exception ex) 
        {
            oErrMessage = "错误信息" + ex.Message;
            return false;
        }
    }

    [WebMethod]
    //迁移DLL方法，根据IMEI获取号段信息
    public bool GetSnByImei(string iImei, string iResCode, out string oSn, out string oFlag, out string oErrMessage) 
    {
        oFlag=String.Empty;
        oSn = String.Empty;
        try
        {
            if(mes_tools.GetSnByImei(iImei,iResCode,out oSn,out oFlag,out oErrMessage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex) 
        {
            oErrMessage = "错误信息:" + ex.Message;
            return false;
        }
    }

    [WebMethod]
    //迁移DLL方法，上传号段信息
    public bool SetALLmodel_imei(string iResCode, string iSN, string iBT, string iWIFI, string iCODE1, string iCODE2, out string oErrMessage) 
        {
            oErrMessage = String.Empty;
            try
            {
                if (mes_tools.SetALLmodel_imei(iResCode,iSN,iBT,iWIFI,iCODE1,iCODE2,out oErrMessage))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                oErrMessage = "错误信息:" + ex.Message;
                return false;
            }
        }
    [WebMethod]
    //迁移DLL方法，根据iwfi获取sn
    public bool GETALLmodel_imei(string iResCode, string iWIFI, out string iSN, out string iBT, out string iCODE1, out string iCODE2, out string oErrMessage)
    {
        iSN = String.Empty;
        iBT = String.Empty;
        iCODE1 = String.Empty;
        iCODE2 = String.Empty;
        oErrMessage = String.Empty;
        try
        {
            if (mes_tools.GETALLmodel_imei(iResCode,iWIFI,out iSN,out iBT,out iCODE1,out iCODE2,out oErrMessage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            oErrMessage = "错误信息:" + ex.Message;
            return false;
        }
    }

    [WebMethod]
    //迁移DLL方法，安卓上传文件
    //oErrMessage = route[0] + route[1] + route[2];
    //public bool UploadFileAndroid(String FtpServerIP, String FtpUser, String FtpPsw, String MemoryStream,String fileName,out String oErrMessage) 
    public bool UploadFileAndroid(String FtpServerIP, String FtpUser, String FtpPsw, byte[] bytes, String filename, out String oErrMessage)
    {
        oErrMessage = null;
        //FTP目录
        String FTPDirectory;
        try
        {
            //连接FTP
            FTPUtil ftpUtil = new FTPUtil(FtpServerIP, "", FtpUser, FtpPsw);
            //时间
            string NowDate = db.GetColumnObject("select to_char(sysdate,'yyyy-mm-dd hh24:mi:ss') as nowTime from dual").ToString();
            string ServerTime = db.GetColumnObject("select to_char(sysdate,'yyyy-MM-dd-hh24') as nowTime from dual").ToString();
            string[] route = ServerTime.Split(new char[] { '-' }, StringSplitOptions.None);
            FTPDirectory=route[0] + route[1] + route[2];
            //文件路径
            ftpUtil.GotoDirectory(FTPDirectory,true);

            oErrMessage = "0";
            FtpWebRequest frequest = null;
            if (!ftpUtil.FileExist_1(ftpUtil.ftpuri))
            {
                frequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpUtil.ftpuri));
                frequest.Credentials = new NetworkCredential(FtpUser, FtpPsw);
                frequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = frequest.GetResponse() as FtpWebResponse;
                //frequest.KeepAlive = false;
            }
            oErrMessage = "11";
            FtpWebRequest frequest1 = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpUtil.ftpuri + "/" + filename));
            frequest1.Credentials = new NetworkCredential(FtpUser, FtpPsw);

            oErrMessage = "12";
            frequest1.KeepAlive = false;// 默认为true，连接不会被关闭 // 在一个命令之后被执行
            oErrMessage = "14";
            //frequest.KeepAlive = false;
            oErrMessage = "13";
            frequest1.Method = WebRequestMethods.Ftp.UploadFile;// 指定执行什么命令
            oErrMessage = "15";
            frequest1.UseBinary = true;// 指定数据传输类型

            frequest1.ContentLength = bytes.Length;// 上传文件时通知服务器文件的大小


            Stream strm = frequest1.GetRequestStream();


            strm.Write(bytes, 0, bytes.Length);
            
            strm.Close();
            oErrMessage = "2";

            return true;
        }
        catch(Exception ex)
        {
            oErrMessage = "错误信息" + oErrMessage+ ex.Message;
            return false;
        }
    }
}
