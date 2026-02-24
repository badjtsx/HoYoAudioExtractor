using static System.Buffers.Binary.BinaryPrimitives;
using System.Diagnostics;
using HoYoAudioExtractor.Entries;

namespace HoYoAudioExtractor.Formats
{
	public class AudioKineticPackage
	{
		public FileInfo FileName { get; set; }
		public int LanguageCount { get; set; }

		public AudioKineticPackage(FileInfo fileName, int languageCount)
		{
			FileName = fileName;
			LanguageCount = languageCount;
		}

		public void Parse(bool extractBnk, DirectoryInfo outputDirectory, bool convertWem)
		{
			Console.WriteLine($"Processing {FileName.Name}...");
			byte[] data = File.ReadAllBytes(FileName.FullName);
			int pos = 0;
			ParseAKPK(data, ref pos);

			var fileEntries = new List<FileEntry>();

			int banks = ReadInt32LittleEndian(data.AsSpan(0x10));
			int streamed = ReadInt32LittleEndian(data.AsSpan(0x14));
			int external = ReadInt32LittleEndian(data.AsSpan(0x18));

			if (banks != 4) ParseFileEntry(data, fileEntries, ref pos, false);
			if (streamed != 4) ParseFileEntry(data, fileEntries, ref pos, false);
			if (external != 4) ParseFileEntry(data, fileEntries, ref pos, true);

			foreach (var entry in fileEntries)
			{
				if (extractBnk && entry is BnkFile bnkEntry)
					bnkEntry.ExtractWem(data, outputDirectory);
				else
					entry.Extract(data, outputDirectory);
			}

			if (convertWem)
			{
				foreach (var entry in Directory.GetFiles(outputDirectory.FullName, "*.wem"))
				{
					FileInfo wemFile = new FileInfo(entry);
					ConvertToWav(wemFile, outputDirectory);
				}
			}
		}

		public void ParseFileEntry(ReadOnlySpan<byte> data, List<FileEntry> fileEntries, ref int pos, bool is64)
		{
			int count = ParseFileCount(data, ref pos);
			for (int i = 0; i < count; i++)
			{
				int savedPos = pos;
				try
				{
					fileEntries.Add(is64 ? ParseByBit(data, ref pos, true) : ParseByBit(data, ref pos, false));
				}
				catch (InvalidDataException ex)
				{
					pos = is64 ? savedPos + 24 : savedPos + 20;
					Console.WriteLine($"Warning: Skipping entry - {ex.Message}");
				}
			}
		}

		public void ParseAKPK(ReadOnlySpan<byte> data, ref int pos)
		{
			if (data[pos..(pos + 4)].SequenceEqual("AKPK"u8))
			{
				int toSubstract = 0;
				pos += 12;
				int MetaDataSize = ReadInt32LittleEndian(data.Slice(pos));
				pos += 4;
				if (ReadInt32LittleEndian(data.Slice(pos)) != 4) { toSubstract = 8; }
				pos += 4;
				if (toSubstract == 0) { if (ReadInt32LittleEndian(data.Slice(pos)) != 4) { toSubstract = 4; } }
				pos += 4;
				if (toSubstract != 0 && ReadInt32LittleEndian(data.Slice(pos)) != 4) { toSubstract = 8; }
				pos += 4;
				LanguageCount = ReadInt32LittleEndian(data.Slice(pos)) - 1;
				pos += 4;
				pos += 4;
				pos += MetaDataSize - toSubstract;
			}
		}

		public int ParseFileCount(ReadOnlySpan<byte> data, ref int pos)
		{
			int AudioFileCount = ReadInt32LittleEndian(data.Slice(pos));
			pos += 4;
			return AudioFileCount;
		}

		public FileEntry ParseByBit(ReadOnlySpan<byte> data, ref int pos, bool is64)
		{
			ulong Id;
			if (is64)
			{
				Id = (ulong)ReadUInt32LittleEndian(data.Slice(pos + 4)) << 32
				   | ReadUInt32LittleEndian(data.Slice(pos));
				pos += 8;
			}
			else
			{
				Id = ReadUInt32LittleEndian(data.Slice(pos));
				pos += 4;
			}
			int blockSize = ReadInt32LittleEndian(data.Slice(pos));
			pos += 4;
			int Size = ReadInt32LittleEndian(data.Slice(pos));
			pos += 4;
			int Offset = ReadInt32LittleEndian(data.Slice(pos)) * blockSize;
			string type = "Unknown";

			if (Offset + 4 < data.Length)
			{
				if (data[Offset..(Offset + 4)].SequenceEqual("BKHD"u8))
				{
					type = "bnk";
					pos += 4;
					pos += 4;
					return new BnkFile(Id, Offset, Size, type);
				}
				else if (data[Offset..(Offset + 4)].SequenceEqual("RIFF"u8))
				{
					type = "wem";
					pos += 4;
					pos += 4;
					return new WemFile(Id, Offset, Size, type);
				}
			}
			else
			{
				pos += 4;
				pos += 4;
				return new WemFile(Id, Offset, Size, type);
			}
			throw new InvalidDataException($"Unknown file type at offset {Offset}");
		}

		public static void ConvertToWav(FileInfo wemFile, DirectoryInfo outputDir)
		{
			string outputPath = Path.Combine(outputDir.FullName,
				Path.GetFileNameWithoutExtension(wemFile.Name) + ".wav");

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "vgmstream-win64/vgmstream-cli",
					Arguments = $"-o \"{outputPath}\" \"{wemFile.FullName}\"",
					UseShellExecute = false
				}
			};
			try
			{
				process.Start();
				process.WaitForExit();
				File.Delete(wemFile.FullName);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to convert {wemFile.Name}: {ex.Message}");
			}
		}
	}
}
