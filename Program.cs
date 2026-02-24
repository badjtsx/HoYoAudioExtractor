using HoYoAudioExtractor.Formats;

public static class Program
{
	public static void Main(String[] args)
	{
		if (args.Length < 2)
		{
			PrintUsage();
			return;
		}

		string inputPath = args[0];
		DirectoryInfo outputDir = new DirectoryInfo(args[1]);
		bool extractBnk = args.Contains("--extractBnk");
		bool convertWem = false;
		if (args.Contains("--convertWem")) {
			convertWem = true;
		}

		if (!outputDir.Exists)
			outputDir.Create();

		if (Directory.Exists(inputPath))
		{
			foreach (string filePath in Directory.GetFiles(inputPath, "*.pck"))
			{
				string fileName = Path.GetFileNameWithoutExtension(filePath);
				string subDir = Path.Combine(outputDir.FullName, fileName);
				Directory.CreateDirectory(subDir);
				Process(new FileInfo(filePath), new DirectoryInfo(subDir), extractBnk, convertWem);
			}
		}
		else if (File.Exists(inputPath))
		{
			Process(new FileInfo(inputPath), outputDir, extractBnk, convertWem);
		}
		else
		{
			Console.WriteLine($"Input path '{inputPath}' does not exist.");
		}

	}
	public static void Process(FileInfo file, DirectoryInfo outputDir, bool extractBnk, bool convertWem)
	{
		AudioKineticPackage pkg = new AudioKineticPackage(file, 0);
		pkg.Parse(extractBnk, outputDir, convertWem);
	}


	public static void PrintUsage()
	{
		Console.WriteLine("Usage: HoYoAudioExtractor <input_file> <output_dir>");
		
		Console.WriteLine("Options:");

		Console.WriteLine("  --extractBnk    Extract .bnk files as well (will extract .wem files from .bnk)");

		Console.WriteLine("  --convertWem    Convert extracted .wem files to .wav format (requires vgmstream-win64)");
	}
} 