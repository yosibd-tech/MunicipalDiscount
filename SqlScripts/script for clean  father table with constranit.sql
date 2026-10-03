SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @table  sysname       = N'dbo.tbClients';
DECLARE @drop   nvarchar(max) = N'',
        @create nvarchar(max) = N'';

SELECT
    @drop = @drop + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(fk.schema_id)) + N'.'
          + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
          + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10),
    @create = @create + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(fk.schema_id)) + N'.'
            + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
            + N' WITH CHECK ADD CONSTRAINT ' + QUOTENAME(fk.name) + N' FOREIGN KEY ('
            + STUFF((SELECT N',' + QUOTENAME(COL_NAME(c.parent_object_id, c.parent_column_id))
                     FROM sys.foreign_key_columns c
                     WHERE c.constraint_object_id = fk.object_id
                     ORDER BY c.constraint_column_id
                     FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, N'')
            + N') REFERENCES ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.referenced_object_id)) + N'.'
            + QUOTENAME(OBJECT_NAME(fk.referenced_object_id)) + N' ('
            + STUFF((SELECT N',' + QUOTENAME(COL_NAME(c.referenced_object_id, c.referenced_column_id))
                     FROM sys.foreign_key_columns c
                     WHERE c.constraint_object_id = fk.object_id
                     ORDER BY c.constraint_column_id
                     FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, N'')
            + N');' + CHAR(10)
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID(@table);

PRINT @drop;
PRINT @create;   -- save this text before executing

BEGIN TRAN;
    EXEC sp_executesql @drop;
    EXEC(N'TRUNCATE TABLE ' + @table);
    EXEC sp_executesql @create;
COMMIT;