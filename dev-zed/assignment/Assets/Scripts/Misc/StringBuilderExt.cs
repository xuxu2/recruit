using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sample
{
	public static partial class StrBuilderEx
	{
		private static readonly char[] ms_digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		private static readonly uint ms_default_decimal_places = 5;
		private static readonly char ms_default_pad_char = '0';

		public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char, uint base_val)
		{
			Debug.Assert(pad_amount >= 0);
			Debug.Assert(base_val > 0 && base_val <= 16);

			uint length = 0;
			uint length_calc = uint_val;

			do
			{
				length_calc /= base_val;
				length++;
			}
			while (length_calc > 0);

			string_builder.Append(pad_char, (int)System.Math.Max(pad_amount, length));

			int strpos = string_builder.Length;

			while (length > 0)
			{
				strpos--;

				string_builder[strpos] = ms_digits[uint_val % base_val];

				uint_val /= base_val;
				length--;
			}

			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val)
		{
			string_builder.Concat(uint_val, 0, ms_default_pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount)
		{
			string_builder.Concat(uint_val, pad_amount, ms_default_pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char)
		{
			string_builder.Concat(uint_val, pad_amount, pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char, uint base_val)
		{
			Debug.Assert(pad_amount >= 0);
			Debug.Assert(base_val > 0 && base_val <= 16);

			if (int_val < 0)
			{
				string_builder.Append('-');
				uint uint_val = uint.MaxValue - ((uint)int_val) + 1; 
				string_builder.Concat(uint_val, pad_amount, pad_char, base_val);
			}
			else
			{
				string_builder.Concat((uint)int_val, pad_amount, pad_char, base_val);
			}

			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, int int_val)
		{
			string_builder.Concat(int_val, 0, ms_default_pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount)
		{
			string_builder.Concat(int_val, pad_amount, ms_default_pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char)
		{
			string_builder.Concat(int_val, pad_amount, pad_char, 10);
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount, char pad_char)
		{
			Debug.Assert(pad_amount >= 0);

			if (decimal_places == 0)
			{
				int int_val;
				if (float_val >= 0.0f)
				{
					int_val = (int)(float_val + 0.5f);
				}
				else
				{
					int_val = (int)(float_val - 0.5f);
				}

				string_builder.Concat(int_val, pad_amount, pad_char, 10);
			}
			else
			{
				int int_part = (int)float_val;

				string_builder.Concat(int_part, pad_amount, pad_char, 10);
				string_builder.Append('.');

				float remainder = System.Math.Abs(float_val - int_part);

				do
				{
					remainder *= 10;
					decimal_places--;
				}
				while (decimal_places > 0);

				remainder += 0.5f;
				string_builder.Concat((uint)remainder, 0, '0', 10);
			}
			return string_builder;
		}

		public static StringBuilder Concat(this StringBuilder string_builder, float float_val)
		{
			string_builder.Concat(float_val, ms_default_decimal_places, 0, ms_default_pad_char);
			return string_builder;
		}
		public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places)
		{
			string_builder.Concat(float_val, decimal_places, 0, ms_default_pad_char);
			return string_builder;
		}
		public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount)
		{
			string_builder.Concat(float_val, decimal_places, pad_amount, ms_default_pad_char);
			return string_builder;
		}
		public static string ToString_GC(this StringBuilder string_builder)
		{
			return string_builder.ToString();
		}

		public static string Concat(params string[] strings)
		{
			var sb = ObjectPoolManager.New<StringBuilder>();

			sb.Length = 0;
			for (var i = 0; i < strings.Length; ++i)
			{
				sb.Append(strings[i] == null ? "null" : strings[i]);
			}
			var value = sb.ToString();

			sb.Length = 0;

			ObjectPoolManager.Return(sb);

			return value;
		}

		public static string Concat(List<string> strings)
		{
			StringBuilder sb = ObjectPoolManager.New<StringBuilder>();

			sb.Length = 0;
			for (int i = 0; i < strings.Count; ++i)
			{
				sb.Append(strings[i] == null ? "null" : strings[i]);
			}
			string value = sb.ToString();

			sb.Length = 0;

			ObjectPoolManager.Return(sb);

			return value;
		}
	}
}