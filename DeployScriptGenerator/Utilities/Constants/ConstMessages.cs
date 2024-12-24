using System;

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

    internal const string CMD_MSG_EXITING = "Exiting...";
    internal const string CMD_MSG_INVALID_RESPONSE =
        "Response \"{0}\" is invalid. Supported responses are 1, 2, and 3 as shown below.";
}
