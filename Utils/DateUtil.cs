using System;

namespace NotionDeadlineFairy.Utils
{
	public enum DateType
	{
		NONE,
		TODAY,
		THIS_WEEK,
		THIS_MONTH
	}

	public static class DateUtil
	{
		public static string ToDateString(DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd");
		}

		public static DateTime? getTime(DateType dateType, DateTime? nowUtc = null)
		{
			if (dateType == DateType.NONE)
				return null;

			var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");

			var utc = nowUtc ?? DateTime.UtcNow;
			var nowKst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), kst);

			var today = nowKst.Date; 
			DateTime endExclusive;  

			switch (dateType)
			{
				case DateType.TODAY:
					endExclusive = today.AddDays(1);
					break;

				case DateType.THIS_WEEK:
					// DayOfWeek는 enum이라 int로 캐스팅해야 함
					int diffFromMonday = (((int)today.DayOfWeek) + 6) % 7; // Monday=0 ... Sunday=6
					var startOfWeek = today.AddDays(-diffFromMonday);
					endExclusive = startOfWeek.AddDays(7);
					break;

				case DateType.THIS_MONTH:
					// 이번달 끝: 이번달 1일 기준으로 다음달 1일(배타)
					var firstOfMonth = new DateTime(today.Year, today.Month, 1);
					endExclusive = firstOfMonth.AddMonths(1);
					break;

				default:
					return null;
			}

			// 포함 상한선: 배타 끝 - 1ms => 23:59:59.999
			return endExclusive.AddMilliseconds(-1);
		}
	}
}