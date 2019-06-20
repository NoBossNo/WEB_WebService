using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.OracleClient;

namespace FSMesService
{
    class OracleConnManager {

        #region   全局变量
        public static string M_ConnStr = " Data Source= (DESCRIPTION = " +
                                          "    (ADDRESS_LIST = " +
                                          "      (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.100.6)(PORT = 1521)) " +
                                          "    ) " +
                                          "    (CONNECT_DATA = " +
                                          "      (SERVICE_NAME = MES) " +
                                          "    ) "+
                                          "  );User ID=admin;Password=topmes118";

        public static OracleConnection Conn = null;  //声明链接对象
        public static OracleTransaction transaction = null;
        
        #endregion

        #region   连接数据库
        public static OracleConnection GetCon() {
            if (Conn != null && Conn.State != ConnectionState.Closed) {
                return Conn;
            }
            Conn = new OracleConnection(M_ConnStr);
            Conn.Open();
            return Conn;
        }
        #endregion
    }
}
