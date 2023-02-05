using System;
namespace Lawrence
{
	public class Logger
	{
		public enum Priority
		{
			Log = 0,
			Error = 1,
			Trace = 2
		}

		static Logger shared;

		string logFile;



		public Logger()
		{
		}

		void Log(Priority priority, string value)
		{

		}

		public static Logger Shared()
		{
			if (shared == null)
			{
				shared = new Logger();
			}

			return shared;
		}

		public static void SetLogFile(string filename)
		{
			Logger.Shared().logFile = filename;
		}

		public static void Log(string value)
		{
			Logger.Shared();
		}
	}
}

