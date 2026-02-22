using GenshinAudioExtractor.Entries;

namespace GenshinAudioExtractor.Formats
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
			using (MemoryStream ms = new MemoryStream(Data))
			using (BinaryReader reader = new BinaryReader(ms))
			{
				ParseSections(reader);
				ParseAudioEntries();
			}
		}

		public void ParseSections(BinaryReader reader)
		{
			while (true)
			{
				string magicBytes = new string(reader.ReadChars(4));
				if (magicBytes.Length < 4) break;
				string magic = magicBytes;

				if (magic != "BKHD" && magic != "DIDX" && magic != "DATA")
				{
					reader.BaseStream.Seek(-4, SeekOrigin.Current);
					break;
				}

				int size = reader.ReadInt32();

				if (magic == "DATA")
				{
					_dataOffset = (int)reader.BaseStream.Position;
					_dataSize = size;
					reader.BaseStream.Seek(size, SeekOrigin.Current);
					break;
				}
				else
				{
					byte[] sectionData = reader.ReadBytes(size);
					Sections[magic] = new SoundbankSection(magic, sectionData);
				}
			}

			int remaining = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
			_trailingData = reader.ReadBytes(remaining);
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

				int audioId = BitConverter.ToInt32(didxData, offset);
				int fileOffset = BitConverter.ToInt32(didxData, offset + 4);
				int fileSize = BitConverter.ToInt32(didxData, offset + 8);

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
