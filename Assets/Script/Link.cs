using UnityEngine;

namespace Assets.Script
{
    public class Link : MonoBehaviour
    {
        private LineRenderer _lineRenderer;

        public LineRenderer LineRenderer
        {
            get
            {
                if(_lineRenderer==null) _lineRenderer = GetComponent<LineRenderer>();
                return _lineRenderer;
            }
        }

        private Node _endPoint1;
        private Node _endPoint2;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public Node EndPoint1
        {
            get=>_endPoint1;
            set
            {
                _endPoint1 = value;
                LineRenderer.SetPosition(0, _endPoint1.Position);
            }
        }

        public Node EndPoint2
        {
            get=>_endPoint2;
            set
            {
                _endPoint2 = value;
                LineRenderer.SetPosition(1, _endPoint2.Position);
            }
        }
    }
}
