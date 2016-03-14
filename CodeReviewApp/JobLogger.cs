using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace CodeReviewApp
{
    public class JobLogger
    {
        private static bool _logToFile;
        private static bool _logToConsole;
        private static bool _logMessage;
        private static bool _logWarning;
        private static bool _logError;
        private static bool _logToDatabase;
        private bool _initialized;
        public JobLogger(bool logToFile, bool logToConsole, bool logToDatabase, bool logMessage, bool logWarning, bool logError)
        {
            _logMessage = logMessage;
            _logWarning = logWarning;
            _logError = logError;   
            _logToDatabase = logToDatabase;
            _logToFile = logToFile;
            _logToConsole = logToConsole;
        }
        public void LogMessage(Enums.LogType logType, string message)
        {

            try {

                message = message.Trim();

                //Validations
                bool logValidation = false;

                switch (logType)
                {
                    case Enums.LogType.Message: logValidation = _logMessage; break;
                    case Enums.LogType.Warning: logValidation = _logWarning; break;
                    case Enums.LogType.Error: logValidation = _logError; break;
                }
          
                if (!_logToConsole && !_logToFile && !_logToDatabase)
                {
                    throw new Exception("Invalid configuration");
                }
                else if (!Enum.IsDefined(typeof(Enums.LogType), logType))
                {
                    throw new Exception("Error or Warning or Message must be specified");
                }
                else if (!logValidation)
                {
                    throw new Exception(string.Format("Log for the specified Type ({0}) is turned off", logType.ToString()));
                }
                else if (string.IsNullOrEmpty(message))
                {
                    throw new Exception("Message can't be empty");
                }              
                
                //Log to DataBase
                if (_logToDatabase)
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "LogMessage";
                        command.Parameters.AddWithValue("@LogTypeID", (short)logType);
                        command.Parameters.AddWithValue("@LogMessage", message);
                        command.ExecuteNonQuery();
                    }
                }

                string logLine = string.Format("({0}) [{1}]: {2}", DateTime.Now.ToString("h:mm:ss"), logType.ToString(), message);

                //Log to File
                if (_logToFile)
                {
                    string logText = string.Empty;
                    string logFileDirectory = ConfigurationManager.AppSettings["LogFileDirectory"] + "LogFile" + DateTime.Now.ToShortDateString().Replace("/", "") + ".txt";

                    if (File.Exists(logFileDirectory))
                    {
                        logText = File.ReadAllText(logFileDirectory);
                    }

                    logText = logText + logLine + Environment.NewLine;

                    File.WriteAllText(logFileDirectory, logText);
                }

                //Log to Console
                if (_logToConsole)
                {
                    ConsoleColor consoleColor;

                    switch (logType)
                    {
                        case Enums.LogType.Warning: consoleColor = ConsoleColor.Yellow; break;
                        case Enums.LogType.Error: consoleColor = ConsoleColor.Red; break;
                        default: consoleColor = ConsoleColor.White; break;
                    }

                    Console.ForegroundColor = consoleColor;

                    Console.WriteLine(DateTime.Now.ToShortDateString() + " - " + logLine);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
