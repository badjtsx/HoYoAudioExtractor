# Hoyo Audio Extractor

A command-line tool for extracting audio files from Audiokinetic package (.pck) 
files used in HoYoverse games (Genshin Impact, Honkai: Star Rail, Honkai Impact, ZZZ).

## Requirements
- .NET 10.0
- vgmstream-cli (optional, for .wem to .wav conversion)
  - Place the vgmstream-win64 folder in the same directory as the executable

## Usage
```
HoYoAudioExtractor <input> <output_dir> [options]
```

### Options
- `--extractBnk` — Extract .wem files from embedded sound banks
- `--convertWem` — Convert extracted .wem files to .wav using vgmstream

### Examples
```
# Extract a single package
HoYoAudioExtractor Banks0.pck output/

# Extract all packages in a directory
HoYoAudioExtractor "C:\Program Files\Genshin Impact\Genshin Impact game\GenshinImpact_Data\StreamingAssets\AudioAssets" output/

# Extract and convert to wav
HoYoAudioExtractor Streamed0.pck output/ --convertWem
```

## Supported Games
- Genshin Impact
- Honkai: Star Rail
- Honkai Impact 3rd
- Zenless Zone Zero

## Notes
- Bank files (.bnk) without embedded audio are metadata-only and contain no extractable sound
- vgmstream is not included, download it from https://github.com/vgmstream/vgmstream