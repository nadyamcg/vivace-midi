using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Vivace.Models;

namespace Vivace.Services;
// service class responsible for MIDI file operations using DryWetMIDI
public class MidiFileService
{
    private readonly MidiSpecificationDetector _specificationDetector;

    public MidiFileService()
    {
        _specificationDetector = new MidiSpecificationDetector();
    }

    public MidiFileInfo? LoadMidiFile(string filePath)
    // let's not mark this one as static, might need to inject dependencies later
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"MIDI file not found: {filePath}", filePath);
        }

        try
        {
            // DryWetMIDI parses the binary MIDI file format into an object we can work with
            var midiFile = MidiFile.Read(filePath);

            // extract file name from full path
            var fileName = Path.GetFileName(filePath);

            // count all MIDI events across all tracks
            // sift through all track chunks and count their events
            var eventCount = midiFile.GetTrackChunks().Sum(chunk => chunk.Events.Count);

            // detect if MIDI file is empty (no events at all)
            var isEmpty = eventCount == 0;

            // grab tempo map, tells us how tempo changes throughout the piece
            var tempoMap = midiFile.GetTempoMap();

            // calculate the total duration of the MIDI file
            // just find last event's time and convert it to a TimeSpan
            var duration = TimeSpan.Zero;
            if (eventCount > 0)
            {
                // get the time of the last event in metric time
                var lastEventTime = midiFile.GetTimedEvents()
                    .LastOrDefault()?.TimeAs<MetricTimeSpan>(tempoMap);
                if (lastEventTime != null)
                {
                    duration = (TimeSpan)lastEventTime;
                }
            }

            // determine the MIDI file format (0, 1, or 2)
            var formatString = midiFile.OriginalFormat switch
            {
                MidiFileFormat.SingleTrack => "Format 0 (Single Track)",
                MidiFileFormat.MultiTrack => "Format 1 (Multi Track)",
                MidiFileFormat.MultiSequence => "Format 2 (Multi Sequence)",
                _ => "Unknown Format"
            };

            // detect MIDI specification standard (GM, GM2, XG, GS)
            var midiSpecification = _specificationDetector.DetectMidiSpecification(midiFile);

            // count tempo change events
            var tempoEventCount = midiFile.GetTrackChunks()
                .SelectMany(chunk => chunk.Events)
                .OfType<SetTempoEvent>()
                .Count();

            // create and return our domain model
            return new MidiFileInfo
            {
                FileName = fileName,
                FilePath = filePath,
                TrackCount = midiFile.GetTrackChunks().Count(),
                EventCount = eventCount,
                Duration = duration,
                Format = formatString,
                TempoEventCount = tempoEventCount,
                MidiSpecification = midiSpecification,
                IsEmpty = isEmpty
            };
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not FileNotFoundException)
        {
            // wrap any DryWetMIDI-specific exceptions in a more generic exception
            // this should prevent implementation details from leaking to the ViewModel
            throw new InvalidOperationException($"Failed to read MIDI file: {ex.Message}", ex);
        }
    }
}