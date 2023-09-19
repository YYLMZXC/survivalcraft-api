namespace Hjg.Pngcs
{
	public class PngCsUtils
	{
		internal static bool arraysEqual4(byte[] ar1, byte[] ar2)
		{
			if (ar1[0] == ar2[0] && ar1[1] == ar2[1] && ar1[2] == ar2[2])
			{
				return ar1[3] == ar2[3];
			}
			return false;
		}

		internal static bool arraysEqual(byte[] a1, byte[] a2)
		{
			if (a1.Length != a2.Length)
			{
				return false;
			}
			for (int i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}