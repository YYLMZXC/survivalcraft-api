using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game
{
	public static class EnumUtils
	{
		private struct NamesValues
		{
			public ReadOnlyList<string> Names;

			public ReadOnlyList<int> Values;
		}

		private static class Cache
		{
			private static Dictionary<Type, NamesValues> m_namesValuesByType = new Dictionary<Type, NamesValues>();

			public static NamesValues Query(Type type)
			{
				lock (m_namesValuesByType)
				{
					NamesValues namesValues;
					if (!m_namesValuesByType.TryGetValue(type, out var value))
					{
						namesValues = default(NamesValues);
						namesValues.Names = new ReadOnlyList<string>(new List<string>(Enum.GetNames(type)));
						namesValues.Values = new ReadOnlyList<int>(new List<int>(Enum.GetValues(type).Cast<int>()));
						value = namesValues;
						m_namesValuesByType.Add(type, value);
					}
					namesValues = value;
					return namesValues;
				}
			}
		}

		public static string GetEnumName(Type type, int value)
		{
			int num = GetEnumValues(type).IndexOf(value);
			if (num >= 0)
			{
				return GetEnumNames(type)[num];
			}
			return "<invalid enum>";
		}

		public static ReadOnlyList<string> GetEnumNames(Type type)
		{
			return Cache.Query(type).Names;
		}

		public static ReadOnlyList<int> GetEnumValues(Type type)
		{
			return Cache.Query(type).Values;
		}
	}
}
