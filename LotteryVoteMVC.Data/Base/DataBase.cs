using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft;
using System.Reflection;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Data
{
    public class DataBase
    {
        protected readonly string _connectionString;
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public DataBase() : this(DBManager.ConnectionString) { }
        public DataBase(string connectionString)
        {
            this._connectionString = connectionString;
        }

        private DataTable _dataTable;
        private SqlTransaction _transaction;
        public SqlTransaction Transcation
        {
            get
            {
                return _transaction;
            }
            set
            {
                _transaction = value;
            }
        }
        /// <summary>
        /// 上一次执行结果的DataTable.
        /// </summary>
        public DataTable DataTable
        {
            get { return _dataTable; }
            private set { _dataTable = value; }
        }
        /// <summary>
        ///是否使用了事务
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use transaction]; otherwise, <c>false</c>.
        /// </value>
        public bool UseTransaction
        {
            get
            {
                return this.Transcation != null && this.Transcation.Connection != null;
            }
        }

        #region Methods
        public SqlTransaction OpenTransaction(SqlConnection connection)
        {
            this._transaction = connection.BeginTransaction();
            return _transaction;
        }
        public void CommitTransaction()
        {
            if (_transaction == null)
                throw new ApplicationException("提交事务时，事务不能为空!");
            _transaction.Commit();
        }
        public void RollBackTransaction()
        {
            if (_transaction == null)
                throw new ApplicationException("提交事务时，事务不能为空!");
            _transaction.Rollback();
        }
        public void Tandem(DataBase data)
        {
            this.Transcation = data.Transcation;
        }
        /// <summary>
        /// 执行SQL返回一个数据集.
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, params SqlParameter[] cmdParams)
        {
            if (UseTransaction)
                return ExecuteDataTable(Transcation, CommandType.Text, sql, cmdParams);
            else
                return ExecuteDataTable(CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行SQL返回一个数据集.
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(SqlTransaction transaction, string sql, params SqlParameter[] cmdParams)
        {
            return ExecuteDataTable(transaction, CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行命令返回一个数据集.
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return this.DataTable = SqlHelper.ExecuteDataset(_connectionString, cmdType, cmdText, cmdParams).Tables[0];
        }
        /// <summary>
        /// 执行命令返回一个数据集.
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return this.DataTable = SqlHelper.ExecuteDataset(transaction, cmdType, cmdText, cmdParams).Tables[0];
        }
        /// <summary>
        /// 执行命令返回一个数据集.
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return  SqlHelper.ExecuteDataset(_connectionString, cmdType, cmdText, cmdParams);
        }
        /// <summary>
        /// 执行命令返回一个数据集.
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return SqlHelper.ExecuteDataset(transaction, cmdType, cmdText, cmdParams);
        }
        /// <summary>
        /// 执行SQL语句返回受影响的行数.
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params SqlParameter[] cmdParams)
        {
            if (UseTransaction)
                return ExecuteNonQuery(this.Transcation, CommandType.Text, sql, cmdParams);
            else
                return ExecuteNonQuery(CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行SQL语句返回受影响的行数.
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(SqlTransaction transaction, string sql, params SqlParameter[] cmdParams)
        {
            return ExecuteNonQuery(transaction, CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行命令返回受影响的行数
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return SqlHelper.ExecuteNonQuery(_connectionString, cmdType, cmdText, cmdParams);
        }
        /// <summary>
        /// 执行命令返回受影响的行数
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return SqlHelper.ExecuteNonQuery(transaction, cmdType, cmdText, cmdParams);
        }
        /// <summary>
        /// 执行SQL返回结果集中第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params SqlParameter[] cmdParams)
        {
            if (!UseTransaction)
                return ExecuteScalar(CommandType.Text, sql, cmdParams);
            else
                return ExecuteScalar(this.Transcation, CommandType.Text, sql, cmdParams);

        }
        /// <summary>
        /// 执行SQL返回结果集中第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public object ExecuteScalar(SqlTransaction transaction, string sql, params SqlParameter[] cmdParams)
        {
            return ExecuteScalar(transaction, CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行命令返回结果集中第一行第一列
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return SqlHelper.ExecuteScalar(_connectionString, cmdType, cmdText, cmdParams);
        }
        /// <summary>
        /// 执行命令返回结果集中第一行第一列
        /// </summary>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public object ExecuteScalar(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams)
        {
            return SqlHelper.ExecuteScalar(transaction, cmdType, cmdText, cmdParams);
        }

        /// <summary>
        /// 执行SQL返回第一个对象，数据库无结果返回null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public T ExecuteModel<T>(string sql, params SqlParameter[] cmdParams) where T : class
        {
            if (!UseTransaction)
                return ExecuteModel<T>(CommandType.Text, sql, cmdParams);
            else
                return ExecuteModel<T>(this.Transcation, CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行命令返回第一个对象，数据库无结果返回null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public T ExecuteModel<T>(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams) where T : class
        {
            var entity = ModelParser<T>.ParseModel(ExecuteDataTable(cmdType, cmdText, cmdParams));
            return entity;
        }
        public T ExecuteModel<T>(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams) where T : class
        {
            var entity = ModelParser<T>.ParseModel(ExecuteDataTable(transaction, cmdType, cmdText, cmdParams));
            return entity;
        }
        /// <summary>
        /// 执行SQL返回泛型集合.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">SQL语句.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteList<T>(string sql, params SqlParameter[] cmdParams) where T : class
        {
            if (!UseTransaction)
                return ExecuteList<T>(CommandType.Text, sql, cmdParams);
            else
                return ExecuteList<T>(this.Transcation, CommandType.Text, sql, cmdParams);
        }
        /// <summary>
        /// 执行命令返回泛型集合.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteList<T>(CommandType cmdType, string cmdText, params SqlParameter[] cmdParams) where T : class
        {
            var dt = SqlHelper.ExecuteDataset(_connectionString, cmdType, cmdText, cmdParams).Tables[0];
            DataTable = dt;
            return ModelParser<T>.ParseModels(dt);
        }
        /// <summary>
        /// 执行命令返回泛型集合.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction">串联事务.</param>
        /// <param name="cmdType">命令类型.</param>
        /// <param name="cmdText">命令.</param>
        /// <param name="cmdParams">SQL参数.</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteList<T>(SqlTransaction transaction, CommandType cmdType, string cmdText, params SqlParameter[] cmdParams) where T : class
        {
            var dt = SqlHelper.ExecuteDataset(transaction, cmdType, cmdText, cmdParams).Tables[0];
            DataTable = dt;
            return ModelParser<T>.ParseModels(dt);
        }

        public void ExecuteSqlTranWithSqlBulkCopy(string tableName, DataTable dt)
        {
            using (SqlBulkCopy sqlbulkcopy = BuildBulk())
            {
                sqlbulkcopy.DestinationTableName = tableName;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                }
                sqlbulkcopy.WriteToServer(dt);
            }
        }

        private SqlBulkCopy BuildBulk()
        {
            if (UseTransaction)
                return new SqlBulkCopy(this.Transcation.Connection, SqlBulkCopyOptions.TableLock, Transcation);
            else
                return new SqlBulkCopy(this._connectionString, SqlBulkCopyOptions.UseInternalTransaction);
        }

        public void ExecuteWithTransaction(Action act)
        {
            if (!UseTransaction)
                ExecuteInNewTransaction(act);
            else
                try
                {
                    act();
                }
                finally
                {
                    this._transaction = null;
                }
        }
        private void ExecuteInNewTransaction(Action act)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlTransaction trans = OpenTransaction(connection))
                    {
                        try
                        {
                            act();
                            CommitTransaction();
                        }
                        catch
                        {
                            RollBackTransaction();
                            throw;
                        }
                    }
                }
            }
            finally
            {
                this.Transcation = null;
            }
        }
        #endregion
    }
}
