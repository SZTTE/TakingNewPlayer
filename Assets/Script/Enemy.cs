using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Script
{
    public class Enemy : MonoBehaviour
    {
        private GameObject _smallPic;
        private GameObject _bigPic;
        private RoutePosition _routePosition;
        private float _moveDistance = 0.02f;
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
                //赋值
                _routePosition = value;
                
                //设置位置
                Vector2 to = _routePosition.To.Position;
                Vector2 from;
                if (_routePosition.Link.EndPoint1.Position == to) from = _routePosition.Link.EndPoint2.Position;
                else from = _routePosition.Link.EndPoint1.Position;
                Vector2 delta = to - from;
                if (_routePosition.Distance > delta.magnitude) Debug.LogError("有一个敌人被设置了错误的位置：距连接开头的距离大于连接的距离");
                transform.position = from + delta.normalized * _routePosition.Distance;
                
                //设置朝向
                transform.Rotate(Vector3.forward,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);
            }
        }

        public void MoveForward()
        {
            RoutePosition targetPosition = Position;
            targetPosition.Distance += _moveDistance;
            if (targetPosition.Distance >= Position.Link.Distance)//通过了节点
            {
                if (Position.To.EndPic.activeSelf)
                {//到达终点
                }
                else if (GameManager.SearchLinks(Position.To).Count == 1)
                {//到达末端
                }
                else
                {//通过一般末端
                }
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
