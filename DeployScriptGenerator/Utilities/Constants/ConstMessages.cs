namespace DeployScriptGenerator.Utilities.Constants;

public static class ConstMessages
{
    internal const string CMD_MSG_WELCOME =
        @"
#======================================================================#
#                            Welcome to                                #
#                        DeployScriptGenerator!                        #
#======================================================================#
#                                                                      #
#        Menu:                                                         #
#            1. Generate Sample JSON Configs                           #
#            2. Generate Deploy Scripts                                #
#            3. Exit                                                   #
#                                                                      #
#======================================================================#
";

    internal const string EXITING = "Exiting...";
    internal const string INVALID_RESPONSE =
        "Response \"{0}\" is invalid. Supported responses are 1, 2, and 3 as shown below.";
}

internal static class ConstQueries
{
    internal const string FETCH_TABLES =
        @"
            WITH table_info AS (
                SELECT '{{schema_name}}' AS schema_name, '{{table_name}}' AS table_name
            ),
            primary_keys AS (
                SELECT
                    kcu.column_name
                FROM
                    information_schema.table_constraints AS tc
                JOIN
                    information_schema.key_column_usage AS kcu
                ON
                    tc.constraint_name = kcu.constraint_name
                WHERE
                    tc.table_schema = (SELECT schema_name FROM table_info) 
                    AND tc.table_name = (SELECT table_name FROM table_info) 
                    AND tc.constraint_type = 'PRIMARY KEY'
            ),
            column_definitions AS (
                SELECT
                    column_name,
                    CASE 
                        WHEN data_type = 'bigint' AND column_name IN (SELECT column_name FROM primary_keys) THEN 
                            'BIGSERIAL'
                        WHEN data_type = 'character varying' THEN 
                            'VARCHAR(' || character_maximum_length || ')'
                        WHEN data_type = 'character' THEN 
                            'CHAR(' || character_maximum_length || ')'
                        WHEN data_type = 'numeric' THEN 
                            'NUMERIC(' || numeric_precision || ',' || numeric_scale || ')'
                        ELSE 
                            data_type
                    END AS data_type,
                    is_nullable
                FROM
                    information_schema.columns
                WHERE
                    table_schema = (SELECT schema_name FROM table_info) 
                    AND 
                    table_name = (SELECT table_name FROM table_info)
            ),
            constraints AS (
                SELECT
                    tc.constraint_type,
                    kcu.column_name
                FROM
                    information_schema.table_constraints AS tc
                JOIN
                    information_schema.key_column_usage AS kcu
                    ON
                        tc.constraint_name = kcu.constraint_name
                WHERE
                    tc.table_schema = (SELECT schema_name FROM table_info) 
                    AND 
                    tc.table_name = (SELECT table_name FROM table_info)
            )
            SELECT
                'CREATE TABLE ' || (SELECT schema_name FROM table_info) || '.' || (SELECT table_name FROM table_info) || ' (' ||
                string_agg(cd.column_name || ' ' || cd.data_type || 
                        CASE 
                            WHEN cd.is_nullable = 'NO' THEN ' NOT NULL' 
                            ELSE ' NULL' 
                        END, ', ') || 
                CASE 
                    WHEN COUNT(c.constraint_type) > 0 THEN ', ' || string_agg('CONSTRAINT ' || c.constraint_type || ' (' || c.column_name || ')', ', ')
                    ELSE ''
                END || ');' AS create_table_statement
            FROM
                column_definitions cd
            LEFT JOIN
                constraints c 
                ON 
                    cd.column_name = c.column_name
            ;
        ";

    internal const string FETCH_TABLES_CONSTRAINS =
        @"
            SELECT
                c.conname AS name,
                'ALTER TABLE ' || n.nspname || '.' || c.conrelid::regclass || ' ADD CONSTRAINT ' || c.conname || ' ' || 
                CASE 
                    WHEN c.contype = 'p' THEN 'PRIMARY KEY (' || pg_get_constraintdef(c.oid) || ');'
                    WHEN c.contype = 'u' THEN 'UNIQUE (' || pg_get_constraintdef(c.oid) || ');'
                    WHEN c.contype = 'f' THEN 'FOREIGN KEY (' || pg_get_constraintdef(c.oid) || ');'
                    WHEN c.contype = 'c' THEN 'CHECK (' || pg_get_constraintdef(c.oid) || ');'
                    ELSE ''
                END AS script
            FROM
                pg_constraint c
            JOIN
                pg_namespace n 
                ON 
                    n.oid = c.connamespace
            WHERE
                n.nspname = '{{schema_name}}'
                AND 
                c.conrelid = '{{schema_name}}.{{table_name}}'::regclass
            ORDER BY
                c.conname
        ";

    internal const string FETCH_TABLES_INDEXES =
        @"
            SELECT
                i.relname AS name, 
                'CREATE INDEX ' || i.relname || ' ON ' || n.nspname || '.' || t.relname || ' (' || pg_get_indexdef(i.oid) || ');' AS script
            FROM
                pg_index idx
            JOIN
                pg_class t 
                ON 
                    t.oid = idx.indrelid
            JOIN
                pg_namespace n 
                ON 
                    n.oid = t.relnamespace
            JOIN
                pg_class i 
                ON 
                    i.oid = idx.indexrelid
            WHERE
                n.nspname = '{{schema_name}}'
                AND 
                t.relname = '{{table_name}}'
            ORDER BY
                i.relname;
        ";

    internal const string FETCH_TABLES_TRIGGERS =
        @"
            SELECT
                t.tgname AS name,
                pg_get_triggerdef(t.oid) AS script
            FROM
                pg_trigger t
            JOIN
                pg_class c 
                ON 
                    c.oid = t.tgrelid
            JOIN
                pg_namespace n 
                ON 
                    n.oid = c.relnamespace
            WHERE
                n.nspname = '{{schema_name}}' 
                AND
                c.relname = '{{table_name}}';
        ";

    internal const string FETCH_FUNCTIONS =
        @"
            SELECT 
                p.proname AS name,
                pg_get_functiondef(p.oid) AS script,
                COALESCE(array_length(string_to_array(pg_get_function_arguments(p.oid), ', '), 1), 0) AS pcount
            FROM 
                pg_proc p
            JOIN 
                pg_namespace n 
                ON 
                    n.oid = p.pronamespace
            WHERE  
                NOT EXISTS (
                    SELECT 1
                    FROM pg_catalog.pg_aggregate a
                    WHERE a.aggfnoid = p.oid
                )
                AND 
                n.nspname = '{{schema_name}}'
                AND 
                p.proname = '{{function_name}}'
            ORDER BY 
                1 ASC;
        ";
}
