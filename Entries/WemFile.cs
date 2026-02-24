namespace HoYoAudioExtractor.Entries
{
	public class WemFile : FileEntry
	{
		public WemFile(ulong id, int offset, int size, string type) : base(id, offset, size)
		{
		}

		public override void Extract(byte[] data, DirectoryInfo outputDirectory)
		{
			byte[] fileData = new byte[Size];
			Array.Copy(data, Offset, fileData, 0, Size);
			File.WriteAllBytes(Path.Combine(outputDirectory.FullName, $"{Id}.wem"), fileData);
		}
	}
}
