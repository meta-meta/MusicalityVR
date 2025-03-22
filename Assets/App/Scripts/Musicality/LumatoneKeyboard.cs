using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Musicality
{
    struct NoteMapping
    {
        public int Board;
        public int Key;
        public int Note;
    }  
    struct ColorMapping
    {
        public int Board;
        public int Key;
        public float R;
        public float G;
        public float B;
    }
    
    public class LumatoneKeyboard : MonoBehaviour
    {
        [SerializeField] private GameObject lumatoneKeyPrefab;
        [SerializeField] private List<List<Tonnegg>> notePads = new List<List<Tonnegg>>();
        [SerializeField] private List<Transform> boards;

        private new Dictionary<Tonnegg, float> notepadLocalYs = new Dictionary<Tonnegg, float>();

        
        
        
        private List<NoteMapping> noteMappings = new List<NoteMapping>()
        {
 new NoteMapping(){ Board = 1, Key = 0, Note = 72 }, 
 new NoteMapping(){ Board = 1, Key = 1, Note = 74 }, 
 new NoteMapping(){ Board = 1, Key = 10, Note = 66 }, 
 new NoteMapping(){ Board = 1, Key = 11, Note = 68 }, 
 new NoteMapping(){ Board = 1, Key = 12, Note = 70 }, 
 new NoteMapping(){ Board = 1, Key = 13, Note = 55 }, 
 new NoteMapping(){ Board = 1, Key = 14, Note = 57 }, 
 new NoteMapping(){ Board = 1, Key = 15, Note = 59 }, 
 new NoteMapping(){ Board = 1, Key = 16, Note = 61 }, 
 new NoteMapping(){ Board = 1, Key = 17, Note = 63 }, 
 new NoteMapping(){ Board = 1, Key = 18, Note = 65 }, 
 new NoteMapping(){ Board = 1, Key = 19, Note = 48 }, 
 new NoteMapping(){ Board = 1, Key = 2, Note = 67 }, 
 new NoteMapping(){ Board = 1, Key = 20, Note = 50 }, 
 new NoteMapping(){ Board = 1, Key = 21, Note = 52 }, 
 new NoteMapping(){ Board = 1, Key = 22, Note = 54 }, 
 new NoteMapping(){ Board = 1, Key = 23, Note = 56 }, 
 new NoteMapping(){ Board = 1, Key = 24, Note = 58 }, 
 new NoteMapping(){ Board = 1, Key = 25, Note = 43 }, 
 new NoteMapping(){ Board = 1, Key = 26, Note = 45 }, 
 new NoteMapping(){ Board = 1, Key = 27, Note = 47 }, 
 new NoteMapping(){ Board = 1, Key = 28, Note = 49 }, 
 new NoteMapping(){ Board = 1, Key = 29, Note = 51 }, 
 new NoteMapping(){ Board = 1, Key = 3, Note = 69 }, 
 new NoteMapping(){ Board = 1, Key = 30, Note = 53 }, 
 new NoteMapping(){ Board = 1, Key = 31, Note = 36 }, 
 new NoteMapping(){ Board = 1, Key = 32, Note = 38 }, 
 new NoteMapping(){ Board = 1, Key = 33, Note = 40 }, 
 new NoteMapping(){ Board = 1, Key = 34, Note = 42 }, 
 new NoteMapping(){ Board = 1, Key = 35, Note = 44 }, 
 new NoteMapping(){ Board = 1, Key = 36, Note = 46 }, 
 new NoteMapping(){ Board = 1, Key = 37, Note = 31 }, 
 new NoteMapping(){ Board = 1, Key = 38, Note = 33 }, 
 new NoteMapping(){ Board = 1, Key = 39, Note = 35 }, 
 new NoteMapping(){ Board = 1, Key = 4, Note = 71 }, 
 new NoteMapping(){ Board = 1, Key = 40, Note = 37 }, 
 new NoteMapping(){ Board = 1, Key = 41, Note = 39 }, 
 new NoteMapping(){ Board = 1, Key = 42, Note = 41 }, 
 new NoteMapping(){ Board = 1, Key = 43, Note = 24 }, 
 new NoteMapping(){ Board = 1, Key = 44, Note = 26 }, 
 new NoteMapping(){ Board = 1, Key = 45, Note = 28 }, 
 new NoteMapping(){ Board = 1, Key = 46, Note = 30 }, 
 new NoteMapping(){ Board = 1, Key = 47, Note = 32 }, 
 new NoteMapping(){ Board = 1, Key = 48, Note = 34 }, 
 new NoteMapping(){ Board = 1, Key = 49, Note = 21 }, 
 new NoteMapping(){ Board = 1, Key = 5, Note = 73 }, 
 new NoteMapping(){ Board = 1, Key = 50, Note = 23 }, 
 new NoteMapping(){ Board = 1, Key = 51, Note = 25 }, 
 new NoteMapping(){ Board = 1, Key = 52, Note = 27 }, 
 new NoteMapping(){ Board = 1, Key = 53, Note = 29 }, 
 new NoteMapping(){ Board = 1, Key = 54, Note = 20 }, 
 new NoteMapping(){ Board = 1, Key = 55, Note = 22 }, 
 new NoteMapping(){ Board = 1, Key = 6, Note = 75 }, 
 new NoteMapping(){ Board = 1, Key = 7, Note = 60 }, 
 new NoteMapping(){ Board = 1, Key = 8, Note = 62 }, 
 new NoteMapping(){ Board = 1, Key = 9, Note = 64 }, 
 new NoteMapping(){ Board = 2, Key = 0, Note = 72 }, 
 new NoteMapping(){ Board = 2, Key = 1, Note = 74 }, 
 new NoteMapping(){ Board = 2, Key = 10, Note = 66 }, 
 new NoteMapping(){ Board = 2, Key = 11, Note = 68 }, 
 new NoteMapping(){ Board = 2, Key = 12, Note = 70 }, 
 new NoteMapping(){ Board = 2, Key = 13, Note = 55 }, 
 new NoteMapping(){ Board = 2, Key = 14, Note = 57 }, 
 new NoteMapping(){ Board = 2, Key = 15, Note = 59 }, 
 new NoteMapping(){ Board = 2, Key = 16, Note = 61 }, 
 new NoteMapping(){ Board = 2, Key = 17, Note = 63 }, 
 new NoteMapping(){ Board = 2, Key = 18, Note = 65 }, 
 new NoteMapping(){ Board = 2, Key = 19, Note = 48 }, 
 new NoteMapping(){ Board = 2, Key = 2, Note = 67 }, 
 new NoteMapping(){ Board = 2, Key = 20, Note = 50 }, 
 new NoteMapping(){ Board = 2, Key = 21, Note = 52 }, 
 new NoteMapping(){ Board = 2, Key = 22, Note = 54 }, 
 new NoteMapping(){ Board = 2, Key = 23, Note = 56 }, 
 new NoteMapping(){ Board = 2, Key = 24, Note = 58 }, 
 new NoteMapping(){ Board = 2, Key = 25, Note = 43 }, 
 new NoteMapping(){ Board = 2, Key = 26, Note = 45 }, 
 new NoteMapping(){ Board = 2, Key = 27, Note = 47 }, 
 new NoteMapping(){ Board = 2, Key = 28, Note = 49 }, 
 new NoteMapping(){ Board = 2, Key = 29, Note = 51 }, 
 new NoteMapping(){ Board = 2, Key = 3, Note = 69 }, 
 new NoteMapping(){ Board = 2, Key = 30, Note = 53 }, 
 new NoteMapping(){ Board = 2, Key = 31, Note = 36 }, 
 new NoteMapping(){ Board = 2, Key = 32, Note = 38 }, 
 new NoteMapping(){ Board = 2, Key = 33, Note = 40 }, 
 new NoteMapping(){ Board = 2, Key = 34, Note = 42 }, 
 new NoteMapping(){ Board = 2, Key = 35, Note = 44 }, 
 new NoteMapping(){ Board = 2, Key = 36, Note = 46 }, 
 new NoteMapping(){ Board = 2, Key = 37, Note = 31 }, 
 new NoteMapping(){ Board = 2, Key = 38, Note = 33 }, 
 new NoteMapping(){ Board = 2, Key = 39, Note = 35 }, 
 new NoteMapping(){ Board = 2, Key = 4, Note = 71 }, 
 new NoteMapping(){ Board = 2, Key = 40, Note = 37 }, 
 new NoteMapping(){ Board = 2, Key = 41, Note = 39 }, 
 new NoteMapping(){ Board = 2, Key = 42, Note = 41 }, 
 new NoteMapping(){ Board = 2, Key = 43, Note = 24 }, 
 new NoteMapping(){ Board = 2, Key = 44, Note = 26 }, 
 new NoteMapping(){ Board = 2, Key = 45, Note = 28 }, 
 new NoteMapping(){ Board = 2, Key = 46, Note = 30 }, 
 new NoteMapping(){ Board = 2, Key = 47, Note = 32 }, 
 new NoteMapping(){ Board = 2, Key = 48, Note = 34 }, 
 new NoteMapping(){ Board = 2, Key = 49, Note = 21 }, 
 new NoteMapping(){ Board = 2, Key = 5, Note = 73 }, 
 new NoteMapping(){ Board = 2, Key = 50, Note = 23 }, 
 new NoteMapping(){ Board = 2, Key = 51, Note = 25 }, 
 new NoteMapping(){ Board = 2, Key = 52, Note = 27 }, 
 new NoteMapping(){ Board = 2, Key = 53, Note = 29 }, 
 new NoteMapping(){ Board = 2, Key = 54, Note = 20 }, 
 new NoteMapping(){ Board = 2, Key = 55, Note = 22 }, 
 new NoteMapping(){ Board = 2, Key = 6, Note = 75 }, 
 new NoteMapping(){ Board = 2, Key = 7, Note = 60 }, 
 new NoteMapping(){ Board = 2, Key = 8, Note = 62 }, 
 new NoteMapping(){ Board = 2, Key = 9, Note = 64 }, 
 new NoteMapping(){ Board = 3, Key = 0, Note = 72 }, 
 new NoteMapping(){ Board = 3, Key = 1, Note = 98 }, 
 new NoteMapping(){ Board = 3, Key = 10, Note = 90 }, 
 new NoteMapping(){ Board = 3, Key = 11, Note = 92 }, 
 new NoteMapping(){ Board = 3, Key = 12, Note = 94 }, 
 new NoteMapping(){ Board = 3, Key = 13, Note = 55 }, 
 new NoteMapping(){ Board = 3, Key = 14, Note = 81 }, 
 new NoteMapping(){ Board = 3, Key = 15, Note = 83 }, 
 new NoteMapping(){ Board = 3, Key = 16, Note = 85 }, 
 new NoteMapping(){ Board = 3, Key = 17, Note = 87 }, 
 new NoteMapping(){ Board = 3, Key = 18, Note = 89 }, 
 new NoteMapping(){ Board = 3, Key = 19, Note = 48 }, 
 new NoteMapping(){ Board = 3, Key = 2, Note = 67 }, 
 new NoteMapping(){ Board = 3, Key = 20, Note = 74 }, 
 new NoteMapping(){ Board = 3, Key = 21, Note = 76 }, 
 new NoteMapping(){ Board = 3, Key = 22, Note = 78 }, 
 new NoteMapping(){ Board = 3, Key = 23, Note = 80 }, 
 new NoteMapping(){ Board = 3, Key = 24, Note = 82 }, 
 new NoteMapping(){ Board = 3, Key = 25, Note = 43 }, 
 new NoteMapping(){ Board = 3, Key = 26, Note = 69 }, 
 new NoteMapping(){ Board = 3, Key = 27, Note = 71 }, 
 new NoteMapping(){ Board = 3, Key = 28, Note = 73 }, 
 new NoteMapping(){ Board = 3, Key = 29, Note = 75 }, 
 new NoteMapping(){ Board = 3, Key = 3, Note = 93 }, 
 new NoteMapping(){ Board = 3, Key = 30, Note = 77 }, 
 new NoteMapping(){ Board = 3, Key = 31, Note = 36 }, 
 new NoteMapping(){ Board = 3, Key = 32, Note = 62 }, 
 new NoteMapping(){ Board = 3, Key = 33, Note = 64 }, 
 new NoteMapping(){ Board = 3, Key = 34, Note = 66 }, 
 new NoteMapping(){ Board = 3, Key = 35, Note = 68 }, 
 new NoteMapping(){ Board = 3, Key = 36, Note = 70 }, 
 new NoteMapping(){ Board = 3, Key = 37, Note = 31 }, 
 new NoteMapping(){ Board = 3, Key = 38, Note = 57 }, 
 new NoteMapping(){ Board = 3, Key = 39, Note = 59 }, 
 new NoteMapping(){ Board = 3, Key = 4, Note = 95 }, 
 new NoteMapping(){ Board = 3, Key = 40, Note = 61 }, 
 new NoteMapping(){ Board = 3, Key = 41, Note = 63 }, 
 new NoteMapping(){ Board = 3, Key = 42, Note = 65 }, 
 new NoteMapping(){ Board = 3, Key = 43, Note = 24 }, 
 new NoteMapping(){ Board = 3, Key = 44, Note = 50 }, 
 new NoteMapping(){ Board = 3, Key = 45, Note = 52 }, 
 new NoteMapping(){ Board = 3, Key = 46, Note = 54 }, 
 new NoteMapping(){ Board = 3, Key = 47, Note = 56 }, 
 new NoteMapping(){ Board = 3, Key = 48, Note = 58 }, 
 new NoteMapping(){ Board = 3, Key = 49, Note = 45 }, 
 new NoteMapping(){ Board = 3, Key = 5, Note = 97 }, 
 new NoteMapping(){ Board = 3, Key = 50, Note = 47 }, 
 new NoteMapping(){ Board = 3, Key = 51, Note = 49 }, 
 new NoteMapping(){ Board = 3, Key = 52, Note = 51 }, 
 new NoteMapping(){ Board = 3, Key = 53, Note = 53 }, 
 new NoteMapping(){ Board = 3, Key = 54, Note = 44 }, 
 new NoteMapping(){ Board = 3, Key = 55, Note = 46 }, 
 new NoteMapping(){ Board = 3, Key = 6, Note = 99 }, 
 new NoteMapping(){ Board = 3, Key = 7, Note = 60 }, 
 new NoteMapping(){ Board = 3, Key = 8, Note = 86 }, 
 new NoteMapping(){ Board = 3, Key = 9, Note = 88 }, 
 new NoteMapping(){ Board = 4, Key = 0, Note = 96 }, 
 new NoteMapping(){ Board = 4, Key = 1, Note = 98 }, 
 new NoteMapping(){ Board = 4, Key = 10, Note = 90 }, 
 new NoteMapping(){ Board = 4, Key = 11, Note = 92 }, 
 new NoteMapping(){ Board = 4, Key = 12, Note = 94 }, 
 new NoteMapping(){ Board = 4, Key = 13, Note = 79 }, 
 new NoteMapping(){ Board = 4, Key = 14, Note = 81 }, 
 new NoteMapping(){ Board = 4, Key = 15, Note = 83 }, 
 new NoteMapping(){ Board = 4, Key = 16, Note = 85 }, 
 new NoteMapping(){ Board = 4, Key = 17, Note = 87 }, 
 new NoteMapping(){ Board = 4, Key = 18, Note = 89 }, 
 new NoteMapping(){ Board = 4, Key = 19, Note = 72 }, 
 new NoteMapping(){ Board = 4, Key = 2, Note = 91 }, 
 new NoteMapping(){ Board = 4, Key = 20, Note = 74 }, 
 new NoteMapping(){ Board = 4, Key = 21, Note = 76 }, 
 new NoteMapping(){ Board = 4, Key = 22, Note = 78 }, 
 new NoteMapping(){ Board = 4, Key = 23, Note = 80 }, 
 new NoteMapping(){ Board = 4, Key = 24, Note = 82 }, 
 new NoteMapping(){ Board = 4, Key = 25, Note = 67 }, 
 new NoteMapping(){ Board = 4, Key = 26, Note = 69 }, 
 new NoteMapping(){ Board = 4, Key = 27, Note = 71 }, 
 new NoteMapping(){ Board = 4, Key = 28, Note = 73 }, 
 new NoteMapping(){ Board = 4, Key = 29, Note = 75 }, 
 new NoteMapping(){ Board = 4, Key = 3, Note = 93 }, 
 new NoteMapping(){ Board = 4, Key = 30, Note = 77 }, 
 new NoteMapping(){ Board = 4, Key = 31, Note = 60 }, 
 new NoteMapping(){ Board = 4, Key = 32, Note = 62 }, 
 new NoteMapping(){ Board = 4, Key = 33, Note = 64 }, 
 new NoteMapping(){ Board = 4, Key = 34, Note = 66 }, 
 new NoteMapping(){ Board = 4, Key = 35, Note = 68 }, 
 new NoteMapping(){ Board = 4, Key = 36, Note = 70 }, 
 new NoteMapping(){ Board = 4, Key = 37, Note = 55 }, 
 new NoteMapping(){ Board = 4, Key = 38, Note = 57 }, 
 new NoteMapping(){ Board = 4, Key = 39, Note = 59 }, 
 new NoteMapping(){ Board = 4, Key = 4, Note = 95 }, 
 new NoteMapping(){ Board = 4, Key = 40, Note = 61 }, 
 new NoteMapping(){ Board = 4, Key = 41, Note = 63 }, 
 new NoteMapping(){ Board = 4, Key = 42, Note = 65 }, 
 new NoteMapping(){ Board = 4, Key = 43, Note = 48 }, 
 new NoteMapping(){ Board = 4, Key = 44, Note = 50 }, 
 new NoteMapping(){ Board = 4, Key = 45, Note = 52 }, 
 new NoteMapping(){ Board = 4, Key = 46, Note = 54 }, 
 new NoteMapping(){ Board = 4, Key = 47, Note = 56 }, 
 new NoteMapping(){ Board = 4, Key = 48, Note = 58 }, 
 new NoteMapping(){ Board = 4, Key = 49, Note = 45 }, 
 new NoteMapping(){ Board = 4, Key = 5, Note = 97 }, 
 new NoteMapping(){ Board = 4, Key = 50, Note = 47 }, 
 new NoteMapping(){ Board = 4, Key = 51, Note = 49 }, 
 new NoteMapping(){ Board = 4, Key = 52, Note = 51 }, 
 new NoteMapping(){ Board = 4, Key = 53, Note = 53 }, 
 new NoteMapping(){ Board = 4, Key = 54, Note = 44 }, 
 new NoteMapping(){ Board = 4, Key = 55, Note = 46 }, 
 new NoteMapping(){ Board = 4, Key = 6, Note = 99 }, 
 new NoteMapping(){ Board = 4, Key = 7, Note = 84 }, 
 new NoteMapping(){ Board = 4, Key = 8, Note = 86 }, 
 new NoteMapping(){ Board = 4, Key = 9, Note = 88 }, 
 new NoteMapping(){ Board = 5, Key = 0, Note = 96 }, 
 new NoteMapping(){ Board = 5, Key = 1, Note = 98 }, 
 new NoteMapping(){ Board = 5, Key = 10, Note = 90 }, 
 new NoteMapping(){ Board = 5, Key = 11, Note = 92 }, 
 new NoteMapping(){ Board = 5, Key = 12, Note = 94 }, 
 new NoteMapping(){ Board = 5, Key = 13, Note = 79 }, 
 new NoteMapping(){ Board = 5, Key = 14, Note = 81 }, 
 new NoteMapping(){ Board = 5, Key = 15, Note = 83 }, 
 new NoteMapping(){ Board = 5, Key = 16, Note = 85 }, 
 new NoteMapping(){ Board = 5, Key = 17, Note = 87 }, 
 new NoteMapping(){ Board = 5, Key = 18, Note = 89 }, 
 new NoteMapping(){ Board = 5, Key = 19, Note = 72 }, 
 new NoteMapping(){ Board = 5, Key = 2, Note = 91 }, 
 new NoteMapping(){ Board = 5, Key = 20, Note = 74 }, 
 new NoteMapping(){ Board = 5, Key = 21, Note = 76 }, 
 new NoteMapping(){ Board = 5, Key = 22, Note = 78 }, 
 new NoteMapping(){ Board = 5, Key = 23, Note = 80 }, 
 new NoteMapping(){ Board = 5, Key = 24, Note = 82 }, 
 new NoteMapping(){ Board = 5, Key = 25, Note = 67 }, 
 new NoteMapping(){ Board = 5, Key = 26, Note = 69 }, 
 new NoteMapping(){ Board = 5, Key = 27, Note = 71 }, 
 new NoteMapping(){ Board = 5, Key = 28, Note = 73 }, 
 new NoteMapping(){ Board = 5, Key = 29, Note = 75 }, 
 new NoteMapping(){ Board = 5, Key = 3, Note = 93 }, 
 new NoteMapping(){ Board = 5, Key = 30, Note = 77 }, 
 new NoteMapping(){ Board = 5, Key = 31, Note = 60 }, 
 new NoteMapping(){ Board = 5, Key = 32, Note = 62 }, 
 new NoteMapping(){ Board = 5, Key = 33, Note = 64 }, 
 new NoteMapping(){ Board = 5, Key = 34, Note = 66 }, 
 new NoteMapping(){ Board = 5, Key = 35, Note = 68 }, 
 new NoteMapping(){ Board = 5, Key = 36, Note = 70 }, 
 new NoteMapping(){ Board = 5, Key = 37, Note = 55 }, 
 new NoteMapping(){ Board = 5, Key = 38, Note = 57 }, 
 new NoteMapping(){ Board = 5, Key = 39, Note = 59 }, 
 new NoteMapping(){ Board = 5, Key = 4, Note = 95 }, 
 new NoteMapping(){ Board = 5, Key = 40, Note = 61 }, 
 new NoteMapping(){ Board = 5, Key = 41, Note = 63 }, 
 new NoteMapping(){ Board = 5, Key = 42, Note = 65 }, 
 new NoteMapping(){ Board = 5, Key = 43, Note = 48 }, 
 new NoteMapping(){ Board = 5, Key = 44, Note = 50 }, 
 new NoteMapping(){ Board = 5, Key = 45, Note = 52 }, 
 new NoteMapping(){ Board = 5, Key = 46, Note = 54 }, 
 new NoteMapping(){ Board = 5, Key = 47, Note = 56 }, 
 new NoteMapping(){ Board = 5, Key = 48, Note = 58 }, 
 new NoteMapping(){ Board = 5, Key = 49, Note = 45 }, 
 new NoteMapping(){ Board = 5, Key = 5, Note = 97 }, 
 new NoteMapping(){ Board = 5, Key = 50, Note = 47 }, 
 new NoteMapping(){ Board = 5, Key = 51, Note = 49 }, 
 new NoteMapping(){ Board = 5, Key = 52, Note = 51 }, 
 new NoteMapping(){ Board = 5, Key = 53, Note = 53 }, 
 new NoteMapping(){ Board = 5, Key = 54, Note = 44 }, 
 new NoteMapping(){ Board = 5, Key = 55, Note = 46 },
 new NoteMapping(){ Board = 5, Key = 6, Note = 99 }, 
 new NoteMapping(){ Board = 5, Key = 7, Note = 84 }, 
 new NoteMapping(){ Board = 5, Key = 8, Note = 86 }, 
 new NoteMapping(){ Board = 5, Key = 9, Note = 88 }, 
        };
       
        
        
        
        private void Awake()
        {
            foreach (var bank in boards)
            {
                var bankNotePads = new List<Tonnegg>();
                notePads.Add(bankNotePads);
                
                foreach (var i in Enumerable.Range(0, 56))
                {
                    var key = bank.Find(i.ToString("00"));
                    var mr = key.GetComponent<MeshRenderer>();
                    var pos = mr.bounds.center;
                    mr.gameObject.SetActive(false);
                    var notePad = Instantiate(lumatoneKeyPrefab, bank).GetComponent<Tonnegg>();
                    notePad.transform.position = pos + new Vector3(0, -0.01f, 0);
                    notePad.SetNote((NoteEDO)i);
                    notepadLocalYs.Add(notePad, notePad.transform.localPosition.y);
                    MidiLmLumatoneIn.NoteOn += (channel, note, vel) =>
                    {
                        if (notePad.Note.MidiNum() == note)
                        {
                            notePad.LightUp(vel / 127f);
                            var localPos = notePad.transform.localPosition;
                            notePad.transform.localPosition = new Vector3(localPos.x,  notepadLocalYs[notePad] -.01f, localPos.z);
                        }
                    };
                    MidiLmLumatoneIn.CC += (channel, note, vel) =>
                    {
                        if (notePad.Note.MidiNum() == note)
                        {
                            var velAdj = 1 - vel / 127f; // for some reason key down is 0, up is 127
                            notePad.LightUp(velAdj);
                            var localPos = notePad.transform.localPosition;
                            notePad.transform.localPosition = new Vector3(localPos.x,  notepadLocalYs[notePad] -.01f * velAdj, localPos.z);
                        }
                    };
                    MidiLmLumatoneIn.NoteOff += (channel, note) =>
                    {
                        if (notePad.Note.MidiNum() == note)
                        {
                            notePad.LightUp(0);
                            var localPos = notePad.transform.localPosition;
                            notePad.transform.localPosition = new Vector3(localPos.x,  notepadLocalYs[notePad], localPos.z);
                        }
                    };
                    bankNotePads.Add(notePad);
                }
            }

            foreach (var noteMapping in noteMappings)
            {
                var board = noteMapping.Board - 1;
                var keys = notePads[board];
                keys[noteMapping.Key].SetNote(new NoteEDO(noteMapping.Note, 12));
            }
        }
    }
}