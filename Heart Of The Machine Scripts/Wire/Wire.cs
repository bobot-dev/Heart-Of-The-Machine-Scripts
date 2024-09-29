using UnityEngine;

namespace HeartOfTheMachineScripts
{
    public class Wire : MonoBehaviour
    {

        //public MeshRenderer meshRenderer;
        //public MeshFilter meshFilter;
        public TubeRenderer tubeRenderer;

        public Transform startTransform, endTransform;
        public int segmentCount = 10;
        public float totalLength = 10;

        public float radius = 0.5f;
        public int sides = 4;

        private bool _createTris;

        public bool usePhysics = false;

        public float totalWeight = 10;
        public float drag = 1;
        public float angularDrag = 1;

        private Transform[] _segments = new Transform[0];
        public Transform segmentParent;

        private WireMeshData _meshData;

        public Vector3[] vertices;
        private int[,] _vertexIndicesMap;

        private int _preSides;
        private int _prevSegmentCount;
        private float _preTotalLength;
        private float _prevDrag;
        private float _preTotalWeight;
        private float _preAngluarDrag;
        private float _preRadius;

        private void Start()
        {
            vertices = new Vector3[segmentCount * sides * 3];
            GenerateMesh();

            tubeRenderer._radiusOne = radius;
            tubeRenderer._radiusTwo = radius;
            tubeRenderer._sides = sides;
        }

        private void Update()
        {
            if (_prevSegmentCount != segmentCount)
            {
                RemoveSegments();
                _segments = new Transform[segmentCount];
                GenerateSegments();
                GenerateMesh();
            }
            _prevSegmentCount = segmentCount;

            if (sides != _preSides)
            {
                tubeRenderer._sides = sides;
                vertices = new Vector3[segmentCount * sides * 3];
                GenerateMesh();
            }
            _preSides = sides;




            if (totalLength != _preTotalLength || drag != _prevDrag || angularDrag != _preAngluarDrag || totalWeight != _preTotalWeight)
            {
                UpdateWire();
                GenerateMesh();
            }

            _preTotalLength = totalLength;
            _prevDrag = drag;
            _preAngluarDrag = angularDrag;
            _preTotalWeight = totalWeight;

            if (_preRadius != radius && usePhysics)
            {
                UpdateRadius();
                GenerateMesh();
            }
            _preRadius = radius;

            UpdateMesh();
        }

        private void UpdateRadius()
        {
            foreach (var segment in _segments)
            {
                UpdateRadiusOnSegment(segment);
            }
            tubeRenderer._radiusOne = radius;
            tubeRenderer._radiusTwo = radius;
        }

        private void UpdateWire()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                if (i != 0)
                {
                    UpdateLengthOnSegment(_segments[i]);
                }

