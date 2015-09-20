using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	public static partial class Extensions
	{
		public static string RelativateTo(this FileInfo file, DirectoryInfo dir)
		{
			Uri udir = new Uri(dir.FullName + "\\");
			Uri ufile = new Uri(file.FullName);
			Uri urelative = udir.MakeRelativeUri(ufile);
			string path = urelative.ToString();
			path = path.Replace("%20", " ");
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace('/', Path.DirectorySeparatorChar);
			return path;
		}



		public static string Fuse<T>(this IEnumerable<T> items, string glue)
		{
			StringBuilder builder = new StringBuilder();
			bool first = true;
			foreach (T item in items)
			{
				if (!first)
					builder.Append(glue);
				builder.Append(item.ToString());
				first = false;
			}
			return builder.ToString();
		}


		public static T[] ToArray<T>(this List<T> list, int start)
		{
			if (start >= list.Count)
				return new T[0];
			int len = list.Count - start;
			T[] result = new T[len];
			for (int i = 0; i < len; i++)
				result[i] = list[i + start];
			return result;
		}

		/// <summary>
		/// Extracts the content of the local string as a byte-array
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static byte[] GetBytes(this string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		static SHA1 sha = new SHA1CryptoServiceProvider();

		/// <summary>
		/// Computes the Sha1-hash of the local string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static byte[] GetSha1Hash(this string str)
		{
			return sha.ComputeHash(str.GetBytes());
		}

		private static void SwapBytes(byte[] guid, int left, int right)
		{
			byte temp = guid[left];
			guid[left] = guid[right];
			guid[right] = temp;
		}
		private static void SwapByteOrder(byte[] guid)
		{
			SwapBytes(guid, 0, 3);
			SwapBytes(guid, 1, 2);
			SwapBytes(guid, 4, 5);
			SwapBytes(guid, 6, 7);
		}

		/// <summary>
		/// Converts the hashed content of the local string into a Guid
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static Guid ToGuid(this string str)
		{
			byte[] hashed = str.GetSha1Hash();
			byte[] newGuid = new byte[16];
			Array.Copy(hashed, 0, newGuid, 0, 16);

			// set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
			newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));

			// set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
			newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

			// convert the resulting UUID to local byte order (step 13)
			SwapByteOrder(newGuid);
			return new Guid(newGuid);
		}




	}
}
