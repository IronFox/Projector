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
	/// <summary>
	/// Handler for processes normally executed in a console.
	/// Output is redirected and a visual console window is suppressed
	/// </summary>
	public class ConsoleProcess
	{
		private List<string> outputLines = new List<string>();
		private readonly Process process;


		/// <summary>
		/// Constructs the console process
		/// </summary>
		/// <param name="workingDirectory">Working directory of the created process</param>
		/// <param name="executableName">Executable name, relative to given working directory</param>
		/// <param name="runtimeArguments">Arguments passed to the started executable</param>
		/// <param name="priority">Priority to use for the started process</param>
		public ConsoleProcess(string workingDirectory, string executableName, string runtimeArguments, ProcessPriorityClass priority = ProcessPriorityClass.BelowNormal)
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
			process.StartInfo.Arguments = runtimeArguments;

			process.Start();
			process.PriorityClass = priority;
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

		/// <summary>
		/// Kills the local process and any child processes it may have created
		/// </summary>
		public void KillTree()
		{
			KillProcessAndChildren(process.Id);
		}


		/// <summary>
		/// Checks if the local process is still running
		/// </summary>
		public bool IsRunning
		{
			get
			{
				return !process.HasExited;
			}
		}

		/// <summary>
		/// Retrieves the exit code of the locally started process
		/// </summary>
		public int ExitCode
		{
			get
			{
				return process.ExitCode;
			}
		}

		/// <summary>
		/// Waits until the local process has exited
		/// </summary>
		public void WaitForExit()
		{
			process.WaitForExit();
		}

		/// <summary>
		/// Fetches the accumulated console output of the process.
		/// The local process must have exited
		/// </summary>
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
