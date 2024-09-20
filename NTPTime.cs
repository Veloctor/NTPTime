using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

public class NTPTime
{
	public static string TIME_SERVER_URL = "ntp7.aliyun.com";

	public static TimeSpan NetLocalDiff {
		get {
			if (Δt == default)
				SyncNetworkTime();
			return Δt;
		}
	}

	private static TimeSpan Δt;

	/// <summary> 获取当前网络对时的协调世界时 </summary>
	public static DateTime RealTimeUTC {
		get { //先获取diff再获取local UtcNow，避免自动按需对时消耗时间产生误差
			TimeSpan diff = NetLocalDiff;
			return DateTime.UtcNow.Add(diff);
		}
	}

	public static void GetTimeDivRem(double periodSecs, out long quotient, out double remainder)
	{
		const long tps = TimeSpan.TicksPerSecond;
		long periodTicks = (long)(periodSecs * tps);
		quotient = Math.DivRem(RealTimeUTC.Ticks, periodTicks, out long remTicks);
		remainder = (double)remTicks / tps;
	}

	public static void SyncNetworkTime()
	{
		try {
			Span<byte> ntpData = stackalloc byte[48];
			ntpData[0] = 0x1B;
			IPAddress[] addresses = Dns.GetHostEntry(TIME_SERVER_URL).AddressList;
			IPEndPoint ipEndPoint = new(addresses[0], 123);
			using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.ReceiveTimeout = 1000;
			socket.Connect(ipEndPoint);
			DateTime T0 = DateTime.UtcNow;
			socket.Send(ntpData);
			socket.Receive(ntpData);
			DateTime T3 = DateTime.UtcNow;
			DateTime T1 = ReadTimeInNTPBuffer(ntpData, 32);
			DateTime T2 = ReadTimeInNTPBuffer(ntpData, 40);
			Δt = (T1 - T0 + (T2 - T3)) / 2;
		}
		catch (Exception e) {
			Console.WriteLine("NTP对时失败:" + e);
		}
	}

	private static readonly DateTime NtpTimeStart = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	const long tps = TimeSpan.TicksPerSecond;

	private static DateTime ReadTimeInNTPBuffer(in ReadOnlySpan<byte> buffer, int offset)
	{
		ulong data = BinaryPrimitives.ReadUInt64BigEndian(buffer.Slice(offset, 8));
		uint intPart = (uint)(data >> 32);
		uint fracPart = (uint)data;
		long ticks = intPart * tps + (fracPart * tps >> 32);
		return NtpTimeStart.AddTicks(ticks);
	}
}