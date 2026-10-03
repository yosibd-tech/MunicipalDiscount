/* כל הטבלאות שיש בהן עמודה עם "Asset" בשם */

SELECT  t.TABLE_SCHEMA                        AS [סכמה],
        t.TABLE_NAME                          AS [טבלה],
        c.COLUMN_NAME                         AS [עמודה],
        c.DATA_TYPE                           AS [סוג],
        c.CHARACTER_MAXIMUM_LENGTH            AS [אורך],
        c.NUMERIC_PRECISION                   AS [דיוק],
        c.NUMERIC_SCALE                       AS [ספרות],
        c.IS_NULLABLE                         AS [ריק מותר],
        c.ORDINAL_POSITION                    AS [מיקום],
        SUM(p.rows)                           AS [שורות בטבלה]
FROM        INFORMATION_SCHEMA.COLUMNS c
INNER  JOIN INFORMATION_SCHEMA.TABLES  t
        ON  t.TABLE_SCHEMA = c.TABLE_SCHEMA
       AND  t.TABLE_NAME   = c.TABLE_NAME
LEFT   JOIN sys.objects    o  ON o.name = t.TABLE_NAME
                             AND SCHEMA_NAME(o.schema_id) = t.TABLE_SCHEMA
LEFT   JOIN sys.partitions p  ON p.object_id = o.object_id
                             AND p.index_id IN (0, 1)
WHERE  c.COLUMN_NAME LIKE '%Asset%'
  AND  t.TABLE_TYPE = 'BASE TABLE'
GROUP BY t.TABLE_SCHEMA, t.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE,
         c.CHARACTER_MAXIMUM_LENGTH, c.NUMERIC_PRECISION, c.NUMERIC_SCALE,
         c.IS_NULLABLE, c.ORDINAL_POSITION
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION;
