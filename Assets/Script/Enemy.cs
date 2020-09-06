using UnityEngine;

namespace Assets.Script
{
    public class Enemy : MonoBehaviour
    {
        private GameObject _smallPic;
        private GameObject _bigPic;
        private RoutePosition _routePosition;
        private GameObject SmallPic 
        {
            get
            {
                if(_smallPic==null) Init();
                return _smallPic;
            }
        }
        public GameObject BigPic
        {
            get
            {
                if(_bigPic==null) Init();
                return _bigPic;
            }
        }

        public RoutePosition Position
        {
            get { return _routePosition;}
            set
            {
                _routePosition = value;
                Vector2 to = _routePosition.To.Position;
                Vector2 from;
                if (_routePosition.Link.EndPoint1.Position == to) from = _routePosition.Link.EndPoint2.Position;
                else from = _routePosition.Link.EndPoint1.Position;
                Vector2 delta = to - from;
                if (_routePosition.Distance > delta.magnitude) Debug.LogError("有一个敌人被设置了错误的位置：距连接开头的距离大于连接的距离");

                transform.position = from + delta.normalized * _routePosition.Distance;
            }
        }

        void Init()
        {
            _smallPic = transform.Find("EnemySmall").gameObject;
            _bigPic = transform.Find("EnemyBig").gameObject;
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void BecomeBig()
        {
            BigPic.SetActive(true);
            SmallPic.SetActive(false);
        }
        public void BecomeSmall()
        {
            BigPic.SetActive(false);
            SmallPic.SetActive(true);
        }
    }
}
