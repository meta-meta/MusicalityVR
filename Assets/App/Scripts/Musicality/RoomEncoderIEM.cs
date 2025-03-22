using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Musicality
{
    public class RoomEncoderIEM : MonoBehaviour
    {
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Vector3 initialRoomSize = new Vector3(10, 8, 8);
        [SerializeField] private int portOut;
        private OscOut _oscOutRoomEncoder;
        public Transform RoomOrigin { get; private set; }

        public struct Wall
        {
            private MoveAxisConstraint _moveConstraint;
            private RoomEncoderIEM _roomEncoder;
            private RotationAxisConstraint _rotationAxisConstraint;
            private Vector3 _pos;
            public Collider Col;
            public Microsoft.MixedReality.Toolkit.UI.ObjectManipulator Om;
            public Transform Tr;

            public void Update()
            {
                var d = 0.2f;
                var roomSize = _roomEncoder.initialRoomSize;

                Tr.localPosition = new Vector3(
                    Mathf.Abs(_pos.x) < 0.5
                        ? (_roomEncoder._right.Tr.localPosition.x + _roomEncoder._left.Tr.localPosition.x) / 2
                        : Tr.localPosition.x,
                    Mathf.Abs(_pos.y) < 0.5
                        ? (_roomEncoder._ceiling.Tr.localPosition.y + _roomEncoder._floor.Tr.localPosition.y) / 2
                        : Tr.localPosition.y,
                    Mathf.Abs(_pos.z) < 0.5
                        ? (_roomEncoder._front.Tr.localPosition.z + _roomEncoder._back.Tr.localPosition.z) / 2
                        : Tr.localPosition.z
                );

                Tr.localScale = new Vector3(
                    Mathf.Abs(_pos.x) > 0 ? d : roomSize.x - d,
                    Mathf.Abs(_pos.y) > 0 ? d : roomSize.y - d,
                    Mathf.Abs(_pos.z) > 0 ? d : roomSize.z - d);
            }

            private static AxisFlags _lockedRotation = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
            private AxisFlags _unlockedRotation;
            private static AxisFlags _lockedMovement = AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis;
            private AxisFlags _unlockedMovement;

            public static Wall New(Transform parentTransform, string name, Vector3 pos, RoomEncoderIEM roomEncoder)
            {
                var gObj = new GameObject(name);
                gObj.transform.SetParent(parentTransform);

                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.transform.SetParent(gObj.transform, false);
                box.GetComponent<Renderer>().material = roomEncoder.wallMaterial;

                var roomSize = roomEncoder.initialRoomSize;
                var isCeiling = pos.y > 0.5;
                var isFloor = pos.y < -0.5;

                gObj.transform.localPosition = new Vector3(
                    pos.x * (roomSize.x / 2),
                    isCeiling
                        ? roomSize.y
                        : isFloor
                            ? 0
                            : roomSize.y / 2,
                    pos.z * (roomSize.z / 2));

                var om = gObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();

                if (isFloor) om.HostTransform = roomEncoder.transform;


                // no rotation except floor can rotate on y-axis
                var rotCon = gObj.AddComponent<RotationAxisConstraint>();
                rotCon.ConstraintOnRotation = AxisFlags.XAxis |
                                              (isFloor ? 0 : AxisFlags.YAxis) |
                                              AxisFlags.ZAxis;

                // movement constrained to normal of each wall except floor can't be moved
                var moveConstraint = gObj.AddComponent<MoveAxisConstraint>();
                moveConstraint.UseLocalSpaceForConstraint = true;
                moveConstraint.ConstraintOnMovement = (Mathf.Abs(pos.x) < 0.5 ? AxisFlags.XAxis : 0) |
                                                      (Mathf.Abs(pos.y) < 0.5 || isFloor ? AxisFlags.YAxis : 0) |
                                                      (Mathf.Abs(pos.z) < 0.5 ? AxisFlags.ZAxis : 0);
                
                return new Wall()
                {
                    Col = gObj.AddComponent<BoxCollider>(),
                    Om = om,
                    Tr = gObj.transform,
                    _moveConstraint = moveConstraint,
                    _pos = pos,
                    _roomEncoder = roomEncoder,
                    _rotationAxisConstraint = rotCon,
                    _unlockedMovement = moveConstraint.ConstraintOnMovement,
                    _unlockedRotation = rotCon.ConstraintOnRotation
                };
            }

            public void SetIsLocked(bool isLocked)
            {
                _moveConstraint.ConstraintOnMovement = isLocked ? _lockedMovement : _unlockedMovement;
                _rotationAxisConstraint.ConstraintOnRotation = isLocked ? _lockedRotation : _unlockedRotation;
            }
        }

        private Wall _back;
        private Wall _ceiling;
        private Wall _floor;
        private Wall _front;
        private Wall _left;
        private Wall _right;
        private HashSet<Wall> _walls;

        private OscMessage _roomX;
        private OscMessage _roomY;
        private OscMessage _roomZ;
        private OscMessage _wallAttenuationFront;
        private OscMessage _wallAttenuationBack;
        private OscMessage _wallAttenuationLeft;
        private OscMessage _wallAttenuationRight;
        private OscMessage _wallAttenuationCeiling;
        private OscMessage _wallAttenuationFloor;

        private void Awake()
        {
            var gObj = new GameObject("RoomOrigin");
            RoomOrigin = gObj.transform;
            RoomOrigin.SetParent(transform);
        }

        // Start is called before the first frame update
        void Start()
        {
            // var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // box.transform.SetParent(RoomOrigin, false);
            // box.transform.localScale = Vector3.one * 0.1f;
            // box.GetComponent<Renderer>().material = wallMaterial;


            _back = Wall.New(transform, "Back", Vector3.back, this);
            _ceiling = Wall.New(transform, "Ceiling", Vector3.up, this);
            _floor = Wall.New(transform, "Floor", Vector3.down, this);
            _front = Wall.New(transform, "Front", Vector3.forward, this);
            _left = Wall.New(transform, "Left", Vector3.left, this);
            _right = Wall.New(transform, "Right", Vector3.right, this);
            _walls = new HashSet<Wall>() { _back, _front, _left, _right, _floor, _ceiling };

            var ip = GameObject.Find("OSC").GetComponent<OscManager>().RemoteIpAddress;
            _oscOutRoomEncoder = gameObject.AddComponent<OscOut>();
            _oscOutRoomEncoder.Open(portOut, ip);

            _roomX = new OscMessage($"/RoomEncoder/roomX");
            _roomY = new OscMessage($"/RoomEncoder/roomY");
            _roomZ = new OscMessage($"/RoomEncoder/roomZ");

            // -50 : 0 dB
            _wallAttenuationFront = new OscMessage($"/RoomEncoder/wallAttenuationFront");
            _wallAttenuationBack = new OscMessage($"/RoomEncoder/wallAttenuationBack");
            _wallAttenuationLeft = new OscMessage($"/RoomEncoder/wallAttenuationLeft");
            _wallAttenuationRight = new OscMessage($"/RoomEncoder/wallAttenuationRight");
            _wallAttenuationCeiling = new OscMessage($"/RoomEncoder/wallAttenuationCeiling");
            _wallAttenuationFloor = new OscMessage($"/RoomEncoder/wallAttenuationFloor");

            _floor.Om.OnManipulationStarted.AddListener(FloorManipulationStart);
            _floor.Om.OnManipulationEnded.AddListener(FloorManipulationEnd);
        }

        private Handedness _floorManipHand;
        private bool IsFloorManip => _floorManipHand.IsMatch(Handedness.Any);
        public bool isLocked;

        void FloorManipulationStart(ManipulationEventData arg0)
        {
            var handedness = arg0.Pointer.Controller?.ControllerHandedness ?? Handedness.None;
            _floorManipHand = _floorManipHand.IsNone() ? handedness : Handedness.Both;
        }

        void FloorManipulationEnd(ManipulationEventData arg0)
        {
            _floorManipHand = Handedness.None;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (IsFloorManip)
            {
                var lockToggleButton = _floorManipHand.IsLeft()
                    ? OVRInput.RawButton.LHandTrigger
                    : OVRInput.RawButton.RHandTrigger;
                if (OVRInput.GetDown(lockToggleButton))
                {
                    isLocked = !isLocked;
                    _floor.Om.ForceEndManipulation(); // prevent lerp to old position when locking
                    foreach (var wall in _walls) wall.SetIsLocked(isLocked);
                }
            }
            
            // if (isLocked) return;
            
            initialRoomSize = new Vector3(
                _right.Tr.localPosition.x - _left.Tr.localPosition.x,
                _ceiling.Tr.localPosition.y - _floor.Tr.localPosition.y,
                _front.Tr.localPosition.z - _back.Tr.localPosition.z);

            RoomOrigin.position = new Vector3(
                _left.Tr.position.x + (_right.Tr.position.x - _left.Tr.position.x) / 2,
                _floor.Tr.position.y + (_ceiling.Tr.position.y - _floor.Tr.position.y) / 2,
                _back.Tr.position.z + (_front.Tr.position.z - _back.Tr.position.z) / 2);

            _roomX.Set(0, initialRoomSize.z);
            _roomY.Set(0, initialRoomSize.x);
            _roomZ.Set(0, initialRoomSize.y);
            _oscOutRoomEncoder.Send(_roomX);
            _oscOutRoomEncoder.Send(_roomY);
            _oscOutRoomEncoder.Send(_roomZ);

            foreach (var wall in _walls)
            {
                wall.Update();
            }

            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, 0, pos.z);
        }
    }
}