                UpdateWeightOnSegment(_segments[i]);
            }
        }

        private void UpdateRadiusOnSegment(Transform segment)
        {
            if (segment.TryGetComponent(out SphereCollider collider))
            {
                collider.radius = radius;
            }
        }

        private void UpdateWeightOnSegment(Transform segment)
        {
            if (segment.TryGetComponent(out Rigidbody rb))
            {
                rb.mass = totalWeight / segmentCount;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
            }
        }

        private void UpdateLengthOnSegment(Transform segment)
        {
            if (segment.TryGetComponent(out ConfigurableJoint joint))
            {
                joint.connectedAnchor = Vector3.forward * (totalLength / segmentCount);
            }
        }

        private void RemoveSegments()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                if (_segments[i])
                {
                    Destroy(_segments[i].gameObject);
                }
            }
        }

        public void MakeTriggers()
        {
            foreach (var segment in _segments)
            {
                segment.GetComponent<Collider>().isTrigger = true; 
            }
           
        }

        /*private void OnDrawGizmos()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                Gizmos.DrawWireSphere(_segments[i].position, radius);
            
                if (_segments[i].parent.GetChild(_segments[i].GetSiblingIndex() + 1))
                    Gizmos.DrawLine(_segments[i].position, _segments[i].parent.GetChild(_segments[i].GetSiblingIndex() + 1).position);
            }

            if (vertices != null)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i] != null)
                        Gizmos.DrawSphere(vertices[i], 0.05f);
                }
            }
        }*/

        private void GenerateSegments()
        {
            JoinSegment(startTransform, null, true, false);
            Transform prevTransform = startTransform;

            Vector3 dir = (endTransform.position - startTransform.position);

            for (int i = 0; i < segmentCount; i++)
            {
                GameObject segment = new GameObject($"Segment {i}");
                segment.layer = 9;
                segment.transform.SetParent(segmentParent);
                _segments[i] = segment.transform;

                Vector3 pos  = prevTransform.position + (dir / segmentCount);
                segment.transform.position = pos;

                JoinSegment(segment.transform, prevTransform, false, i == 0);

                prevTransform = segment.transform;
            }

            JoinSegment(endTransform, prevTransform, true, true);

            tubeRenderer.positions = _segments;
        }

        private void JoinSegment(Transform current, Transform connectedTransform, bool isKinetic = false, bool isCloseConnected = false)
        {

            if (!current.GetComponent<Rigidbody>())
            {
                Rigidbody rb = current.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = isKinetic;
                rb.mass = totalWeight / segmentCount;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
            }

            

            if (usePhysics)
            {
                SphereCollider collider = current.gameObject.AddComponent<SphereCollider>();
                current.gameObject.AddComponent<StupidAwfulTerribleBandaidFix>();

                collider.radius = radius;
                collider.isTrigger = true;

            }

            if (connectedTransform != null)
            {

                

                ConfigurableJoint joint = current.gameObject.GetComponent<ConfigurableJoint>();
                if (!joint)
                {
                    joint = current.gameObject.AddComponent<ConfigurableJoint>();
                }


                joint.connectedBody = connectedTransform.GetComponent<Rigidbody>();

                joint.autoConfigureConnectedAnchor = false;
                if (isCloseConnected)
                {
                    joint.connectedAnchor = Vector3.forward * 0.1f;
                }
                else
                {
                    joint.connectedAnchor = Vector3.forward * (totalLength / segmentCount);
                }


                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;

                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit softJointLimit = new SoftJointLimit
                {
                    limit = 0
                };
                joint.angularZLimit = softJointLimit;

                JointDrive jointDrive = new JointDrive()
                {
                    positionDamper = 0,
                    positionSpring = 0
                };

                joint.angularXDrive = jointDrive;
                joint.angularXDrive = jointDrive;

            }
        }

        private void UpdateMesh()
        {
            GenerateVertices();

            //meshFilter.mesh.vertices = _vertices;
            //meshFilter.mesh.SetVertices(vertices);
        }

        private void GenerateMesh()
        {
            return;
            _createTris = true;

            if (_meshData == null)
            {
                _meshData = new WireMeshData(sides, segmentCount + 1, false);
            }
            else
            {
                _meshData.ResetMesh(sides, segmentCount + 1, false);
            }

            GenerateIndicesMap();
            GenerateVertices();

            _meshData.ProcessMesh();
            //_mesh = _meshData.CreateMesh();

            //meshFilter.sharedMesh = _mesh;

            _createTris = false;
        }

        private void GenerateIndicesMap()
        {
            _vertexIndicesMap = new int[segmentCount + 1, sides + 1];
            int meshVertexIndex = 0;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                for (int sideIndex = 0; sideIndex < sides; sideIndex++)
                {
                    _vertexIndicesMap[segmentIndex, sideIndex] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        private void GenerateVertices()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                GenerateCircleVerticesAndFaces(_segments[i], i);
            }
        }

        private void GenerateCircleVerticesAndFaces(Transform segmentTransform, int segmentIndex)
        {
            float angleDiff = 360f / sides;

            Quaternion diffRotation = Quaternion.FromToRotation(Vector3.right, segmentTransform.forward);

            for (int sideIndex = 0; sideIndex < sides; sideIndex++)
            {
                float angleInRad = sideIndex * angleDiff * Mathf.Deg2Rad;

                float x = -1 * radius * Mathf.Cos(angleInRad);
                float y = radius * Mathf.Sin(angleInRad);

                Vector3 pointOffset = new Vector3(x, y, 0);

                Vector3 pointRotated = diffRotation * pointOffset;

                Vector3 pointRotatedAtCenterOfTransform = segmentTransform.localPosition + pointRotated;

                int vertexIndex = segmentIndex * sides + sideIndex;
                vertices[vertexIndex] = pointRotatedAtCenterOfTransform;

                if (_createTris)
                {
                    _meshData.AddVertex(pointRotatedAtCenterOfTransform, Vector2.zero, vertexIndex);

                    bool createThisTri = segmentIndex < segmentCount - 1;
                    if (createThisTri)
                    {
                        int currentIncrement = 1;
                        int a = _vertexIndicesMap[segmentIndex, sideIndex];
                        int b = _vertexIndicesMap[segmentIndex + currentIncrement, sideIndex];
                        int c = _vertexIndicesMap[segmentIndex, sideIndex + currentIncrement];
                        int d = _vertexIndicesMap[segmentIndex + currentIncrement, sideIndex + currentIncrement];

                        bool isLastGap = sideIndex == sides - 1;
                        if (isLastGap)
                        {
                            c = _vertexIndicesMap[segmentIndex, 0];
                            d = _vertexIndicesMap[segmentIndex + currentIncrement, 0];
                        }

                        _meshData.AddTriangle(a, d, c);
                        _meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }
    }
}
