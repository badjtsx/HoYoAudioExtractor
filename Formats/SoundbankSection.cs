using System.Buffers.Binary;
using System.Text;

namespace GenshinAudioExtractor.Formats
{
	public class SoundbankSection
	{
		public string Magic { get; set; }
		public byte[] Data { get; set; }

		public SoundbankSection(string magic, byte[] data)
		{
			Magic = magic;
			Data = data;
		}

		public byte[] Serialize(bool littleEndian = true)
		{
			byte[] result = new byte[Magic.Length + 4 + Data.Length];
			Span<byte> span = result;

			Encoding.ASCII.GetBytes(Magic, span);

			if (littleEndian)
				BinaryPrimitives.WriteInt32LittleEndian(span.Slice(Magic.Length), Data.Length);
			else
				BinaryPrimitives.WriteInt32BigEndian(span.Slice(Magic.Length), Data.Length);

			Data.CopyTo(result, Magic.Length + 4);

			return result;
		}
	}
}
