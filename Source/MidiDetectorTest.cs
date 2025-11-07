using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Vivace.Services;

namespace Vivace;

public class MidiDetectorTest
{
    public static void Main(string[] args)
    {
        string demoPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "demo-midis");

        // check if demo-midis directory exists
        if (!Directory.Exists(demoPath))
        {
            // try alternative path (if running from Source directory)
            demoPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "demo-midis");
            if (!Directory.Exists(demoPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: demo-midis directory not found at: {demoPath}");
                Console.ResetColor();
                return;
            }
        }

        var midiFiles = Directory.GetFiles(demoPath, "*.mid", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(demoPath, "*.MID", SearchOption.TopDirectoryOnly))
                                 .OrderBy(f => f)
                                 .ToArray();

        if (midiFiles.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: No MIDI files found in {demoPath}");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       MIDI Specification Detector - Comprehensive Test         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"Found {midiFiles.Length} MIDI files in: {demoPath}");
        Console.WriteLine();

        var detector = new MidiSpecificationDetector();
        int testNumber = 0;

        foreach (var filePath in midiFiles)
        {
            testNumber++;
            string fileName = Path.GetFileName(filePath);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"Test #{testNumber}: {fileName}");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.ResetColor();

            try
            {
                var midiFile = MidiFile.Read(filePath);
                string result = detector.DetectMidiSpecification(midiFile);

                // summary output
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Final Result: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(result);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Failed to process file");
                Console.WriteLine($"Exception: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                     Testing Complete                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }
}
