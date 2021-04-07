using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;

namespace Kit.Sql.Base
{
    public abstract class TableQuery<T> : BaseTableQuery, IEnumerable<T>
    {
        public SqlBase Connection { get; private set; }



        protected Expression _where;
        protected List<Ordering> _orderBys;
        protected int? _limit;
        protected int? _offset;

        protected BaseTableQuery _joinInner;
        protected Expression _joinInnerKeySelector;
        protected BaseTableQuery _joinOuter;
        protected Expression _joinOuterKeySelector;
        protected Expression _joinSelector;

        protected Expression _selector;

        protected TableQuery(SqlBase conn, TableMapping table)
        {
            Connection = conn;
            Table = table;
        }

        public TableQuery(SqlBase conn)
        {
            Connection = conn;
            Table = Connection.GetMapping(typeof(T));
        }

        public abstract TableQuery<U> Clone<U>();

        /// <summary>
        /// Filters the query based on a predicate.
        /// </summary>
        public TableQuery<T> Where(Expression<Func<T, bool>> predExpr)
        {
            if (predExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)predExpr;
                var pred = lambda.Body;
                var q = Clone<T>();
                q.AddWhere(pred);
                return q;
            }
            else
            {
                throw new NotSupportedException("Must be a predicate");
            }
        }

        /// <summary>
        /// Delete all the rows that match this query.
        /// </summary>
        public int Delete(bool ShouldNotify = true)
        {
            return Delete(null, ShouldNotify);
        }

        /// <summary>
        /// Delete all the rows that match this query and the given predicate.
        /// </summary>
        public int Delete(Expression<Func<T, bool>> predExpr, bool ShouldNotify = true)
        {
            if (_limit.HasValue || _offset.HasValue)
                throw new InvalidOperationException("Cannot delete with limits or offsets");

            if (_where == null && predExpr == null)
                throw new InvalidOperationException("No condition specified");

            var pred = _where;

            if (predExpr != null && predExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)predExpr;
                pred = pred != null ? Expression.AndAlso(pred, lambda.Body) : lambda.Body;
            }

            List<object> args = new List<object>();
            List<Condition> conditions = new List<Condition>();
            var cmdText = "delete from \"" + Table.TableName + "\"";
            var w = CompileExpr(pred, args, conditions);
            cmdText += " where " + w.CommandText;

            object[] conditions_array = (this is SQLServerTableQuery<T> ? conditions.ToArray() : args.ToArray());
            if (this.Connection is Kit.Sql.Partitioned.SQLiteConnection partitioned)
            {
                partitioned.ToPartitionedDb(Table.TableName);
            }
            var command = Connection.CreateCommand(cmdText, conditions_array);

