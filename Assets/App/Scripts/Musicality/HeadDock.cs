using MixedReality.Toolkit.SpatialManipulation;
using MRTK_Custom.Dock;
using UnityEngine;

namespace Musicality
{
    public class HeadDock : MonoBehaviour
    {
        [SerializeField] private GameObject dock;
        [SerializeField] private GameObject head;
        private Color _rimColor;
        private DockableCustom _dockable;
        private GameObject _dockViz;
        private GameObject _headViz;
        private Material _headMaterial;
        private ObjectManipulator _objectManipulator;
        private bool _isDocked;
        private static readonly int MatRimColor = Shader.PropertyToID("_RimColor");

        // Start is called before the first frame update
        void Start()
        {
            _dockable = head.GetComponent<DockableCustom>();
            _dockViz = dock.transform.Find("Viz").gameObject;
            _headViz = head.transform.Find("Viz").gameObject;
            _headMaterial = _headViz.GetComponent<Renderer>().material;
            _rimColor = _headMaterial.GetColor(MatRimColor);
        }

        void Update()
        {
            if (_isDocked && _dockable.CanDock) OnUnDock();
            if (!_isDocked && _dockable.CanUndock) OnDock();
        }

        private void OnDock()
        {
            App.ListenerIEM.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.Head;
            _dockViz.SetActive(false);
            _headMaterial.SetColor(MatRimColor, Color.black);
            _isDocked = true;
        }

        private void OnUnDock()
        {
            App.ListenerIEM.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.CustomOverride;
            App.ListenerIEM.GetComponent<SolverHandler>().TransformOverride = head.transform;
            _dockViz.SetActive(true);
            _headMaterial.SetColor(MatRimColor, _rimColor);
            _isDocked = false;
        }

    }
}
