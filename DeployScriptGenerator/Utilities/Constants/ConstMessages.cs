namespace DeployScriptGenerator.Utilities.Constants;

public static class ConstMessages
{
    internal const string CMD_MSG_ERROR = "Error: {0} @ {1}";
    internal const string CMD_MSG_EXITING = "Exiting...";
    internal const string CMD_MSG_INV_RESP =
        "Response \"{0}\" is invalid. Supported responses are 1, 2, and 3 as shown below.";
    internal const string CMD_MSG_RESTART_APP = "Press the [ENTER] key to restart the application.";
    internal const string CMD_MSG_RETRY_OR_MAIN_MENU =
        "Press <r> to try again or press any other key to go back to the main menu?";
    internal const string CMD_MSG_WELCOME =
        @"
#======================================================================#
#                            Welcome to                                #
#                        DeployScriptGenerator!                        #
#======================================================================#
#                                                                      #
#        Menu:                                                         #
#            [1] : Generate Sample JSON Configs                        #
#            [2] : Generate Deploy Scripts                             #
#            [3] : Exit                                                #
#                                                                      #
#======================================================================#
";

    internal const string CONN_DB_ESTABLISHED =
        "Connection to database \"{0}\" has been established.";
    internal const string CONN_DB_FAIL = "Failed to create a database connection.";
    internal const string CONN_DB_SWITCH = "Switching to database [{0}].";
    internal const string CONN_DB_TRY = "Connecting to the database \"{0}\"...";

    internal const string DPY_SCRIPT_GEN_SUCCESS =
        "Deployment Scripts has been generated on Path: \"{0}\"";
    internal const string DPY_SCRIPT_START = "Generating deployment script...";

    internal const string FETCH_FUNC_COMPLETED = "[{0}] : Function processing completed.";
    internal const string FETCH_FUNC_END = "All functions have been processed successfully.";
    internal const string FETCH_FUNC_INSIDE_FUNC_COUNT =
        "{0}Found {1} functions inside function \"{3}\": [{4}].";
    internal const string FETCH_FUNC_INSIDE_FUNC_PROC = "{0}Processing function: {1}";
    internal const string FETCH_FUNC_INSIDE_FUNC_SKIP =
        "{0}[Function] : Function name \"{1}\" will not be processed.";
    internal const string FETCH_FUNC_NOT_EXIST = "{0}[{1}] : Function does not exist.";
    internal const string FETCH_FUNC_PROC_FOUND_COUNT = "{0}[{1}] : Found {2} function.";
    internal const string FETCH_FUNC_PROC_START = "{0}[{1}] : Processing function.";
    internal const string FETCH_FUNC_START = "\nFetching Functions...";

    internal const string FETCH_TBL_COMPLETED = "[{0}.{1}.{2}] : Table process completed.";
    internal const string FETCH_TBL_END = "All tables have been processed.";
    internal const string FETCH_TBL_GET_INFO = "[{0}.{1}.{2}] : Gathering table informations";
    internal const string FETCH_TBL_NOT_EXIST = "[{0}.{1}.{2}] : Table does not exist.";
    internal const string FETCH_TBL_PROP_FOUND_COUNT = "   [{0}] : Found {1} {0}.";
    internal const string FETCH_TBL_PROP_FUNC_FOUND_COUNT =
        "       Found {0} functions on {1} \"{2}\": [{3}].";
    internal const string FETCH_TBL_PROP_FUNC_PROC = "{0}Processing function: {1}";
    internal const string FETCH_TBL_PROP_NOT_FOUND = "   [{0}] : Does not have {0}.";
    internal const string FETCH_TBL_PROP_PROC = "     [{0}] : Processing {1}.";
    internal const string FETCH_TBL_START = "\nFetching Tables...";

    internal const string FILE_NAME_BLD = "{0}_{1}_{2}{3}_{4}_{5}.sql";
    internal const string REM_DIR = "Cleaning up directory on : {0}";

    internal const string SCR_PREP_ASK_PATH =
        "Please specify the location of the json configuration file:";
    internal const string SCR_PREP_PATH_CANNOT_EMPTY = "Configuration file path can not be empty.";
    internal const string SCR_PREP_FILE_NOT_FOUND = "The specified file does not exist.";
    internal const string SCR_PREP_READ_CFG_FILE = "Reading configuration file...";
    internal const string SCR_PREP_READ_CFG_FAIL = "The specified file could not be parsed.";

    internal const string SMPL_CFG_ASK_PATH =
        "Please specify the path of the sample json configuration file.";
    internal const string SMPL_CFG_DIR_NOT_FOUND = "Directory does not exist. Please try again.";
    internal const string SMPL_CFG_PATH_CANNOT_EMPTY = "Path can not be empty.";
    internal const string SMPL_CFG_SAVED =
        "Sample configuration json file successfully saved to: {0}";
}
