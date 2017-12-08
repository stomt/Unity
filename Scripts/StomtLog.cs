﻿using System;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine;

namespace Stomt
{
	public class StomtLog
	{
		private StomtAPI _api;
		private Thread fileReadThread;

		private string logFileContent = null;
		private bool isLogFileReadComplete = false;

		public StomtLog (StomtAPI api)
		{
			this._api = api;
			this.fileReadThread = new Thread(LoadLogFileThread);
			this.fileReadThread.Start();
		}

		public bool hasCopletedLoading() {
			return this.isLogFileReadComplete;
		}

		public string getFileConent() {
			return this.logFileContent;
		}

		public void stopThread() {
			if (this.fileReadThread != null && !this.fileReadThread.IsAlive) {
				this.fileReadThread.Abort ();
			}
		}
			
		// PRIVATE HELPERS

		private void LoadLogFileThread()
		{
			logFileContent = ReadFile(GetLogFilePath());
			this.isLogFileReadComplete = true;
		}
			
		private string GetLogFilePath()
		{
			string logFilePath = "";
			//////////////////////////////////////////////////////////////////
			// Windows Paths
			//////////////////////////////////////////////////////////////////

			if(Application.platform == RuntimePlatform.WindowsEditor)
			{
				logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Unity\\Editor\\Editor.log";
			}

			if(Application.platform == RuntimePlatform.WindowsPlayer)
			{
				logFilePath = "_EXECNAME_Data_\\output_log.txt";
			}

			//////////////////////////////////////////////////////////////////
			// OSX Paths
			//////////////////////////////////////////////////////////////////

			if(Application.platform == RuntimePlatform.OSXEditor)
			{
				logFilePath = "~/Library/Logs/Unity/Editor.log";
			}

			if(Application.platform == RuntimePlatform.OSXPlayer)
			{
				logFilePath = "~/Library/Logs/Unity/Player.log";
			}

			//////////////////////////////////////////////////////////////////
			// Linux Paths
			//////////////////////////////////////////////////////////////////

			if(Application.platform == RuntimePlatform.LinuxEditor)
			{
				logFilePath = "~/.config/unity3d/CompanyName/ProductName/Editor.log";
			}

			if(Application.platform == RuntimePlatform.LinuxPlayer)
			{
				logFilePath = "~/.config/unity3d/CompanyName/ProductName/Player.log";
			}

			if(!string.IsNullOrEmpty(logFilePath))
			{
				if (File.Exists (logFilePath)) {
					return logFilePath;
				} else {
					Debug.Log ("does not exist" + logFilePath);
				}
			}

			return "";
		}

		private string ReadFile(string FilePath)
		{
			if (string.IsNullOrEmpty(FilePath)) {
				return null;
			}

			var fileInfo = new System.IO.FileInfo(FilePath);

			if (fileInfo.Length > 30000000) 
			{
				Debug.LogWarning("Log file too big. Size: " + fileInfo.Length + "Bytes. Path: " + FilePath);

				var track = this._api.initStomtTrack();
				track.event_category = "log";
				track.event_action = "tooBig";
				track.save ();

				return null; 
			}

			string FileCopyPath = FilePath + ".tmp.copy";

			// Copy File for reading an already opened file
			File.Copy(FilePath, FileCopyPath, true);

			// Read File
			StreamReader reader = new StreamReader(FileCopyPath);
			string content = reader.ReadToEnd();

			// Close stream and delete file copy
			reader.Close();
			File.Delete(FilePath + ".tmp.copy");

			return content;
		}
	}
}
