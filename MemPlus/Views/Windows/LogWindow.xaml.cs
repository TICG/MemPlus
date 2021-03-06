﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.UTILS;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public partial class LogWindow
    {
        #region Variables
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// The LogType that is currently being monitored
        /// </summary>
        private readonly LogType? _logType;
        /// <summary>
        /// A boolean to indicate whether automatic scrolling is enabled or not
        /// </summary>
        private bool _autoScroll;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new LogWindow object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add and view logs</param>
        /// <param name="logType">The LogType that is currently being monitored. Can be null if all logs should be monitored</param>
        internal LogWindow(LogController logController, LogType? logType)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing LogWindow"));

            _logType = logType;

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            FillLogView();

            _logController.LogAddedEvent += LogAddedEvent;
            _logController.LogsClearedEvent += LogsClearedEvent;
            _logController.LogDeletedEvent += LogDeletedEvent;
            _logController.LogTypeClearedEvent += LogTypeClearedEvent;

            _autoScroll = true;

            _logController.AddLog(new ApplicationLog("Done initializing LogWindow"));
        }

        /// <summary>
        /// Load the current properties into the GUI
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading LogWindow properties"));
            try
            {
                if (Properties.Settings.Default.WindowDragging)
                {
                    MouseDown += OnMouseDown;
                }
                else
                {
                    MouseDown -= OnMouseDown;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new ApplicationLog("Done loading LogWindow properties"));
        }

        /// <summary>
        /// Method that is called when the Window should be dragged
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The MouseButtonEventArgs</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Method that is called when all Log objects of a certain type have been cleared
        /// </summary>
        /// <param name="clearedList">The list of Log objects that were removed</param>
        private void LogTypeClearedEvent(List<Log> clearedList)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Log l in clearedList)
                {
                    LsvLogs.Items.Remove(l);
                }
            });
        }

        /// <summary>
        /// Fill the ListView will all current Log objects
        /// </summary>
        private void FillLogView()
        {
            foreach (Log l in _logController.GetLogs(_logType))
            {
                LsvLogs.Items.Add(l);
            }
        }

        /// <summary>
        /// Method that is called when a Log object was removed
        /// </summary>
        /// <param name="log">The Log object that was removed</param>
        private void LogDeletedEvent(Log log)
        {
            if (_logType != null && log.LogType != _logType) return;
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Remove(log);
            });
        }

        /// <summary>
        /// Method that is called when all logs were removed
        /// </summary>
        private void LogsClearedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Clear();
            });
        }

        /// <summary>
        /// Method that is called when a Log object was added
        /// </summary>
        /// <param name="log">The Log object that was added</param>
        private void LogAddedEvent(Log log)
        {
            if (_logType != null && log.LogType != _logType) return;
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Add(log);

                if (!_autoScroll) return;
                LsvLogs.ScrollIntoView(LsvLogs.Items[LsvLogs.Items.Count - 1]);
            });
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing LogWindow theme style"));
            GuiManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing LogWindow theme style"));
        }

        /// <summary>
        /// Method that is called when a scroll action happened in the ListView
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The ScrollEventArgs</param>
        private void LsvLogs_OnScroll(object sender, ScrollEventArgs e)
        {
            if (!(e.OriginalSource is ScrollBar sb)) return;
            if (sb.Orientation == Orientation.Horizontal) return;

            _autoScroll = Math.Abs(sb.Value - sb.Maximum) < 1;
        }

        /// <summary>
        /// Method that is called when all Log objects of a certain type should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs(_logType);
        }

        /// <summary>
        /// Method that is called when all Log objects of a certain type should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (Utils.ExportLogs(_logType, _logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when a Log object should be removed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvLogs.SelectedItems.Count == 0) return;
            _logController.RemoveLog(LsvLogs.SelectedItem as Log);
        }

        /// <summary>
        /// Method that is called when a Log object should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvLogs.SelectedItems.Count == 0) return;
            if (!(LsvLogs.SelectedItem is Log selectedLog)) return;

            try
            {
                Clipboard.SetText(selectedLog.Time + "\t" + selectedLog.Data);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the mouse wheel is used
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The MouseWheelEventArgs</param>
        private void LsvLogs_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _autoScroll = false;
        }
    }
}