            int result = command.ExecuteNonQuery();
            if (ShouldNotify && result > 0 && Table.SyncDirection != SyncDirection.NoSync)
            {
                if (Connection is SQLiteConnection sqlite)
                {
                    sqlite.OnTableChanged((Kit.Sql.Sqlite.TableMapping)Table, NotifyTableChangedAction.Delete, this);
                }
            }
            return result;
        }

        /// <summary>
        /// Yields a given number of elements from the query and then skips the remainder.
        /// </summary>
        public TableQuery<T> Take(int n)
        {
            var q = Clone<T>();
            q._limit = n;
            return q;
        }

        /// <summary>
        /// Skips a given number of elements from the query and then yields the remainder.
        /// </summary>
        public TableQuery<T> Skip(int n)
        {
            var q = Clone<T>();
            q._offset = n;
            return q;
        }

        /// <summary>
        /// Returns the element at a given index
        /// </summary>
        public T ElementAt(int index)
        {
            return Skip(index).Take(1).First();
        }

        protected bool _deferred;
        public TableQuery<T> Deferred()
        {
            var q = Clone<T>();
            q._deferred = true;
            return q;
        }

        /// <summary>
        /// Order the query results according to a key.
        /// </summary>
        public TableQuery<T> OrderBy<U>(Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, true);
        }

        /// <summary>
        /// Order the query results according to a key.
        /// </summary>
        public TableQuery<T> OrderByDescending<U>(Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, false);
        }

        /// <summary>
        /// Order the query results according to a key.
        /// </summary>
        public TableQuery<T> ThenBy<U>(Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, true);
        }

        /// <summary>
        /// Order the query results according to a key.
        /// </summary>
        public TableQuery<T> ThenByDescending<U>(Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, false);
        }

        TableQuery<T> AddOrderBy<U>(Expression<Func<T, U>> orderExpr, bool asc)
        {
            if (orderExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)orderExpr;

                MemberExpression mem = null;

                var unary = lambda.Body as UnaryExpression;
                if (unary != null && unary.NodeType == ExpressionType.Convert)
                {
                    mem = unary.Operand as MemberExpression;
                }
                else
                {
                    mem = lambda.Body as MemberExpression;
                }

                if (mem != null && (mem.Expression.NodeType == ExpressionType.Parameter))
                {
                    var q = Clone<T>();
                    if (q._orderBys == null)
                    {
                        q._orderBys = new List<Ordering>();
                    }
                    q._orderBys.Add(new Ordering
                    {
                        ColumnName = Table.FindColumnWithPropertyName(mem.Member.Name).Name,
                        Ascending = asc
                    });
                    return q;
                }
                else
                {
                    throw new NotSupportedException("Order By does not support: " + orderExpr);
                }
            }
            else
            {
                throw new NotSupportedException("Must be a predicate");
            }
        }

        private void AddWhere(Expression pred)
        {
            if (_where == null)
            {
                _where = pred;
            }
            else
            {
                _where = Expression.AndAlso(_where, pred);
            }
        }

        ///// <summary>
        ///// Performs an inner join of two queries based on matching keys extracted from the elements.
        ///// </summary>
        //public TableQuery<TResult> Join<TInner, TKey, TResult> (
        //	TableQuery<TInner> inner,
        //	Expression<Func<T, TKey>> outerKeySelector,
        //	Expression<Func<TInner, TKey>> innerKeySelector,
        //	Expression<Func<T, TInner, TResult>> resultSelector)
        //{
        //	var q = new TableQuery<TResult> (Connection, Connection.GetMapping (typeof (TResult))) {
        //		_joinOuter = this,
        //		_joinOuterKeySelector = outerKeySelector,
        //		_joinInner = inner,
        //		_joinInnerKeySelector = innerKeySelector,
        //		_joinSelector = resultSelector,
        //	};
        //	return q;
        //}

        // Not needed until Joins are supported
        // Keeping this commented out forces the default Linq to objects processor to run
        //public TableQuery<TResult> Select<TResult> (Expression<Func<T, TResult>> selector)
        //{
        //	var q = Clone<TResult> ();
        //	q._selector = selector;
        //	return q;
        //}

        protected abstract CommandBase GenerateCommand(string selectionList);


        protected class CompileResult
        {
            public string CommandText { get; set; }

            public object Value { get; set; }
            public Condition CurrentCondition { get; set; }
        }

        protected CompileResult CompileExpr(Expression expr, List<object> queryArgs, List<Condition> conditions)
        {
            if (expr == null)
            {
                throw new NotSupportedException("Expression is NULL");
            }
            else if (expr is BinaryExpression)
            {
                var bin = (BinaryExpression)expr;

                // VB turns 'x=="foo"' into 'CompareString(x,"foo",true/false)==0', so we need to unwrap it
                // http://blogs.msdn.com/b/vbteam/archive/2007/09/18/vb-expression-trees-string-comparisons.aspx
                if (bin.Left.NodeType == ExpressionType.Call)
                {
                    var call = (MethodCallExpression)bin.Left;
                    if (call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators"
                        && call.Method.Name == "CompareString")
                        bin = Expression.MakeBinary(bin.NodeType, call.Arguments[0], call.Arguments[1]);
                }


                var leftr = CompileExpr(bin.Left, queryArgs, conditions);
                var rightr = CompileExpr(bin.Right, queryArgs, conditions);

                if (leftr.CurrentCondition != null && !leftr.CurrentCondition.IsComplete)
                {
                    Condition joinedCondition = new Condition(leftr.CurrentCondition, rightr.CurrentCondition);
                    if (joinedCondition.IsComplete)
                    {
                        conditions.Add(joinedCondition);
                        conditions.Remove(rightr.CurrentCondition);
                        conditions.Remove(leftr.CurrentCondition);
                    }
                }


                //If either side is a parameter and is null, then handle the other side specially (for "is null"/"is not null")
                string text;
                if (leftr.CommandText == "?" && leftr.Value == null)
                    text = CompileNullBinaryExpression(bin, rightr);
                else if (rightr.CommandText == "?" && rightr.Value == null)
                    text = CompileNullBinaryExpression(bin, leftr);
                else
                {
                    if (rightr.CommandText == "?" && this is SQLServerTableQuery<T>)
                    {
                        rightr.CommandText = "@" + leftr.CurrentCondition.ColumnName.Replace("\"", "");
                    }
                    text = "(" + leftr.CommandText + " " + GetSqlName(bin) + " " + rightr.CommandText + ")";
                }


                leftr.CurrentCondition = null;
                rightr.CurrentCondition = null;
                return new CompileResult { CommandText = text };
            }
            else if (expr.NodeType == ExpressionType.Not)
            {
                var operandExpr = ((UnaryExpression)expr).Operand;
                var opr = CompileExpr(operandExpr, queryArgs, conditions);
                object val = opr.Value;
                if (val is bool)
                    val = !((bool)val);

                if (this is SQLServerTableQuery<T>)
                {
                    Condition condition = new Condition(opr.CommandText, val);
                    return new CompileResult
                    {
                        CommandText = "(" + opr.CommandText + "==0)",
                        Value = val,
                        CurrentCondition = condition
                    };
                }
                else
                {
                    Condition condition = new Condition(opr.CommandText, val);
                    return new CompileResult
                    {
                        CommandText = "NOT(" + opr.CommandText + ")",
                        Value = val,
                        CurrentCondition = condition
                    };
                }


            }
            else if (expr.NodeType == ExpressionType.Call)
            {

                Condition CurrentCondition = null;
                var call = (MethodCallExpression)expr;
                var args = new CompileResult[call.Arguments.Count];
                var obj = call.Object != null ? CompileExpr(call.Object, queryArgs, conditions) : null;

                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = CompileExpr(call.Arguments[i], queryArgs, conditions);
                }

                var sqlCall = "";

                if (call.Method.Name == "Like" && args.Length == 2)
                {
                    sqlCall = "(" + args[0].CommandText + " like " + args[1].CommandText + ")";
                }
                else if (call.Method.Name == "Contains" && args.Length == 2)
                {
                    sqlCall = "(" + args[1].CommandText + " in " + args[0].CommandText + ")";
                }
                else if (call.Method.Name == "Contains" && args.Length == 1)
                {
                    if (call.Object != null && call.Object.Type == typeof(string))
                    {
                        if (this.Connection is SQLServerConnection)
                        {
                            sqlCall = "( (" + obj.CommandText + "LIKE '%'+@" + args[0].CurrentCondition.ColumnName + "+'%') )";
                        }
                        else
                        {
                            sqlCall = "( instr(" + obj.CommandText + "," + args[0].CommandText + ") >0 )";
                        }
                    }
                    else
                    {
                        if (this.Connection is SQLServerConnection)
                        {
                            sqlCall = "(@" + args[0].CurrentCondition.ColumnName + " in " + obj.CommandText + ")";
                        }
                        else
                        {
                            sqlCall = "(" + args[0].CommandText + " in " + obj.CommandText + ")";
                        }
                    }

                    CurrentCondition = args[0].CurrentCondition;
                }
                else if (call.Method.Name == "StartsWith" && args.Length >= 1)
                {
                    var startsWithCmpOp = StringComparison.CurrentCulture;
                    if (args.Length == 2)
                    {
                        startsWithCmpOp = (StringComparison)args[1].Value;
                    }
                    switch (startsWithCmpOp)
                    {
                        case StringComparison.Ordinal:
                        case StringComparison.CurrentCulture:
                            sqlCall = "( substr(" + obj.CommandText + ", 1, " + args[0].Value.ToString().Length + ") =  " + args[0].CommandText + ")";
                            break;
                        case StringComparison.OrdinalIgnoreCase:
                        case StringComparison.CurrentCultureIgnoreCase:
                            sqlCall = "(" + obj.CommandText + " like (" + args[0].CommandText + " || '%'))";
                            break;
                    }

                }
                else if (call.Method.Name == "EndsWith" && args.Length >= 1)
                {
                    var endsWithCmpOp = StringComparison.CurrentCulture;
                    if (args.Length == 2)
                    {
                        endsWithCmpOp = (StringComparison)args[1].Value;
                    }
                    switch (endsWithCmpOp)
                    {
                        case StringComparison.Ordinal:
                        case StringComparison.CurrentCulture:
                            sqlCall = "( substr(" + obj.CommandText + ", length(" + obj.CommandText + ") - " + args[0].Value.ToString().Length + "+1, " + args[0].Value.ToString().Length + ") =  " + args[0].CommandText + ")";
                            break;
                        case StringComparison.OrdinalIgnoreCase:
                        case StringComparison.CurrentCultureIgnoreCase:
                            sqlCall = "(" + obj.CommandText + " like ('%' || " + args[0].CommandText + "))";
                            break;
                    }
                }
                else if (call.Method.Name == "Equals" && args.Length == 1)
                {
                    sqlCall = "(" + obj.CommandText + " = (" + args[0].CommandText + "))";
                }
                else if (call.Method.Name == "ToLower")
                {
                    sqlCall = "(lower(" + obj.CommandText + "))";
                }
                else if (call.Method.Name == "ToUpper")
                {
                    sqlCall = "(upper(" + obj.CommandText + "))";
                }
                else if (call.Method.Name == "Replace" && args.Length == 2)
                {
                    sqlCall = "(replace(" + obj.CommandText + "," + args[0].CommandText + "," + args[1].CommandText + "))";
                }
                else if (call.Method.Name == "IsNullOrEmpty" && args.Length == 1)
                {
                    if (this.Connection is SQLServerConnection)
                    {
                        sqlCall = "(@" + args[0].CurrentCondition.ColumnName + " is null or @" + args[0].CurrentCondition.ColumnName + " ='' )";
                    }
                    else
                    {
                        sqlCall = "(" + args[0].CommandText + " is null or" + args[0].CommandText + " ='' )";
                    }

                    CurrentCondition = args[0].CurrentCondition;
                }
                else
                {
                    sqlCall = call.Method.Name.ToLower() + "(" + string.Join(",", args.Select(a => a.CommandText).ToArray()) + ")";
                }
                return new CompileResult { CommandText = sqlCall, CurrentCondition = CurrentCondition };

            }
            else if (expr.NodeType == ExpressionType.Constant)
            {
                var c = (ConstantExpression)expr;
                queryArgs.Add(c.Value);
                var condition = new Condition(c.Value);
                conditions.Add(condition);
                return new CompileResult
                {
                    CommandText = "?",
                    Value = c.Value,
                    CurrentCondition = condition
                };
            }
            else if (expr.NodeType == ExpressionType.Convert)
            {
                var u = (UnaryExpression)expr;
                var ty = u.Type;
                var valr = CompileExpr(u.Operand, queryArgs, conditions);

                var truvalue = valr.Value != null ? ConvertTo(valr.Value, ty) : null;
                if (truvalue is int enuTruvalue)
                {
                    if (u.Operand.Type.IsEnum)
                    {
                        if (u.Operand.Type.IsDefined(typeof(StoreAsTextAttribute), false))
                        {
                            string text_enum = Enum.GetName(u.Operand.Type, enuTruvalue);
                            if (!string.IsNullOrEmpty(text_enum))
                            {
                                truvalue = text_enum;
                            }
                        }

                    }
                }
                return new CompileResult
                {
                    CommandText = valr.CommandText,
                    Value = truvalue,
                    CurrentCondition = new Condition(valr.CommandText.Replace("\"", String.Empty), truvalue)
                };
            }
            else if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var mem = (MemberExpression)expr;

                var paramExpr = mem.Expression as ParameterExpression;
                if (paramExpr == null)
                {
                    var convert = mem.Expression as UnaryExpression;
                    if (convert != null && convert.NodeType == ExpressionType.Convert)
                    {
                        paramExpr = convert.Operand as ParameterExpression;
                    }
                }

                if (paramExpr != null)
                {
                    //
                    // This is a column of our table, output just the column name
                    // Need to translate it if that column name is mapped
                    //
                    var columnName = Table.FindColumnWithPropertyName(mem.Member.Name).Name;
                    return new CompileResult
                    {
                        CommandText = "\"" + columnName + "\"",
                        CurrentCondition = new Condition(columnName)
                    };
                }
                else
                {
                    object obj = null;
                    if (mem.Expression != null)
                    {
                        var r = CompileExpr(mem.Expression, queryArgs, conditions);
                        if (r.Value == null)
                        {
                            throw new NotSupportedException("Member access failed to compile expression");
                        }
                        if (r.CommandText == "?")
                        {
                            int remove_index = queryArgs.Count - 1;
                            queryArgs.RemoveAt(remove_index);
                            conditions.RemoveAt(remove_index);
                        }
                        obj = r.Value;
                    }

                    //
                    // Get the member value
                    //
                    object val = null;
                    string fieldname = string.Empty;
                    if (mem.Member is PropertyInfo)
                    {
                        var m = (PropertyInfo)mem.Member;
                        val = m.GetValue(obj, null);
                        fieldname = m.Name;
                    }
                    else if (mem.Member is FieldInfo)
                    {
                        var m = (FieldInfo)mem.Member;
                        val = m.GetValue(obj);
                        fieldname = m.Name;
                    }
                    else
                    {
                        throw new NotSupportedException("MemberExpr: " + mem.Member.GetType());
                    }

                    //
                    // Work special magic for enumerables
                    //
                    if (val != null && val is System.Collections.IEnumerable && !(val is string) && !(val is System.Collections.Generic.IEnumerable<byte>))
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.Append("(");
                        var head = "";
                        foreach (var a in (System.Collections.IEnumerable)val)
                        {
                            queryArgs.Add(a);
                            sb.Append(head);
                            sb.Append("?");
                            head = ",";
                        }
                        sb.Append(")");
                        return new CompileResult
                        {
                            CommandText = sb.ToString(),
                            Value = val
                        };
                    }
                    else
                    {
                        var con = new Condition(fieldname, val);
                        queryArgs.Add(val);
                        conditions.Add(con);
                        return new CompileResult
                        {
                            CommandText = "?",
                            Value = val,
                            CurrentCondition = con
                        };
                    }
                }
            }
            throw new NotSupportedException("Cannot compile: " + expr.NodeType.ToString());
        }


        protected static object ConvertTo(object obj, Type t)
        {
            Type nut = Nullable.GetUnderlyingType(t);

            if (nut != null)
            {
                if (obj == null)
                    return null;
                return Convert.ChangeType(obj, nut);
            }
            else
            {
                return Convert.ChangeType(obj, t);
            }
        }

        /// <summary>
        /// Compiles a BinaryExpression where one of the parameters is null.
        /// </summary>
        /// <param name="expression">The expression to compile</param>
        /// <param name="parameter">The non-null parameter</param>
        protected string CompileNullBinaryExpression(BinaryExpression expression, CompileResult parameter)
        {
            if (expression.NodeType == ExpressionType.Equal)
                return "(" + parameter.CommandText + " is ?)";
            else if (expression.NodeType == ExpressionType.NotEqual)
                return "(" + parameter.CommandText + " is not ?)";
            else if (expression.NodeType == ExpressionType.GreaterThan
                     || expression.NodeType == ExpressionType.GreaterThanOrEqual
                     || expression.NodeType == ExpressionType.LessThan
                     || expression.NodeType == ExpressionType.LessThanOrEqual)
                return "(" + parameter.CommandText + " < ?)"; // always false
            else
                throw new NotSupportedException("Cannot compile Null-BinaryExpression with type " + expression.NodeType.ToString());
        }

        protected string GetSqlName(Expression expr)
        {
            var n = expr.NodeType;
            if (n == ExpressionType.GreaterThan)
                return ">";
            else if (n == ExpressionType.GreaterThanOrEqual)
            {
                return ">=";
            }
            else if (n == ExpressionType.LessThan)
            {
                return "<";
            }
            else if (n == ExpressionType.LessThanOrEqual)
            {
                return "<=";
            }
            else if (n == ExpressionType.And)
            {
                return "&";
            }
            else if (n == ExpressionType.AndAlso)
            {
                return "and";
            }
            else if (n == ExpressionType.Or)
            {
                return "|";
            }
            else if (n == ExpressionType.OrElse)
            {
                return "or";
            }
            else if (n == ExpressionType.Equal)
            {
                return "=";
            }
            else if (n == ExpressionType.NotEqual)
            {
                return "!=";
            }
            else
            {
                throw new NotSupportedException("Cannot get SQL for: " + n);
            }
        }

        /// <summary>
        /// Execute SELECT COUNT(*) on the query
        /// </summary>
        public int Count()
        {
            return GenerateCommand("count(*)").ExecuteScalar<int>();
        }

        /// <summary>
        /// Execute SELECT COUNT(*) on the query with an additional WHERE clause.
        /// </summary>
        public int Count(Expression<Func<T, bool>> predExpr)
        {
            return Where(predExpr).Count();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_deferred)
                return GenerateCommand("*").ExecuteQuery<T>().GetEnumerator();

            return GenerateCommand("*").ExecuteDeferredQuery<T>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Queries the database and returns the results as a List.
        /// </summary>
        public List<T> ToList()
        {
            return GenerateCommand("*").ExecuteQuery<T>();
        }

        /// <summary>
        /// Queries the database and returns the results as an array.
        /// </summary>
        public T[] ToArray()
        {
            return GenerateCommand("*").ExecuteQuery<T>().ToArray();
        }

        /// <summary>
        /// Returns the first element of this query.
        /// </summary>
        public T First()
        {
            var query = Take(1);
            return query.ToList().First();
        }

        /// <summary>
        /// Returns the first element of this query, or null if no element is found.
        /// </summary>
        public T FirstOrDefault()
        {
            var query = Take(1);
            return query.ToList().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first element of this query that matches the predicate.
        /// </summary>
        public T First(Expression<Func<T, bool>> predExpr)
        {
            return Where(predExpr).First();
        }

        /// <summary>
        /// Returns the first element of this query that matches the predicate, or null
        /// if no element is found.
        /// </summary>
        public T FirstOrDefault(Expression<Func<T, bool>> predExpr)
        {
            return Where(predExpr).FirstOrDefault();
        }
    }
}