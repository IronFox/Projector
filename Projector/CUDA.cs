using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	/// <summary>
	/// Handler class for Nvidia CUDA
	/// </summary>
	public static class CUDA
	{
		private static List<string>? gpuCodes;
		private static string? version;

		private static string? RunProcess(string arguments)
		{

			Process myProcess = new Process();

			myProcess.StartInfo.FileName = "nvcc.exe";
			myProcess.StartInfo.Arguments = arguments;
			myProcess.StartInfo.RedirectStandardError = true;
			myProcess.StartInfo.RedirectStandardOutput = true;
			myProcess.StartInfo.UseShellExecute = false;
			myProcess.StartInfo.CreateNoWindow = true;
			try
			{
				if (!myProcess.Start())
					throw new Exception("Failed to start nvcc process. Is CUDA installed on your system?");

				string output = myProcess.StandardOutput.ReadToEnd().Trim();
				myProcess.WaitForExit();
				return output;
			}
			catch (Exception e)
			{
				EventLog.Warn(null, null, $"Error (nvcc.exe {arguments}): {e}");
				return null;
			}
			
		}

		private static List<string> QueryGpuCodes()
		{
			var scodes = RunProcess("--list-gpu-code");
			var rs = new List<string>();
			if (scodes == null)
				return rs;

			foreach (string code in scodes.Split(new char[] { '\r', '\n' }))
			{
				if (string.IsNullOrWhiteSpace(code))
					continue;
				if (!code.StartsWith("sm_"))
					throw new InvalidOperationException("Unexpected string returned by nvcc --list-gpu-code: " + code);
				rs.Add(code.Substring(3));
			}
			return rs;
		}

		private static string? QueryVersion()
		{
			var v = RunProcess("--version");
			string head = "Cuda compilation tools, release ";
			if (v is not null)
				foreach (string line in v.Split(new char[] { '\r', '\n' }))
				{
					if (line.StartsWith(head))
					{
						int commaAt = line.IndexOf(',', head.Length);
						if (commaAt >= 0)
							return line.Substring(head.Length, commaAt - head.Length).Trim();
						return line.Substring(head.Length).Trim();
					}
				}
			return null;
		}

		/// <summary>
		/// Queries the version of the installed CUDA compiler. May return null
		/// </summary>
		public static string? Version
		{
			get
			{
				if (version != null)
					return version;
				version = QueryVersion();
				return version;
			}
		}

		/// <summary>
		/// Attempts to identify the path where common CUDA include files are located. May return null.
		/// If the property does not return null, then the return path is valid and points to a folder
		/// </summary>
		public static string? CommonInc
		{
			get
			{
				string from = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (from == null)
					return null;
				string path = Path.Combine(from, "NVIDIA Corporation","CUDA Samples", "v" + Version, "common", "inc");
				if (new FilePath(path).DirectoryExists)
					return path;
				return null;
			}
		}

		/// <summary>
		/// Queries the compilation codes supported by the installed CUDA compiler. May return null
		/// </summary>
		public static IEnumerable<string> GpuCodes {
			get
			{
				if (gpuCodes == null)
					gpuCodes = QueryGpuCodes();
				return gpuCodes;
			}
		}
	}
}
