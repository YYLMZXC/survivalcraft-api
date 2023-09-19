using System;

namespace NVorbis
{
	public class ParameterChangeEventArgs : EventArgs
	{
		public DataPacket FirstPacket
		{
			get;
			private set;
		}

		public ParameterChangeEventArgs(DataPacket firstPacket)
		{
			FirstPacket = firstPacket;
		}
	}
}