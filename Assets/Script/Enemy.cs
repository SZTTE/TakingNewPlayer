using System.Collections.Generic;
using System.Linq;
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
                Vector2 from = Position.Link.GetNodeBeside(Position.To).Position;
                Vector2 delta = to - from;
                Debug.Log("from = "+from+",to = "+to);
                if (_routePosition.Distance > delta.magnitude) Debug.LogError("有一个敌人被设置了错误的位置：距连接开头的距离大于连接的距离");
                transform.position = from + delta.normalized * _routePosition.Distance;
                
                //设置朝向
                transform.rotation = UnityEngine.Quaternion.AngleAxis(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90,Vector3.forward);
            }
        }

        /// <summary>
        /// 根据当前位置，获得应该前往的下一个连接
        /// </summary>
        public Link NextLink
        {
            get
            {
                //1.获取所有的另一端点
                var links = GameManager.SearchLinks(Position.To);
                var linkDic = new Dictionary<float,Link>();//根据角度排序的linkList
                foreach (var link in links)//把每个节点都添加到linkDic里
                {
                    Node centerNode = Position.To;
                    Node anotherNode;
                    anotherNode = link.EndPoint1 == centerNode ? link.EndPoint2 : link.EndPoint1;
                    Vector2 delta = anotherNode.Position - centerNode.Position;
                    float angle = Mathf.Atan2(delta.y, delta.x);
                    linkDic.Add(angle,link);
                }
                //获取已经排好序的linkDic，寻找靠右的节点（编号比原来的连接大一，如果不存在则取编号最小的）
                var orderedLinks = linkDic.Values.ToList();
                int myLinkIndex = orderedLinks.FindIndex(x=>x==Position.Link);
                if (orderedLinks.Count - 1 == myLinkIndex) //我的链接是最后一个连接
                    return orderedLinks[0];
                else
                    return orderedLinks[myLinkIndex + 1];
            }
        }

        private void MoveForward(float distance)
        {
            RoutePosition targetPosition = Position;
            targetPosition.Distance += distance;
            if (targetPosition.Distance >= Position.Link.Distance)//通过了节点
            {
                if (Position.To.EndPic.activeSelf)
                {//到达终点
                    UnityEngine.Time.timeScale = 0;
                    Debug.Log("游戏结束：敌人到达终点");
                }
                else if (GameManager.SearchLinks(Position.To).Count == 1)
                {//到达末端
                    Destroy(gameObject);
                }
                else
                {//通过一般末端
                    float distanceUsed = targetPosition.Distance - Position.Link.Distance;//在本次连接中移动的距离
                    //设置新出发地，并且以剩余距离为参数重新出发
                    targetPosition.Link = NextLink;
                    targetPosition.To = targetPosition.Link.GetNodeBeside(Position.To);
                    targetPosition.Distance = 0;
                    Position = targetPosition;
                    MoveForward(distance-distanceUsed);
                }
            }
            else
            {//没有通过节点
                Position = targetPosition;
            }
        }

        public void MoveForward()
        {
            MoveForward(_moveDistance);
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
