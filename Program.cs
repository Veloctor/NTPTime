using System.Diagnostics;
using System.Text;

static class Program
{
	static void Main()
	{
		StringBuilder sb = new();
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		while (true) {
			var before = NTPTime.NetLocalDiff;
			NTPTime.SyncNetworkTime();
			var change = NTPTime.NetLocalDiff - before;
			sb.Clear();
			sb.Append("本地时间: ");
			sb.AppendLine(FmtDate(DateTime.Now));
			sb.Append("网络时间: ");
			sb.AppendLine(FmtDate(NTPTime.RealTimeUTC.ToLocalTime()));
			sb.Append("时差: ");
			sb.Append(NTPTime.NetLocalDiff.TotalMilliseconds.ToString("F1"));
			sb.AppendLine("ms");
			sb.Append("变化: ");
			sb.Append(change.TotalMilliseconds.ToString("F1"));
			sb.AppendLine("ms");
			Console.Clear();
			Console.WriteLine(sb);
			Thread.Sleep(900);
		}
	}

	static string FmtDate(DateTime dt) => dt.ToString("yyy/M/d H:mm:ss.fff K");
}