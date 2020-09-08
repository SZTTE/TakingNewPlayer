using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Assets.Script
{
    public class Node : MonoBehaviour
    {
        private GameObject _beginPic;
        private GameObject _endPic;
        private GameObject _normalPic;

        public GameObject BeginPic
        {
            get
            {
                if (_beginPic == null) Init();
                return _beginPic;
            }
        }
        public GameObject EndPic
        {
            get
            {
                if (_endPic == null) Init();
                return _endPic;
            }
        }
        public GameObject NormalPic
        {
            get
            {
                if (_normalPic == null) Init();
                return _normalPic;
            }
        }

        private void Init()
        {
            _beginPic = transform.Find("RelocateBegin").gameObject;
            _endPic = transform.Find("RelocateEnd").gameObject;
            _normalPic = transform.Find("RelocateNormal").gameObject;
        }

        // Start is called before the first frame update
        void Start()
        {
        }
        
        

        // Update is called once per frame
        void Update()
        {
        
        }

        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public void BecomeBegin()
        {
            BeginPic.SetActive(true);
            EndPic.SetActive(false);
            NormalPic.SetActive(false);
        }
        public void BecomeEnd()
        {
            BeginPic.SetActive(false);
            EndPic.SetActive(true);
            NormalPic.SetActive(false);
        }
        public void BecomeNormal()
        {
            BeginPic.SetActive(false);
            EndPic.SetActive(false);
            NormalPic.SetActive(true);
        }
    }
}
