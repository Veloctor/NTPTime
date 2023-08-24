using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

static class Program
{
	static void Main()
	{
		StringBuilder sb = new();
		Stopwatch sw = Stopwatch.StartNew();
		while (true) {
			NTPTime.SyncNetworkTime();
			sw.Restart();
			sb.Clear();
			sb.Append("当前时区本地时间: ");
			sb.AppendLine(FmtDate(DateTime.Now));
			sb.Append("当前时区网络时间: ");
			sb.AppendLine(FmtDate(NTPTime.RealTimeLocalTimeZone));
			sb.Append("当前UTC网络时间: ");
			sb.AppendLine(FmtDate(NTPTime.RealTimeUTC));
			sb.Append("网络与本地时差: ");
			sb.Append(NTPTime.NetLocalDiff.TotalMilliseconds.ToString("F1"));
			sb.Append("ms");
			Console.Clear();
			Console.WriteLine(sb);
			Delay(1000, sw);
		}
	}

	static void Delay(int ms, Stopwatch? recordingSw = null)
	{
		recordingSw ??= Stopwatch.StartNew();
		long longMs = ms - recordingSw.ElapsedMilliseconds;
		Thread.Sleep((int)((longMs * 7) >> 3));
		while(recordingSw.ElapsedMilliseconds < ms) { }
	}

	static string FmtDate(DateTime dt) => dt.ToString("yyy/M/d H:mm:ss.fff K");
}