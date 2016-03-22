using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Threading;

namespace ElkNET
{
    public class DBConnection
    {     
        private OracleConnection connection;

        /// <summary>
        /// Все таблицы, которые нужны в ЛОСЕ
        /// </summary>
        public DataSet mainDataSet = new DataSet("parusDataSet");

        public const string tsd_tc_table = "aa_v_tsd_tc_new";
        public const string from_tsd_table = "aa_v_from_tsd";
        public const string boxes_tsd_table = "aa_v_boxes_tsd_new";
        public const string marks_tsd_table = "aa_v_marks_tsd_new";

        /// <summary>
        /// Создает соединение с БД, используя последние имя пользователя и пароль.
        /// Не устанавливает соединение.
        /// </summary>
        public DBConnection() : this(ConnectionSettings.login, ConnectionSettings.password) { }

        /// <summary>
        /// Создает соединение с БД
        /// Не устанавливает соединение.
        /// </summary>
        /// <param name="login">имя пользователя</param>
        /// <param name="password">пароль</param>
        public DBConnection(string login, string password)
        {
            try
            {
                this.connection = new OracleConnection();
                this.connection.ConnectionString = String.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL={0})(HOST={1})(PORT={2})))(CONNECT_DATA=(SERVICE_NAME={3})));User Id={4};Password={5};",
                        ConnectionSettings.protocol,
                        ConnectionSettings.host,
                        ConnectionSettings.port,
                        ConnectionSettings.service_name,
                        login,
                        password);
                this.mainDataSet.Tables.Add(tsd_tc_table);
                this.mainDataSet.Tables.Add(from_tsd_table);
                this.mainDataSet.Tables.Add(boxes_tsd_table);
                this.mainDataSet.Tables.Add(marks_tsd_table);
            }
            catch { throw; }
        }

        ~DBConnection()
        {
            try
            {
                connection.Close();
            }
            catch { }
        }

        /// <summary>
        /// Пытается установить соединение с базой Oracle
        /// </summary>
        public void Open()
        {
            try
            {
                this.connection.Open();
            }
            catch { throw; }
        }

        /// <summary>
        /// Закрывает текущее соединение с базой Oracle
        /// </summary>
        public void Close()
        {
            try
            {
                this.connection.Close();
            }
            catch { }
        }

        public void clearDataSet()
        {
            try
            {
                this.mainDataSet.Tables[tsd_tc_table].Clear();
                this.mainDataSet.Tables[from_tsd_table].Clear();
                this.mainDataSet.Tables[boxes_tsd_table].Clear();
                this.mainDataSet.Tables[marks_tsd_table].Clear();
            }
            catch { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="hasDocInfo"></param>
        /// <param name="docName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DataView getTable_tsd_tc(DateTime? startDate, DateTime? endDate, bool hasDocInfo, string docName = null, string fileName = null, bool isError = false)
        {
            OracleCommand dbCommand = new OracleCommand()
            {
                Connection = this.connection,
                Transaction = null
            };
            dbCommand.CommandText = "select * from " + tsd_tc_table+" tbl";
            if (hasDocInfo) dbCommand.CommandText += " where tbl.SDOC is not null";
                else dbCommand.CommandText += " where tbl.SDOC is null";
            dbCommand.CommandText += (startDate.HasValue ? String.Format(" and tbl.DOCDATE >= '{0}'", startDate.Value.ToShortDateString()) : "")
                        + (endDate.HasValue ? String.Format(" and tbl.DOCDATE <= '{0}'", endDate.Value.ToShortDateString()) : "")
                        + (!string.IsNullOrWhiteSpace(docName) ? String.Format(" and tbl.SDOC = '{0}'", docName) : "")
                        + (!string.IsNullOrWhiteSpace(fileName) ? String.Format(" and (tbl.FILE_NAME = '{0}' or tbl.FILE_NAME = '{0}.txt')", fileName) : "")
                        + (isError ? " and tbl.ISERROR = 1" : "");
            OracleDataAdapter oda = new OracleDataAdapter() { SelectCommand = dbCommand };
            this.mainDataSet.Tables[tsd_tc_table].Clear();
            oda.Fill(this.mainDataSet, tsd_tc_table);
            return this.mainDataSet.Tables[tsd_tc_table].AsDataView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns>Таблица исходных файлов из ТСД</returns>
        public DataView getTable_from_tsd(string file_name)
        {
            OracleCommand dbCommand = new OracleCommand()
            {
                CommandText = String.Format("select * from {0} tbl where tbl.FILE_NAME='{1}'", from_tsd_table, file_name),
                Connection = this.connection,
                Transaction = null
            };
            OracleDataAdapter oda = new OracleDataAdapter() { SelectCommand = dbCommand };
            this.mainDataSet.Tables[from_tsd_table].Clear();
            oda.Fill(this.mainDataSet, from_tsd_table);
            return this.mainDataSet.Tables[from_tsd_table].AsDataView();
        }

        /// <summary>
        /// Загружает из базы таблицу с коробками
        /// </summary>
        /// <param name="file_name">Имя файла, выгруженного из ТСД</param>
        /// <returns>Таблица с коробками</returns>
        public DataView getTable_boxes_tsd(string file_name)
        {
            OracleCommand dbCommand = new OracleCommand()
            {
                CommandText = String.Format("select * from {0} tbl where tbl.FILE_NAME='{1}'", boxes_tsd_table, file_name),
                Connection = this.connection,
                Transaction = null
            };
            OracleDataAdapter oda = new OracleDataAdapter() { SelectCommand = dbCommand };
            this.mainDataSet.Tables[boxes_tsd_table].Clear();
            oda.Fill(this.mainDataSet, boxes_tsd_table);
            return this.mainDataSet.Tables[boxes_tsd_table].AsDataView();
        }

        /// <summary>
        /// Загружает из базы таблицу с акцизными марками
        /// </summary>
        /// <param name="rn">RN отсканированной коробки</param>
        /// <returns>таблица акцизных марок</returns>
        public DataView getTable_marks_tsd(string rn)
        {
            OracleCommand dbCommand = new OracleCommand()
            {
                CommandText = String.Format("select * from {0} tbl where tbl.PRN={1}", marks_tsd_table, rn),
                Connection = this.connection,
                Transaction = null
            };
            OracleDataAdapter oda = new OracleDataAdapter() { SelectCommand = dbCommand };
            this.mainDataSet.Tables[marks_tsd_table].Clear();
            this.mainDataSet.Tables[marks_tsd_table].Clear();
            oda.Fill(this.mainDataSet, marks_tsd_table);
            return this.mainDataSet.Tables[marks_tsd_table].AsDataView();
        }

        /// <summary>
        /// Изменяет строку в таблице процедурой aa_p_egais_tsd_update
        /// </summary>
        /// <param name="tableName">название таблицы</param>
        /// <param name="rn">RN записи</param>
        /// <param name="new_barcode">новый штрихкод</param>
        /// <param name="new_quantity">новое количество</param>
        /// <param name="new_dateBottling">новая дата розлива</param>
        public void updateRowFromDB(string tableName, string rn, string new_barcode, string new_quantity, DateTime? new_dateBottling = null)
        {
            try
            {
                OracleCommand dbCommand = new OracleCommand("aa_p_egais_tsd_update", this.connection);
                dbCommand.CommandType = CommandType.StoredProcedure;
                if (tableName.Equals(boxes_tsd_table)) dbCommand.Parameters.Add("stable", OracleDbType.Varchar2, "Grid_boxes", ParameterDirection.Input);
                if (tableName.Equals(from_tsd_table)) dbCommand.Parameters.Add("stable", OracleDbType.Varchar2, "Grid_from", ParameterDirection.Input);
                dbCommand.Parameters.Add("nrn", OracleDbType.Int32, rn, ParameterDirection.Input);
                dbCommand.Parameters.Add("p_barcode", OracleDbType.Varchar2, new_barcode, ParameterDirection.Input);
                dbCommand.Parameters.Add("p_quant", OracleDbType.Varchar2, new_quantity, ParameterDirection.Input);
                if(new_dateBottling.HasValue) dbCommand.Parameters.Add("p_date_razl", OracleDbType.Date, new_dateBottling.Value, ParameterDirection.Input);
                OracleDataAdapter oda = new OracleDataAdapter(dbCommand);
                dbCommand.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Удаляет запись из таблицы
        /// </summary>
        /// <param name="tableName">имя таблицы</param>
        /// <param name="rn">RN записи</param>
        public void deleteRowFromDB(string tableName, string rn)
        {
            try
            {
                OracleCommand dbCommand = new OracleCommand("aa_p_egais_tsd_delete", this.connection);
                dbCommand.CommandType = CommandType.StoredProcedure;
                if (tableName.Equals(marks_tsd_table)) dbCommand.Parameters.Add("stable", OracleDbType.Varchar2, "Grid_marks", ParameterDirection.Input);
                if (tableName.Equals(boxes_tsd_table)) dbCommand.Parameters.Add("stable", OracleDbType.Varchar2, "Grid_boxes", ParameterDirection.Input);
                dbCommand.Parameters.Add("nrn", OracleDbType.Int32, rn, ParameterDirection.Input);
                OracleDataAdapter oda = new OracleDataAdapter(dbCommand);
                dbCommand.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }
    }
}