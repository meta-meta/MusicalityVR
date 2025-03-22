using System;
using System.Collections.Generic;

namespace Musicality
{
    public enum UnpitchedNoteCollection
    {
        CR78,
        Int17,
        TR808,
    }

    public static class UnpitchedNoteCollections
    {
        private static readonly HashSet<NoteUnpitchedMidi> CR78 = new HashSet<NoteUnpitchedMidi>()
        {
            new NoteUnpitchedMidi(0, "BongoH", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(1, "BongoL", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(2, "CH3", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(3, "CH5", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(4, "Conga5", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(5, "Conga7", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(6, "Kick", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(7, "KickHi", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(8, "OH3", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(9, "OH5", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(10, "OH7", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(11, "Perc17", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(12, "Perc27", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(13, "Rim", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(14, "Snare13", UnpitchedNoteCollection.CR78),
            new NoteUnpitchedMidi(15, "Snare14", UnpitchedNoteCollection.CR78),
        };
       
        private static readonly HashSet<NoteUnpitchedMidi> TR808 = new HashSet<NoteUnpitchedMidi>()
        {
            new NoteUnpitchedMidi(0, "BD Lo", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(1, "BD Long", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(2, "BD Hi", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(3, "CH", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(4, "Clap", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(5, "Claves", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(6, "Cowbell", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(7, "Cymbal", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(8, "Maracas", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(9, "OH", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(10, "Rim", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(11, "Snare Lo", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(12, "Snare Hi", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(13, "Tom Hi", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(14, "Tom Lo", UnpitchedNoteCollection.TR808),
            new NoteUnpitchedMidi(15, "Tom Mid", UnpitchedNoteCollection.TR808),
        };

        private static readonly HashSet<NoteUnpitchedMidi> INT17 = new HashSet<NoteUnpitchedMidi>()
        {
            new NoteUnpitchedMidi(1, "1", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(2, "2", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(3, "3", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(4, "4", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(5, "5", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(6, "6", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(7, "7", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(8, "8", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(9, "9", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(10, "10", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(11, "11", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(12, "12", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(13, "13", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(14, "14", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(15, "15", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(16, "16", UnpitchedNoteCollection.Int17),
            new NoteUnpitchedMidi(17, "17", UnpitchedNoteCollection.Int17),
        };

        public static HashSet<NoteUnpitchedMidi> Get(UnpitchedNoteCollection collectionType)
        {
            return collectionType switch
            {
                UnpitchedNoteCollection.CR78 => CR78,
                UnpitchedNoteCollection.Int17 => INT17,
                UnpitchedNoteCollection.TR808 => TR808,
                _ => throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null)
            };
        }

    }
}