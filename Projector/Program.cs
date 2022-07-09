using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


	


namespace Projector
{

    static class Program
    {

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();


		static NamedPipeServerStream? pipeServer;
		static Thread? pipeServerThread;
		static ProjectView? view;
		static readonly object pipeServerLock = new object();


		private static void CreatePipeServer()
		{
			pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1);
			pipeServerThread = new Thread(new ThreadStart(() => PipeServerThread(pipeServer)));
			pipeServerThread.Start();
		}

		private static void PipeServerThread(NamedPipeServerStream pipeServer)
		{
	
			StreamReader sr = new StreamReader(pipeServer);
			try
			{
				int loopCnt = 0;
				do
				{
					try
					{
						pipeServer.WaitForConnection();
						string? input = sr.ReadLine();

						if (view is not null && input is not null)
							view.HandleInput(input);
					}
					catch (Exception)
					{ }
					if (pipeServer != null)
						try
						{
							pipeServer.WaitForPipeDrain();
							if (pipeServer.IsConnected)
							{
								pipeServer.Disconnect();
							}
						}
						catch (Exception)
						{ }
					Thread.Sleep(10);

					if (++loopCnt > 10)
					{
						loopCnt = 0;

						lock(pipeServerLock)
						{
							try
							{
								sr.Close();
							}
							catch { }
							try
							{
								CreatePipeServer();
							}
							catch { }
						}
					}
				} while (pipeServer != null);
			}
			catch (Exception)
			{


			}
		}


		public static readonly string PipeName = "Solution Projector Pipe";
		public static readonly string MutexName = "Solution Projector Mutex";

		public static void End()
		{
			//lock(pipeServerLock)
			//{
			//	if (pipeServer != null)
			//	{
			//		pipeServer.Close();
			//		pipeServer = null;

			//		using (NamedPipeClientStream npcs = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
			//		{
			//			try
			//			{
			//				npcs.Connect(100);
			//			}
			//			catch (Exception)
			//			{ }
			//		}

			//		pipeServerThread.Join();
			//	}
			//}
			Application.Exit();
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);
		private const int ATTACH_PARENT_PROCESS = -1;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main()
        {
			ApplicationConfiguration.Initialize();

			var cmds = Environment.GetCommandLineArgs();

			if (cmds.Length > 1)
			{
				AttachConsole(ATTACH_PARENT_PROCESS);
				if (cmds[1] == "--help" || cmds[1] == "-h")
				{
					Console.WriteLine("Usage: projector [parameters]");
					Console.WriteLine("  (no parameters): start GUI");
					Console.WriteLine("  --help/-h: show this text");
					Console.WriteLine("  --make/-m X.solution: generate .make files for all involved projects of the specified solution");
					Application.Exit();
					return;
				}
				if (cmds[1] == "--make" || cmds[1] == "-m")
				{
					if (cmds.Length < 2)
					{
						Console.Error.WriteLine("Missing parameter. --make/-m Requires a valid .solution file");
						return;
					}
					EventLog.LogToConsole = true;
					List<Project> projects = new List<Project>();

					bool isNew;
					var solutionPath = new FilePath(cmds[2]);
					var solution = Solution.LoadNew(solutionPath, out isNew);
					if (solution is null)
					{
						EventLog.Warn(null, null, "Unable to load solution from " + solutionPath);
						Console.Error.WriteLine("Unable to load solution from " + solutionPath);
						Application.Exit();
						return;

					}

					solution.ScanEmptySources();
					foreach (var p in solution.Projects)
					{
						projects.Add(p);
					}
					DependencyTree.Clear();
					foreach (var p in projects)
						p.RegisterDependencyNodes();
					DependencyTree.ParseDependencies();
					DependencyTree.GenerateMakefiles();

					EventLog.Inform(null,null,"All done. Exiting.");
					Application.Exit();
					return;
				}
				Console.WriteLine("Unknown parameter given. Use --help to see the list of compatible parameters.");
				Application.Exit();
			}

			//if (Environment.OSVersion.Version.Major >= 6)
				//SetProcessDPIAware();
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());             // Edit as needed

			//bool createdNew = true;
			//using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
			//{
			//	if (createdNew)
			//	{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

				//	CreatePipeServer();

					view = new ProjectView();
					Application.Run(view);
				//}
				//else
				//{
				//	Process current = Process.GetCurrentProcess();
				//	foreach (Process process in Process.GetProcessesByName(current.ProcessName))
				//	{
				//		if (process.Id != current.Id)
				//		{
				//			SetForegroundWindow(process.MainWindowHandle);

				//			string[] parameters = Environment.GetCommandLineArgs();
				//			if (parameters.Length > 1)
				//			{ 
				//				NamedPipeClientStream pipeClient = new NamedPipeClientStream(".",PipeName, PipeDirection.Out, PipeOptions.None);

				//				if (pipeClient.IsConnected != true)
				//					pipeClient.Connect();

				//				StreamWriter sw = new StreamWriter(pipeClient);

				//				try
				//				{
				//					sw.WriteLine(parameters[1]);
				//					sw.Flush();
				//					pipeClient.Close();
				//				}
				//				catch (Exception ex) { throw ex; }
				//			}

				//			break;
				//		}
				//	}
				//}
			//}
        }

		/// <summary>
		/// Flushes <paramref name="writer"/>, checks for changes in file content, and updates if necessary.
		/// Closes <paramref name="writer"/> when done.
		/// </summary>
		/// <param name="outPath">Path to check/write to</param>
		/// <param name="writer">Writer to flush, write to disk (if necessary) and close</param>
		/// <param name="forceWrite">Force writing this file. Writing may also be force via global settings</param>
		/// <returns>True, if the file was written, false if it already matched</returns>
		internal static bool ExportToDisk(FilePath outPath, StreamWriter writer, bool forceWrite=false)
		{
			writer.Flush();
			Stream stream = writer.BaseStream;
			bool changed = (view != null && view.ForceOverwriteProjectFiles) || forceWrite || !ContentMatches(outPath, stream);
			if (changed)
			{
				WriteToFile(outPath, stream);
			}
			writer.Close();
			return changed;
		}

		private static bool ContentMatches(FilePath path, Stream stream)
		{
			try
			{
				StreamReader r = System.IO.File.OpenText(path.FullName);

				stream.Seek(0, SeekOrigin.Begin);
				bool match = StreamsMatch(r.BaseStream, stream);
				r.Close();
				return match;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		private static bool StreamsMatch(Stream streamA, Stream streamB)
		{
			StreamReader a = new StreamReader(streamA);
			StreamReader b = new StreamReader(streamB);
			char[] ba = new char[1024], bb = new char[1024];

			while (!a.EndOfStream && !b.EndOfStream)
			{
				int ra = a.ReadBlock(ba, 0, ba.Length);
				int rb = b.ReadBlock(bb, 0, bb.Length);

				if (ra != rb || !EqualContent(ba, bb, ra))
				{
					return false;
				}
			}
			return a.EndOfStream && b.EndOfStream;
		}

		private static void WriteToFile(FilePath outPath, Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			StreamWriter writer = System.IO.File.CreateText(outPath.FullName);
			stream.CopyTo(writer.BaseStream);
			writer.Close();
		}

		private static bool EqualContent(char[] a, char[] b, int len)
		{
			for (int i = 0; i < len; i++)
				if (a[i] != b[i])
					return false;
			return true;
		}
	}
}
