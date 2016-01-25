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
		static NamedPipeServerStream pipeServer;
		static Thread pipeServerThread;
		static ProjectView view;
		static object pipeServerLock = new object();


		private static void CreatePipeServer()
		{
			pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1);
			pipeServerThread = new Thread(new ThreadStart(PipeServerThread));
			pipeServerThread.Start();
		}

		private static void PipeServerThread()
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
						string input = sr.ReadLine();

						if (view != null)
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
			lock(pipeServerLock)
			{
				if (pipeServer != null)
				{
					pipeServer.Close();
					pipeServer = null;

					using (NamedPipeClientStream npcs = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None))
					{
						try
						{
							npcs.Connect(100);
						}
						catch (Exception)
						{ }
					}

					pipeServerThread.Join();
				}
			}
			Application.Exit();
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			bool createdNew = true;
			using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
			{
				if (createdNew)
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					CreatePipeServer();

					view = new ProjectView();
					Application.Run(view);
				}
				else
				{
					Process current = Process.GetCurrentProcess();
					foreach (Process process in Process.GetProcessesByName(current.ProcessName))
					{
						if (process.Id != current.Id)
						{
							SetForegroundWindow(process.MainWindowHandle);

							string[] parameters = Environment.GetCommandLineArgs();
							if (parameters.Length > 1)
							{ 
								NamedPipeClientStream pipeClient = new NamedPipeClientStream(".",PipeName, PipeDirection.Out, PipeOptions.None);

								if (pipeClient.IsConnected != true)
									pipeClient.Connect();

								StreamWriter sw = new StreamWriter(pipeClient);

								try
								{
									sw.WriteLine(parameters[1]);
									sw.Flush();
									pipeClient.Close();
								}
								catch (Exception ex) { throw ex; }
							}

							break;
						}
					}
				}
			}
        }
    }
}
