using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	public class ConsoleProcess
	{
		private List<string> outputLines = new List<string>();
		private Process process;



		public ConsoleProcess(string workingDirectory, string executableName, string parameters)
		{
			process = new Process();
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.OutputDataReceived += new DataReceivedEventHandler(HandleOutput);
			process.ErrorDataReceived += new DataReceivedEventHandler(HandleOutput);

			process.StartInfo.WorkingDirectory = workingDirectory;
			process.StartInfo.FileName = Path.Combine(workingDirectory, executableName);
			process.StartInfo.Arguments = parameters;

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}

		/// <summary>
		/// Kill a process, and all of its children, grandchildren, etc.
		/// https://stackoverflow.com/questions/5901679/kill-process-tree-programmatically-in-c-sharp
		/// Contango
		/// </summary>
		/// <param name="pid">Process ID.</param>
		private static void KillProcessAndChildren(int pid)
		{
			// Cannot close 'system idle process'.
			if (pid == 0)
			{
				return;
			}
			ManagementObjectSearcher searcher = new ManagementObjectSearcher
			  ("Select * From Win32_Process Where ParentProcessID=" + pid);
			ManagementObjectCollection moc = searcher.Get();
			foreach (ManagementObject mo in moc)
			{
				KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch
			{
				// Process already exited, or access denied, or something
			}
		}

		public void KillTree()
		{
			if (process != null)
				KillProcessAndChildren(process.Id);
		}


		public bool IsRunning
		{
			get
			{
				return process != null && !process.HasExited;
			}
		}

		public int ExitCode
		{
			get
			{
				if (process == null)
					throw new InvalidOperationException("Process has not been started");
				return process.ExitCode;
			}
		}

		public void WaitForExit()
		{
			if (process == null)
				return;
			process.WaitForExit();
		}

		public ICollection<string> Output
		{
			get
			{
				if (IsRunning)
					throw new InvalidOperationException("Cannot access output strings until process has come to a halt");
				return outputLines;
			}
		}



		private void HandleOutput(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if (!String.IsNullOrEmpty(outLine.Data))
			{
				outputLines.Add(outLine.Data);
			}
		}

	}
}
