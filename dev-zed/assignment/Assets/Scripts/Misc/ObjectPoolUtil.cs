namespace Sample
{
	public class ObjectPoolUtil 
	{
		public static int CalcElapsedTickCount(bool test, int startTickCount, int currentTickCount)
		{
			var elapsedTickCount = currentTickCount - startTickCount;

			if (elapsedTickCount < 0)
			{
				//elapsedTimeTick = currentTickcount + (int.MaxValue - tickcount);
				elapsedTickCount += int.MaxValue;
			}
			return elapsedTickCount;
		}
	}
}