using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LotteryVoteMVC.Models
{
    public class ModelParser<T> where T : class
    {
        public static IDictionary<int, Func<DataRow, T>> dic = new Dictionary<int, Func<DataRow, T>>();
        private static object _lockHelper = new object();
        public static Func<DataRow, T> GetParser()
        {
            var setValueMethod = typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object), typeof(object[]) });
            var getValueMethod = typeof(PropertyInfo).GetMethod("GetValue", new Type[] { typeof(object), typeof(object[]) });
            var getPropertyMethod = typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string) });
            var changeTypeMethod = typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) });

            ParameterExpression row = Expression.Parameter(typeof(DataRow), "Row");
            ParameterExpression model = Expression.Parameter(typeof(T), "model");
            ParameterExpression start = Expression.Parameter(typeof(int), "Start");
            ParameterExpression end = Expression.Parameter(typeof(int), "End");
            ParameterExpression column = Expression.Parameter(typeof(DataColumn), "column");
            ParameterExpression columnName = Expression.Parameter(typeof(string), "Column");
            ParameterExpression columnValue = Expression.Parameter(typeof(object), "columnValue");
            ParameterExpression property = Expression.Parameter(typeof(PropertyInfo), "property");

            LabelTarget lable = Expression.Label(typeof(int));

            BlockExpression block = Expression.Block(new[] { start, end, model, column, columnName, columnValue, property },
                Expression.Assign(start, Expression.Constant(0)),
                Expression.Assign(end, Expression.Property(Expression.Property(Expression.Property(row, "Table"), "Columns"), "Count")),
                Expression.Assign(model, Expression.New(typeof(T))),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(start, end),
                        Expression.Block(
                            Expression.Assign(column,
                                Expression.Property(Expression.Property(Expression.Property(row, "Table"), "Columns"), "Item", start)
                            ),
                            Expression.Assign(columnName,
                                Expression.Property(column, "ColumnName")
                            ),
                            Expression.Assign(property, Expression.Call(Expression.Constant(typeof(T)), getPropertyMethod, columnName)),
                            Expression.IfThen(Expression.NotEqual(property, Expression.Constant(null)),
                                Expression.Block(
                                    Expression.Assign(columnValue, Expression.Property(row, "Item", column)),
                                    Expression.Condition(Expression.Equal(columnValue, Expression.Constant(DBNull.Value)),
                                        Expression.Call(property, setValueMethod,
                                            model,
                                            Expression.Constant(null),
                                            Expression.NewArrayInit(typeof(object))),
                //Expression.Assign(
                //    Expression.Property(model, Expression.Lambda<Func<ParameterExpression, string>>(columnName).Compile()(columnName)),
                //    Expression.Call(null, changeTypeMethod,
                //        columnValue,
                //        Expression.Property(column, "DataType"))
                //)
                                        Expression.Call(property, setValueMethod,
                                            model,
                                            Expression.Call(null, changeTypeMethod,
                                                columnValue,
                                                Expression.Property(column, "DataType")),
                                            Expression.NewArrayInit(typeof(object))
                                        )
                                    )
                                )
                            ),
                            Expression.PostIncrementAssign(start)
                        ),
                        Expression.Break(lable, start)
                    ),
                    lable
                ),
                Expression.Block(
                   Expression.Assign(property, Expression.Call(Expression.Constant(typeof(T)), getPropertyMethod, Expression.Constant("DataRow"))),
                    Expression.IfThen(
                        Expression.NotEqual(property, Expression.Constant(null)),
                        Expression.Call(property, setValueMethod, model, row, Expression.NewArrayInit(typeof(object)))
                    )
                ),
                model
            );
            Expression<Func<DataRow, T>> lambda = Expression.Lambda<Func<DataRow, T>>(block, row);
            return lambda.Compile();
        }
        public static T ParseModel(DataRow row)
        {
            var key = row.Table.GetHashCode();
            if (!dic.ContainsKey(key))
            {
                lock (_lockHelper)
                {
                    if (!dic.ContainsKey(key))
                        dic.Add(key, GetParser());
                }
            }
            return dic[key](row);
        }
        public static T ParseModel(DataTable dt)
        {
            if (dt.Rows.Count > 0)
                return ParseModel(dt.Rows[0]);
            return default(T);
        }
        public static IEnumerable<T> ParseModels(DataTable dt)
        {
            return dt.AsEnumerable().Select(it => ParseModel(it));
        }
        public static IEnumerable<T> ParseModels(IEnumerable<DataRow> rows)
        {
            return rows.Select(it => ParseModel(it));
        }
    }
}
