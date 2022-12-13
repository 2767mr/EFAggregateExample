This is a small repo to reproduce an issue with Entity Framwork Core 7.0.1

## Program output
```sql
Navigation Join - executed on client:
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], [b].[BlogId], [p].[PostId], [p].[BlogId], [p].[ContentLength], [p].[Title], [p0].[Title], [p0].[PostId]
      FROM [Blogs] AS [b]
      LEFT JOIN [Posts] AS [p] ON [b].[BlogId] = [p].[BlogId]
      LEFT JOIN [Posts] AS [p0] ON [b].[BlogId] = [p0].[BlogId]
      ORDER BY [b].[BlogId], [p].[PostId]
[{"Url":"blogA","PostTitles":"_none_"},{"Url":"blogB","PostTitles":"postA, postB"}]

Navigation Concat - executed on client:
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], [b].[BlogId], [p].[PostId], [p].[BlogId], [p].[ContentLength], [p].[Title], [p0].[Title], [p0].[PostId]
      FROM [Blogs] AS [b]
      LEFT JOIN [Posts] AS [p] ON [b].[BlogId] = [p].[BlogId]
      LEFT JOIN [Posts] AS [p0] ON [b].[BlogId] = [p0].[BlogId]
      ORDER BY [b].[BlogId], [p].[PostId]
[{"Url":"blogA","PostTitles":"_none_"},{"Url":"blogB","PostTitles":"postApostB"}]

GroupBy Union Join - executed on server:
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (42ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], COALESCE(STRING_AGG([p].[Title], N', '), N'') AS [PostTitles]
      FROM [Posts] AS [p]
      INNER JOIN [Blogs] AS [b] ON [p].[BlogId] = [b].[BlogId]
      GROUP BY [b].[BlogId], [b].[Url]
      UNION
      SELECT [b0].[Url], N'_none_' AS [PostTitles]
      FROM [Blogs] AS [b0]
      WHERE (
          SELECT COUNT(*)
          FROM [Posts] AS [p0]
          WHERE [b0].[BlogId] = [p0].[BlogId]) = 0
[{"Url":"blogA","PostTitles":"_none_"},{"Url":"blogB","PostTitles":"postA, postB"}]

Navigation sum - executed on server:
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (17ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], [b].[BlogId], [p].[PostId], [p].[BlogId], [p].[ContentLength], [p].[Title], (
          SELECT COALESCE(SUM([p0].[ContentLength]), 0)
          FROM [Posts] AS [p0]
          WHERE [b].[BlogId] = [p0].[BlogId])
      FROM [Blogs] AS [b]
      LEFT JOIN [Posts] AS [p] ON [b].[BlogId] = [p].[BlogId]
      ORDER BY [b].[BlogId]
[{"Url":"blogA","TotalLength":0},{"Url":"blogB","TotalLength":3}]

GroupBy distinct sum - executed on server:
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (22ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], COALESCE(SUM(DISTINCT ([p].[ContentLength])), 0) AS [TotalLength]
      FROM [Posts] AS [p]
      INNER JOIN [Blogs] AS [b] ON [p].[BlogId] = [b].[BlogId]
      GROUP BY [b].[BlogId], [b].[Url]
[{"Url":"blogB","TotalLength":3}]

GroupBy Union distinct Join - exception:
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (237ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Url], COALESCE(STRING_AGG(DISTINCT ([p].[Title]), N', '), N'') AS [PostTitles]
      FROM [Posts] AS [p]
      INNER JOIN [Blogs] AS [b] ON [p].[BlogId] = [b].[BlogId]
      GROUP BY [b].[BlogId], [b].[Url]
      UNION
      SELECT [b0].[Url], N'_none_' AS [PostTitles]
      FROM [Blogs] AS [b0]
      WHERE (
          SELECT COUNT(*)
          FROM [Posts] AS [p0]
          WHERE [b0].[BlogId] = [p0].[BlogId]) = 0
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'BloggingContext'.
      Microsoft.Data.SqlClient.SqlException (0x80131904): Incorrect syntax near ','.
         at Microsoft.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__208_0(Task`1 result)
         at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
         at System.Threading.Tasks.Task.<>c.<.cctor>b__272_0(Object obj)
         at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
      --- End of stack trace from previous location ---
         at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
         at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
      --- End of stack trace from previous location ---
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
      ClientConnectionId:d0da65ea-d434-4b7a-83ab-0f9ea7a7beac
      Error Number:102,State:1,Class:15
      Microsoft.Data.SqlClient.SqlException (0x80131904): Incorrect syntax near ','.
         at Microsoft.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__208_0(Task`1 result)
         at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
         at System.Threading.Tasks.Task.<>c.<.cctor>b__272_0(Object obj)
         at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
      --- End of stack trace from previous location ---
         at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
         at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
      --- End of stack trace from previous location ---
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
      ClientConnectionId:d0da65ea-d434-4b7a-83ab-0f9ea7a7beac
      Error Number:102,State:1,Class:15
```