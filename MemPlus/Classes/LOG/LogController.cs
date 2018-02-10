﻿using System;
using System.Collections.Generic;

namespace MemPlus.Classes.LOG
{
    /// <summary>
    /// Class containing methods to control logs
    /// </summary>
    public class LogController
    {
        #region Variables
        /// <summary>
        /// The list of available Log objects
        /// </summary>
        private readonly List<Log> _logList;
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate that will be called when a Log object was added
        /// </summary>
        /// <param name="l">The Log object that was added</param>
        internal delegate void LogAdded(Log l);
        /// <summary>
        /// Delegate that will be called when a Log object was removed
        /// </summary>
        /// <param name="l">The Log object that was deleted</param>
        internal delegate void LogDeleted(Log l);
        /// <summary>
        /// Delegate that will be called when all Log objects are removed
        /// </summary>
        internal delegate void LogsCleared();
        /// <summary>
        /// Delegate that will be called when a list of Log objects with a specific LogType were removed
        /// </summary>
        /// <param name="clearedList">The list of Log objects that were removed</param>
        internal delegate void LogTypeCleared(List<Log> clearedList);

        /// <summary>
        /// Method that will be called when a Log object was added
        /// </summary>
        internal LogAdded LogAddedEvent;
        /// <summary>
        /// Method that will be called when a Log object was removed
        /// </summary>
        internal LogDeleted LogDeletedEvent;
        /// <summary>
        /// Method that will be called when all Log objects were removed
        /// </summary>
        internal LogsCleared LogsClearedEvent;
        /// <summary>
        /// Method that will be called when a list of Log objects with a specific LogType were removed
        /// </summary>
        internal LogTypeCleared LogTypeClearedEvent;
        #endregion

        /// <summary>
        /// Initialize a new LogController object
        /// </summary>
        internal LogController()
        {
            _logList = new List<Log>();
        }

        /// <summary>
        /// Add a Log object to the list of logs
        /// </summary>
        /// <param name="l">The Log object that needs to be added</param>
        internal void AddLog(Log l)
        {
            _logList.Add(l);
            LogAddedEvent?.Invoke(l);
        }

        /// <summary>
        /// Remove a Log object from the list of logs
        /// </summary>
        /// <param name="l">The Log object that needs to be removed</param>
        internal void RemoveLog(Log l)
        {
            if (_logList.Contains(l))
            {
                _logList.Remove(l);
                LogDeletedEvent?.Invoke(l);
            }
            else
            {
                throw new ArgumentException("Log could not be found!");
            }
        }

        /// <summary>
        /// Clear all Log objects that have a specific LogType
        /// </summary>
        /// <param name="logType">The LogType that Log objects need to contain in order to be removed</param>
        internal void ClearLogs(LogType logType)
        {
            List<Log> deleted = new List<Log>();

            for (int i = _logList.Count - 1; i >= 0; i--)
            {
                if (_logList[i].LogType != logType) continue;
                deleted.Add(_logList[i]);
                _logList.RemoveAt(i);
            }

            LogTypeClearedEvent?.Invoke(deleted);
        }

        /// <summary>
        /// Clear all Log objects
        /// </summary>
        internal void ClearLogs()
        {
            _logList.Clear();
            LogsClearedEvent?.Invoke();
        }

        /// <summary>
        /// Retrieve the list of currently available Log objects
        /// </summary>
        /// <returns>The list of currently available Log objects</returns>
        internal List<Log> GetLogs()
        {
            return _logList;
        }

        /// <summary>
        /// Export logs to the disk
        /// </summary>
        /// <param name="path">The path where logs should be stored</param>
        /// <param name="logType">The type of logs that should be saved. Can be null if all logs should be saved</param>
        /// <param name="exportType">The type of export that should be performed</param>
        internal void Export(string path, LogType? logType, ExportType exportType)
        {
            List<Log> exportList;

            if (logType != null)
            {
                exportList = new List<Log>();
                foreach (Log l in _logList)
                {
                    if (l.LogType == logType)
                    {
                        exportList.Add(l);
                    }
                }
            }
            else
            {
                exportList = _logList;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (exportType)
            {
                case ExportType.Html:
                    LogExporter.ExportHtml(path, exportList);
                    break;
                default:
                    LogExporter.ExportTxt(path, exportList);
                    break;
                case ExportType.Csv:
                    LogExporter.ExportCsv(path, exportList);
                    break;
                case ExportType.Excel:
                    LogExporter.ExportExcel(path, exportList);
                    break;
            }
        }
    }
}
