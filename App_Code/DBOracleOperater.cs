using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Data.OracleClient;

namespace FSMesService
{
     class DBOracleOperater {
        private static DBOracleOperater db = null;
        private OracleConnection Conn;
        private int _TimeOut = 360;

        #region 构造函数
        private DBOracleOperater() {
        }

        public static DBOracleOperater GetInstance() {
            if (db != null) {
                return db;
            }

            db = new DBOracleOperater();
            return db;
        }
        #endregion

        #region 其他
        private OracleDataReader GetTableData(string sql) {
            Conn = OracleConnManager.GetCon();
            OracleCommand cmd = Conn.CreateCommand();

            cmd.CommandTimeout = _TimeOut;
            cmd.CommandText = sql;
            OracleDataReader reader = cmd.ExecuteReader();
            return reader;
        }

        #region 执行SQL语句

        public int ExcuteSQL(string sql) {
            Conn = OracleConnManager.GetCon();
            OracleCommand cmd = new OracleCommand(sql, Conn);
            cmd.CommandTimeout = _TimeOut;
            int count = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return count;
        }

        public int ExcuteSQL(string sql,Hashtable ht) {
            Conn = OracleConnManager.GetCon();
            OracleCommand cmd = Conn.CreateCommand();            
            cmd.CommandTimeout = _TimeOut;
            cmd.CommandText = sql;
            foreach (string key in ht.Keys) {
                cmd.Parameters.AddWithValue(key, ht[key].ToString());
            }
            int count = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return count;
        }

        public DataTable SelectDataBase(string sql) {
            try {
                Conn = OracleConnManager.GetCon();
                DataSet ds = new DataSet();
                OracleDataAdapter da = new OracleDataAdapter(sql, Conn);
                da.SelectCommand.CommandTimeout = _TimeOut;//设置超时时间
                //ds.Locale = System.Globalization.CultureInfo.CurrentCulture;
                da.Fill(ds);
                return ds.Tables[0];
            } catch (Exception ex) {
                OracleConnManager.Conn = null;
                throw ex;
                
                //Console.WriteLine(sql);
                //Console.WriteLine(ex.StackTrace);
            }
            return null;
        }
        ////传入table
        //public List<T> ConvertTo<T>(DataTable table)
        //{
        //    if (table == null)
        //    {
        //        return null;
        //    }

        //    List<DataRow> rows = new List<DataRow>();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        rows.Add(row);
        //    }

        //    return ConvertTo<T>(rows);
        //}

        //public List<T> ConvertTo<T>(List<DataRow> rows)
        //{
        //    List<T> list = null;

        //    if (rows != null)
        //    {
        //        list = new List<T>();

        //        foreach (DataRow row in rows)
        //        {
        //            T item = CreateItem<T>(row);
        //            list.Add(item);
        //        }
        //    }

        //    return list;
        //}

        //public T CreateItem<T>(DataRow row)
        //{
        //    T obj = default(T);
        //    if (row != null)
        //    {
        //        obj = Activator.CreateInstance<T>();

        //        foreach (DataColumn column in row.Table.Columns)
        //        {
        //            PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
        //            try
        //            {
        //                object value = row[column.ColumnName];
        //                prop.SetValue(obj, value, null);
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //        }
        //    }

        //    return obj;
        //}

