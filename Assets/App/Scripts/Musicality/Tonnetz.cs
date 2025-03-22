using System.Collections.Generic;
using OscSimpl;
using UnityEngine;

namespace Musicality
{
    public class Tonnetz : MonoBehaviour
    {
        public GameObject prefab;
        public bool isColorChromatic;
        private OscOut _oscOutMax;
        public string oscAddr;

        [SerializeField] private float hpad = Mathf.PI / 28;
        [SerializeField] private float vpad = 0.08f;
        [SerializeField] private Layout layout = Layout.StringInstrument;

        // StringInstrument
        [SerializeField] private Transform docks;
        [SerializeField] private int stringCount = 4;
        [SerializeField] private int fretCount = 19;
        [SerializeField] private int stringInterval = 7;
        [SerializeField] private int lowestNote = 24;
        [SerializeField] private float radius = 0.7f;
        [SerializeField] private bool reset;
        private bool _reset;

        private readonly List<GameObject> _dockPool = new List<GameObject>();

        public enum Layout
        {
            Lattice,
            Spiral,
            StringInstrument,
            Tonnetz,
        }
        
        private void OnValidate()
        {
            if (!Application.isPlaying || !gameObject.activeInHierarchy) return;
            
            if (_reset != reset)
            {
                // foreach (Transform dock in docks) DestroyImmediate(dock.gameObject);
                _dockPool.Clear();
                _reset = reset;
                return;
            }
            Refresh();
        }

        private void Refresh()
        {
            foreach (var o in _dockPool) o.SetActive(false);
            var c = 0;

            for (var s = 0; s < stringCount; s++)
            {
                for (var n = 0; n < fretCount; n++)
                {
                    var note = lowestNote + n + s * stringInterval;

                    if (_dockPool.Count == c) _dockPool.Add(Instantiate(prefab, docks));
                    var obj = _dockPool[c];
                    obj.SetActive(true);
                    c++;

                    obj.transform.localPosition = new Vector3(
                        radius * Mathf.Cos(-n * hpad),
                        s * vpad - 0.5f,
                        radius * Mathf.Sin(-n * hpad) //+ Mathf.Sin(s * Mathf.PI / 48)
                    );

                    obj.transform.LookAt(docks);

                    // var mat = isColorChromatic ? note % 12 : (note * 7) % 12;
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/glass-n{mat}");
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/n{mat}");
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/note");

                    // var dockPos = obj.GetComponent<DockPositionCustom>();
                    // dockPos.Note = (NoteEDO)note;

                    // set notePicker config on dockPos so docked note gets that too
                    // var notePicker = obj.GetComponentInChildren<SpiralNotePicker>();
                    // notePicker.jiIsOtonalIncluded = false;
                    // notePicker.jiIsUtonalIncluded = false;
                    // notePicker.ForceRefreshData();
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
            
            return;
            _oscOutMax = GameObject.Find("OSC").GetComponent<OscManager>().OscOutMaxMsp;
            oscAddr = "/lattice";
            // return;

            for (var s = 0; s < 10; s++)
            {
                for (var n = 0; n < 24; n++)
                {
                    var note = 24 + n + s * 7;

                    var notePad = Instantiate(prefab, transform);
                    notePad.transform.localPosition = new Vector3(
                        0.7f * Mathf.Cos(-n * hpad),
                        s * vpad - 0.5f,
                        0.7f * Mathf.Sin(-n * hpad) //+ Mathf.Sin(s * Mathf.PI / 48)
                    );

                    notePad.transform.LookAt(transform);

                    // var mat = isColorChromatic ? note % 12 : (note * 7) % 12;
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/glass-n{mat}");
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/n{mat}");
                    // notePad.GetComponent<MeshRenderer>().material = Resources.Load<Material>($"Materials/note");

                    var notePadCmp = notePad.GetComponent<Tonnegg>();
                    notePadCmp.oscAddr = oscAddr;

                    var notePicker = notePad.GetComponentInChildren<SpiralNotePicker>();
                    notePicker.jiIsOtonalIncluded = false;
                    notePicker.jiIsUtonalIncluded = false;
                    notePicker.ForceRefreshData();

                    notePadCmp.SetNote((NoteEDO)note);
                    notePicker.SetAngleFromNote(notePadCmp.Note);
                    notePicker.gameObject.SetActive(false);
                }
            }
        }

        void FixedUpdate()
        {
            /* TODO:
                 * show interval between hands when close to notes
             *   * colored eggs are "shells" over the actual notes. They can all be shifted (transpose) to make the new red egg wrap around static D rather than static C
             *   * ability to select a note and some scale or chord mask on top so that the out of key eggs cannot be hit
             */

            // TODO: individual sustain for each hand (need 2 separate instances of pianoteq or implement per-note sustain here, issue note-offs on release)
            // if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            //      OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)))
            // {
            //     _oscOutMax.Send(new OscMessage($"{oscAddr}/cc").Add(64).Add(127));
            // }
            //
            // if ((OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) ||
            //      OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger)))
            // {
            //     _oscOutMax.Send(new OscMessage($"{oscAddr}/cc").Add(64).Add(0));
            // }
        }
    }
}