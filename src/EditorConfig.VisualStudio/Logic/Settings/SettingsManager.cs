﻿using EditorConfig.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Linq;
using EditorConfig.VisualStudio.Helpers;

namespace EditorConfig.VisualStudio.Logic.Settings
{
    internal class SettingsManager : IDisposable
    {
        internal FileConfiguration _settings = null;
        internal FileConfiguration Settings { get { return _settings; }}
        private readonly IWpfTextView _view;
        private LocalSettings _localSettings;
        private readonly ErrorListProvider _messageList;
        private ErrorTask _message;

        internal SettingsManager(IWpfTextView view, ITextDocument document, ErrorListProvider messageList)
        {
            _view = view;
            _messageList = messageList;
            _message = null;

            LoadSettings(document.FilePath);
        }

        /// <summary>
        /// Loads the settings for the given file path
        /// </summary>
        internal void LoadSettings(string path)
        {
            ClearMessage();
            _settings = null;

            // Prevent parsing of internet-located documents,
            // or documents that do not have proper paths.
            if (path.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || path.Equals("Temp.txt"))
                return;

            try
            {
                if (!ConfigLoader.TryLoad(path, out _settings))
                    return;

                _localSettings = new LocalSettings(_view, Settings);
                _localSettings.Apply();
            }
            catch (Exception e)
            {
                ShowError(path, "EditorConfig core error: " + e.Message);
            }
        }

        /// <summary>
        /// Adds an error message to the Visual Studio tasks pane
        /// </summary>
        private void ShowError(string path, string text)
        {
            _message = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Error,
                Category = TaskCategory.Comments,
                Document = path,
                Line = 0,
                Column = 0,
                Text = text
            };

            _messageList.Tasks.Add(_message);
            _messageList.Show();
        }

        /// <summary>
        /// Removes the file's messages, if any
        /// </summary>
        private void ClearMessage()
        {
            if (_message != null)
                _messageList.Tasks.Remove(_message);
            _message = null;
        }

        public void Dispose()
        {
            ClearMessage();
        }
    }
}
