namespace HoYoAudioExtractor.Entries
{
	public abstract class FileEntry
	{
		public ulong Id { get; set; }
		public int Offset { get; set; }
		public int Size { get; set; }
		public FileEntry(ulong id, int offset, int size)
		{
			Id = id;
			Offset = offset;
			Size = size;
		}

		public abstract void Extract(byte[] data, DirectoryInfo outputDirectory);
	}
}