        /// <summary>
        /// 分页查询SQL语句
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页条数</param>
        /// <returns>DataTable</returns>
        /// <creator>zhanghang at 20130710</creator>
        public DataTable SelectDataBase(string sql, int pageIndex, int pageSize)
        {
            try
            {
                string pageSql = "SELECT *  FROM (SELECT A.*, ROWNUM RN " +
                                 " FROM (" + sql + ") A WHERE ROWNUM <= " + (pageIndex) * pageSize+ ") " +
                                 " WHERE RN >= " + ((pageIndex-1) * pageSize+1) + " ";
                Conn = OracleConnManager.GetCon();
                DataSet ds = new DataSet();
                OracleDataAdapter da = new OracleDataAdapter(pageSql, Conn);
                da.SelectCommand.CommandTimeout = _TimeOut;//设置超时时间
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                OracleConnManager.Conn = null;
                Console.WriteLine(sql);
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }        

        #endregion
        //判断SQL语句 返回布尔值
        public bool CheckSQL(string sql) {
            OracleDataReader reader = GetTableData(sql);
            try {
                if (reader.Read()) {
                    return true;
                }
            } catch (Exception) {
                throw;
            } finally {
                reader.Close();
                reader.Dispose();
                CloseConnection();
            }
            return false;
        }

        private void CloseConnection() {
            //if (Conn==null ) {
            //    return;
            //}
            //if (Conn.State == ConnectionState.Open) {
            //    Conn.Close();
            //    Conn = null;
            //}
        }

        //根据SQL语句获取一列的值
        public Object GetColumnObject(string sql) {
            return GetColumnObject(sql, 0);
        }

        public Object GetColumnObject(string sql, string ColumnName) {
            OracleDataReader reader = GetTableData(sql);
            try {
                if (reader.Read()) {
                    return reader[ColumnName];
                }
            } catch (Exception) {
                throw;
            } finally {
                reader.Close();
                reader.Dispose();
                CloseConnection();
            }
            return null;
        }

        public Object GetColumnObject(string sql, int ColIndex) {
            OracleDataReader reader = GetTableData(sql);
            try {
                if (reader.Read()) {
                    return reader[ColIndex];
                }
            } catch (Exception) {
                throw;
            } finally {
                reader.Close();
                reader.Dispose();
                CloseConnection();
            }
            return null;
        }
        #endregion

        #region 向数据库写数据
        public int InsertDBFrmDataTable(string TableStrcutSQL, DataTable dt, Boolean isByOrder) {
            if (dt == null || dt.Rows.Count <= 0) {
                return 0;
            }
            try {
                Conn = OracleConnManager.GetCon();
                OracleDataAdapter ad = new OracleDataAdapter(TableStrcutSQL, Conn);//取个结构
                
                DataTable dt1 = new DataTable();
                OracleCommandBuilder cmb = new OracleCommandBuilder(ad);
                ad.Fill(dt1);

                for (int i = 0; i < dt.Rows.Count; i++) {
                    DataRow row = dt1.NewRow();
                    for (int j = 0; j < dt.Columns.Count; j++) {
                        row[j] = dt.Rows[i][j].ToString();//按顺序放参数值
                    }
                    dt1.Rows.Add(row);
                }
                return ad.Update(dt1);
            } finally {
                CloseConnection();
            }
        }

        public int InsertDBFrmDataTable(string TableStrcutSQL, DataTable dt) {
            if (dt == null || dt.Rows.Count <= 0) {
                return 0;
            }
            try {
                Conn = OracleConnManager.GetCon();
                OracleDataAdapter ad = new OracleDataAdapter(TableStrcutSQL, Conn);//取个结构   

                OracleCommandBuilder cmb = new OracleCommandBuilder(ad);
                ad.Fill(dt);

                return ad.Update(dt);
            } finally {
                CloseConnection();
            }
        }

        public int EditDBFrmDataTable(string TableStrcutSQL, DataTable dt) {
            if (dt == null || dt.Rows.Count <= 0) {
                return 0;
            }

            try {
                Conn = OracleConnManager.GetCon();
                OracleCommand sqlCommand = new OracleCommand(TableStrcutSQL, Conn);

                OracleDataAdapter sqlAdapater = new OracleDataAdapter(sqlCommand);//取个结构 
                OracleCommandBuilder sqlBuilder = new OracleCommandBuilder(sqlAdapater);//必须有     

                DataTable dt1 = dt.GetChanges(DataRowState.Deleted);
                if (dt1 == null || dt1.Rows.Count <= 0) {

                } else {
                    sqlAdapater.Fill(dt1);
                    sqlAdapater.Update(dt1);
                }


                DataTable dt2 = dt.GetChanges(DataRowState.Modified);
                if (dt2 == null || dt2.Rows.Count <= 0) {

                } else {
                    sqlAdapater.Fill(dt2);
                    sqlAdapater.Update(dt2);
                }


                DataTable dt3 = dt.GetChanges(DataRowState.Added);
                if (null == dt3 || dt3.Rows.Count <= 0) {

                } else {
                    sqlAdapater.Fill(dt3);
                    sqlAdapater.Update(dt3);
                }


                return 0;
            } finally {
                CloseConnection();
            }
        }

        public bool InsertDBFrmList(ArrayList list, string _NameSpaceName, string _ObjeclClassName) {
            if (list == null || list.Count <= 0) {
                return false;
            }
            Assembly assembly = Assembly.Load(_NameSpaceName);
            object obj = assembly.CreateInstance(_ObjeclClassName);
            if (null == obj) {
                return false;
            }
            Type type = obj.GetType();
            PropertyInfo[] ps = type.GetProperties();

            Conn = OracleConnManager.GetCon();
            OracleTransaction tr = Conn.BeginTransaction();
            OracleCommand cmd = this.Conn.CreateCommand();
            cmd.Transaction = tr;
            try {
                string _fs = "";
                string _vs = "";
                foreach (PropertyInfo pi in ps) {//循环每个字段
                    string _fieldName = pi.Name;
                    _fs += _fieldName + ",";
                    _vs += "@" + _fieldName + ",";
                }
                string _tn = _ObjeclClassName.Substring(_ObjeclClassName.LastIndexOf('.') + 1);
                string sql = "insert into " + _tn + " (" + _fs.Substring(0, _fs.Length - 1) + ") values ("
                    + _vs.Substring(0, _vs.Length - 1) + ")";


                foreach (object item in list) {
                    cmd.CommandText = sql;
                    foreach (PropertyInfo pi in ps) {//循环每个字段
                        string _fieldName = pi.Name;
                        string _typeName = pi.PropertyType.Name;
                        object _valueObj = pi.GetValue(item, null);


                        if (_typeName.ToLower().Contains("string")) {
                            if (_valueObj == null) {
                                _valueObj = "";
                            }
                        } else if (_typeName.ToLower().Contains("double")) {
                            if (_valueObj == null) {
                                _valueObj = 0.0;
                            }
                        } else if (_typeName.ToLower().Contains("int")) {
                            if (_valueObj == null) {
                                _valueObj = 0;
                            }
                        }
                        cmd.Parameters.AddWithValue("@" + _fieldName, _valueObj);

                    }
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }

                tr.Commit();
                return true;
            } catch (Exception e) {
                try {
                    tr.Rollback();
                } catch (Exception) {
                    throw;
                }
                throw e;
            } finally {
                tr.Dispose();
                tr = null;
                cmd.Dispose();
                cmd = null;
                CloseConnection();  
            }            
        }

        public bool InsertDBFrmObject(Object item, string _NameSpaceName, string _ObjeclClassName) {
            if (item == null) {
                return false;
            }
            Assembly assembly = Assembly.Load(_NameSpaceName);
            object obj = assembly.CreateInstance(_ObjeclClassName);
            if (null == obj) {
                return false;
            }
            Type type = obj.GetType();
            PropertyInfo[] ps = type.GetProperties();

            Conn = OracleConnManager.GetCon();
            OracleCommand cmd = this.Conn.CreateCommand();
            try {
                string _fs = "";
                string _vs = "";
                foreach (PropertyInfo pi in ps) {//循环每个字段
                    string _fieldName = pi.Name;
                    _fs += _fieldName + ",";
                    _vs += "@" + _fieldName + ",";
                }
                string _tn = _ObjeclClassName.Substring(_ObjeclClassName.LastIndexOf('.') + 1);
                string sql = "insert into " + _tn + " (" + _fs.Substring(0, _fs.Length - 1) + ") values ("
                    + _vs.Substring(0, _vs.Length - 1) + ")";

                cmd.CommandText = sql;
                foreach (PropertyInfo pi in ps) {//循环每个字段                        
                    string _fieldName = pi.Name;
                    string _typeName = pi.PropertyType.Name;
                    object _valueObj = pi.GetValue(item, null);
                    if (_typeName.ToLower().Contains("string")) {
                        if (_valueObj == null) {
                            _valueObj = "";
                        }
                    } else if (_typeName.ToLower().Contains("double")) {
                        if (_valueObj == null) {
                            _valueObj = 0.0;
                        }
                    } else if (_typeName.ToLower().Contains("int")) {
                        if (_valueObj == null) {
                            _valueObj = 0;
                        }
                    }
                    //cmd.Parameters.AddWithValue("@" + _fieldName, _valueObj);
                }
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                return true;
            } finally {
                cmd.Dispose();
                CloseConnection();
            }

            //   return false;

        }

        #endregion

        #region Object
        public object GetObject(string sql, string _ObjeclClassName, string _NameSpaceName) {
            if (sql == null || sql.Trim().Equals("")) {
                return null;
            }
            OracleDataReader reader = GetTableData(sql);
            try {
                if (reader.Read()) {
                    Assembly assembly = Assembly.Load(_NameSpaceName);
                    object obj = assembly.CreateInstance(_ObjeclClassName);
                    if (null == obj) {
                        return null;
                    }
                    Type type = obj.GetType();
                    PropertyInfo[] ps = type.GetProperties();
                    foreach (PropertyInfo pi in ps) {
                        string _fieldName = pi.Name;
                        string _typeName = pi.PropertyType.Name;
                        if (IsContainColumn(reader, _fieldName)) {//查询的结果包含列
                            if (_typeName.ToLower() == "string") {//赋字符串
                                pi.SetValue(obj, reader[_fieldName].ToString().Trim(), null);
                            } else if (_typeName.ToLower().Contains("int")) {//赋整数
                                pi.SetValue(obj, ConvertToInt32(reader[_fieldName].ToString().Trim()), null);
                            } else if (_typeName.ToLower().Contains("datetime")) {//赋整数
                                pi.SetValue(obj, ConvertToDatetime(reader[_fieldName]), null);
                            } else if (_typeName.ToLower().Contains("date")) {//赋整数
                                pi.SetValue(obj, ConvertToDatetime(reader[_fieldName]), null);
                            } else if (_typeName.ToLower().Contains("byte")) {//赋byte
                                pi.SetValue(obj, reader[_fieldName], null);
                            } else {//赋double
                                pi.SetValue(obj, ConvertToDouble(reader[_fieldName].ToString().Trim()), null);
                            }
                        }
                    }

                    return obj;
                }
            } catch (Exception e) {
                throw e;
            } finally {
                reader.Close();
                reader.Dispose();
                CloseConnection();
            }
            return null;
        }
        //查询reader结果集中，是否包含了指定的列
        private bool IsContainColumn(OracleDataReader reader, string _ColumnName) {
            for (int i = 0; i < reader.FieldCount; i++) {
                if (reader.GetName(i).Trim().ToUpper() == _ColumnName.ToUpper()) {
                    return true;
                }
            }
            return false;
        }

        private double ConvertToDouble(string d) {
            if (d == null || d.Trim() == "") {
                return 0.0;
            }
            return Convert.ToDouble(d);
        }

        private int ConvertToInt32(string d) {
            if (d == null || d.Trim() == "") {
                return 0;
            }
            return Convert.ToInt32(d);
        }
        private int ConvertToInt32(object d) {
            if (d == null) {
                return 0;
            }
            return this.ConvertToInt32(d.ToString());
        }
        private DateTime ConvertToDatetime(object d) {
            if (d == null) {
                return DateTime.Now;
            }
            return Convert.ToDateTime(d.ToString());
        }

        public string ConvertToString(object Str) {
            if (null == Str) {
                return "";
            }

            return Str.ToString();
        }
        #endregion

        #region 存储过程

        public string[] CallPrc(string _ProcName, string[] _OutputResults, Hashtable ht,out string str) {

             str = null;
            if (_ProcName == null || _ProcName == "") {
                
                return null;
            }
            try 
            {
                Conn = OracleConnManager.GetCon();
            }
            catch (Exception ex)
            {

                str = "bbb"+ex.Message;
            }
            OracleCommand cmd = Conn.CreateCommand();
            
            try {
                cmd.CommandType = CommandType.StoredProcedure;//设置调用的类型为存储过程
                cmd.CommandText = _ProcName;
                
                OracleParameter _Parme;
                if (_OutputResults != null) {
                    for (int i = 0; i < _OutputResults.Length; i++) {
                        _Parme = cmd.Parameters.Add(_OutputResults[i], OracleType.NVarChar, 1000);
                        _Parme.Direction = ParameterDirection.Output;
                    }
                }

                if (ht == null || ht.Count <= 0) {

                } else {
                    IDictionaryEnumerator ide = ht.GetEnumerator();
                    while (ide.MoveNext()) {
                        _Parme = cmd.Parameters.Add(ide.Key.ToString(),OracleType.NVarChar);
                        _Parme.Direction = ParameterDirection.Input;
                        _Parme.Value = ide.Value;
                    }
                }

                cmd.ExecuteNonQuery();

                if (_OutputResults != null) {
                    for (int i = 0; i < _OutputResults.Length; i++) {
                        _OutputResults[i] = cmd.Parameters[_OutputResults[i]].Value.ToString();
                    }
                }

                return _OutputResults;
            } catch (Exception ex) {
                
                throw ex;
            } finally {
                cmd.Dispose();
                CloseConnection();
            }
        }
   
        #endregion

       
    }
}
