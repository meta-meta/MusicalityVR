using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musicality
{
    public class IntervalViz : MonoBehaviour
    {
        [SerializeField] private GameObject intervalPrefab;

        private readonly Dictionary<ValueTuple<MalletHead, Tonnegg>, HashSet<Interval>> _pointerNotepadsToIntervals =
            new Dictionary<(MalletHead, Tonnegg), HashSet<Interval>>();

        private readonly Dictionary<Interval, ValueTuple<ValueTuple<MalletHead, Tonnegg>, ValueTuple<MalletHead, Tonnegg>>>
            _intervalsToConnections =
                new Dictionary<Interval, ValueTuple<ValueTuple<MalletHead, Tonnegg>, ValueTuple<MalletHead, Tonnegg>>>();

        private readonly HashSet<ValueTuple<MalletHead, Tonnegg>> _pointerNotepadIntersections =
            new HashSet<(MalletHead, Tonnegg)>();

        // private IObjectPool TODO: Upgrade unity and use this

        
        private void Awake()
        {
            Tonnegg.OnMalletEnter += OnMalletEnter;
            Tonnegg.OnMalletHeadExit += OnMalletExit;
        }

        private void OnDisable()
        {
            _pointerNotepadIntersections.Clear();
            _intervalsToConnections.Clear();
            _pointerNotepadsToIntervals.Clear();
            
            foreach (var o in transform)
            {
                Destroy(((Transform)o).gameObject);
            }
        }

        private void OnDestroy()
        {
            Tonnegg.OnMalletEnter -= OnMalletEnter;
            Tonnegg.OnMalletHeadExit -= OnMalletExit;
        }

        private void OnMalletEnter(Tonnegg tonnegg, MalletHead malletHead)
        {
            Debug.Log($"enter notepad:{tonnegg.GetHashCode()} pointer:{malletHead.GetHashCode()}");
            var tuple = (pointer: malletHead, notePad: tonnegg);

            foreach (var (otherPointer, otherNotePad) in _pointerNotepadIntersections)
            {
                var interval = Instantiate(intervalPrefab, transform).GetComponent<Interval>(); // TODO: pool
                interval.CalculateAndSetInterval(tonnegg.Note, otherNotePad.Note, malletHead.transform,
                    otherPointer.transform);

                var otherTuple = (otherPointer, otherNotePad);

                if (!_pointerNotepadsToIntervals.ContainsKey(tuple))
                    _pointerNotepadsToIntervals.Add(tuple, new HashSet<Interval>());
                if (!_pointerNotepadsToIntervals.ContainsKey(otherTuple))
                    _pointerNotepadsToIntervals.Add(otherTuple, new HashSet<Interval>());
                
                _pointerNotepadsToIntervals[tuple].Add(interval);
                _pointerNotepadsToIntervals[otherTuple].Add(interval);
                _intervalsToConnections.Add(interval, (tuple, otherTuple));
            }
            
            _pointerNotepadIntersections.Add(tuple);
        }

        private void OnMalletExit(Tonnegg tonnegg, MalletHead malletHead)
        {
            Debug.Log($"exit notepad:{tonnegg.GetHashCode()} pointer:{malletHead.GetHashCode()}");

            var tuple = (pointer: malletHead, notePad: tonnegg);
            _pointerNotepadIntersections.Remove(tuple);
            
            if (!_pointerNotepadsToIntervals.TryGetValue(tuple, out var intervals)) return;

            foreach (var interval in intervals)
            {
                if (!_intervalsToConnections.TryGetValue(interval, out var connection)) continue;
                _pointerNotepadsToIntervals.Remove(connection.Item1);
                _pointerNotepadsToIntervals.Remove(connection.Item2);
                _intervalsToConnections.Remove(interval);
                Destroy(interval.gameObject);
            }
            
            intervals.Clear();
        }
    }
}