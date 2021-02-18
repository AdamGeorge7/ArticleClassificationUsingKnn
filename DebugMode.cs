using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace iad_test
{
    class DebugMode
	{
		public Stopwatch stopwatch { get; private set; }
		public StreamWriter streamWriter { get; private set; }

		public DebugMode(string logFilename)
		{
			stopwatch = new Stopwatch();
			streamWriter = new StreamWriter(logFilename);
		}

		public void WriteLogLine(string message, bool mustWriteInTerminal)
		{
			string formattedMessage = ToString() + " [" + message + "]";

			streamWriter.WriteLine(formattedMessage);

			if (mustWriteInTerminal)
			{
				Console.WriteLine(formattedMessage);
			}
		}

		public override string ToString()
		{
			TimeSpan ts = stopwatch.Elapsed;
			string elapsed = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, (int)(ts.Milliseconds * 0.1));

			return "Time elapsed: " + elapsed;
		}
	}
}
