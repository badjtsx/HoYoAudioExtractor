using System.Buffers.Binary;
using HoYoAudioExtractor.Entries;

namespace HoYoAudioExtractor.Formats
{
	public class SoundBank
	{
		public byte[] Data { get; set; }
		public List<WemFile> Files { get; set; }

		public Dictionary<string, SoundbankSection> Sections { get; set; }

		private int _dataOffset;
		private int _dataSize;
		private byte[]? _trailingData;

		public SoundBank(byte[] data)
		{
			Data = data;
			Files = new List<WemFile>();
			Sections = new Dictionary<string, SoundbankSection>();
		}

		public void Parse()
		{
			int pos = 0;
			ParseSections(Data, ref pos);
			ParseAudioEntries();
		}

		public void ParseSections(ReadOnlySpan<byte> data, ref int pos)
		{
			while (pos + 8 <= data.Length)
			{
				string magic = System.Text.Encoding.ASCII.GetString(data.Slice(pos, 4));
				if (magic != "BKHD" && magic != "DIDX" && magic != "DATA") break;
				pos += 4;

				int size = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(pos));
				pos += 4;

				if (magic == "DATA")
				{
					_dataOffset = pos;
					_dataSize = size;
					pos += size;
					break;
				}
				else
				{
					Sections[magic] = new SoundbankSection(magic, data.Slice(pos, size).ToArray());
					pos += size;
				}
			}
			_trailingData = data[pos..].ToArray();
		}

		public void ParseAudioEntries()
		{
			if (!Sections.ContainsKey("DIDX"))
				return;

			byte[] didxData = Sections["DIDX"].Data;

			int entrySize = 12;
			int entryCount = didxData.Length / entrySize;

			for (int i = 0; i < entryCount; i++)
			{
				int offset = i * entrySize;

				int audioId = BinaryPrimitives.ReadInt32LittleEndian(didxData.AsSpan(offset));
				int fileOffset = BinaryPrimitives.ReadInt32LittleEndian(didxData.AsSpan(offset + 4));
				int fileSize = BinaryPrimitives.ReadInt32LittleEndian(didxData.AsSpan(offset + 8));

				Files.Add(new WemFile((ulong)audioId, fileOffset, fileSize, "wem"));
			}
		}
		public void ExtractAll(DirectoryInfo OutputDirectory)
		{
			if (!OutputDirectory.Exists)
				OutputDirectory.Create();
			foreach (WemFile entry in Files)
			{
				Console.WriteLine($"Extracting {entry.Id}.wem...");
				string outputFileName = $"{entry.Id}.wem";
				FileInfo outputPath = new FileInfo(Path.Combine(OutputDirectory.FullName, outputFileName));
				File.WriteAllBytes(outputPath.FullName, Data[(_dataOffset + entry.Offset)..(_dataOffset + entry.Offset + entry.Size)]);
			}
		}

	}
}
