using HoYoAudioExtractor.Formats;

namespace HoYoAudioExtractor.Entries
{
	public class BnkFile : FileEntry
	{
		public BnkFile(ulong id, int offset, int size, string type) : base(id, offset, size)
		{
		}

		public override void Extract(byte[] data, DirectoryInfo outputDirectory)
		{
			Console.WriteLine($"Extracting BNK file {Id}...");
			byte[] fileData = new byte[Size];
			Array.Copy(data, Offset, fileData, 0, Size);
			File.WriteAllBytes(Path.Combine(outputDirectory.FullName, $"{Id}.bnk"), fileData);
		}

		public void ExtractWem(byte[] data, DirectoryInfo outputDirectory)
		{
			Console.WriteLine($"Extracting WEM files from BNK file {Id}...");
			byte[] bankData = data[Offset..(Offset + Size)];
			var bank = new SoundBank(bankData);
			bank.Parse();
			bank.ExtractAll(outputDirectory);
		}
	}
}
