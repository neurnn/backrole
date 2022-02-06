using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Backrole.Crypto.Internals
{
    internal static class ArrayHelpers
	{
		/// <summary>
		/// Merge multiple arrays to single array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Inputs"></param>
		/// <returns></returns>
		public static T[] Merge<T>(params T[][] Inputs)
		{
			var Result = new T[Inputs.Sum(Array => Array.Length)];
			int Offset = 0;

			foreach (var Each in Inputs)
			{
				Buffer.BlockCopy(Each, 0, Result, Offset, Each.Length);
				Offset += Each.Length;
			}

			return Result;
		}

		/// <summary>
		/// Concat an array to the array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Input1"></param>
		/// <param name="Input2"></param>
		/// <returns></returns>
		public static T[] Concat<T>(this T[] Input1, T[] Input2)
		{
			var Result = new T[Input1.Length + Input2.Length];
			Buffer.BlockCopy(Input1, 0, Result, 0, Input1.Length);
			Buffer.BlockCopy(Input2, 0, Result, Input1.Length, Input2.Length);
			return Result;
		}

		/// <summary>
		/// Extract the subset of the array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Input"></param>
		/// <param name="Start"></param>
		/// <param name="Length"></param>
		/// <returns></returns>
		public static T[] Subset<T>(this T[] Input, int Start, int Length)
			=> new ArraySegment<T>(Input, Start, Length).ToArray();

		/// <summary>
		/// Extract the subset of the array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Input"></param>
		/// <param name="Start"></param>
		/// <returns></returns>
		public static T[] Subset<T>(this T[] Input, int Start)
			=> Subset(Input, Start, Input.Length - Start);
    }
}
